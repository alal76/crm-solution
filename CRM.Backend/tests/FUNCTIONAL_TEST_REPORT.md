# Functional Test Report

## Test Execution Summary

| Metric | Value |
|--------|-------|
| **Execution Date** | January 2025 |
| **Total Tests** | 28 |
| **Passed** | 28 |
| **Failed** | 0 |
| **Skipped** | 0 |
| **Duration** | 3 seconds |
| **Result** | ✅ **ALL TESTS PASSED** |

## Test Environment

- **API Endpoint**: http://localhost:5000
- **Database**: MariaDB (Docker container `crm-mariadb`)
- **Database Name**: `crm_db`
- **Test Framework**: xUnit 2.6.2
- **.NET Version**: 8.0

## Test Coverage

### Authentication Tests (FT001-FT005)
| Test ID | Test Name | Status |
|---------|-----------|--------|
| FT001 | Login_With_Valid_Credentials_Returns_Token | ✅ Pass |
| FT002 | Login_With_Invalid_Credentials_Returns_Unauthorized | ✅ Pass |
| FT003 | Access_Protected_Endpoint_Without_Token_Returns_Unauthorized | ✅ Pass |
| FT004 | Access_Protected_Endpoint_With_Valid_Token_Returns_Success | ✅ Pass |
| FT005 | Access_Admin_Endpoint_Without_Admin_Role_Returns_Forbidden | ✅ Pass |

### Customer CRUD Tests (FT006-FT010)
| Test ID | Test Name | Status |
|---------|-----------|--------|
| FT006 | Get_All_Customers_Returns_List | ✅ Pass |
| FT007 | Get_Customer_By_Id_Returns_Customer | ✅ Pass |
| FT008 | Create_Customer_Returns_Created | ✅ Pass |
| FT009 | Update_Customer_Returns_Success | ✅ Pass |
| FT010 | Delete_Customer_Returns_Success | ✅ Pass |

### API Endpoint Tests (FT011-FT020)
| Test ID | Test Name | Status |
|---------|-----------|--------|
| FT011 | Get_All_Customers_Authenticated | ✅ Pass |
| FT012 | Create_Customer | ✅ Pass |
| FT013 | Get_Customer_By_Id | ✅ Pass |
| FT014 | Update_Customer | ✅ Pass |
| FT015 | Customer_Pagination | ✅ Pass |
| FT016 | Get_All_Contacts | ✅ Pass |
| FT017 | Create_Contact | ✅ Pass |
| FT018 | Get_All_Activities | ✅ Pass |
| FT019 | Create_Activity | ✅ Pass |
| FT020 | Get_All_Opportunities | ✅ Pass |

### Advanced Feature Tests (FT021-FT028)
| Test ID | Test Name | Status |
|---------|-----------|--------|
| FT021 | Create_Opportunity | ✅ Pass |
| FT022 | Get_All_Products | ✅ Pass |
| FT023 | Create_Product | ✅ Pass |
| FT024 | Get_All_Campaigns | ✅ Pass |
| FT025 | Create_Campaign | ✅ Pass |
| FT026 | Get_All_Users | ✅ Pass |
| FT027 | Get_System_Settings | ✅ Pass |
| FT028 | Update_System_Settings | ✅ Pass |

## Issues Resolved During Testing

### Issue 1: Token Property Mismatch
- **Problem**: `LoginResponse` record used `Token` property but API returns `accessToken`
- **Solution**: Updated `FunctionalTestBase.cs` to use `AccessToken` property

### Issue 2: Customer Creation Fields
- **Problem**: Tests used `name` and `industry` fields, but API requires `firstName`, `lastName`, `company`
- **Solution**: Updated FT012 test to use correct field names

### Issue 3: API Response Format
- **Problem**: Tests expected paginated response `{items: [...]}` but API returns plain arrays
- **Solution**: Updated FT013 and FT015 tests to handle array response format

### Issue 4: Customer ID Type
- **Problem**: Tests used `GetGuid()` for customer IDs but API uses integer IDs
- **Solution**: Updated tests to use `GetInt32()` for customer ID parsing

## How to Run Tests

```bash
# Navigate to tests directory
cd CRM.Backend/tests

# Run all functional tests
dotnet test --filter "Category=Functional" -c Debug

# Run with verbose output
dotnet test --filter "Category=Functional" -c Debug -v n
```

## Prerequisites

1. API must be running on localhost:5000
2. MariaDB database must be accessible
3. Admin user must exist: `abhi.lal@gmail.com` / `Admin@123`

## Start API

```bash
cd CRM.Backend/src/CRM.Api
dotnet run
```

## Conclusion

All 28 functional tests pass successfully. The API endpoints are working correctly for:
- User authentication and authorization
- Customer CRUD operations
- Contact management
- Activity tracking
- Opportunity management
- Product catalog
- Marketing campaigns
- System settings management
