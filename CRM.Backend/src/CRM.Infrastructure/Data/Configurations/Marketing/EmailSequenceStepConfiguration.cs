using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Core.Entities;

namespace CRM.Infrastructure.Data.Configurations.Marketing
{
    public class EmailSequenceStepConfiguration : IEntityTypeConfiguration<EmailSequenceStep>
    {
        public void Configure(EntityTypeBuilder<EmailSequenceStep> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EmailSequenceId)
                .IsRequired();

            builder.HasOne(e => e.EmailSequence)
                .WithMany(s => s.Steps)
                .HasForeignKey(e => e.EmailSequenceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Order).IsRequired();
            builder.Property(e => e.Template).IsRequired().HasMaxLength(2000);
        }
    }
}
