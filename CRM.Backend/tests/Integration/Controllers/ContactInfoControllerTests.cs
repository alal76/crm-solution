using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Label.Should().Be(create.Label);
            item.Line1.Should().Be(create.Line1);
            item.Line2.Should().Be(create.Line2);
            item.Line3.Should().Be(create.Line3);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.PostalCode.Should().Be(create.PostalCode);
            item.County.Should().Be(create.County);
            item.CountryCode.Should().Be(create.CountryCode);
            item.Country.Should().Be(create.Country);
            item.ZipCodeId.Should().Be(create.ZipCodeId);
            item.LocalityId.Should().Be(create.LocalityId);
            item.Locality.Should().Be(create.Locality);
            item.AddressXml.Should().Be(create.AddressXml);
            item.Latitude.Should().Be(create.Latitude);
            item.Longitude.Should().Be(create.Longitude);
            item.GeocodeAccuracy.Should().Be(create.GeocodeAccuracy);
            item.IsVerified.Should().Be(create.IsVerified);
            item.VerifiedDate.Should().Be(create.VerifiedDate);
            item.VerificationSource.Should().Be(create.VerificationSource);
            item.IsResidential.Should().Be(create.IsResidential);
            item.DeliveryInstructions.Should().Be(create.DeliveryInstructions);
            item.AccessHours.Should().Be(create.AccessHours);
            item.SiteContactName.Should().Be(create.SiteContactName);
            item.SiteContactPhone.Should().Be(create.SiteContactPhone);
            item.Notes.Should().Be(create.Notes);
            item.FormattedAddress.Should().Be(create.FormattedAddress);
            item.LinkId.Should().Be(create.LinkId);
            item.AddressType.Should().Be(create.AddressType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.IsActive.Should().Be(create.IsActive);
            item.LinkNotes.Should().Be(create.LinkNotes);
            item.Label.Should().Be(create.Label);
            item.Line1.Should().Be(create.Line1);
            item.Line2.Should().Be(create.Line2);
            item.Line3.Should().Be(create.Line3);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.PostalCode.Should().Be(create.PostalCode);
            item.County.Should().Be(create.County);
            item.CountryCode.Should().Be(create.CountryCode);
            item.Country.Should().Be(create.Country);
            item.ZipCodeId.Should().Be(create.ZipCodeId);
            item.LocalityId.Should().Be(create.LocalityId);
            item.Locality.Should().Be(create.Locality);
            item.Latitude.Should().Be(create.Latitude);
            item.Longitude.Should().Be(create.Longitude);
            item.IsResidential.Should().Be(create.IsResidential);
            item.DeliveryInstructions.Should().Be(create.DeliveryInstructions);
            item.AccessHours.Should().Be(create.AccessHours);
            item.SiteContactName.Should().Be(create.SiteContactName);
            item.SiteContactPhone.Should().Be(create.SiteContactPhone);
            item.Notes.Should().Be(create.Notes);
            item.Label.Should().Be(create.Label);
            item.Line1.Should().Be(create.Line1);
            item.Line2.Should().Be(create.Line2);
            item.Line3.Should().Be(create.Line3);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.PostalCode.Should().Be(create.PostalCode);
            item.County.Should().Be(create.County);
            item.CountryCode.Should().Be(create.CountryCode);
            item.Country.Should().Be(create.Country);
            item.ZipCodeId.Should().Be(create.ZipCodeId);
            item.LocalityId.Should().Be(create.LocalityId);
            item.Locality.Should().Be(create.Locality);
            item.Latitude.Should().Be(create.Latitude);
            item.Longitude.Should().Be(create.Longitude);
            item.GeocodeAccuracy.Should().Be(create.GeocodeAccuracy);
            item.IsVerified.Should().Be(create.IsVerified);
            item.VerificationSource.Should().Be(create.VerificationSource);
            item.IsResidential.Should().Be(create.IsResidential);
            item.DeliveryInstructions.Should().Be(create.DeliveryInstructions);
            item.AccessHours.Should().Be(create.AccessHours);
            item.SiteContactName.Should().Be(create.SiteContactName);
            item.SiteContactPhone.Should().Be(create.SiteContactPhone);
            item.Notes.Should().Be(create.Notes);
            item.AddressId.Should().Be(create.AddressId);
            item.NewAddress.Should().Be(create.NewAddress);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.AddressType.Should().Be(create.AddressType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.Notes.Should().Be(create.Notes);
            item.Label.Should().Be(create.Label);
            item.CountryCode.Should().Be(create.CountryCode);
            item.AreaCode.Should().Be(create.AreaCode);
            item.Number.Should().Be(create.Number);
            item.Extension.Should().Be(create.Extension);
            item.FormattedNumber.Should().Be(create.FormattedNumber);
            item.CanSMS.Should().Be(create.CanSMS);
            item.CanWhatsApp.Should().Be(create.CanWhatsApp);
            item.CanFax.Should().Be(create.CanFax);
            item.IsVerified.Should().Be(create.IsVerified);
            item.VerifiedDate.Should().Be(create.VerifiedDate);
            item.BestTimeToCall.Should().Be(create.BestTimeToCall);
            item.Notes.Should().Be(create.Notes);
            item.FullNumber.Should().Be(create.FullNumber);
            item.LinkId.Should().Be(create.LinkId);
            item.PhoneType.Should().Be(create.PhoneType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.DoNotCall.Should().Be(create.DoNotCall);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.IsActive.Should().Be(create.IsActive);
            item.LinkNotes.Should().Be(create.LinkNotes);
            item.Label.Should().Be(create.Label);
            item.CountryCode.Should().Be(create.CountryCode);
            item.AreaCode.Should().Be(create.AreaCode);
            item.Number.Should().Be(create.Number);
            item.Extension.Should().Be(create.Extension);
            item.CanSMS.Should().Be(create.CanSMS);
            item.CanWhatsApp.Should().Be(create.CanWhatsApp);
            item.CanFax.Should().Be(create.CanFax);
            item.BestTimeToCall.Should().Be(create.BestTimeToCall);
            item.Notes.Should().Be(create.Notes);
            item.PhoneId.Should().Be(create.PhoneId);
            item.NewPhone.Should().Be(create.NewPhone);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.PhoneType.Should().Be(create.PhoneType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.DoNotCall.Should().Be(create.DoNotCall);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.Notes.Should().Be(create.Notes);
            item.Label.Should().Be(create.Label);
            item.Email.Should().Be(create.Email);
            item.IsVerified.Should().Be(create.IsVerified);
            item.VerifiedDate.Should().Be(create.VerifiedDate);
            item.BounceCount.Should().Be(create.BounceCount);
            item.LastBounceDate.Should().Be(create.LastBounceDate);
            item.HardBounce.Should().Be(create.HardBounce);
            item.LastEmailSent.Should().Be(create.LastEmailSent);
            item.LastEmailOpened.Should().Be(create.LastEmailOpened);
            item.EmailEngagementScore.Should().Be(create.EmailEngagementScore);
            item.IsDeliverable.Should().Be(create.IsDeliverable);
            item.Notes.Should().Be(create.Notes);
            item.LinkId.Should().Be(create.LinkId);
            item.EmailType.Should().Be(create.EmailType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.DoNotEmail.Should().Be(create.DoNotEmail);
            item.UnsubscribedDate.Should().Be(create.UnsubscribedDate);
            item.MarketingOptIn.Should().Be(create.MarketingOptIn);
            item.TransactionalOnly.Should().Be(create.TransactionalOnly);
            item.CanSendMarketing.Should().Be(create.CanSendMarketing);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.IsActive.Should().Be(create.IsActive);
            item.LinkNotes.Should().Be(create.LinkNotes);
            item.Label.Should().Be(create.Label);
            item.Email.Should().Be(create.Email);
            item.Notes.Should().Be(create.Notes);
            item.EmailId.Should().Be(create.EmailId);
            item.NewEmail.Should().Be(create.NewEmail);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.EmailType.Should().Be(create.EmailType);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.DoNotEmail.Should().Be(create.DoNotEmail);
            item.MarketingOptIn.Should().Be(create.MarketingOptIn);
            item.TransactionalOnly.Should().Be(create.TransactionalOnly);
            item.ValidFrom.Should().Be(create.ValidFrom);
            item.ValidTo.Should().Be(create.ValidTo);
            item.Notes.Should().Be(create.Notes);
            item.Platform.Should().Be(create.Platform);
            item.PlatformOther.Should().Be(create.PlatformOther);
            item.AccountType.Should().Be(create.AccountType);
            item.HandleOrUsername.Should().Be(create.HandleOrUsername);
            item.ProfileUrl.Should().Be(create.ProfileUrl);
            item.FollowerCount.Should().Be(create.FollowerCount);
            item.FollowingCount.Should().Be(create.FollowingCount);
            item.IsVerifiedAccount.Should().Be(create.IsVerifiedAccount);
            item.IsActive.Should().Be(create.IsActive);
            item.LastActivityDate.Should().Be(create.LastActivityDate);
            item.EngagementLevel.Should().Be(create.EngagementLevel);
            item.PlatformName.Should().Be(create.PlatformName);
            item.Notes.Should().Be(create.Notes);
            item.LinkId.Should().Be(create.LinkId);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.PreferredForContact.Should().Be(create.PreferredForContact);
            item.LinkNotes.Should().Be(create.LinkNotes);
            item.Platform.Should().Be(create.Platform);
            item.PlatformOther.Should().Be(create.PlatformOther);
            item.AccountType.Should().Be(create.AccountType);
            item.HandleOrUsername.Should().Be(create.HandleOrUsername);
            item.ProfileUrl.Should().Be(create.ProfileUrl);
            item.FollowerCount.Should().Be(create.FollowerCount);
            item.FollowingCount.Should().Be(create.FollowingCount);
            item.IsVerifiedAccount.Should().Be(create.IsVerifiedAccount);
            item.Notes.Should().Be(create.Notes);
            item.SocialMediaAccountId.Should().Be(create.SocialMediaAccountId);
            item.NewSocialMedia.Should().Be(create.NewSocialMedia);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.IsPrimary.Should().Be(create.IsPrimary);
            item.PreferredForContact.Should().Be(create.PreferredForContact);
            item.Notes.Should().Be(create.Notes);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.EntityName.Should().Be(create.EntityName);
            item.SourceEntityType.Should().Be(create.SourceEntityType);
            item.SourceEntityId.Should().Be(create.SourceEntityId);
            item.TargetEntityType.Should().Be(create.TargetEntityType);
            item.TargetEntityId.Should().Be(create.TargetEntityId);
            item.DefaultAddressType.Should().Be(create.DefaultAddressType);
            item.DefaultPhoneType.Should().Be(create.DefaultPhoneType);
            item.DefaultEmailType.Should().Be(create.DefaultEmailType);
            item.SetAsPrimary.Should().Be(create.SetAsPrimary);
            item.Name.Should().Be(create.Name);
            item.AlternateName.Should().Be(create.AlternateName);
            item.LocalityType.Should().Be(create.LocalityType);
            item.ZipCodeId.Should().Be(create.ZipCodeId);
            item.City.Should().Be(create.City);
            item.StateCode.Should().Be(create.StateCode);
            item.CountryCode.Should().Be(create.CountryCode);
            item.Latitude.Should().Be(create.Latitude);
            item.Longitude.Should().Be(create.Longitude);
            item.IsUserCreated.Should().Be(create.IsUserCreated);
            item.IsActive.Should().Be(create.IsActive);
            item.Name.Should().Be(create.Name);
            item.AlternateName.Should().Be(create.AlternateName);
            item.LocalityType.Should().Be(create.LocalityType);
            item.ZipCodeId.Should().Be(create.ZipCodeId);
            item.City.Should().Be(create.City);
            item.StateCode.Should().Be(create.StateCode);
            item.CountryCode.Should().Be(create.CountryCode);
            item.Latitude.Should().Be(create.Latitude);
            item.Longitude.Should().Be(create.Longitude);
            item.SocialMediaAccountId.Should().Be(create.SocialMediaAccountId);
            item.FollowedByUserId.Should().Be(create.FollowedByUserId);
            item.FollowedByUserName.Should().Be(create.FollowedByUserName);
            item.EntityType.Should().Be(create.EntityType);
            item.EntityId.Should().Be(create.EntityId);
            item.EntityName.Should().Be(create.EntityName);
            item.FollowedAt.Should().Be(create.FollowedAt);
            item.IsActive.Should().Be(create.IsActive);
            item.NotifyOnActivity.Should().Be(create.NotifyOnActivity);
            item.NotificationFrequency.Should().Be(create.NotificationFrequency);
            item.LastNotifiedAt.Should().Be(create.LastNotifiedAt);
            item.Notes.Should().Be(create.Notes);
            item.Platform.Should().Be(create.Platform);
            item.HandleOrUsername.Should().Be(create.HandleOrUsername);
            item.ProfileUrl.Should().Be(create.ProfileUrl);
            item.SocialMediaAccountId.Should().Be(create.SocialMediaAccountId);
            item.NotifyOnActivity.Should().Be(create.NotifyOnActivity);
            item.NotificationFrequency.Should().Be(create.NotificationFrequency);
            item.Notes.Should().Be(create.Notes);
            item.NotifyOnActivity.Should().Be(create.NotifyOnActivity);
            item.NotificationFrequency.Should().Be(create.NotificationFrequency);
            item.Notes.Should().Be(create.Notes);
            item.Email.Should().Be(create.Email);
            item.PhoneNumber.Should().Be(create.PhoneNumber);
            item.SocialMediaHandle.Should().Be(create.SocialMediaHandle);
            item.SocialMediaPlatform.Should().Be(create.SocialMediaPlatform);
            item.CountryCode.Should().Be(create.CountryCode);
            item.CheckMxRecords.Should().Be(create.CheckMxRecords);
            item.IsValid.Should().Be(create.IsValid);
            item.Message.Should().Be(create.Message);
            item.ErrorMessage.Should().Be(create.ErrorMessage);
            item.SuggestedCorrection.Should().Be(create.SuggestedCorrection);
            item.FormattedValue.Should().Be(create.FormattedValue);
            item.NormalizedValue.Should().Be(create.NormalizedValue);

            var getRes = await _client.GetAsync($"/api/contactinfo/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/contactinfo/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/contactinfo/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/contactinfo/{{item.Id}}");
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

