// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// Centralized validation error messages used consistently across all DTOs and validations.
/// Enables easy maintenance and consistent user-facing messages.
/// </summary>
public static class ValidationMessages
{
    /// <summary>
    /// Generic/Common validation messages.
    /// </summary>
    public static class Common
    {
        public const string Required = "{0} is required.";
        public const string InvalidFormat = "{0} has an invalid format.";
        public const string MaxLengthExceeded = "{0} cannot exceed {1} characters.";
        public const string MinLengthNotMet = "{0} must be at least {1} characters.";
        public const string RangeOutOfBounds = "{0} must be between {1} and {2}.";
        public const string InvalidValueType = "{0} must be of type {1}.";
        public const string DuplicateValue = "{0} '{1}' already exists.";
        public const string NotFound = "{0} with ID {1} not found.";
        public const string Unauthorized = "You do not have permission to perform this action.";
        public const string ServerError = "An unexpected server error occurred. Please try again later.";
        public const string ValidationFailed = "One or more validation errors occurred.";
    }

    /// <summary>
    /// Email validation messages.
    /// </summary>
    public static class Email
    {
        public const string Invalid = "Email address has an invalid format.";
        public const string InvalidDomain = "Email domain '{0}' is not allowed. Allowed domains: {1}";
        public const string AlreadyExists = "An account with this email already exists.";
        public const string IsRequired = "Email address is required.";
    }

    /// <summary>
    /// Phone number validation messages.
    /// </summary>
    public static class Phone
    {
        public const string InvalidFormat = "Phone number must be in E.164 format (e.g., +14155552671).";
        public const string TooShort = "Phone number must contain at least 10 digits.";
        public const string TooLong = "Phone number cannot exceed 15 digits.";
    }

    /// <summary>
    /// Password validation messages.
    /// </summary>
    public static class Password
    {
        public const string Required = "Password is required.";
        public const string TooShort = "Password must be at least {0} characters long.";
        public const string MustContainUppercase = "Password must contain at least one uppercase letter.";
        public const string MustContainLowercase = "Password must contain at least one lowercase letter.";
        public const string MustContainDigit = "Password must contain at least one digit.";
        public const string MustContainSpecialChar = "Password must contain at least one special character (!@#$%^&*).";
        public const string CannotBeSame = "New password must be different from the current password.";
        public const string ConfirmationMismatch = "Password and confirmation password do not match.";
        public const string PreviouslyUsed = "This password has been used recently. Please choose a different password.";
        public const string Weak = "Password is too weak. Please use a stronger password.";
    }

    /// <summary>
    /// Currency/Financial validation messages.
    /// </summary>
    public static class Currency
    {
        public const string InvalidCode = "Currency code must be a valid 3-letter ISO 4217 code (e.g., USD, EUR, GBP).";
        public const string MustBePositive = "{0} must be greater than 0.";
        public const string CannotBeNegative = "{0} cannot be negative.";
        public const string MaxExceeded = "{0} cannot exceed {1}.";
        public const string InvalidPrecision = "{0} must have at most {1} decimal places.";
        public const string InvalidAmount = "Amount must be a valid currency value.";
        public const string RefundExceedsPayment = "Refund amount cannot exceed the original payment amount.";
    }

    /// <summary>
    /// Date validation messages.
    /// </summary>
    public static class Date
    {
        public const string InvalidFormat = "Date must be in ISO 8601 format (YYYY-MM-DD).";
        public const string InThePast = "Date cannot be in the past.";
        public const string InTheFuture = "Date cannot be in the future.";
        public const string StartAfterEnd = "Start date must be before end date.";
        public const string EndBeforeStart = "End date must be after start date.";
        public const string DueBeforeCreated = "Due date must be after creation date.";
        public const string MinDaysFromToday = "Date must be at least {0} days from today.";
        public const string MaxDaysFromToday = "Date must be at most {0} days from today.";
    }

    /// <summary>
    /// URL/URI validation messages.
    /// </summary>
    public static class Url
    {
        public const string Invalid = "URL has an invalid format.";
        public const string InvalidScheme = "URL scheme must be one of: {0}";
        public const string DomainNotAllowed = "Domain '{0}' is not allowed.";
    }

    /// <summary>
    /// Account/Organization validation messages.
    /// </summary>
    public static class Account
    {
        public const string NameRequired = "Account name is required.";
        public const string NameTooLong = "Account name cannot exceed 255 characters.";
        public const string TypeRequired = "Account type is required.";
        public const string InvalidType = "Account type '{0}' is not valid.";
        public const string DuplicateName = "An account with this name already exists.";
        public const string NotFound = "Account not found.";
        public const string CannotDelete = "Account cannot be deleted because it has active relationships.";
        public const string InvalidStatus = "Account status '{0}' is not valid.";
    }

    /// <summary>
    /// Contact validation messages.
    /// </summary>
    public static class Contact
    {
        public const string FirstNameRequired = "First name is required.";
        public const string FirstNameTooLong = "First name cannot exceed 100 characters.";
        public const string LastNameRequired = "Last name is required.";
        public const string LastNameTooLong = "Last name cannot exceed 100 characters.";
        public const string EmailRequired = "Email address is required.";
        public const string RoleRequired = "Contact role is required.";
        public const string InvalidRole = "Contact role '{0}' is not valid.";
        public const string NotFound = "Contact not found.";
        public const string DuplicateEmail = "A contact with this email already exists.";
        public const string AccountRequired = "Contact must be associated with an account.";
    }

    /// <summary>
    /// Opportunity/Quote validation messages.
    /// </summary>
    public static class Opportunity
    {
        public const string NameRequired = "Opportunity name is required.";
        public const string StageRequired = "Opportunity stage is required.";
        public const string InvalidStage = "Opportunity stage '{0}' is not valid.";
        public const string AmountRequired = "Opportunity amount is required.";
        public const string InvalidAmount = "Opportunity amount must be greater than 0.";
        public const string CloseDateRequired = "Expected close date is required.";
        public const string CloseDateInPast = "Expected close date cannot be in the past.";
        public const string NotFound = "Opportunity not found.";
        public const string CannotClose = "Opportunity in stage '{0}' cannot be closed.";
    }

    /// <summary>
    /// Ticket/Support validation messages.
    /// </summary>
    public static class Ticket
    {
        public const string SubjectRequired = "Ticket subject is required.";
        public const string SubjectTooLong = "Ticket subject cannot exceed 255 characters.";
        public const string DescriptionRequired = "Ticket description is required.";
        public const string StatusRequired = "Ticket status is required.";
        public const string InvalidStatus = "Ticket status '{0}' is not valid.";
        public const string PriorityRequired = "Ticket priority is required.";
        public const string InvalidPriority = "Ticket priority '{0}' is not valid.";
        public const string SeverityRequired = "Severity level is required.";
        public const string InvalidSeverity = "Severity level '{0}' is not valid.";
        public const string NotFound = "Support ticket not found.";
        public const string AlreadyClosed = "Ticket is already closed and cannot be modified.";
        public const string CannotReassign = "Ticket cannot be reassigned in its current status.";
    }

    /// <summary>
    /// Campaign validation messages.
    /// </summary>
    public static class Campaign
    {
        public const string NameRequired = "Campaign name is required.";
        public const string NameTooLong = "Campaign name cannot exceed 255 characters.";
        public const string TypeRequired = "Campaign type is required.";
        public const string StatusRequired = "Campaign status is required.";
        public const string StartDateRequired = "Start date is required.";
        public const string EndDateRequired = "End date is required.";
        public const string EndBeforeStart = "End date must be after start date.";
        public const string BudgetRequired = "Budget is required.";
        public const string InvalidBudget = "Budget must be greater than 0.";
        public const string BudgetExceeded = "Spending has exceeded the campaign budget.";
        public const string NoRecipients = "Campaign must have at least one recipient.";
        public const string NotFound = "Campaign not found.";
        public const string CannotModify = "Campaign in status '{0}' cannot be modified.";
    }

    /// <summary>
    /// Invoice validation messages.
    /// </summary>
    public static class Invoice
    {
        public const string NumberRequired = "Invoice number is required.";
        public const string NumberMustBeUnique = "Invoice number '{0}' already exists.";
        public const string InvalidNumber = "Invoice number format is invalid.";
        public const string AmountRequired = "Invoice amount is required.";
        public const string AmountMustBePositive = "Invoice amount must be greater than 0.";
        public const string TaxAmountInvalid = "Tax amount cannot be greater than the invoice amount.";
        public const string DueDateRequired = "Due date is required.";
        public const string DueDateInPast = "Due date cannot be in the past.";
        public const string StatusInvalid = "Invoice status '{0}' is not valid.";
        public const string CannotModify = "Invoice cannot be modified once {0}.";
        public const string NotFound = "Invoice not found.";
        public const string LineItemsRequired = "Invoice must contain at least one line item.";
    }

    /// <summary>
    /// User/Authentication validation messages.
    /// </summary>
    public static class User
    {
        public const string UsernameRequired = "Username is required.";
        public const string UsernameAlreadyExists = "Username '{0}' is already in use.";
        public const string UsernameTooShort = "Username must be at least {0} characters.";
        public const string UsernameTooLong = "Username cannot exceed {0} characters.";
        public const string FirstNameRequired = "First name is required.";
        public const string LastNameRequired = "Last name is required.";
        public const string EmailRequired = "Email address is required.";
        public const string EmailAlreadyExists = "Email '{0}' is already in use.";
        public const string RoleRequired = "User role is required.";
        public const string InvalidRole = "User role '{0}' is not valid.";
        public const string PasswordRequired = "Password is required.";
        public const string InvalidCredentials = "Username or password is incorrect.";
        public const string AccountLocked = "Account is locked due to too many failed login attempts.";
        public const string NotFound = "User not found.";
        public const string CannotDeleteSelf = "You cannot delete your own account.";
        public const string MustChangePassword = "Password must be changed on first login.";
    }

    /// <summary>
    /// Batch/Bulk operation validation messages.
    /// </summary>
    public static class BulkOperation
    {
        public const string ItemsRequired = "At least one item is required for bulk operations.";
        public const string ItemsEmpty = "Items collection cannot be empty.";
        public const string TooManyItems = "Bulk operation cannot process more than {0} items.";
        public const string PartialFailure = "Bulk operation completed with {0} failures out of {1} items.";
        public const string AllItemsFailed = "All items in the bulk operation failed.";
    }

    /// <summary>
    /// Search/Filter validation messages.
    /// </summary>
    public static class Search
    {
        public const string InvalidPageNumber = "Page number must be at least 1.";
        public const string InvalidPageSize = "Page size must be between 1 and {0}.";
        public const string InvalidSortField = "Sort field '{0}' is not valid.";
        public const string SearchTermTooLong = "Search term cannot exceed {0} characters.";
        public const string NoResults = "No results found matching your criteria.";
    }

    /// <summary>
    /// Permission/Authorization validation messages.
    /// </summary>
    public static class Permission
    {
        public const string Denied = "You do not have permission to perform this action.";
        public const string InsufficientPrivileges = "Your account does not have sufficient privileges for this operation.";
        public const string RecordNotAccessible = "You do not have access to this record.";
        public const string FeatureNotEnabled = "This feature is not enabled for your account.";
        public const string CannotModifyOthersRecords = "You can only modify your own records.";
    }

    /// <summary>
    /// System/Configuration validation messages.
    /// </summary>
    public static class System
    {
        public const string FeatureFlagInvalid = "Feature flag '{0}' is not valid.";
        public const string ConfigurationMissing = "Required configuration '{0}' is missing.";
        public const string ServiceUnavailable = "The service is currently unavailable. Please try again later.";
        public const string DatabaseError = "A database error occurred. Please try again later.";
        public const string ExternalServiceError = "External service error: {0}";
    }

    /// <summary>
    /// Helper method to format a validation message with parameters.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders (e.g., "must be between {0} and {1}").</param>
    /// <param name="args">The arguments to insert into placeholders.</param>
    /// <returns>The formatted validation message.</returns>
    public static string Format(string messageTemplate, params object?[] args)
    {
        try
        {
            return string.Format(messageTemplate, args);
        }
        catch
        {
            return messageTemplate;
        }
    }
}
