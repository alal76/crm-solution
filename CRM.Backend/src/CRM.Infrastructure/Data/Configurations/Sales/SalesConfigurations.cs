// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Sales;

/// <summary>
/// Entity configuration for Opportunity.
/// </summary>
public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        // Link Opportunity -> Account (required)
        builder.HasOne(e => e.Account)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Link Opportunity -> Lead (optional, source lead)
        builder.HasOne(e => e.Lead)
            .WithMany(l => l.Opportunities)
            .HasForeignKey(e => e.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Link Opportunity -> User (sales owner)
        builder.HasOne(e => e.SalesOwner)
            .WithMany()
            .HasForeignKey(e => e.SalesOwnerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for OpportunityProduct junction table.
/// </summary>
public class OpportunityProductConfiguration : IEntityTypeConfiguration<OpportunityProduct>
{
    public void Configure(EntityTypeBuilder<OpportunityProduct> builder)
    {
        builder.HasKey(op => new { op.OpportunityId, op.ProductId });

        builder.HasOne(op => op.Opportunity)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.Product)
            .WithMany()
            .HasForeignKey(op => op.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPercent).HasPrecision(5, 2);
        builder.Property(e => e.LineTotal).HasPrecision(18, 2);
    }
}

/// <summary>
/// Entity configuration for Lead.
/// </summary>
public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional Lead configurations can be added here
    }
}

/// <summary>
/// Entity configuration for LeadProductInterest junction table.
/// </summary>
public class LeadProductInterestConfiguration : IEntityTypeConfiguration<LeadProductInterest>
{
    public void Configure(EntityTypeBuilder<LeadProductInterest> builder)
    {
        builder.HasKey(lpi => new { lpi.LeadId, lpi.ProductId });

        builder.HasOne(lpi => lpi.Lead)
            .WithMany(l => l.ProductInterests)
            .HasForeignKey(lpi => lpi.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lpi => lpi.Product)
            .WithMany()
            .HasForeignKey(lpi => lpi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Entity configuration for Quote.
/// </summary>
public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional Quote configurations can be added here
    }
}

/// <summary>
/// Entity configuration for QuoteLineItem.
/// </summary>
public class QuoteLineItemConfiguration : IEntityTypeConfiguration<QuoteLineItem>
{
    public void Configure(EntityTypeBuilder<QuoteLineItem> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional QuoteLineItem configurations can be added here
    }
}
