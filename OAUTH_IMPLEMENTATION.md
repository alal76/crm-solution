# Google & Microsoft OAuth Implementation Guide

## Overview
This document describes the implementation of Google and Microsoft OAuth authentication in the CRM application. GitHub OAuth has been removed as requested.

## Features Implemented

### 1. **Google OAuth 2.0 Sign-In**
- Login with Google account
- Automatic user creation on first sign-in
- User information extracted from Google JWT token
- Works on both login and registration pages

### 2. **Microsoft OAuth 2.0 (Azure AD)**
- Login with Microsoft account
- Automatic user creation on first sign-in
- User information extracted from Microsoft ID token
- Supports organizational accounts

### 3. **Removed Features**
- GitHub OAuth has been completely removed from both frontend and backend

## Setup Instructions

### Frontend Configuration

#### 1. Install Dependencies
```bash
cd CRM.Frontend
npm install --legacy-peer-deps
```

#### 2. Environment Variables
Create a `.env` file in the `CRM.Frontend` directory:

```env
# Google OAuth Configuration
REACT_APP_GOOGLE_CLIENT_ID=your_google_client_id_here

# Microsoft OAuth Configuration
REACT_APP_MICROSOFT_CLIENT_ID=your_microsoft_client_id_here
REACT_APP_MICROSOFT_TENANT_ID=your_microsoft_tenant_id_here

# API Configuration
REACT_APP_API_BASE_URL=http://localhost:5000
```

### Getting OAuth Credentials

#### Google OAuth Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable the Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add authorized redirect URIs:
   - `http://localhost:3000` (for development)
   - Your production domain (for production)
6. Copy the **Client ID** to `.env`

#### Microsoft OAuth Setup
1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to Azure Active Directory > App registrations
3. Click "New registration"
4. Add Redirect URI: `http://localhost:3000/login` (for development)
5. Go to Certificates & secrets, create a new client secret
6. Copy the **Application (Client) ID** and **Tenant ID** to `.env`

## Frontend Implementation

### Components Updated

#### 1. **LoginPage.tsx**
- Added Google Sign-In button with automatic login
- Added Microsoft login button with OAuth 2.0 flow initiation
- Removed GitHub login option

#### 2. **RegisterPage.tsx**
- Added Google Sign-Up button
- Added Microsoft sign-up button
- Removed GitHub sign-up option

#### 3. **AuthContext.tsx**
- `googleLogin(idToken)` - Handles Google JWT token validation
- `initiateMicrosoftLogin()` - Initiates Microsoft OAuth 2.0 flow
- Automatically handles OAuth redirects and token storage

### OAuth Flow

#### Google OAuth Flow
```
User clicks "Google" button
    ↓
Google Sign-In widget renders
    ↓
User authenticates with Google
    ↓
Google returns JWT ID token
    ↓
Token sent to backend (/auth/oauth-login)
    ↓
User logged in or created
```

#### Microsoft OAuth Flow
```
User clicks "Microsoft" button
    ↓
Redirect to Microsoft login
    ↓
User authenticates with Microsoft
    ↓
Redirect back with authorization code
    ↓
Backend exchanges code for ID token (if implemented)
    ↓
User logged in or created
```

## Backend Implementation

### Authentication Service Updates

#### New Methods

1. **`ValidateProviderTokenAsync(provider, token)`**
   - Routes token validation to provider-specific methods
   - Supports: "google", "microsoft"

2. **`ValidateGoogleTokenAsync(token)`**
   - Decodes JWT token from Google
   - Extracts: sub (user ID), email, given_name, family_name
   - Returns: (userId, email, firstName, lastName)

3. **`ValidateMicrosoftTokenAsync(token)`**
   - Decodes JWT token from Microsoft
   - Extracts: oid (user ID), upn (email), name
   - Returns: (userId, email, firstName, lastName)

### OAuth Login Endpoint

**POST** `/api/auth/oauth-login`

Request body:
```json
{
  "provider": "google",  // or "microsoft"
  "token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEyMyJ9..."
}
```

Response:
```json
{
  "userId": 1,
  "username": "john_abc123",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Sales",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresAt": "2026-01-16T22:00:00Z"
}
```

### User Creation Logic
When an OAuth user logs in for the first time:
1. Extract user information from OAuth token
2. Check if email already exists (prevents duplicates)
3. Create new user with:
   - Email from OAuth provider
   - Username generated from email prefix + OAuth ID (first 6 chars)
   - First/Last name from OAuth token (if available)
   - Random password (never used for login)
   - Role: "Sales" (default)
4. Store OAuthToken record with provider and token info

## Security Considerations

### Current Implementation
- **Development Only**: Token validation currently parses JWT without signature verification
- **Production**: Implement proper JWT signature verification with provider public keys

### Recommended Production Changes

1. **Google Token Validation**
   ```csharp
   // Verify JWT signature with Google's public keys
   // Implement using https://github.com/googleapis/google-api-dotnet-client
   ```

2. **Microsoft Token Validation**
   ```csharp
   // Verify JWT signature with Microsoft's public keys
   // Use Azure AD configuration endpoint
   // Implement using Microsoft.IdentityModel.Tokens
   ```

3. **Token Expiration**
   - Check `exp` claim in token
   - Validate token not expired

4. **Audience Validation**
   - Verify `aud` claim matches your application
   - Ensure token intended for your app

## Testing

### Test Google Login
1. Go to `http://localhost:3000/login`
2. Click "Google" button
3. Authenticate with Google account
4. Should be redirected to dashboard

### Test Microsoft Login
1. Go to `http://localhost:3000/login`
2. Click "Microsoft" button
3. Authenticate with Microsoft account
4. Should be redirected to dashboard

### Test User Creation
1. Use a new Google/Microsoft account
2. First login should create a new user
3. Subsequent logins should use existing user
4. Check database for OAuthToken records

## Troubleshooting

### "Google Client ID not configured"
- Ensure `.env` has `REACT_APP_GOOGLE_CLIENT_ID`
- Restart frontend server after adding .env

### "Microsoft login failed"
- Verify `REACT_APP_MICROSOFT_CLIENT_ID` and `REACT_APP_MICROSOFT_TENANT_ID`
- Check redirect URIs in Azure AD app registration

### "Invalid token format"
- Verify token is properly formatted JWT
- Check token not expired
- Ensure token from correct provider

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Email/password login |
| POST | `/api/auth/register` | Email/password registration |
| POST | `/api/auth/oauth-login` | OAuth provider login |
| POST | `/api/auth/verify` | Verify JWT token |
| GET | `/api/auth/me` | Get current user profile |

## Environment Variables Reference

| Variable | Description | Required |
|----------|-------------|----------|
| `REACT_APP_GOOGLE_CLIENT_ID` | Google OAuth Client ID | Yes |
| `REACT_APP_MICROSOFT_CLIENT_ID` | Microsoft OAuth Client ID | Yes |
| `REACT_APP_MICROSOFT_TENANT_ID` | Microsoft Azure Tenant ID | Yes |
| `REACT_APP_API_BASE_URL` | Backend API URL | Yes |

## Files Modified

### Frontend
- `CRM.Frontend/package.json` - Added `@react-oauth/google`
- `CRM.Frontend/src/pages/LoginPage.tsx` - Implemented Google & Microsoft OAuth
- `CRM.Frontend/src/pages/RegisterPage.tsx` - Implemented Google & Microsoft OAuth signup
- `CRM.Frontend/src/contexts/AuthContext.tsx` - Added OAuth handlers
- `CRM.Frontend/.env.example` - Environment variables template

### Backend
- `CRM.Backend/src/CRM.Infrastructure/Services/AuthenticationService.cs`
  - Enhanced `OAuthLoginAsync()` with token validation
  - Added `ValidateProviderTokenAsync()`
  - Added `ValidateGoogleTokenAsync()`
  - Added `ValidateMicrosoftTokenAsync()`

## Next Steps

1. **Configure OAuth Credentials**: Get credentials from Google Cloud and Azure Portal
2. **Set Environment Variables**: Add credentials to `.env` file
3. **Test OAuth Flows**: Verify login/signup with both providers
4. **Implement Production Token Validation**: Add signature verification
5. **Deploy**: Configure production URLs in OAuth app settings

## Support

For issues or questions:
1. Check browser console for errors
2. Check backend logs for token validation errors
3. Verify OAuth app configuration in Google Cloud/Azure Portal
4. Ensure redirect URIs match your deployment URL
