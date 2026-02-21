// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Integration;
using CRM.Core.Interfaces.ITSM;
using CRM.Core.Interfaces.Notifications;
using CRM.Core.Interfaces.Workers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Integration;
using CRM.Infrastructure.Services.ITSM;
using CRM.Infrastructure.Services.Notifications;
using CRM.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContext<CrmDbContext>();
        services.AddScoped<ICrmDbContext>(sp => sp.GetRequiredService<CrmDbContext>());
        services.AddScoped<IWorkerQueue, DbWorkerQueue>();
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.AddScoped<IEscalationProcessor, EscalationProcessor>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddHostedService<WorkerHost>();
    })
    .Build();

await host.RunAsync();
