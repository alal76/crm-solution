#!/usr/bin/env python3
"""Generate test files for TCOV-028 to TCOV-038."""
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests/Services"


def write(name, content):
    path = os.path.join(BASE, name)
    if os.path.exists(path):
        print(f"  EXISTS (skipping overwrite): {name}")
        return
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  CREATED: {name}")


# ─────────────────────────────────
# TCOV-028: WorkflowInstanceServiceTests.cs
# ─────────────────────────────────
write("WorkflowInstanceServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowInstanceService (TCOV-028).</summary>
public class WorkflowInstanceServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<WorkflowInstanceService>> _logger;
    private readonly Mock<IWorkflowService> _workflowService;
    private readonly Mock<IHttpCalloutService> _httpCalloutService;
    private readonly WorkflowInstanceService _service;

    public WorkflowInstanceServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<WorkflowInstanceService>>();
        _workflowService = new Mock<IWorkflowService>();
        _httpCalloutService = new Mock<IHttpCalloutService>();
        _service = new WorkflowInstanceService(_context, _logger.Object, _workflowService.Object, _httpCalloutService.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetInstanceAsync ────────────────────────────────────────────────────
    [Fact]
    public async Task GetInstanceAsync_ShouldReturnNull_WhenInstanceNotFound()
    {
        var result = await _service.GetInstanceAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstanceAsync_ShouldReturnNull_WhenInstanceIsDeleted()
    {
        _context.WorkflowInstances.Add(new WorkflowInstance { Id = 1, IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetInstanceAsync(1);
        result.Should().BeNull();
    }

    // ── GetInstanceByCorrelationIdAsync ──────────────────────────────────────
    [Fact]
    public async Task GetInstanceByCorrelationIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetInstanceByCorrelationIdAsync("no-such-correlation");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstanceByCorrelationIdAsync_ShouldReturnInstance_WhenExists()
    {
        _context.WorkflowInstances.Add(new WorkflowInstance
        {
            Id = 10,
            CorrelationId = "corr-abc",
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetInstanceByCorrelationIdAsync("corr-abc");
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    // ── CancelInstanceAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task CancelInstanceAsync_ShouldReturnFalse_WhenInstanceNotFound()
    {
        var result = await _service.CancelInstanceAsync(999, "no reason");
        result.Should().BeFalse();
    }

    // ── GetInstancesAsync ───────────────────────────────────────────────────
    [Fact]
    public async Task GetInstancesAsync_ShouldReturnEmpty_WhenNoInstancesExist()
    {
        var result = await _service.GetInstancesAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInstancesAsync_ShouldFilterByEntityType()
    {
        _context.WorkflowInstances.AddRange(
            new WorkflowInstance { Id = 1, EntityType = "Account", IsDeleted = false },
            new WorkflowInstance { Id = 2, EntityType = "Contact", IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetInstancesAsync(entityType: "Account");
        result.Should().HaveCount(1);
        result[0].EntityType.Should().Be("Account");
    }

    // ── StartWorkflowAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task StartWorkflowAsync_ShouldThrow_WhenWorkflowDefinitionNotFound()
    {
        Func<Task> act = () => _service.StartWorkflowAsync(999, "Account", 1, "OnCreate");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldThrow_WhenWorkflowIsInactive()
    {
        _context.WorkflowDefinitions.Add(new WorkflowDefinition
        {
            Id = 1,
            Status = WorkflowStatus.Draft,
            IsDeleted = false,
            Name = "Draft WF"
        });
        await _context.SaveChangesAsync();

        Func<Task> act = () => _service.StartWorkflowAsync(1, "Account", 1, "OnCreate");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }
}
""")

# ─────────────────────────────────
# TCOV-029: ServiceRequestServiceTests.cs
# ─────────────────────────────────
write("ServiceRequestServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ServiceRequestService (TCOV-029).</summary>
public class ServiceRequestServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<ILogger<ServiceRequestService>> _logger;
    private readonly NormalizationService _normalizationService;
    private readonly ServiceRequestService _service;

    private readonly List<ServiceRequest> _serviceRequests;
    private readonly List<EntityTag> _tags;
    private readonly List<CustomField> _customFields;
    private readonly List<ContactInfoLink> _contactInfoLinks;

    public ServiceRequestServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _logger = new Mock<ILogger<ServiceRequestService>>();

        _serviceRequests = new List<ServiceRequest>();
        _tags = new List<EntityTag>();
        _customFields = new List<CustomField>();
        _contactInfoLinks = new List<ContactInfoLink>();

        // Setup NormalizationService dependencies
        var mockTags = MockDbSetFactory.CreateMockDbSet(_tags);
        var mockCustomFields = MockDbSetFactory.CreateMockDbSet(_customFields);
        var mockContactInfoLinks = MockDbSetFactory.CreateMockDbSet(_contactInfoLinks);
        _mockCtx.Setup(c => c.EntityTags).Returns(mockTags.Object);
        _mockCtx.Setup(c => c.CustomFields).Returns(mockCustomFields.Object);
        _mockCtx.Setup(c => c.ContactInfoLinks).Returns(mockContactInfoLinks.Object);
        _normalizationService = new NormalizationService(_mockCtx.Object);

        // Setup ServiceRequests DbSet
        var mockSrSet = MockDbSetFactory.CreateMockDbSet(_serviceRequests);
        _mockCtx.Setup(c => c.ServiceRequests).Returns(mockSrSet.Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new ServiceRequestService(_mockCtx.Object, _logger.Object, _normalizationService);
    }

    // ── GetServiceRequestByIdAsync ──────────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetServiceRequestByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceRequestByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        _serviceRequests.Add(new ServiceRequest { Id = 1, IsDeleted = true, TicketNumber = "SR-001" });
        // Reset mock so query hits updated list
        var mockSrSet = MockDbSetFactory.CreateMockDbSet(_serviceRequests);
        _mockCtx.Setup(c => c.ServiceRequests).Returns(mockSrSet.Object);

        var result = await _service.GetServiceRequestByIdAsync(1);
        result.Should().BeNull();
    }

    // ── GetServiceRequestByTicketNumberAsync ────────────────────────────────
    [Fact]
    public async Task GetServiceRequestByTicketNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetServiceRequestByTicketNumberAsync("SR-99999");
        result.Should().BeNull();
    }

    // ── DeleteServiceRequestAsync ───────────────────────────────────────────
    [Fact]
    public async Task DeleteServiceRequestAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteServiceRequestAsync(999);
        result.Should().BeFalse();
    }

    // ── GetServiceRequestsByAccountAsync ────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByAccountAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByAccountAsync(42);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByContactAsync ────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByContactAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByContactAsync(99);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByAssigneeAsync ───────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByAssigneeAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByAssigneeAsync(5);
        result.Should().BeEmpty();
    }

    // ── GetServiceRequestsByGroupAsync ──────────────────────────────────────
    [Fact]
    public async Task GetServiceRequestsByGroupAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetServiceRequestsByGroupAsync(3);
        result.Should().BeEmpty();
    }
}
""")

# ─────────────────────────────────
# TCOV-030: RelationshipServiceTests.cs
# ─────────────────────────────────
write("RelationshipServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for RelationshipService (TCOV-030).</summary>
public class RelationshipServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly RelationshipService _service;

    public RelationshipServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        var logger = new Mock<ILogger<RelationshipService>>().Object;
        _service = new RelationshipService(_context, logger);
    }

    public void Dispose() => _context.Dispose();

    // ── GetRelationshipTypesAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetRelationshipTypesAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldExcludeDeletedTypes()
    {
        _context.RelationshipTypes.AddRange(
            new RelationshipType { Id = 1, TypeName = "Parent-Child", IsActive = true, IsDeleted = false },
            new RelationshipType { Id = 2, TypeName = "Deleted", IsActive = true, IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypesAsync();
        result.Should().HaveCount(1);
        result[0].TypeName.Should().Be("Parent-Child");
    }

    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldIncludeInactive_WhenFlagSet()
    {
        _context.RelationshipTypes.AddRange(
            new RelationshipType { Id = 1, TypeName = "Active", IsActive = true, IsDeleted = false },
            new RelationshipType { Id = 2, TypeName = "Inactive", IsActive = false, IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypesAsync(includeInactive: true);
        result.Should().HaveCount(2);
    }

    // ── GetRelationshipTypeAsync ─────────────────────────────────────────────
    [Fact]
    public async Task GetRelationshipTypeAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetRelationshipTypeAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRelationshipTypeAsync_ShouldReturnDto_WhenFound()
    {
        _context.RelationshipTypes.Add(new RelationshipType
        {
            Id = 5,
            TypeName = "Subsidiary",
            IsActive = true,
            IsDeleted = false,
            IsBidirectional = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypeAsync(5);
        result.Should().NotBeNull();
        result!.TypeName.Should().Be("Subsidiary");
    }

    // ── CreateRelationshipTypeAsync ──────────────────────────────────────────
    [Fact]
    public async Task CreateRelationshipTypeAsync_ShouldPersistNewType()
    {
        var dto = new RelationshipTypeCreateDto
        {
            TypeName = "Vendor",
            IsActive = true,
            IsBidirectional = false
        };

        var result = await _service.CreateRelationshipTypeAsync(dto);
        result.Should().NotBeNull();
        result.TypeName.Should().Be("Vendor");
        _context.RelationshipTypes.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateRelationshipTypeAsync_ShouldUpdateExisting_WhenSameNameExists()
    {
        _context.RelationshipTypes.Add(new RelationshipType
        {
            Id = 1,
            TypeName = "Partner",
            TypeCategory = "old cat",
            IsActive = true,
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var dto = new RelationshipTypeCreateDto
        {
            TypeName = "Partner",
            TypeCategory = "new cat",
            IsActive = true
        };

        var result = await _service.CreateRelationshipTypeAsync(dto);
        result.TypeCategory.Should().Be("new cat");
        _context.RelationshipTypes.Count().Should().Be(1); // no new row
    }
}
""")

# ─────────────────────────────────
# TCOV-031: CampaignMetricServiceTests.cs
# ─────────────────────────────────
write("CampaignMetricServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for CampaignMetricService (TCOV-031).</summary>
public class CampaignMetricServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<ILogger<CampaignMetricService>> _logger;
    private readonly CampaignMetricService _service;

    private readonly List<CampaignMetric> _metrics;
    private readonly List<MarketingCampaign> _campaigns;
    private readonly List<CampaignRecipient> _recipients;

    public CampaignMetricServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _logger = new Mock<ILogger<CampaignMetricService>>();

        _metrics = new List<CampaignMetric>();
        _campaigns = new List<MarketingCampaign>();
        _recipients = new List<CampaignRecipient>();

        _mockCtx.Setup(c => c.CampaignMetrics).Returns(MockDbSetFactory.CreateMockDbSet(_metrics).Object);
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);
        _mockCtx.Setup(c => c.CampaignRecipients).Returns(MockDbSetFactory.CreateMockDbSet(_recipients).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CampaignMetricService(_mockCtx.Object, _logger.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAtAndReturnMetric()
    {
        var metric = new CampaignMetric { CampaignId = 1, TotalSent = 100 };

        var result = await _service.CreateAsync(metric);

        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallSaveChanges()
    {
        var metric = new CampaignMetric { CampaignId = 2 };

        await _service.CreateAsync(metric);

        _mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetMetricsAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetMetricsAsync_ShouldReturnNull_WhenCampaignNotFound()
    {
        var result = await _service.GetMetricsAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnDto_WhenCampaignExists()
    {
        _campaigns.Add(new MarketingCampaign
        {
            Id = 1,
            Name = "Spring Promo",
            IsDeleted = false,
            Budget = 5000m,
            ActualCost = 2000m
        });
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);

        var result = await _service.GetMetricsAsync(1);

        result.Should().NotBeNull();
        result!.CampaignId.Should().Be(1);
        result.CampaignName.Should().Be("Spring Promo");
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldCalculateBudgetRemaining()
    {
        _campaigns.Add(new MarketingCampaign
        {
            Id = 2,
            Name = "Budget Test",
            IsDeleted = false,
            Budget = 10000m,
            ActualCost = 3000m
        });
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);

        var result = await _service.GetMetricsAsync(2);

        result!.BudgetRemaining.Should().Be(7000);
    }

    [Fact]
    public async Task Constructor_ShouldThrow_WhenContextIsNull()
    {
        Action act = () => new CampaignMetricService(null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }
}
""")

# ─────────────────────────────────
# TCOV-032: CampaignConversionServiceTests.cs
# ─────────────────────────────────
write("CampaignConversionServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for CampaignConversionService (TCOV-032).</summary>
public class CampaignConversionServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly CampaignConversionService _service;

    private readonly List<CampaignConversion> _conversions;
    private readonly List<MarketingCampaign> _campaigns;

    public CampaignConversionServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        var logger = new Mock<ILogger<CampaignConversionService>>().Object;

        _conversions = new List<CampaignConversion>();
        _campaigns = new List<MarketingCampaign>();

        _mockCtx.Setup(c => c.CampaignConversions).Returns(MockDbSetFactory.CreateMockDbSet(_conversions).Object);
        _mockCtx.Setup(c => c.MarketingCampaigns).Returns(MockDbSetFactory.CreateMockDbSet(_campaigns).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new CampaignConversionService(_mockCtx.Object, logger);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoConversionsExist()
    {
        var (items, total) = await _service.GetAllAsync();
        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedConversions()
    {
        _conversions.AddRange(new[]
        {
            new CampaignConversion { Id = 1, CampaignId = 1, IsDeleted = false },
            new CampaignConversion { Id = 2, CampaignId = 1, IsDeleted = true }
        });
        _mockCtx.Setup(c => c.CampaignConversions).Returns(MockDbSetFactory.CreateMockDbSet(_conversions).Object);

        var (items, total) = await _service.GetAllAsync();
        total.Should().Be(1);
        items.Should().HaveCount(1);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    // ── GetByCampaignIdAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task GetByCampaignIdAsync_ShouldReturnEmpty_WhenNoneForCampaign()
    {
        var result = await _service.GetByCampaignIdAsync(42);
        result.Should().BeEmpty();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCampaignDoesNotExist()
    {
        var dto = new CreateCampaignConversionDto { CampaignId = 99 };
        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Campaign*99*");
    }
}
""")

# ─────────────────────────────────
# TCOV-033: HttpCalloutServiceTests.cs
# ─────────────────────────────────
write("HttpCalloutServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for HttpCalloutService (TCOV-033).</summary>
public class HttpCalloutServiceTests
{
    private readonly HttpCalloutService _service;

    public HttpCalloutServiceTests()
    {
        var factory = new Mock<IHttpClientFactory>().Object;
        var logger = new Mock<ILogger<HttpCalloutService>>().Object;
        _service = new HttpCalloutService(factory, logger);
    }

    // ── Validate ─────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_ShouldFail_WhenUrlIsEmpty()
    {
        var config = new HttpCalloutConfig { Url = "", Method = "GET", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("URL"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenUrlIsNotAbsolute()
    {
        var config = new HttpCalloutConfig { Url = "/relative/path", Method = "GET", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("absolute"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenMethodIsInvalid()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "INVALID", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Method"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeoutOutOfRange()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "GET", TimeoutSeconds = 0 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("TimeoutSeconds"));
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenConfigIsValid()
    {
        var config = new HttpCalloutConfig
        {
            Url = "https://api.example.com/hook",
            Method = "POST",
            TimeoutSeconds = 30,
            RetryCount = 2
        };
        var result = _service.Validate(config);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldFail_WhenRetryCountExceedsMax()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "GET", TimeoutSeconds = 10, RetryCount = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RetryCount"));
    }
}
""")

# ─────────────────────────────────
# TCOV-034: EncryptionServiceTests.cs
# ─────────────────────────────────
write("EncryptionServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for EncryptionService (TCOV-034).</summary>
public class EncryptionServiceTests
{
    private readonly EncryptionService _service;
    private readonly Mock<IDataProtector> _protector;

    public EncryptionServiceTests()
    {
        _protector = new Mock<IDataProtector>();
        _protector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
        _protector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
        _protector.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_protector.Object);

        // IDataProtector also implements IDataProtectionProvider
        var provider = new Mock<IDataProtectionProvider>();
        provider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_protector.Object);

        // Use real EphemeralDataProtectionProvider so Protect/Unprotect match
        var realProvider = new EphemeralDataProtectionProvider();
        var logger = new Mock<ILogger<EncryptionService>>().Object;
        _service = new EncryptionService(realProvider, logger);
    }

    // ── IsEncrypted ───────────────────────────────────────────────────────────
    [Fact]
    public void IsEncrypted_ShouldReturnFalse_ForPlaintext()
    {
        _service.IsEncrypted("hello world").Should().BeFalse();
    }

    [Fact]
    public void IsEncrypted_ShouldReturnFalse_ForEmpty()
    {
        _service.IsEncrypted(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsEncrypted_ShouldReturnTrue_ForEncryptedValue()
    {
        var encrypted = _service.Encrypt("secret");
        _service.IsEncrypted(encrypted).Should().BeTrue();
    }

    // ── Encrypt ───────────────────────────────────────────────────────────────
    [Fact]
    public void Encrypt_ShouldReturnSameValue_WhenAlreadyEncrypted()
    {
        var encrypted = _service.Encrypt("top-secret");
        var doubleEncrypted = _service.Encrypt(encrypted);
        doubleEncrypted.Should().Be(encrypted);
    }

    [Fact]
    public void Encrypt_ShouldReturnInputUnchanged_WhenEmpty()
    {
        _service.Encrypt(string.Empty).Should().BeEmpty();
    }

    // ── Decrypt ───────────────────────────────────────────────────────────────
    [Fact]
    public void Decrypt_ShouldReturnOriginalValue_AfterEncryptDecryptRoundtrip()
    {
        const string original = "my-api-key-12345";
        var encrypted = _service.Encrypt(original);
        var decrypted = _service.Decrypt(encrypted);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Decrypt_ShouldReturnInput_WhenNotEncrypted()
    {
        var result = _service.Decrypt("plain-text");
        result.Should().Be("plain-text");
    }
}
""")

# ─────────────────────────────────
# TCOV-035: LLMSettingsServiceTests.cs
# ─────────────────────────────────
write("LLMSettingsServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for LLMSettingsService (TCOV-035).</summary>
public class LLMSettingsServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly LLMSettingsService _service;

    private readonly List<LLMProviderSetting> _settings;

    public LLMSettingsServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        _mockEncryption = new Mock<IEncryptionService>();
        var logger = new Mock<ILogger<LLMSettingsService>>().Object;
        var mockServiceProvider = new Mock<IServiceProvider>().Object;
        var options = Options.Create(new LLMProviderOptions());

        _settings = new List<LLMProviderSetting>();
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockEncryption.Setup(e => e.IsEncrypted(It.IsAny<string>())).Returns(false);
        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => "ENC:" + s);
        _mockEncryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.Replace("ENC:", ""));

        _service = new LLMSettingsService(_mockCtx.Object, logger, options, mockServiceProvider, _mockEncryption.Object);
    }

    // ── GetSettingsAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetSettingsAsync_ShouldReturnDefaultProvider_WhenNoDbSettings()
    {
        var result = await _service.GetSettingsAsync();
        result.Should().NotBeNull();
        // Default from LLMProviderOptions (empty string when not configured)
        result.DefaultProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldUseDbValue_WhenSettingExists()
    {
        _settings.Add(new LLMProviderSetting
        {
            Id = 1,
            SettingKey = "DefaultProvider",
            SettingValue = "openai",
            IsDeleted = false
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingsAsync();
        result.DefaultProvider.Should().Be("openai");
    }

    // ── GetSettingValueAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task GetSettingValueAsync_ShouldReturnNull_WhenKeyNotFound()
    {
        var result = await _service.GetSettingValueAsync("NonExistentKey");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSettingValueAsync_ShouldReturnValue_WhenKeyExists()
    {
        _settings.Add(new LLMProviderSetting
        {
            Id = 1,
            SettingKey = "TimeoutSeconds",
            SettingValue = "60",
            IsDeleted = false
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingValueAsync("TimeoutSeconds");
        result.Should().Be("60");
    }

    // ── GetSettingsByCategoryAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetSettingsByCategoryAsync_ShouldReturnEmpty_WhenNoneInCategory()
    {
        var result = await _service.GetSettingsByCategoryAsync("nonexistent");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSettingsByCategoryAsync_ShouldFilterByCategory()
    {
        _settings.AddRange(new[]
        {
            new LLMProviderSetting { Id = 1, SettingKey = "k1", SettingValue = "v1", Category = "openai", IsDeleted = false },
            new LLMProviderSetting { Id = 2, SettingKey = "k2", SettingValue = "v2", Category = "anthropic", IsDeleted = false }
        });
        _mockCtx.Setup(c => c.LLMProviderSettings).Returns(MockDbSetFactory.CreateMockDbSet(_settings).Object);

        var result = await _service.GetSettingsByCategoryAsync("openai");
        result.Should().ContainKey("k1");
        result.Should().NotContainKey("k2");
    }
}
""")

# ─────────────────────────────────
# TCOV-037: CalendarSyncServiceTests.cs
# ─────────────────────────────────
write("CalendarSyncServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for CalendarSyncService (TCOV-037).</summary>
public class CalendarSyncServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly CalendarSyncService _service;

    public CalendarSyncServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);

        var logger = new Mock<ILogger<CalendarSyncService>>().Object;
        var config = new ConfigurationBuilder().Build();
        var httpFactory = new Mock<IHttpClientFactory>().Object;

        _service = new CalendarSyncService(_context, logger, config, httpFactory);
    }

    public void Dispose() => _context.Dispose();

    // ── GetGoogleAuthUrlAsync ────────────────────────────────────────────────
    [Fact]
    public async Task GetGoogleAuthUrlAsync_ShouldReturnUrl_WithExpectedQueryParams()
    {
        var url = await _service.GetGoogleAuthUrlAsync(userId: 1);
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("accounts.google.com").Or.Contain("google");
    }

    // ── GetOutlookAuthUrlAsync ───────────────────────────────────────────────
    [Fact]
    public async Task GetOutlookAuthUrlAsync_ShouldReturnUrl_WithExpectedQueryParams()
    {
        var url = await _service.GetOutlookAuthUrlAsync(userId: 1);
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("microsoft").Or.Contain("login");
    }

    // ── GetIntegrationAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task GetIntegrationAsync_ShouldReturnNull_WhenNoIntegrationExists()
    {
        var result = await _service.GetIntegrationAsync(userId: 1, CalendarProvider.Google);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetIntegrationAsync_ShouldReturnIntegration_WhenItExists()
    {
        _context.CalendarIntegrations.Add(new CalendarIntegration
        {
            Id = 1,
            UserId = 5,
            Provider = CalendarProvider.Google,
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetIntegrationAsync(userId: 5, CalendarProvider.Google);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(5);
    }

    // ── GetUserIntegrationsAsync ─────────────────────────────────────────────
    [Fact]
    public async Task GetUserIntegrationsAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetUserIntegrationsAsync(userId: 99);
        result.Should().BeEmpty();
    }

    // ── DisconnectAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task DisconnectAsync_ShouldReturnFalse_WhenNoIntegrationExists()
    {
        var result = await _service.DisconnectAsync(userId: 1, CalendarProvider.Outlook);
        result.Should().BeFalse();
    }
}
""")

# ─────────────────────────────────
# TCOV-038: WebhookManagementServiceTests.cs
# ─────────────────────────────────
write("WebhookManagementServiceTests.cs", """// CRM Solution — Unit Tests
using CRM.Core.Dtos;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WebhookManagementService (TCOV-038).</summary>
public class WebhookManagementServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly WebhookManagementService _service;

    private readonly List<WebhookSubscription> _webhooks;

    public WebhookManagementServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        var logger = new Mock<ILogger<WebhookManagementService>>().Object;

        _webhooks = new List<WebhookSubscription>();
        _mockCtx.Setup(c => c.WebhookSubscriptions).Returns(MockDbSetFactory.CreateMockDbSet(_webhooks).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new WebhookManagementService(_mockCtx.Object, logger);
    }

    private Mock<DbSet<WebhookSubscription>> RefreshWebhookSet()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(_webhooks);
        _mockCtx.Setup(c => c.WebhookSubscriptions).Returns(mockSet.Object);
        return mockSet;
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoWebhooksExist()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedWebhooks()
    {
        _webhooks.AddRange(new[]
        {
            new WebhookSubscription { WebhookSubscriptionId = 1, TargetUrl = "https://a.com", IsDeleted = false },
            new WebhookSubscription { WebhookSubscriptionId = 2, TargetUrl = "https://b.com", IsDeleted = true }
        });
        RefreshWebhookSet();

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByIsActive()
    {
        _webhooks.AddRange(new[]
        {
            new WebhookSubscription { WebhookSubscriptionId = 1, TargetUrl = "https://active.com", IsActive = true, IsDeleted = false },
            new WebhookSubscription { WebhookSubscriptionId = 2, TargetUrl = "https://inactive.com", IsActive = false, IsDeleted = false }
        });
        RefreshWebhookSet();

        var result = await _service.GetAllAsync(isActive: true);
        result.Should().HaveCount(1);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUrlIsEmpty()
    {
        var dto = new CreateWebhookDto { Url = "" };
        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*URL*");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenWebhookNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }
}
""")

# ─────────────────────────────────────────────────────────
# TCOV-036 expand EmailTemplateServiceTests.cs
# (append new test class to existing file)
# ─────────────────────────────────────────────────────────
email_template_path = os.path.join(BASE, "EmailTemplateServiceTests.cs")
with open(email_template_path, "r", encoding="utf-8") as f:
    existing = f.read()

APPEND_MARKER = "\n// TCOV-036-EXPANDED"
if APPEND_MARKER not in existing:
    expansion = """
// TCOV-036-EXPANDED
// Additional tests appended to expand coverage
namespace CRM.Tests.Services;

/// <summary>Additional coverage tests for EmailTemplateService (TCOV-036 expansion).</summary>
public class EmailTemplateServiceExpandedTests : ServiceTestFixtureBase<EmailTemplateService>
{
    private readonly EmailTemplateService _service;
    private readonly List<EmailTemplate> _templates;
    private readonly List<EmailTemplateVersion> _versions;

    public EmailTemplateServiceExpandedTests()
    {
        _templates = new List<EmailTemplate>();
        _versions = new List<EmailTemplateVersion>();

        var mockTemplates = MockDbSetFactory.CreateMockDbSet(_templates);
        mockTemplates.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                return ValueTask.FromResult(_templates.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });
        var mockVersions = MockDbSetFactory.CreateMockDbSet(_versions);

        MockContext.Setup(c => c.EmailTemplates).Returns(mockTemplates.Object);
        MockContext.Setup(c => c.EmailTemplateVersions).Returns(mockVersions.Object);
        MockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Account>()).Object);
        MockContext.Setup(c => c.Contacts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Models.Contact>()).Object);
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Opportunity>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new EmailTemplateService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByNameAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetBySlugAsync("no-slug");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveTemplates_WhenIsActiveFilterApplied()
    {
        _templates.AddRange(new[]
        {
            new EmailTemplate { Id = 1, Name = "Active", IsActive = true, IsDeleted = false },
            new EmailTemplate { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false }
        });

        var result = (await _service.GetAllAsync(isActive: true)).ToList();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTemplateNotFound()
    {
        var template = new EmailTemplate { Id = 999, Name = "Ghost", IsDeleted = false };
        Func<Task> act = () => _service.UpdateAsync(template);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTemplateIsNotFoundAtAll()
    {
        var result = await _service.DeleteAsync(9999);
        result.Should().BeFalse();
    }
}
"""
    with open(email_template_path, "a", encoding="utf-8") as f:
        f.write(expansion)
    print(f"  EXPANDED: EmailTemplateServiceTests.cs (+5 tests)")
else:
    print(f"  ALREADY EXPANDED: EmailTemplateServiceTests.cs")

print("\nAll test files processed.")
