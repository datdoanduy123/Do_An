import React, { useEffect, useState } from 'react';
import { 
  CheckCircle2, 
  Clock, 
  AlertCircle, 
  Calendar,
  Search,
  ChevronRight,
  MoreVertical,
  Flag
} from 'lucide-react';
import TaskService from '../../services/TaskService';
import type { CongViecDto } from '../../services/TaskService';
import './MyTasks.css';

/**
 * Trang danh sách công việc của tôi (Dành cho nhân viên).
 */
const MyTasksPage: React.FC = () => {
  const [tasks, setTasks] = useState<CongViecDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const fetchMyTasks = async () => {
      try {
        setLoading(true);
        const data = await TaskService.getMyTasks();
        setTasks(data || []);
      } catch (error) {
        console.error('Error fetching my tasks:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchMyTasks();
  }, []);

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 1: return { color: '#ef4444', label: 'Cao', bg: '#fee2e2' };
      case 2: return { color: '#f59e0b', label: 'Trung bình', bg: '#fef3c7' };
      default: return { color: '#10b981', label: 'Thấp', bg: '#d1fae5' };
    }
  };

  const getStatusInfo = (status: number) => {
    switch (status) {
      case 0: return { label: 'To Do', class: 'todo' };
      case 1: return { label: 'In Progress', class: 'inprogress' };
      case 2: return { label: 'Review', class: 'review' };
      case 3: return { label: 'Done', class: 'done' };
      default: return { label: 'Khác', class: 'other' };
    }
  };

  const filteredTasks = tasks.filter(task => {
    const matchesFilter = filter === 'all' || 
      (filter === 'todo' && task.trangThai === 0) ||
      (filter === 'inprogress' && task.trangThai === 1) ||
      (filter === 'review' && task.trangThai === 2) ||
      (filter === 'done' && task.trangThai === 3);
    
    const matchesSearch = task.tieuDe.toLowerCase().includes(searchTerm.toLowerCase());
    
    return matchesFilter && matchesSearch;
  });

  if (loading) return <div className="loading-state">Đang tải danh sách công việc...</div>;

  return (
    <div className="my-tasks-container">
      <header className="page-header">
        <div className="header-info">
          <h1>Công việc của tôi</h1>
          <p>Quản lý và cập nhật tiến độ các công việc đã được giao.</p>
        </div>
        <div className="header-actions">
          <div className="search-box">
            <Search size={18} />
            <input 
              type="text" 
              placeholder="Tìm kiếm công việc..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
      </header>

      {/* Stats Summary */}
      <div className="task-stats-row">
        <div className="stat-item">
          <span className="stat-val">{tasks.length}</span>
          <span className="stat-name">Tổng cộng</span>
        </div>
        <div className="stat-item orange">
          <span className="stat-val">{tasks.filter(t => t.trangThai === 1).length}</span>
          <span className="stat-name">Đang làm</span>
        </div>
        <div className="stat-item green">
          <span className="stat-val">{tasks.filter(t => t.trangThai === 3).length}</span>
          <span className="stat-name">Hoàn thành</span>
        </div>
      </div>

      <div className="tasks-main-card">
        <div className="card-toolbar">
          <div className="filter-tabs">
            <button className={filter === 'all' ? 'active' : ''} onClick={() => setFilter('all')}>Tất cả</button>
            <button className={filter === 'todo' ? 'active' : ''} onClick={() => setFilter('todo')}>Cần làm</button>
            <button className={filter === 'inprogress' ? 'active' : ''} onClick={() => setFilter('inprogress')}>Đang làm</button>
            <button className={filter === 'review' ? 'active' : ''} onClick={() => setFilter('review')}>Chờ duyệt</button>
            <button className={filter === 'done' ? 'active' : ''} onClick={() => setFilter('done')}>Hoàn thành</button>
          </div>
        </div>

        <div className="task-list">
          {filteredTasks.length > 0 ? (
            filteredTasks.map(task => {
              const priority = getPriorityColor(task.doUuTien);
              const status = getStatusInfo(task.trangThai);
              return (
                <div key={task.id} className="task-row-item">
                  <div className="task-main-info">
                    <div className="task-status-icon">
                      {task.trangThai === 3 ? <CheckCircle2 size={20} color="#10b981" /> : <Clock size={20} color="#64748b" />}
                    </div>
                    <div className="task-text">
                      <h4>{task.tieuDe}</h4>
                      <div className="task-meta">
                        <span className="project-tag">ID Dự án: {task.duAnId}</span>
                        <span className="dot">•</span>
                        <span className="deadline-text">
                          <Calendar size={12} />
                          {task.ngayKetThucDuKien ? new Date(task.ngayKetThucDuKien).toLocaleDateString('vi-VN') : 'Không có hạn'}
                        </span>
                      </div>
                    </div>
                  </div>
                  
                  <div className="task-tags">
                    <span className="priority-tag" style={{ backgroundColor: priority.bg, color: priority.color }}>
                      <Flag size={12} fill={priority.color} />
                      {priority.label}
                    </span>
                    <span className={`status-pill ${status.class}`}>{status.label}</span>
                  </div>

                  <div className="task-actions">
                    <button className="icon-btn-ghost"><MoreVertical size={18} /></button>
                    <button className="btn-detail">
                      Chi tiết
                      <ChevronRight size={16} />
                    </button>
                  </div>
                </div>
              );
            })
          ) : (
            <div className="empty-tasks">
              <AlertCircle size={48} color="#94a3b8" />
              <p>Không tìm thấy công việc nào thỏa mãn điều kiện.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MyTasksPage;
