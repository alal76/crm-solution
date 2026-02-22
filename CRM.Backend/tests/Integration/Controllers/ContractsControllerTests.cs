using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.ContractNumber.Should().Be(create.ContractNumber);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.AccountId.Should().Be(create.AccountId);
            item.AccountName.Should().Be(create.AccountName);
            item.ContactId.Should().Be(create.ContactId);
            item.ContactName.Should().Be(create.ContactName);
            item.OwnerId.Should().Be(create.OwnerId);
            item.OwnerName.Should().Be(create.OwnerName);
            item.Status.Should().Be(create.Status);
            item.ContractType.Should().Be(create.ContractType);
            item.StartDate.Should().Be(create.StartDate);
            item.EndDate.Should().Be(create.EndDate);
            item.TotalValue.Should().Be(create.TotalValue);
            item.AnnualValue.Should().Be(create.AnnualValue);
            item.ActivatedAt.Should().Be(create.ActivatedAt);
            item.TerminatedAt.Should().Be(create.TerminatedAt);
            item.SignedDate.Should().Be(create.SignedDate);
            item.IsSigned.Should().Be(create.IsSigned);
            item.AutoRenew.Should().Be(create.AutoRenew);
            item.RenewalTermMonths.Should().Be(create.RenewalTermMonths);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.DocumentUrl.Should().Be(create.DocumentUrl);
            item.ParentContractId.Should().Be(create.ParentContractId);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.ActivatedDate.Should().Be(create.ActivatedDate);
            item.TerminatedDate.Should().Be(create.TerminatedDate);
            item.RenewalNoticeSent.Should().Be(create.RenewalNoticeSent);
            item.RenewalNoticeSentDate.Should().Be(create.RenewalNoticeSentDate);
            item.RenewalInitiatedAt.Should().Be(create.RenewalInitiatedAt);
            item.RenewalCompletedAt.Should().Be(create.RenewalCompletedAt);
            item.RenewalTermMonthsOverride.Should().Be(create.RenewalTermMonthsOverride);
            item.SpecialConditions.Should().Be(create.SpecialConditions);
            item.TerminationClause.Should().Be(create.TerminationClause);
            item.TerminationReason.Should().Be(create.TerminationReason);
            item.CurrencyCode.Should().Be(create.CurrencyCode);
            item.BillingFrequency.Should().Be(create.BillingFrequency);
            item.RenewalNoticeDays.Should().Be(create.RenewalNoticeDays);
            item.ContractFileUrl.Should().Be(create.ContractFileUrl);
            item.ContractFileName.Should().Be(create.ContractFileName);
            item.ContractFileSize.Should().Be(create.ContractFileSize);
            item.SignedContractFileUrl.Should().Be(create.SignedContractFileUrl);
            item.SignedContractFileName.Should().Be(create.SignedContractFileName);
            item.ApprovedByUserId.Should().Be(create.ApprovedByUserId);
            item.ApprovedByName.Should().Be(create.ApprovedByName);
            item.ApprovedDate.Should().Be(create.ApprovedDate);
            item.RejectionReason.Should().Be(create.RejectionReason);
            item.SuspensionReason.Should().Be(create.SuspensionReason);
            item.SuspendedDate.Should().Be(create.SuspendedDate);
            item.QuoteId.Should().Be(create.QuoteId);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.AccountId.Should().Be(create.AccountId);
            item.ContactId.Should().Be(create.ContactId);
            item.OwnerId.Should().Be(create.OwnerId);
            item.ContractType.Should().Be(create.ContractType);
            item.StartDate.Should().Be(create.StartDate);
            item.EndDate.Should().Be(create.EndDate);
            item.TotalValue.Should().Be(create.TotalValue);
            item.AnnualValue.Should().Be(create.AnnualValue);
            item.AutoRenew.Should().Be(create.AutoRenew);
            item.RenewalTermMonths.Should().Be(create.RenewalTermMonths);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.Name.Should().Be(create.Name);
            item.Description.Should().Be(create.Description);
            item.ContactId.Should().Be(create.ContactId);
            item.OwnerId.Should().Be(create.OwnerId);
            item.ContractType.Should().Be(create.ContractType);
            item.EndDate.Should().Be(create.EndDate);
            item.TotalValue.Should().Be(create.TotalValue);
            item.AnnualValue.Should().Be(create.AnnualValue);
            item.AutoRenew.Should().Be(create.AutoRenew);
            item.RenewalTermMonths.Should().Be(create.RenewalTermMonths);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.AccountId.Should().Be(create.AccountId);
            item.Status.Should().Be(create.Status);
            item.ContractType.Should().Be(create.ContractType);
            item.AutoRenew.Should().Be(create.AutoRenew);
            item.ExpiringBefore.Should().Be(create.ExpiringBefore);
            item.ExpiringAfter.Should().Be(create.ExpiringAfter);
            item.Page.Should().Be(create.Page);
            item.PageSize.Should().Be(create.PageSize);
            item.SortBy.Should().Be(create.SortBy);
            item.SortOrder.Should().Be(create.SortOrder);
            item.ContractId.Should().Be(create.ContractId);
            item.LineNumber.Should().Be(create.LineNumber);
            item.Description.Should().Be(create.Description);
            item.Quantity.Should().Be(create.Quantity);
            item.UnitPrice.Should().Be(create.UnitPrice);
            item.TotalAmount.Should().Be(create.TotalAmount);
            item.Description.Should().Be(create.Description);
            item.Quantity.Should().Be(create.Quantity);
            item.UnitPrice.Should().Be(create.UnitPrice);
            item.OriginalContractId.Should().Be(create.OriginalContractId);
            item.RenewalContractId.Should().Be(create.RenewalContractId);
            item.OriginalEndDate.Should().Be(create.OriginalEndDate);
            item.NewEndDate.Should().Be(create.NewEndDate);
            item.RenewalDate.Should().Be(create.RenewalDate);
            item.IsCompleted.Should().Be(create.IsCompleted);
            item.TotalContracts.Should().Be(create.TotalContracts);
            item.ActiveContracts.Should().Be(create.ActiveContracts);
            item.ExpiringContracts.Should().Be(create.ExpiringContracts);
            item.ExpiredContracts.Should().Be(create.ExpiredContracts);
            item.PendingRenewals.Should().Be(create.PendingRenewals);
            item.TotalContractValue.Should().Be(create.TotalContractValue);
            item.ActiveContractValue.Should().Be(create.ActiveContractValue);
            item.RenewalRate.Should().Be(create.RenewalRate);
            item.AverageContractLength.Should().Be(create.AverageContractLength);
            item.ContractId.Should().Be(create.ContractId);
            item.AllSigned.Should().Be(create.AllSigned);
            item.TotalSigners.Should().Be(create.TotalSigners);
            item.SignedCount.Should().Be(create.SignedCount);
            item.SignerId.Should().Be(create.SignerId);
            item.SignerName.Should().Be(create.SignerName);
            item.SignerEmail.Should().Be(create.SignerEmail);
            item.HasSigned.Should().Be(create.HasSigned);
            item.SignedAt.Should().Be(create.SignedAt);
            item.FileName.Should().Be(create.FileName);
            item.FilePath.Should().Be(create.FilePath);
            item.DocumentType.Should().Be(create.DocumentType);
            item.UploadedAt.Should().Be(create.UploadedAt);
            item.UploadedBy.Should().Be(create.UploadedBy);
            item.Reason.Should().Be(create.Reason);
            item.TerminationDate.Should().Be(create.TerminationDate);
            item.RenewalTermMonths.Should().Be(create.RenewalTermMonths);
            item.RenewalNotes.Should().Be(create.RenewalNotes);

            var getRes = await _client.GetAsync($"/api/contracts/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/contracts/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/contracts/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/contracts/{{item.Id}}");
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

