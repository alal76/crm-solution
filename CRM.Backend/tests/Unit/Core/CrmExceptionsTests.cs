// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under GNU AGPL v3

using System.Net;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for CRM custom exception types
/// </summary>
public class CrmExceptionsTests
{
    #region CrmException (Base Class) Tests

    [Fact]
    public void CrmException_DefaultConstructor_ShouldSetStatusCode500()
    {
        // Arrange & Act
        var exception = new CrmException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.ErrorCode.Should().BeNull();
        exception.Message.Should().Be("Exception of type 'CRM.Core.Exceptions.CrmException' was thrown.");
    }

    [Fact]
    public void CrmException_WithMessage_ShouldPreserveMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new CrmException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void CrmException_WithMessageAndInnerException_ShouldPreserveBoth()
    {
        // Arrange
        var message = "Outer exception";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new CrmException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void CrmException_WithStatusCodeAndMessage_ShouldSetBoth()
    {
        // Arrange
        var statusCode = HttpStatusCode.BadRequest;
        var message = "Bad request error";

        // Act
        var exception = new CrmException(statusCode, message);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void CrmException_WithStatusCodeMessageAndErrorCode_ShouldSetAll()
    {
        // Arrange
        var statusCode = HttpStatusCode.UnprocessableEntity;
        var message = "Validation failed";
        var errorCode = "VALIDATION_FAILED";

        // Act
        var exception = new CrmException(statusCode, message, errorCode);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
    }

    #endregion

    #region EntityNotFoundException Tests

    [Fact]
    public void EntityNotFoundException_WithEntityTypeAndId_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Customer";
        var entityId = "123";

        // Act
        var exception = new EntityNotFoundException(entityType, entityId);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.EntityType.Should().Be(entityType);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Be("Customer with id '123' was not found.");
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
    }

    [Fact]
    public void EntityNotFoundException_WithIntId_ShouldConvertToString()
    {
        // Arrange
        var entityType = "Opportunity";
        var entityId = 456;

        // Act
        var exception = new EntityNotFoundException(entityType, entityId);

        // Assert
        exception.EntityId.Should().Be("456");
        exception.Message.Should().Be("Opportunity with id '456' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_WithMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var message = "Custom not found message";

        // Act
        var exception = new EntityNotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
    }

    #endregion

    #region ValidationException Tests

    [Fact]
    public void ValidationException_WithMessage_ShouldSetDefaults()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var exception = new ValidationException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationException_WithErrors_ShouldPreserveErrors()
    {
        // Arrange
        var message = "Multiple validation errors";
        var errors = new Dictionary<string, string>
        {
            { "Email", "Email is required" },
            { "Name", "Name must be at least 2 characters" },
            { "Phone", "Invalid phone format" }
        };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        exception.Errors.Should().HaveCount(3);
        exception.Errors["Email"].Should().Be("Email is required");
        exception.Errors["Name"].Should().Be("Name must be at least 2 characters");
        exception.Errors["Phone"].Should().Be("Invalid phone format");
    }

    [Fact]
    public void ValidationException_WithNullErrors_ShouldHaveEmptyDictionary()
    {
        // Act
        var exception = new ValidationException("Error", null);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    #endregion

    #region BusinessRuleException Tests

    [Fact]
    public void BusinessRuleException_WithRuleName_ShouldSetProperties()
    {
        // Arrange
        var ruleName = "MinimumOrderAmount";
        var message = "Order amount must be at least $10";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        exception.RuleName.Should().Be(ruleName);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
    }

    [Fact]
    public void BusinessRuleException_WithMessageOnly_ShouldHaveNullRuleName()
    {
        // Arrange
        var message = "Business rule violated";

        // Act
        var exception = new BusinessRuleException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        exception.Message.Should().Be(message);
        exception.RuleName.Should().BeNull();
    }

    #endregion

    #region AuthorizationException Tests

    [Fact]
    public void AuthorizationException_WithMessage_ShouldSet403()
    {
        // Arrange
        var message = "Access denied";

        // Act
        var exception = new AuthorizationException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("AUTHORIZATION_FAILED");
    }

    [Fact]
    public void AuthorizationException_WithRequiredPermission_ShouldPreservePermission()
    {
        // Arrange
        var message = "Insufficient permissions";
        var requiredPermission = "CanDeleteCustomers";

        // Act
        var exception = new AuthorizationException(message, requiredPermission);

        // Assert
        exception.RequiredPermission.Should().Be(requiredPermission);
        exception.Message.Should().Be(message);
    }

    #endregion

    #region AuthenticationException Tests

    [Fact]
    public void AuthenticationException_DefaultConstructor_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new AuthenticationException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        exception.Message.Should().Be("Authentication required.");
        exception.ErrorCode.Should().Be("AUTHENTICATION_REQUIRED");
    }

    [Fact]
    public void AuthenticationException_WithMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var message = "Token expired";

        // Act
        var exception = new AuthenticationException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        exception.Message.Should().Be(message);
    }

    #endregion

    #region ConcurrencyException Tests

    [Fact]
    public void ConcurrencyException_WithEntityTypeAndId_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Quote";
        var entityId = "789";

        // Act
        var exception = new ConcurrencyException(entityType, entityId);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.EntityType.Should().Be(entityType);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Be("Quote with id '789' was modified by another user. Please refresh and try again.");
        exception.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public void ConcurrencyException_WithMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var message = "Concurrent modification detected";

        // Act
        var exception = new ConcurrencyException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region ServiceException Tests

    [Fact]
    public void ServiceException_WithServiceNameAndMessage_ShouldSetProperties()
    {
        // Arrange
        var serviceName = "AccountService";
        var message = "Failed to process request";

        // Act
        var exception = new ServiceException(serviceName, message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.ServiceName.Should().Be(serviceName);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("SERVICE_ERROR");
    }

    [Fact]
    public void ServiceException_WithInnerException_ShouldPreserveInner()
    {
        // Arrange
        var serviceName = "EmailService";
        var message = "Email sending failed";
        var innerException = new InvalidOperationException("SMTP connection failed");

        // Act
        var exception = new ServiceException(serviceName, message, innerException);

        // Assert
        exception.ServiceName.Should().Be(serviceName);
        exception.InnerException.Should().Be(innerException);
        exception.InnerException!.Message.Should().Be("SMTP connection failed");
    }

    #endregion

    #region ExternalServiceException Tests

    [Fact]
    public void ExternalServiceException_WithAllParameters_ShouldSetProperties()
    {
        // Arrange
        var serviceName = "Meilisearch";
        var message = "Search service unavailable";
        var externalStatusCode = 503;

        // Act
        var exception = new ExternalServiceException(serviceName, message, externalStatusCode);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        exception.ServiceName.Should().Be(serviceName);
        exception.Message.Should().Be(message);
        exception.ExternalStatusCode.Should().Be(externalStatusCode);
        exception.ErrorCode.Should().Be("EXTERNAL_SERVICE_ERROR");
    }

    [Fact]
    public void ExternalServiceException_WithInnerException_ShouldPreserveInner()
    {
        // Arrange
        var serviceName = "PaymentGateway";
        var message = "Payment processing failed";
        var innerException = new HttpRequestException("Connection refused");
        var externalStatusCode = 0;

        // Act
        var exception = new ExternalServiceException(serviceName, message, externalStatusCode, innerException);

        // Assert
        exception.ServiceName.Should().Be(serviceName);
        exception.InnerException.Should().Be(innerException);
        exception.ExternalStatusCode.Should().Be(0);
    }

    #endregion

    #region RateLimitException Tests

    [Fact]
    public void RateLimitException_WithRetryAfter_ShouldSetProperties()
    {
        // Arrange
        var retryAfterSeconds = 60;

        // Act
        var exception = new RateLimitException(retryAfterSeconds);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        exception.RetryAfterSeconds.Should().Be(retryAfterSeconds);
        exception.Message.Should().Be("Rate limit exceeded. Retry after 60 seconds.");
        exception.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public void RateLimitException_WithCustomMessage_ShouldUseMessage()
    {
        // Arrange
        var message = "API quota exceeded";
        var retryAfterSeconds = 300;

        // Act
        var exception = new RateLimitException(message, retryAfterSeconds);

        // Assert
        exception.Message.Should().Be(message);
        exception.RetryAfterSeconds.Should().Be(retryAfterSeconds);
    }

    [Fact]
    public void RateLimitException_WithZeroRetryAfter_ShouldAccept()
    {
        // Act
        var exception = new RateLimitException(0);

        // Assert
        exception.RetryAfterSeconds.Should().Be(0);
    }

    #endregion

    #region ConfigurationException Tests

    [Fact]
    public void ConfigurationException_WithConfigKey_ShouldSetProperties()
    {
        // Arrange
        var configKey = "Jwt:Secret";
        var message = "JWT secret is not configured";

        // Act
        var exception = new ConfigurationException(configKey, message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.ConfigurationKey.Should().Be(configKey);
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("CONFIGURATION_ERROR");
    }

    [Fact]
    public void ConfigurationException_WithMessageOnly_ShouldHaveNullKey()
    {
        // Arrange
        var message = "Configuration is invalid";

        // Act
        var exception = new ConfigurationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ConfigurationKey.Should().BeNull();
    }

    #endregion

    #region DuplicateEntityException Tests

    [Fact]
    public void DuplicateEntityException_WithEntityTypeAndIds_ShouldSetProperties()
    {
        // Arrange
        var entityType = "Contact";
        var duplicateIds = new List<string> { "101", "102", "103" };

        // Act
        var exception = new DuplicateEntityException(entityType, duplicateIds);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.EntityType.Should().Be(entityType);
        exception.DuplicateIds.Should().HaveCount(3);
        exception.DuplicateIds.Should().Contain(new[] { "101", "102", "103" });
        exception.ErrorCode.Should().Be("DUPLICATE_ENTITY");
    }

    [Fact]
    public void DuplicateEntityException_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Lead";
        var duplicateIds = new List<string> { "1", "2" };

        // Act
        var exception = new DuplicateEntityException(entityType, duplicateIds);

        // Assert
        exception.Message.Should().Contain("Lead");
        exception.Message.Should().Contain("duplicate");
    }

    [Fact]
    public void DuplicateEntityException_WithMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var message = "Duplicate email addresses found";

        // Act
        var exception = new DuplicateEntityException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public void DuplicateEntityException_WithEmptyIds_ShouldHaveEmptyList()
    {
        // Act
        var exception = new DuplicateEntityException("Customer", new List<string>());

        // Assert
        exception.DuplicateIds.Should().BeEmpty();
    }

    #endregion

    #region Exception Inheritance Tests

    [Fact]
    public void AllExceptions_ShouldInheritFromCrmException()
    {
        // Assert
        typeof(EntityNotFoundException).Should().BeDerivedFrom<CrmException>();
        typeof(ValidationException).Should().BeDerivedFrom<CrmException>();
        typeof(BusinessRuleException).Should().BeDerivedFrom<CrmException>();
        typeof(AuthorizationException).Should().BeDerivedFrom<CrmException>();
        typeof(AuthenticationException).Should().BeDerivedFrom<CrmException>();
        typeof(ConcurrencyException).Should().BeDerivedFrom<CrmException>();
        typeof(ServiceException).Should().BeDerivedFrom<CrmException>();
        typeof(ExternalServiceException).Should().BeDerivedFrom<CrmException>();
        typeof(RateLimitException).Should().BeDerivedFrom<CrmException>();
        typeof(ConfigurationException).Should().BeDerivedFrom<CrmException>();
        typeof(DuplicateEntityException).Should().BeDerivedFrom<CrmException>();
    }

    [Fact]
    public void AllExceptions_ShouldBeThrowable()
    {
        // Assert - verify all exceptions can be thrown and caught
        Assert.Throws<EntityNotFoundException>(() => throw new EntityNotFoundException("Test", "1"));
        Assert.Throws<ValidationException>(() => throw new ValidationException("Test"));
        Assert.Throws<BusinessRuleException>(() => throw new BusinessRuleException("Test"));
        Assert.Throws<AuthorizationException>(() => throw new AuthorizationException("Test"));
        Assert.Throws<AuthenticationException>(() => throw new AuthenticationException());
        Assert.Throws<ConcurrencyException>(() => throw new ConcurrencyException("Test"));
        Assert.Throws<ServiceException>(() => throw new ServiceException("Test", "Test"));
        Assert.Throws<ExternalServiceException>(() => throw new ExternalServiceException("Test", "Test", 500));
        Assert.Throws<RateLimitException>(() => throw new RateLimitException(60));
        Assert.Throws<ConfigurationException>(() => throw new ConfigurationException("Test"));
        Assert.Throws<DuplicateEntityException>(() => throw new DuplicateEntityException("Test"));
    }

    [Fact]
    public void AllExceptions_ShouldBeCatchableAsCrmException()
    {
        // Act & Assert
        try
        {
            throw new EntityNotFoundException("Customer", "123");
        }
        catch (CrmException ex)
        {
            ex.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region HTTP Status Code Tests

    [Theory]
    [InlineData(typeof(EntityNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(ValidationException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(BusinessRuleException), HttpStatusCode.UnprocessableEntity)]
    [InlineData(typeof(AuthorizationException), HttpStatusCode.Forbidden)]
    [InlineData(typeof(AuthenticationException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(ConcurrencyException), HttpStatusCode.Conflict)]
    [InlineData(typeof(ServiceException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(ExternalServiceException), HttpStatusCode.BadGateway)]
    [InlineData(typeof(RateLimitException), HttpStatusCode.TooManyRequests)]
    [InlineData(typeof(ConfigurationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(DuplicateEntityException), HttpStatusCode.Conflict)]
    public void Exception_ShouldHaveCorrectStatusCode(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        CrmException exception = exceptionType.Name switch
        {
            nameof(EntityNotFoundException) => new EntityNotFoundException("Test", "1"),
            nameof(ValidationException) => new ValidationException("Test"),
            nameof(BusinessRuleException) => new BusinessRuleException("Test"),
            nameof(AuthorizationException) => new AuthorizationException("Test"),
            nameof(AuthenticationException) => new AuthenticationException(),
            nameof(ConcurrencyException) => new ConcurrencyException("Test"),
            nameof(ServiceException) => new ServiceException("Service", "Test"),
            nameof(ExternalServiceException) => new ExternalServiceException("Service", "Test", 500),
            nameof(RateLimitException) => new RateLimitException(60),
            nameof(ConfigurationException) => new ConfigurationException("Test"),
            nameof(DuplicateEntityException) => new DuplicateEntityException("Test"),
            _ => throw new ArgumentException($"Unknown exception type: {exceptionType.Name}")
        };

        // Assert
        exception.StatusCode.Should().Be(expectedStatusCode);
    }

    #endregion

    #region Error Code Tests

    [Theory]
    [InlineData(typeof(EntityNotFoundException), "ENTITY_NOT_FOUND")]
    [InlineData(typeof(ValidationException), "VALIDATION_ERROR")]
    [InlineData(typeof(BusinessRuleException), "BUSINESS_RULE_VIOLATION")]
    [InlineData(typeof(AuthorizationException), "AUTHORIZATION_FAILED")]
    [InlineData(typeof(AuthenticationException), "AUTHENTICATION_REQUIRED")]
    [InlineData(typeof(ConcurrencyException), "CONCURRENCY_CONFLICT")]
    [InlineData(typeof(ServiceException), "SERVICE_ERROR")]
    [InlineData(typeof(ExternalServiceException), "EXTERNAL_SERVICE_ERROR")]
    [InlineData(typeof(RateLimitException), "RATE_LIMIT_EXCEEDED")]
    [InlineData(typeof(ConfigurationException), "CONFIGURATION_ERROR")]
    [InlineData(typeof(DuplicateEntityException), "DUPLICATE_ENTITY")]
    public void Exception_ShouldHaveCorrectErrorCode(Type exceptionType, string expectedErrorCode)
    {
        // Arrange
        CrmException exception = exceptionType.Name switch
        {
            nameof(EntityNotFoundException) => new EntityNotFoundException("Test", "1"),
            nameof(ValidationException) => new ValidationException("Test"),
            nameof(BusinessRuleException) => new BusinessRuleException("Test"),
            nameof(AuthorizationException) => new AuthorizationException("Test"),
            nameof(AuthenticationException) => new AuthenticationException(),
            nameof(ConcurrencyException) => new ConcurrencyException("Test"),
            nameof(ServiceException) => new ServiceException("Service", "Test"),
            nameof(ExternalServiceException) => new ExternalServiceException("Service", "Test", 500),
            nameof(RateLimitException) => new RateLimitException(60),
            nameof(ConfigurationException) => new ConfigurationException("Test"),
            nameof(DuplicateEntityException) => new DuplicateEntityException("Test"),
            _ => throw new ArgumentException($"Unknown exception type: {exceptionType.Name}")
        };

        // Assert
        exception.ErrorCode.Should().Be(expectedErrorCode);
    }

    #endregion
}
