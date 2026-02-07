// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Product entity and related enums.
/// Product is a comprehensive entity supporting physical products, services, subscriptions,
/// and bundles with various pricing models and billing frequencies.
/// </summary>
public class ProductEntityTests
{
    #region ProductStatus Enum Tests

    public class ProductStatusTests
    {
        [Theory]
        [InlineData(ProductStatus.Draft, 0)]
        [InlineData(ProductStatus.Active, 1)]
        [InlineData(ProductStatus.Discontinued, 2)]
        [InlineData(ProductStatus.OutOfStock, 3)]
        [InlineData(ProductStatus.ComingSoon, 4)]
        [InlineData(ProductStatus.Archived, 5)]
        [InlineData(ProductStatus.Limited, 6)]
        [InlineData(ProductStatus.Beta, 7)]
        [InlineData(ProductStatus.EndOfLife, 8)]
        public void ProductStatus_ShouldHaveCorrectValue(ProductStatus status, int expected)
        {
            ((int)status).Should().Be(expected);
        }

        [Fact]
        public void ProductStatus_AllValues_ShouldHaveNineStatuses()
        {
            var values = Enum.GetValues<ProductStatus>();
            values.Should().HaveCount(9);
        }
    }

    #endregion

    #region ProductType Enum Tests

    public class ProductTypeTests
    {
        [Theory]
        [InlineData(ProductType.Physical, 0)]
        [InlineData(ProductType.Digital, 1)]
        [InlineData(ProductType.Service, 2)]
        [InlineData(ProductType.Subscription, 3)]
        [InlineData(ProductType.Bundle, 4)]
        [InlineData(ProductType.Rental, 5)]
        [InlineData(ProductType.Consulting, 6)]
        [InlineData(ProductType.ManagedService, 7)]
        [InlineData(ProductType.SupportContract, 8)]
        [InlineData(ProductType.Training, 9)]
        [InlineData(ProductType.License, 10)]
        [InlineData(ProductType.ProfessionalServices, 11)]
        [InlineData(ProductType.Implementation, 12)]
        public void ProductType_ShouldHaveCorrectValue(ProductType type, int expected)
        {
            ((int)type).Should().Be(expected);
        }

        [Fact]
        public void ProductType_AllValues_ShouldHaveThirteenTypes()
        {
            var values = Enum.GetValues<ProductType>();
            values.Should().HaveCount(13);
        }
    }

    #endregion

    #region BillingFrequency Enum Tests

    public class BillingFrequencyTests
    {
        [Theory]
        [InlineData(BillingFrequency.OneTime, 0)]
        [InlineData(BillingFrequency.Daily, 1)]
        [InlineData(BillingFrequency.Weekly, 2)]
        [InlineData(BillingFrequency.BiWeekly, 3)]
        [InlineData(BillingFrequency.Monthly, 4)]
        [InlineData(BillingFrequency.Quarterly, 5)]
        [InlineData(BillingFrequency.SemiAnnually, 6)]
        [InlineData(BillingFrequency.Annually, 7)]
        [InlineData(BillingFrequency.MultiYear, 8)]
        [InlineData(BillingFrequency.Custom, 9)]
        [InlineData(BillingFrequency.UsageBased, 10)]
        public void BillingFrequency_ShouldHaveCorrectValue(BillingFrequency frequency, int expected)
        {
            ((int)frequency).Should().Be(expected);
        }

        [Fact]
        public void BillingFrequency_AllValues_ShouldHaveElevenFrequencies()
        {
            var values = Enum.GetValues<BillingFrequency>();
            values.Should().HaveCount(11);
        }
    }

    #endregion

    #region PricingModel Enum Tests

    public class PricingModelTests
    {
        [Theory]
        [InlineData(PricingModel.FixedPrice, 0)]
        [InlineData(PricingModel.TieredPricing, 1)]
        [InlineData(PricingModel.VolumePricing, 2)]
        [InlineData(PricingModel.UsageBased, 3)]
        [InlineData(PricingModel.PerUser, 4)]
        [InlineData(PricingModel.PerFeature, 5)]
        [InlineData(PricingModel.FlatRate, 6)]
        [InlineData(PricingModel.Hourly, 7)]
        [InlineData(PricingModel.Daily, 8)]
        [InlineData(PricingModel.ProjectBased, 9)]
        [InlineData(PricingModel.CustomQuote, 10)]
        [InlineData(PricingModel.Freemium, 11)]
        public void PricingModel_ShouldHaveCorrectValue(PricingModel model, int expected)
        {
            ((int)model).Should().Be(expected);
        }

        [Fact]
        public void PricingModel_AllValues_ShouldHaveTwelveModels()
        {
            var values = Enum.GetValues<PricingModel>();
            values.Should().HaveCount(12);
        }
    }

    #endregion

    #region UnitOfMeasure Enum Tests

    public class UnitOfMeasureTests
    {
        [Theory]
        [InlineData(UnitOfMeasure.Each, 0)]
        [InlineData(UnitOfMeasure.Hour, 1)]
        [InlineData(UnitOfMeasure.Day, 2)]
        [InlineData(UnitOfMeasure.Week, 3)]
        [InlineData(UnitOfMeasure.Month, 4)]
        [InlineData(UnitOfMeasure.Year, 5)]
        [InlineData(UnitOfMeasure.User, 6)]
        [InlineData(UnitOfMeasure.Device, 7)]
        [InlineData(UnitOfMeasure.Transaction, 8)]
        [InlineData(UnitOfMeasure.Gigabyte, 9)]
        [InlineData(UnitOfMeasure.ApiCall, 10)]
        [InlineData(UnitOfMeasure.Project, 11)]
        [InlineData(UnitOfMeasure.License, 12)]
        [InlineData(UnitOfMeasure.Kilogram, 13)]
        [InlineData(UnitOfMeasure.Meter, 14)]
        [InlineData(UnitOfMeasure.Liter, 15)]
        [InlineData(UnitOfMeasure.Case, 16)]
        [InlineData(UnitOfMeasure.Pallet, 17)]
        public void UnitOfMeasure_ShouldHaveCorrectValue(UnitOfMeasure unit, int expected)
        {
            ((int)unit).Should().Be(expected);
        }

        [Fact]
        public void UnitOfMeasure_AllValues_ShouldHaveEighteenUnits()
        {
            var values = Enum.GetValues<UnitOfMeasure>();
            values.Should().HaveCount(18);
        }
    }

    #endregion

    #region RevenueRecognitionMethod Enum Tests

    public class RevenueRecognitionMethodTests
    {
        [Theory]
        [InlineData(RevenueRecognitionMethod.Immediate, 0)]
        [InlineData(RevenueRecognitionMethod.OverTime, 1)]
        [InlineData(RevenueRecognitionMethod.OnDelivery, 2)]
        [InlineData(RevenueRecognitionMethod.Milestone, 3)]
        [InlineData(RevenueRecognitionMethod.PercentageOfCompletion, 4)]
        public void RevenueRecognitionMethod_ShouldHaveCorrectValue(RevenueRecognitionMethod method, int expected)
        {
            ((int)method).Should().Be(expected);
        }

        [Fact]
        public void RevenueRecognitionMethod_AllValues_ShouldHaveFiveMethods()
        {
            var values = Enum.GetValues<RevenueRecognitionMethod>();
            values.Should().HaveCount(5);
        }
    }

    #endregion

    #region ServiceTier Enum Tests

    public class ServiceTierTests
    {
        [Theory]
        [InlineData(ServiceTier.Basic, 0)]
        [InlineData(ServiceTier.Standard, 1)]
        [InlineData(ServiceTier.Professional, 2)]
        [InlineData(ServiceTier.Enterprise, 3)]
        [InlineData(ServiceTier.Premium, 4)]
        [InlineData(ServiceTier.Custom, 5)]
        public void ServiceTier_ShouldHaveCorrectValue(ServiceTier tier, int expected)
        {
            ((int)tier).Should().Be(expected);
        }

        [Fact]
        public void ServiceTier_AllValues_ShouldHaveSixTiers()
        {
            var values = Enum.GetValues<ServiceTier>();
            values.Should().HaveCount(6);
        }
    }

    #endregion

    #region ContractTermCategory Enum Tests

    public class ContractTermCategoryTests
    {
        [Theory]
        [InlineData(ContractTermCategory.NoContract, 0)]
        [InlineData(ContractTermCategory.Weekly, 1)]
        [InlineData(ContractTermCategory.Monthly, 2)]
        [InlineData(ContractTermCategory.Quarterly, 3)]
        [InlineData(ContractTermCategory.SemiAnnual, 4)]
        [InlineData(ContractTermCategory.Annual, 5)]
        [InlineData(ContractTermCategory.TwoYear, 6)]
        [InlineData(ContractTermCategory.ThreeYear, 7)]
        [InlineData(ContractTermCategory.FiveYear, 8)]
        [InlineData(ContractTermCategory.Custom, 9)]
        public void ContractTermCategory_ShouldHaveCorrectValue(ContractTermCategory term, int expected)
        {
            ((int)term).Should().Be(expected);
        }

        [Fact]
        public void ContractTermCategory_AllValues_ShouldHaveTenCategories()
        {
            var values = Enum.GetValues<ContractTermCategory>();
            values.Should().HaveCount(10);
        }
    }

    #endregion

    #region Product Entity Default Values Tests

    public class ProductDefaultValuesTests
    {
        [Fact]
        public void Product_DefaultValues_BasicInfoShouldBeCorrect()
        {
            // Arrange & Act
            var product = new Product();

            // Assert - Identification
            product.Name.Should().BeEmpty();
            product.Description.Should().BeEmpty();
            product.ShortDescription.Should().BeNull();
            product.SKU.Should().BeEmpty();
            product.ProductCode.Should().BeNull();
            product.Barcode.Should().BeNull();
            product.ExternalId.Should().BeNull();
            product.InternalReference.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_ClassificationShouldBeCorrect()
        {
            var product = new Product();

            product.ProductType.Should().Be(ProductType.Physical);
            product.Status.Should().Be(ProductStatus.Active);
            product.Category.Should().BeEmpty();
            product.SubCategory.Should().BeNull();
            product.ProductFamily.Should().BeNull();
            product.Brand.Should().BeNull();
            product.Manufacturer.Should().BeNull();
            product.Tags.Should().BeNull();
            product.ServiceTier.Should().Be(ServiceTier.Standard);
            product.IsService.Should().BeFalse();
            product.IsSubscription.Should().BeFalse();
        }

        [Fact]
        public void Product_DefaultValues_UnitPricingShouldBeCorrect()
        {
            var product = new Product();

            product.Price.Should().Be(0);
            product.ListPrice.Should().BeNull();
            product.Cost.Should().Be(0);
            product.MinimumPrice.Should().BeNull();
            product.WholesalePrice.Should().BeNull();
            product.PartnerPrice.Should().BeNull();
            product.Margin.Should().Be(0);
            product.TargetMargin.Should().BeNull();
            product.UnitOfMeasure.Should().Be(UnitOfMeasure.Each);
            product.CustomUnitOfMeasure.Should().BeNull();
            product.MinimumQuantity.Should().Be(1);
            product.MaximumQuantity.Should().BeNull();
            product.QuantityIncrement.Should().Be(1);
        }

        [Fact]
        public void Product_DefaultValues_ContractPricingShouldBeCorrect()
        {
            var product = new Product();

            product.WeeklyPrice.Should().BeNull();
            product.MonthlyPrice.Should().BeNull();
            product.QuarterlyPrice.Should().BeNull();
            product.SemiAnnualPrice.Should().BeNull();
            product.AnnualPrice.Should().BeNull();
            product.TwoYearPrice.Should().BeNull();
            product.ThreeYearPrice.Should().BeNull();
            product.ContractPricing.Should().BeNull();
            product.DefaultContractTerm.Should().Be(ContractTermCategory.Monthly);
            product.MinimumContractTerm.Should().Be(ContractTermCategory.NoContract);
        }

        [Fact]
        public void Product_DefaultValues_TermDiscountsShouldBeCorrect()
        {
            var product = new Product();

            product.WeeklyTermDiscount.Should().Be(0);
            product.MonthlyTermDiscount.Should().Be(0);
            product.QuarterlyTermDiscount.Should().Be(5);
            product.SemiAnnualTermDiscount.Should().Be(10);
            product.AnnualTermDiscount.Should().Be(15);
            product.TwoYearTermDiscount.Should().Be(20);
            product.ThreeYearTermDiscount.Should().Be(25);
            product.MaxTermDiscount.Should().Be(30);
        }

        [Fact]
        public void Product_DefaultValues_VolumeDiscountsShouldBeCorrect()
        {
            var product = new Product();

            product.VolumeDiscounts.Should().BeNull();
            product.PricingTiers.Should().BeNull();
            product.MaxVolumeDiscount.Should().Be(25);
            product.MaxTotalDiscount.Should().Be(40);
        }

        [Fact]
        public void Product_DefaultValues_SubscriptionFieldsShouldBeCorrect()
        {
            var product = new Product();

            product.BillingFrequency.Should().Be(BillingFrequency.OneTime);
            product.PricingModel.Should().Be(PricingModel.FixedPrice);
            product.RecurringPrice.Should().BeNull();
            product.SetupFee.Should().BeNull();
            product.ActivationFee.Should().BeNull();
            product.CancellationFee.Should().BeNull();
            product.TrialPeriodDays.Should().BeNull();
            product.ContractLengthMonths.Should().BeNull();
            product.MinContractLengthMonths.Should().BeNull();
            product.BillingDayOfMonth.Should().BeNull();
            product.AutoRenewal.Should().BeTrue();
            product.RenewalPrice.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_ServiceFieldsShouldBeCorrect()
        {
            var product = new Product();

            product.HourlyRate.Should().BeNull();
            product.DailyRate.Should().BeNull();
            product.MinimumBillableHours.Should().BeNull();
            product.BillableHourIncrement.Should().Be(0.25m);
            product.OvertimeMultiplier.Should().BeNull();
            product.WeekendMultiplier.Should().BeNull();
            product.HolidayMultiplier.Should().BeNull();
            product.IncludesOnsiteWork.Should().BeFalse();
            product.TravelIncluded.Should().BeFalse();
            product.MaterialsIncluded.Should().BeFalse();
        }

        [Fact]
        public void Product_DefaultValues_TaxAndCurrencyShouldBeCorrect()
        {
            var product = new Product();

            product.CurrencyCode.Should().Be("USD");
            product.IsTaxable.Should().BeTrue();
            product.TaxRate.Should().BeNull();
            product.TaxCategory.Should().BeNull();
            product.TaxExemptionCode.Should().BeNull();
            product.RevenueRecognition.Should().Be(RevenueRecognitionMethod.Immediate);
        }

        [Fact]
        public void Product_DefaultValues_InventoryShouldBeCorrect()
        {
            var product = new Product();

            product.Quantity.Should().Be(0);
            product.ReorderLevel.Should().BeNull();
            product.ReorderQuantity.Should().BeNull();
            product.MaxQuantity.Should().BeNull();
            product.ReservedQuantity.Should().BeNull();
            product.AvailableQuantity.Should().BeNull();
            product.WarehouseLocation.Should().BeNull();
            product.TrackInventory.Should().BeTrue();
            product.AllowBackorder.Should().BeFalse();
            product.LeadTimeDays.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_PhysicalAttributesShouldBeCorrect()
        {
            var product = new Product();

            product.Weight.Should().BeNull();
            product.WeightUnit.Should().Be("kg");
            product.Length.Should().BeNull();
            product.Width.Should().BeNull();
            product.Height.Should().BeNull();
            product.DimensionUnit.Should().Be("cm");
            product.ShippingClass.Should().BeNull();
            product.IsHazardous.Should().BeFalse();
            product.SpecialHandling.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_MediaShouldBeCorrect()
        {
            var product = new Product();

            product.ImageUrl.Should().BeEmpty();
            product.ThumbnailUrl.Should().BeNull();
            product.AdditionalImages.Should().BeNull();
            product.VideoUrl.Should().BeNull();
            product.DocumentUrls.Should().BeNull();
            product.DatasheetUrl.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_SalesPerformanceShouldBeCorrect()
        {
            var product = new Product();

            product.TotalSold.Should().Be(0);
            product.TotalRevenue.Should().Be(0);
            product.AverageRating.Should().Be(0);
            product.ReviewCount.Should().Be(0);
            product.IsFeatured.Should().BeFalse();
            product.IsBestSeller.Should().BeFalse();
            product.IsNew.Should().BeFalse();
            product.IsOnSale.Should().BeFalse();
            product.SalePrice.Should().BeNull();
        }

        [Fact]
        public void Product_DefaultValues_StatusFlagsShouldBeCorrect()
        {
            var product = new Product();

            product.IsActive.Should().BeTrue();
            product.IsVisible.Should().BeTrue();
            product.IsPurchasable.Should().BeTrue();
        }
    }

    #endregion

    #region Product Computed Properties Tests

    public class ProductComputedPropertiesTests
    {
        [Theory]
        [InlineData(ProductType.Physical, true)]
        [InlineData(ProductType.Rental, true)]
        [InlineData(ProductType.Digital, false)]
        [InlineData(ProductType.Service, false)]
        [InlineData(ProductType.Subscription, false)]
        [InlineData(ProductType.Bundle, false)]
        public void Product_ComputedIsPhysical_ShouldReturnCorrectValue(ProductType type, bool expected)
        {
            var product = new Product { ProductType = type };
            product.IsPhysical.Should().Be(expected);
        }

        [Theory]
        [InlineData(BillingFrequency.OneTime, false)]
        [InlineData(BillingFrequency.Monthly, true)]
        [InlineData(BillingFrequency.Quarterly, true)]
        [InlineData(BillingFrequency.Annually, true)]
        [InlineData(BillingFrequency.Daily, true)]
        [InlineData(BillingFrequency.UsageBased, true)]
        public void Product_ComputedIsRecurring_ShouldReturnCorrectValue(BillingFrequency frequency, bool expected)
        {
            var product = new Product { BillingFrequency = frequency };
            product.IsRecurring.Should().Be(expected);
        }

        [Fact]
        public void Product_ComputedEffectivePrice_WhenNotOnSale_ShouldReturnPrice()
        {
            var product = new Product { Price = 100m, IsOnSale = false, SalePrice = null };
            product.EffectivePrice.Should().Be(100m);
        }

        [Fact]
        public void Product_ComputedEffectivePrice_WhenOnSaleWithSalePrice_ShouldReturnSalePrice()
        {
            var product = new Product { Price = 100m, IsOnSale = true, SalePrice = 80m };
            product.EffectivePrice.Should().Be(80m);
        }

        [Fact]
        public void Product_ComputedEffectivePrice_WhenOnSaleWithoutSalePrice_ShouldReturnPrice()
        {
            var product = new Product { Price = 100m, IsOnSale = true, SalePrice = null };
            product.EffectivePrice.Should().Be(100m);
        }

        [Theory]
        [InlineData(100, 60, 40)] // (100-60)/100 * 100 = 40%
        [InlineData(200, 150, 25)] // (200-150)/200 * 100 = 25%
        [InlineData(50, 0, 100)]  // (50-0)/50 * 100 = 100%
        [InlineData(0, 0, 0)]     // Division by zero protection
        public void Product_ComputedCalculatedMargin_ShouldReturnCorrectValue(
            decimal price, decimal cost, decimal expectedMargin)
        {
            var product = new Product { Price = price, Cost = cost };
            product.CalculatedMargin.Should().Be(expectedMargin);
        }

        [Fact]
        public void Product_ComputedIsCurrentlyOnSale_WithValidDates_ShouldBeTrue()
        {
            var now = DateTime.UtcNow;
            var product = new Product
            {
                IsOnSale = true,
                SaleStartDate = now.AddDays(-1),
                SaleEndDate = now.AddDays(7)
            };
            product.IsCurrentlyOnSale.Should().BeTrue();
        }

        [Fact]
        public void Product_ComputedIsCurrentlyOnSale_WhenNotOnSale_ShouldBeFalse()
        {
            var product = new Product { IsOnSale = false };
            product.IsCurrentlyOnSale.Should().BeFalse();
        }

        [Fact]
        public void Product_ComputedIsCurrentlyOnSale_BeforeStartDate_ShouldBeFalse()
        {
            var product = new Product
            {
                IsOnSale = true,
                SaleStartDate = DateTime.UtcNow.AddDays(1),
                SaleEndDate = DateTime.UtcNow.AddDays(7)
            };
            product.IsCurrentlyOnSale.Should().BeFalse();
        }

        [Fact]
        public void Product_ComputedIsCurrentlyOnSale_AfterEndDate_ShouldBeFalse()
        {
            var product = new Product
            {
                IsOnSale = true,
                SaleStartDate = DateTime.UtcNow.AddDays(-7),
                SaleEndDate = DateTime.UtcNow.AddDays(-1)
            };
            product.IsCurrentlyOnSale.Should().BeFalse();
        }

        [Fact]
        public void Product_ComputedIsCurrentlyOnSale_WithNullDates_ShouldBeTrue()
        {
            var product = new Product
            {
                IsOnSale = true,
                SaleStartDate = null,
                SaleEndDate = null
            };
            product.IsCurrentlyOnSale.Should().BeTrue();
        }
    }

    #endregion

    #region Product Configuration Scenarios Tests

    public class ProductConfigurationScenariosTests
    {
        [Fact]
        public void Product_PhysicalProduct_ShouldBeConfiguredCorrectly()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Laptop Pro 15",
                SKU = "LAP-PRO-15",
                ProductType = ProductType.Physical,
                Category = "Electronics",
                SubCategory = "Laptops",
                Brand = "TechBrand",
                Price = 1299.99m,
                Cost = 850,
                Quantity = 50,
                ReorderLevel = 10,
                Weight = 2.5m,
                WeightUnit = "kg",
                TrackInventory = true,
                IsTaxable = true
            };

            product.IsPhysical.Should().BeTrue();
            product.IsRecurring.Should().BeFalse();
            product.CalculatedMargin.Should().BeApproximately(34.6m, 0.1m);
        }

        [Fact]
        public void Product_SaaSSubscription_ShouldBeConfiguredCorrectly()
        {
            var product = new Product
            {
                Id = 2,
                Name = "Cloud CRM - Professional",
                SKU = "CRM-PRO",
                ProductType = ProductType.Subscription,
                IsSubscription = true,
                Category = "Software",
                ServiceTier = ServiceTier.Professional,
                BillingFrequency = BillingFrequency.Monthly,
                PricingModel = PricingModel.PerUser,
                MonthlyPrice = 49.99m,
                AnnualPrice = 499.99m, // 2 months free
                AnnualTermDiscount = 17,
                SetupFee = 199m,
                TrialPeriodDays = 14,
                AutoRenewal = true,
                RevenueRecognition = RevenueRecognitionMethod.OverTime,
                IncludedUsers = 5,
                AdditionalUserPrice = 9.99m,
                UptimeGuaranteePercent = 99.9m,
                TrackInventory = false
            };

            product.IsRecurring.Should().BeTrue();
            product.IsPhysical.Should().BeFalse();
            product.AutoRenewal.Should().BeTrue();
        }

        [Fact]
        public void Product_ConsultingService_ShouldBeConfiguredCorrectly()
        {
            var product = new Product
            {
                Id = 3,
                Name = "Technical Consulting",
                SKU = "SVC-CONSULT",
                ProductType = ProductType.Consulting,
                IsService = true,
                Category = "Services",
                UnitOfMeasure = UnitOfMeasure.Hour,
                HourlyRate = 175m,
                DailyRate = 1400m,
                MinimumBillableHours = 2,
                BillableHourIncrement = 0.25m,
                OvertimeMultiplier = 1.5m,
                WeekendMultiplier = 2.0m,
                IncludesOnsiteWork = true,
                TravelIncluded = false,
                PricingModel = PricingModel.Hourly,
                BillingFrequency = BillingFrequency.OneTime,
                TrackInventory = false
            };

            product.IsPhysical.Should().BeFalse();
            product.IsRecurring.Should().BeFalse();
        }

        [Fact]
        public void Product_ProductBundle_ShouldBeConfiguredCorrectly()
        {
            var bundleComponents = "[{\"productId\":1,\"quantity\":1},{\"productId\":2,\"quantity\":1}]";
            var product = new Product
            {
                Id = 4,
                Name = "Laptop + Support Bundle",
                SKU = "BUNDLE-LAP-SUP",
                ProductType = ProductType.Bundle,
                Category = "Bundles",
                Price = 1599.99m,
                ListPrice = 1799.99m, // Show savings
                BundleComponents = bundleComponents,
                TrackInventory = false
            };

            product.EffectivePrice.Should().Be(1599.99m);
        }

        [Fact]
        public void Product_OnSaleWithDates_ShouldReturnSalePrice()
        {
            var product = new Product
            {
                Id = 5,
                Name = "Sale Product",
                Price = 199.99m,
                SalePrice = 149.99m,
                IsOnSale = true,
                SaleStartDate = DateTime.UtcNow.AddDays(-1),
                SaleEndDate = DateTime.UtcNow.AddDays(7)
            };

            product.EffectivePrice.Should().Be(149.99m);
            product.IsCurrentlyOnSale.Should().BeTrue();
        }
    }

    #endregion

    #region Product Term Discount Scenarios Tests

    public class ProductTermDiscountScenariosTests
    {
        [Fact]
        public void Product_DefaultTermDiscounts_ShouldHaveExpectedValues()
        {
            var product = new Product();

            // Default term discounts as defined in Product entity
            product.WeeklyTermDiscount.Should().Be(0);
            product.MonthlyTermDiscount.Should().Be(0);
            product.QuarterlyTermDiscount.Should().Be(5);
            product.SemiAnnualTermDiscount.Should().Be(10);
            product.AnnualTermDiscount.Should().Be(15);
            product.TwoYearTermDiscount.Should().Be(20);
            product.ThreeYearTermDiscount.Should().Be(25);
            product.MaxTermDiscount.Should().Be(30);
        }

        [Fact]
        public void Product_DefaultTermDiscounts_ShouldFormProgressiveScale()
        {
            var product = new Product();

            // Verify progressive discount scale (defaults already form a progressive scale)
            // WeeklyTermDiscount = 0 = MonthlyTermDiscount (both 0), so start from Quarterly
            product.QuarterlyTermDiscount.Should().BeGreaterThan(product.MonthlyTermDiscount);
            product.SemiAnnualTermDiscount.Should().BeGreaterThan(product.QuarterlyTermDiscount);
            product.AnnualTermDiscount.Should().BeGreaterThan(product.SemiAnnualTermDiscount);
            product.TwoYearTermDiscount.Should().BeGreaterThan(product.AnnualTermDiscount);
            product.ThreeYearTermDiscount.Should().BeGreaterThan(product.TwoYearTermDiscount);
        }

        [Fact]
        public void Product_CustomTermDiscounts_ShouldOverrideDefaults()
        {
            var product = new Product
            {
                MonthlyTermDiscount = 5,
                QuarterlyTermDiscount = 10,
                AnnualTermDiscount = 25,
                TwoYearTermDiscount = 35,
                ThreeYearTermDiscount = 40
            };

            product.MonthlyTermDiscount.Should().Be(5);
            product.QuarterlyTermDiscount.Should().Be(10);
            product.AnnualTermDiscount.Should().Be(25);
            product.ThreeYearTermDiscount.Should().Be(40);
        }
    }

    #endregion

    #region Product Inheritance Tests

    public class ProductInheritanceTests
    {
        [Fact]
        public void Product_InheritsFromBaseEntity_ShouldHaveBaseProperties()
        {
            var now = DateTime.UtcNow;
            var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            var product = new Product
            {
                Id = 42,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now,
                IsDeleted = false,
                RowVersion = rowVersion
            };

            product.Id.Should().Be(42);
            product.CreatedAt.Should().Be(now.AddDays(-30));
            product.UpdatedAt.Should().Be(now);
            product.IsDeleted.Should().BeFalse();
            product.RowVersion.Should().BeEquivalentTo(rowVersion);
        }
    }

    #endregion
}
