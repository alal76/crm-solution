// CRM Solution - Hexagonal Architecture
// Output Ports (Driven Ports)
//
// HEXAGONAL ARCHITECTURE NOTE:
// Output ports define how the application interacts with external systems.
// These are implemented by secondary adapters (repositories, external services).
// The application/domain layer uses these ports; Infrastructure implements them.
//
// Pattern: [Service] → [Output Port] → [Repository/External Service Adapter]

using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Core.Ports.Output;

#region Repository Ports (Data Access)

/// <summary>
/// Generic output port for entity repository operations.
/// Extends the existing IRepository interface for hexagonal naming.
/// </summary>
/// <typeparam name="T">Entity type derived from BaseEntity</typeparam>
public interface IRepositoryPort<T> : IRepository<T> where T : BaseEntity { }

/// <summary>
/// Customer-specific repository port with additional query methods.
/// </summary>
public interface ICustomerRepositoryPort : IRepositoryPort<Customer>
{
    Task<IEnumerable<Customer>> GetIndividualsAsync();
    Task<IEnumerable<Customer>> GetOrganizationsAsync();
    Task<IEnumerable<Customer>> GetByLifecycleStageAsync(CustomerLifecycleStage stage);
    Task<IEnumerable<Customer>> GetByPriorityAsync(CustomerPriority priority);
    Task<IEnumerable<Customer>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
}

/// <summary>
/// Opportunity-specific repository port.
/// </summary>
public interface IOpportunityRepositoryPort : IRepositoryPort<Opportunity>
{
    Task<IEnumerable<Opportunity>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<Opportunity>> GetByStageAsync(string stage);
    Task<IEnumerable<Opportunity>> GetByOwnerAsync(int userId);
    Task<decimal> GetTotalPipelineValueAsync();
}

/// <summary>
/// Product-specific repository port.
/// </summary>
public interface IProductRepositoryPort : IRepositoryPort<Product>
{
    Task<IEnumerable<Product>> GetActiveAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<string>> GetCategoriesAsync();
}

/// <summary>
/// MarketingCampaign-specific repository port.
/// </summary>
public interface ICampaignRepositoryPort : IRepositoryPort<MarketingCampaign>
{
    Task<IEnumerable<MarketingCampaign>> GetActiveAsync();
    Task<IEnumerable<MarketingCampaign>> GetByStatusAsync(string status);
}

/// <summary>
/// User-specific repository port.
/// </summary>
public interface IUserRepositoryPort : IRepositoryPort<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetByGroupAsync(int groupId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

/// <summary>
/// UserGroup-specific repository port.
/// </summary>
public interface IUserGroupRepositoryPort : IRepositoryPort<UserGroup>
{
    Task<UserGroup?> GetByNameAsync(string name);
    Task<UserGroup?> GetWithMembersAsync(int id);
}

#endregion

#region External Service Ports

/// <summary>
/// Port for JWT token operations.
/// </summary>
public interface ITokenPort
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}

/// <summary>
/// Port for password hashing operations.
/// </summary>
public interface IPasswordPort
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Port for email sending operations.
/// </summary>
public interface IEmailPort
{
    Task<bool> SendAsync(string to, string subject, string body);
    Task<bool> SendTemplateAsync(string to, string templateName, object data);
}

/// <summary>
/// Port for external API calls.
/// </summary>
public interface IExternalApiPort
{
    Task<T?> GetAsync<T>(string url) where T : class;
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data) 
        where TRequest : class 
        where TResponse : class;
}

/// <summary>
/// Port for file storage operations.
/// </summary>
public interface IFileStoragePort
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream?> GetAsync(string path);
    Task<bool> DeleteAsync(string path);
    Task<bool> ExistsAsync(string path);
}

/// <summary>
/// Port for caching operations.
/// </summary>
public interface ICachePort
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// Port for 2FA/TOTP operations.
/// </summary>
public interface ITotpPort
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret, string issuer);
    bool ValidateCode(string secret, string code);
}

#endregion

#region Unit of Work Port

/// <summary>
/// Port for coordinating transactions across multiple repositories.
/// </summary>
public interface IUnitOfWorkPort
{
    IRepositoryPort<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

#endregion
