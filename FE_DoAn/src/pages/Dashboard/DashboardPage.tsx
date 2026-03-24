import React, { useEffect, useState } from 'react';
import { 
  CheckCircle2, 
  Clock, 
  AlertCircle, 
  Calendar,
  Briefcase,
  Star,
  ChevronRight,
  TrendingUp,
  Users,
  Target,
  User as UserIcon
} from 'lucide-react';
import {
  PieChart, Pie, Cell, ResponsiveContainer, Tooltip as RechartsTooltip, Legend,
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  AreaChart, Area
} from 'recharts';
import DashboardService from '../../services/DashboardService';
import type { DashboardStats } from '../../services/DashboardService';
import UserService from '../../services/UserService';
import './Dashboard.css';

/**
 * Trang Dashboard quản lý v2 với giao diện được thiết kế lại theo phong cách Premium Crystal.
 */
const DashboardPage: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [userProfile, setUserProfile] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [dashboardStats, profile] = await Promise.all([
          DashboardService.getDashboardData(),
          UserService.getProfile()
        ]);
        setStats(dashboardStats);
        setUserProfile(profile);
      } catch (error) {
        console.error('Error loading dashboard:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="loading-container-full">
        <div className="modern-loader"></div>
        <p style={{ fontWeight: 600, color: '#6366f1' }}>Đang tải dữ liệu tổng quan...</p>
      </div>
    );
  }

  // Bảng màu rực rỡ hơn cho biểu đồ
  const COLORS = ['#94a3b8', '#6366f1', '#f59e0b', '#10b981', '#ef4444'];

  return (
    <div className="dashboard-container-v2">
      {/* Header Welcome - Redesigned with Avatar */}
      <header className="dashboard-header-v2">
        <div className="welcome-section">
          <div className="welcome-greet">
             <div className="user-avatar-main">
                {userProfile?.hoTen?.charAt(0) || <UserIcon size={24}/>}
             </div>
             <div className="greet-text">
                <h1>Chào mừng, {userProfile?.hoTen || 'Quản lý'}! 👋</h1>
                <p>Hôm nay có <strong>{stats?.pendingReviews}</strong> công việc đang chờ bạn phê duyệt.</p>
             </div>
          </div>
        </div>
        <div className="header-actions">
          <div className="date-badge">
            <Calendar size={18} />
            <span>{new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' })}</span>
          </div>
        </div>
      </header>

      {/* Main Stats Grid */}
      <div className="stats-grid-v2">
        <div className="stat-card-glass total-projects">
          <div className="stat-icon"><Briefcase /></div>
          <div className="stat-info">
            <span className="label">Tổng dự án</span>
            <span className="value">{stats?.totalProjects}</span>
          </div>
          <div className="stat-trend positive">+2 tháng này</div>
        </div>
        <div className="stat-card-glass in-progress">
          <div className="stat-icon"><Clock /></div>
          <div className="stat-info">
            <span className="label">Đang triển khai</span>
            <span className="value">{stats?.inProgressTasks}</span>
          </div>
        </div>
        <div className="stat-card-glass pending">
          <div className="stat-icon"><AlertCircle /></div>
          <div className="stat-info">
            <span className="label">Chờ phê duyệt</span>
            <span className="value">{stats?.pendingReviews}</span>
          </div>
        </div>
        <div className="stat-card-glass completed">
          <div className="stat-icon"><CheckCircle2 /></div>
          <div className="stat-info">
            <span className="label">Đã hoàn thành</span>
            <span className="value">{stats?.completedTasks}</span>
          </div>
        </div>
      </div>

      <div className="dashboard-content-layout">
        {/* Left Column: Charts */}
        <div className="charts-column">
          <div className="chart-row">
            <div className="chart-card glass">
              <h3><Target size={20} /> Trạng thái công việc</h3>
              <div className="chart-wrapper">
                <ResponsiveContainer width="100%" height={250}>
                  <PieChart>
                    <Pie
                      data={stats?.taskStatusDistribution}
                      cx="50%"
                      cy="50%"
                      innerRadius={65}
                      outerRadius={85}
                      paddingAngle={5}
                      dataKey="count"
                      nameKey="status"
                    >
                      {stats?.taskStatusDistribution.map((entry: any, index: number) => (
                        <Cell key={`cell-${index}`} fill={entry.color || COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <RechartsTooltip 
                      contentStyle={{ backgroundColor: 'white', border: 'none', borderRadius: '12px', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)' }}
                      itemStyle={{ color: '#1e293b', fontWeight: 600 }}
                    />
                    <Legend verticalAlign="bottom" height={36} iconType="circle"/>
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>

            <div className="chart-card glass">
              <h3><Users size={20} /> Phân bổ nguồn lực</h3>
              <div className="chart-wrapper">
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={stats?.teamWorkload}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                    <XAxis dataKey="name" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                    <YAxis stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                    <RechartsTooltip 
                       cursor={{fill: 'rgba(99, 102, 241, 0.05)'}}
                       contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.1)' }}
                    />
                    <Bar dataKey="taskCount" fill="#6366f1" radius={[6, 6, 0, 0]} barSize={28} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          <div className="chart-card glass full-width">
            <h3><TrendingUp size={20} /> Tiến độ các dự án tiêu biểu</h3>
            <div className="chart-wrapper">
              <ResponsiveContainer width="100%" height={300}>
                <AreaChart data={stats?.projectProgress}>
                  <defs>
                    <linearGradient id="colorProgress" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#6366f1" stopOpacity={0.2}/>
                      <stop offset="95%" stopColor="#6366f1" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis dataKey="projectName" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                  <YAxis stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                  <RechartsTooltip />
                  <Area 
                    type="monotone" 
                    dataKey="progress" 
                    stroke="#6366f1" 
                    fillOpacity={1} 
                    fill="url(#colorProgress)" 
                    strokeWidth={4}
                  />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>

        {/* Right Column: Secondary Info */}
        <div className="side-column">
          <div className="info-card glass">
            <div className="card-header">
              <h3>Công việc khẩn cấp</h3>
              <Star className="star-glow" size={18} fill="#f59e0b" color="#f59e0b" />
            </div>
            <div className="priority-tasks-list-v2">
              {(!stats?.myPriorityTasks || stats.myPriorityTasks.length === 0) ? (
                <div className="empty-tasks">
                   <div className="empty-icon">✨</div>
                   <p>Mọi thứ đã được xử lý xong!</p>
                </div>
              ) : (
                stats?.myPriorityTasks.map((task: any) => (
                  <div key={task.id} className="priority-task-item-v2">
                    <div className="task-indicator" style={{ backgroundColor: task.doUuTien >= 2 ? '#ef4444' : '#f59e0b' }}></div>
                    <div className="task-content">
                      <span className="task-title">{task.tieuDe}</span>
                      <span className="task-meta">TASK-{task.id} • {task.thoiGianUocTinh || 0}h dự kiến</span>
                    </div>
                    <ChevronRight size={16} className="arrow" />
                  </div>
                ))
              )}
            </div>
          </div>

          <div className="info-card glass">
            <div className="card-header">
              <h3>Dự án mới cập nhật</h3>
              <Briefcase size={18} color="#6366f1" />
            </div>
            <div className="recent-projects-list-v2">
              {stats?.recentProjects.map((p: any) => (
                <div key={p.id} className="mini-project-card-v2">
                  <div className="project-info">
                    <h4>{p.tenDuAn}</h4>
                    <span className="date-meta">Cập nhật: {new Date().toLocaleDateString('vi-VN')}</span>
                  </div>
                  <div className="project-progress-mini">
                    <div className="progress-info">
                      <span>Tiến độ</span>
                      <span>{p.progress || 0}%</span>
                    </div>
                    <div className="progress-bar-bg">
                      <div 
                        className="progress-bar-fill" 
                        style={{ 
                          width: `${p.progress || 0}%`,
                          backgroundColor: (p.progress || 0) >= 100 ? '#10b981' : '#6366f1'
                        }}
                      ></div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
