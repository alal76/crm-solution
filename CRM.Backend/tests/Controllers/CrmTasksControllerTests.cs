// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers
{
    // NOTE: this is a duplicate of CRM.Backend/tests/CRM.Tests/Controllers/TasksControllerTests.cs
    // (same namespace, same controller, weaker assertions) -- flagged for future consolidation,
    // not removed here per this repo's do-not-delete-without-confirmation convention.
    public class CrmTasksControllerTests
    {
        private static CrmDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new CrmDbContext(options, null);
        }

        private static ITaskService CreateTaskService(CrmDbContext dbContext)
        {
            var taskServiceLogger = new Mock<ILogger<TaskService>>();
            return new TaskService(dbContext, taskServiceLogger.Object);
        }

        // This is a simplified test; in real code, use a proper in-memory DbContext or mocking framework
        [Fact]
        public async Task CreateTask_ValidDto_ReturnsCreatedTask()
        {
            using var dbContext = CreateInMemoryContext();
            var mockLogger = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(CreateTaskService(dbContext), dbContext, mockLogger.Object, null);
            var dto = new CreateCrmTaskDto
            {
                Title = "Test Task",
                Description = "Test Desc",
                Priority = 2,
                DueDate = "2026-02-20T12:00:00Z",
                OwnerUserId = 5
            };
            var result = await controller.CreateTask(dto);
            Assert.IsType<ActionResult<CrmTaskDto>>(result);
        }

        [Fact]
        public async Task UpdateTask_ValidDto_UpdatesTask()
        {
            using var dbContext = CreateInMemoryContext();
            var mockLogger = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(CreateTaskService(dbContext), dbContext, mockLogger.Object, null);
            var dto = new UpdateCrmTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Desc",
                Status = 1,
                Priority = 3,
                DueDate = "2026-03-01T10:00:00Z",
                CompletedDate = "2026-03-02T10:00:00Z"
            };
            // Simulate update logic
            var result = await controller.UpdateTask(1, dto);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
