using CRM.Core.Dtos;
using CRM.Core.Models;

namespace CRM.Core.Interfaces;

public interface IContactsService
{
    Task<ContactDto> GetByIdAsync(int id);
    Task<List<ContactDto>> GetAllAsync();
    Task<List<ContactDto>> GetByTypeAsync(string contactType);
    Task<ContactDto> CreateAsync(CreateContactRequest request, string modifiedBy);
    Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request, string modifiedBy);
    Task<bool> DeleteAsync(int id);
    Task<SocialMediaLinkDto> AddSocialMediaLinkAsync(int contactId, AddSocialMediaRequest request);
    Task<bool> RemoveSocialMediaLinkAsync(int linkId);

    // Customer assignment methods
    Task<List<ContactDto>> GetByCustomerIdAsync(int customerId);
    Task AssignToCustomerAsync(int contactId, int customerId);
    Task UnassignFromCustomerAsync(int contactId);
}
