// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class ContactInfoControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ContactInfoControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_ContactInfo_Succeeds()
        {
            var create = new
            {
                Label = "Test",
                Line1 = "Test",
                Line2 = "Test",
                Line3 = "Test",
                City = "Test",
                State = "Test",
                PostalCode = "Test",
                County = "Test",
                CountryCode = "Test",
                Country = "Test",
                ZipCodeId = 1,
                LocalityId = 1,
                Locality = "Test",
                AddressXml = "Test",
                Latitude = 1,
                Longitude = 1,
                GeocodeAccuracy = "Test",
                IsVerified = true,
                VerifiedDate = DateTime.UtcNow,
                VerificationSource = "Test",
                IsResidential = true,
                DeliveryInstructions = "Test",
                AccessHours = "Test",
                SiteContactName = "Test",
                SiteContactPhone = "Test",
                Notes = "Test",
                FormattedAddress = "Test",
                LinkId = 1,
                AddressType = "Test",
                IsPrimary = true,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow,
                IsActive = true,
                LinkNotes = "Test",
                AddressId = 1,
                NewAddress = (object?)null,
                EntityType = "Test",
                EntityId = 1,
                AreaCode = "Test",
                Number = "Test",
                Extension = "Test",
                FormattedNumber = "Test",
                CanSMS = true,
                CanWhatsApp = true,
                CanFax = true,
                BestTimeToCall = "Test",
                FullNumber = "Test",
                PhoneType = "Test",
                DoNotCall = true,
                PhoneId = 1,
                NewPhone = (object?)null,
                Email = "Test",
                BounceCount = 1,
                LastBounceDate = DateTime.UtcNow,
                HardBounce = true,
                LastEmailSent = DateTime.UtcNow,
                LastEmailOpened = DateTime.UtcNow,
                EmailEngagementScore = 1,
                IsDeliverable = true,
                EmailType = "Test",
                DoNotEmail = true,
                UnsubscribedDate = DateTime.UtcNow,
                MarketingOptIn = true,
                TransactionalOnly = true,
                CanSendMarketing = true,
                EmailId = 1,
                NewEmail = (object?)null,
                Platform = "Test",
                PlatformOther = "Test",
                AccountType = "Test",
                HandleOrUsername = "Test",
                ProfileUrl = "Test",
                FollowerCount = 1,
                FollowingCount = 1,
                IsVerifiedAccount = true,
                LastActivityDate = DateTime.UtcNow,
                EngagementLevel = "Test",
                PlatformName = "Test",
                PreferredForContact = true,
                SocialMediaAccountId = 1,
                NewSocialMedia = (object?)null,
                EntityName = "Test",
                SourceEntityType = "Test",
                SourceEntityId = 1,
                TargetEntityType = "Test",
                TargetEntityId = 1,
                DefaultAddressType = "Test",
                DefaultPhoneType = "Test",
                DefaultEmailType = "Test",
                SetAsPrimary = true,
                Name = "Test",
                AlternateName = "Test",
                LocalityType = "Test",
                StateCode = "Test",
                IsUserCreated = true,
                FollowedByUserId = 1,
                FollowedByUserName = "Test",
                FollowedAt = DateTime.UtcNow,
                NotifyOnActivity = true,
                NotificationFrequency = "Test",
                LastNotifiedAt = DateTime.UtcNow,
                PhoneNumber = "Test",
                SocialMediaHandle = "Test",
                SocialMediaPlatform = "Test",
                CheckMxRecords = true,
                IsValid = true,
                Message = "Test",
                ErrorMessage = "Test",
                SuggestedCorrection = "Test",
                FormattedValue = "Test",
                NormalizedValue = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/contactinfo", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/contactinfo/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Label = "Test2",
                Line1 = "Test",
                Line2 = "Test",
                Line3 = "Test",
                City = "Test",
                State = "Test",
                PostalCode = "Test",
                County = "Test",
                CountryCode = "Test",
                Country = "Test",
                ZipCodeId = 1,
                LocalityId = 1,
                Locality = "Test",
                AddressXml = "Test",
                Latitude = 1,
                Longitude = 1,
                GeocodeAccuracy = "Test",
                IsVerified = true,
                VerifiedDate = DateTime.UtcNow,
                VerificationSource = "Test",
                IsResidential = true,
                DeliveryInstructions = "Test",
                AccessHours = "Test",
                SiteContactName = "Test",
                SiteContactPhone = "Test",
                Notes = "Test",
                FormattedAddress = "Test",
                LinkId = 1,
                AddressType = "Test",
                IsPrimary = true,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow,
                IsActive = true,
                LinkNotes = "Test",
                AddressId = 1,
                NewAddress = (object?)null,
                EntityType = "Test",
                EntityId = 1,
                AreaCode = "Test",
                Number = "Test",
                Extension = "Test",
                FormattedNumber = "Test",
                CanSMS = true,
                CanWhatsApp = true,
                CanFax = true,
                BestTimeToCall = "Test",
                FullNumber = "Test",
                PhoneType = "Test",
                DoNotCall = true,
                PhoneId = 1,
                NewPhone = (object?)null,
                Email = "Test",
                BounceCount = 1,
                LastBounceDate = DateTime.UtcNow,
                HardBounce = true,
                LastEmailSent = DateTime.UtcNow,
                LastEmailOpened = DateTime.UtcNow,
                EmailEngagementScore = 1,
                IsDeliverable = true,
                EmailType = "Test",
                DoNotEmail = true,
                UnsubscribedDate = DateTime.UtcNow,
                MarketingOptIn = true,
                TransactionalOnly = true,
                CanSendMarketing = true,
                EmailId = 1,
                NewEmail = (object?)null,
                Platform = "Test",
                PlatformOther = "Test",
                AccountType = "Test",
                HandleOrUsername = "Test",
                ProfileUrl = "Test",
                FollowerCount = 1,
                FollowingCount = 1,
                IsVerifiedAccount = true,
                LastActivityDate = DateTime.UtcNow,
                EngagementLevel = "Test",
                PlatformName = "Test",
                PreferredForContact = true,
                SocialMediaAccountId = 1,
                NewSocialMedia = (object?)null,
                EntityName = "Test",
                SourceEntityType = "Test",
                SourceEntityId = 1,
                TargetEntityType = "Test",
                TargetEntityId = 1,
                DefaultAddressType = "Test",
                DefaultPhoneType = "Test",
                DefaultEmailType = "Test",
                SetAsPrimary = true,
                Name = "Test",
                AlternateName = "Test",
                LocalityType = "Test",
                StateCode = "Test",
                IsUserCreated = true,
                FollowedByUserId = 1,
                FollowedByUserName = "Test",
                FollowedAt = DateTime.UtcNow,
                NotifyOnActivity = true,
                NotificationFrequency = "Test",
                LastNotifiedAt = DateTime.UtcNow,
                PhoneNumber = "Test",
                SocialMediaHandle = "Test",
                SocialMediaPlatform = "Test",
                CheckMxRecords = true,
                IsValid = true,
                Message = "Test",
                ErrorMessage = "Test",
                SuggestedCorrection = "Test",
                FormattedValue = "Test",
                NormalizedValue = "Test"
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/contactinfo/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/contactinfo/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/contactinfo/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/contactinfo/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
