import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/Auth/LoginPage';
import DashboardPage from './pages/Dashboard/DashboardPage';
import PermissionsPage from './pages/Management/PermissionsPage';
import RolesPage from './pages/Management/RolesPage';
import UsersPage from './pages/Management/UsersPage';
import SkillsPage from './pages/Management/SkillsPage';
import MainLayout from './layouts/MainLayout';
import './App.css';

/**
 * Thành phần chính của ứng dụng, quản lý điều hướng.
 */
function App() {
  return (
    <Router>
      <Routes>
        {/* Đường dẫn trang Đăng nhập không dùng Layout chung */}
        <Route path="/login" element={<LoginPage />} />
        
        {/* Các đường dẫn yêu cầu Layout chung */}
        <Route 
          path="/dashboard" 
          element={
            <MainLayout>
              <DashboardPage />
            </MainLayout>
          } 
        />
        
        {/* Đường dẫn mặc định chuyển hướng sang Đăng nhập hoặc Dashboard tùy trạng thái auth */}
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        
        {/* Có thể thêm các route khác tại đây */}
        <Route path="/projects" element={<MainLayout><div>Trang Dự án</div></MainLayout>} />
        <Route path="/members" element={<MainLayout><div>Trang Thành viên</div></MainLayout>} />
        <Route path="/management/users" element={<MainLayout><UsersPage /></MainLayout>} />
        <Route path="/management/roles" element={<MainLayout><RolesPage /></MainLayout>} />
        <Route path="/management/permissions" element={<MainLayout><PermissionsPage /></MainLayout>} />
        <Route path="/management/skills" element={<MainLayout><SkillsPage /></MainLayout>} />
        <Route path="/management/permission-groups" element={<MainLayout><div>Trang Quản lý Nhóm quyền</div></MainLayout>} />
        <Route path="/settings" element={<MainLayout><div>Trang Cấu hình</div></MainLayout>} />
      </Routes>
    </Router>
  );
}

export default App;
