import React from 'react';
import { 
  CheckCircle2, 
  Clock, 
  AlertCircle, 
  TrendingUp,
  Calendar,
  MoreVertical,
  Plus,
  Briefcase
} from 'lucide-react';
import './Dashboard.css';

/**
 * Trang Dashboard chính hiển thị tổng quan dự án.
 */
const DashboardPage: React.FC = () => {
  const stats = [
    { label: 'Dự án đang chạy', value: '12', icon: <Briefcase size={24} />, color: '#6366f1' },
    { label: 'Công việc hoàn thành', value: '128', icon: <CheckCircle2 size={24} />, color: '#10b981' },
    { label: 'Việc cần làm', value: '24', icon: <Clock size={24} />, color: '#f59e0b' },
    { label: 'Vấn đề tồn đọng', value: '3', icon: <AlertCircle size={24} />, color: '#ef4444' },
  ];

  const recentProjects = [
    { name: 'Hệ thống Quản lý Đồ án', deadline: '2024-05-15', progress: 75, status: 'On Track' },
    { name: 'Ứng dụng AI Chatbot', deadline: '2024-06-20', progress: 40, status: 'In Review' },
    { name: 'Website E-commerce', deadline: '2024-04-30', progress: 100, status: 'Completed' },
  ];

  return (
    <div className="dashboard-content">
      {/* Welcome Section */}
      <div className="welcome-section">
        <div className="welcome-text">
          <h2>Chào buổi tối, Admin! 👋</h2>
          <p>Dưới đây là tóm tắt các hoạt động quan trọng trong ngày hôm nay.</p>
        </div>
        <button className="add-project-btn">
          <Plus size={18} />
          <span>Tạo dự án mới</span>
        </button>
      </div>

      {/* Stats Grid */}
      <div className="stats-grid">
        {stats.map((stat, idx) => (
          <div key={idx} className="stat-card">
            <div className="stat-icon" style={{ backgroundColor: `${stat.color}15`, color: stat.color }}>
              {stat.icon}
            </div>
            <div className="stat-info">
              <span className="stat-label">{stat.label}</span>
              <span className="stat-value">{stat.value}</span>
            </div>
            <div className="stat-trend positive">
              <TrendingUp size={14} />
              <span>12%</span>
            </div>
          </div>
        ))}
      </div>

      <div className="dashboard-grid">
        {/* Projects Table */}
        <div className="dashboard-card projects-card">
          <div className="card-header">
            <h3>Dự án gần đây</h3>
            <button className="text-btn">Xem tất cả</button>
          </div>
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Tên dự án</th>
                  <th>Hạn chót</th>
                  <th>Tiến độ</th>
                  <th>Trạng thái</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {recentProjects.map((project, idx) => (
                  <tr key={idx}>
                    <td>
                      <div className="project-name-cell">
                        <div className="project-avatar">{project.name.charAt(0)}</div>
                        <span>{project.name}</span>
                      </div>
                    </td>
                    <td>
                      <div className="deadline-cell">
                        <Calendar size={14} />
                        <span>{project.deadline}</span>
                      </div>
                    </td>
                    <td>
                      <div className="progress-container">
                        <div className="progress-bar-bg">
                          <div className="progress-bar-fill" style={{ width: `${project.progress}%` }} />
                        </div>
                        <span className="progress-text">{project.progress}%</span>
                      </div>
                    </td>
                    <td>
                      <span className={`status-badge ${project.status.toLowerCase().replace(' ', '-')}`}>
                        {project.status}
                      </span>
                    </td>
                    <td>
                      <button className="icon-btn-sm">
                        <MoreVertical size={16} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Calendar/Upcoming Section */}
        <div className="dashboard-card upcoming-card">
          <div className="card-header">
            <h3>Sắp tới</h3>
            <button className="icon-btn-sm"><Plus size={16} /></button>
          </div>
          <div className="upcoming-list">
            <div className="upcoming-item">
              <div className="event-date">
                <span className="day">18</span>
                <span className="month">Th4</span>
              </div>
              <div className="event-info">
                <h4>Họp Team Sprint 1</h4>
                <p>9:00 AM - 10:30 AM</p>
              </div>
            </div>
            <div className="upcoming-item">
              <div className="event-date blue">
                <span className="day">20</span>
                <span className="month">Th4</span>
              </div>
              <div className="event-info">
                <h4>Nộp báo cáo giữa kỳ</h4>
                <p>Hạn cuối: 11:59 PM</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
