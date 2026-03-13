import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/Auth/LoginPage';
import './App.css';

/**
 * Thành phần chính của ứng dụng, quản lý điều hướng.
 */
function App() {
  return (
    <Router>
      <Routes>
        {/* Đường dẫn mặc định chuyển hướng sang Đăng nhập */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        
        {/* Đường dẫn trang Đăng nhập */}
        <Route path="/login" element={<LoginPage />} />
        
        {/* Có thể thêm các route khác của đồ án tại đây */}
        {/* <Route path="/dashboard" element={<DashboardPage />} /> */}
      </Routes>
    </Router>
  );
}

export default App;
