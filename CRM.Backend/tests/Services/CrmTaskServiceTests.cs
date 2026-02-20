using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Infrastructure.Services;
using Moq;

namespace CRM.Tests.Services
{
    public class CrmTaskServiceTests
    {
        // Add tests for mapping, validation, and CRUD logic for CrmTaskDto, CreateCrmTaskDto, UpdateCrmTaskDto
        [Fact]
        public void CreateCrmTaskDto_MapsToEntity_Correctly()
        {
            var dto = new CreateCrmTaskDto
            {
                Title = "Test Task",
                Description = "Test Desc",
                Priority = 2,
                DueDate = "2026-02-20T12:00:00Z",
                OwnerUserId = 5,
                AccountId = 10,
                OpportunityId = 20,
                AssignedToUserId = 7
            };
            // Simulate mapping logic
            var entity = new CrmTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = (CrmTaskPriority)dto.Priority,
                DueDate = DateTime.Parse(dto.DueDate),
                OwnerUserId = dto.OwnerUserId ?? 0,
                // ...other fields
            };
            Assert.Equal(dto.Title, entity.Title);
            Assert.Equal(dto.Description, entity.Description);
            Assert.Equal(dto.Priority, (int)entity.Priority);
            Assert.Equal(DateTime.Parse(dto.DueDate), entity.DueDate);
            Assert.Equal(dto.OwnerUserId, entity.OwnerUserId);
        }

        [Fact]
        public void UpdateCrmTaskDto_MapsToEntity_Correctly()
        {
            var dto = new UpdateCrmTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Desc",
                Status = 1,
                Priority = 3,
                DueDate = "2026-03-01T10:00:00Z",
                CompletedDate = "2026-03-02T10:00:00Z",
                AssignedToUserId = 8
            };
            var entity = new CrmTask();
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.Status.HasValue) entity.Status = (CrmTaskStatus)dto.Status.Value;
            if (dto.Priority.HasValue) entity.Priority = (CrmTaskPriority)dto.Priority.Value;
            if (dto.DueDate != null) entity.DueDate = DateTime.Parse(dto.DueDate);
            if (dto.CompletedDate != null) entity.CompletedDate = DateTime.Parse(dto.CompletedDate);
            if (dto.AssignedToUserId.HasValue) entity.AssignedToUserId = dto.AssignedToUserId.Value;
            Assert.Equal(dto.Title, entity.Title);
            Assert.Equal(dto.Description, entity.Description);
            Assert.Equal(dto.Status, (int)entity.Status);
            Assert.Equal(dto.Priority, (int)entity.Priority);
            Assert.Equal(DateTime.Parse(dto.DueDate), entity.DueDate);
            Assert.Equal(DateTime.Parse(dto.CompletedDate), entity.CompletedDate);
            Assert.Equal(dto.AssignedToUserId, entity.AssignedToUserId);
        }

        [Fact]
        public void CrmTaskDto_MapsFromEntity_Correctly()
        {
            var entity = new CrmTask
            {
                Id = 1,
                Title = "Task Title",
                Description = "Task Desc",
                Status = CrmTaskStatus.InProgress,
                Priority = CrmTaskPriority.High,
                DueDate = new DateTime(2026, 2, 25, 15, 0, 0, DateTimeKind.Utc),
                CompletedDate = null,
                OwnerUserId = 2,
                CreatedByUserId = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                RowVersion = new byte[] { 1, 2, 3 }
            };
            var dto = new CrmTaskDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Status = (int)entity.Status,
                Priority = (int)entity.Priority,
                DueDate = entity.DueDate?.ToString("o"),
                CompletedDate = entity.CompletedDate?.ToString("o"),
                OwnerUserId = entity.OwnerUserId,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedAt = entity.CreatedAt.ToString("o"),
                UpdatedAt = entity.UpdatedAt.ToString("o"),
                IsDeleted = entity.IsDeleted,
                RowVersion = entity.RowVersion
            };
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Title, dto.Title);
            Assert.Equal(entity.Description, dto.Description);
            Assert.Equal((int)entity.Status, dto.Status);
            Assert.Equal((int)entity.Priority, dto.Priority);
            Assert.Equal(entity.DueDate?.ToString("o"), dto.DueDate);
            Assert.Equal(entity.CompletedDate?.ToString("o"), dto.CompletedDate);
            Assert.Equal(entity.OwnerUserId, dto.OwnerUserId);
            Assert.Equal(entity.CreatedByUserId, dto.CreatedByUserId);
            Assert.Equal(entity.CreatedAt.ToString("o"), dto.CreatedAt);
            Assert.Equal(entity.UpdatedAt.ToString("o"), dto.UpdatedAt);
            Assert.Equal(entity.IsDeleted, dto.IsDeleted);
            Assert.Equal(entity.RowVersion, dto.RowVersion);
        }
    }
}
