// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Constants;

public static class WorkerControlStates
{
    public const string Running = "Running";
    public const string Paused = "Paused";
    public const string StopRequested = "StopRequested";
    public const string RestartRequested = "RestartRequested";
    public const string Stopped = "Stopped";
}
