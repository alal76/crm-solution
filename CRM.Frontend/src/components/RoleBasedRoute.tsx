import React from 'react';
import { Navigate } from 'react-router-dom';
import { Alert } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { useProfile } from '../contexts/ProfileContext';

interface RoleBasedRouteProps {
  children: React.ReactNode;
  requiredPage?: string;
  requiredPermission?: string;
}

/**
 * Component that protects routes based on user profile and permissions
 * Can optionally require specific pages or permissions
 */
function RoleBasedRoute({ 
  children, 
  requiredPage,
  requiredPermission 
}: RoleBasedRouteProps) {
  const { isAuthenticated } = useAuth();
  const { profile, canAccessPage, hasPermission } = useProfile();

  // Not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // If no profile is set, allow access (fallback for users without assigned profiles)
  // Admin role (based on JWT) should have full access
  if (!profile) {
    return <>{children}</>;
  }

  // Check page access
  if (requiredPage && !canAccessPage(requiredPage)) {
    return (
      <div className="p-4">
        <Alert severity="error">
          <strong>Access Denied</strong>
          <p>You do not have permission to access this page.</p>
          <p>Your profile: {profile.userProfileName || 'Not assigned'}</p>
        </Alert>
      </div>
    );
  }

  // Check permission
  if (requiredPermission) {
    const permissionKey = requiredPermission as any;
    if (!hasPermission(permissionKey)) {
      return (
        <div className="p-4">
          <Alert severity="error">
            <strong>Access Denied</strong>
            <p>You do not have the required permission: {requiredPermission}</p>
          </Alert>
        </div>
      );
    }
  }

  return <>{children}</>;
}

export default RoleBasedRoute;
