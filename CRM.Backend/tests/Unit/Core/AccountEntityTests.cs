// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Account entity and related enums.
/// Tests cover enums, defaults, computed properties, validation attributes, and scenarios.
/// </summary>
public class AccountEntityTests
{
    #region AccountCategory Enum Tests

    public class AccountCategoryTests
    {
        [Theory]
        [InlineData(AccountCategory.Individual, 0)]
        [InlineData(AccountCategory.Organization, 1)]
        public void AccountCategory_ShouldHaveCorrectValue(AccountCategory category, int expected)
        {
            ((int)category).Should().Be(expected);
        }

        [Fact]
        public void AccountCategory_AllValues_ShouldHaveTwoCategories()
        {
            var values = Enum.GetValues<AccountCategory>();
            values.Should().HaveCount(2);
        }

        [Fact]
        public void AccountCategory_ShouldParseFromString()
        {
            Enum.TryParse<AccountCategory>("Organization", out var result).Should().BeTrue();
            result.Should().Be(AccountCategory.Organization);
        }
    }

    #endregion

    #region AccountLifecycleStage Enum Tests

    public class AccountLifecycleStageTests
    {
        [Theory]
        [InlineData(AccountLifecycleStage.Other, 0)]
        [InlineData(AccountLifecycleStage.Lead, 1)]
        [InlineData(AccountLifecycleStage.Opportunity, 2)]
        [InlineData(AccountLifecycleStage.Active, 3)]
        [InlineData(AccountLifecycleStage.AtRisk, 4)]
        [InlineData(AccountLifecycleStage.Churned, 5)]
        [InlineData(AccountLifecycleStage.WinBack, 6)]
        public void AccountLifecycleStage_ShouldHaveCorrectValue(AccountLifecycleStage stage, int expected)
        {
            ((int)stage).Should().Be(expected);
        }

        [Fact]
        public void AccountLifecycleStage_AllValues_ShouldHaveSevenStages()
        {
            var values = Enum.GetValues<AccountLifecycleStage>();
            values.Should().HaveCount(7);
        }

        [Fact]
        public void AccountLifecycleStage_ShouldRepresentFullLifecycle()
        {
            // Lifecycle flow: Other → Lead → Opportunity → Active → AtRisk → Churned → WinBack → Lead
            ((int)AccountLifecycleStage.Other).Should().BeLessThan((int)AccountLifecycleStage.Lead);
            ((int)AccountLifecycleStage.Lead).Should().BeLessThan((int)AccountLifecycleStage.Opportunity);
            ((int)AccountLifecycleStage.Opportunity).Should().BeLessThan((int)AccountLifecycleStage.Active);
            ((int)AccountLifecycleStage.Active).Should().BeLessThan((int)AccountLifecycleStage.AtRisk);
            ((int)AccountLifecycleStage.AtRisk).Should().BeLessThan((int)AccountLifecycleStage.Churned);
            ((int)AccountLifecycleStage.Churned).Should().BeLessThan((int)AccountLifecycleStage.WinBack);
        }
    }

    #endregion

    #region AccountType Enum Tests

    public class AccountTypeTests
    {
        [Theory]
        [InlineData(AccountType.Individual, 0)]
        [InlineData(AccountType.SmallBusiness, 1)]
        [InlineData(AccountType.MidMarket, 2)]
        [InlineData(AccountType.Enterprise, 3)]
        [InlineData(AccountType.Government, 4)]
        [InlineData(AccountType.NonProfit, 5)]
        public void AccountType_ShouldHaveCorrectValue(AccountType type, int expected)
        {
            ((int)type).Should().Be(expected);
        }

        [Fact]
        public void AccountType_AllValues_ShouldHaveSixTypes()
        {
            var values = Enum.GetValues<AccountType>();
            values.Should().HaveCount(6);
        }

        [Fact]
        public void AccountType_ShouldCoverBusinessSizes()
        {
            // Business-related types
            Enum.IsDefined(typeof(AccountType), AccountType.SmallBusiness).Should().BeTrue();
            Enum.IsDefined(typeof(AccountType), AccountType.MidMarket).Should().BeTrue();
            Enum.IsDefined(typeof(AccountType), AccountType.Enterprise).Should().BeTrue();
        }
    }

    #endregion

    #region AccountPriority Enum Tests

    public class AccountPriorityTests
    {
        [Theory]
        [InlineData(AccountPriority.Low, 0)]
        [InlineData(AccountPriority.Medium, 1)]
        [InlineData(AccountPriority.High, 2)]
        [InlineData(AccountPriority.Critical, 3)]
        public void AccountPriority_ShouldHaveCorrectValue(AccountPriority priority, int expected)
        {
            ((int)priority).Should().Be(expected);
        }

        [Fact]
        public void AccountPriority_AllValues_ShouldHaveFourLevels()
        {
            var values = Enum.GetValues<AccountPriority>();
            values.Should().HaveCount(4);
        }

        [Fact]
        public void AccountPriority_ShouldBeOrderedByImportance()
        {
            ((int)AccountPriority.Low).Should().BeLessThan((int)AccountPriority.Medium);
            ((int)AccountPriority.Medium).Should().BeLessThan((int)AccountPriority.High);
            ((int)AccountPriority.High).Should().BeLessThan((int)AccountPriority.Critical);
        }
    }

    #endregion

    #region Account Default Values Tests

    public class AccountDefaultValuesTests
    {
        [Fact]
        public void Account_DefaultValues_CategoryShouldBeCorrect()
        {
            var account = new Account();

            account.Category.Should().Be(AccountCategory.Individual);
        }

        [Fact]
        public void Account_DefaultValues_IndividualFieldsShouldBeCorrect()
        {
            var account = new Account();

            account.FirstName.Should().BeEmpty();
            account.LastName.Should().BeEmpty();
            account.Salutation.Should().BeNull();
            account.Suffix.Should().BeNull();
            account.DateOfBirth.Should().BeNull();
            account.Gender.Should().BeNull();
            account.LinkedContactId.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_OrganizationFieldsShouldBeCorrect()
        {
            var account = new Account();

            account.Company.Should().BeEmpty();
            account.LegalName.Should().BeNull();
            account.DbaName.Should().BeNull();
            account.TaxId.Should().BeNull();
            account.RegistrationNumber.Should().BeNull();
            account.YearFounded.Should().BeNull();
            account.PrimaryContactId.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_ContactInfoShouldBeCorrect()
        {
            var account = new Account();

            account.Email.Should().BeEmpty();
            account.SecondaryEmail.Should().BeNull();
            account.Phone.Should().BeEmpty();
            account.MobilePhone.Should().BeNull();
            account.FaxNumber.Should().BeNull();
            account.JobTitle.Should().BeNull();
            account.Website.Should().BeNull();
        }

        // DISABLED: Address fields were removed and normalized via EntityAddressLink. See address normalization phase.
        /*
        [Fact]
        public void Account_DefaultValues_BillingAddressShouldBeCorrect()
        {
            var account = new Account();

            account.Address.Should().BeEmpty();
            account.Address2.Should().BeNull();
            account.City.Should().BeEmpty();
            account.State.Should().BeEmpty();
            account.ZipCode.Should().BeEmpty();
            account.Country.Should().BeEmpty();
        }

        [Fact]
        public void Account_DefaultValues_ShippingAddressShouldBeCorrect()
        {
            var account = new Account();

            account.ShippingAddress.Should().BeNull();
            account.ShippingAddress2.Should().BeNull();
            account.ShippingCity.Should().BeNull();
            account.ShippingState.Should().BeNull();
            account.ShippingZipCode.Should().BeNull();
            account.ShippingCountry.Should().BeNull();
            account.ShippingSameAsBilling.Should().BeTrue();
        }
        */

        [Fact]
        public void Account_DefaultValues_BusinessInfoShouldBeCorrect()
        {
            var account = new Account();

            account.Industry.Should().BeNull();
            account.SubIndustry.Should().BeNull();
            account.NumberOfEmployees.Should().BeNull();
            account.EmployeeRange.Should().BeNull();
            account.AnnualRevenue.Should().Be(0);
            account.RevenueRange.Should().BeNull();
            account.AccountType.Should().Be(AccountType.Individual);
            account.Priority.Should().Be(AccountPriority.Medium);
            account.StockSymbol.Should().BeNull();
            account.Ownership.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_LifecycleShouldBeCorrect()
        {
            var account = new Account();

            account.LifecycleStage.Should().Be(AccountLifecycleStage.Other);
            account.LeadSource.Should().BeNull();
            account.FirstContactDate.Should().BeNull();
            account.ConversionDate.Should().BeNull();
            account.LastActivityDate.Should().BeNull();
            account.NextFollowUpDate.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_FinancialShouldBeCorrect()
        {
            var account = new Account();

            account.TotalPurchases.Should().Be(0);
            account.AccountBalance.Should().Be(0);
            account.CreditLimit.Should().Be(0);
            account.PaymentTerms.Should().BeNull();
            account.PreferredPaymentMethod.Should().BeNull();
            account.Currency.Should().BeNull();
            account.CurrencyLookupId.Should().BeNull();
            account.BillingCycle.Should().BeNull();
            account.BillingCycleLookupId.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_ScoringShouldBeCorrect()
        {
            var account = new Account();

            account.LeadScore.Should().Be(0);
            account.AccountHealthScore.Should().Be(50);
            account.NpsScore.Should().Be(0);
            account.SatisfactionRating.Should().Be(0);
        }

        // DISABLED: Preference fields were moved to Preferences entity
        /*
        [Fact]
        public void Account_DefaultValues_SocialAndPreferencesShouldBeCorrect()
        {
            var account = new Account();

            account.LinkedInUrl.Should().BeNull();
            account.TwitterHandle.Should().BeNull();
            account.FacebookUrl.Should().BeNull();
            account.OptInEmail.Should().BeTrue();
            account.OptInSms.Should().BeFalse();
            account.OptInPhone.Should().BeTrue();
            account.PreferredContactMethod.Should().BeNull();
            account.PreferredContactTime.Should().BeNull();
            account.Timezone.Should().BeNull();
            account.PreferredLanguage.Should().BeNull();
        }
        */

        [Fact]
        public void Account_DefaultValues_AssignmentShouldBeCorrect()
        {
            var account = new Account();

            account.AssignedToUserId.Should().BeNull();
            account.AccountManagerId.Should().BeNull();
            account.Territory.Should().BeNull();
            account.Region.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_ClassificationShouldBeCorrect()
        {
            var account = new Account();

            account.Tags.Should().BeNull();
            account.Segment.Should().BeNull();
            account.ReferralSource.Should().BeNull();
            account.ReferredByAccountId.Should().BeNull();
            account.ParentAccountId.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_LeadConversionShouldBeCorrect()
        {
            var account = new Account();

            account.ConvertedFromLeadId.Should().BeNull();
            account.SourceCampaignId.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_DocumentationShouldBeCorrect()
        {
            var account = new Account();

            account.Notes.Should().BeEmpty();
            account.InternalNotes.Should().BeNull();
            account.Description.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_BrandingShouldBeCorrect()
        {
            var account = new Account();

            account.LogoUrl.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_CustomFieldsShouldBeCorrect()
        {
            var account = new Account();

            account.CustomFields.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_MergeTrackingShouldBeCorrect()
        {
            var account = new Account();

            account.MergedIntoId.Should().BeNull();
            account.MergeGroupId.Should().BeNull();
            account.IsMergedDuplicate.Should().BeFalse();
            account.MergedAt.Should().BeNull();
        }

        [Fact]
        public void Account_DefaultValues_NavigationPropertiesShouldBeNull()
        {
            var account = new Account();

            account.Opportunities.Should().BeNull();
            account.Interactions.Should().BeNull();
            account.AccountContacts.Should().BeNull();
            account.Subscriptions.Should().BeNull();
            account.Contacts.Should().BeNull();
            account.ContactInfoLinks.Should().BeNull();
            account.CurrencyLookup.Should().BeNull();
            account.BillingCycleLookup.Should().BeNull();
            account.AssignedToUser.Should().BeNull();
            account.AccountManager.Should().BeNull();
            account.ReferredByAccount.Should().BeNull();
            account.ParentAccount.Should().BeNull();
            account.ConvertedFromLead.Should().BeNull();
            account.SourceCampaign.Should().BeNull();
        }
    }

    #endregion

    #region Account Computed Properties Tests

    public class AccountComputedPropertiesTests
    {
        [Fact]
        public void Account_DisplayName_ForIndividual_ShouldReturnFullName()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Doe",
                Company = "Some Company"
            };

            account.DisplayName.Should().Be("John Doe");
        }

        [Fact]
        public void Account_DisplayName_ForOrganization_ShouldReturnCompanyName()
        {
            var account = new Account
            {
                Category = AccountCategory.Organization,
                FirstName = "John",
                LastName = "Doe",
                Company = "Acme Corporation"
            };

            account.DisplayName.Should().Be("Acme Corporation");
        }

        [Fact]
        public void Account_DisplayName_ForIndividualWithOnlyFirstName_ShouldTrim()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = ""
            };

            account.DisplayName.Should().Be("John");
        }

        [Fact]
        public void Account_DisplayName_ForIndividualWithOnlyLastName_ShouldTrim()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "",
                LastName = "Doe"
            };

            account.DisplayName.Should().Be("Doe");
        }

        [Fact]
        public void Account_DisplayName_ForIndividualWithNoName_ShouldReturnEmpty()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "",
                LastName = ""
            };

            account.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Account_DisplayName_ForOrganizationWithEmptyCompany_ShouldReturnEmpty()
        {
            var account = new Account
            {
                Category = AccountCategory.Organization,
                Company = ""
            };

            account.DisplayName.Should().BeEmpty();
        }
    }

    #endregion

    #region Account Inheritance Tests

    public class AccountInheritanceTests
    {
        [Fact]
        public void Account_InheritsFromBaseEntity_ShouldHaveBaseProperties()
        {
            var now = DateTime.UtcNow;
            var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            var account = new Account
            {
                Id = 42,
                CreatedAt = now,
                UpdatedAt = now.AddHours(1),
                IsDeleted = true,
                RowVersion = rowVersion
            };

            account.Id.Should().Be(42);
            account.CreatedAt.Should().Be(now);
            account.UpdatedAt.Should().Be(now.AddHours(1));
            account.IsDeleted.Should().BeTrue();
            account.RowVersion.Should().Equal(rowVersion);
        }
    }

    #endregion

    #region Account Scenario Tests - Individual Account

    public class IndividualAccountScenarioTests
    {
        [Fact]
        public void Account_IndividualAccount_ShouldBeConfiguredCorrectly()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "Jane",
                LastName = "Smith",
                Salutation = "Ms.",
                DateOfBirth = new DateTime(1985, 6, 15),
                Gender = "Female",
                Email = "jane.smith@example.com",
                Phone = "+1-555-123-4567",
                MobilePhone = "+1-555-987-6543",
                JobTitle = "Senior Consultant",
                AccountType = AccountType.Individual,
                LifecycleStage = AccountLifecycleStage.Active,
                LeadScore = 85,
                AccountHealthScore = 90
            };

            account.Category.Should().Be(AccountCategory.Individual);
            account.DisplayName.Should().Be("Jane Smith");
            account.DateOfBirth.Should().NotBeNull();
            account.AccountType.Should().Be(AccountType.Individual);
            account.LifecycleStage.Should().Be(AccountLifecycleStage.Active);
        }

        [Fact]
        public void Account_IndividualWithLinkedContact_ShouldHaveContactId()
        {
            var account = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Doe",
                LinkedContactId = 100
            };

            account.LinkedContactId.Should().Be(100);
        }
    }

    #endregion

    #region Account Scenario Tests - Organization Account

    public class OrganizationAccountScenarioTests
    {
        [Fact]
        public void Account_OrganizationAccount_ShouldBeConfiguredCorrectly()
        {
            var account = new Account
            {
                Category = AccountCategory.Organization,
                Company = "Acme Corporation",
                LegalName = "Acme Corporation Inc.",
                DbaName = "Acme Corp",
                TaxId = "12-3456789",
                RegistrationNumber = "REG-2020-001",
                YearFounded = 1995,
                Email = "info@acme.com",
                Phone = "+1-555-000-0000",
                Website = "https://www.acme.com",
                Industry = "Technology",
                SubIndustry = "Software Development",
                NumberOfEmployees = 500,
                EmployeeRange = "201-500",
                AnnualRevenue = 50000000,
                RevenueRange = "$10M-$100M",
                AccountType = AccountType.Enterprise,
                LifecycleStage = AccountLifecycleStage.Active,
                StockSymbol = "ACME",
                Ownership = "Public"
            };

            account.Category.Should().Be(AccountCategory.Organization);
            account.DisplayName.Should().Be("Acme Corporation");
            account.AccountType.Should().Be(AccountType.Enterprise);
            account.AnnualRevenue.Should().Be(50000000);
        }

        [Fact]
        public void Account_OrganizationWithPrimaryContact_ShouldHaveContactId()
        {
            var account = new Account
            {
                Category = AccountCategory.Organization,
                Company = "Test Company",
                PrimaryContactId = 200
            };

            account.PrimaryContactId.Should().Be(200);
        }

        [Fact]
        public void Account_OrganizationWithParent_ShouldSupportHierarchy()
        {
            var parentAccount = new Account
            {
                Id = 1,
                Category = AccountCategory.Organization,
                Company = "Parent Corp"
            };

            var subsidiaryAccount = new Account
            {
                Id = 2,
                Category = AccountCategory.Organization,
                Company = "Subsidiary Inc",
                ParentAccountId = 1
            };

            subsidiaryAccount.ParentAccountId.Should().Be(1);
        }
    }

    #endregion

    #region Account Scenario Tests - Financial

    public class AccountFinancialScenarioTests
    {
        [Fact]
        public void Account_FinancialSetup_ShouldBeConfiguredCorrectly()
        {
            var account = new Account
            {
                TotalPurchases = 150000,
                AccountBalance = 5000,
                CreditLimit = 50000,
                PaymentTerms = "Net 30",
                PreferredPaymentMethod = "Wire Transfer",
                Currency = "USD",
                BillingCycle = "Monthly"
            };

            account.TotalPurchases.Should().Be(150000);
            account.AccountBalance.Should().Be(5000);
            account.CreditLimit.Should().Be(50000);
            account.PaymentTerms.Should().Be("Net 30");
            account.Currency.Should().Be("USD");
        }

        [Fact]
        public void Account_NegativeBalance_ShouldBeAllowed()
        {
            var account = new Account
            {
                AccountBalance = -1500
            };

            account.AccountBalance.Should().Be(-1500);
        }
    }

    #endregion

    #region Account Scenario Tests - Scoring

    public class AccountScoringScenarioTests
    {
        [Fact]
        public void Account_HealthyScores_ShouldBeInRange()
        {
            var account = new Account
            {
                LeadScore = 85,
                AccountHealthScore = 90,
                NpsScore = 75,
                SatisfactionRating = 4.5
            };

            account.LeadScore.Should().BeInRange(0, 100);
            account.AccountHealthScore.Should().BeInRange(0, 100);
            account.NpsScore.Should().BeInRange(-100, 100);
            account.SatisfactionRating.Should().BeInRange(0, 5);
        }

        [Fact]
        public void Account_NegativeNpsScore_ShouldBeAllowed()
        {
            var account = new Account
            {
                NpsScore = -50
            };

            account.NpsScore.Should().Be(-50);
        }
    }

    #endregion

    #region Account Scenario Tests - Lifecycle

    public class AccountLifecycleScenarioTests
    {
        [Fact]
        public void Account_LifecycleProgression_ShouldTrackDates()
        {
            var firstContact = new DateTime(2024, 1, 1);
            var conversion = new DateTime(2024, 2, 15);
            var lastActivity = new DateTime(2024, 3, 20);
            var nextFollowUp = new DateTime(2024, 4, 1);

            var account = new Account
            {
                LifecycleStage = AccountLifecycleStage.Active,
                LeadSource = "Website",
                FirstContactDate = firstContact,
                ConversionDate = conversion,
                LastActivityDate = lastActivity,
                NextFollowUpDate = nextFollowUp
            };

            account.FirstContactDate.Should().Be(firstContact);
            account.ConversionDate.Should().BeAfter(firstContact);
            account.LastActivityDate.Should().BeAfter(conversion);
            account.NextFollowUpDate.Should().BeAfter(lastActivity);
        }

        [Fact]
        public void Account_AtRiskAccount_ShouldIndicateChurnRisk()
        {
            var account = new Account
            {
                LifecycleStage = AccountLifecycleStage.AtRisk,
                AccountHealthScore = 30,
                NpsScore = -20
            };

            account.LifecycleStage.Should().Be(AccountLifecycleStage.AtRisk);
            account.AccountHealthScore.Should().BeLessThan(50);
            account.NpsScore.Should().BeNegative();
        }
    }

    #endregion

    #region Account Scenario Tests - Merge Tracking

    public class AccountMergeTrackingTests
    {
        [Fact]
        public void Account_MergedDuplicate_ShouldBeTracked()
        {
            var mergedAt = DateTime.UtcNow;

            var mergedAccount = new Account
            {
                Id = 100,
                MergedIntoId = 1,
                MergeGroupId = 50,
                IsMergedDuplicate = true,
                MergedAt = mergedAt,
                IsDeleted = true
            };

            mergedAccount.MergedIntoId.Should().Be(1);
            mergedAccount.MergeGroupId.Should().Be(50);
            mergedAccount.IsMergedDuplicate.Should().BeTrue();
            mergedAccount.MergedAt.Should().Be(mergedAt);
            mergedAccount.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Account_NotMerged_ShouldHaveNullMergeFields()
        {
            var account = new Account();

            account.MergedIntoId.Should().BeNull();
            account.MergeGroupId.Should().BeNull();
            account.IsMergedDuplicate.Should().BeFalse();
            account.MergedAt.Should().BeNull();
        }
    }

    #endregion

    #region Account Scenario Tests - Shipping Address
    // DISABLED: Shipping address fields were removed and normalized via Address entity (EntityAddressLink pattern)
    /*
    public class AccountShippingAddressTests
    {
        [Fact]
        public void Account_ShippingSameAsBilling_ShouldDefaultToTrue()
        {
            var account = new Account();

            account.ShippingSameAsBilling.Should().BeTrue();
        }

        [Fact]
        public void Account_DifferentShippingAddress_ShouldBeConfigurable()
        {
            var account = new Account
            {
                Address = "123 Billing St",
                City = "Billing City",
                State = "BC",
                ZipCode = "11111",
                Country = "USA",
                ShippingSameAsBilling = false,
                ShippingAddress = "456 Shipping Ave",
                ShippingCity = "Shipping City",
                ShippingState = "SC",
                ShippingZipCode = "22222",
                ShippingCountry = "USA"
            };

            account.ShippingSameAsBilling.Should().BeFalse();
            account.ShippingAddress.Should().NotBe(account.Address);
            account.ShippingCity.Should().NotBe(account.City);
        }
    }
    */
    #endregion

    #region Account Scenario Tests - Lead Conversion

    public class AccountLeadConversionTests
    {
        [Fact]
        public void Account_ConvertedFromLead_ShouldTrackSource()
        {
            var account = new Account
            {
                ConvertedFromLeadId = 500,
                SourceCampaignId = 100,
                LeadSource = "Trade Show",
                ConversionDate = DateTime.UtcNow
            };

            account.ConvertedFromLeadId.Should().Be(500);
            account.SourceCampaignId.Should().Be(100);
            account.LeadSource.Should().Be("Trade Show");
            account.ConversionDate.Should().NotBeNull();
        }
    }

    #endregion

    #region Account Scenario Tests - Communication Preferences

    public class AccountCommunicationPreferencesTests
    {
        // DISABLED: OptIn and communication preference fields moved to Preferences entity
        // All test methods are disabled
        /*
        [Fact]
        public void Account_OptInDefaults_ShouldBeReasonable()
        {
            var account = new Account();

            // Email and phone opt-in by default, SMS opt-out
            account.OptInEmail.Should().BeTrue();
            account.OptInPhone.Should().BeTrue();
            account.OptInSms.Should().BeFalse();
        }

        [Fact]
        public void Account_AllOptOut_ShouldBeConfigurable()
        {
            var account = new Account
            {
                OptInEmail = false,
                OptInPhone = false,
                OptInSms = false
            };

            account.OptInEmail.Should().BeFalse();
            account.OptInPhone.Should().BeFalse();
            account.OptInSms.Should().BeFalse();
        }

        [Fact]
        public void Account_CommunicationPreferences_ShouldBeConfigurable()
        {
            var account = new Account
            {
                PreferredContactMethod = "Email",
                PreferredContactTime = "Morning",
                Timezone = "America/New_York",
                PreferredLanguage = "en-US"
            };

            account.PreferredContactMethod.Should().Be("Email");
            account.PreferredContactTime.Should().Be("Morning");
            account.Timezone.Should().Be("America/New_York");
            account.PreferredLanguage.Should().Be("en-US");
        }
        */
    }

    #endregion

    #region Account Scenario Tests - Referral

    public class AccountReferralTests
    {
        [Fact]
        public void Account_ReferredByAnotherAccount_ShouldTrackSource()
        {
            var account = new Account
            {
                ReferralSource = "Partner Program",
                ReferredByAccountId = 999
            };

            account.ReferralSource.Should().Be("Partner Program");
            account.ReferredByAccountId.Should().Be(999);
        }
    }

    #endregion

}
