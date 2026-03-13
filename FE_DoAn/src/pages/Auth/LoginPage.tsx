import React, { useState } from 'react';
import AuthService from '../../services/AuthService';
import './Login.css';

/**
 * Component trang Đăng nhập.
 * Hiển thị giao diện đăng nhập với phong cách Glassmorphism.
 */
const LoginPage: React.FC = () => {
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
      
      if (response.success) {
        // Giả sử API trả về token trong data
        if (response.data && response.data.token) {
          AuthService.setSession(response.data.token);
        }
        alert('Đăng nhập thành công!');
        // Chuyển hướng người dùng (Redirect) ở đây nếu cần
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
