// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Instrumentation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for LoggingExtensions static class
/// </summary>
public class LoggingExtensionsTests
{
    private readonly Mock<ILogger> _mockLogger;

    public LoggingExtensionsTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    #region Controller Logging Tests

    [Fact]
    public void LogControllerEntry_ShouldLogInformation()
    {
        // Arrange
        var controller = "AccountsController";
        var action = "GetAll";

        // Act
        _mockLogger.Object.LogControllerEntry(controller, action);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(controller) && v.ToString()!.Contains(action)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerEntry_WithParameters_ShouldIncludeParams()
    {
        // Arrange
        var controller = "AccountsController";
        var action = "GetById";
        var parameters = new { id = 42 };

        // Act
        _mockLogger.Object.LogControllerEntry(controller, action, parameters);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("42")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerExit_ShouldLogInformation()
    {
        // Arrange
        var controller = "AccountsController";
        var action = "GetAll";
        var statusCode = 200;
        var durationMs = 150L;

        // Act
        _mockLogger.Object.LogControllerExit(controller, action, statusCode, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(controller) &&
                    v.ToString()!.Contains(action) &&
                    v.ToString()!.Contains("200")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerError_ShouldLogError()
    {
        // Arrange
        var controller = "AccountsController";
        var action = "Create";
        var exception = new InvalidOperationException("Test error");

        // Act
        _mockLogger.Object.LogControllerError(controller, action, exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(controller) &&
                    v.ToString()!.Contains(action)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Service Logging Tests

    [Fact]
    public void LogServiceOperation_ShouldLogDebug()
    {
        // Arrange
        var service = "AccountService";
        var operation = "GetAllAsync";

        // Act
        _mockLogger.Object.LogServiceOperation(service, operation);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains(operation)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceSuccess_ShouldLogDebug()
    {
        // Arrange
        var service = "AccountService";
        var operation = "CreateAsync";
        var result = new { id = 123 };

        // Act
        _mockLogger.Object.LogServiceSuccess(service, operation, result);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains("completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceError_ShouldLogError()
    {
        // Arrange
        var service = "AccountService";
        var operation = "UpdateAsync";
        var exception = new ArgumentException("Invalid argument");

        // Act
        _mockLogger.Object.LogServiceError(service, operation, exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceWarning_ShouldLogWarning()
    {
        // Arrange
        var service = "AccountService";
        var operation = "SyncAsync";
        var warning = "Rate limit approaching";

        // Act
        _mockLogger.Object.LogServiceWarning(service, operation, warning);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains(warning)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Database Logging Tests

    [Fact]
    public void LogDatabaseQuery_ShouldLogDebug()
    {
        // Arrange
        var table = "Accounts";
        var operation = "SELECT";

        // Act
        _mockLogger.Object.LogDatabaseQuery(table, operation);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(table) &&
                    v.ToString()!.Contains(operation)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseQueryResult_ShouldIncludeRecordCount()
    {
        // Arrange
        var table = "Accounts";
        var operation = "SELECT";
        var recordCount = 42;
        var durationMs = 25L;

        // Act
        _mockLogger.Object.LogDatabaseQueryResult(table, operation, recordCount, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("42") &&
                    v.ToString()!.Contains("25")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseError_ShouldLogError()
    {
        // Arrange
        var table = "Accounts";
        var operation = "INSERT";
        var exception = new Exception("Connection failed");

        // Act
        _mockLogger.Object.LogDatabaseError(table, operation, exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(table) &&
                    v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseSlowQuery_ShouldLogWarning()
    {
        // Arrange
        var table = "Accounts";
        var operation = "SELECT";
        var durationMs = 5000L;

        // Act
        _mockLogger.Object.LogDatabaseSlowQuery(table, operation, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("SLOW") &&
                    v.ToString()!.Contains("5000")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Authentication Logging Tests

    [Fact]
    public void LogAuthAttempt_ShouldLogInformation()
    {
        // Arrange
        var username = "testuser";
        var method = "Password";

        // Act
        _mockLogger.Object.LogAuthAttempt(username, method);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(username) &&
                    v.ToString()!.Contains(method)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthSuccess_ShouldLogInformation()
    {
        // Arrange
        var username = "testuser";
        var userId = 42;

        // Act
        _mockLogger.Object.LogAuthSuccess(username, userId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(username) &&
                    v.ToString()!.Contains("42") &&
                    v.ToString()!.Contains("successful")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthFailure_ShouldLogWarning()
    {
        // Arrange
        var username = "testuser";
        var reason = "Invalid password";

        // Act
        _mockLogger.Object.LogAuthFailure(username, reason);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(username) &&
                    v.ToString()!.Contains(reason)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthLogout_ShouldLogInformation()
    {
        // Arrange
        var username = "testuser";
        var userId = 42;

        // Act
        _mockLogger.Object.LogAuthLogout(username, userId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(username) &&
                    v.ToString()!.Contains("Logout")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Performance Logging Tests

    [Fact]
    public void LogPerformanceMetric_ShouldLogDebug()
    {
        // Arrange
        var metric = "ResponseTime";
        var value = 125.5;
        var unit = "ms";

        // Act
        _mockLogger.Object.LogPerformanceMetric(metric, value, unit);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(metric) &&
                    v.ToString()!.Contains("125.5")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogPerformanceWarning_ShouldLogWarning()
    {
        // Arrange
        var operation = "DatabaseQuery";
        var thresholdMs = 1000L;
        var actualMs = 2500L;

        // Act
        _mockLogger.Object.LogPerformanceWarning(operation, thresholdMs, actualMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(operation) &&
                    v.ToString()!.Contains("exceeded")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Business Logic Logging Tests

    [Fact]
    public void LogBusinessEvent_ShouldLogInformation()
    {
        // Arrange
        var eventType = "AccountCreated";
        var entity = "Account";
        var entityId = 42;

        // Act
        _mockLogger.Object.LogBusinessEvent(eventType, entity, entityId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(eventType) &&
                    v.ToString()!.Contains(entity) &&
                    v.ToString()!.Contains("42")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogBusinessEvent_WithoutEntityId_ShouldShowNA()
    {
        // Arrange
        var eventType = "SystemStartup";
        var entity = "System";

        // Act
        _mockLogger.Object.LogBusinessEvent(eventType, entity);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(eventType) &&
                    v.ToString()!.Contains("N/A")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWorkflowExecution_ShouldLogInformation()
    {
        // Arrange
        var workflowName = "LeadQualification";
        var workflowId = 1;
        var status = "Completed";

        // Act
        _mockLogger.Object.LogWorkflowExecution(workflowName, workflowId, status);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(workflowName) &&
                    v.ToString()!.Contains(status)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCommunication_ShouldLogInformation()
    {
        // Arrange
        var channelType = "Email";
        var direction = "Outbound";
        var recipient = "customer@example.com";
        var status = "Sent";

        // Act
        _mockLogger.Object.LogCommunication(channelType, direction, recipient, status);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(channelType) &&
                    v.ToString()!.Contains(recipient) &&
                    v.ToString()!.Contains(status)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Logging Tests

    [Fact]
    public void LogExternalCall_ShouldLogDebug()
    {
        // Arrange
        var service = "PaymentGateway";
        var endpoint = "/api/charge";
        var method = "POST";

        // Act
        _mockLogger.Object.LogExternalCall(service, endpoint, method);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains(method)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogExternalResponse_ShouldLogDebug()
    {
        // Arrange
        var service = "PaymentGateway";
        var statusCode = 200;
        var durationMs = 350L;

        // Act
        _mockLogger.Object.LogExternalResponse(service, statusCode, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(service) &&
                    v.ToString()!.Contains("200")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region System Logging Tests

    [Fact]
    public void LogSystemStartup_ShouldLogInformation()
    {
        // Arrange
        var component = "CRM.Api";
        var version = "3.12.0";
        var environment = "Production";

        // Act
        _mockLogger.Object.LogSystemStartup(component, version, environment);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(component) &&
                    v.ToString()!.Contains(version) &&
                    v.ToString()!.Contains(environment)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogSystemShutdown_ShouldLogInformation()
    {
        // Arrange
        var component = "CRM.Api";

        // Act
        _mockLogger.Object.LogSystemShutdown(component);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(component) &&
                    v.ToString()!.Contains("shutting down")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(true, LogLevel.Information)]
    [InlineData(false, LogLevel.Warning)]
    public void LogHealthCheck_ShouldLogAppropriateLevel(bool isHealthy, LogLevel expectedLevel)
    {
        // Arrange
        var component = "Database";
        var details = "Connection OK";

        // Act
        _mockLogger.Object.LogHealthCheck(component, isHealthy, details);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(component)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void LogControllerEntry_WithNullParameters_ShouldNotThrow()
    {
        // Arrange
        var controller = "AccountsController";
        var action = "GetAll";

        // Act
        var act = () => _mockLogger.Object.LogControllerEntry(controller, action, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogServiceOperation_WithNullContext_ShouldNotThrow()
    {
        // Arrange
        var service = "AccountService";
        var operation = "GetAllAsync";

        // Act
        var act = () => _mockLogger.Object.LogServiceOperation(service, operation, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogDatabaseQuery_WithNullParameters_ShouldNotThrow()
    {
        // Arrange
        var table = "Accounts";
        var operation = "SELECT";

        // Act
        var act = () => _mockLogger.Object.LogDatabaseQuery(table, operation, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogBusinessEvent_WithNullDetails_ShouldNotThrow()
    {
        // Arrange
        var eventType = "AccountCreated";
        var entity = "Account";

        // Act
        var act = () => _mockLogger.Object.LogBusinessEvent(eventType, entity, 42, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogHealthCheck_WithNullDetails_ShouldNotThrow()
    {
        // Arrange
        var component = "Database";

        // Act
        var act = () => _mockLogger.Object.LogHealthCheck(component, true, null);

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
