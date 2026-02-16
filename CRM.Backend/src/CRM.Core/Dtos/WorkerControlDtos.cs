// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

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
