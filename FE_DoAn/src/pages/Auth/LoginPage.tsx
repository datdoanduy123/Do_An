import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthService from '../../services/AuthService';
import UserService from '../../services/UserService';
import './Login.css';

/**
 * Component trang Đăng nhập.
 * Hiển thị giao diện đăng nhập với phong cách Glassmorphism.
 */
const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [tenDangNhap, setTenDangNhap] = useState('');
  const [matKhau, setMatKhau] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  /**
   * Xử lý sự kiện gửi form đăng nhập.
   */
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const response = await AuthService.login({ tenDangNhap, matKhau });
      
      if (response.success && response.data?.token) {
        AuthService.setSession(response.data.token);
        
        try {
          // Lấy thông tin người dùng ngay sau khi đăng nhập để điều hướng đúng vai trò
          const profile = await UserService.getProfile();
          
          const isAdminOrManager = profile.vaiTros?.some((r: string) => {
            const nr = r.toLowerCase().replace(/\s+/g, '');
            return nr === 'quanly' || nr === 'admin' || nr === 'quảnlý';
          });

          // Điều hướng dựa trên vai trò
          if (isAdminOrManager) {
            navigate('/dashboard');
          } else {
            // Nếu là nhân viên, điều hướng về trang công việc của tôi
            navigate('/my-tasks');
          }
        } catch (profileError) {
          console.error('Lỗi khi lấy thông tin vai trò:', profileError);
          // Fallback mặc định nếu có lỗi lấy profile
          navigate('/my-tasks');
        }
      } else {
        setError(response.message || 'Tên đăng nhập hoặc mật khẩu không đúng.');
      }
    } catch (err) {
      setError('Đã xảy ra lỗi không xác định. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <h1>Đăng nhập</h1>
          <p>Chào mừng bạn trở lại với hệ thống</p>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label htmlFor="username">Tên đăng nhập</label>
            <div className="input-wrapper">
              <input
                id="username"
                type="text"
                placeholder="Nhập tên đăng nhập của bạn"
                value={tenDangNhap}
                onChange={(e) => setTenDangNhap(e.target.value)}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="password">Mật khẩu</label>
            <div className="input-wrapper">
              <input
                id="password"
                type="password"
                placeholder="••••••••"
                value={matKhau}
                onChange={(e) => setMatKhau(e.target.value)}
                required
              />
            </div>
          </div>

          <button type="submit" className="login-button" disabled={loading}>
            {loading ? 'Đang xử lý...' : 'Đăng nhập'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default LoginPage;
