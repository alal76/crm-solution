// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Dtos;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Account and Contact DTOs.
/// Tests property initialization, default values, computed properties, and collections.
/// </summary>
public class AccountContactDtoTests
{
    #region AccountDto Tests

    public class AccountDtoTests
    {
        [Fact]
        public void AccountDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new AccountDto();

            // Assert - Strings default to empty
            dto.Category.Should().Be("Individual");
            dto.FirstName.Should().BeEmpty();
            dto.LastName.Should().BeEmpty();
            dto.Email.Should().BeEmpty();
            dto.Phone.Should().BeEmpty();
            dto.Company.Should().BeEmpty();
            dto.Address.Should().BeEmpty();
            dto.City.Should().BeEmpty();
            dto.State.Should().BeEmpty();
            dto.ZipCode.Should().BeEmpty();
            dto.Country.Should().BeEmpty();
            dto.Notes.Should().BeEmpty();
            dto.DisplayName.Should().BeEmpty();

            // Assert - Numerics default to zero
            dto.Id.Should().Be(0);
            dto.AnnualRevenue.Should().Be(0);
            dto.TotalPurchases.Should().Be(0);
            dto.AccountBalance.Should().Be(0);
            dto.CreditLimit.Should().Be(0);
            dto.LeadScore.Should().Be(0);
            dto.AccountHealthScore.Should().Be(50);
            dto.NpsScore.Should().Be(0);
            dto.SatisfactionRating.Should().Be(0);
            dto.ContactCount.Should().Be(0);

            // Assert - Bools
            dto.ShippingSameAsBilling.Should().BeTrue();
            dto.OptInEmail.Should().BeTrue();
            dto.OptInSms.Should().BeFalse();
            dto.OptInPhone.Should().BeTrue();

            // Assert - Strings with defaults
            dto.AccountType.Should().Be("Individual");
            dto.Priority.Should().Be("Medium");
            dto.LifecycleStage.Should().Be("Lead");
        }

        [Fact]
        public void AccountDto_IsOrganization_WhenCategoryIsOrganization_ShouldReturnTrue()
        {
            // Arrange
            var dto = new AccountDto { Category = "Organization" };

            // Act & Assert
            dto.IsOrganization.Should().BeTrue();
        }

        [Fact]
        public void AccountDto_IsOrganization_WhenCategoryIsIndividual_ShouldReturnFalse()
        {
            // Arrange
            var dto = new AccountDto { Category = "Individual" };

            // Act & Assert
            dto.IsOrganization.Should().BeFalse();
        }

        [Fact]
        public void AccountDto_IsOrganization_WhenCategoryIsOther_ShouldReturnFalse()
        {
            // Arrange
            var dto = new AccountDto { Category = "Other" };

            // Act & Assert
            dto.IsOrganization.Should().BeFalse();
        }

        [Fact]
        public void AccountDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            // Act
            var dto = new AccountDto
            {
                Id = 123,
                Category = "Organization",
                FirstName = "John",
                LastName = "Doe",
                Salutation = "Mr.",
                Suffix = "Jr.",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "Male",
                LinkedContactId = 456,
                LinkedContactName = "Jane Doe",
                Company = "Acme Corp",
                LegalName = "Acme Corporation LLC",
                DbaName = "Acme",
                TaxId = "12-3456789",
                RegistrationNumber = "REG-12345",
                YearFounded = 2010,
                PrimaryContactId = 789,
                PrimaryContactName = "Bob Smith",
                Email = "john@acme.com",
                SecondaryEmail = "backup@acme.com",
                Phone = "555-1234",
                MobilePhone = "555-5678",
                FaxNumber = "555-9999",
                JobTitle = "CEO",
                Website = "https://acme.com",
                Address = "123 Main St",
                Address2 = "Suite 100",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA",
                ShippingAddress = "456 Warehouse Dr",
                ShippingCity = "Chicago",
                ShippingState = "IL",
                ShippingZipCode = "60601",
                ShippingCountry = "USA",
                ShippingSameAsBilling = false,
                Industry = "Technology",
                SubIndustry = "Software",
                NumberOfEmployees = 500,
                EmployeeRange = "100-1000",
                AnnualRevenue = 10000000m,
                RevenueRange = "$10M-$50M",
                AccountType = "Enterprise",
                Priority = "High",
                StockSymbol = "ACME",
                Ownership = "Public",
                LifecycleStage = "Customer",
                LeadSource = "Referral",
                FirstContactDate = now.AddDays(-30),
                ConversionDate = now.AddDays(-15),
                LastActivityDate = now.AddDays(-1),
                NextFollowUpDate = now.AddDays(7),
                TotalPurchases = 50000m,
                AccountBalance = 5000m,
                CreditLimit = 100000m,
                PaymentTerms = "Net 30",
                PreferredPaymentMethod = "Wire",
                Currency = "USD",
                BillingCycle = "Monthly",
                LeadScore = 85,
                AccountHealthScore = 90,
                NpsScore = 9,
                SatisfactionRating = 4.5,
                LinkedInUrl = "https://linkedin.com/company/acme",
                TwitterHandle = "@acme",
                FacebookUrl = "https://facebook.com/acme",
                OptInEmail = true,
                OptInSms = true,
                OptInPhone = true,
                PreferredContactMethod = "Email",
                PreferredContactTime = "Morning",
                Timezone = "America/Chicago",
                PreferredLanguage = "en-US",
                AssignedToUserId = 101,
                AssignedToUserName = "Sales Rep",
                AccountManagerId = 102,
                AccountManagerName = "Account Manager",
                Territory = "Midwest",
                Region = "North America",
                Tags = "enterprise,key-account",
                Segment = "Enterprise",
                ReferralSource = "Partner",
                ReferredByAccountId = 999,
                ReferredByAccountName = "Partner Corp",
                ParentAccountId = 888,
                ParentAccountName = "Parent Corp",
                Notes = "Important customer",
                InternalNotes = "VIP treatment",
                Description = "Large enterprise account",
                CustomFields = "{}",
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now,
                RowVersion = rowVersion,
                DisplayName = "Acme Corp",
                ContactCount = 5
            };

            // Assert - Sample of key properties
            dto.Id.Should().Be(123);
            dto.Company.Should().Be("Acme Corp");
            dto.Email.Should().Be("john@acme.com");
            dto.AnnualRevenue.Should().Be(10000000m);
            dto.AccountHealthScore.Should().Be(90);
            dto.RowVersion.Should().BeEquivalentTo(rowVersion);
            dto.DisplayName.Should().Be("Acme Corp");
            dto.IsOrganization.Should().BeTrue();
        }

        [Fact]
        public void AccountDto_Contacts_CanBeInitialized()
        {
            // Arrange
            var dto = new AccountDto
            {
                Contacts = new List<AccountContactDto>
                {
                    new AccountContactDto { Id = 1, ContactName = "Contact 1" },
                    new AccountContactDto { Id = 2, ContactName = "Contact 2" }
                }
            };

            // Assert
            dto.Contacts.Should().HaveCount(2);
            dto.Contacts[0].ContactName.Should().Be("Contact 1");
        }

        [Fact]
        public void AccountDto_EmailAddresses_CanBeInitialized()
        {
            // Arrange
            var dto = new AccountDto
            {
                EmailAddresses = new List<LinkedEmailDto>
                {
                    new LinkedEmailDto { Email = "primary@example.com", IsPrimary = true },
                    new LinkedEmailDto { Email = "secondary@example.com", IsPrimary = false }
                }
            };

            // Assert
            dto.EmailAddresses.Should().HaveCount(2);
            dto.EmailAddresses[0].IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void AccountDto_NullableCollections_CanBeNull()
        {
            // Arrange
            var dto = new AccountDto();

            // Assert
            dto.Contacts.Should().BeNull();
            dto.EmailAddresses.Should().BeNull();
            dto.PhoneNumbers.Should().BeNull();
            dto.Addresses.Should().BeNull();
            dto.SocialMediaAccounts.Should().BeNull();
        }
    }

    #endregion

    #region AccountContactDto Tests

    public class AccountContactDtoDetailTests
    {
        [Fact]
        public void AccountContactDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new AccountContactDto();

            // Assert
            dto.Id.Should().Be(0);
            dto.AccountId.Should().Be(0);
            dto.ContactId.Should().Be(0);
            dto.ContactName.Should().BeEmpty();
            dto.Role.Should().Be("Primary");
            dto.IsPrimaryContact.Should().BeFalse();
            dto.IsDecisionMaker.Should().BeFalse();
            dto.ReceivesBillingNotifications.Should().BeFalse();
            dto.ReceivesMarketingEmails.Should().BeTrue();
            dto.ReceivesTechnicalUpdates.Should().BeFalse();
        }

        [Fact]
        public void AccountContactDto_AllProperties_CanBeSet()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var dto = new AccountContactDto
            {
                Id = 1,
                AccountId = 100,
                ContactId = 200,
                ContactName = "Jane Smith",
                ContactEmail = "jane@example.com",
                ContactPhone = "555-9876",
                Role = "Technical",
                IsPrimaryContact = true,
                IsDecisionMaker = true,
                ReceivesBillingNotifications = true,
                ReceivesMarketingEmails = true,
                ReceivesTechnicalUpdates = true,
                PositionAtAccount = "CTO",
                DepartmentAtAccount = "Engineering",
                RelationshipStartDate = now.AddDays(-365),
                RelationshipEndDate = null,
                Notes = "Key technical contact",
                CreatedAt = now
            };

            // Assert
            dto.ContactName.Should().Be("Jane Smith");
            dto.IsPrimaryContact.Should().BeTrue();
            dto.IsDecisionMaker.Should().BeTrue();
            dto.PositionAtAccount.Should().Be("CTO");
        }
    }

    #endregion

    #region CreateAccountDto Tests

    public class CreateAccountDtoTests
    {
        [Fact]
        public void CreateAccountDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new CreateAccountDto();

            // Assert
            dto.Category.Should().Be(AccountCategory.Individual);
            dto.Email.Should().BeEmpty();
            dto.Phone.Should().BeEmpty();
            dto.AccountType.Should().Be(AccountType.Individual);
            dto.Priority.Should().Be(AccountPriority.Medium);
            dto.LifecycleStage.Should().Be(AccountLifecycleStage.Lead);
            dto.ShippingSameAsBilling.Should().BeTrue();
            dto.OptInEmail.Should().BeTrue();
            dto.OptInSms.Should().BeFalse();
            dto.OptInPhone.Should().BeTrue();
        }

        [Fact]
        public void CreateAccountDto_OrganizationFields_CanBeSet()
        {
            // Arrange & Act
            var dto = new CreateAccountDto
            {
                Category = AccountCategory.Organization,
                Company = "Tech Corp",
                LegalName = "Technology Corporation Inc.",
                DbaName = "TechCorp",
                TaxId = "98-7654321",
                RegistrationNumber = "TC-12345",
                YearFounded = 2015
            };

            // Assert
            dto.Category.Should().Be(AccountCategory.Organization);
            dto.Company.Should().Be("Tech Corp");
            dto.YearFounded.Should().Be(2015);
        }

        [Fact]
        public void CreateAccountDto_IndividualFields_CanBeSet()
        {
            // Arrange & Act
            var dto = new CreateAccountDto
            {
                Category = AccountCategory.Individual,
                FirstName = "Alice",
                LastName = "Johnson",
                Salutation = "Ms.",
                DateOfBirth = new DateTime(1985, 8, 20),
                Gender = "Female",
                LinkedContactId = 42
            };

            // Assert
            dto.Category.Should().Be(AccountCategory.Individual);
            dto.FirstName.Should().Be("Alice");
            dto.LastName.Should().Be("Johnson");
            dto.DateOfBirth.Should().Be(new DateTime(1985, 8, 20));
        }

        [Fact]
        public void CreateAccountDto_BusinessInfo_CanBeSet()
        {
            // Arrange & Act
            var dto = new CreateAccountDto
            {
                Industry = "Healthcare",
                SubIndustry = "Medical Devices",
                NumberOfEmployees = 1500,
                EmployeeRange = "1000-5000",
                AnnualRevenue = 50000000m,
                RevenueRange = "$50M-$100M",
                AccountType = AccountType.Enterprise,
                Priority = AccountPriority.High
            };

            // Assert
            dto.Industry.Should().Be("Healthcare");
            dto.NumberOfEmployees.Should().Be(1500);
            dto.AnnualRevenue.Should().Be(50000000m);
            dto.AccountType.Should().Be(AccountType.Enterprise);
            dto.Priority.Should().Be(AccountPriority.High);
        }
    }

    #endregion

    #region UpdateAccountDto Tests

    public class UpdateAccountDtoTests
    {
        [Fact]
        public void UpdateAccountDto_AllNullByDefault()
        {
            // Arrange & Act
            var dto = new UpdateAccountDto();

            // Assert - All nullable properties should be null
            dto.Category.Should().BeNull();
            dto.FirstName.Should().BeNull();
            dto.LastName.Should().BeNull();
            dto.Company.Should().BeNull();
            dto.Email.Should().BeNull();
            dto.Phone.Should().BeNull();
            dto.AccountType.Should().BeNull();
            dto.Priority.Should().BeNull();
            dto.LifecycleStage.Should().BeNull();
            dto.ShippingSameAsBilling.Should().BeNull();
            dto.CreditLimit.Should().BeNull();
            dto.LeadScore.Should().BeNull();
            dto.AccountHealthScore.Should().BeNull();
            dto.OptInEmail.Should().BeNull();
        }

        [Fact]
        public void UpdateAccountDto_PartialUpdate_CanSetSomeFields()
        {
            // Arrange & Act
            var dto = new UpdateAccountDto
            {
                Email = "newemail@example.com",
                Phone = "555-NEW-PHONE",
                AccountHealthScore = 75,
                OptInSms = true
            };

            // Assert - Set fields
            dto.Email.Should().Be("newemail@example.com");
            dto.Phone.Should().Be("555-NEW-PHONE");
            dto.AccountHealthScore.Should().Be(75);
            dto.OptInSms.Should().BeTrue();

            // Assert - Other fields remain null
            dto.FirstName.Should().BeNull();
            dto.Company.Should().BeNull();
            dto.AccountType.Should().BeNull();
        }

        [Fact]
        public void UpdateAccountDto_FinancialFields_CanBeSet()
        {
            // Arrange & Act
            var dto = new UpdateAccountDto
            {
                CreditLimit = 250000m,
                PaymentTerms = "Net 45",
                PreferredPaymentMethod = "ACH",
                Currency = "EUR",
                BillingCycle = "Quarterly"
            };

            // Assert
            dto.CreditLimit.Should().Be(250000m);
            dto.PaymentTerms.Should().Be("Net 45");
            dto.Currency.Should().Be("EUR");
        }

        [Fact]
        public void UpdateAccountDto_SocialFields_CanBeSet()
        {
            // Arrange & Act
            var dto = new UpdateAccountDto
            {
                LinkedInUrl = "https://linkedin.com/in/user",
                TwitterHandle = "@userhandle",
                FacebookUrl = "https://facebook.com/user"
            };

            // Assert
            dto.LinkedInUrl.Should().Be("https://linkedin.com/in/user");
            dto.TwitterHandle.Should().Be("@userhandle");
            dto.FacebookUrl.Should().Be("https://facebook.com/user");
        }
    }

    #endregion

    #region LinkContactToAccountDto Tests

    public class LinkContactToAccountDtoTests
    {
        [Fact]
        public void LinkContactToAccountDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new LinkContactToAccountDto();

            // Assert
            dto.ContactId.Should().Be(0);
            dto.Role.Should().Be(AccountContactRole.Primary);
            dto.IsPrimaryContact.Should().BeFalse();
            dto.IsDecisionMaker.Should().BeFalse();
            dto.ReceivesBillingNotifications.Should().BeFalse();
            dto.ReceivesMarketingEmails.Should().BeTrue();
            dto.ReceivesTechnicalUpdates.Should().BeFalse();
            dto.PositionAtAccount.Should().BeNull();
            dto.DepartmentAtAccount.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        [Fact]
        public void LinkContactToAccountDto_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var dto = new LinkContactToAccountDto
            {
                ContactId = 999,
                Role = AccountContactRole.Technical,
                IsPrimaryContact = true,
                IsDecisionMaker = true,
                ReceivesBillingNotifications = true,
                ReceivesMarketingEmails = false,
                ReceivesTechnicalUpdates = true,
                PositionAtAccount = "VP Engineering",
                DepartmentAtAccount = "Engineering",
                Notes = "Technical decision maker"
            };

            // Assert
            dto.ContactId.Should().Be(999);
            dto.Role.Should().Be(AccountContactRole.Technical);
            dto.IsPrimaryContact.Should().BeTrue();
            dto.PositionAtAccount.Should().Be("VP Engineering");
        }
    }

    #endregion

    #region UpdateAccountContactDto Tests

    public class UpdateAccountContactDtoTests
    {
        [Fact]
        public void UpdateAccountContactDto_AllNullByDefault()
        {
            // Arrange & Act
            var dto = new UpdateAccountContactDto();

            // Assert
            dto.Role.Should().BeNull();
            dto.IsPrimaryContact.Should().BeNull();
            dto.IsDecisionMaker.Should().BeNull();
            dto.ReceivesBillingNotifications.Should().BeNull();
            dto.ReceivesMarketingEmails.Should().BeNull();
            dto.ReceivesTechnicalUpdates.Should().BeNull();
            dto.PositionAtAccount.Should().BeNull();
            dto.DepartmentAtAccount.Should().BeNull();
            dto.RelationshipEndDate.Should().BeNull();
            dto.Notes.Should().BeNull();
        }

        [Fact]
        public void UpdateAccountContactDto_PartialUpdate_CanSetSomeFields()
        {
            // Arrange
            var endDate = DateTime.UtcNow.AddDays(30);

            // Act
            var dto = new UpdateAccountContactDto
            {
                Role = AccountContactRole.Billing,
                ReceivesBillingNotifications = true,
                RelationshipEndDate = endDate
            };

            // Assert
            dto.Role.Should().Be(AccountContactRole.Billing);
            dto.ReceivesBillingNotifications.Should().BeTrue();
            dto.RelationshipEndDate.Should().Be(endDate);
            dto.IsPrimaryContact.Should().BeNull();
        }
    }

    #endregion

    #region ContactDto Tests

    public class ContactDtoTests
    {
        [Fact]
        public void ContactDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new ContactDto();

            // Assert
            dto.Id.Should().Be(0);
            dto.ContactType.Should().Be("Other");
            dto.FirstName.Should().BeEmpty();
            dto.LastName.Should().BeEmpty();
            dto.Status.Should().Be("Active");
            dto.SocialMediaLinks.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ContactDto_AllProperties_CanBeSet()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dob = new DateTime(1980, 3, 15);

            // Act
            var dto = new ContactDto
            {
                Id = 42,
                ContactType = "Professional",
                FirstName = "Robert",
                LastName = "Williams",
                MiddleName = "James",
                EmailPrimary = "robert@example.com",
                EmailSecondary = "r.williams@work.com",
                PhonePrimary = "555-1111",
                PhoneSecondary = "555-2222",
                Address = "789 Oak Ave",
                City = "Denver",
                State = "CO",
                Country = "USA",
                ZipCode = "80202",
                JobTitle = "Software Engineer",
                Department = "Engineering",
                Company = "Tech Solutions",
                ReportsTo = "Jane Manager",
                Notes = "Key technical contact",
                DateOfBirth = dob,
                DateAdded = now.AddDays(-90),
                LastModified = now,
                ModifiedBy = "admin",
                AccountId = 100,
                Status = "Active"
            };

            // Assert
            dto.Id.Should().Be(42);
            dto.ContactType.Should().Be("Professional");
            dto.FirstName.Should().Be("Robert");
            dto.LastName.Should().Be("Williams");
            dto.EmailPrimary.Should().Be("robert@example.com");
            dto.JobTitle.Should().Be("Software Engineer");
            dto.AccountId.Should().Be(100);
        }

        [Fact]
        public void ContactDto_SocialMediaLinks_CanBePopulated()
        {
            // Arrange
            var dto = new ContactDto
            {
                SocialMediaLinks = new List<SocialMediaLinkDto>
                {
                    new SocialMediaLinkDto { Platform = "LinkedIn", Url = "https://linkedin.com/in/user", Handle = "user" },
                    new SocialMediaLinkDto { Platform = "Twitter", Url = "https://twitter.com/user", Handle = "@user" }
                }
            };

            // Assert
            dto.SocialMediaLinks.Should().HaveCount(2);
            dto.SocialMediaLinks[0].Platform.Should().Be("LinkedIn");
            dto.SocialMediaLinks[1].Handle.Should().Be("@user");
        }

        [Fact]
        public void ContactDto_NormalizedCollections_CanBeInitialized()
        {
            // Arrange
            var dto = new ContactDto
            {
                EmailAddresses = new List<LinkedEmailDto>
                {
                    new LinkedEmailDto { Email = "work@example.com", IsPrimary = true }
                },
                PhoneNumbers = new List<LinkedPhoneDto>
                {
                    new LinkedPhoneDto { Number = "555-3333", IsPrimary = true }
                },
                Addresses = new List<LinkedAddressDto>
                {
                    new LinkedAddressDto { City = "Seattle", IsPrimary = true }
                }
            };

            // Assert
            dto.EmailAddresses.Should().HaveCount(1);
            dto.PhoneNumbers.Should().HaveCount(1);
            dto.Addresses.Should().HaveCount(1);
        }
    }

    #endregion

    #region CreateContactRequest Tests

    public class CreateContactRequestTests
    {
        [Fact]
        public void CreateContactRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new CreateContactRequest();

            // Assert
            dto.ContactType.Should().Be("Other");
            dto.FirstName.Should().BeEmpty();
            dto.LastName.Should().BeEmpty();
        }

        [Fact]
        public void CreateContactRequest_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var dto = new CreateContactRequest
            {
                ContactType = "Business",
                FirstName = "Sarah",
                LastName = "Connor",
                MiddleName = "Jane",
                EmailPrimary = "sarah@example.com",
                EmailSecondary = "sarah.connor@work.com",
                PhonePrimary = "555-4444",
                PhoneSecondary = "555-5555",
                Address = "123 Future St",
                City = "Los Angeles",
                State = "CA",
                Country = "USA",
                ZipCode = "90001",
                JobTitle = "Security Consultant",
                Department = "Operations",
                Company = "Cyberdyne",
                ReportsTo = "Miles Dyson",
                Notes = "Important contact",
                DateOfBirth = new DateTime(1965, 11, 13)
            };

            // Assert
            dto.ContactType.Should().Be("Business");
            dto.FirstName.Should().Be("Sarah");
            dto.LastName.Should().Be("Connor");
            dto.DateOfBirth.Should().Be(new DateTime(1965, 11, 13));
        }
    }

    #endregion

    #region UpdateContactRequest Tests

    public class UpdateContactRequestTests
    {
        [Fact]
        public void UpdateContactRequest_AllNullByDefault()
        {
            // Arrange & Act
            var dto = new UpdateContactRequest();

            // Assert
            dto.ContactType.Should().BeNull();
            dto.FirstName.Should().BeNull();
            dto.LastName.Should().BeNull();
            dto.EmailPrimary.Should().BeNull();
            dto.PhonePrimary.Should().BeNull();
            dto.JobTitle.Should().BeNull();
        }

        [Fact]
        public void UpdateContactRequest_PartialUpdate_CanSetSomeFields()
        {
            // Arrange & Act
            var dto = new UpdateContactRequest
            {
                EmailPrimary = "updated@example.com",
                JobTitle = "Senior Developer"
            };

            // Assert
            dto.EmailPrimary.Should().Be("updated@example.com");
            dto.JobTitle.Should().Be("Senior Developer");
            dto.FirstName.Should().BeNull();
            dto.Company.Should().BeNull();
        }
    }

    #endregion

    #region SocialMediaLinkDto Tests

    public class SocialMediaLinkDtoTests
    {
        [Fact]
        public void SocialMediaLinkDto_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new SocialMediaLinkDto();

            // Assert
            dto.Id.Should().Be(0);
            dto.Platform.Should().BeEmpty();
            dto.Url.Should().BeEmpty();
            dto.Handle.Should().BeNull();
        }

        [Fact]
        public void SocialMediaLinkDto_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var dto = new SocialMediaLinkDto
            {
                Id = 5,
                Platform = "GitHub",
                Url = "https://github.com/user",
                Handle = "@user"
            };

            // Assert
            dto.Id.Should().Be(5);
            dto.Platform.Should().Be("GitHub");
            dto.Url.Should().Be("https://github.com/user");
            dto.Handle.Should().Be("@user");
        }
    }

    #endregion

    #region AddSocialMediaRequest Tests

    public class AddSocialMediaRequestTests
    {
        [Fact]
        public void AddSocialMediaRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var dto = new AddSocialMediaRequest();

            // Assert
            dto.Platform.Should().BeEmpty();
            dto.Url.Should().BeEmpty();
            dto.Handle.Should().BeNull();
        }

        [Fact]
        public void AddSocialMediaRequest_AllProperties_CanBeSet()
        {
            // Arrange & Act
            var dto = new AddSocialMediaRequest
            {
                Platform = "Instagram",
                Url = "https://instagram.com/user",
                Handle = "@instauser"
            };

            // Assert
            dto.Platform.Should().Be("Instagram");
            dto.Url.Should().Be("https://instagram.com/user");
            dto.Handle.Should().Be("@instauser");
        }
    }

    #endregion

    #region Integration/Scenario Tests

    public class AccountContactIntegrationTests
    {
        [Fact]
        public void AccountDto_WithContacts_SimulatesRealWorldScenario()
        {
            // Arrange - Create an organization account with contacts
            var account = new AccountDto
            {
                Id = 1,
                Category = "Organization",
                Company = "Enterprise Solutions Inc.",
                Email = "info@enterprise.com",
                Phone = "800-555-1000",
                Industry = "Technology",
                AccountType = "Enterprise",
                Priority = "High",
                AccountHealthScore = 85,
                Contacts = new List<AccountContactDto>
                {
                    new AccountContactDto
                    {
                        ContactId = 101,
                        ContactName = "Alice Executive",
                        ContactEmail = "alice@enterprise.com",
                        Role = "Executive",
                        IsPrimaryContact = true,
                        IsDecisionMaker = true
                    },
                    new AccountContactDto
                    {
                        ContactId = 102,
                        ContactName = "Bob Technical",
                        ContactEmail = "bob@enterprise.com",
                        Role = "Technical",
                        IsDecisionMaker = false,
                        ReceivesTechnicalUpdates = true
                    },
                    new AccountContactDto
                    {
                        ContactId = 103,
                        ContactName = "Carol Billing",
                        ContactEmail = "carol@enterprise.com",
                        Role = "Billing",
                        ReceivesBillingNotifications = true
                    }
                },
                ContactCount = 3
            };

            // Assert
            account.IsOrganization.Should().BeTrue();
            account.Contacts.Should().HaveCount(3);
            account.Contacts.Should().ContainSingle(c => c.IsPrimaryContact);
            account.Contacts.Should().ContainSingle(c => c.IsDecisionMaker);
            account.Contacts.Should().ContainSingle(c => c.ReceivesTechnicalUpdates);
            account.Contacts.Should().ContainSingle(c => c.ReceivesBillingNotifications);
        }

        [Fact]
        public void CreateAccountDto_ForIndividual_HasProperDefaults()
        {
            // Arrange
            var dto = new CreateAccountDto
            {
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Individual",
                Email = "john@personal.com",
                Phone = "555-1234"
            };

            // Assert
            dto.Category.Should().Be(AccountCategory.Individual);
            dto.AccountType.Should().Be(AccountType.Individual);
            dto.Priority.Should().Be(AccountPriority.Medium);
            dto.LifecycleStage.Should().Be(AccountLifecycleStage.Lead);
            dto.OptInEmail.Should().BeTrue();
            dto.OptInPhone.Should().BeTrue();
            dto.OptInSms.Should().BeFalse();
        }

        [Fact]
        public void ContactDto_WithNormalizedAddresses_SimulatesNewArchitecture()
        {
            // Arrange
            var contact = new ContactDto
            {
                Id = 1,
                FirstName = "Multi",
                LastName = "Address",
                // Legacy fields
                EmailPrimary = "legacy@example.com",
                PhonePrimary = "555-LEGACY",
                // Normalized collections
                EmailAddresses = new List<LinkedEmailDto>
                {
                    new LinkedEmailDto { Id = 1, Email = "work@example.com", EmailType = "Work", IsPrimary = true },
                    new LinkedEmailDto { Id = 2, Email = "personal@example.com", EmailType = "Personal", IsPrimary = false }
                },
                PhoneNumbers = new List<LinkedPhoneDto>
                {
                    new LinkedPhoneDto { Id = 1, Number = "555-WORK", PhoneType = "Work", IsPrimary = true },
                    new LinkedPhoneDto { Id = 2, Number = "555-MOBILE", PhoneType = "Mobile", IsPrimary = false }
                },
                Addresses = new List<LinkedAddressDto>
                {
                    new LinkedAddressDto { Id = 1, City = "Seattle", AddressType = "Work", IsPrimary = true },
                    new LinkedAddressDto { Id = 2, City = "Portland", AddressType = "Home", IsPrimary = false }
                }
            };

            // Assert - Both legacy and normalized data co-exist during migration
            contact.EmailPrimary.Should().Be("legacy@example.com");
            contact.EmailAddresses.Should().HaveCount(2);
            contact.EmailAddresses.Should().ContainSingle(e => e.IsPrimary);
            contact.PhoneNumbers.Should().HaveCount(2);
            contact.Addresses.Should().HaveCount(2);
        }
    }

    #endregion
}
