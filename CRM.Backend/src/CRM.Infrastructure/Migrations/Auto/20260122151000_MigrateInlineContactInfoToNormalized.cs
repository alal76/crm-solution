using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    public partial class MigrateInlineContactInfoToNormalized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: create ContactDetail rows for email/phone/fax and Address rows for billing/shipping,
            // then link via ContactInfoLink. Uses NOT EXISTS checks to avoid duplicates.

            // Customers: ContactDetails (Email, SecondaryEmail, Phone, MobilePhone, Fax) and Addresses
            migrationBuilder.Sql(@"
-- Customers: ContactDetails
INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, CreatedAt)
SELECT 0, c.Email, 'Primary Email', 1, NOW() FROM Customers c
WHERE c.Email IS NOT NULL AND c.Email <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactDetails cd WHERE cd.Value = c.Email AND cd.DetailType = 0);

INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, CreatedAt)
SELECT 0, c.SecondaryEmail, 'Secondary Email', 0, NOW() FROM Customers c
WHERE c.SecondaryEmail IS NOT NULL AND c.SecondaryEmail <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactDetails cd WHERE cd.Value = c.SecondaryEmail AND cd.DetailType = 0);

INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, CreatedAt)
SELECT 1, c.Phone, 'Phone', 1, NOW() FROM Customers c
WHERE c.Phone IS NOT NULL AND c.Phone <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactDetails cd WHERE cd.Value = c.Phone AND cd.DetailType = 1);

INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, CreatedAt)
SELECT 1, c.MobilePhone, 'Mobile', 0, NOW() FROM Customers c
WHERE c.MobilePhone IS NOT NULL AND c.MobilePhone <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactDetails cd WHERE cd.Value = c.MobilePhone AND cd.DetailType = 1);

INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, CreatedAt)
SELECT 2, c.FaxNumber, 'Fax', 0, NOW() FROM Customers c
WHERE c.FaxNumber IS NOT NULL AND c.FaxNumber <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactDetails cd WHERE cd.Value = c.FaxNumber AND cd.DetailType = 2);

-- Customers: Addresses (billing/primary)
INSERT INTO Address (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, CreatedAt)
SELECT 'Primary', c.Address, c.Address2, c.City, c.State, c.ZipCode, c.Country, 1, NOW()
FROM Customers c
WHERE c.Address IS NOT NULL AND c.Address <> ''
  AND NOT EXISTS(SELECT 1 FROM Address a WHERE a.Line1 = c.Address AND a.City = c.City AND a.PostalCode = c.ZipCode);

-- Customers: Shipping Addresses
INSERT INTO Address (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, CreatedAt)
SELECT 'Shipping', c.ShippingAddress, c.ShippingAddress2, c.ShippingCity, c.ShippingState, c.ShippingZipCode, c.ShippingCountry, 0, NOW()
FROM Customers c
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress <> ''
  AND NOT EXISTS(SELECT 1 FROM Address a WHERE a.Line1 = c.ShippingAddress AND a.City = c.ShippingCity AND a.PostalCode = c.ShippingZipCode);

-- Link ContactDetails/Addresses to Customers via ContactInfoLink
-- Link primary email
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, ContactDetailId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 1, cd.Id, 1, NOW()
FROM Customers c
JOIN ContactDetails cd ON cd.Value = c.Email AND cd.DetailType = 0
WHERE c.Email IS NOT NULL AND c.Email <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.ContactDetailId = cd.Id);

-- Link secondary email
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, ContactDetailId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 1, cd.Id, 0, NOW()
FROM Customers c
JOIN ContactDetails cd ON cd.Value = c.SecondaryEmail AND cd.DetailType = 0
WHERE c.SecondaryEmail IS NOT NULL AND c.SecondaryEmail <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.ContactDetailId = cd.Id);

-- Link phone
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, ContactDetailId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 1, cd.Id, 1, NOW()
FROM Customers c
JOIN ContactDetails cd ON cd.Value = c.Phone AND cd.DetailType = 1
WHERE c.Phone IS NOT NULL AND c.Phone <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.ContactDetailId = cd.Id);

-- Link mobile
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, ContactDetailId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 1, cd.Id, 0, NOW()
FROM Customers c
JOIN ContactDetails cd ON cd.Value = c.MobilePhone AND cd.DetailType = 1
WHERE c.MobilePhone IS NOT NULL AND c.MobilePhone <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.ContactDetailId = cd.Id);

-- Link fax
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, ContactDetailId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 1, cd.Id, 0, NOW()
FROM Customers c
JOIN ContactDetails cd ON cd.Value = c.FaxNumber AND cd.DetailType = 2
WHERE c.FaxNumber IS NOT NULL AND c.FaxNumber <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.ContactDetailId = cd.Id);

-- Link addresses: join on line1+postal to find inserted Address rows
INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, AddressId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 0, a.Id, 1, NOW()
FROM Customers c
JOIN Address a ON a.Line1 = c.Address AND a.PostalCode = c.ZipCode
WHERE c.Address IS NOT NULL AND c.Address <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.AddressId = a.Id);

INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, AddressId, IsPrimaryForOwner, CreatedAt)
SELECT 0, c.Id, 0, a.Id, 0, NOW()
FROM Customers c
JOIN Address a ON a.Line1 = c.ShippingAddress AND a.PostalCode = c.ShippingZipCode
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress <> ''
  AND NOT EXISTS(SELECT 1 FROM ContactInfoLinks l WHERE l.OwnerType = 0 AND l.OwnerId = c.Id AND l.AddressId = a.Id);

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty (data migration is idempotent; rollback must be manual)
        }
    }
}
