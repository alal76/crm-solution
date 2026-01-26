// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Contact Model and Entity Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Models;
using System;

namespace CRM.Tests.Models;

/// <summary>
/// Unit tests for Contact model
/// </summary>
public class ContactModelTests
{
    #region Contact Creation Tests

    [Fact]
    public void Contact_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        contact.Id.Should().Be(0);
        contact.Status.Should().Be(ContactStatus.Active);
        contact.FirstName.Should().BeEmpty();
        contact.LastName.Should().BeEmpty();
    }

    [Fact]
    public void Contact_Name_CanBeSet()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        contact.FirstName.Should().Be("John");
        contact.LastName.Should().Be("Doe");
    }

    [Fact]
    public void Contact_MiddleName_CanBeSet()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe"
        };

        // Assert
        contact.MiddleName.Should().Be("Michael");
    }

    [Fact]
    public void Contact_Salutation_CanBeSet()
    {
        // Arrange
        var contact = new Contact
        {
            Salutation = "Dr.",
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        contact.Salutation.Should().Be("Dr.");
    }

    [Fact]
    public void Contact_Suffix_CanBeSet()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            Suffix = "Jr."
        };

        // Assert
        contact.Suffix.Should().Be("Jr.");
    }

    #endregion

    #region Contact Type Tests

    [Theory]
    [InlineData(ContactType.Employee)]
    [InlineData(ContactType.Partner)]
    [InlineData(ContactType.Lead)]
    [InlineData(ContactType.Customer)]
    [InlineData(ContactType.Vendor)]
    [InlineData(ContactType.Other)]
    public void Contact_Type_AllValidValues(ContactType type)
    {
        // Arrange
        var contact = new Contact { ContactType = type };

        // Assert
        contact.ContactType.Should().Be(type);
    }

    #endregion

    #region Contact Status Tests

    [Theory]
    [InlineData(ContactStatus.Active)]
    [InlineData(ContactStatus.Inactive)]
    [InlineData(ContactStatus.Archived)]
    [InlineData(ContactStatus.Blocked)]
    public void Contact_Status_AllValidValues(ContactStatus status)
    {
        // Arrange
        var contact = new Contact { Status = status };

        // Assert
        contact.Status.Should().Be(status);
    }

    #endregion

    #region Contact Information Tests

    [Fact]
    public void Contact_PrimaryEmail_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact { EmailPrimary = "john@company.com" };

        // Assert
        contact.EmailPrimary.Should().Be("john@company.com");
    }

    [Fact]
    public void Contact_AllEmails_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            EmailPrimary = "primary@company.com",
            EmailSecondary = "secondary@company.com",
            EmailWork = "work@company.com"
        };

        // Assert
        contact.EmailPrimary.Should().Be("primary@company.com");
        contact.EmailSecondary.Should().Be("secondary@company.com");
        contact.EmailWork.Should().Be("work@company.com");
    }

    [Fact]
    public void Contact_AllPhones_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            PhonePrimary = "555-0001",
            PhoneSecondary = "555-0002",
            PhoneMobile = "555-0003",
            PhoneWork = "555-0004",
            PhoneFax = "555-0005"
        };

        // Assert
        contact.PhonePrimary.Should().Be("555-0001");
        contact.PhoneMobile.Should().Be("555-0003");
        contact.PhoneFax.Should().Be("555-0005");
    }

    #endregion

    #region Address Tests

    [Fact]
    public void Contact_Address_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            Address = "123 Main St",
            Address2 = "Suite 100",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        // Assert
        contact.Address.Should().Be("123 Main St");
        contact.City.Should().Be("New York");
        contact.ZipCode.Should().Be("10001");
    }

    #endregion

    #region Social Media Tests

    [Fact]
    public void Contact_SocialMedia_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            LinkedInUrl = "https://linkedin.com/in/johndoe"
        };

        // Assert
        contact.LinkedInUrl.Should().Contain("linkedin.com");
    }

    #endregion

    #region Lead Status Tests

    [Fact]
    public void Contact_LeadStatus_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            ContactType = ContactType.Lead,
            LeadStatus = LeadStatus.Qualified
        };

        // Assert
        contact.ContactType.Should().Be(ContactType.Lead);
        contact.LeadStatus.Should().Be(LeadStatus.Qualified);
    }

    [Theory]
    [InlineData(LeadStatus.New)]
    [InlineData(LeadStatus.Contacted)]
    [InlineData(LeadStatus.Qualified)]
    [InlineData(LeadStatus.Unqualified)]
    [InlineData(LeadStatus.Converted)]
    public void Contact_LeadStatus_AllValidValues(LeadStatus status)
    {
        // Arrange
        var contact = new Contact { LeadStatus = status };

        // Assert
        contact.LeadStatus.Should().Be(status);
    }

    #endregion

    #region Communication Preference Tests

    [Theory]
    [InlineData(PreferredContactMethod.Email)]
    [InlineData(PreferredContactMethod.Phone)]
    [InlineData(PreferredContactMethod.SMS)]
    [InlineData(PreferredContactMethod.Mail)]
    [InlineData(PreferredContactMethod.Social)]
    [InlineData(PreferredContactMethod.Any)]
    public void Contact_PreferredContactMethod_AllValidValues(PreferredContactMethod method)
    {
        // Arrange
        var contact = new Contact { PreferredContactMethod = method };

        // Assert
        contact.PreferredContactMethod.Should().Be(method);
    }

    #endregion

    #region Work Information Tests

    [Fact]
    public void Contact_WorkInfo_CanBeSet()
    {
        // Arrange & Act
        var contact = new Contact
        {
            JobTitle = "Software Engineer",
            Department = "Engineering",
            Company = "Tech Corp",
            ReportsTo = "Jane Manager"
        };

        // Assert
        contact.JobTitle.Should().Be("Software Engineer");
        contact.Department.Should().Be("Engineering");
        contact.Company.Should().Be("Tech Corp");
        contact.ReportsTo.Should().Be("Jane Manager");
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public void Contact_DateOfBirth_CanBeSet()
    {
        // Arrange
        var dob = new DateTime(1990, 5, 15);

        // Act
        var contact = new Contact { DateOfBirth = dob };

        // Assert
        contact.DateOfBirth.Should().Be(dob);
    }

    #endregion

    #region Opt-In Tests

    [Fact]
    public void Contact_OptIn_Defaults()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        contact.OptInEmail.Should().BeTrue();
        contact.OptInSms.Should().BeFalse();
        contact.OptInPhone.Should().BeTrue();
        contact.DoNotContact.Should().BeFalse();
    }

    [Fact]
    public void Contact_DoNotContact_CanBeToggled()
    {
        // Arrange & Act
        var contact = new Contact { DoNotContact = true };

        // Assert
        contact.DoNotContact.Should().BeTrue();
    }

    #endregion
}
