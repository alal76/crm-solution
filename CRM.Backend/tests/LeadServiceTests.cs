// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Backend.Tests;

public class LeadServiceTests
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

    private static LeadService CreateService(CrmDbContext context, Mock<IEntityEventDispatcher> dispatcher)
    {
        return new LeadService(context, dispatcher.Object, Mock.Of<ILogger<LeadService>>());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateLeadAndSetStatus()
    {
        using var context = CreateContext();
        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var lead = new Lead
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Phone = "555-0001",
            CompanyName = "Acme"
        };

        var id = await service.CreateAsync(lead);

        var saved = await context.Leads.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(LeadLifecycleStatus.New, saved!.Status);
        dispatcher.Verify(d => d.DispatchEntityEvent("Lead", id, WorkflowTriggerType.OnCreate, null, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedLeads()
    {
        using var context = CreateContext();
        context.Leads.Add(new Lead { FirstName = "A", LastName = "B", Email = "a@b.com", Phone = "1", CompanyName = "C" });
        context.Leads.Add(new Lead { FirstName = "C", LastName = "D", Email = "c@d.com", Phone = "2", CompanyName = "E" });
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var result = await service.GetAllAsync(page: 1, pageSize: 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsLead()
    {
        using var context = CreateContext();
        var lead = new Lead { FirstName = "Test", LastName = "Lead", Email = "lead@test.com", Phone = "9", CompanyName = "Co" };
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var result = await service.GetByIdAsync(lead.Id);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesLead()
    {
        using var context = CreateContext();
        var lead = new Lead { FirstName = "Old", LastName = "Name", Email = "old@name.com", Phone = "7", CompanyName = "Co" };
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var updated = await service.UpdateAsync(lead.Id, l => l.FirstName = "New");
        var saved = await context.Leads.FindAsync(lead.Id);

        Assert.True(updated);
        Assert.Equal("New", saved!.FirstName);
        dispatcher.Verify(d => d.DispatchEntityEvent("Lead", lead.Id, WorkflowTriggerType.OnUpdate, null, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesLead()
    {
        using var context = CreateContext();
        var lead = new Lead { FirstName = "Del", LastName = "Me", Email = "del@me.com", Phone = "5", CompanyName = "Co" };
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var deleted = await service.DeleteAsync(lead.Id);
        var saved = await context.Leads.FindAsync(lead.Id);

        Assert.True(deleted);
        Assert.True(saved!.IsDeleted);
    }

    [Fact]
    public async Task ConvertAsync_CreatesOpportunityAndUpdatesLeadStatus()
    {
        using var context = CreateContext();
        var account = new Account { Company = "Acme", Email = "acc@acme.com", Phone = "123", Category = AccountCategory.Organization };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var lead = new Lead
        {
            FirstName = "Convert",
            LastName = "Me",
            Email = "convert@me.com",
            Phone = "321",
            CompanyName = "Acme",
            AccountId = account.Id
        };
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var result = await service.ConvertAsync(lead.Id, null, account.Id, 1000m, DateTime.UtcNow.AddDays(30));

        var updatedLead = await context.Leads.FindAsync(lead.Id);
        var opportunity = await context.Opportunities.FindAsync(result.OpportunityId);

        Assert.NotNull(opportunity);
        Assert.Equal(LeadLifecycleStatus.Converted, updatedLead!.Status);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsMatchingLeads()
    {
        using var context = CreateContext();
        context.Leads.Add(new Lead { FirstName = "New", LastName = "Lead", Email = "new@lead.com", Phone = "1", CompanyName = "Co", Status = LeadLifecycleStatus.New });
        context.Leads.Add(new Lead { FirstName = "Working", LastName = "Lead", Email = "work@lead.com", Phone = "2", CompanyName = "Co", Status = LeadLifecycleStatus.Working });
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var results = await service.GetByStatusAsync(LeadLifecycleStatus.Working);

        Assert.Single(results);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCounts()
    {
        using var context = CreateContext();
        context.Leads.Add(new Lead { FirstName = "New", LastName = "Lead", Email = "new@lead.com", Phone = "1", CompanyName = "Co", Status = LeadLifecycleStatus.New });
        context.Leads.Add(new Lead { FirstName = "Qualified", LastName = "Lead", Email = "qual@lead.com", Phone = "2", CompanyName = "Co", Status = LeadLifecycleStatus.Qualified });
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var stats = await service.GetStatsAsync();
        Assert.NotNull(stats);
    }

    [Fact]
    public async Task SearchAsync_FindsMatches()
    {
        using var context = CreateContext();
        context.Leads.Add(new Lead { FirstName = "Alpha", LastName = "Lead", Email = "alpha@lead.com", Phone = "1", CompanyName = "Acme" });
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var results = await service.SearchAsync("Acme");
        Assert.Single(results);
    }

    [Fact]
    public async Task AssignOwnerAsync_UpdatesOwner()
    {
        using var context = CreateContext();
        var lead = new Lead { FirstName = "Assign", LastName = "Me", Email = "assign@me.com", Phone = "3", CompanyName = "Co" };
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var dispatcher = new Mock<IEntityEventDispatcher>();
        var service = CreateService(context, dispatcher);

        var updated = await service.AssignOwnerAsync(lead.Id, 99);
        var saved = await context.Leads.FindAsync(lead.Id);

        Assert.True(updated);
        Assert.Equal(99, saved!.OwnerId);
    }
}
