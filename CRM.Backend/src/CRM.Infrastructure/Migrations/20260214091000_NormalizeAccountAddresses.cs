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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto;

public partial class NormalizeAccountAddresses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Billing addresses
        migrationBuilder.Sql(@"
INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, CreatedAt, UpdatedAt, IsDeleted)
SELECT 'Billing', c.Address, c.Address2, c.City, c.State, c.ZipCode, c.Country, NOW(), NOW(), 0
FROM Customers c
WHERE c.Address IS NOT NULL AND c.Address <> ''
  AND NOT EXISTS (
    SELECT 1 FROM Addresses a
    WHERE a.Line1 = c.Address
      AND (a.Line2 <=> c.Address2)
      AND a.City = c.City
      AND a.State = c.State
      AND (a.PostalCode <=> c.ZipCode)
      AND a.Country = c.Country
  );
");

        migrationBuilder.Sql(@"
INSERT INTO EntityAddressLinks (AddressId, EntityType, EntityId, AddressType, IsPrimary, CreatedAt, UpdatedAt, IsDeleted)
SELECT a.Id, 'Account', c.Id, 'Billing', 1, NOW(), NOW(), 0
FROM Customers c
JOIN Addresses a ON a.Line1 = c.Address
    AND (a.Line2 <=> c.Address2)
    AND a.City = c.City
    AND a.State = c.State
    AND (a.PostalCode <=> c.ZipCode)
    AND a.Country = c.Country
WHERE c.Address IS NOT NULL AND c.Address <> ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityAddressLinks l
    WHERE l.EntityType = 'Account'
      AND l.EntityId = c.Id
      AND l.AddressType = 'Billing'
      AND l.AddressId = a.Id
  );
");

        // Shipping addresses
        migrationBuilder.Sql(@"
INSERT INTO Addresses (Label, Line1, Line2, City, State, PostalCode, Country, CreatedAt, UpdatedAt, IsDeleted)
SELECT 'Shipping', c.ShippingAddress, NULL, c.ShippingCity, c.ShippingState, c.ShippingZipCode, c.ShippingCountry, NOW(), NOW(), 0
FROM Customers c
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress <> ''
  AND NOT EXISTS (
    SELECT 1 FROM Addresses a
    WHERE a.Line1 = c.ShippingAddress
      AND a.City = c.ShippingCity
      AND a.State = c.ShippingState
      AND (a.PostalCode <=> c.ShippingZipCode)
      AND a.Country = c.ShippingCountry
  );
");

        migrationBuilder.Sql(@"
INSERT INTO EntityAddressLinks (AddressId, EntityType, EntityId, AddressType, IsPrimary, CreatedAt, UpdatedAt, IsDeleted)
SELECT a.Id, 'Account', c.Id, 'Shipping', 1, NOW(), NOW(), 0
FROM Customers c
JOIN Addresses a ON a.Line1 = c.ShippingAddress
    AND a.City = c.ShippingCity
    AND a.State = c.ShippingState
    AND (a.PostalCode <=> c.ShippingZipCode)
    AND a.Country = c.ShippingCountry
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress <> ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityAddressLinks l
    WHERE l.EntityType = 'Account'
      AND l.EntityId = c.Id
      AND l.AddressType = 'Shipping'
      AND l.AddressId = a.Id
  );
");

        // Drop denormalized address columns from Customers
        migrationBuilder.DropColumn(name: "Address", table: "Customers");
        migrationBuilder.DropColumn(name: "Address2", table: "Customers");
        migrationBuilder.DropColumn(name: "City", table: "Customers");
        migrationBuilder.DropColumn(name: "State", table: "Customers");
        migrationBuilder.DropColumn(name: "ZipCode", table: "Customers");
        migrationBuilder.DropColumn(name: "Country", table: "Customers");
        migrationBuilder.DropColumn(name: "ShippingAddress", table: "Customers");
        migrationBuilder.DropColumn(name: "ShippingCity", table: "Customers");
        migrationBuilder.DropColumn(name: "ShippingState", table: "Customers");
        migrationBuilder.DropColumn(name: "ShippingZipCode", table: "Customers");
        migrationBuilder.DropColumn(name: "ShippingCountry", table: "Customers");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Address", table: "Customers", type: "varchar(500)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Address2", table: "Customers", type: "varchar(200)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "City", table: "Customers", type: "varchar(100)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "State", table: "Customers", type: "varchar(100)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ZipCode", table: "Customers", type: "varchar(20)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Country", table: "Customers", type: "varchar(100)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ShippingAddress", table: "Customers", type: "varchar(500)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ShippingCity", table: "Customers", type: "varchar(100)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ShippingState", table: "Customers", type: "varchar(100)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ShippingZipCode", table: "Customers", type: "varchar(20)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ShippingCountry", table: "Customers", type: "varchar(100)", nullable: true);

        // Down migrations for data restoration are intentionally omitted.
    }
}
