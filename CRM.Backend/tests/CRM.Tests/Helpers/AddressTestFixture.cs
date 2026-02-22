// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Tests.Helpers;

/// <summary>
/// Reusable test fixture for address-related tests.
/// Provides fluent builders and setup methods for creating test data.
///
/// FUNCTIONAL VIEW:
/// - Fluent builders for creating test addresses
/// - Fluent builders for creating test accounts with addresses
/// - Helper methods for database setup and cleanup
/// - Data seeding utilities for integration tests
///
/// TECHNICAL VIEW:
/// - Fluent builder pattern for test data creation
/// - Supports complex object hierarchies
/// - Allows easy customization of test data
/// - Reusable across multiple test classes
/// </summary>
public class AddressTestFixture
{
    /// <summary>
    /// Fluent builder for creating Address test data.
    /// </summary>
    public class TestAddressBuilder
    {
        private int _id = 1;
        private string _label = "Primary";
        private string _line1 = "123 Main Street";
        private string? _line2 = null;
        private string? _line3 = null;
        private string _city = "New York";
        private string? _state = "NY";
        private string? _postalCode = "10001";
        private string? _county = null;
        private string _countryCode = "US";
        private string _country = "United States";
        private decimal? _latitude = null;
        private decimal? _longitude = null;
        private string? _geocodeAccuracy = null;
        private bool _isVerified = false;
        private DateTime? _verifiedDate = null;
        private string? _verificationSource = null;
        private bool? _isResidential = null;
        private string? _deliveryInstructions = null;
        private string? _accessHours = null;
        private string? _siteContactName = null;
        private string? _siteContactPhone = null;
        private string? _notes = null;
        private bool _isDeleted = false;

        public TestAddressBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public TestAddressBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public TestAddressBuilder WithLine1(string line1)
        {
            _line1 = line1;
            return this;
        }

        public TestAddressBuilder WithLine2(string? line2)
        {
            _line2 = line2;
            return this;
        }

        public TestAddressBuilder WithLine3(string? line3)
        {
            _line3 = line3;
            return this;
        }

        public TestAddressBuilder WithCity(string city)
        {
            _city = city;
            return this;
        }

        public TestAddressBuilder WithState(string? state)
        {
            _state = state;
            return this;
        }

        public TestAddressBuilder WithPostalCode(string? postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public TestAddressBuilder WithCounty(string? county)
        {
            _county = county;
            return this;
        }

        public TestAddressBuilder WithCountry(string country)
        {
            _country = country;
            return this;
        }

        public TestAddressBuilder WithCountryCode(string countryCode)
        {
            _countryCode = countryCode;
            return this;
        }

        public TestAddressBuilder WithCoordinates(decimal latitude, decimal longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
            return this;
        }

        public TestAddressBuilder WithGeocodeAccuracy(string accuracy)
        {
            _geocodeAccuracy = accuracy;
            return this;
        }

        public TestAddressBuilder WithVerification(string source)
        {
            _isVerified = true;
            _verifiedDate = DateTime.UtcNow;
            _verificationSource = source;
            return this;
        }

        public TestAddressBuilder AsResidential()
        {
            _isResidential = true;
            return this;
        }

        public TestAddressBuilder AsBusiness()
        {
            _isResidential = false;
            return this;
        }

        public TestAddressBuilder WithDeliveryInstructions(string instructions)
        {
            _deliveryInstructions = instructions;
            return this;
        }

        public TestAddressBuilder WithAccessHours(string hours)
        {
            _accessHours = hours;
            return this;
        }

        public TestAddressBuilder WithSiteContact(string name, string phone)
        {
            _siteContactName = name;
            _siteContactPhone = phone;
            return this;
        }

        public TestAddressBuilder WithNotes(string notes)
        {
            _notes = notes;
            return this;
        }

        public TestAddressBuilder AsDeleted()
        {
            _isDeleted = true;
            return this;
        }

        public Address Build()
        {
            return new Address
            {
                Id = _id,
                Label = _label,
                Line1 = _line1,
                Line2 = _line2,
                Line3 = _line3,
                City = _city,
                State = _state,
                PostalCode = _postalCode,
                County = _county,
                CountryCode = _countryCode,
                Country = _country,
                Latitude = _latitude,
                Longitude = _longitude,
                GeocodeAccuracy = _geocodeAccuracy,
                IsVerified = _isVerified,
                VerifiedDate = _verifiedDate,
                VerificationSource = _verificationSource,
                IsResidential = _isResidential,
                DeliveryInstructions = _deliveryInstructions,
                AccessHours = _accessHours,
                SiteContactName = _siteContactName,
                SiteContactPhone = _siteContactPhone,
                Notes = _notes,
                IsDeleted = _isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Fluent builder for creating Account test data with addresses.
    /// </summary>
    public class TestAccountBuilder
    {
        private int _id = 1;
        private string _email = "test@example.com";
        private string _firstName = "Test Company";
        private string? _lastName = null;
        private string _phone = string.Empty;
        private string? _website = null;
        private bool _isDeleted = false;
        private readonly List<Address> _addresses = new();
        private readonly List<EntityAddressLink> _addressLinks = new();

        public TestAccountBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public TestAccountBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public TestAccountBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public TestAccountBuilder WithLastName(string? lastName)
        {
            _lastName = lastName;
            return this;
        }

        public TestAccountBuilder WithPhone(string? phone)
        {
            _phone = phone ?? string.Empty;
            return this;
        }

        public TestAccountBuilder WithWebsite(string? website)
        {
            _website = website;
            return this;
        }

        public TestAccountBuilder WithAddress(Address address)
        {
            _addresses.Add(address);
            _addressLinks.Add(new EntityAddressLink
            {
                Id = _addressLinks.Count + 1,
                AddressId = address.Id,
                EntityId = _id,
                EntityType = EntityType.Account,
                AddressType = AddressType.Primary,
                IsPrimary = _addressLinks.Count == 0
            });
            return this;
        }

        public TestAccountBuilder WithBillingAddress(Address address)
        {
            _addresses.Add(address);
            _addressLinks.Add(new EntityAddressLink
            {
                Id = _addressLinks.Count + 1,
                AddressId = address.Id,
                EntityId = _id,
                EntityType = EntityType.Account,
                AddressType = AddressType.Billing,
                IsPrimary = _addressLinks.Count == 0
            });
            return this;
        }

        public TestAccountBuilder WithShippingAddress(Address address)
        {
            _addresses.Add(address);
            _addressLinks.Add(new EntityAddressLink
            {
                Id = _addressLinks.Count + 1,
                AddressId = address.Id,
                EntityId = _id,
                EntityType = EntityType.Account,
                AddressType = AddressType.Shipping,
                IsPrimary = _addressLinks.Count == 0
            });
            return this;
        }

        public TestAccountBuilder AsDeleted()
        {
            _isDeleted = true;
            return this;
        }

        public Account Build()
        {
            var account = new Account
            {
                Id = _id,
                Email = _email,
                FirstName = _firstName,
                LastName = _lastName ?? string.Empty,
                Phone = _phone,
                Website = _website,
                IsDeleted = _isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EntityAddressLinks = _addressLinks
            };

            return account;
        }

        public (Account Account, List<Address> Addresses, List<EntityAddressLink> Links) BuildWithAddresses()
        {
            var account = Build();
            return (account, _addresses, _addressLinks);
        }
    }

    /// <summary>
    /// Create a builder for a test address.
    /// </summary>
    public static TestAddressBuilder CreateAddress()
    {
        return new TestAddressBuilder();
    }

    /// <summary>
    /// Create a builder for a test account.
    /// </summary>
    public static TestAccountBuilder CreateAccount()
    {
        return new TestAccountBuilder();
    }

    /// <summary>
    /// Create multiple test addresses with different cities.
    /// </summary>
    public static List<Address> CreateMultipleAddresses(int count)
    {
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix" };
        var states = new[] { "NY", "CA", "IL", "TX", "AZ" };
        var addresses = new List<Address>();

        for (int i = 0; i < count; i++)
        {
            var city = cities[i % cities.Length];
            var state = states[i % states.Length];

            addresses.Add(CreateAddress()
                .WithId(i + 1)
                .WithLabel($"Address {i + 1}")
                .WithLine1($"{100 + i} Main Street")
                .WithCity(city)
                .WithState(state)
                .WithPostalCode($"{10000 + i}")
                .Build());
        }

        return addresses;
    }

    /// <summary>
    /// Create a complete test dataset with accounts and addresses.
    /// </summary>
    public static (List<Account> Accounts, List<Address> Addresses, List<EntityAddressLink> Links)
        CreateTestDataset()
    {
        var accounts = new List<Account>();
        var addresses = new List<Address>();
        var links = new List<EntityAddressLink>();

        // Create account 1 with 2 addresses
        var addr1 = CreateAddress().WithId(1).WithLine1("123 Main St").WithCity("New York").Build();
        var addr2 = CreateAddress().WithId(2).WithLine1("456 Oak Ave").WithCity("New York").Build();

        var account1 = CreateAccount()
            .WithId(1)
            .WithEmail("acme@example.com")
            .WithFirstName("ACME Corp")
            .WithBillingAddress(addr1)
            .WithShippingAddress(addr2)
            .Build();

        addresses.AddRange(new[] { addr1, addr2 });
        accounts.Add(account1);

        // Create account 2 with 1 address
        var addr3 = CreateAddress().WithId(3).WithLine1("789 Pine Rd").WithCity("Los Angeles").Build();
        var account2 = CreateAccount()
            .WithId(2)
            .WithEmail("globex@example.com")
            .WithFirstName("Globex Corp")
            .WithBillingAddress(addr3)
            .Build();

        addresses.Add(addr3);
        accounts.Add(account2);

        // Populate links from accounts
        foreach (var account in accounts)
        {
            if (account.EntityAddressLinks != null)
            {
                links.AddRange(account.EntityAddressLinks);
            }
        }

        return (accounts, addresses, links);
    }

    /// <summary>
    /// Seed test data into the database context.
    /// </summary>
    public static async Task SeedAddressesAsync(
        ICrmDbContext context,
        List<Address> addresses,
        CancellationToken cancellationToken = default)
    {
        foreach (var address in addresses)
        {
            context.Addresses.Add(address);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Seed test data into the database context.
    /// </summary>
    public static async Task SeedAccountsAsync(
        ICrmDbContext context,
        List<Account> accounts,
        CancellationToken cancellationToken = default)
    {
        foreach (var account in accounts)
        {
            context.Accounts.Add(account);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Seed entity address links into the database context.
    /// </summary>
    public static async Task SeedEntityAddressLinksAsync(
        ICrmDbContext context,
        List<EntityAddressLink> links,
        CancellationToken cancellationToken = default)
    {
        foreach (var link in links)
        {
            context.EntityAddressLinks.Add(link);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Clean up all address-related data from the database context.
    /// </summary>
    public static async Task CleanupAddressDataAsync(
        ICrmDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Delete address links first (foreign key constraint)
        var links = context.EntityAddressLinks.Where(l => !l.IsDeleted).ToList();
        foreach (var link in links)
        {
            link.IsDeleted = true;
            context.EntityAddressLinks.Update(link);
        }

        // Delete addresses
        var addresses = context.Addresses.Where(a => !a.IsDeleted).ToList();
        foreach (var address in addresses)
        {
            address.IsDeleted = true;
            context.Addresses.Update(address);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get all active addresses (not soft-deleted).
    /// </summary>
    public static List<Address> GetActiveAddresses(ICrmDbContext context)
    {
        return context.Addresses.Where(a => !a.IsDeleted).ToList();
    }

    /// <summary>
    /// Get addresses for a specific account.
    /// </summary>
    public static List<Address> GetAddressesForAccount(
        ICrmDbContext context,
        int accountId)
    {
        return context.EntityAddressLinks
            .Where(l => l.EntityId == accountId && l.EntityType == EntityType.Account && !l.IsDeleted)
            .Join(
                context.Addresses.Where(a => !a.IsDeleted),
                link => link.AddressId,
                address => address.Id,
                (link, address) => address
            )
            .ToList();
    }

    /// <summary>
    /// Verify address data integrity.
    /// </summary>
    public static bool VerifyAddressIntegrity(Address address)
    {
        // Required fields
        if (string.IsNullOrWhiteSpace(address.Line1))
            return false;

        if (string.IsNullOrWhiteSpace(address.City))
            return false;

        if (string.IsNullOrWhiteSpace(address.Country))
            return false;

        // Timestamp validation
        if (address.CreatedAt == default)
            return false;

        if (address.UpdatedAt == default)
            return false;

        if (address.UpdatedAt < address.CreatedAt)
            return false;

        return true;
    }

    /// <summary>
    /// Verify all addresses in a collection for data integrity.
    /// </summary>
    public static bool VerifyAddressesIntegrity(List<Address> addresses)
    {
        return addresses.All(VerifyAddressIntegrity);
    }
}
