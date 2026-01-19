using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for user approval workflow
/// </summary>
public interface IUserApprovalService
{
    Task<IEnumerable<UserApprovalRequestDto>> GetApprovalRequestsAsync(int? status = null);
    Task<UserApprovalRequestDto?> GetApprovalRequestByIdAsync(int id);
    Task CreateApprovalRequestAsync(string email, string firstName, string lastName, string? company = null, string? phone = null);
    Task<UserDto> ApproveUserAsync(int approvalRequestId, int reviewedByUserId, ApproveUserRequest request);
    Task RejectUserAsync(int approvalRequestId, int reviewedByUserId, string rejectionReason);
}
