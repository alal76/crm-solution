// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the Source-Available License (see LICENSE) as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.SalesService.Controllers;

/// <summary>
/// API controller for managing contracts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(CrmDbContext context, ILogger<ContractsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all contracts with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? status = null,
        [FromQuery] string? contractType = null,
        [FromQuery] int? accountId = null,
        [FromQuery] bool? expiringSoon = null)
    {
        try
        {
            var query = _context.Contracts
                .Include(c => c.Account)
                .Include(c => c.Contact)
                .Include(c => c.Owner)
                .Where(c => !c.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(contractType) && Enum.TryParse<ContractType>(contractType, true, out var typeEnum))
            {
                query = query.Where(c => c.ContractType == typeEnum);
            }

            if (accountId.HasValue)
            {
                query = query.Where(c => c.AccountId == accountId.Value);
            }

            if (expiringSoon == true)
            {
                var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
                query = query.Where(c => c.Status == ContractStatus.Active && c.EndDate <= thirtyDaysFromNow);
            }

            var totalCount = await query.CountAsync();
            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.ContractNumber,
                    c.Name,
                    c.Description,
                    Status = c.Status.ToString(),
                    StatusValue = (int)c.Status,
                    ContractType = c.ContractType.ToString(),
                    ContractTypeValue = (int)c.ContractType,
                    c.AccountId,
                    AccountName = c.Account != null
                        ? (c.Account.Category == AccountCategory.Organization
                            ? c.Account.Company
                            : $"{c.Account.FirstName} {c.Account.LastName}")
                        : null,
                    c.ContactId,
                    ContactName = c.Contact != null ? $"{c.Contact.FirstName} {c.Contact.LastName}" : null,
                    c.OwnerId,
                    OwnerName = c.Owner != null ? $"{c.Owner.FirstName} {c.Owner.LastName}" : null,
                    c.StartDate,
                    c.EndDate,
                    c.SignedDate,
                    c.Value,
                    c.CurrencyCode,
                    c.BillingFrequency,
                    c.AutoRenew,
                    c.RenewalNoticeDays,
                    c.ContractFileName,
                    c.ContractFileUrl,
                    c.ParentContractId,
                    c.OpportunityId,
                    c.QuoteId,
                    c.DaysUntilExpiration,
                    c.IsExpiringSoon,
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = contracts,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contracts");
            return StatusCode(500, "An error occurred while retrieving contracts");
        }
    }

    /// <summary>
    /// Get contract by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .Include(c => c.Account)
                .Include(c => c.Contact)
                .Include(c => c.Owner)
                .Include(c => c.ParentContract)
                .Include(c => c.Opportunity)
                .Include(c => c.Quote)
                .Include(c => c.ApprovedByUser)
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    c.ContractNumber,
                    c.Name,
                    c.Description,
                    Status = c.Status.ToString(),
                    StatusValue = (int)c.Status,
                    ContractType = c.ContractType.ToString(),
                    ContractTypeValue = (int)c.ContractType,
                    c.AccountId,
                    AccountName = c.Account != null
                        ? (c.Account.Category == AccountCategory.Organization
                            ? c.Account.Company
                            : $"{c.Account.FirstName} {c.Account.LastName}")
                        : null,
                    c.ContactId,
                    ContactName = c.Contact != null ? $"{c.Contact.FirstName} {c.Contact.LastName}" : null,
                    c.OwnerId,
                    OwnerName = c.Owner != null ? $"{c.Owner.FirstName} {c.Owner.LastName}" : null,
                    c.ParentContractId,
                    ParentContractNumber = c.ParentContract != null ? c.ParentContract.ContractNumber : null,
                    c.OpportunityId,
                    OpportunityName = c.Opportunity != null ? c.Opportunity.Name : null,
                    c.QuoteId,
                    QuoteNumber = c.Quote != null ? c.Quote.QuoteNumber : null,
                    c.StartDate,
                    c.EndDate,
                    c.SignedDate,
                    c.ActivatedDate,
                    c.TerminatedDate,
                    c.Value,
                    c.CurrencyCode,
                    c.BillingFrequency,
                    c.AutoRenew,
                    c.RenewalNoticeDays,
                    c.RenewalNoticeSent,
                    c.RenewalNoticeSentDate,
                    c.Terms,
                    c.SpecialConditions,
                    c.TerminationClause,
                    c.ContractFileUrl,
                    c.ContractFileName,
                    c.ContractFileSize,
                    c.ContractFileMimeType,
                    c.SignedContractFileUrl,
                    c.SignedContractFileName,
                    c.ApprovedByUserId,
                    ApprovedByName = c.ApprovedByUser != null ? $"{c.ApprovedByUser.FirstName} {c.ApprovedByUser.LastName}" : null,
                    c.ApprovedDate,
                    c.RejectionReason,
                    c.DaysUntilExpiration,
                    c.IsExpiringSoon,
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
            return StatusCode(500, "An error occurred while retrieving the contract");
        }
    }

    /// <summary>
    /// Create a new contract
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
    {
        try
        {
            var contract = new Contract
            {
                ContractNumber = $"CON-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Name = request.Name,
                Description = request.Description,
                Status = Enum.TryParse<ContractStatus>(request.Status, true, out var status) ? status : ContractStatus.Draft,
                ContractType = Enum.TryParse<ContractType>(request.ContractType, true, out var type) ? type : ContractType.Service,
                AccountId = request.AccountId,
                ContactId = request.ContactId,
                OwnerId = request.OwnerId ?? GetCurrentUserId(),
                ParentContractId = request.ParentContractId,
                OpportunityId = request.OpportunityId,
                QuoteId = request.QuoteId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SignedDate = request.SignedDate,
                Value = request.Value,
                CurrencyCode = request.CurrencyCode ?? "USD",
                BillingFrequency = request.BillingFrequency,
                AutoRenew = request.AutoRenew,
                RenewalNoticeDays = request.RenewalNoticeDays,
                Terms = request.Terms,
                SpecialConditions = request.SpecialConditions,
                TerminationClause = request.TerminationClause,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created contract {ContractId} - {ContractNumber}", contract.Id, contract.ContractNumber);
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, new { contract.Id, contract.ContractNumber, contract.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            return StatusCode(500, "An error occurred while creating the contract");
        }
    }

    /// <summary>
    /// Update a contract
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContractRequest request)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            // Update fields
            if (request.Name != null) contract.Name = request.Name;
            if (request.Description != null) contract.Description = request.Description;
            if (request.Status != null && Enum.TryParse<ContractStatus>(request.Status, true, out var status))
            {
                contract.Status = status;
                if (status == ContractStatus.Active && !contract.ActivatedDate.HasValue)
                    contract.ActivatedDate = DateTime.UtcNow;
                if (status == ContractStatus.Terminated && !contract.TerminatedDate.HasValue)
                    contract.TerminatedDate = DateTime.UtcNow;
            }
            if (request.ContractType != null && Enum.TryParse<ContractType>(request.ContractType, true, out var type))
                contract.ContractType = type;
            if (request.AccountId.HasValue) contract.AccountId = request.AccountId.Value;
            if (request.ContactId.HasValue) contract.ContactId = request.ContactId;
            if (request.OwnerId.HasValue) contract.OwnerId = request.OwnerId;
            if (request.ParentContractId.HasValue) contract.ParentContractId = request.ParentContractId;
            if (request.OpportunityId.HasValue) contract.OpportunityId = request.OpportunityId;
            if (request.QuoteId.HasValue) contract.QuoteId = request.QuoteId;
            if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
            if (request.SignedDate.HasValue) contract.SignedDate = request.SignedDate;
            if (request.Value.HasValue) contract.Value = request.Value.Value;
            if (request.CurrencyCode != null) contract.CurrencyCode = request.CurrencyCode;
            if (request.BillingFrequency != null) contract.BillingFrequency = request.BillingFrequency;
            contract.AutoRenew = request.AutoRenew ?? contract.AutoRenew;
            if (request.RenewalNoticeDays.HasValue) contract.RenewalNoticeDays = request.RenewalNoticeDays.Value;
            if (request.Terms != null) contract.Terms = request.Terms;
            if (request.SpecialConditions != null) contract.SpecialConditions = request.SpecialConditions;
            if (request.TerminationClause != null) contract.TerminationClause = request.TerminationClause;

            contract.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated contract {ContractId}", id);
            return Ok(new { message = "Contract updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract {ContractId}", id);
            return StatusCode(500, "An error occurred while updating the contract");
        }
    }

    /// <summary>
    /// Delete a contract (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            contract.IsDeleted = true;
            contract.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted contract {ContractId}", id);
            return Ok(new { message = "Contract deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract {ContractId}", id);
            return StatusCode(500, "An error occurred while deleting the contract");
        }
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Approve a contract
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (contract.Status != ContractStatus.PendingApproval)
                return BadRequest(new { message = "Contract must be in Pending Approval status to approve" });

            contract.Status = ContractStatus.Approved;
            contract.ApprovedByUserId = GetCurrentUserId();
            contract.ApprovedDate = DateTime.UtcNow;
            contract.RejectionReason = null;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Approved contract {ContractId}", id);
            return Ok(new { message = "Contract approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving contract {ContractId}", id);
            return StatusCode(500, "An error occurred while approving the contract");
        }
    }

    /// <summary>
    /// Reject a contract
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectContractRequest request)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (contract.Status != ContractStatus.PendingApproval)
                return BadRequest(new { message = "Contract must be in Pending Approval status to reject" });

            contract.Status = ContractStatus.Draft;
            contract.RejectionReason = request.Reason;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rejected contract {ContractId}", id);
            return Ok(new { message = "Contract rejected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting contract {ContractId}", id);
            return StatusCode(500, "An error occurred while rejecting the contract");
        }
    }

    /// <summary>
    /// Activate a contract
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (contract.Status != ContractStatus.Approved && contract.Status != ContractStatus.OnHold)
                return BadRequest(new { message = "Contract must be Approved or On Hold to activate" });

            contract.Status = ContractStatus.Active;
            contract.ActivatedDate = DateTime.UtcNow;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Activated contract {ContractId}", id);
            return Ok(new { message = "Contract activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating contract {ContractId}", id);
            return StatusCode(500, "An error occurred while activating the contract");
        }
    }

    /// <summary>
    /// Terminate a contract
    /// </summary>
    [HttpPost("{id}/terminate")]
    public async Task<IActionResult> Terminate(int id, [FromBody] TerminateContractRequest request)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            contract.Status = ContractStatus.Terminated;
            contract.TerminatedDate = DateTime.UtcNow;
            contract.TerminationClause = request.Reason;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Terminated contract {ContractId}", id);
            return Ok(new { message = "Contract terminated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating contract {ContractId}", id);
            return StatusCode(500, "An error occurred while terminating the contract");
        }
    }

    /// <summary>
    /// Renew a contract
    /// </summary>
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> Renew(int id, [FromBody] RenewContractRequest request)
    {
        try
        {
            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (existingContract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            // Mark existing contract as renewed
            existingContract.Status = ContractStatus.Renewed;
            existingContract.UpdatedAt = DateTime.UtcNow;

            // Create new contract as renewal
            var newContract = new Contract
            {
                ContractNumber = $"CON-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Name = $"{existingContract.Name} - Renewal",
                Description = existingContract.Description,
                Status = ContractStatus.Draft,
                ContractType = existingContract.ContractType,
                AccountId = existingContract.AccountId,
                ContactId = existingContract.ContactId,
                OwnerId = existingContract.OwnerId,
                ParentContractId = existingContract.Id,
                OpportunityId = existingContract.OpportunityId,
                StartDate = request.NewStartDate ?? existingContract.EndDate.AddDays(1),
                EndDate = request.NewEndDate ?? existingContract.EndDate.AddYears(1),
                Value = request.NewValue ?? existingContract.Value,
                CurrencyCode = existingContract.CurrencyCode,
                BillingFrequency = existingContract.BillingFrequency,
                AutoRenew = existingContract.AutoRenew,
                RenewalNoticeDays = existingContract.RenewalNoticeDays,
                Terms = existingContract.Terms,
                SpecialConditions = existingContract.SpecialConditions,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(newContract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Renewed contract {ContractId} with new contract {NewContractId}", id, newContract.Id);
            return Ok(new {
                message = "Contract renewed successfully",
                newContractId = newContract.Id,
                newContractNumber = newContract.ContractNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing contract {ContractId}", id);
            return StatusCode(500, "An error occurred while renewing the contract");
        }
    }

    #endregion

    #region File Management

    /// <summary>
    /// Upload contract file
    /// </summary>
    [HttpPost("{id}/upload")]
    public async Task<IActionResult> UploadFile(int id, IFormFile file)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided" });

            // Validate file type
            var allowedTypes = new[] { "application/pdf", "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only PDF and Word documents are allowed" });

            // Create contracts directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{contract.ContractNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update contract
            contract.ContractFileUrl = $"/uploads/contracts/{fileName}";
            contract.ContractFileName = file.FileName;
            contract.ContractFileSize = file.Length;
            contract.ContractFileMimeType = file.ContentType;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Uploaded file for contract {ContractId}", id);
            return Ok(new {
                message = "File uploaded successfully",
                fileName = contract.ContractFileName,
                fileUrl = contract.ContractFileUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for contract {ContractId}", id);
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    /// <summary>
    /// Download contract file
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (string.IsNullOrEmpty(contract.ContractFileUrl))
                return NotFound(new { message = "No file attached to this contract" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                contract.ContractFileUrl.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found on server" });

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contract.ContractFileMimeType ?? "application/octet-stream",
                contract.ContractFileName ?? "contract.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file for contract {ContractId}", id);
            return StatusCode(500, "An error occurred while downloading the file");
        }
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get contracts expiring soon
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 30)
    {
        try
        {
            var expirationDate = DateTime.UtcNow.AddDays(days);
            var contracts = await _context.Contracts
                .Include(c => c.Account)
                .Where(c => !c.IsDeleted && c.Status == ContractStatus.Active && c.EndDate <= expirationDate)
                .OrderBy(c => c.EndDate)
                .Select(c => new
                {
                    c.Id,
                    c.ContractNumber,
                    c.Name,
                    c.AccountId,
                    AccountName = c.Account != null
                        ? (c.Account.Category == AccountCategory.Organization
                            ? c.Account.Company
                            : $"{c.Account.FirstName} {c.Account.LastName}")
                        : null,
                    c.EndDate,
                    c.Value,
                    c.DaysUntilExpiration,
                    c.AutoRenew,
                    c.RenewalNoticeSent
                })
                .ToListAsync();

            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expiring contracts");
            return StatusCode(500, "An error occurred while retrieving expiring contracts");
        }
    }

    /// <summary>
    /// Get contract statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _context.Contracts
                .Where(c => !c.IsDeleted)
                .GroupBy(c => 1)
                .Select(g => new
                {
                    TotalContracts = g.Count(),
                    ActiveContracts = g.Count(c => c.Status == ContractStatus.Active),
                    DraftContracts = g.Count(c => c.Status == ContractStatus.Draft),
                    ExpiredContracts = g.Count(c => c.Status == ContractStatus.Expired),
                    TotalValue = g.Where(c => c.Status == ContractStatus.Active).Sum(c => c.Value),
                    ExpiringThisMonth = g.Count(c => c.Status == ContractStatus.Active &&
                        c.EndDate <= DateTime.UtcNow.AddDays(30)),
                    RenewedContracts = g.Count(c => c.Status == ContractStatus.Renewed)
                })
                .FirstOrDefaultAsync();

            return Ok(stats ?? new
            {
                TotalContracts = 0,
                ActiveContracts = 0,
                DraftContracts = 0,
                ExpiredContracts = 0,
                TotalValue = 0m,
                ExpiringThisMonth = 0,
                RenewedContracts = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }

    #endregion
}

#region Request DTOs

public class CreateContractRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ContractType { get; set; }
    public int AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OwnerId { get; set; }
    public int? ParentContractId { get; set; }
    public int? OpportunityId { get; set; }
    public int? QuoteId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public decimal Value { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BillingFrequency { get; set; }
    public bool AutoRenew { get; set; }
    public int RenewalNoticeDays { get; set; } = 30;
    public string? Terms { get; set; }
    public string? SpecialConditions { get; set; }
    public string? TerminationClause { get; set; }
}

public class UpdateContractRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? ContractType { get; set; }
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

public class RejectContractRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class TerminateContractRequest
{
    public string? Reason { get; set; }
}

public class RenewContractRequest
{
    public DateTime? NewStartDate { get; set; }
    public DateTime? NewEndDate { get; set; }
    public decimal? NewValue { get; set; }
}

#endregion
