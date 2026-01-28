using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing user approval workflow
/// </summary>
public class UserApprovalService : IUserApprovalService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<UserApprovalService> _logger;

    public UserApprovalService(ICrmDbContext context, ILogger<UserApprovalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserApprovalRequestDto>> GetApprovalRequestsAsync(int? status = null)
    {
        try
        {
            var query = _context.UserApprovalRequests.AsQueryable();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var requests = await query
                .Include(r => r.ReviewedByUser)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new UserApprovalRequestDto
                {
                    Id = r.Id,
                    Email = r.Email,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Company = r.Company,
                    Phone = r.Phone,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByUserName = r.ReviewedByUser != null ? $"{r.ReviewedByUser.FirstName} {r.ReviewedByUser.LastName}" : null,
                    RejectionReason = r.RejectionReason
                })
                .ToListAsync();

            return requests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving approval requests");
            throw;
        }
    }

    public async Task<UserApprovalRequestDto?> GetApprovalRequestByIdAsync(int id)
    {
        try
        {
            var request = await _context.UserApprovalRequests
                .Include(r => r.ReviewedByUser)
                .Where(r => r.Id == id)
                .Select(r => new UserApprovalRequestDto
                {
                    Id = r.Id,
                    Email = r.Email,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Company = r.Company,
                    Phone = r.Phone,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByUserName = r.ReviewedByUser != null ? $"{r.ReviewedByUser.FirstName} {r.ReviewedByUser.LastName}" : null,
                    RejectionReason = r.RejectionReason
                })
                .FirstOrDefaultAsync();

            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving approval request {id}");
            throw;
        }
    }

    public async Task CreateApprovalRequestAsync(string email, string firstName, string lastName, string? company = null, string? phone = null)
    {
        try
        {
            var existingRequest = await _context.UserApprovalRequests
                .FirstOrDefaultAsync(r => r.Email == email && r.Status == (int)ApprovalStatus.Pending);

            if (existingRequest != null)
                throw new InvalidOperationException("A pending approval request for this email already exists");

            var approvalRequest = new UserApprovalRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Company = company,
                Phone = phone,
                Status = (int)ApprovalStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.UserApprovalRequests.Add(approvalRequest);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating approval request for {email}");
            throw;
        }
    }

    public async Task<UserDto> ApproveUserAsync(int approvalRequestId, int reviewedByUserId, ApproveUserRequest request)
    {
        try
        {
            var approvalRequest = await _context.UserApprovalRequests.FindAsync(approvalRequestId);
            if (approvalRequest == null)
                throw new KeyNotFoundException($"Approval request {approvalRequestId} not found");

            if (approvalRequest.Status != (int)ApprovalStatus.Pending)
                throw new InvalidOperationException("This approval request has already been reviewed");

            // Use stored password hash if available, otherwise generate a temporary password
            string passwordHash;
            if (!string.IsNullOrEmpty(approvalRequest.PasswordHash))
            {
                passwordHash = approvalRequest.PasswordHash;
            }
            else
            {
                var tempPassword = GenerateTemporaryPassword();
                passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                _logger.LogWarning("No password hash found for approval request {Id}, generated temporary password", approvalRequestId);
            }

            // Create the user
            var user = new User
            {
                Email = approvalRequest.Email,
                Username = approvalRequest.Email,
                FirstName = approvalRequest.FirstName,
                LastName = approvalRequest.LastName,
                PasswordHash = passwordHash,
                Role = ParseRole(request.AssignedRole ?? "Sales"),
                IsActive = true,
                EmailVerified = false,
                DepartmentId = request.DepartmentId,
                UserProfileId = request.UserProfileId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Update approval request
            approvalRequest.Status = (int)ApprovalStatus.Approved;
            approvalRequest.ReviewedAt = DateTime.UtcNow;
            approvalRequest.ReviewedByUserId = reviewedByUserId;
            approvalRequest.AssignedUserId = user.Id;

            _context.UserApprovalRequests.Update(approvalRequest);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = ((UserRole)user.Role).ToString(),
                IsActive = user.IsActive,
                DepartmentId = user.DepartmentId,
                UserProfileId = user.UserProfileId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving user request {approvalRequestId}");
            throw;
        }
    }

    public async Task RejectUserAsync(int approvalRequestId, int reviewedByUserId, string rejectionReason)
    {
        try
        {
            var approvalRequest = await _context.UserApprovalRequests.FindAsync(approvalRequestId);
            if (approvalRequest == null)
                throw new KeyNotFoundException($"Approval request {approvalRequestId} not found");

            if (approvalRequest.Status != (int)ApprovalStatus.Pending)
                throw new InvalidOperationException("This approval request has already been reviewed");

            approvalRequest.Status = (int)ApprovalStatus.Rejected;
            approvalRequest.ReviewedAt = DateTime.UtcNow;
            approvalRequest.ReviewedByUserId = reviewedByUserId;
            approvalRequest.RejectionReason = rejectionReason;

            _context.UserApprovalRequests.Update(approvalRequest);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting user request {approvalRequestId}");
            throw;
        }
    }

    private int ParseRole(string role)
    {
        return role.ToLower() switch
        {
            "admin" => (int)UserRole.Admin,
            "manager" => (int)UserRole.Manager,
            "sales" => (int)UserRole.Sales,
            "support" => (int)UserRole.Support,
            "guest" => (int)UserRole.Guest,
            _ => (int)UserRole.Sales
        };
    }

    private string GenerateTemporaryPassword()
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        var random = new Random();
        var password = new System.Text.StringBuilder();

        // Ensure at least one uppercase, one number, and one special character
        password.Append(validChars[random.Next(26, 52)]); // Uppercase
        password.Append(validChars[random.Next(52, 62)]); // Number
        password.Append(validChars[random.Next(62)]); // Special char

        // Fill the rest randomly
        for (int i = 3; i < 12; i++)
            password.Append(validChars[random.Next(validChars.Length)]);

        // Shuffle the password
        var chars = password.ToString().ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (chars[i], chars[randomIndex]) = (chars[randomIndex], chars[i]);
        }

        return new string(chars);
    }
}
