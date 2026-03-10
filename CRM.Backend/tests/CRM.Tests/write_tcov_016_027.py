"""
Script to create/expand test files for TCOV-016 to TCOV-027.
Run from /Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests/
"""
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"

files = {}

# ============================================================
# TCOV-016: SampleDataSeederServiceTests.cs – add 3 more tests
# ============================================================
SAMPLE_SEEDER_APPEND = """
    [Fact]
    public async Task IsSampleDataSeededAsync_ShouldReturnFalse_WhenDatabaseIsEmpty()
    {
        var result = await _service.IsSampleDataSeededAsync();
        // Fresh InMemory DB has no SystemSettings row → should return false
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldPopulateStepsList()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.Steps.Should().NotBeEmpty();
        // Every step should have a non-empty name
        result.Steps.All(s => !string.IsNullOrEmpty(s.Step)).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReturnPositiveDuration()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.TotalDurationMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSampleDataStatsAsync_ShouldReturnStats()
    {
        // Seed first so there's at least the SystemSettings row
        await _service.SeedAllSampleDataWithLogAsync();
        var act = async () => await _service.GetSampleDataStatsAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearSampleDataAsync_ShouldNotThrow()
    {
        var act = async () => await _service.ClearSampleDataAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedSampleUsersAsync_ShouldNotThrow()
    {
        var act = async () => await _service.SeedSampleUsersAsync();
        await act.Should().NotThrowAsync();
    }
}
"""

# ============================================================
# TCOV-017: ReportServiceTests.cs – add 8 more tests
# ============================================================
REPORT_SERVICE_APPEND = """
    // ── Additional tests added for TCOV-017 ──────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedReports()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 10, Name = "R1", IsDeleted = false, DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 11, Name = "R2", IsDeleted = true,  DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupDbSets(reportDefinitions: reports);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("R1");
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnFilteredReports()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 20, Name = "Sales", Category = "Sales", IsDeleted = false, DataSource = ReportDataSource.Opportunities, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 21, Name = "HR",    Category = "HR",    IsDeleted = false, DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupDbSets(reportDefinitions: reports);

        var result = await _service.GetByCategoryAsync("Sales");

        result.Should().HaveCount(1);
        result.First().Category.Should().Be("Sales");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReport_WhenExists()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 30, Name = "Single", IsDeleted = false, DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupDbSets(reportDefinitions: reports);

        var result = await _service.GetByIdAsync(30);

        result.Should().NotBeNull();
        result!.Id.Should().Be(30);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        SetupDbSets(reportDefinitions: new List<ReportDefinitionEntity>());

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenReportNotFound()
    {
        SetupDbSets(reportDefinitions: new List<ReportDefinitionEntity>());

        var result = await _service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetFoldersAsync_ShouldReturnEmpty_WhenNoFolders()
    {
        var mockFolders = MockDbSetFactory.CreateMockDbSet(new List<ReportFolder>());
        MockContext.Setup(c => c.ReportFolders).Returns(mockFolders.Object);
        SetupDbSets();

        var result = await _service.GetFoldersAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToFavoritesAsync_ShouldReturnTrue()
    {
        var result = await _service.AddToFavoritesAsync(100, 42);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ShouldReturnTrue()
    {
        // Add first so there's something to remove
        await _service.AddToFavoritesAsync(100, 42);

        var result = await _service.RemoveFromFavoritesAsync(100, 42);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetStandardReportsAsync_ShouldReturnOnlyStandardReports()
    {
        var reports = new List<ReportDefinitionEntity>
        {
            new() { Id = 50, Name = "S1", IsStandard = true,  IsDeleted = false, DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 51, Name = "C1", IsStandard = false, IsDeleted = false, DataSource = ReportDataSource.Accounts, Status = ReportStatus.Active, ColumnsJson = "[]", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        SetupDbSets(reportDefinitions: reports);

        var result = await _service.GetStandardReportsAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("S1");
    }
}
"""

# ============================================================
# TCOV-018: ContactInfoServiceTests.cs – new file (8 tests)
# ============================================================
files["Services/ContactInfoServiceTests.cs"] = """\
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ContactInfoService (TCOV-018).</summary>
public class ContactInfoServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly ContactInfoService _service;

    public ContactInfoServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ContactInfoTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _service = new ContactInfoService(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Address tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAddressAsync_ShouldPersistAddress()
    {
        var dto = new CreateAddressDto
        {
            Line1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };

        var result = await _service.CreateAddressAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.City.Should().Be("Springfield");
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnNull_WhenAddressDoesNotExist()
    {
        var result = await _service.GetAddressByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnAddress_WhenExists()
    {
        var dto = new CreateAddressDto { Line1 = "456 Oak Ave", City = "Denver", State = "CO", PostalCode = "80201" };
        var created = await _service.CreateAddressAsync(dto);

        var result = await _service.GetAddressByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.City.Should().Be("Denver");
    }

    [Fact]
    public async Task GetAddressesAsync_ShouldReturnEmpty_WhenNoLinksExist()
    {
        var result = await _service.GetAddressesAsync(EntityType.Account, entityId: 1);
        result.Should().BeEmpty();
    }

    // ── Phone tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePhoneNumberAsync_ShouldPersistPhone()
    {
        var dto = new CreatePhoneNumberDto
        {
            Number = "+1-555-0100",
            PhoneType = PhoneType.Work,
            CountryCode = "US"
        };

        var result = await _service.CreatePhoneNumberAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Number.Should().Be("+1-555-0100");
    }

    [Fact]
    public async Task GetPhoneNumbersAsync_ShouldReturnEmpty_WhenNoLinksExist()
    {
        var result = await _service.GetPhoneNumbersAsync(EntityType.Contact, entityId: 1);
        result.Should().BeEmpty();
    }

    // ── Email tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEmailAddressAsync_ShouldPersistEmail()
    {
        var dto = new CreateEmailAddressDto
        {
            Email = "test@example.com",
            EmailType = EmailType.Work
        };

        var result = await _service.CreateEmailAddressAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindEmailByAddressAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        var result = await _service.FindEmailByAddressAsync("nobody@nowhere.test");
        result.Should().BeNull();
    }
}
"""

# ============================================================
# TCOV-019: LLMServiceTests.cs – new file (7 tests)
# ============================================================
files["Services/LLMServiceTests.cs"] = """\
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for LLMService (TCOV-019).</summary>
public class LLMServiceTests
{
    private static LLMService CreateService(Action<LLMProviderOptions>? configure = null)
    {
        var opts = new LLMProviderOptions();
        configure?.Invoke(opts);
        var options = Options.Create(opts);
        var logger = NullLogger<LLMService>.Instance;
        return new LLMService(logger, options);
    }

    [Fact]
    public void IsConfigured_ShouldReturnFalse_WhenOpenAIApiKeyIsEmpty()
    {
        var svc = CreateService();
        svc.IsConfigured("openai").Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_ShouldReturnTrue_WhenOpenAIApiKeyIsSet()
    {
        var svc = CreateService(o => o.OpenAI.ApiKey = "sk-test-key-1234567890");
        svc.IsConfigured("openai").Should().BeTrue();
    }

    [Fact]
    public void IsConfigured_ShouldReturnFalse_ForUnknownProvider()
    {
        var svc = CreateService();
        svc.IsConfigured("nonexistent-provider-xyz").Should().BeFalse();
    }

    [Fact]
    public void GetAvailableProviders_ShouldReturnNonEmptyList()
    {
        var svc = CreateService();
        var providers = svc.GetAvailableProviders();
        providers.Should().NotBeNull();
        providers.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAvailableModels_ShouldReturnNonEmptyList()
    {
        var svc = CreateService();
        var models = svc.GetAvailableModels();
        models.Should().NotBeNull();
        models.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IsConfiguredAsync_ShouldReturnFalse_WhenNoApiKeySet()
    {
        var svc = CreateService();
        var result = await svc.IsConfiguredAsync("openai");
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAvailableProviders_ShouldIncludeOpenAI()
    {
        var svc = CreateService();
        var providers = svc.GetAvailableProviders();
        providers.Should().Contain(p => p.Name != null && p.Name.ToLower().Contains("openai"));
    }
}
"""

# ============================================================
# TCOV-020: WorkflowWorkerServiceTests.cs – new file (5 tests)
# ============================================================
files["Services/WorkflowWorkerServiceTests.cs"] = """\
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowWorkerService (TCOV-020).</summary>
public class WorkflowWorkerServiceTests
{
    private static WorkflowWorkerService CreateService(WorkflowWorkerOptions? options = null)
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var logger = NullLogger<WorkflowWorkerService>.Instance;
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new System.Net.Http.HttpClient());

        var scriptLogger = NullLogger<ScriptEngineFactory>.Instance;
        var scriptEngineFactory = new ScriptEngineFactory(
            Enumerable.Empty<IScriptEngine>(), scriptLogger);

        return new WorkflowWorkerService(sp, logger, httpClientFactory.Object, scriptEngineFactory, options);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultMaxConcurrentTasks_ShouldBeFive()
    {
        var opts = new WorkflowWorkerOptions();
        opts.MaxConcurrentTasks.Should().Be(5);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultPollIntervalSeconds_ShouldBeFive()
    {
        var opts = new WorkflowWorkerOptions();
        opts.PollIntervalSeconds.Should().Be(5);
    }

    [Fact]
    public void WorkflowWorkerOptions_DefaultMaxRetryCount_ShouldBeThree()
    {
        var opts = new WorkflowWorkerOptions();
        opts.MaxRetryCount.Should().Be(3);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithDefaultOptions()
    {
        var act = () => CreateService();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithCustomOptions()
    {
        var opts = new WorkflowWorkerOptions
        {
            MaxConcurrentTasks = 10,
            PollIntervalSeconds = 30,
            WorkerId = "test-worker-01"
        };
        var act = () => CreateService(opts);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_ShouldComplete_WhenServiceNotYetStarted()
    {
        var svc = CreateService();
        var act = async () => await svc.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
"""

# ============================================================
# TCOV-021: WorkflowTriggerServiceTests.cs – new file (7 tests)
# ============================================================
files["Services/WorkflowTriggerServiceTests.cs"] = """\
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos.Workflow;
using CRM.Core.Entities.Workflow;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WorkflowTriggerService (TCOV-021).</summary>
public class WorkflowTriggerServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<WorkflowTriggerService>> _mockLogger;
    private readonly WorkflowTriggerService _service;

    public WorkflowTriggerServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WorkflowTriggerTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<WorkflowTriggerService>>();
        _service = new WorkflowTriggerService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<WorkflowDefinition> SeedWorkflowDefinitionAsync()
    {
        var defn = new WorkflowDefinition
        {
            Name = "TestWorkflow",
            Description = "For testing",
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        _context.WorkflowDefinitions.Add(defn);
        await _context.SaveChangesAsync();
        return defn;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTriggersExist()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTriggerDoesNotExist()
    {
        var result = await _service.GetByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTrigger_WithValidDto()
    {
        var defn = await SeedWorkflowDefinitionAsync();

        var dto = new CreateWorkflowTriggerDto
        {
            WorkflowDefinitionId = defn.Id,
            Name = "OnCreate",
            TriggerType = WorkflowTriggerType.EntityCreated,
            EntityType = "Account",
            IsActive = true
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("OnCreate");
        result.WorkflowDefinitionId.Should().Be(defn.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTrigger_WhenExists()
    {
        var defn = await SeedWorkflowDefinitionAsync();
        var dto = new CreateWorkflowTriggerDto
        {
            WorkflowDefinitionId = defn.Id,
            Name = "TestTrigger",
            TriggerType = WorkflowTriggerType.Manual,
            IsActive = true
        };
        var created = await _service.CreateAsync(dto);

        var result = await _service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestTrigger");
    }

    [Fact]
    public void ValidateCronExpression_ShouldReturnTrue_ForValidExpression()
    {
        var valid = _service.ValidateCronExpression("0 * * * *", out var error);
        valid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateCronExpression_ShouldReturnFalse_ForInvalidExpression()
    {
        var valid = _service.ValidateCronExpression("not-a-cron-expression", out var error);
        valid.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public void ValidateFilterConditions_ShouldReturnTrue_ForValidJson()
    {
        var valid = _service.ValidateFilterConditions("{\"field\":\"status\",\"op\":\"eq\",\"value\":\"active\"}", out var error);
        valid.Should().BeTrue();
        error.Should().BeNull();
    }
}
"""

# ============================================================
# TCOV-023: DuplicateDetectionServiceTests.cs – add 1 more test
# ============================================================
DUPLICATE_DETECTION_APPEND = """
    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenEntityTypeIsInvalid()
    {
        // An unknown entity type should be handled gracefully (no exception, empty duplicates)
        var fields = new Dictionary<string, string?> { ["email"] = "test@ex.com" };

        var result = await _service.CheckForDuplicatesAsync("NonExistentEntityTypeXYZ", fields);

        result.Should().NotBeNull();
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveRulesAsync_CanBeInvokedViaCheckForDuplicates_WithNoRules()
    {
        // Contact entity type is valid; with no rules the check should return cleanly
        var fields = new Dictionary<string, string?> { ["firstName"] = "Jane", ["lastName"] = "Doe" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields);

        result.Should().NotBeNull();
        result.IsDuplicate.Should().BeFalse();
    }
}
"""

# ============================================================
# TCOV-024: FormBuilderServiceTests.cs – add 5 more tests
# ============================================================
FORM_BUILDER_APPEND = """
    // ── Additional tests added for TCOV-024 ──────────────────────────────

    [Fact]
    public async Task GetAllFormsAsync_ShouldReturnOnlyPublishedForms_WhenFilteredByStatus()
    {
        // Arrange – add one Published and one Draft form
        var published = CreateTestForm("Published Form", FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(published);
        var draft = CreateTestForm("Draft Form", FormStatus.Draft);
        await _dbContext.FormDefinitions.AddAsync(draft);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFormsAsync(status: FormStatus.Published);

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("Published Form");
    }

    [Fact]
    public async Task GetFormByKeyAsync_ShouldReturnForm_WhenKeyExists()
    {
        // Arrange
        var form = CreateTestForm("Key Form");
        form.FormKey = "unique-key-abc";
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFormByKeyAsync("unique-key-abc");

        // Assert
        result.Should().NotBeNull();
        result!.FormKey.Should().Be("unique-key-abc");
    }

    [Fact]
    public async Task GetFormByKeyAsync_ShouldReturnNull_WhenKeyNotFound()
    {
        var result = await _service.GetFormByKeyAsync("nonexistent-key-zzz");
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateFormAsync_ShouldSetCreatedAt()
    {
        // Arrange
        var form = CreateTestForm("Time Form");
        form.FormKey = string.Empty; // let service generate

        // Act
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await _service.CreateFormAsync(form);
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        result.CreatedAt.Should().BeAfter(before);
        result.CreatedAt.Should().BeBefore(after);
    }

    [Fact]
    public async Task GetAllFormsAsync_ShouldNotReturnDeletedForms()
    {
        // Arrange
        var deleted = CreateTestForm("Deleted Form");
        deleted.IsDeleted = true;
        await _dbContext.FormDefinitions.AddAsync(deleted);
        var active = CreateTestForm("Active Form");
        await _dbContext.FormDefinitions.AddAsync(active);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFormsAsync();

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("Active Form");
    }
}
"""

# ===========================================================
# Helper: append tests to an existing file (replace closing })
# ===========================================================
def append_to_file(path, content_to_append):
    """Remove the final lone } and append new test methods + closing }."""
    with open(path, 'r') as f:
        original = f.read()
    # Strip trailing whitespace/newlines then remove last }
    stripped = original.rstrip()
    if stripped.endswith('}'):
        stripped = stripped[:-1].rstrip()
    new_content = stripped + '\n' + content_to_append
    with open(path, 'w') as f:
        f.write(new_content)
    print(f"  APPENDED -> {path}")


def write_new_file(relative_path, content):
    path = os.path.join(BASE, relative_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w') as f:
        f.write(content)
    print(f"  CREATED  -> {path}")


if __name__ == "__main__":
    print("Writing TCOV-016 to TCOV-024 test files...")

    # TCOV-016: Append to existing SampleDataSeederServiceTests.cs
    append_to_file(
        os.path.join(BASE, "Services/SampleDataSeederServiceTests.cs"),
        SAMPLE_SEEDER_APPEND
    )

    # TCOV-017: Append to existing ReportServiceTests.cs
    append_to_file(
        os.path.join(BASE, "Services/ReportServiceTests.cs"),
        REPORT_SERVICE_APPEND
    )

    # TCOV-018,019,020,021: Create new files
    for rel_path, content in files.items():
        write_new_file(rel_path, content)

    # TCOV-023: Append to existing DuplicateDetectionServiceTests.cs
    append_to_file(
        os.path.join(BASE, "Services/DuplicateDetectionServiceTests.cs"),
        DUPLICATE_DETECTION_APPEND
    )

    # TCOV-024: Append to existing FormBuilderServiceTests.cs
    append_to_file(
        os.path.join(BASE, "Services/FormBuilderServiceTests.cs"),
        FORM_BUILDER_APPEND
    )

    print("Done!")
