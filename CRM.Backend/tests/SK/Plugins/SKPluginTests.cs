// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.SK.Plugins;

/// <summary>
/// Tests for all SK CRM Plugins (signatures verified from source code).
/// Tests: PluginName, Description, success JSON paths, error JSON paths, exception safety.
/// </summary>
public sealed class SKPluginTests
{
    // ── JSON helpers ──────────────────────────────────────────────────────
    private static bool IsSuccessJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            // CrmPluginBase.SuccessResult() returns {"error":false,"data":...}
            if (doc.RootElement.TryGetProperty("error", out var e) && !e.GetBoolean()) return true;
            // Fallback: legacy {"success":true}
            if (doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean()) return true;
            return false;
        }
        catch { return false; }
    }

    private static bool IsErrorJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            // CrmPluginBase.ErrorResult() returns {"error":true,...}
            if (doc.RootElement.TryGetProperty("error", out var e) && e.GetBoolean()) return true;
            // Fallback: legacy {"success":false}
            if (doc.RootElement.TryGetProperty("success", out var s) && !s.GetBoolean()) return true;
            return false;
        }
        catch { return false; }
    }

    // ═════════════════════════════════════════════════════════════════════
    // 1. LeadPlugin
    //    ctor: LeadPlugin(ILeadService, ILogger<LeadPlugin>)
    //    KernelFunctions: GetLeadAsync(int), SearchLeadsAsync(string, int=10),
    //                     GetLeadStatsAsync(), UpdateLeadScoreAsync(int, int)
    //    ILeadService: GetByIdAsync(int)→LeadDto?,
    //                  SearchAsync(string)→IEnumerable<LeadSummaryDto>,
    //                  GetStatsAsync()→object
    // ═════════════════════════════════════════════════════════════════════
    private static (LeadPlugin plugin, Mock<ILeadService> svc) MakeLeadPlugin()
    {
        var svc = new Mock<ILeadService>();
        return (new LeadPlugin(svc.Object, new Mock<ILogger<LeadPlugin>>().Object), svc);
    }

    [Fact] public void LeadPlugin_PluginName_IsNotNullOrEmpty()
        => MakeLeadPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void LeadPlugin_Description_IsNotNullOrEmpty()
        => MakeLeadPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task LeadPlugin_GetLeadAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new LeadDto { Id = 1, FirstName = "Alice" });
        IsSuccessJson(await plugin.GetLeadAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_GetLeadAsync_ReturnsErrorJson_WhenNotFound()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((LeadDto?)null);
        IsErrorJson(await plugin.GetLeadAsync(999)).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_GetLeadAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB down"));
        IsErrorJson(await plugin.GetLeadAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_SearchLeadsAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.SearchAsync("alice"))
            .ReturnsAsync(new List<LeadSummaryDto> { new() { Id = 1 } });
        IsSuccessJson(await plugin.SearchLeadsAsync("alice")).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_SearchLeadsAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.SearchAsync(It.IsAny<string>())).ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.SearchLeadsAsync("q")).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_GetLeadStatsAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeLeadPlugin();
        svc.Setup(s => s.GetStatsAsync()).ReturnsAsync(new { TotalLeads = 50 });
        IsSuccessJson(await plugin.GetLeadStatsAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task LeadPlugin_UpdateLeadScoreAsync_ReturnsErrorJson_WhenScoreOver100()
    {
        var (plugin, _) = MakeLeadPlugin();
        IsErrorJson(await plugin.UpdateLeadScoreAsync(1, 150)).Should().BeTrue(because: "score 150 > 100");
    }

    [Fact]
    public async Task LeadPlugin_UpdateLeadScoreAsync_ReturnsErrorJson_WhenScoreNegative()
    {
        var (plugin, _) = MakeLeadPlugin();
        IsErrorJson(await plugin.UpdateLeadScoreAsync(1, -1)).Should().BeTrue(because: "score -1 < 0");
    }

    // ═════════════════════════════════════════════════════════════════════
    // 2. AccountPlugin
    //    ctor: AccountPlugin(IAccountService, ICrmDbContext, ILogger<AccountPlugin>)
    //    KernelFunctions: GetAccountAsync(int), SearchAccountsAsync(string)
    //    IAccountService: GetAccountByIdAsync(int)→AccountDto?,
    //                     SearchAccountsAsync(string)→IEnumerable<AccountDto>
    // ═════════════════════════════════════════════════════════════════════
    private static (AccountPlugin plugin, Mock<IAccountService> svc) MakeAccountPlugin()
    {
        var svc = new Mock<IAccountService>();
        var db = new Mock<ICrmDbContext>();
        return (new AccountPlugin(svc.Object, db.Object, new Mock<ILogger<AccountPlugin>>().Object), svc);
    }

    [Fact] public void AccountPlugin_PluginName_IsNotNullOrEmpty()
        => MakeAccountPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void AccountPlugin_Description_IsNotNullOrEmpty()
        => MakeAccountPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task AccountPlugin_GetAccountAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeAccountPlugin();
        svc.Setup(s => s.GetAccountByIdAsync(1)).ReturnsAsync(new AccountDto { Id = 1, Company = "Acme Corp" });
        IsSuccessJson(await plugin.GetAccountAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task AccountPlugin_GetAccountAsync_ReturnsErrorJson_WhenNotFound()
    {
        var (plugin, svc) = MakeAccountPlugin();
        svc.Setup(s => s.GetAccountByIdAsync(999)).ReturnsAsync((AccountDto?)null);
        IsErrorJson(await plugin.GetAccountAsync(999)).Should().BeTrue();
    }

    [Fact]
    public async Task AccountPlugin_GetAccountAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeAccountPlugin();
        svc.Setup(s => s.GetAccountByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB error"));
        IsErrorJson(await plugin.GetAccountAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task AccountPlugin_SearchAccountsAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeAccountPlugin();
        svc.Setup(s => s.SearchAccountsAsync("acme"))
            .ReturnsAsync(new List<AccountDto> { new() { Id = 1, Company = "Acme" } });
        IsSuccessJson(await plugin.SearchAccountsAsync("acme")).Should().BeTrue();
    }

    [Fact]
    public async Task AccountPlugin_SearchAccountsAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeAccountPlugin();
        svc.Setup(s => s.SearchAccountsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.SearchAccountsAsync("q")).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 3. ContactPlugin
    //    ctor: ContactPlugin(IContactsService, ICrmDbContext, ILogger<ContactPlugin>)
    //    KernelFunctions: GetContactAsync(int), SearchContactsAsync(string, int=10)
    //    IContactsService: GetByIdAsync(int)→ContactDto (non-nullable),
    //                      GetAllAsync()→List<ContactDto>
    // ═════════════════════════════════════════════════════════════════════
    private static (ContactPlugin plugin, Mock<IContactsService> svc) MakeContactPlugin()
    {
        var svc = new Mock<IContactsService>();
        var db = new Mock<ICrmDbContext>();
        return (new ContactPlugin(svc.Object, db.Object, new Mock<ILogger<ContactPlugin>>().Object), svc);
    }

    [Fact] public void ContactPlugin_PluginName_IsNotNullOrEmpty()
        => MakeContactPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void ContactPlugin_Description_IsNotNullOrEmpty()
        => MakeContactPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task ContactPlugin_GetContactAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeContactPlugin();
        svc.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ContactDto { Id = 1, FirstName = "Bob" });
        IsSuccessJson(await plugin.GetContactAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task ContactPlugin_GetContactAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeContactPlugin();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.GetContactAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task ContactPlugin_SearchContactsAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeContactPlugin();
        svc.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ContactDto>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", EmailPrimary = "alice@example.com" }
        });
        IsSuccessJson(await plugin.SearchContactsAsync("alice")).Should().BeTrue();
    }

    [Fact]
    public async Task ContactPlugin_SearchContactsAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeContactPlugin();
        svc.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("DB error"));
        IsErrorJson(await plugin.SearchContactsAsync("alice")).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 4. QuotePlugin
    //    ctor: QuotePlugin(IQuoteService, ILogger<QuotePlugin>)
    //    KernelFunctions: GetQuoteAsync(int)
    //    IQuoteService: GetByIdAsync(int)→Quote?
    // ═════════════════════════════════════════════════════════════════════
    private static (QuotePlugin plugin, Mock<IQuoteService> svc) MakeQuotePlugin()
    {
        var svc = new Mock<IQuoteService>();
        return (new QuotePlugin(svc.Object, new Mock<ILogger<QuotePlugin>>().Object), svc);
    }

    [Fact] public void QuotePlugin_PluginName_IsNotNullOrEmpty()
        => MakeQuotePlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void QuotePlugin_Description_IsNotNullOrEmpty()
        => MakeQuotePlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task QuotePlugin_GetQuoteAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeQuotePlugin();
        svc.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new Quote { Id = 1, OpportunityId = 10 });
        IsSuccessJson(await plugin.GetQuoteAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task QuotePlugin_GetQuoteAsync_ReturnsSuccessJson_WhenNotFound()
    {
        var (plugin, svc) = MakeQuotePlugin();
        svc.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Quote?)null);
        // Plugin returns SuccessResult({found:false}) for not-found quotes
        var result = await plugin.GetQuoteAsync(999);
        IsSuccessJson(result).Should().BeTrue($"got: {result}");
        result.Should().Contain("found");
    }

    [Fact]
    public async Task QuotePlugin_GetQuoteAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeQuotePlugin();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.GetQuoteAsync(1)).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 5. SearchPlugin
    //    ctor: SearchPlugin(ISearchPort, ILogger<SearchPlugin>)
    //    KernelFunctions: GlobalSearchAsync(string, int=20), SearchByTypeAsync(string, string, int=20)
    //    ISearchPort: SearchAsync(SearchRequest, CancellationToken=default)→SearchResult
    // ═════════════════════════════════════════════════════════════════════
    private static (SearchPlugin plugin, Mock<ISearchPort> port) MakeSearchPlugin()
    {
        var port = new Mock<ISearchPort>();
        return (new SearchPlugin(port.Object, new Mock<ILogger<SearchPlugin>>().Object), port);
    }

    [Fact] public void SearchPlugin_PluginName_IsNotNullOrEmpty()
        => MakeSearchPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void SearchPlugin_Description_IsNotNullOrEmpty()
        => MakeSearchPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task SearchPlugin_GlobalSearchAsync_ReturnsSuccessJson_WithHits()
    {
        var (plugin, port) = MakeSearchPlugin();
        port.Setup(p => p.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult
            {
                Query = "acme", TotalCount = 1, ProcessingTimeMs = 5,
                Hits = new List<SearchHit> { new() { EntityType = "Account", Id = "1", Title = "Acme Corp" } }
            });
        IsSuccessJson(await plugin.GlobalSearchAsync("acme")).Should().BeTrue();
    }

    [Fact]
    public async Task SearchPlugin_GlobalSearchAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, port) = MakeSearchPlugin();
        port.Setup(p => p.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Search unavailable"));
        IsErrorJson(await plugin.GlobalSearchAsync("query")).Should().BeTrue();
    }

    [Fact]
    public async Task SearchPlugin_SearchByTypeAsync_ReturnsSuccessJson()
    {
        var (plugin, port) = MakeSearchPlugin();
        port.Setup(p => p.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResult { Query = "test", TotalCount = 0, Hits = new List<SearchHit>() });
        IsSuccessJson(await plugin.SearchByTypeAsync("test", "Account")).Should().BeTrue();
    }

    [Fact]
    public async Task SearchPlugin_SearchByTypeAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, port) = MakeSearchPlugin();
        port.Setup(p => p.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.SearchByTypeAsync("x", "Lead")).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 6. ServiceRequestPlugin
    //    ctor: ServiceRequestPlugin(IServiceRequestService, ICrmDbContext, ILogger<>)
    //    KernelFunctions: GetTicketAsync(int ticketId)
    //    IServiceRequestService: GetServiceRequestByIdAsync(int)→ServiceRequestDto?
    // ═════════════════════════════════════════════════════════════════════
    private static (ServiceRequestPlugin plugin, Mock<IServiceRequestService> svc) MakeSRPlugin()
    {
        var svc = new Mock<IServiceRequestService>();
        var db = new Mock<ICrmDbContext>();
        return (new ServiceRequestPlugin(svc.Object, db.Object, new Mock<ILogger<ServiceRequestPlugin>>().Object), svc);
    }

    [Fact] public void ServiceRequestPlugin_PluginName_IsNotNullOrEmpty()
        => MakeSRPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void ServiceRequestPlugin_Description_IsNotNullOrEmpty()
        => MakeSRPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task ServiceRequestPlugin_GetTicketAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeSRPlugin();
        svc.Setup(s => s.GetServiceRequestByIdAsync(1))
            .ReturnsAsync(new ServiceRequestDto { Id = 1, TicketNumber = "SR-001" });
        IsSuccessJson(await plugin.GetTicketAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task ServiceRequestPlugin_GetTicketAsync_ReturnsErrorJson_WhenNotFound()
    {
        var (plugin, svc) = MakeSRPlugin();
        svc.Setup(s => s.GetServiceRequestByIdAsync(999)).ReturnsAsync((ServiceRequestDto?)null);
        IsErrorJson(await plugin.GetTicketAsync(999)).Should().BeTrue();
    }

    [Fact]
    public async Task ServiceRequestPlugin_GetTicketAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeSRPlugin();
        svc.Setup(s => s.GetServiceRequestByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB error"));
        IsErrorJson(await plugin.GetTicketAsync(1)).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 7. OpportunityPlugin
    //    ctor: OpportunityPlugin(IOpportunityService, ICrmDbContext, ILogger<>)
    //    KernelFunctions: GetOpportunityAsync(int)
    //    IOpportunityService: GetOpportunityByIdAsync(int)→Opportunity?
    // ═════════════════════════════════════════════════════════════════════
    private static (OpportunityPlugin plugin, Mock<IOpportunityService> svc) MakeOppPlugin()
    {
        var svc = new Mock<IOpportunityService>();
        var db = new Mock<ICrmDbContext>();
        return (new OpportunityPlugin(svc.Object, db.Object, new Mock<ILogger<OpportunityPlugin>>().Object), svc);
    }

    [Fact] public void OpportunityPlugin_PluginName_IsNotNullOrEmpty()
        => MakeOppPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void OpportunityPlugin_Description_IsNotNullOrEmpty()
        => MakeOppPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task OpportunityPlugin_GetOpportunityAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeOppPlugin();
        svc.Setup(s => s.GetOpportunityByIdAsync(1))
            .ReturnsAsync(new Opportunity { Id = 1, Name = "Big Deal" });
        IsSuccessJson(await plugin.GetOpportunityAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task OpportunityPlugin_GetOpportunityAsync_ReturnsErrorJson_WhenNotFound()
    {
        var (plugin, svc) = MakeOppPlugin();
        svc.Setup(s => s.GetOpportunityByIdAsync(999)).ReturnsAsync((Opportunity?)null);
        IsErrorJson(await plugin.GetOpportunityAsync(999)).Should().BeTrue();
    }

    [Fact]
    public async Task OpportunityPlugin_GetOpportunityAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeOppPlugin();
        svc.Setup(s => s.GetOpportunityByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.GetOpportunityAsync(1)).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 8. CalendarPlugin
    //    ctor: CalendarPlugin(IActivityService, ILogger<CalendarPlugin>)
    //    KernelFunctions: GetActivitiesAsync(int? accountId=null, int? userId=null,
    //                         string? activityType=null, int daysBack=30, int limit=25)
    //                     GetUpcomingAsync(int limit=20)
    //    IActivityService: GetActivitiesAsync(int?,int?,int?,int?,DateTime?,DateTime?,int)
    //                      GetRecentAsync(int)
    // ═════════════════════════════════════════════════════════════════════
    private static (CalendarPlugin plugin, Mock<IActivityService> svc) MakeCalendarPlugin()
    {
        var svc = new Mock<IActivityService>();
        return (new CalendarPlugin(svc.Object, new Mock<ILogger<CalendarPlugin>>().Object), svc);
    }

    [Fact] public void CalendarPlugin_PluginName_IsNotNullOrEmpty()
        => MakeCalendarPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void CalendarPlugin_Description_IsNotNullOrEmpty()
        => MakeCalendarPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task CalendarPlugin_GetActivitiesAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeCalendarPlugin();
        svc.Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityDto> { new() { Id = 1, Title = "Call" } });
        IsSuccessJson(await plugin.GetActivitiesAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task CalendarPlugin_GetActivitiesAsync_WithAccountFilter_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeCalendarPlugin();
        svc.Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityDto>());
        IsSuccessJson(await plugin.GetActivitiesAsync(accountId: 5)).Should().BeTrue();
    }

    [Fact]
    public async Task CalendarPlugin_GetActivitiesAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeCalendarPlugin();
        svc.Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB error"));
        IsErrorJson(await plugin.GetActivitiesAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task CalendarPlugin_GetUpcomingAsync_ReturnsSuccessJson()
    {
        var (plugin, svc) = MakeCalendarPlugin();
        svc.Setup(s => s.GetRecentAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityDto> { new() { Id = 2, Title = "Follow-up" } });
        IsSuccessJson(await plugin.GetUpcomingAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task CalendarPlugin_GetUpcomingAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeCalendarPlugin();
        svc.Setup(s => s.GetRecentAsync(It.IsAny<int>())).ThrowsAsync(new Exception("error"));
        IsErrorJson(await plugin.GetUpcomingAsync()).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 9. ContractPlugin
    //    ctor: ContractPlugin(IContractService, ILogger<ContractPlugin>)
    //    KernelFunctions: GetContractAsync(int contractId),
    //                     SearchContractsAsync(string?,int?,string?)
    //    IContractService: GetByIdAsync(int, CancellationToken=default)→Contract?
    //    NOTE: not-found returns SuccessResult({found:false}), NOT ErrorResult
    // ═════════════════════════════════════════════════════════════════════
    private static (ContractPlugin plugin, Mock<IContractService> svc) MakeContractPlugin()
    {
        var svc = new Mock<IContractService>();
        return (new ContractPlugin(svc.Object, new Mock<ILogger<ContractPlugin>>().Object), svc);
    }

    [Fact] public void ContractPlugin_PluginName_IsNotNullOrEmpty()
        => MakeContractPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void ContractPlugin_Description_IsNotNullOrEmpty()
        => MakeContractPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task ContractPlugin_GetContractAsync_ReturnsSuccessJson_WhenFound()
    {
        var (plugin, svc) = MakeContractPlugin();
        svc.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contract { Id = 1, ContractNumber = "C-001", Name = "Support" });
        IsSuccessJson(await plugin.GetContractAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task ContractPlugin_GetContractAsync_ReturnsSuccessJson_WhenNotFound_WithFoundFalse()
    {
        // Source code returns SuccessResult({found:false}) when contract is null, not ErrorResult
        var (plugin, svc) = MakeContractPlugin();
        svc.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Contract?)null);
        var result = await plugin.GetContractAsync(999);
        IsSuccessJson(result).Should().BeTrue(because: "ContractPlugin returns success+found:false for missing records");
        result.Should().Contain("found");
    }

    [Fact]
    public async Task ContractPlugin_GetContractAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, svc) = MakeContractPlugin();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        IsErrorJson(await plugin.GetContractAsync(1)).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 10. EmailPlugin
    //     ctor: EmailPlugin(IEmailTemplateService, INotificationPort, ILogger<EmailPlugin>)
    //     KernelFunctions: GetEmailTemplatesAsync(bool activeOnly=true),
    //                      SearchTemplatesAsync(string keyword)
    //     IEmailTemplateService: GetAllAsync(EmailTemplateCategory? category,
    //                                        bool? isActive, CancellationToken ct)→IEnumerable<EmailTemplate>
    //     GetEmailTemplatesAsync calls GetAllAsync(null, activeOnly ? true : null)
    //     SearchTemplatesAsync calls GetAllAsync(null, null) then filters in memory
    // ═════════════════════════════════════════════════════════════════════
    private static (EmailPlugin plugin, Mock<IEmailTemplateService> emailSvc) MakeEmailPlugin()
    {
        var emailSvc = new Mock<IEmailTemplateService>();
        var notifPort = new Mock<INotificationPort>();
        return (new EmailPlugin(emailSvc.Object, notifPort.Object, new Mock<ILogger<EmailPlugin>>().Object), emailSvc);
    }

    [Fact] public void EmailPlugin_PluginName_IsNotNullOrEmpty()
        => MakeEmailPlugin().plugin.PluginName.Should().NotBeNullOrEmpty();

    [Fact] public void EmailPlugin_Description_IsNotNullOrEmpty()
        => MakeEmailPlugin().plugin.Description.Should().NotBeNullOrEmpty();

    [Fact]
    public async Task EmailPlugin_GetEmailTemplatesAsync_WithDefaultActiveOnly_ReturnsSuccessJson()
    {
        var (plugin, emailSvc) = MakeEmailPlugin();
        // activeOnly=true → GetAllAsync(null, true, ct)
        emailSvc.Setup(s => s.GetAllAsync(
                It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailTemplate>
            {
                new() { Id = 1, Name = "Welcome", Subject = "Welcome!", IsActive = true }
            });
        IsSuccessJson(await plugin.GetEmailTemplatesAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task EmailPlugin_GetEmailTemplatesAsync_ActiveOnly_False_ReturnsSuccessJson()
    {
        var (plugin, emailSvc) = MakeEmailPlugin();
        // activeOnly=false → GetAllAsync(null, null, ct)
        emailSvc.Setup(s => s.GetAllAsync(
                It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailTemplate>
            {
                new() { Id = 2, Name = "Archived", Subject = "Old", IsActive = false }
            });
        IsSuccessJson(await plugin.GetEmailTemplatesAsync(activeOnly: false)).Should().BeTrue();
    }

    [Fact]
    public async Task EmailPlugin_GetEmailTemplatesAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, emailSvc) = MakeEmailPlugin();
        emailSvc.Setup(s => s.GetAllAsync(
                It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));
        IsErrorJson(await plugin.GetEmailTemplatesAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task EmailPlugin_SearchTemplatesAsync_ReturnsSuccessJson_WithMatches()
    {
        var (plugin, emailSvc) = MakeEmailPlugin();
        // SearchTemplatesAsync calls GetAllAsync(null, null) then filters in memory
        emailSvc.Setup(s => s.GetAllAsync(
                It.IsAny<EmailTemplateCategory?>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailTemplate>
            {
                new() { Id = 1, Name = "Welcome Email", Subject = "Welcome to CRM", IsActive = true },
                new() { Id = 2, Name = "Follow-up", Subject = "Following up", IsActive = true }
            });
        IsSuccessJson(await plugin.SearchTemplatesAsync("Welcome")).Should().BeTrue();
    }

    [Fact]
    public async Task EmailPlugin_SearchTemplatesAsync_ReturnsErrorJson_OnException()
    {
        var (plugin, emailSvc) = MakeEmailPlugin();
        emailSvc.Setup(s => s.GetAllAsync(
                It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("timeout"));
        IsErrorJson(await plugin.SearchTemplatesAsync("q")).Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════════
    // 11. Cross-plugin invariants
    // ═════════════════════════════════════════════════════════════════════
    [Fact]
    public void AllPlugins_PluginNames_AreDistinct()
    {
        var names = new[]
        {
            MakeLeadPlugin().plugin.PluginName,
            MakeAccountPlugin().plugin.PluginName,
            MakeContactPlugin().plugin.PluginName,
            MakeQuotePlugin().plugin.PluginName,
            MakeSearchPlugin().plugin.PluginName,
            MakeSRPlugin().plugin.PluginName,
            MakeOppPlugin().plugin.PluginName,
            MakeCalendarPlugin().plugin.PluginName,
            MakeContractPlugin().plugin.PluginName,
            MakeEmailPlugin().plugin.PluginName,
        };
        names.Should().OnlyHaveUniqueItems();
        foreach (var n in names)
            n.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AllPlugins_Descriptions_AreNonEmpty()
    {
        var descriptions = new[]
        {
            MakeLeadPlugin().plugin.Description,
            MakeAccountPlugin().plugin.Description,
            MakeContactPlugin().plugin.Description,
            MakeQuotePlugin().plugin.Description,
            MakeSearchPlugin().plugin.Description,
            MakeSRPlugin().plugin.Description,
            MakeOppPlugin().plugin.Description,
            MakeCalendarPlugin().plugin.Description,
            MakeContractPlugin().plugin.Description,
            MakeEmailPlugin().plugin.Description,
        };
        foreach (var d in descriptions)
            d.Should().NotBeNullOrEmpty();
    }
}
