// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

    // Account assignment methods
    Task<List<ContactDto>> GetByAccountIdAsync(int accountId);
    Task AssignToAccountAsync(int contactId, int accountId);
    Task UnassignFromAccountAsync(int contactId);
}
