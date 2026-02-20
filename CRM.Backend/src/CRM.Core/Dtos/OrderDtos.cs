using System;
using System.Collections.Generic;

namespace CRM.Core.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int AccountId { get; set; }
        public int? ContactId { get; set; }
        public int Status { get; set; }
        public string OrderDate { get; set; }
        public string? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public string? Notes { get; set; }
        public List<OrderLineItemDto> LineItems { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class CreateOrderDto
    {
        public int AccountId { get; set; }
        public int? ContactId { get; set; }
        public string OrderDate { get; set; }
        public string? DueDate { get; set; }
        public string Currency { get; set; }
        public string? Notes { get; set; }
        public List<CreateOrderLineItemDto> LineItems { get; set; }
    }

    public class UpdateOrderDto
    {
        public int Status { get; set; }
        public string? DueDate { get; set; }
        public string? Notes { get; set; }
        public List<UpdateOrderLineItemDto> LineItems { get; set; }
    }

    public class OrderLineItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    public class CreateOrderLineItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
    }

    public class UpdateOrderLineItemDto
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
    }
}
