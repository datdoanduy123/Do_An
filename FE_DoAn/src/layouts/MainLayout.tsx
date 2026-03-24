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
  Layers,
  CheckCircle2
} from 'lucide-react';
import AuthService from '../services/AuthService';
import UserService from '../services/UserService';
import type { NguoiDungDto } from '../services/UserService';
import SignalRService from '../services/SignalRService';
import NotificationService, { type Notification } from '../services/NotificationService';
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
  const [user, setUser] = React.useState<NguoiDungDto | null>(null);

  // Notifications State
  const [notifications, setNotifications] = React.useState<Notification[]>([]);
  const [showNotifications, setShowNotifications] = React.useState(false);
  const [unreadCount, setUnreadCount] = React.useState(0);

  const fetchNotifications = async (userId: number) => {
    try {
      const data = await NotificationService.getByUser(userId);
      setNotifications(data);
      const count = await NotificationService.getUnreadCount(userId);
      setUnreadCount(count);
    } catch (err) {
      console.error('Failed to fetch notifications:', err);
    }
  };

  React.useEffect(() => {
    // Khởi tạo SignalR
    const initSignalR = async (userId: number) => {
      await SignalRService.startConnection();
      await SignalRService.joinUser(userId);
      
      // Lắng nghe sự kiện cập nhật bảng Kanban (toàn dự án)
      SignalRService.on('TaskUpdated', () => {
         // Cập nhật ngầm (silent) nếu đang ở trang dự án
      });

      // Lắng nghe sự kiện thông báo cá nhân (đẩy vào Bell Icon)
      SignalRService.off('ReceiveNotification');
      SignalRService.on('ReceiveNotification', (data: { id: number, title: string, message: string }) => {
        const newNotif: Notification = {
          id: data.id,
          userId: userId,
          title: data.title,
          message: data.message,
          createdAt: new Date().toISOString(),
          isRead: false
        };
        setNotifications((prev: Notification[]) => [newNotif, ...prev].slice(0, 20));
        setUnreadCount((prev: number) => prev + 1);
      });
    };

    const fetchUser = async () => {
      try {
        const profile = await UserService.getProfile();
        setUser(profile);
        if (profile) {
          initSignalR(profile.id);
          fetchNotifications(profile.id);
        }
      } catch (error) {
        console.error('Failed to fetch user profile:', error);
      }
    };
    fetchUser();

    return () => {
      SignalRService.off('TaskUpdated');
      SignalRService.off('ReceiveNotification');
    };
  }, []);

  const clearAllNotifications = async () => {
    if (!user) return;
    try {
      await NotificationService.deleteAll(user.id);
      setNotifications([]);
      setUnreadCount(0);
    } catch (err) {
      console.error('Failed to clear notifications:', err);
    }
  };

  const markAllAsRead = async () => {
    if (!user) return;
    try {
      await NotificationService.markAllRead(user.id);
      setNotifications((prev: Notification[]) => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      console.error('Failed to mark all as read:', err);
    }
  };

  const markAsRead = async (notifId: number) => {
    try {
      await NotificationService.markRead(notifId);
      setNotifications((prev: Notification[]) => prev.map(n => 
        n.id === notifId ? { ...n, isRead: true } : n
      ));
      setUnreadCount((prev: number) => Math.max(0, prev - 1));
    } catch (err) {
      console.error('Failed to mark as read:', err);
    }
  };

  const handleLogout = () => {
    AuthService.logout();
    navigate('/login');
  };

  const toggleSubMenu = (key: string) => {
    setOpenSubMenus(prev => ({ ...prev, [key]: !prev[key] }));
  };

  const menuItems: MenuItem[] = [];
  
  // Chỉ hiển thị menu Quản lý nếu là Quản lý hoặc Admin
  const checkIsAdmin = (roles: string[]) => {
    return roles.some(r => {
      const normalized = r.toLowerCase().replace(/\s+/g, '');
      return normalized === 'quanly' || normalized === 'admin' || normalized === 'quảnlý';
    });
  };

  const isAdmin = user ? checkIsAdmin(user.vaiTros) : false;

  // Quyền truy cập Tổng quan chỉ dành cho Quản lý
  if (isAdmin) {
    menuItems.push({ path: '/dashboard', icon: <LayoutDashboard size={20} />, label: 'Tổng quan' });
  }

  // Menu chung cho mọi người (nhưng Dashboard đã bị tách ra trên)
  menuItems.push(
    { path: '/projects', icon: <Briefcase size={20} />, label: 'Dự án' },
    { path: '/my-tasks', icon: <CheckCircle2 size={20} />, label: 'Công việc của tôi' },
    { path: '/members', icon: <Users size={20} />, label: 'Thành viên' }
  );
  
  if (isAdmin) {
    menuItems.push({ 
      label: 'Quản lý', 
      icon: <ShieldCheck size={20} />,
      subItems: [
        { path: '/management/users', label: 'Người dùng', icon: <Users size={18} /> },
        { path: '/management/roles', label: 'Vai trò', icon: <UserCheck size={18} /> },
        { path: '/management/permissions', label: 'Quyền', icon: <Key size={18} /> },
        { path: '/management/permission-groups', label: 'Nhóm quyền', icon: <Layers size={18} /> },
        { path: '/management/skills', label: 'Kỹ năng', icon: <Search size={18} /> },
        { path: '/management/task-approval', label: 'Duyệt công việc', icon: <CheckCircle2 size={18} /> },
      ]
    });
  }

  menuItems.push({ path: '/settings', icon: <Settings size={20} />, label: 'Cấu hình' });

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
            <div className="notification-wrapper">
              <button 
                className="icon-btn notification-btn"
                onClick={() => {
                  setShowNotifications(!showNotifications);
                  if (!showNotifications) setUnreadCount(0);
                }}
              >
                <Bell size={20} />
                {unreadCount > 0 && <div className="notification-badge-count">{unreadCount}</div>}
              </button>

                {showNotifications && (
                  <div className="notification-dropdown">
                    <div className="dropdown-header">
                      <h3>Thông báo</h3>
                      <div className="header-actions">
                        <button className="text-btn" onClick={markAllAsRead}>Đã đọc hết</button>
                        <button className="text-btn" onClick={clearAllNotifications}>Xóa tất cả</button>
                      </div>
                    </div>
                    <div className="dropdown-content">
                      {notifications.length > 0 ? (
                        notifications.map((notif) => (
                          <div 
                            key={notif.id} 
                            className={`notification-item ${notif.isRead ? 'read' : 'unread'}`}
                            onClick={() => !notif.isRead && markAsRead(notif.id)}
                          >
                            <div className="notif-icon"><Bell size={16} /></div>
                            <div className="notif-info">
                              <p className="notif-message">{notif.message}</p>
                              <span className="notif-time">
                                {new Date(notif.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                              </span>
                            </div>
                          </div>
                        ))
                      ) : (
                        <div className="empty-notif">Không có thông báo mới</div>
                      )}
                    </div>
                  </div>
                )}
            </div>
            <div 
              className="user-profile clickable" 
              onClick={() => navigate('/profile')}
              title="Xem thông tin cá nhân"
            >
              <div className="user-info">
                <span className="user-name">{user?.hoTen || 'Đang tải...'}</span>
                <span className="user-role">{isAdmin ? 'Quản trị viên' : 'Nhân viên'}</span>
              </div>
              <div className="user-avatar">
                <img src={`https://ui-avatars.com/api/?name=${encodeURIComponent(user?.hoTen || 'U')}&background=6366f1&color=fff`} alt="Avatar" />
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
