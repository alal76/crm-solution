// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for contract management operations.
/// Handles contract lifecycle from creation to renewal, including
/// status management, approval workflows, documents, and reporting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ContractsController : CrmControllerBase
{
    private const string ContractNotFoundMessage = "Contract {0} not found";
    private readonly IContractService _contractService;
    private readonly IContractExportService _contractExportService;
    private readonly ILogger<ContractsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractsController"/> class.
    /// </summary>
    public ContractsController(IContractService contractService, IContractExportService contractExportService, ILogger<ContractsController> logger)
    {
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
        _contractExportService = contractExportService ?? throw new ArgumentNullException(nameof(contractExportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // CRUD Operations
    // ========================================================================

    /// <summary>Gets all contracts with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] ContractStatus? status = null,
        [FromQuery] ContractType? contractType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
                var contracts = await _contractService.GetAllAsync(accountId, status, cancellationToken);

        // Apply contract type filter if specified
        if (contractType.HasValue)
        {
            contracts = contracts.Where(c => c.ContractType == contractType.Value);
        }

        var totalCount = contracts.Count();
        var items = contracts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToList();

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>Gets a contract by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        return Ok(MapToDto(contract));
    }

    /// <summary>Gets a contract by contract number.</summary>
    [HttpGet("by-number/{contractNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByContractNumber(string contractNumber, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByContractNumberAsync(contractNumber, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = $"Contract '{contractNumber}' not found" });
        }

        return Ok(MapToDto(contract));
    }

    /// <summary>Creates a new contract.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var contract = new Contract
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status ?? ContractStatus.Draft,
            ContractType = request.ContractType ?? ContractType.Service,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            OwnerId = request.OwnerId,
            ParentContractId = request.ParentContractId,
            OpportunityId = request.OpportunityId,
            QuoteId = request.QuoteId,
            StartDate = request.StartDate ?? DateTime.UtcNow,
            EndDate = request.EndDate ?? DateTime.UtcNow.AddYears(1),
            SignedDate = request.SignedDate,
            Value = request.Value ?? 0,
            CurrencyCode = request.CurrencyCode ?? "USD",
            BillingFrequency = request.BillingFrequency,
            AutoRenew = request.AutoRenew ?? false,
            RenewalNoticeDays = request.RenewalNoticeDays ?? 30,
            Terms = request.Terms,
            SpecialConditions = request.SpecialConditions,
            TerminationClause = request.TerminationClause,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _contractService.CreateAsync(contract, cancellationToken);
        _logger.LogInformation("Contract {ContractId} created: {ContractName}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>Updates an existing contract.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        // Field-by-field patching
        if (request.Name != null)
        {
            contract.Name = request.Name;
        }
        if (request.Description != null)
        {
            contract.Description = request.Description;
        }
        if (request.Status.HasValue)
        {
            contract.Status = request.Status.Value;
        }
        if (request.ContractType.HasValue)
        {
            contract.ContractType = request.ContractType.Value;
        }
        if (request.AccountId.HasValue)
        {
            contract.AccountId = request.AccountId.Value;
        }
        if (request.ContactId.HasValue)
        {
            contract.ContactId = request.ContactId;
        }
        if (request.OwnerId.HasValue)
        {
            contract.OwnerId = request.OwnerId;
        }
        if (request.ParentContractId.HasValue)
        {
            contract.ParentContractId = request.ParentContractId;
        }
        if (request.OpportunityId.HasValue)
        {
            contract.OpportunityId = request.OpportunityId;
        }
        if (request.QuoteId.HasValue)
        {
            contract.QuoteId = request.QuoteId;
        }
        if (request.StartDate.HasValue)
        {
            contract.StartDate = request.StartDate.Value;
        }
        if (request.EndDate.HasValue)
        {
            contract.EndDate = request.EndDate.Value;
        }
        if (request.SignedDate.HasValue)
        {
            contract.SignedDate = request.SignedDate;
        }
        if (request.Value.HasValue)
        {
            contract.Value = request.Value.Value;
        }
        if (request.CurrencyCode != null)
        {
            contract.CurrencyCode = request.CurrencyCode;
        }
        if (request.BillingFrequency != null)
        {
            contract.BillingFrequency = request.BillingFrequency;
        }
        if (request.AutoRenew.HasValue)
        {
            contract.AutoRenew = request.AutoRenew.Value;
        }
        if (request.RenewalNoticeDays.HasValue)
        {
            contract.RenewalNoticeDays = request.RenewalNoticeDays.Value;
        }
        if (request.Terms != null)
        {
            contract.Terms = request.Terms;
        }
        if (request.SpecialConditions != null)
        {
            contract.SpecialConditions = request.SpecialConditions;
        }
        if (request.TerminationClause != null)
        {
            contract.TerminationClause = request.TerminationClause;
        }
        contract.UpdatedAt = DateTime.UtcNow;

        var updated = await _contractService.UpdateAsync(contract, cancellationToken);
        _logger.LogInformation("Contract {ContractId} updated", id);

        return Ok(MapToDto(updated));
    }

    /// <summary>Deletes a contract (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var result = await _contractService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return StatusCode(500, new { message = "Error deleting contract" });
        }

        _logger.LogInformation("Contract {ContractId} deleted", id);
        return NoContent();
    }

    // ========================================================================
    // Status Management
    // ========================================================================

    /// <summary>Approves a contract.</summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        if (contract.Status != ContractStatus.PendingApproval)
        {
            return BadRequest(new { message = "Contract must be in PendingApproval status to approve" });
        }

        var updated = await _contractService.UpdateStatusAsync(id, ContractStatus.Approved, cancellationToken);
        _logger.LogInformation("Contract {ContractId} approved", id);

        return Ok(MapToDto(updated));
    }

    /// <summary>Rejects a contract.</summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectContractRequest request, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        if (contract.Status != ContractStatus.PendingApproval)
        {
            return BadRequest(new { message = "Contract must be in PendingApproval status to reject" });
        }

        contract.RejectionReason = request.Reason;
        var updated = await _contractService.UpdateStatusAsync(id, ContractStatus.Draft, cancellationToken);
        _logger.LogInformation("Contract {ContractId} rejected: {Reason}", id, request.Reason);

        return Ok(MapToDto(updated));
    }

    /// <summary>Activates a contract.</summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var result = await _contractService.ActivateAsync(id, cancellationToken);
        _logger.LogInformation("Contract {ContractId} activated", id);

        return Ok(MapToDto(result));
    }

    /// <summary>Suspends a contract.</summary>
    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Suspend(int id, [FromBody] SuspendContractRequest? request = null, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var result = await _contractService.SuspendAsync(id, request?.Reason ?? "No reason provided", cancellationToken);
        _logger.LogInformation("Contract {ContractId} suspended", id);

        return Ok(MapToDto(result));
    }

    /// <summary>Terminates a contract.</summary>
    [HttpPost("{id}/terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Terminate(int id, [FromBody] TerminateContractRequest? request = null, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var result = await _contractService.TerminateAsync(id, request?.Reason ?? "No reason provided", null, cancellationToken);
        _logger.LogInformation("Contract {ContractId} terminated", id);

        return Ok(MapToDto(result));
    }

    /// <summary>Expires a contract.</summary>
    [HttpPost("{id}/expire")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Expire(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var result = await _contractService.ExpireAsync(id, cancellationToken);
        _logger.LogInformation("Contract {ContractId} expired", id);

        return Ok(MapToDto(result));
    }

    // ========================================================================
    // Renewal Operations
    // ========================================================================

    /// <summary>Initiates contract renewal.</summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Renew(int id, [FromBody] RenewContractRequest? request = null, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var renewed = await _contractService.InitiateRenewalAsync(id, cancellationToken);
        _logger.LogInformation("Contract {ContractId} renewal initiated, new contract {NewContractId}", id, renewed.Id);

        return Ok(MapToDto(renewed));
    }

    /// <summary>Gets contracts due for renewal within specified days.</summary>
    [HttpGet("due-for-renewal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDueForRenewal([FromQuery] int withinDays = 30, CancellationToken cancellationToken = default)
    {
                var contracts = await _contractService.GetContractsDueForRenewalAsync(withinDays, cancellationToken);
        return Ok(contracts.Select(c => MapToDto(c)));
    }

    /// <summary>Gets renewal history for a contract.</summary>
    [HttpGet("{id}/renewal-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRenewalHistory(int id, CancellationToken cancellationToken = default)
    {
                var history = await _contractService.GetRenewalHistoryAsync(id, cancellationToken);
        return Ok(history.Select(c => MapToDto(c)));
    }

    // ========================================================================
    // Amendment Operations
    // ========================================================================

    /// <summary>Creates an amendment to a contract.</summary>
    [HttpPost("{id}/amendments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAmendment(int id, [FromBody] CreateContractRequest request, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var amendment = new Contract
        {
            Name = request.Name ?? $"Amendment to {contract.Name}",
            Description = request.Description,
            ContractType = ContractType.Amendment,
            AccountId = contract.AccountId,
            ContactId = request.ContactId ?? contract.ContactId,
            OwnerId = request.OwnerId ?? contract.OwnerId,
            StartDate = request.StartDate ?? DateTime.UtcNow,
            EndDate = request.EndDate ?? contract.EndDate,
            Value = request.Value ?? 0,
            CurrencyCode = request.CurrencyCode ?? contract.CurrencyCode,
            Terms = request.Terms,
            SpecialConditions = request.SpecialConditions,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _contractService.CreateAmendmentAsync(id, amendment, cancellationToken);
        _logger.LogInformation("Amendment created for contract {ContractId}, new amendment {AmendmentId}", id, created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    /// <summary>Gets amendments for a contract.</summary>
    [HttpGet("{id}/amendments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAmendments(int id, CancellationToken cancellationToken = default)
    {
                var amendments = await _contractService.GetAmendmentsAsync(id, cancellationToken);
        return Ok(amendments.Select(c => MapToDto(c)));
    }

    // ========================================================================
    // Signature Operations
    // ========================================================================

    /// <summary>Sends a contract for signature.</summary>
    [HttpPost("{id}/send-for-signature")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendForSignature(int id, [FromBody] SendForSignatureRequest request, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var signers = request.Signers.Select(s => new ContractSigner
        {
            ContactId = s.ContactId,
            Name = s.Name,
            Email = s.Email,
            Title = s.Title,
            SigningOrder = s.SigningOrder
        });

        var result = await _contractService.SendForSignatureAsync(id, signers, cancellationToken);
        if (!result)
        {
            return StatusCode(500, new { message = "Failed to send contract for signature" });
        }

        _logger.LogInformation("Contract {ContractId} sent for signature", id);
        return Ok(new { message = "Contract sent for signature", contractId = id });
    }

    /// <summary>Gets signature status for a contract.</summary>
    [HttpGet("{id}/signature-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSignatureStatus(int id, CancellationToken cancellationToken = default)
    {
                var status = await _contractService.GetSignatureStatusAsync(id, cancellationToken);
        return Ok(status);
    }

    // ========================================================================
    // Document Operations
    // ========================================================================

    /// <summary>Gets documents for a contract.</summary>
    [HttpGet("{id}/documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocuments(int id, CancellationToken cancellationToken = default)
    {
                var documents = await _contractService.GetDocumentsAsync(id, cancellationToken);
        return Ok(documents);
    }

    /// <summary>Generates a PDF for a contract.</summary>
    [HttpGet("{id}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GeneratePdf(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var pdf = await _contractService.GenerateContractPdfAsync(id, cancellationToken);
        return File(pdf, "application/pdf", $"contract-{contract.ContractNumber}.pdf");
    }

    // ========================================================================
    // Queries & Reports
    // ========================================================================

    /// <summary>Gets active contracts for an account.</summary>
    [HttpGet("active/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveContracts(int accountId, CancellationToken cancellationToken = default)
    {
                var contracts = await _contractService.GetActiveContractsAsync(accountId, cancellationToken);
        return Ok(contracts.Select(c => MapToDto(c)));
    }

    /// <summary>Gets expiring contracts within a date range.</summary>
    [HttpGet("expiring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExpiringContracts(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
                var fromDate = DateTime.UtcNow;
        var toDate = DateTime.UtcNow.AddDays(days);
        var contracts = await _contractService.GetExpiringContractsAsync(fromDate, toDate, cancellationToken);
        return Ok(contracts.Select(c => MapToDto(c)));
    }

    /// <summary>Gets contract statistics.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
                var stats = await _contractService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>Searches contracts.</summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search([FromQuery] string q = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Search query is required" });
        }

        var contracts = await _contractService.SearchAsync(q, cancellationToken);
        return Ok(contracts.Select(c => MapToDto(c)));
    }

    /// <summary>Gets total contract value for an account.</summary>
    [HttpGet("value/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalContractValue(int accountId, CancellationToken cancellationToken = default)
    {
                var value = await _contractService.GetTotalContractValueAsync(accountId, cancellationToken);
        return Ok(new { accountId, totalValue = value });
    }

    // ========================================================================
    // Create from Other Entities
    // ========================================================================

    /// <summary>Creates a contract from a quote.</summary>
    [HttpPost("from-quote/{quoteId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFromQuote(int quoteId, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.CreateFromQuoteAsync(quoteId, cancellationToken);
        _logger.LogInformation("Contract {ContractId} created from quote {QuoteId}", contract.Id, quoteId);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, MapToDto(contract));
    }

    /// <summary>Creates a contract from an order.</summary>
    [HttpPost("from-order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFromOrder(int orderId, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.CreateFromOrderAsync(orderId, cancellationToken);
        _logger.LogInformation("Contract {ContractId} created from order {OrderId}", contract.Id, orderId);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, MapToDto(contract));
    }

    // ========================================================================
    // Bulk Operations (TODO-SALES005-013)
    // ========================================================================

    /// <summary>Bulk updates the status of multiple contracts.</summary>
    [HttpPost("bulk-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ContractIds == null || request.ContractIds.Count == 0)
        {
            return BadRequest(new { message = "At least one contract ID is required" });
        }

        if (!Enum.IsDefined(typeof(ContractStatus), request.Status))
        {
            return BadRequest(new { message = "Invalid contract status" });
        }

        var updatedCount = await _contractService.BulkUpdateStatusAsync(
            request.ContractIds,
            request.Status,
            cancellationToken);

        _logger.LogInformation("Bulk status update: {UpdatedCount}/{TotalCount} contracts updated to {Status}",
            updatedCount, request.ContractIds.Count, request.Status);

        return Ok(new
        {
            totalRequested = request.ContractIds.Count,
            successCount = updatedCount,
            failedCount = request.ContractIds.Count - updatedCount,
            newStatus = request.Status.ToString()
        });
    }

    /// <summary>Request DTO for bulk status update.</summary>
    public class BulkStatusUpdateRequest
    {
        public List<int> ContractIds { get; set; } = new();
        public ContractStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    // ========================================================================
    // Export Operations (TODO-SALES005-014)
    // ========================================================================

    /// <summary>Exports a contract to the specified format (PDF, Excel, Word).</summary>
    [HttpGet("{id}/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Export(int id, [FromQuery] string format = "pdf", CancellationToken cancellationToken = default)
    {
        try
        {
            var supportedFormats = _contractExportService.GetSupportedFormats();
            if (!supportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"Unsupported format '{format}'. Supported formats: {string.Join(", ", supportedFormats)}" });
            }

            var result = await _contractExportService.ExportAsync(id, format, cancellationToken);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }
    }

    /// <summary>Exports multiple contracts in bulk.</summary>
    [HttpPost("export-bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportBulk([FromBody] BulkExportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ContractIds == null || request.ContractIds.Length == 0)
        {
            return BadRequest(new { message = "At least one contract ID is required" });
        }

        var result = await _contractExportService.ExportBulkAsync(
            request.ContractIds,
            request.Format ?? "pdf",
            cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>Gets supported export formats.</summary>
    [HttpGet("export-formats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetExportFormats()
    {
        var formats = _contractExportService.GetSupportedFormats();
        return Ok(new { formats });
    }

    /// <summary>Request DTO for bulk export.</summary>
    public class BulkExportRequest
    {
        public int[] ContractIds { get; set; } = Array.Empty<int>();
        public string? Format { get; set; }
    }

    // ========================================================================
    // Version History Operations (TODO-SALES005-016)
    // ========================================================================

    /// <summary>Gets the version history of a contract.</summary>
    [HttpGet("{id}/versions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetVersionHistory(int id, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var versions = await _contractService.GetVersionHistoryAsync(id, cancellationToken);
        return Ok(new
        {
            contractId = id,
            versions = versions.Select(v => new
            {
                v.Id,
                v.VersionNumber,
                v.ChangeDescription,
                ModifiedByUserId = v.CreatedById,
                v.CreatedAt,
                SnapshotData = v.SnapshotJson
            })
        });
    }

    /// <summary>Creates a version snapshot of the current contract state.</summary>
    [HttpPost("{id}/versions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateVersionSnapshot(int id, [FromBody] CreateVersionRequest request, CancellationToken cancellationToken = default)
    {
                var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract == null)
        {
            return NotFound(new { message = string.Format(ContractNotFoundMessage, id) });
        }

        var version = await _contractService.CreateVersionSnapshotAsync(
            id,
            request.ChangeDescription ?? "Manual snapshot",
            request.ModifiedByUserId,
            cancellationToken);

        _logger.LogInformation("Created version snapshot {VersionId} for contract {ContractId}", version.Id, id);

        return CreatedAtAction(nameof(GetVersionHistory), new { id }, new
        {
            version.Id,
            version.VersionNumber,
            version.ChangeDescription,
            ModifiedByUserId = version.CreatedById,
            version.CreatedAt
        });
    }

    /// <summary>Restores a contract to a previous version.</summary>
    [HttpPost("{id}/versions/{versionId}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreVersion(int id, int versionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractService.RestoreVersionAsync(id, versionId, cancellationToken);

            _logger.LogInformation("Restored contract {ContractId} to version {VersionId}", id, versionId);

            return Ok(MapToDto(contract));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Request DTO for creating a version snapshot.</summary>
    public class CreateVersionRequest
    {
        public string? ChangeDescription { get; set; }
        public int? ModifiedByUserId { get; set; }
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================
    private static ContractDto MapToDto(Contract c)
    {
        return new ContractDto
        {
            // Identification
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            Name = c.Name,
            Description = c.Description,

            // Status & Type (enum values mapped directly)
            Status = c.Status,
            ContractType = c.ContractType,

            // Relationships
            AccountId = c.AccountId,
            AccountName = c.Account?.Company ?? c.Account?.FirstName,
            ContactId = c.ContactId,
            ContactName = c.Contact != null ? $"{c.Contact.FirstName} {c.Contact.LastName}" : null,
            OwnerId = c.OwnerId,
            OwnerName = c.Owner != null ? $"{c.Owner.FirstName} {c.Owner.LastName}" : null,
            ParentContractId = c.ParentContractId,
            OpportunityId = c.OpportunityId,
            QuoteId = c.QuoteId,

            // Dates
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            SignedDate = c.SignedDate,
            ActivatedDate = c.ActivatedDate,
            ActivatedAt = c.ActivatedDate,
            TerminatedDate = c.TerminatedDate,
            TerminatedAt = c.TerminatedDate,

            // Financial
            TotalValue = c.Value,
            CurrencyCode = c.CurrencyCode,
            BillingFrequency = c.BillingFrequency,

            // Renewal
            AutoRenew = c.AutoRenew,
            RenewalNoticeDays = c.RenewalNoticeDays,
            RenewalNoticeSent = c.RenewalNoticeSent,
            RenewalNoticeSentDate = c.RenewalNoticeSentDate,
            RenewalInitiatedAt = c.RenewalInitiatedAt,
            RenewalCompletedAt = c.RenewalCompletedAt,

            // Terms
            TermsAndConditions = c.Terms,
            SpecialConditions = c.SpecialConditions,
            TerminationClause = c.TerminationClause,
            TerminationReason = c.TerminationReason,
            PaymentTerms = c.PaymentTerms,

            // Documents
            ContractFileUrl = c.ContractFileUrl,
            ContractFileName = c.ContractFileName,
            ContractFileSize = c.ContractFileSize,
            ContractFileMimeType = c.ContractFileMimeType,
            SignedContractFileUrl = c.SignedContractFileUrl,
            SignedContractFileName = c.SignedContractFileName,
            SentForSignatureAt = c.SentForSignatureAt,
            SignedBy = c.SignedBy,

            // Approval
            ApprovedByUserId = c.ApprovedByUserId,
            ApprovedByName = c.ApprovedByUser != null ? $"{c.ApprovedByUser.FirstName} {c.ApprovedByUser.LastName}" : null,
            ApprovedDate = c.ApprovedDate,
            RejectionReason = c.RejectionReason,

            // Suspension
            SuspensionReason = c.SuspensionReason,
            SuspendedDate = c.SuspendedDate,

            // Computed (IsSigned computed by DTO from SignedDate; IsExpiringSoon and DaysUntilExpiry are DTO computed properties)
            IsSigned = c.IsSigned,

            // Timestamps
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt ?? DateTime.UtcNow
        };
    }

    // ========================================================================
    // Request DTOs
    // ========================================================================

    /// <summary>Request DTO for creating a contract.</summary>
    public class CreateContractRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ContractStatus? Status { get; set; }
        public ContractType? ContractType { get; set; }
        public int AccountId { get; set; }
        public int? ContactId { get; set; }
        public int? OwnerId { get; set; }
        public int? ParentContractId { get; set; }
        public int? OpportunityId { get; set; }
        public int? QuoteId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public decimal? Value { get; set; }
        public string? CurrencyCode { get; set; }
        public string? BillingFrequency { get; set; }
        public bool? AutoRenew { get; set; }
        public int? RenewalNoticeDays { get; set; }
        public string? Terms { get; set; }
        public string? SpecialConditions { get; set; }
        public string? TerminationClause { get; set; }
    }

    /// <summary>Request DTO for updating a contract (all fields nullable for patching).</summary>
    public class UpdateContractRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ContractStatus? Status { get; set; }
        public ContractType? ContractType { get; set; }
        public int? AccountId { get; set; }
        public int? ContactId { get; set; }
        public int? OwnerId { get; set; }
        public int? ParentContractId { get; set; }
        public int? OpportunityId { get; set; }
        public int? QuoteId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public decimal? Value { get; set; }
        public string? CurrencyCode { get; set; }
        public string? BillingFrequency { get; set; }
        public bool? AutoRenew { get; set; }
        public int? RenewalNoticeDays { get; set; }
        public string? Terms { get; set; }
        public string? SpecialConditions { get; set; }
        public string? TerminationClause { get; set; }
    }

    /// <summary>Request DTO for rejecting a contract.</summary>
    public class RejectContractRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Request DTO for suspending a contract.</summary>
    public class SuspendContractRequest
    {
        public string? Reason { get; set; }
    }

    /// <summary>Request DTO for terminating a contract.</summary>
    public class TerminateContractRequest
    {
        public string? Reason { get; set; }
    }

    /// <summary>Request DTO for renewing a contract.</summary>
    public class RenewContractRequest
    {
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public decimal? NewValue { get; set; }
    }

    /// <summary>Request DTO for sending contract for signature.</summary>
    public class SendForSignatureRequest
    {
        public List<SignerInput> Signers { get; set; } = new();
    }

    /// <summary>Signer information for signature request.</summary>
    public class SignerInput
    {
        public int? ContactId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int SigningOrder { get; set; }
    }
}
