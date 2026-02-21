using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class ContactsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ContactsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Contacts_Succeeds()
        {
            var create = new { Platform = "Test", Url = "Test", Handle = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, DateAdded = DateTime.UtcNow, LastModified = DateTime.UtcNow, ModifiedBy = "Test", Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", LeadStatus = "Test", AccountId = 1, Status = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", Platform = "Test", Url = "Test", Handle = "Test" };
            var cRes = await _client.PostAsJsonAsync("/api/contacts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.Platform.Should().Be(create.Platform);
            item.Url.Should().Be(create.Url);
            item.Handle.Should().Be(create.Handle);
            item.ContactType.Should().Be(create.ContactType);
            item.FirstName.Should().Be(create.FirstName);
            item.LastName.Should().Be(create.LastName);
            item.MiddleName.Should().Be(create.MiddleName);
            item.EmailPrimary.Should().Be(create.EmailPrimary);
            item.EmailSecondary.Should().Be(create.EmailSecondary);
            item.PhonePrimary.Should().Be(create.PhonePrimary);
            item.PhoneSecondary.Should().Be(create.PhoneSecondary);
            item.Address.Should().Be(create.Address);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.Country.Should().Be(create.Country);
            item.ZipCode.Should().Be(create.ZipCode);
            item.JobTitle.Should().Be(create.JobTitle);
            item.Department.Should().Be(create.Department);
            item.Company.Should().Be(create.Company);
            item.ReportsTo.Should().Be(create.ReportsTo);
            item.Notes.Should().Be(create.Notes);
            item.DateOfBirth.Should().Be(create.DateOfBirth);
            item.DateAdded.Should().Be(create.DateAdded);
            item.LastModified.Should().Be(create.LastModified);
            item.ModifiedBy.Should().Be(create.ModifiedBy);
            item.Salutation.Should().Be(create.Salutation);
            item.Suffix.Should().Be(create.Suffix);
            item.Nickname.Should().Be(create.Nickname);
            item.Gender.Should().Be(create.Gender);
            item.PhoneMobile.Should().Be(create.PhoneMobile);
            item.PhoneFax.Should().Be(create.PhoneFax);
            item.Website.Should().Be(create.Website);
            item.LinkedInUrl.Should().Be(create.LinkedInUrl);
            item.TwitterHandle.Should().Be(create.TwitterHandle);
            item.DoNotContact.Should().Be(create.DoNotContact);
            item.PreferredContactMethod.Should().Be(create.PreferredContactMethod);
            item.LeadStatus.Should().Be(create.LeadStatus);
            item.AccountId.Should().Be(create.AccountId);
            item.Status.Should().Be(create.Status);
            item.ContactType.Should().Be(create.ContactType);
            item.FirstName.Should().Be(create.FirstName);
            item.LastName.Should().Be(create.LastName);
            item.MiddleName.Should().Be(create.MiddleName);
            item.EmailPrimary.Should().Be(create.EmailPrimary);
            item.EmailSecondary.Should().Be(create.EmailSecondary);
            item.PhonePrimary.Should().Be(create.PhonePrimary);
            item.PhoneSecondary.Should().Be(create.PhoneSecondary);
            item.Address.Should().Be(create.Address);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.Country.Should().Be(create.Country);
            item.ZipCode.Should().Be(create.ZipCode);
            item.JobTitle.Should().Be(create.JobTitle);
            item.Department.Should().Be(create.Department);
            item.Company.Should().Be(create.Company);
            item.ReportsTo.Should().Be(create.ReportsTo);
            item.Notes.Should().Be(create.Notes);
            item.DateOfBirth.Should().Be(create.DateOfBirth);
            item.Salutation.Should().Be(create.Salutation);
            item.Suffix.Should().Be(create.Suffix);
            item.Nickname.Should().Be(create.Nickname);
            item.Gender.Should().Be(create.Gender);
            item.PhoneMobile.Should().Be(create.PhoneMobile);
            item.PhoneFax.Should().Be(create.PhoneFax);
            item.Website.Should().Be(create.Website);
            item.LinkedInUrl.Should().Be(create.LinkedInUrl);
            item.TwitterHandle.Should().Be(create.TwitterHandle);
            item.DoNotContact.Should().Be(create.DoNotContact);
            item.PreferredContactMethod.Should().Be(create.PreferredContactMethod);
            item.ContactType.Should().Be(create.ContactType);
            item.FirstName.Should().Be(create.FirstName);
            item.LastName.Should().Be(create.LastName);
            item.MiddleName.Should().Be(create.MiddleName);
            item.EmailPrimary.Should().Be(create.EmailPrimary);
            item.EmailSecondary.Should().Be(create.EmailSecondary);
            item.PhonePrimary.Should().Be(create.PhonePrimary);
            item.PhoneSecondary.Should().Be(create.PhoneSecondary);
            item.Address.Should().Be(create.Address);
            item.City.Should().Be(create.City);
            item.State.Should().Be(create.State);
            item.Country.Should().Be(create.Country);
            item.ZipCode.Should().Be(create.ZipCode);
            item.JobTitle.Should().Be(create.JobTitle);
            item.Department.Should().Be(create.Department);
            item.Company.Should().Be(create.Company);
            item.ReportsTo.Should().Be(create.ReportsTo);
            item.Notes.Should().Be(create.Notes);
            item.DateOfBirth.Should().Be(create.DateOfBirth);
            item.Salutation.Should().Be(create.Salutation);
            item.Suffix.Should().Be(create.Suffix);
            item.Nickname.Should().Be(create.Nickname);
            item.Gender.Should().Be(create.Gender);
            item.PhoneMobile.Should().Be(create.PhoneMobile);
            item.PhoneFax.Should().Be(create.PhoneFax);
            item.Website.Should().Be(create.Website);
            item.LinkedInUrl.Should().Be(create.LinkedInUrl);
            item.TwitterHandle.Should().Be(create.TwitterHandle);
            item.DoNotContact.Should().Be(create.DoNotContact);
            item.PreferredContactMethod.Should().Be(create.PreferredContactMethod);
            item.Platform.Should().Be(create.Platform);
            item.Url.Should().Be(create.Url);
            item.Handle.Should().Be(create.Handle);

            var getRes = await _client.GetAsync($"/api/contacts/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { Platform = "Test2", Url = "Test", Handle = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, DateAdded = DateTime.UtcNow, LastModified = DateTime.UtcNow, ModifiedBy = "Test", Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", LeadStatus = "Test", AccountId = 1, Status = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", ContactType = "Test", FirstName = "Test", LastName = "Test", MiddleName = "Test", EmailPrimary = "Test", EmailSecondary = "Test", PhonePrimary = "Test", PhoneSecondary = "Test", Address = "Test", City = "Test", State = "Test", Country = "Test", ZipCode = "Test", JobTitle = "Test", Department = "Test", Company = "Test", ReportsTo = "Test", Notes = "Test", DateOfBirth = DateTime.UtcNow, Salutation = "Test", Suffix = "Test", Nickname = "Test", Gender = "Test", PhoneMobile = "Test", PhoneFax = "Test", Website = "Test", LinkedInUrl = "Test", TwitterHandle = "Test", DoNotContact = true, PreferredContactMethod = "Test", Platform = "Test2", Url = "Test", Handle = "Test" };
            var pRes = await _client.PatchAsJsonAsync($"/api/contacts/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/contacts/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/contacts/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/contacts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

