import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import UserService from '../services/UserService';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string;
}

/**
 * Thành phần bảo vệ route dựa trên trạng thái đăng nhập và vai trò.
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRole }) => {
  const [authorized, setAuthorized] = React.useState<boolean | null>(null);
  const location = useLocation();
  const token = sessionStorage.getItem('accessToken');

  React.useEffect(() => {
    const checkAuth = async () => {
      if (!token) {
        setAuthorized(false);
        return;
      }

      if (!requiredRole) {
        setAuthorized(true);
        return;
      }

      try {
        const profile = await UserService.getProfile();
        const checkRole = (roles: string[], target: string) => {
          if (!roles || !Array.isArray(roles)) return false;
          const normalizedTarget = String(target).toUpperCase().trim().replace(/\s+/g, '');
          return roles.some(r => {
            const normalizedR = String(r).toUpperCase().trim().replace(/\s+/g, '');
            return normalizedR === normalizedTarget || 
                   normalizedR === 'ADMIN' || 
                   normalizedR === 'QUẢNLÝ' || 
                   normalizedR === 'QUAN_LY' ||
                   normalizedR === 'QUANLY';
          });
        };
        const hasRole = checkRole(profile.vaiTros, requiredRole);
        setAuthorized(hasRole);
      } catch (error) {
        setAuthorized(false);
      }
    };

    checkAuth();
  }, [token, requiredRole]);

  if (authorized === null) return <div className="loading-screen">Đang xác thực quyền truy cập...</div>;

  if (!authorized) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
