// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IOrderService for order management operations.
/// Handles order lifecycle from creation to fulfillment.
/// </summary>
public class OrderService : IOrderService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<OrderService> _logger;
    private readonly IEntityEventDispatcher _eventDispatcher;

    public OrderService(ICrmDbContext context, ILogger<OrderService> logger, IEntityEventDispatcher eventDispatcher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }


    /// <summary>
    /// Loads a raw Order entity (with line items) from the database.
    /// Used internally by status management and fulfillment methods.
    /// </summary>
    private async Task<Order?> GetOrderEntityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<OrderDto>> GetAllAsync(
        int? accountId = null,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Account)
            .Include(o => o.LineItems)
            .Where(o => !o.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(o => o.AccountId == accountId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync(cancellationToken);
        return orders.Select(MapToOrderDto).ToList();
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Account)
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
        return order == null ? null : MapToOrderDto(order);
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Account)
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);
        return order == null ? null : MapToOrderDto(order);
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = MapFromCreateOrderDto(dto);
        if (string.IsNullOrEmpty(order.OrderNumber))
        {
            order.OrderNumber = await GenerateOrderNumberAsync(cancellationToken);
        }
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created order {OrderNumber} for account {AccountId}", order.OrderNumber, order.AccountId);
        await _eventDispatcher.DispatchEntityEventAsync("Order", order.Id, WorkflowTriggerType.OnCreate);
        return MapToOrderDto(order);
    }

    /// <inheritdoc />
    public async Task<OrderDto> UpdateAsync(UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == dto.Id && !o.IsDeleted, cancellationToken);
        if (existing == null)
        {
            throw new InvalidOperationException($"Order {dto.Id} not found");
        }
        MapUpdateOrderDtoToEntity(dto, existing);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated order {OrderNumber}", existing.OrderNumber);
        await _eventDispatcher.DispatchEntityEventAsync("Order", existing.Id, WorkflowTriggerType.OnUpdate);
        return MapToOrderDto(existing);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order == null || order.IsDeleted)
        {
            return false;
        }

        order.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted order {OrderNumber}", order.OrderNumber);

        // Fire workflow triggers for entity deletion
        await _eventDispatcher.DispatchEntityEventAsync("Order", id, WorkflowTriggerType.OnDelete);

        return true;
    }

    #endregion

    #region Order Operations

    /// <inheritdoc />
    public async Task<OrderDto> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId && !q.IsDeleted, cancellationToken);

        if (quote == null)
        {
            throw new InvalidOperationException($"Quote {quoteId} not found");
        }

        var order = /* ...existing logic... */ await CreateFromQuoteInternal(quoteId, cancellationToken);
        return MapToOrderDto(order);
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateFromOpportunityAsync(int opportunityId, CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
        {
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");
        }

        var order = /* ...existing logic... */ await CreateFromOpportunityInternal(opportunityId, cancellationToken);
        return MapToOrderDto(order);
    }

    // --- Mapping helpers ---
    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            // Identification
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ExternalOrderId = order.ExternalOrderId,
            CustomerPONumber = order.CustomerPONumber,
            ReferenceNumber = order.ReferenceNumber,

            // Order Details
            Name = order.Name,
            Description = order.Description,
            Status = (int)order.Status,
            OrderType = (int)order.OrderType,
            FulfillmentMethod = (int)order.FulfillmentMethod,
            Priority = (int)order.Priority,

            // Dates
            OrderDate = order.OrderDate.ToString("o"),
            ApprovedDate = order.ApprovedDate?.ToString("o"),
            RequestedDeliveryDate = order.RequestedDeliveryDate?.ToString("o"),
            PromisedDeliveryDate = order.PromisedDeliveryDate?.ToString("o"),
            ShippedDate = order.ShippedDate?.ToString("o"),
            DeliveredDate = order.DeliveredDate?.ToString("o"),
            CompletedDate = order.CompletedDate?.ToString("o"),
            CancelledDate = order.CancelledDate?.ToString("o"),
            ContractStartDate = order.ContractStartDate?.ToString("o"),
            ContractEndDate = order.ContractEndDate?.ToString("o"),

            // Pricing
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            DiscountPercent = order.DiscountPercent,
            DiscountReason = order.DiscountReason,
            TaxAmount = order.TaxAmount,
            TaxRate = order.TaxRate,
            ShippingAmount = order.ShippingAmount,
            HandlingAmount = order.HandlingAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode,
            ExchangeRate = order.ExchangeRate,
            BaseCurrencyAmount = order.BaseCurrencyAmount,

            // Revenue Recognition
            MRR = order.MRR,
            ARR = order.ARR,
            TCV = order.TCV,
            ACV = order.ACV,
            OneTimeRevenue = order.OneTimeRevenue,
            RecurringRevenue = order.RecurringRevenue,

            // Billing Address
            BillingName = order.BillingName,
            BillingCompany = order.BillingCompany,
            BillingStreet = order.BillingStreet,
            BillingCity = order.BillingCity,
            BillingState = order.BillingState,
            BillingPostalCode = order.BillingPostalCode,
            BillingCountry = order.BillingCountry,
            BillingPhone = order.BillingPhone,
            BillingEmail = order.BillingEmail,

            // Shipping Address
            ShippingName = order.ShippingName,
            ShippingCompany = order.ShippingCompany,
            ShippingStreet = order.ShippingStreet,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingPhone = order.ShippingPhone,
            ShippingEmail = order.ShippingEmail,
            ShippingInstructions = order.ShippingInstructions,

            // Shipping Details
            ShippingMethod = order.ShippingMethod,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            ShippingWeight = order.ShippingWeight,
            PackageCount = order.PackageCount,

            // Payment
            PaymentTerms = order.PaymentTerms,
            PaymentMethod = order.PaymentMethod,
            AmountInvoiced = order.AmountInvoiced,
            AmountPaid = order.AmountPaid,
            BalanceDue = order.BalanceDue,
            IsPaid = order.IsPaid,

            // Relationships
            QuoteId = order.QuoteId,
            AccountId = order.AccountId,
            ContactId = order.ContactId,
            OpportunityId = order.OpportunityId,
            OwnerId = order.OwnerId,
            ApprovedById = order.ApprovedById,
            ParentOrderId = order.ParentOrderId,

            // Notes & Attachments
            InternalNotes = order.InternalNotes,
            SpecialInstructions = order.SpecialInstructions,
            CancellationReason = order.CancellationReason,
            TermsAndConditions = order.TermsAndConditions,

            // Workflow Dates
            SubmittedDate = order.SubmittedDate?.ToString("o"),
            FulfilledDate = order.FulfilledDate?.ToString("o"),

            // Hold Status
            HoldReason = order.HoldReason,
            HoldDate = order.HoldDate?.ToString("o"),

            // Rejection
            RejectionReason = order.RejectionReason,

            // Return Information
            ReturnReason = order.ReturnReason,

            // Discount Codes
            DiscountCode = order.DiscountCode,
            CouponCode = order.CouponCode,

            // Audit Fields
            Source = order.Source,
            SourceIpAddress = order.SourceIpAddress,

            // Audit
            CreatedAt = order.CreatedAt.ToString("o"),
            UpdatedAt = order.UpdatedAt?.ToString("o") ?? string.Empty,
            IsDeleted = order.IsDeleted,
            RowVersion = order.RowVersion ?? Array.Empty<byte>(),

            // Line Items
            LineItems = order.LineItems?.Where(li => !li.IsDeleted).Select(li => new OrderLineItemDto
            {
                Id = li.Id,
                LineNumber = li.LineNumber,
                ProductId = li.ProductId,
                Name = li.Name,
                Description = li.Description,
                SKU = li.SKU,
                ProductCode = li.ProductCode,
                Quantity = li.Quantity,
                UnitOfMeasure = li.UnitOfMeasure,
                UnitPrice = li.UnitPrice,
                DiscountAmount = li.DiscountAmount,
                DiscountPercent = li.DiscountPercent,
                ExtendedAmount = li.ExtendedAmount,
                TaxAmount = li.TaxAmount,
                TotalAmount = li.TotalAmount,
                Notes = li.Notes
            }).ToList() ?? new List<OrderLineItemDto>(),

            // Invoice and Subscription IDs
            InvoiceIds = order.Invoices?.Where(i => !i.IsDeleted).Select(i => i.Id).ToList() ?? new List<int>(),
            SubscriptionIds = order.Subscriptions?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? new List<int>()
        };
    }

    private static Order MapFromCreateOrderDto(CreateOrderDto dto)
    {
        var order = new Order
        {
            AccountId = dto.AccountId,
            ContactId = dto.ContactId,
            Name = dto.Name,
            Description = dto.Description,
            OrderType = (OrderType)dto.OrderType,
            FulfillmentMethod = (FulfillmentMethod)dto.FulfillmentMethod,
            Priority = (OrderPriority)dto.Priority,
            OrderDate = DateTime.Parse(dto.OrderDate),
            // ...map other fields as needed...
            LineItems = dto.LineItems?.Select(li => new OrderLineItem
            {
                ProductId = li.ProductId,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TotalAmount = li.Quantity * li.UnitPrice - li.DiscountAmount
            }).ToList() ?? new List<OrderLineItem>()
        };
        return order;
    }

    private static void MapUpdateOrderDtoToEntity(UpdateOrderDto dto, Order entity)
    {
        if (dto.Name != null)
        {
            entity.Name = dto.Name;
        }
        if (dto.Description != null)
        {
            entity.Description = dto.Description;
        }
        if (dto.Status.HasValue)
        {
            entity.Status = (OrderStatus)dto.Status.Value;
        }
        if (dto.OrderType.HasValue)
        {
            entity.OrderType = (OrderType)dto.OrderType.Value;
        }
        if (dto.FulfillmentMethod.HasValue)
        {
            entity.FulfillmentMethod = (FulfillmentMethod)dto.FulfillmentMethod.Value;
        }
        if (dto.Priority.HasValue)
        {
            entity.Priority = (OrderPriority)dto.Priority.Value;
        }
        // ...map other updatable fields as needed...
        // Optionally update line items, etc.
    }

    // Internal helpers for quote/opportunity creation
    private async Task<Order> CreateFromQuoteInternal(int quoteId, CancellationToken cancellationToken)
    {
        // ...existing logic from previous CreateFromQuoteAsync...
        var quote = await _context.Quotes
            .Include(q => q.QuoteLineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId && !q.IsDeleted, cancellationToken);
        if (quote == null)
        {
            throw new InvalidOperationException($"Quote {quoteId} not found");
        }
        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(cancellationToken),
            AccountId = quote.AccountId ?? 0,
            ContactId = quote.ContactId,
            QuoteId = quoteId,
            OpportunityId = quote.OpportunityId,
            Status = OrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            Subtotal = quote.Subtotal,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            CurrencyCode = quote.CurrencyCode ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        int lineNumber = 1;
        foreach (var quoteLine in (quote.QuoteLineItems ?? Enumerable.Empty<QuoteLineItem>()).Where(l => !l.IsDeleted))
        {
            order.LineItems.Add(new OrderLineItem
            {
                LineNumber = lineNumber++,
                ProductId = quoteLine.ProductId,
                Description = quoteLine.Description ?? string.Empty,
                Quantity = quoteLine.Quantity,
                UnitPrice = quoteLine.UnitPrice,
                DiscountAmount = quoteLine.TotalDiscount,
                TotalAmount = quoteLine.Total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created order {OrderNumber} from quote {QuoteId}", order.OrderNumber, quoteId);
        return order;
    }

    private async Task<Order> CreateFromOpportunityInternal(int opportunityId, CancellationToken cancellationToken)
    {
        // ...existing logic from previous CreateFromOpportunityAsync...
        var opportunity = await _context.Opportunities
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);
        if (opportunity == null)
        {
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");
        }
        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(cancellationToken),
            AccountId = opportunity.AccountId,
            OpportunityId = opportunityId,
            Status = OrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            TotalAmount = opportunity.EstimatedValue,
            CurrencyCode = "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        int lineNumber = 1;
        foreach (var oppProduct in opportunity.Products.Where(p => !p.IsDeleted))
        {
            order.LineItems.Add(new OrderLineItem
            {
                LineNumber = lineNumber++,
                ProductId = oppProduct.ProductId,
                Quantity = oppProduct.Quantity,
                UnitPrice = oppProduct.UnitPrice ?? 0,
                TotalAmount = oppProduct.TotalPrice ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        order.Subtotal = order.LineItems.Sum(l => l.TotalAmount);
        order.TotalAmount = order.Subtotal;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created order {OrderNumber} from opportunity {OpportunityId}", order.OrderNumber, opportunityId);
        return order;
    }

    /// <inheritdoc />
    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = "ORD";
        var year = DateTime.UtcNow.ToString("yy");
        var month = DateTime.UtcNow.ToString("MM");

        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith($"{prefix}-{year}{month}"))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastOrder != null)
        {
            var parts = lastOrder.OrderNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}-{year}{month}-{sequence:D4}";
    }

    /// <inheritdoc />
    public async Task<Order> CloneOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var original = await GetOrderEntityAsync(orderId, cancellationToken);
        if (original == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var clone = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(cancellationToken),
            AccountId = original.AccountId,
            ContactId = original.ContactId,
            Status = OrderStatus.Draft,
            OrderType = original.OrderType,
            OrderDate = DateTime.UtcNow,
            Subtotal = original.Subtotal,
            TaxAmount = original.TaxAmount,
            DiscountAmount = original.DiscountAmount,
            TotalAmount = original.TotalAmount,
            CurrencyCode = original.CurrencyCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var line in original.LineItems.Where(l => !l.IsDeleted))
        {
            clone.LineItems.Add(new OrderLineItem
            {
                LineNumber = line.LineNumber,
                ProductId = line.ProductId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = line.TaxAmount,
                TotalAmount = line.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Orders.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cloned order {OriginalNumber} to {NewNumber}", original.OrderNumber, clone.OrderNumber);
        return clone;
    }

    #endregion

    #region Status Management

    /// <inheritdoc />
    public async Task<Order> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = status;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} status updated to {Status}", order.OrderNumber, status);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> SubmitForApprovalAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        if (order.Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException($"Order must be in Draft status to submit for approval");
        }

        order.Status = OrderStatus.PendingApproval;
        order.SubmittedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} submitted for approval", order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> ApproveAsync(int orderId, int approvedById, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = OrderStatus.Approved;
        order.ApprovedById = approvedById;
        order.ApprovedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} approved by user {UserId}", order.OrderNumber, approvedById);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> RejectAsync(int orderId, int rejectedById, string reason, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = OrderStatus.Cancelled;
        order.RejectionReason = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} rejected by user {UserId}: {Reason}", order.OrderNumber, rejectedById, reason);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> CancelAsync(int orderId, string reason, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        if (order.Status == OrderStatus.Fulfilled || order.Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Cannot cancel a fulfilled or delivered order");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledDate = DateTime.UtcNow;
        order.CancellationReason = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled: {Reason}", order.OrderNumber, reason);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> PutOnHoldAsync(int orderId, string reason, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = OrderStatus.OnHold;
        order.HoldReason = reason;
        order.HoldDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} put on hold: {Reason}", order.OrderNumber, reason);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> ReleaseFromHoldAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        if (order.Status != OrderStatus.OnHold)
        {
            throw new InvalidOperationException("Order is not on hold");
        }

        order.Status = OrderStatus.Processing;
        order.HoldReason = null;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} released from hold", order.OrderNumber);
        return order;
    }

    #endregion

    #region Fulfillment

    /// <inheritdoc />
    public async Task<Order> MarkAsFulfilledAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = OrderStatus.Fulfilled;
        order.FulfilledDate = DateTime.UtcNow;

        // Mark all line items as fulfilled
        foreach (var line in order.LineItems.Where(l => !l.IsDeleted))
        {
            line.FulfilledQuantity = line.Quantity;
            line.FulfilledDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} marked as fulfilled", order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> MarkAsPartiallyFulfilledAsync(int orderId, IEnumerable<int> fulfilledLineItemIds, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var fulfilledIds = fulfilledLineItemIds.ToHashSet();
        foreach (var line in order.LineItems.Where(l => !l.IsDeleted && fulfilledIds.Contains(l.Id)))
        {
            line.FulfilledQuantity = line.Quantity;
            line.FulfilledDate = DateTime.UtcNow;
        }

        order.Status = OrderStatus.PartiallyFulfilled;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} marked as partially fulfilled", order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> MarkAsDeliveredAsync(int orderId, DateTime? deliveryDate = null, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.Status = OrderStatus.Delivered;
        order.DeliveredDate = deliveryDate ?? DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} marked as delivered", order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> ProcessReturnAsync(int orderId, IEnumerable<OrderReturnItem> returnItems, string reason, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        foreach (var returnItem in returnItems)
        {
            var line = order.LineItems.FirstOrDefault(l => l.Id == returnItem.LineItemId && !l.IsDeleted);
            if (line != null)
            {
                line.ReturnedQuantity = line.ReturnedQuantity + returnItem.Quantity;
                line.ReturnReason = returnItem.Reason;
            }
        }

        // Check if all items are returned
        var allReturned = order.LineItems.All(l => l.IsDeleted || l.ReturnedQuantity >= l.Quantity);
        order.Status = allReturned ? OrderStatus.Returned : OrderStatus.PartiallyFulfilled;
        order.ReturnReason = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Processed return for order {OrderNumber}: {Reason}", order.OrderNumber, reason);
        return order;
    }

    #endregion

    #region Line Items

    /// <inheritdoc />
    public async Task<OrderLineItem> AddLineItemAsync(int orderId, OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        lineItem.OrderId = orderId;
        lineItem.CreatedAt = DateTime.UtcNow;

        if (lineItem.LineNumber == 0)
        {
            lineItem.LineNumber = (order.LineItems.Max(l => (int?)l.LineNumber) ?? 0) + 1;
        }

        _context.OrderLineItems.Add(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateTotalsAsync(orderId, cancellationToken);

        return lineItem;
    }

    /// <inheritdoc />
    public async Task<OrderLineItem> UpdateLineItemAsync(OrderLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var existing = await _context.OrderLineItems.FindAsync(new object[] { lineItem.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Line item {lineItem.Id} not found");
        }

        _context.OrderLineItems.Update(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateTotalsAsync(lineItem.OrderId, cancellationToken);

        return lineItem;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default)
    {
        var lineItem = await _context.OrderLineItems.FindAsync(new object[] { lineItemId }, cancellationToken);
        if (lineItem == null || lineItem.IsDeleted)
        {
            return false;
        }

        var orderId = lineItem.OrderId;
        lineItem.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateTotalsAsync(orderId, cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderLineItem>> GetLineItemsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderLineItems
            .Where(l => l.OrderId == orderId && !l.IsDeleted)
            .OrderBy(l => l.LineNumber)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Queries

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && o.OrderDate >= fromDate && o.OrderDate <= toDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetOrdersRequiringActionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && (
                o.Status == OrderStatus.OnHold ||
                o.Status == OrderStatus.ActionRequired ||
                o.Status == OrderStatus.PendingApproval))
            .OrderBy(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrderStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.Where(o => !o.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);
        var fulfilledOrders = orders.Where(o => o.Status == OrderStatus.Fulfilled || o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).ToList();

        return new OrderStatistics
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Draft || o.Status == OrderStatus.PendingApproval),
            ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing || o.Status == OrderStatus.Approved),
            FulfilledOrders = fulfilledOrders.Count,
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = fulfilledOrders.Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0,
            FulfillmentRate = orders.Count > 0 ? (double)fulfilledOrders.Count / orders.Count * 100 : 0,
            AverageFulfillmentTime = fulfilledOrders.Where(o => o.FulfilledDate.HasValue)
                .Select(o => (o.FulfilledDate!.Value - o.OrderDate).TotalDays)
                .DefaultIfEmpty(0)
                .Average()
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _context.Orders
            .Include(o => o.Account)
            .Where(o => !o.IsDeleted && (
                o.OrderNumber.ToLower().Contains(term) ||
                (o.Account != null && (
                    (o.Account.Company != null && o.Account.Company.ToLower().Contains(term)) ||
                    o.Account.Email.ToLower().Contains(term)))))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Calculations

    /// <inheritdoc />
    public async Task<Order> RecalculateTotalsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var activeLines = order.LineItems.Where(l => !l.IsDeleted).ToList();

        order.Subtotal = activeLines.Sum(l => l.Quantity * l.UnitPrice);
        order.DiscountAmount = activeLines.Sum(l => l.DiscountAmount);
        order.TaxAmount = activeLines.Sum(l => l.TaxAmount);
        order.TotalAmount = order.Subtotal - order.DiscountAmount + order.TaxAmount + order.ShippingAmount;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recalculated totals for order {OrderNumber}", order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> ApplyDiscountAsync(int orderId, decimal discountAmount, string? discountCode = null, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.DiscountAmount += discountAmount;
        order.DiscountCode = discountCode;
        order.TotalAmount = order.Subtotal - order.DiscountAmount + order.TaxAmount + order.ShippingAmount;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Applied discount of {Amount} to order {OrderNumber}", discountAmount, order.OrderNumber);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> ApplyCouponAsync(int orderId, string couponCode, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        // In production, validate coupon and calculate discount
        // For now, just store the coupon code
        order.CouponCode = couponCode;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Applied coupon {CouponCode} to order {OrderNumber}", couponCode, order.OrderNumber);
        return order;
    }

    #endregion

    #region Invoicing

    /// <inheritdoc />
    public async Task<Invoice> CreateInvoiceAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyMM}-{orderId:D4}",
            AccountId = order.AccountId,
            OrderId = orderId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Copy line items
        foreach (var orderLine in order.LineItems.Where(l => !l.IsDeleted))
        {
            invoice.LineItems.Add(new InvoiceLineItem
            {
                LineNumber = orderLine.LineNumber,
                ProductId = orderLine.ProductId,
                Description = orderLine.Description ?? string.Empty,
                Quantity = orderLine.Quantity,
                UnitPrice = orderLine.UnitPrice,
                DiscountAmount = orderLine.DiscountAmount,
                TaxAmount = orderLine.TaxAmount,
                TotalAmount = orderLine.TotalAmount,
                OrderLineItemId = orderLine.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created invoice {InvoiceNumber} for order {OrderNumber}", invoice.InvoiceNumber, order.OrderNumber);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Invoice>> GetInvoicesAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.OrderId == orderId && !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    #endregion
}
