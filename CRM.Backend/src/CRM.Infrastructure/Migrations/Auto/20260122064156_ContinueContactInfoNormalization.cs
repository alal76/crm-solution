using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class ContinueContactInfoNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "ContactInfoLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactDetailId",
                table: "ContactInfoLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SocialAccountId",
                table: "ContactInfoLinks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_AddressId",
                table: "ContactInfoLinks",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_ContactDetailId",
                table: "ContactInfoLinks",
                column: "ContactDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfoLinks_SocialAccountId",
                table: "ContactInfoLinks",
                column: "SocialAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfoLinks_Addresses_AddressId",
                table: "ContactInfoLinks",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfoLinks_ContactDetails_ContactDetailId",
                table: "ContactInfoLinks",
                column: "ContactDetailId",
                principalTable: "ContactDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInfoLinks_SocialAccounts_SocialAccountId",
                table: "ContactInfoLinks",
                column: "SocialAccountId",
                principalTable: "SocialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Data migration: materialize inline addresses, emails, phones and social links
            // Customers -> Addresses / ContactDetails / SocialAccounts + ContactInfoLinks
            migrationBuilder.Sql(@"
                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 'Primary', COALESCE(Address, ''), Address2, City, State, ZipCode, Country, 1, CONCAT('migrated_customer:', Id), NOW(), 0
                FROM Customers
                WHERE (COALESCE(Address,'') <> '' OR COALESCE(Address2,'') <> '' OR COALESCE(City,'') <> '' OR COALESCE(Country,'') <> '');

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 0, Email, 'Primary', 1, CONCAT('migrated_customer_email:', Id), NOW(), 0
                FROM Customers
                WHERE Email IS NOT NULL AND Email <> '';

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, Phone, 'Primary', 1, CONCAT('migrated_customer_phone:', Id), NOW(), 0
                FROM Customers
                WHERE Phone IS NOT NULL AND Phone <> '';

                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_customer_social:', Id), NOW(), 0
                FROM Customers
                WHERE LinkedInUrl IS NOT NULL AND LinkedInUrl <> '';

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 0, c.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_address_from_customer', NOW(), 0
                FROM Customers c
                JOIN Addresses a ON a.Notes = CONCAT('migrated_customer:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 0, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_customer', NOW(), 0
                FROM Customers c
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_customer_email:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 0, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_customer', NOW(), 0
                FROM Customers c
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_customer_phone:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 0, c.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_customer', NOW(), 0
                FROM Customers c
                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_customer_social:', c.Id);
            ");

            // Accounts -> Billing address & billing contact
            migrationBuilder.Sql(@"
                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 'Billing', COALESCE(BillingAddress,''), NULL, BillingCity, BillingState, BillingZip, BillingCountry, 1, CONCAT('migrated_account_billing:', Id), NOW(), 0
                FROM Accounts
                WHERE (COALESCE(BillingAddress,'') <> '' OR COALESCE(BillingCity,'') <> '' OR COALESCE(BillingCountry,'') <> '');

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 0, BillingContactEmail, 'Billing', 1, CONCAT('migrated_account_email:', Id), NOW(), 0
                FROM Accounts
                WHERE BillingContactEmail IS NOT NULL AND BillingContactEmail <> '';

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, BillingContactPhone, 'Billing', 1, CONCAT('migrated_account_phone:', Id), NOW(), 0
                FROM Accounts
                WHERE BillingContactPhone IS NOT NULL AND BillingContactPhone <> '';

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 1, acc.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_billing_address_from_account', NOW(), 0
                FROM Accounts acc
                JOIN Addresses a ON a.Notes = CONCAT('migrated_account_billing:', acc.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 1, acc.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_billing_contact_email_from_account', NOW(), 0
                FROM Accounts acc
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_account_email:', acc.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 1, acc.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_billing_contact_phone_from_account', NOW(), 0
                FROM Accounts acc
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_account_phone:', acc.Id);
            ");

            // Contacts -> primary address, email, phone, socials
            migrationBuilder.Sql(@"
                INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 'Primary', COALESCE(Address,''), Address2, City, State, ZipCode, Country, 1, CONCAT('migrated_contact:', Id), NOW(), 0
                FROM Contacts
                WHERE (COALESCE(Address,'') <> '' OR COALESCE(Address2,'') <> '' OR COALESCE(City,'') <> '' OR COALESCE(Country,'') <> '');

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 0, EmailPrimary, 'Primary', 1, CONCAT('migrated_contact_email:', Id), NOW(), 0
                FROM Contacts
                WHERE EmailPrimary IS NOT NULL AND EmailPrimary <> '';

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, PhonePrimary, 'Primary', 1, CONCAT('migrated_contact_phone:', Id), NOW(), 0
                FROM Contacts
                WHERE PhonePrimary IS NOT NULL AND PhonePrimary <> '';

                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_contact_social:', Id), NOW(), 0
                FROM Contacts
                WHERE LinkedInUrl IS NOT NULL AND LinkedInUrl <> '';

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 2, c.Id, 0, a.Id, a.Id, NULL, NULL, 1, 'migrated_address_from_contact', NOW(), 0
                FROM Contacts c
                JOIN Addresses a ON a.Notes = CONCAT('migrated_contact:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 2, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_contact', NOW(), 0
                FROM Contacts c
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_contact_email:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 2, c.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_contact', NOW(), 0
                FROM Contacts c
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_contact_phone:', c.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 2, c.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_contact', NOW(), 0
                FROM Contacts c
                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_contact_social:', c.Id);
            ");

            // Leads -> primary email/phone/social
            migrationBuilder.Sql(@"
                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 0, Email, 'Primary', 1, CONCAT('migrated_lead_email:', Id), NOW(), 0
                FROM Leads
                WHERE Email IS NOT NULL AND Email <> '';

                INSERT INTO ContactDetails (DetailType, Value, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, Phone, 'Primary', 1, CONCAT('migrated_lead_phone:', Id), NOW(), 0
                FROM Leads
                WHERE Phone IS NOT NULL AND Phone <> '';

                INSERT INTO SocialAccounts (Network, HandleOrUrl, Label, IsPrimary, Notes, CreatedAt, IsDeleted)
                SELECT 1, LinkedInUrl, 'LinkedIn', 1, CONCAT('migrated_lead_social:', Id), NOW(), 0
                FROM Leads
                WHERE LinkedInUrl IS NOT NULL AND LinkedInUrl <> '';

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 3, l.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_email_from_lead', NOW(), 0
                FROM Leads l
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_lead_email:', l.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 3, l.Id, 1, cd.Id, NULL, cd.Id, NULL, 1, 'migrated_phone_from_lead', NOW(), 0
                FROM Leads l
                JOIN ContactDetails cd ON cd.Notes = CONCAT('migrated_lead_phone:', l.Id);

                INSERT INTO ContactInfoLinks (OwnerType, OwnerId, InfoKind, InfoId, AddressId, ContactDetailId, SocialAccountId, IsPrimaryForOwner, Notes, CreatedAt, IsDeleted)
                SELECT 3, l.Id, 2, sa.Id, NULL, NULL, sa.Id, 1, 'migrated_social_from_lead', NOW(), 0
                FROM Leads l
                JOIN SocialAccounts sa ON sa.Notes = CONCAT('migrated_lead_social:', l.Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfoLinks_Addresses_AddressId",
                table: "ContactInfoLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfoLinks_ContactDetails_ContactDetailId",
                table: "ContactInfoLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactInfoLinks_SocialAccounts_SocialAccountId",
                table: "ContactInfoLinks");

            migrationBuilder.DropIndex(
                name: "IX_ContactInfoLinks_AddressId",
                table: "ContactInfoLinks");

            migrationBuilder.DropIndex(
                name: "IX_ContactInfoLinks_ContactDetailId",
                table: "ContactInfoLinks");

            migrationBuilder.DropIndex(
                name: "IX_ContactInfoLinks_SocialAccountId",
                table: "ContactInfoLinks");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "ContactInfoLinks");

            migrationBuilder.DropColumn(
                name: "ContactDetailId",
                table: "ContactInfoLinks");

            migrationBuilder.DropColumn(
                name: "SocialAccountId",
                table: "ContactInfoLinks");
        }
    }
}
