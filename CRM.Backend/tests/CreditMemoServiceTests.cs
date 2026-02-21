// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Backend.Tests;

public class CreditMemoServiceTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new CrmDbContext(options, configuration);
    }

    private static CreditMemoService CreateService(CrmDbContext context)
    {
        return new CreditMemoService(context, Mock.Of<ILogger<CreditMemoService>>());
    }

    [Fact]
    public async Task CreateAsync_GeneratesCreditMemoNumber()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var memo = new CreditMemo { AccountId = account.Id, Amount = -50m, Status = CreditMemoStatus.Draft };
        var created = await service.CreateAsync(memo);

        Assert.False(string.IsNullOrWhiteSpace(created.CreditMemoNumber));
        Assert.Equal(account.Id, created.AccountId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCreditMemo()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        var memo = new CreditMemo { AccountId = account.Id, Amount = -50m, CreditMemoNumber = "CM-1" };
        context.CreditMemos.Add(memo);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(memo.Id);
        Assert.NotNull(result);
        Assert.Equal("CM-1", result!.CreditMemoNumber);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesCreditMemo()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        var memo = new CreditMemo { AccountId = account.Id, Amount = -50m, CreditMemoNumber = "CM-2" };
        context.CreditMemos.Add(memo);
        await context.SaveChangesAsync();

        var deleted = await service.DeleteAsync(memo.Id);
        var saved = await context.CreditMemos.FindAsync(memo.Id);

        Assert.True(deleted);
        Assert.True(saved!.IsDeleted);
    }

    [Fact]
    public async Task CreateFromInvoiceAsync_CopiesLineItems()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var invoice = new Invoice
        {
            AccountId = account.Id,
            InvoiceNumber = "INV-1",
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 100m
        };
        invoice.LineItems.Add(new InvoiceLineItem
        {
            Description = "Item",
            Quantity = 1,
            UnitPrice = 100m,
            TotalAmount = 100m
        });
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var cm = await service.CreateFromInvoiceAsync(invoice.Id);

        Assert.Equal(invoice.Id, cm.SourceInvoiceId);
        Assert.Single(cm.LineItems);
    }

    [Fact]
    public async Task ApplyAsync_UpdatesStatusAndBalances()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var invoice = new Invoice
        {
            AccountId = account.Id,
            InvoiceNumber = "INV-2",
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 100m
        };
        context.Invoices.Add(invoice);
        var memo = new CreditMemo
        {
            AccountId = account.Id,
            CreditMemoNumber = "CM-3",
            Amount = -50m,
            Status = CreditMemoStatus.Approved
        };
        context.CreditMemos.Add(memo);
        await context.SaveChangesAsync();

        var applied = await service.ApplyAsync(memo.Id, invoice.Id);

        Assert.True(applied.AmountApplied > 0);
        Assert.Contains(applied.Status, new[] { CreditMemoStatus.PartiallyApplied, CreditMemoStatus.Applied });
    }

    [Fact]
    public async Task UnapplyAsync_ResetsAmounts()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        var memo = new CreditMemo
        {
            AccountId = account.Id,
            CreditMemoNumber = "CM-4",
            Amount = -100m,
            AmountApplied = 50m,
            Status = CreditMemoStatus.PartiallyApplied
        };
        context.CreditMemos.Add(memo);
        await context.SaveChangesAsync();

        var unapplied = await service.UnapplyAsync(memo.Id);

        Assert.Equal(0m, unapplied.AmountApplied);
        Assert.Equal(CreditMemoStatus.Approved, unapplied.Status);
    }

    [Fact]
    public async Task RefundAsync_UpdatesStatus()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        var memo = new CreditMemo
        {
            AccountId = account.Id,
            CreditMemoNumber = "CM-5",
            Amount = -100m,
            Status = CreditMemoStatus.Approved
        };
        context.CreditMemos.Add(memo);
        await context.SaveChangesAsync();

        var refunded = await service.RefundAsync(memo.Id);

        Assert.Equal(CreditMemoStatus.Refunded, refunded.Status);
        Assert.NotNull(refunded.RefundedDate);
    }
}
