// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Ports.Output.Messaging;

/// <summary>
/// Redis-specific message queue and cache provider interface.
/// </summary>
/// <remarks>
/// Deprecated: use <see cref="IDistributedStreamPort"/> which uses technology-neutral naming.
/// </remarks>
[Obsolete("Use IDistributedStreamPort instead. IRedisProvider leaks Redis-specific naming into the core port layer.")]
public interface IRedisProvider : IDistributedStreamPort { }
