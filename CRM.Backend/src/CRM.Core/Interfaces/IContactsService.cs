// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
