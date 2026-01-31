using CRM.Infrastructure.Data;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly CrmDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AccountsController> _logger;
    private readonly IConfiguration _config;
    private readonly IAccountService _accountService;
    private readonly ICustomerService _customerService;

    // defaults
    private const long DefaultMaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] DefaultAllowedMimeTypes = new[] { "application/pdf", "image/png", "image/jpeg", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

    public AccountsController(
        CrmDbContext db, 
        IWebHostEnvironment env, 
        ILogger<AccountsController> logger, 
        IConfiguration config, 
        IAccountService accountService,
        ICustomerService customerService)
    {
        _db = db;
        _env = env;
        _logger = logger;
        _config = config;
        _accountService = accountService;
        _customerService = customerService;
    }

    // ============================================================
    // CRUD Operations for Accounts (delegating to CustomerService)
    // ============================================================

    /// <summary>
    /// Get all accounts (customers).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(500, new { message = "Error retrieving accounts", error = ex.Message });
        }
    }

    /// <summary>
    /// Get account by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Account not found" });
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {Id}", id);
            return StatusCode(500, new { message = "Error retrieving account", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new account.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto request)
    {
        try
        {
            var result = await _customerService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return StatusCode(500, new { message = "Error creating account", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing account.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto request)
    {
        try
        {
            var result = await _customerService.UpdateCustomerAsync(id, request);
            if (result == null)
                return NotFound(new { message = "Account not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {Id}", id);
            return StatusCode(500, new { message = "Error updating account", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an account.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            if (!success)
                return NotFound(new { message = "Account not found" });
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            return StatusCode(500, new { message = "Error deleting account", error = ex.Message });
        }
    }

    // ============================================================
    // Contract Management Operations
    // ============================================================

    private string GetStoragePath()
    {
        var configured = _config["CONTRACT_STORAGE_PATH"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        // default to app data folder
        return Path.Combine(_env.ContentRootPath, "data", "contracts");
    }

    private long GetMaxFileSize()
    {
        var s = _config["MAX_CONTRACT_FILE_SIZE_BYTES"];
        if (long.TryParse(s, out var v) && v > 0) return v;
        return DefaultMaxFileSize;
    }

    private string[] GetAllowedMimeTypes()
    {
        var s = _config["ALLOWED_CONTRACT_MIME_TYPES"];
        if (string.IsNullOrWhiteSpace(s)) return DefaultAllowedMimeTypes;
        return s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Upload a contract document for an account. Multipart form file under key `file`.
    /// </summary>
    [HttpPost("{accountId}/upload-contract")]
    public async Task<IActionResult> UploadContract(int accountId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var maxSize = GetMaxFileSize();
        if (file.Length > maxSize)
            return BadRequest(new { message = $"File too large. Max allowed is {maxSize} bytes" });

        var allowed = GetAllowedMimeTypes();
        if (!string.IsNullOrWhiteSpace(file.ContentType) && !allowed.Contains(file.ContentType))
            return BadRequest(new { message = $"File type '{file.ContentType}' is not allowed" });

        var account = await _db.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        var contractsDir = GetStoragePath();
        Directory.CreateDirectory(contractsDir);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var physicalPath = Path.Combine(contractsDir, uniqueFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        account.ContractFileName = file.FileName;
        // store a relative path where possible; if configured path is inside content root, store relative
        var contentRoot = _env.ContentRootPath.TrimEnd(Path.DirectorySeparatorChar, '/');
        if (physicalPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
        {
            account.ContractFilePath = physicalPath.Substring(contentRoot.Length).TrimStart(Path.DirectorySeparatorChar, '/').Replace("\\", "/");
        }
        else
        {
            account.ContractFilePath = physicalPath.Replace("\\", "/");
        }

        account.ContractContentType = file.ContentType;
        account.ContractFileSize = file.Length;
        account.UpdatedAt = DateTime.UtcNow;

        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            account.Id,
            account.ContractFileName,
            account.ContractFilePath,
            account.ContractContentType,
            account.ContractFileSize
        });
    }

    /// <summary>
    /// Download the contract file for an account (if present).
    /// </summary>
    [HttpGet("{accountId}/contract")]
    public async Task<IActionResult> DownloadContract(int accountId)
    {
        var account = await _db.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        if (string.IsNullOrWhiteSpace(account.ContractFilePath))
            return NotFound(new { message = "No contract uploaded for this account" });

        string physicalPath;
        // if stored relative to content root, resolve
        if (!Path.IsPathRooted(account.ContractFilePath))
            physicalPath = Path.Combine(_env.ContentRootPath, account.ContractFilePath.Replace('/', Path.DirectorySeparatorChar));
        else
            physicalPath = account.ContractFilePath;

        if (!System.IO.File.Exists(physicalPath))
            return NotFound(new { message = "Contract file not found on server" });

        var contentType = account.ContractContentType ?? "application/octet-stream";
        var fileName = account.ContractFileName ?? Path.GetFileName(physicalPath);
        return PhysicalFile(physicalPath, contentType, fileName);
    }

    /// <summary>
    /// Delete the uploaded contract file for an account (removes file and clears metadata).
    /// </summary>
    [HttpDelete("{accountId}/contract")]
    public async Task<IActionResult> DeleteContract(int accountId)
    {
        var account = await _db.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        if (!string.IsNullOrWhiteSpace(account.ContractFilePath))
        {
            string physicalPath;
            if (!Path.IsPathRooted(account.ContractFilePath))
                physicalPath = Path.Combine(_env.ContentRootPath, account.ContractFilePath.Replace('/', Path.DirectorySeparatorChar));
            else
                physicalPath = account.ContractFilePath;

            try
            {
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed deleting contract file {Path}", physicalPath);
            }

            account.ContractFileName = null;
            account.ContractFilePath = null;
            account.ContractContentType = null;
            account.ContractFileSize = null;
            account.UpdatedAt = DateTime.UtcNow;

            _db.Accounts.Update(account);
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Contract removed" });
    }
}
