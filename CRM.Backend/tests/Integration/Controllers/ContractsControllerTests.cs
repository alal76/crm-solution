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
    public class ContractsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ContractsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Contracts_Succeeds()
        {
            var create = new
            {
                ContractNumber = "Test",
                Name = "Test",
                Description = "Test",
                AccountId = 1,
                AccountName = "Test",
                ContactId = 1,
                ContactName = "Test",
                OwnerId = 1,
                OwnerName = "Test",
                Status = (object?)null,
                ContractType = (object?)null,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TotalValue = 1,
                AnnualValue = 1,
                ActivatedAt = DateTime.UtcNow,
                TerminatedAt = DateTime.UtcNow,
                SignedDate = DateTime.UtcNow,
                IsSigned = true,
                AutoRenew = true,
                RenewalTermMonths = 1,
                PaymentTerms = "Test",
                TermsAndConditions = "Test",
                DocumentUrl = "Test",
                ParentContractId = 1,
                OpportunityId = 1,
                ActivatedDate = DateTime.UtcNow,
                TerminatedDate = DateTime.UtcNow,
                RenewalNoticeSent = true,
                RenewalNoticeSentDate = DateTime.UtcNow,
                RenewalInitiatedAt = DateTime.UtcNow,
                RenewalCompletedAt = DateTime.UtcNow,
                RenewalTermMonthsOverride = 1,
                SpecialConditions = "Test",
                TerminationClause = "Test",
                TerminationReason = "Test",
                CurrencyCode = "Test",
                BillingFrequency = "Test",
                RenewalNoticeDays = 1,
                ContractFileUrl = "Test",
                ContractFileName = "Test",
                ContractFileSize = 1,
                SignedContractFileUrl = "Test",
                SignedContractFileName = "Test",
                ApprovedByUserId = 1,
                ApprovedByName = "Test",
                ApprovedDate = DateTime.UtcNow,
                RejectionReason = "Test",
                SuspensionReason = "Test",
                SuspendedDate = DateTime.UtcNow,
                QuoteId = 1,
                ExpiringBefore = DateTime.UtcNow,
                ExpiringAfter = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                ContractId = 1,
                LineNumber = 1,
                Quantity = 1,
                UnitPrice = 1,
                TotalAmount = 1,
                OriginalContractId = 1,
                RenewalContractId = 1,
                OriginalEndDate = DateTime.UtcNow,
                NewEndDate = DateTime.UtcNow,
                RenewalDate = DateTime.UtcNow,
                IsCompleted = true,
                TotalContracts = 1,
                ActiveContracts = 1,
                ExpiringContracts = 1,
                ExpiredContracts = 1,
                PendingRenewals = 1,
                TotalContractValue = 1,
                ActiveContractValue = 1,
                RenewalRate = 1,
                AverageContractLength = 1,
                AllSigned = true,
                TotalSigners = 1,
                SignedCount = 1,
                SignerId = 1,
                SignerName = "Test",
                SignerEmail = "Test",
                HasSigned = true,
                SignedAt = DateTime.UtcNow,
                FileName = "Test",
                FilePath = "Test",
                DocumentType = "Test",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "Test",
                Reason = "Test",
                TerminationDate = DateTime.UtcNow,
                RenewalNotes = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/contracts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/contracts/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                ContractNumber = "Test2",
                Name = "Test",
                Description = "Test",
                AccountId = 1,
                AccountName = "Test",
                ContactId = 1,
                ContactName = "Test",
                OwnerId = 1,
                OwnerName = "Test",
                Status = (object?)null,
                ContractType = (object?)null,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TotalValue = 1,
                AnnualValue = 1,
                ActivatedAt = DateTime.UtcNow,
                TerminatedAt = DateTime.UtcNow,
                SignedDate = DateTime.UtcNow,
                IsSigned = true,
                AutoRenew = true,
                RenewalTermMonths = 1,
                PaymentTerms = "Test",
                TermsAndConditions = "Test",
                DocumentUrl = "Test",
                ParentContractId = 1,
                OpportunityId = 1,
                ActivatedDate = DateTime.UtcNow,
                TerminatedDate = DateTime.UtcNow,
                RenewalNoticeSent = true,
                RenewalNoticeSentDate = DateTime.UtcNow,
                RenewalInitiatedAt = DateTime.UtcNow,
                RenewalCompletedAt = DateTime.UtcNow,
                RenewalTermMonthsOverride = 1,
                SpecialConditions = "Test",
                TerminationClause = "Test",
                TerminationReason = "Test",
                CurrencyCode = "Test",
                BillingFrequency = "Test",
                RenewalNoticeDays = 1,
                ContractFileUrl = "Test",
                ContractFileName = "Test",
                ContractFileSize = 1,
                SignedContractFileUrl = "Test",
                SignedContractFileName = "Test",
                ApprovedByUserId = 1,
                ApprovedByName = "Test",
                ApprovedDate = DateTime.UtcNow,
                RejectionReason = "Test",
                SuspensionReason = "Test",
                SuspendedDate = DateTime.UtcNow,
                QuoteId = 1,
                ExpiringBefore = DateTime.UtcNow,
                ExpiringAfter = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                ContractId = 1,
                LineNumber = 1,
                Quantity = 1,
                UnitPrice = 1,
                TotalAmount = 1,
                OriginalContractId = 1,
                RenewalContractId = 1,
                OriginalEndDate = DateTime.UtcNow,
                NewEndDate = DateTime.UtcNow,
                RenewalDate = DateTime.UtcNow,
                IsCompleted = true,
                TotalContracts = 1,
                ActiveContracts = 1,
                ExpiringContracts = 1,
                ExpiredContracts = 1,
                PendingRenewals = 1,
                TotalContractValue = 1,
                ActiveContractValue = 1,
                RenewalRate = 1,
                AverageContractLength = 1,
                AllSigned = true,
                TotalSigners = 1,
                SignedCount = 1,
                SignerId = 1,
                SignerName = "Test",
                SignerEmail = "Test",
                HasSigned = true,
                SignedAt = DateTime.UtcNow,
                FileName = "Test",
                FilePath = "Test",
                DocumentType = "Test",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "Test",
                Reason = "Test",
                TerminationDate = DateTime.UtcNow,
                RenewalNotes = "Test"
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/contracts/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/contracts/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/contracts/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/contracts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
