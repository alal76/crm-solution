// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
