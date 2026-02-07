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
/// Tests validate exception constructors, properties, status codes, and error codes.
/// </summary>
public class CrmExceptionsTests
{
    #region EntityNotFoundException Tests

    [Fact]
    public void EntityNotFoundException_WithEntityTypeAndId_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Customer";
        var entityId = 123;

        // Act
        var exception = new EntityNotFoundException(entityType, entityId);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        exception.EntityType.Should().Be(entityType);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain("Customer");
        exception.Message.Should().Contain("123");
        exception.ErrorCode.Should().Be("ENTITY_NOT_FOUND");
    }

    [Fact]
    public void EntityNotFoundException_WithEntityTypeOnly_ShouldCreateMessageWithoutId()
    {
        // Arrange
        var entityType = "Opportunity";

        // Act
        var exception = new EntityNotFoundException(entityType);

        // Assert
        exception.EntityId.Should().BeNull();
        exception.Message.Should().Contain("Opportunity");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public void EntityNotFoundException_WithStringId_ShouldWorkCorrectly()
    {
        // Arrange
        var entityType = "Product";
        var entityId = "SKU-12345";

        // Act
        var exception = new EntityNotFoundException(entityType, entityId);

        // Assert
        exception.EntityId.Should().Be("SKU-12345");
        exception.Message.Should().Contain("SKU-12345");
    }

    [Fact]
    public void EntityNotFoundException_ShouldHaveStatus404()
    {
        // Act
        var exception = new EntityNotFoundException("Test");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } },
            { "Name", new[] { "Name must be at least 2 characters", "Name cannot contain numbers" } },
            { "Phone", new[] { "Invalid phone format" } }
        };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        exception.Errors.Should().HaveCount(3);
        exception.Errors["Email"].Should().Contain("Email is required");
        exception.Errors["Name"].Should().HaveCount(2);
        exception.Errors["Phone"].Should().Contain("Invalid phone format");
    }

    [Fact]
    public void ValidationException_WithNullErrors_ShouldHaveEmptyDictionary()
    {
        // Act
        IDictionary<string, string[]>? nullErrors = null;
        var exception = new ValidationException("Error", nullErrors);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationException_WithFieldAndError_ShouldCreateSingleError()
    {
        // Arrange
        var field = "Email";
        var error = "Invalid email format";

        // Act
        var exception = new ValidationException(field, error);

        // Assert
        exception.Errors.Should().ContainKey(field);
        exception.Errors[field].Should().Contain(error);
        exception.Message.Should().Contain(field);
        exception.Message.Should().Contain(error);
    }

    #endregion

    #region BusinessRuleException Tests

    [Fact]
    public void BusinessRuleException_WithRuleNameAndMessage_ShouldSetProperties()
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
    public void BusinessRuleException_ShouldReturn422StatusCode()
    {
        // Act
        var exception = new BusinessRuleException("TestRule", "Test message");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        ((int)exception.StatusCode).Should().Be(422);
    }

    [Fact]
    public void BusinessRuleException_ShouldPreserveRuleName()
    {
        // Arrange
        var ruleName = "CannotDeleteActiveOpportunity";

        // Act
        var exception = new BusinessRuleException(ruleName, "Cannot delete an active opportunity");

        // Assert
        exception.RuleName.Should().Be(ruleName);
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
        exception.ErrorCode.Should().Be("ACCESS_DENIED");
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

    [Fact]
    public void AuthorizationException_WithoutPermission_ShouldHaveNullPermission()
    {
        // Act
        var exception = new AuthorizationException("Access denied");

        // Assert
        exception.RequiredPermission.Should().BeNull();
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
        exception.Message.Should().Be("Authentication required");
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

    [Fact]
    public void AuthenticationException_ShouldReturn401()
    {
        // Act
        var exception = new AuthenticationException();

        // Assert
        ((int)exception.StatusCode).Should().Be(401);
    }

    #endregion

    #region ConcurrencyException Tests

    [Fact]
    public void ConcurrencyException_WithEntityTypeAndId_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Quote";
        var entityId = 789;

        // Act
        var exception = new ConcurrencyException(entityType, entityId);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.EntityType.Should().Be(entityType);
        exception.EntityId.Should().Be(entityId);
        exception.Message.Should().Contain(entityType);
        exception.Message.Should().Contain("modified by another user");
        exception.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public void ConcurrencyException_WithEntityTypeOnly_ShouldWorkCorrectly()
    {
        // Act
        var exception = new ConcurrencyException("Account");

        // Assert
        exception.EntityType.Should().Be("Account");
        exception.EntityId.Should().BeNull();
        exception.Message.Should().Contain("Account");
    }

    [Fact]
    public void ConcurrencyException_ShouldReturn409()
    {
        // Act
        var exception = new ConcurrencyException("Test");

        // Assert
        ((int)exception.StatusCode).Should().Be(409);
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

    [Fact]
    public void ServiceException_ShouldReturn500()
    {
        // Act
        var exception = new ServiceException("TestService", "Test error");

        // Assert
        ((int)exception.StatusCode).Should().Be(500);
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
        exception.Message.Should().Contain(serviceName);
        exception.Message.Should().Contain(message);
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
        int? externalStatusCode = null;

        // Act
        var exception = new ExternalServiceException(serviceName, message, externalStatusCode, innerException);

        // Assert
        exception.ServiceName.Should().Be(serviceName);
        exception.InnerException.Should().Be(innerException);
        exception.ExternalStatusCode.Should().BeNull();
    }

    [Fact]
    public void ExternalServiceException_ShouldReturn502()
    {
        // Act
        var exception = new ExternalServiceException("TestService", "Test error");

        // Assert
        ((int)exception.StatusCode).Should().Be(502);
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
        exception.StatusCode.Should().Be((HttpStatusCode)429);
        exception.RetryAfterSeconds.Should().Be(retryAfterSeconds);
        exception.Message.Should().Contain("60");
        exception.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public void RateLimitException_DefaultRetryAfter_ShouldBe60Seconds()
    {
        // Act
        var exception = new RateLimitException();

        // Assert
        exception.RetryAfterSeconds.Should().Be(60);
    }

    [Fact]
    public void RateLimitException_WithZeroRetryAfter_ShouldAccept()
    {
        // Act
        var exception = new RateLimitException(0);

        // Assert
        exception.RetryAfterSeconds.Should().Be(0);
    }

    [Fact]
    public void RateLimitException_ShouldReturn429()
    {
        // Act
        var exception = new RateLimitException();

        // Assert
        ((int)exception.StatusCode).Should().Be(429);
    }

    #endregion

    #region ConfigurationException Tests

    [Fact]
    public void ConfigurationException_WithConfigKeyAndMessage_ShouldSetProperties()
    {
        // Arrange
        var configKey = "Jwt:Secret";
        var message = "JWT secret is not configured";

        // Act
        var exception = new ConfigurationException(configKey, message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.ConfigurationKey.Should().Be(configKey);
        exception.Message.Should().Contain(configKey);
        exception.Message.Should().Contain(message);
        exception.ErrorCode.Should().Be("CONFIGURATION_ERROR");
    }

    [Fact]
    public void ConfigurationException_ShouldReturn500()
    {
        // Act
        var exception = new ConfigurationException("TestKey", "Test error");

        // Assert
        ((int)exception.StatusCode).Should().Be(500);
    }

    [Fact]
    public void ConfigurationException_ShouldPreserveConfigKey()
    {
        // Arrange
        var configKey = "ConnectionStrings:DefaultConnection";

        // Act
        var exception = new ConfigurationException(configKey, "Connection string not found");

        // Assert
        exception.ConfigurationKey.Should().Be(configKey);
    }

    #endregion

    #region DuplicateEntityException Tests

    [Fact]
    public void DuplicateEntityException_WithEntityTypeAndIds_ShouldSetProperties()
    {
        // Arrange
        var entityType = "Contact";
        var duplicateIds = new List<int> { 101, 102, 103 };

        // Act
        var exception = new DuplicateEntityException(entityType, duplicateIds);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.EntityType.Should().Be(entityType);
        exception.DuplicateIds.Should().HaveCount(3);
        exception.DuplicateIds.Should().Contain(new[] { 101, 102, 103 });
        exception.ErrorCode.Should().Be("DUPLICATE_DETECTED");
    }

    [Fact]
    public void DuplicateEntityException_ShouldCreateCorrectMessage()
    {
        // Arrange
        var entityType = "Lead";
        var duplicateIds = new List<int> { 1, 2 };

        // Act
        var exception = new DuplicateEntityException(entityType, duplicateIds);

        // Assert
        exception.Message.Should().Contain("Lead");
        exception.Message.Should().Contain("duplicate");
    }

    [Fact]
    public void DuplicateEntityException_WithEmptyIds_ShouldHaveEmptyList()
    {
        // Act
        var exception = new DuplicateEntityException("Customer", new List<int>());

        // Assert
        exception.DuplicateIds.Should().BeEmpty();
    }

    [Fact]
    public void DuplicateEntityException_ShouldReturn409()
    {
        // Act
        var exception = new DuplicateEntityException("Test", new[] { 1 });

        // Assert
        ((int)exception.StatusCode).Should().Be(409);
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
        // Assert - verify all exceptions can be thrown and caught using Record.Exception pattern
        var ex1 = Record.Exception(() => ThrowException(new EntityNotFoundException("Test", 1)));
        var ex2 = Record.Exception(() => ThrowException(new ValidationException("Test")));
        var ex3 = Record.Exception(() => ThrowException(new BusinessRuleException("TestRule", "Test message")));
        var ex4 = Record.Exception(() => ThrowException(new AuthorizationException("Test")));
        var ex5 = Record.Exception(() => ThrowException(new AuthenticationException()));
        var ex6 = Record.Exception(() => ThrowException(new ConcurrencyException("Test")));
        var ex7 = Record.Exception(() => ThrowException(new ServiceException("Test", "Test")));
        var ex8 = Record.Exception(() => ThrowException(new ExternalServiceException("Test", "Test")));
        var ex9 = Record.Exception(() => ThrowException(new RateLimitException(60)));
        var ex10 = Record.Exception(() => ThrowException(new ConfigurationException("Key", "Test")));
        var ex11 = Record.Exception(() => ThrowException(new DuplicateEntityException("Test", new[] { 1 })));

        Assert.IsType<EntityNotFoundException>(ex1);
        Assert.IsType<ValidationException>(ex2);
        Assert.IsType<BusinessRuleException>(ex3);
        Assert.IsType<AuthorizationException>(ex4);
        Assert.IsType<AuthenticationException>(ex5);
        Assert.IsType<ConcurrencyException>(ex6);
        Assert.IsType<ServiceException>(ex7);
        Assert.IsType<ExternalServiceException>(ex8);
        Assert.IsType<RateLimitException>(ex9);
        Assert.IsType<ConfigurationException>(ex10);
        Assert.IsType<DuplicateEntityException>(ex11);
    }

    private static void ThrowException(Exception ex) => throw ex;

    [Fact]
    public void AllExceptions_ShouldBeCatchableAsCrmException()
    {
        // Act & Assert
        try
        {
            throw new EntityNotFoundException("Customer", 123);
        }
        catch (CrmException ex)
        {
            ex.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public void CrmException_ShouldBeAbstract()
    {
        // Assert
        typeof(CrmException).Should().BeAbstract();
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
    [InlineData(typeof(ConfigurationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(DuplicateEntityException), HttpStatusCode.Conflict)]
    public void Exception_ShouldHaveCorrectStatusCode(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        CrmException exception = exceptionType.Name switch
        {
            nameof(EntityNotFoundException) => new EntityNotFoundException("Test", 1),
            nameof(ValidationException) => new ValidationException("Test"),
            nameof(BusinessRuleException) => new BusinessRuleException("Rule", "Test"),
            nameof(AuthorizationException) => new AuthorizationException("Test"),
            nameof(AuthenticationException) => new AuthenticationException(),
            nameof(ConcurrencyException) => new ConcurrencyException("Test"),
            nameof(ServiceException) => new ServiceException("Service", "Test"),
            nameof(ExternalServiceException) => new ExternalServiceException("Service", "Test"),
            nameof(ConfigurationException) => new ConfigurationException("Key", "Test"),
            nameof(DuplicateEntityException) => new DuplicateEntityException("Test", new[] { 1 }),
            _ => throw new ArgumentException($"Unknown exception type: {exceptionType.Name}")
        };

        // Assert
        exception.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void RateLimitException_ShouldHave429StatusCode()
    {
        // Arrange & Act
        var exception = new RateLimitException();

        // Assert - HttpStatusCode doesn't have TooManyRequests in older frameworks, cast to int
        ((int)exception.StatusCode).Should().Be(429);
    }

    #endregion

    #region Error Code Tests

    [Theory]
    [InlineData(typeof(EntityNotFoundException), "ENTITY_NOT_FOUND")]
    [InlineData(typeof(ValidationException), "VALIDATION_ERROR")]
    [InlineData(typeof(BusinessRuleException), "BUSINESS_RULE_VIOLATION")]
    [InlineData(typeof(AuthorizationException), "ACCESS_DENIED")]
    [InlineData(typeof(AuthenticationException), "AUTHENTICATION_REQUIRED")]
    [InlineData(typeof(ConcurrencyException), "CONCURRENCY_CONFLICT")]
    [InlineData(typeof(ServiceException), "SERVICE_ERROR")]
    [InlineData(typeof(ExternalServiceException), "EXTERNAL_SERVICE_ERROR")]
    [InlineData(typeof(RateLimitException), "RATE_LIMIT_EXCEEDED")]
    [InlineData(typeof(ConfigurationException), "CONFIGURATION_ERROR")]
    [InlineData(typeof(DuplicateEntityException), "DUPLICATE_DETECTED")]
    public void Exception_ShouldHaveCorrectErrorCode(Type exceptionType, string expectedErrorCode)
    {
        // Arrange
        CrmException exception = exceptionType.Name switch
        {
            nameof(EntityNotFoundException) => new EntityNotFoundException("Test", 1),
            nameof(ValidationException) => new ValidationException("Test"),
            nameof(BusinessRuleException) => new BusinessRuleException("Rule", "Test"),
            nameof(AuthorizationException) => new AuthorizationException("Test"),
            nameof(AuthenticationException) => new AuthenticationException(),
            nameof(ConcurrencyException) => new ConcurrencyException("Test"),
            nameof(ServiceException) => new ServiceException("Service", "Test"),
            nameof(ExternalServiceException) => new ExternalServiceException("Service", "Test"),
            nameof(RateLimitException) => new RateLimitException(),
            nameof(ConfigurationException) => new ConfigurationException("Key", "Test"),
            nameof(DuplicateEntityException) => new DuplicateEntityException("Test", new[] { 1 }),
            _ => throw new ArgumentException($"Unknown exception type: {exceptionType.Name}")
        };

        // Assert
        exception.ErrorCode.Should().Be(expectedErrorCode);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ValidationException_WithSingleFieldError_ShouldFormatCorrectly()
    {
        // Act
        var exception = new ValidationException("Email", "is required");

        // Assert
        exception.Errors.Should().ContainKey("Email");
        exception.Errors["Email"].First().Should().Be("is required");
    }

    [Fact]
    public void EntityNotFoundException_WithGuidId_ShouldWork()
    {
        // Arrange
        var guidId = Guid.NewGuid();

        // Act
        var exception = new EntityNotFoundException("Document", guidId);

        // Assert
        exception.EntityId.Should().Be(guidId);
    }

    [Fact]
    public void ServiceException_WithNullInnerException_ShouldNotThrow()
    {
        // Act
        var exception = new ServiceException("TestService", "Test error", null);

        // Assert
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ExternalServiceException_WithNullStatusCode_ShouldAccept()
    {
        // Act
        var exception = new ExternalServiceException("API", "Error", null);

        // Assert
        exception.ExternalStatusCode.Should().BeNull();
    }

    [Fact]
    public void DuplicateEntityException_WithSingleId_ShouldWork()
    {
        // Act
        var exception = new DuplicateEntityException("User", new[] { 42 });

        // Assert
        exception.DuplicateIds.Should().ContainSingle();
        exception.DuplicateIds.Should().Contain(42);
    }

    #endregion
}
