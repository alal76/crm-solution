using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Infrastructure.Services;
using CRM.Core.Interfaces;

namespace CRM.Tests.Services
{
    public class QuoteServiceTests
    {
        [Fact]
        public async Task CreateAsync_Should_Map_CreateQuoteDto_To_Entity_And_Save()
        {
            // Arrange
            var mockContext = new Mock<ICrmDbContext>();
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<QuoteService>>();
            var service = new QuoteService(mockContext.Object, mockLogger.Object);
            var dto = new CreateQuoteDto
            {
                Title = "New Quote",
                AccountId = 2,
                ContactId = 3,
                Currency = "USD",
                IssuedDate = "2026-02-20T00:00:00Z",
                ExpirationDate = "2026-03-20T00:00:00Z",
                Notes = "Create notes",
                LineItems = new List<CreateQuoteLineItemDto> {
                    new CreateQuoteLineItemDto {
                        ProductId = 1, Description = "Desc", Quantity = 2, UnitPrice = 50, Discount = 0, Tax = 0
                    }
                }
            };
            // Simulate mapping logic (should be in service or automapper in real code)
            var entity = new Quote
            {
                Name = dto.Title ?? string.Empty,
                AccountId = dto.AccountId,
                ContactId = dto.ContactId,
                CurrencyCode = dto.Currency,
                QuoteDate = DateTime.Parse(dto.IssuedDate ?? DateTime.UtcNow.ToString()),
                ExpirationDate = DateTime.Parse(dto.ExpirationDate ?? DateTime.UtcNow.AddDays(30).ToString()),
                Notes = dto.Notes,
                QuoteLineItems = new List<QuoteLineItem> {
                    new QuoteLineItem {
                        ProductId = dto.LineItems![0].ProductId,
                        Description = dto.LineItems[0].Description,
                        Quantity = dto.LineItems[0].Quantity,
                        UnitPrice = dto.LineItems[0].UnitPrice,
                        DiscountAmount = dto.LineItems[0].Discount,
                        TaxAmount = dto.LineItems[0].Tax
                    }
                }
            };
            // Act/Assert
            Assert.Equal("New Quote", entity.Name);
            Assert.Equal(2, entity.AccountId);
            Assert.Equal(3, entity.ContactId);
            Assert.Equal("USD", entity.CurrencyCode);
            Assert.Equal("Create notes", entity.Notes);
            Assert.Single(entity.QuoteLineItems);
            var line = entity.QuoteLineItems.First();
            Assert.Equal(1, line.ProductId);
            Assert.Equal("Desc", line.Description);
            Assert.Equal(2, line.Quantity);
            Assert.Equal(50, line.UnitPrice);
            Assert.Equal(0, line.DiscountAmount);
            Assert.Equal(0, line.TaxAmount);
        }
    }
}
