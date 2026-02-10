using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface IEmailSequenceService
    {
        Task<EmailSequence> CreateSequenceAsync(EmailSequence sequence, CancellationToken cancellationToken = default);

        Task<EmailSequenceEnrollment> EnrollContactAsync(int sequenceId, int contactId, int? enrolledById = null, CancellationToken cancellationToken = default);

        Task<bool> StartSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<bool> StopSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<SequenceStatusDto> GetSequenceStatusAsync(int sequenceId, CancellationToken cancellationToken = default);
    }

    public class SequenceStatusDto
    {
        public int SequenceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public EmailSequenceStatus Status { get; set; }
        public int TotalEnrolled { get; set; }
        public int ActiveEnrollments { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalEmailsSent { get; set; }
    }
}
