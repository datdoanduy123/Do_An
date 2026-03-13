import React from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Briefcase, 
  Users, 
  Settings, 
  LogOut, 
  Bell, 
  Search,
  Menu,
  ShieldCheck,
  ChevronDown,
  UserCheck,
  Key,
  Layers
} from 'lucide-react';
import AuthService from '../services/AuthService';
import './MainLayout.css';

interface MainLayoutProps {
  children: React.ReactNode;
}

interface MenuItem {
  path?: string;
  icon: React.ReactNode;
  label: string;
  subItems?: { path: string; label: string; icon: React.ReactNode }[];
}

/**
 * MainLayout cung cấp cấu trúc Sidebar và Header đồng nhất cho ứng dụng.
 */
const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [isSidebarOpen, setSidebarOpen] = React.useState(true);
  const [openSubMenus, setOpenSubMenus] = React.useState<Record<string, boolean>>({ management: true });

  const handleLogout = () => {
    AuthService.logout();
    navigate('/login');
  };

  const toggleSubMenu = (key: string) => {
    setOpenSubMenus(prev => ({ ...prev, [key]: !prev[key] }));
  };

  const menuItems: MenuItem[] = [
    { path: '/dashboard', icon: <LayoutDashboard size={20} />, label: 'Tổng quan' },
    { path: '/projects', icon: <Briefcase size={20} />, label: 'Dự án' },
    { path: '/members', icon: <Users size={20} />, label: 'Thành viên' },
    { 
      label: 'Quản lý', 
      icon: <ShieldCheck size={20} />,
      subItems: [
        { path: '/management/roles', label: 'Vai trò', icon: <UserCheck size={18} /> },
        { path: '/management/permissions', label: 'Quyền', icon: <Key size={18} /> },
        { path: '/management/permission-groups', label: 'Nhóm quyền', icon: <Layers size={18} /> },
      ]
    },
    { path: '/settings', icon: <Settings size={20} />, label: 'Cấu hình' },
  ];

  return (
    <div className="layout-container">
      {/* Sidebar */}
      <aside className={`sidebar ${isSidebarOpen ? 'open' : 'closed'}`}>
        <div className="sidebar-header">
          <div className="logo-box">
            <div className="logo-icon">D</div>
            <span className="logo-text">DoAnHub</span>
          </div>
        </div>

        <nav className="sidebar-nav">
          {menuItems.map((item, idx) => {
            const isManagement = item.label === 'Quản lý';
            const hasSubItems = !!item.subItems;
            const isOpen = openSubMenus['management'] && isManagement;

            if (hasSubItems) {
              return (
                <div key={idx} className="nested-menu-container">
                  <div 
                    className={`nav-item clickable ${isOpen ? 'expanded' : ''}`} 
                    onClick={() => toggleSubMenu('management')}
                  >
                    {item.icon}
                    <span className="nav-label">{item.label}</span>
                    <ChevronDown size={16} className={`chevron-icon ${isOpen ? 'rotate' : ''}`} />
                  </div>
                  
                  {isOpen && isSidebarOpen && (
                    <div className="sub-menu">
                      {item.subItems?.map((sub, sIdx) => (
                        <Link 
                          key={sIdx} 
                          to={sub.path} 
                          className={`sub-nav-item ${location.pathname === sub.path ? 'active' : ''}`}
                        >
                          {sub.icon}
                          <span>{sub.label}</span>
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
              );
            }

            return (
              <Link 
                key={idx} 
                to={item.path!} 
                className={`nav-item ${location.pathname === item.path ? 'active' : ''}`}
              >
                {item.icon}
                <span className="nav-label">{item.label}</span>
                {location.pathname === item.path && <div className="active-indicator" />}
              </Link>
            );
          })}
        </nav>

        <div className="sidebar-footer">
          <button onClick={handleLogout} className="nav-item logout-btn">
            <LogOut size={20} />
            <span className="nav-label">Đăng xuất</span>
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="main-area">
        <header className="main-header">
          <div className="header-left">
            <button className="icon-btn toggle-sidebar" onClick={() => setSidebarOpen(!isSidebarOpen)}>
              <Menu size={20} />
            </button>
            <div className="search-bar">
              <Search size={18} className="search-icon" />
              <input type="text" placeholder="Tìm kiếm dự án, công việc..." />
            </div>
          </div>

          <div className="header-right">
            <button className="icon-btn notification-btn">
              <Bell size={20} />
              <div className="notification-badge" />
            </button>
            <div className="user-profile">
              <div className="user-info">
                <span className="user-name">Admin User</span>
                <span className="user-role">Quản trị viên</span>
              </div>
              <div className="user-avatar">
                <img src="https://ui-avatars.com/api/?name=Admin+User&background=6366f1&color=fff" alt="Avatar" />
              </div>
            </div>
          </div>
        </header>

        <div className="content-body">
          {children}
        </div>
      </main>
    </div>
  );
};

export default MainLayout;
