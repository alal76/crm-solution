// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

public class WorkerControlStatusDto
{
    public string ControlState { get; set; } = "Running";
    public int MaxWorkers { get; set; }
    public DateTime Timestamp { get; set; }
}

public class UpdateWorkerMaxInstancesRequest
{
    public int MaxWorkers { get; set; }
}
