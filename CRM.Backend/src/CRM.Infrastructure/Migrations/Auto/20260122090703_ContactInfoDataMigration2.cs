using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class ContactInfoDataMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                        // Data-only idempotent migration: materialize inline contact info into normalized tables
                        migrationBuilder.Sql(@"
                                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 'Primary', COALESCE(c.Address,''), c.Address2, c.City, c.State, c.ZipCode, c.Country, 1, CONCAT('migrated_customer:', c.Id), NOW(), 0
                                FROM Customers c
                                LEFT JOIN Addresses a ON a.Notes = CONCAT('migrated_customer:', c.Id)
                                WHERE a.Id IS NULL
                                    AND (COALESCE(c.Address,'') <> '' OR COALESCE(c.Address2,'') <> '' OR COALESCE(c.City,'') <> '' OR COALESCE(c.Country,'') <> '');

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.Email, 'Primary', 1, CONCAT('migrated_customer_email:', c.Id), NOW(), 0
                                FROM Customers c
                                LEFT JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_customer_email:', c.Id)
                                WHERE cd.Id IS NULL
                                    AND c.Email IS NOT NULL AND c.Email <> '';

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, c.Phone, 'Primary', 1, CONCAT('migrated_customer_phone:', c.Id), NOW(), 0
                                FROM Customers c
                                LEFT JOIN ContactDetails cd2 ON cd2.Notes = CONCAT('migrated_customer_phone:', c.Id)
                                WHERE cd2.Id IS NULL
                                    AND c.Phone IS NOT NULL AND c.Phone <> '';

                                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, c.LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_customer_social:', c.Id), NOW(), 0
                                FROM Customers c
                                LEFT JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_customer_social:', c.Id)
                                WHERE sa.Id IS NULL
                                    AND c.LinkedInUrl IS NOT NULL AND c.LinkedInUrl <> '';

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_address_from_customer', NOW(), 0
                                FROM Customers c
                                JOIN Addresses a ON a.Notes = CONCAT('migrated_customer:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_address_from_customer' AND l.OwnerType = 0 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_customer', NOW(), 0
                                FROM Customers c
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_customer_email:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_email_from_customer' AND l.OwnerType = 0 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_customer', NOW(), 0
                                FROM Customers c
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_customer_phone:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_phone_from_customer' AND l.OwnerType = 0 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_customer', NOW(), 0
                                FROM Customers c
                                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_customer_social:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_social_from_customer' AND l.OwnerType = 0 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;
                        ");

                        // Additional owners: Accounts, Contacts, Leads (same guarded pattern)
                        migrationBuilder.Sql(@"
                                -- Accounts
                                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 'Billing', COALESCE(acc.BillingAddress,''), NULL, acc.BillingCity, acc.BillingState, acc.BillingZip, acc.BillingCountry, 1, CONCAT('migrated_account_billing:', acc.Id), NOW(), 0
                                FROM Accounts acc
                                LEFT JOIN Addresses a ON a.Notes = CONCAT('migrated_account_billing:', acc.Id)
                                WHERE a.Id IS NULL
                                    AND (COALESCE(acc.BillingAddress,'') <> '' OR COALESCE(acc.BillingCity,'') <> '' OR COALESCE(acc.BillingCountry,'') <> '');

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 0, acc.BillingContactEmail, 'Billing', 1, CONCAT('migrated_account_email:', acc.Id), NOW(), 0
                                FROM Accounts acc
                                LEFT JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_account_email:', acc.Id)
                                WHERE cd.Id IS NULL
                                    AND acc.BillingContactEmail IS NOT NULL AND acc.BillingContactEmail <> '';

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, acc.BillingContactPhone, 'Billing', 1, CONCAT('migrated_account_phone:', acc.Id), NOW(), 0
                                FROM Accounts acc
                                LEFT JOIN ContactDetails cd2 ON cd2.Notes = CONCAT('migrated_account_phone:', acc.Id)
                                WHERE cd2.Id IS NULL
                                    AND acc.BillingContactPhone IS NOT NULL AND acc.BillingContactPhone <> '';

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 1, acc.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_billing_address_from_account', NOW(), 0
                                FROM Accounts acc
                                JOIN Addresses a ON a.Notes = CONCAT('migrated_account_billing:', acc.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_billing_address_from_account' AND l.OwnerType = 1 AND l.OwnerId = acc.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 1, acc.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_billing_contact_email_from_account', NOW(), 0
                                FROM Accounts acc
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_account_email:', acc.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_billing_contact_email_from_account' AND l.OwnerType = 1 AND l.OwnerId = acc.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 1, acc.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_billing_contact_phone_from_account', NOW(), 0
                                FROM Accounts acc
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_account_phone:', acc.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_billing_contact_phone_from_account' AND l.OwnerType = 1 AND l.OwnerId = acc.Id
                                WHERE l.Id IS NULL;

                                -- Contacts
                                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 'Primary', COALESCE(c.Address,''), c.Address2, c.City, c.State, c.ZipCode, c.Country, 1, CONCAT('migrated_contact:', c.Id), NOW(), 0
                                FROM Contacts c
                                LEFT JOIN Addresses a ON a.Notes = CONCAT('migrated_contact:', c.Id)
                                WHERE a.Id IS NULL
                                    AND (COALESCE(c.Address,'') <> '' OR COALESCE(c.Address2,'') <> '' OR COALESCE(c.City,'') <> '' OR COALESCE(c.Country,'') <> '');

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 0, c.EmailPrimary, 'Primary', 1, CONCAT('migrated_contact_email:', c.Id), NOW(), 0
                                FROM Contacts c
                                LEFT JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_contact_email:', c.Id)
                                WHERE cd.Id IS NULL
                                    AND c.EmailPrimary IS NOT NULL AND c.EmailPrimary <> '';

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, c.PhonePrimary, 'Primary', 1, CONCAT('migrated_contact_phone:', c.Id), NOW(), 0
                                FROM Contacts c
                                LEFT JOIN ContactDetails cd2 ON cd2.Notes = CONCAT('migrated_contact_phone:', c.Id)
                                WHERE cd2.Id IS NULL
                                    AND c.PhonePrimary IS NOT NULL AND c.PhonePrimary <> '';

                                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, c.LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_contact_social:', c.Id), NOW(), 0
                                FROM Contacts c
                                LEFT JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_contact_social:', c.Id)
                                WHERE sa.Id IS NULL
                                    AND c.LinkedInUrl IS NOT NULL AND c.LinkedInUrl <> '';

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 2, c.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_address_from_contact', NOW(), 0
                                FROM Contacts c
                                JOIN Addresses a ON a.Notes = CONCAT('migrated_contact:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_address_from_contact' AND l.OwnerType = 2 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 2, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_contact', NOW(), 0
                                FROM Contacts c
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_contact_email:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_email_from_contact' AND l.OwnerType = 2 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 2, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_contact', NOW(), 0
                                FROM Contacts c
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_contact_phone:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_phone_from_contact' AND l.OwnerType = 2 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 2, c.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_contact', NOW(), 0
                                FROM Contacts c
                                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_contact_social:', c.Id)
                                LEFT JOIN ContactInfoLinks l ON l.Notes = 'migrated_social_from_contact' AND l.OwnerType = 2 AND l.OwnerId = c.Id
                                WHERE l.Id IS NULL;
                        ");

                        migrationBuilder.Sql(@"
                                -- Leads
                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 0, l.Email, 'Primary', 1, CONCAT('migrated_lead_email:', l.Id), NOW(), 0
                                FROM Leads l
                                LEFT JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_lead_email:', l.Id)
                                WHERE cd.Id IS NULL
                                    AND l.Email IS NOT NULL AND l.Email <> '';

                                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, l.Phone, 'Primary', 1, CONCAT('migrated_lead_phone:', l.Id), NOW(), 0
                                FROM Leads l
                                LEFT JOIN ContactDetails cd2 ON cd2.Notes = CONCAT('migrated_lead_phone:', l.Id)
                                WHERE cd2.Id IS NULL
                                    AND l.Phone IS NOT NULL AND l.Phone <> '';

                                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                                SELECT 1, l.LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_lead_social:', l.Id), NOW(), 0
                                FROM Leads l
                                LEFT JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_lead_social:', l.Id)
                                WHERE sa.Id IS NULL
                                    AND l.LinkedInUrl IS NOT NULL AND l.LinkedInUrl <> '';

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 3, l.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_lead', NOW(), 0
                                FROM Leads l
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_lead_email:', l.Id)
                                LEFT JOIN ContactInfoLinks li ON li.Notes = 'migrated_email_from_lead' AND li.OwnerType = 3 AND li.OwnerId = l.Id
                                WHERE li.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 3, l.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_lead', NOW(), 0
                                FROM Leads l
                                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_lead_phone:', l.Id)
                                LEFT JOIN ContactInfoLinks li ON li.Notes = 'migrated_phone_from_lead' AND li.OwnerType = 3 AND li.OwnerId = l.Id
                                WHERE li.Id IS NULL;

                                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                                SELECT 3, l.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_lead', NOW(), 0
                                FROM Leads l
                                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_lead_social:', l.Id)
                                LEFT JOIN ContactInfoLinks li ON li.Notes = 'migrated_social_from_lead' AND li.OwnerType = 3 AND li.OwnerId = l.Id
                                WHERE li.Id IS NULL;
                        ");
                }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ContactInfoLinks WHERE Notes LIKE 'migrated_%';
                DELETE FROM Addresses WHERE Notes LIKE 'migrated_%';
                DELETE FROM ContactDetails WHERE Notes LIKE 'migrated_%';
                DELETE FROM SocialAccounts WHERE Notes LIKE 'migrated_%';
            ");
        }
    }
}
