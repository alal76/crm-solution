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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface IEmailSequenceService
    {
        Task<IEnumerable<EmailSequence>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<EmailSequence?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<EmailSequence> CreateSequenceAsync(EmailSequence sequence, CancellationToken cancellationToken = default);

        Task<EmailSequence> UpdateAsync(EmailSequence sequence, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<EmailSequenceEnrollment> EnrollContactAsync(int sequenceId, int contactId, int? enrolledById = null, CancellationToken cancellationToken = default);

        Task<bool> StartSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<bool> StopSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<SequenceStatusDto> GetSequenceStatusAsync(int sequenceId, CancellationToken cancellationToken = default);
    }
}
