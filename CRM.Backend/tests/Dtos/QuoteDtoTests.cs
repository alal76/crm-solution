// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using Xunit;

namespace CRM.Tests.Dtos
{
    public class QuoteDtoTests
    {
        [Fact]
        public void QuoteDto_AllFields_CanBeSetAndRead()
        {
            var dto = new QuoteDto
            {
                Id = 1,
                QuoteNumber = "Q-2026-00001",
                Title = "Test Quote",
                AccountId = 2,
                AccountName = "Test Account",
                ContactId = 3,
                ContactName = "Test Contact",
                Status = 1,
                Currency = "USD",
                IssuedDate = "2026-02-20T00:00:00Z",
                ExpirationDate = "2026-03-20T00:00:00Z",
                Notes = "Test notes",
                Subtotal = 100,
                DiscountTotal = 10,
                TaxTotal = 5,
                GrandTotal = 95,
                LineItems = new List<QuoteLineItemDto> {
                    new QuoteLineItemDto {
                        Id = 1, QuoteId = 1, ProductId = 1, ProductName = "Product", Description = "Desc", Quantity = 2, UnitPrice = 50, Discount = 0, Tax = 0, LineTotal = 100
                    }
                },
                CreatedAt = "2026-02-20T00:00:00Z",
                UpdatedAt = "2026-02-21T00:00:00Z",
                IsDeleted = false,
                RowVersion = new byte[] { 1, 2, 3 },
                CreatedByUserId = 10,
                UpdatedByUserId = 11
            };

            Assert.Equal(1, dto.Id);
            Assert.Equal("Q-2026-00001", dto.QuoteNumber);
            Assert.Equal("Test Quote", dto.Title);
            Assert.Equal(2, dto.AccountId);
            Assert.Equal("Test Account", dto.AccountName);
            Assert.Equal(3, dto.ContactId);
            Assert.Equal("Test Contact", dto.ContactName);
            Assert.Equal(1, dto.Status);
            Assert.Equal("USD", dto.Currency);
            Assert.Equal("2026-02-20T00:00:00Z", dto.IssuedDate);
            Assert.Equal("2026-03-20T00:00:00Z", dto.ExpirationDate);
            Assert.Equal("Test notes", dto.Notes);
            Assert.Equal(100, dto.Subtotal);
            Assert.Equal(10, dto.DiscountTotal);
            Assert.Equal(5, dto.TaxTotal);
            Assert.Equal(95, dto.GrandTotal);
            Assert.Single(dto.LineItems);
            Assert.Equal("2026-02-20T00:00:00Z", dto.CreatedAt);
            Assert.Equal("2026-02-21T00:00:00Z", dto.UpdatedAt);
            Assert.False(dto.IsDeleted);
            Assert.Equal(new byte[] { 1, 2, 3 }, dto.RowVersion);
            Assert.Equal(10, dto.CreatedByUserId);
            Assert.Equal(11, dto.UpdatedByUserId);
        }

        [Fact]
        public void CreateQuoteDto_AllFields_CanBeSetAndRead()
        {
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

            Assert.Equal("New Quote", dto.Title);
            Assert.Equal(2, dto.AccountId);
            Assert.Equal(3, dto.ContactId);
            Assert.Equal("USD", dto.Currency);
            Assert.Equal("2026-02-20T00:00:00Z", dto.IssuedDate);
            Assert.Equal("2026-03-20T00:00:00Z", dto.ExpirationDate);
            Assert.Equal("Create notes", dto.Notes);
            Assert.Single(dto.LineItems);
        }

        [Fact]
        public void UpdateQuoteDto_AllFields_CanBeSetAndRead()
        {
            var dto = new UpdateQuoteDto
            {
                Title = "Update Quote",
                AccountId = 2,
                ContactId = 3,
                Status = 2,
                Currency = "USD",
                IssuedDate = "2026-02-20T00:00:00Z",
                ExpirationDate = "2026-03-20T00:00:00Z",
                Notes = "Update notes",
                LineItems = new List<UpdateQuoteLineItemDto> {
                    new UpdateQuoteLineItemDto {
                        Id = 1, Description = "Desc", Quantity = 2, UnitPrice = 50, Discount = 0, Tax = 0
                    }
                }
            };

            Assert.Equal("Update Quote", dto.Title);
            Assert.Equal(2, dto.AccountId);
            Assert.Equal(3, dto.ContactId);
            Assert.Equal(2, dto.Status);
            Assert.Equal("USD", dto.Currency);
            Assert.Equal("2026-02-20T00:00:00Z", dto.IssuedDate);
            Assert.Equal("2026-03-20T00:00:00Z", dto.ExpirationDate);
            Assert.Equal("Update notes", dto.Notes);
            Assert.Single(dto.LineItems);
        }
    }
}
