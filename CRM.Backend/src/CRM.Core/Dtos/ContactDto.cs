namespace CRM.Core.Dtos;

public class SocialMediaLinkDto
{
    public int Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Handle { get; set; }
}

public class ContactDto
{
    public int Id { get; set; }
    public string ContactType { get; set; } = "Other";
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }
    
    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? LastModified { get; set; }
    public string? ModifiedBy { get; set; }
    
    public List<SocialMediaLinkDto> SocialMediaLinks { get; set; } = new();
}

public class CreateContactRequest
{
    public string ContactType { get; set; } = "Other";
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }
    
    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class UpdateContactRequest
{
    public string? ContactType { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }
    
    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class AddSocialMediaRequest
{
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Handle { get; set; }
}
