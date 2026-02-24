// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for order return management.
/// Handles the full lifecycle of order returns and refunds.
/// </summary>
public interface IOrderReturnService
{
    #region CRUD Operations

    /// <summary>Gets all order returns with optional filtering.</summary>
    Task<IEnumerable<OrderReturn>> GetAllAsync(
        int? orderId = null,
        int? accountId = null,
        OrderReturnStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an order return by ID.</summary>
    Task<OrderReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets an order return by return number.</summary>
    Task<OrderReturn?> GetByReturnNumberAsync(string returnNumber, CancellationToken cancellationToken = default);

    /// <summary>Creates a new order return.</summary>
    Task<OrderReturn> CreateAsync(CreateOrderReturnDto dto, int initiatedById, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing order return.</summary>
    Task<OrderReturn> UpdateAsync(int id, UpdateOrderReturnDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes an order return (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Workflow Operations

    /// <summary>Approves an order return request.</summary>
    Task<OrderReturn> ApproveAsync(int id, int approvedById, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>Rejects an order return request.</summary>
    Task<OrderReturn> RejectAsync(int id, int rejectedById, string reason, CancellationToken cancellationToken = default);

    /// <summary>Marks items as received.</summary>
    Task<OrderReturn> MarkReceivedAsync(int id, string? trackingNumber = null, CancellationToken cancellationToken = default);

    /// <summary>Processes the refund for an order return.</summary>
    Task<OrderReturn> ProcessRefundAsync(int id, string transactionId, CancellationToken cancellationToken = default);

    /// <summary>Completes the order return.</summary>
    Task<OrderReturn> CompleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Cancels an order return.</summary>
    Task<OrderReturn> CancelAsync(int id, string reason, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets returns for an order.</summary>
    Task<IEnumerable<OrderReturn>> GetByOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Gets returns for a customer/account.</summary>
    Task<IEnumerable<OrderReturn>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default);

    /// <summary>Gets pending returns requiring action.</summary>
    Task<IEnumerable<OrderReturn>> GetPendingReturnsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets return statistics.</summary>
    Task<OrderReturnStatisticsDto> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Generates return number.</summary>
    Task<string> GenerateReturnNumberAsync(CancellationToken cancellationToken = default);

    #endregion
}
