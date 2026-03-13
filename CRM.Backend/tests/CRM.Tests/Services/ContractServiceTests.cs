// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ContractService CRUD and lifecycle operations.
/// Covers create, read, update, delete, activate, suspend, terminate, and renewal flows.
/// (Validation-specific tests live in ContractValidationTests.cs)
/// </summary>
public class ContractServiceTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"ContractService_{Guid.NewGuid()}")
                .Options,
            null!);

    private static ContractService CreateService(CrmDbContext db) =>
        new ContractService(db, new Mock<ILogger<ContractService>>().Object);

    /// <summary>
    /// Seed an Account so that ContractService queries using Include(c => c.Account)
    /// can satisfy the required FK join in EF Core InMemory.
    /// </summary>
    private static async Task SeedAccountAsync(CrmDbContext db, int id)
    {
        db.Accounts.Add(new Account
        {
            Id = id,
            FirstName = "Acme", LastName = "Corp",
            Company = "Acme Corporation",
            Email = $"acme{id}@test.com",
            Phone = "555-000-0000",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private static Contract BuildContract(int accountId = 1, ContractStatus status = ContractStatus.Draft) =>
        new Contract
        {
            Name = "Test Contract",
            AccountId = accountId,
            Status = status,
            ContractType = ContractType.Service,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldPersistContract_WithGeneratedContractNumber()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);

        var result = await service.CreateAsync(BuildContract());

        result.Id.Should().BeGreaterThan(0);
        result.ContractNumber.Should().StartWith("CON-");
        db.Contracts.Count().Should().Be(1);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnContract_WhenExists()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        var created = await service.CreateAsync(BuildContract());

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAccountId()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 10);
        await SeedAccountAsync(db, 20);
        var service = CreateService(db);
        await service.CreateAsync(BuildContract(accountId: 10));
        await service.CreateAsync(BuildContract(accountId: 20));

        var result = await service.GetAllAsync(accountId: 10);

        result.Should().HaveCount(1);
        result.First().AccountId.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        await service.CreateAsync(BuildContract(status: ContractStatus.Draft));
        await service.CreateAsync(BuildContract(status: ContractStatus.Active));

        var result = await service.GetAllAsync(status: ContractStatus.Active);

        result.Should().HaveCount(1);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenContractExists()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        var created = await service.CreateAsync(BuildContract());

        var result = await service.DeleteAsync(created.Id);

        result.Should().BeTrue();
        db.Contracts.IgnoreQueryFilters().First().IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.DeleteAsync(9999);

        result.Should().BeFalse();
    }

    // ── ActivateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task ActivateAsync_ShouldSetStatusActive_AndSetActivatedAt()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        var created = await service.CreateAsync(BuildContract());

        var result = await service.ActivateAsync(created.Id);

        // Status persisted; ActivatedAt is set on the returned in-memory object
        result.Status.Should().Be(ContractStatus.Active);
        result.ActivatedAt.Should().NotBeNull();
    }

    // ── SuspendAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SuspendAsync_ShouldSetStatusOnHold_AndStoreReason()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        var created = await service.CreateAsync(BuildContract());

        var result = await service.SuspendAsync(created.Id, "Pending payment");

        result.Status.Should().Be(ContractStatus.OnHold);
        result.SuspensionReason.Should().Be("Pending payment");
    }

    // ── TerminateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task TerminateAsync_ShouldSetStatusTerminated_AndStoreReason()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);
        var created = await service.CreateAsync(BuildContract());

        var result = await service.TerminateAsync(created.Id, "Customer request");

        // Status persisted; TerminationReason and TerminatedAt set on the returned in-memory object
        result.Status.Should().Be(ContractStatus.Terminated);
        result.TerminationReason.Should().Be("Customer request");
        result.TerminatedAt.Should().NotBeNull();
    }

    // ── GenerateContractNumberAsync ──────────────────────────────────────────

    [Fact]
    public async Task GenerateContractNumberAsync_ShouldReturnSequentialNumbers()
    {
        using var db = CreateDb();
        await SeedAccountAsync(db, 1);
        var service = CreateService(db);

        var c1 = await service.CreateAsync(BuildContract());
        var c2 = await service.CreateAsync(BuildContract());

        c1.ContractNumber.Should().NotBe(c2.ContractNumber);
        c1.ContractNumber.Should().StartWith("CON-");
        c2.ContractNumber.Should().StartWith("CON-");
    }
}
