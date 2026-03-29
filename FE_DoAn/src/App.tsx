import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/Auth/LoginPage';
import DashboardPage from './pages/Dashboard/DashboardPage';
import PermissionsPage from './pages/Management/PermissionsPage';
import RolesPage from './pages/Management/RolesPage';
import UsersPage from './pages/Management/UsersPage';
import SkillsPage from './pages/Management/SkillsPage';
import AiRulesPage from './pages/Management/AiRulesPage';
import ProjectsPage from './pages/Projects/ProjectsPage';
import ProjectDetailPage from './pages/Projects/ProjectDetailPage';
import SprintDetailPage from './pages/Projects/SprintDetailPage';
import ProfilePage from './pages/Management/ProfilePage';
import MyTasksPage from './pages/Tasks/MyTasksPage';
import MainLayout from './layouts/MainLayout';
import ProtectedRoute from './components/ProtectedRoute';
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
            <ProtectedRoute requiredRole="quanly">
              <MainLayout>
                <DashboardPage />
              </MainLayout>
            </ProtectedRoute>
          }
        />

        {/* Đường dẫn mặc định chuyển hướng sang Đăng nhập */}
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Có thể thêm các route khác tại đây */}
        <Route path="/projects" element={<ProtectedRoute><MainLayout><ProjectsPage /></MainLayout></ProtectedRoute>} />
        <Route path="/projects/:id" element={<ProtectedRoute><MainLayout><ProjectDetailPage /></MainLayout></ProtectedRoute>} />
        <Route path="/sprints/:id" element={<ProtectedRoute><MainLayout><SprintDetailPage /></MainLayout></ProtectedRoute>} />
        <Route path="/profile" element={<ProtectedRoute><MainLayout><ProfilePage /></MainLayout></ProtectedRoute>} />
        <Route path="/my-tasks" element={<ProtectedRoute><MainLayout><MyTasksPage /></MainLayout></ProtectedRoute>} />
        <Route path="/members" element={<ProtectedRoute><MainLayout><div>Trang Thành viên</div></MainLayout></ProtectedRoute>} />
        
        {/* Các trang quản trị yêu cầu quyền Quản lý */}
        <Route path="/management/users" element={<ProtectedRoute requiredRole="quanly"><MainLayout><UsersPage /></MainLayout></ProtectedRoute>} />
        <Route path="/management/roles" element={<ProtectedRoute requiredRole="quanly"><MainLayout><RolesPage /></MainLayout></ProtectedRoute>} />
        <Route path="/management/permissions" element={<ProtectedRoute requiredRole="quanly"><MainLayout><PermissionsPage /></MainLayout></ProtectedRoute>} />
        <Route path="/management/skills" element={<ProtectedRoute requiredRole="quanly"><MainLayout><SkillsPage /></MainLayout></ProtectedRoute>} />
        <Route path="/management/ai-rules" element={<ProtectedRoute requiredRole="quanly"><MainLayout><AiRulesPage /></MainLayout></ProtectedRoute>} />
        <Route path="/management/permission-groups" element={<ProtectedRoute requiredRole="quanly"><MainLayout><div>Trang Quản lý Nhóm quyền</div></MainLayout></ProtectedRoute>} />
        <Route path="/settings" element={<ProtectedRoute><MainLayout><div>Trang Cấu hình</div></MainLayout></ProtectedRoute>} />
      </Routes>
    </Router>
  );
}

export default App;
