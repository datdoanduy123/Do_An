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
          const normalize = (s: string) => s.toLowerCase().replace(/\s+/g, '');
          const normalizedTarget = normalize(target);
          return roles.some(r => {
            const normalizedR = normalize(r);
            return normalizedR === normalizedTarget || normalizedR === 'admin' || normalizedR === 'quảnlý';
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
