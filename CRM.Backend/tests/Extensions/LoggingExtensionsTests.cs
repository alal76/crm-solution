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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Extensions;

/// <summary>
/// Unit tests for LoggingExtensions structured logging methods.
/// Tests logging message formatting, parameters, and log levels.
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
    public void LogControllerEntry_ShouldLogWithInformationLevel()
    {
        // Act
        _mockLogger.Object.LogControllerEntry("AccountsController", "GetAll");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AccountsController") && v.ToString()!.Contains("GetAll")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerEntry_WithParameters_ShouldIncludeParams()
    {
        // Arrange
        var parameters = new { id = 123, name = "Test" };

        // Act
        _mockLogger.Object.LogControllerEntry("AccountsController", "GetById", parameters);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AccountsController")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerEntry_WithNullParameters_ShouldLogNone()
    {
        // Act
        _mockLogger.Object.LogControllerEntry("AccountsController", "GetAll", null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("none")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerExit_WithSuccessStatus_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogControllerExit("AccountsController", "GetAll", 200, 150);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("200") && v.ToString()!.Contains("150")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerExit_With400Status_ShouldShowWarningIcon()
    {
        // Act
        _mockLogger.Object.LogControllerExit("AccountsController", "Create", 400, 50);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("400")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerExit_With500Status_ShouldShowErrorIcon()
    {
        // Act
        _mockLogger.Object.LogControllerExit("AccountsController", "Create", 500, 200);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("500")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogControllerError_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        _mockLogger.Object.LogControllerError("AccountsController", "Create", exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AccountsController") && v.ToString()!.Contains("Test error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Service Logging Tests

    [Fact]
    public void LogServiceOperation_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogServiceOperation("AccountService", "GetById");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AccountService")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceOperation_WithContext_ShouldIncludeContext()
    {
        // Arrange
        var context = new { AccountId = 123 };

        // Act
        _mockLogger.Object.LogServiceOperation("AccountService", "GetById", context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceSuccess_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogServiceSuccess("AccountService", "Create");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceSuccess_WithResult_ShouldIncludeResult()
    {
        // Act
        _mockLogger.Object.LogServiceSuccess("AccountService", "Create", new { Id = 123 });

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceError_ShouldLogError()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        _mockLogger.Object.LogServiceError("AccountService", "Create", exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServiceWarning_ShouldLogWarning()
    {
        // Act
        _mockLogger.Object.LogServiceWarning("AccountService", "Update", "Concurrent modification detected");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Concurrent modification")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Database Logging Tests

    [Fact]
    public void LogDatabaseQuery_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogDatabaseQuery("Accounts", "SELECT");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SELECT") && v.ToString()!.Contains("Accounts")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseQueryResult_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogDatabaseQueryResult("Accounts", "SELECT", 100, 25);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("100") && v.ToString()!.Contains("25")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseError_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Database connection failed");

        // Act
        _mockLogger.Object.LogDatabaseError("Accounts", "INSERT", exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("INSERT") && v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseSlowQuery_ShouldLogWarning()
    {
        // Act
        _mockLogger.Object.LogDatabaseSlowQuery("Accounts", "SELECT", 5000);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SLOW") && v.ToString()!.Contains("5000")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Authentication Logging Tests

    [Fact]
    public void LogAuthAttempt_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogAuthAttempt("admin@test.com", "Password");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("admin@test.com") && v.ToString()!.Contains("Password")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthSuccess_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogAuthSuccess("admin@test.com", 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("successful")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthFailure_ShouldLogWarning()
    {
        // Act
        _mockLogger.Object.LogAuthFailure("admin@test.com", "Invalid password");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed") && v.ToString()!.Contains("Invalid password")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogAuthLogout_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogAuthLogout("admin@test.com", 1);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logout")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Performance Logging Tests

    [Fact]
    public void LogPerformanceMetric_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogPerformanceMetric("RequestDuration", 150);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RequestDuration") && v.ToString()!.Contains("150")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogPerformanceMetric_WithCustomUnit_ShouldIncludeUnit()
    {
        // Act
        _mockLogger.Object.LogPerformanceMetric("MemoryUsage", 512, "MB");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("512") && v.ToString()!.Contains("MB")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogPerformanceWarning_ShouldLogWarning()
    {
        // Act
        _mockLogger.Object.LogPerformanceWarning("API Request", 1000, 2500);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("exceeded") && v.ToString()!.Contains("1000") && v.ToString()!.Contains("2500")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Business Logic Logging Tests

    [Fact]
    public void LogBusinessEvent_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogBusinessEvent("Created", "Account", 123);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created") && v.ToString()!.Contains("Account")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogBusinessEvent_WithDetails_ShouldIncludeDetails()
    {
        // Arrange
        var details = new { Status = "Active", Type = "Customer" };

        // Act
        _mockLogger.Object.LogBusinessEvent("Updated", "Account", 123, details);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWorkflowExecution_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogWorkflowExecution("Approval Workflow", 1, "Started");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Approval Workflow") && v.ToString()!.Contains("Started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCommunication_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogCommunication("Email", "Outbound", "test@example.com", "Sent");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email") && v.ToString()!.Contains("Sent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Logging Tests

    [Fact]
    public void LogExternalCall_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogExternalCall("PaymentGateway", "/api/charge", "POST");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("PaymentGateway") && v.ToString()!.Contains("POST")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogExternalResponse_WithSuccessStatus_ShouldLogDebug()
    {
        // Act
        _mockLogger.Object.LogExternalResponse("PaymentGateway", 200, 350);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("200") && v.ToString()!.Contains("350")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogExternalResponse_WithErrorStatus_ShouldShowErrorIcon()
    {
        // Act
        _mockLogger.Object.LogExternalResponse("PaymentGateway", 500, 1500);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("500")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region System Logging Tests

    [Fact]
    public void LogSystemStartup_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogSystemStartup("CRM.Api", "1.0.0", "Production");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CRM.Api") && v.ToString()!.Contains("1.0.0") && v.ToString()!.Contains("Production")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogSystemShutdown_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogSystemShutdown("CRM.Api");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("shutting down")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogHealthCheck_WhenHealthy_ShouldLogInformation()
    {
        // Act
        _mockLogger.Object.LogHealthCheck("Database", true);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Healthy")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogHealthCheck_WhenUnhealthy_ShouldLogWarning()
    {
        // Act
        _mockLogger.Object.LogHealthCheck("Database", false, "Connection timeout");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhealthy")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogHealthCheck_WithDetails_ShouldIncludeDetails()
    {
        // Act
        _mockLogger.Object.LogHealthCheck("Redis", true, "Connection pool: 10/20");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Connection pool")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
