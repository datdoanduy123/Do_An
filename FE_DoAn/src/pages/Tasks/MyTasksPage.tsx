import React, { useEffect, useState } from 'react';
import {
  CheckCircle2,
  Clock,
  AlertCircle,
  Calendar,
  Search,
  MoreVertical,
  Flag,
  X,
  Lock,
  Link
} from 'lucide-react';
import TaskService from '../../services/TaskService';
import UserService from '../../services/UserService';
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
  const [currentUser, setCurrentUser] = useState<any>(null);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const profile = await UserService.getProfile();
        setCurrentUser(profile);
      } catch (e) {
        console.error('Failed to fetch profile in MyTasks', e);
      }
    };
    fetchProfile();
  }, []);

  // State cho Modal cập nhật tiến độ
  const [showProgressModal, setShowProgressModal] = useState(false);
  const [selectedTask, setSelectedTask] = useState<CongViecDto | null>(null);
  const [progressData, setProgressData] = useState({
    thoiGianThem: 0,
    ghiChu: '',
    trangThai: 0
  });


  // Toast State
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 5200); // Tăng thời gian hiển thị cho dễ đọc
  };

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

  const isTaskWorkable = (task: CongViecDto) => {
    if (!task.sprintId) return true; // Task không thuộc Sprint nào luôn workable
    if (task.sprintStatus === 1) return true; // Sprint đang chạy
    if (task.sprintStatus === 0) {
      const now = new Date();
      const sprintStart = task.ngayBatDauSprint ? new Date(task.ngayBatDauSprint) : null;
      const sprintEnd = task.ngayKetThucSprint ? new Date(task.ngayKetThucSprint) : null;
      return !!(sprintStart && sprintEnd && now >= sprintStart && now <= sprintEnd);
    }
    return false;
  };

  const handleOpenProgressModal = (task: CongViecDto, targetStatus?: number) => {
    // Kiểm tra nếu Sprint chưa bắt đầu hoặc đã kết thúc
    if (!isTaskWorkable(task)) {
      showToast('Công việc này thuộc Sprint chưa bắt đầu hoặc đã kết thúc.', 'error');
      return;
    }

    setSelectedTask(task);
    setProgressData({
      thoiGianThem: 0,
      ghiChu: '',
      trangThai: targetStatus !== undefined ? targetStatus : task.trangThai
    });
    setShowProgressModal(true);
  };


  const handleUpdateProgressSubmit = async () => {
    if (!selectedTask) return;

    try {
      const success = await TaskService.updateProgress(selectedTask.id, {
        trangThai: progressData.trangThai,
        thoiGianLamViecThem: progressData.thoiGianThem,
        ghiChu: progressData.ghiChu
      });

      if (success) {
        setTasks(prev => prev.map(t =>
          t.id === selectedTask.id
            ? {
              ...t,
              trangThai: progressData.trangThai as any,
              thoiGianThucTe: (t.thoiGianThucTe || 0) + progressData.thoiGianThem
            }
            : t
        ));
        showToast('Cập nhật tiến độ thành công!');
        setShowProgressModal(false);
      }
    } catch (error: any) {
      console.error('Failed to update progress:', error);
      showToast(error.message || 'Cập nhật thất bại.', 'error');
    }
  };

  const calculateProgress = (spent: number = 0, estimate: number = 0) => {
    if (estimate <= 0) return 0;
    const percent = (spent / estimate) * 100;
    return percent > 100 ? 100 : Math.round(percent);
  };

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 0: return { color: '#10b981', label: 'Thấp', bg: '#d1fae5' };
      case 1: return { color: '#3b82f6', label: 'Vừa', bg: '#dbeafe' };
      case 2: return { color: '#f59e0b', label: 'Cao', bg: '#fef3c7' };
      default: return { color: '#3b82f6', label: 'Vừa', bg: '#dbeafe' };
    }
  };

  const getStatusInfo = (status: number) => {
    switch (status) {
      case 0: return { label: 'To Do', class: 'todo' };
      case 1: return { label: 'In Progress', class: 'inprogress' };
      case 2: return { label: 'Review', class: 'review' };
      case 3: return { label: 'Done', class: 'done' };
      case 4: return { label: 'Cancelled', class: 'cancelled' };
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
            (() => {
              // Nhóm các task theo Sprint
              const groups: { [key: string]: { name: string; status?: number; tasks: CongViecDto[]; startDate?: string; endDate?: string } } = {};

              filteredTasks.forEach(task => {
                const key = task.sprintId ? `sprint-${task.sprintId}` : 'backlog';
                if (!groups[key]) {
                  groups[key] = {
                    name: task.sprintId ? (task.tenSprint || `Sprint ${task.sprintId}`) : 'Công việc ngoài Sprint',
                    status: task.sprintStatus,
                    startDate: task.ngayBatDauSprint,
                    endDate: task.ngayKetThucSprint,
                    tasks: []
                  };
                }
                groups[key].tasks.push(task);
              });

              // Sắp xếp thứ tự các nhóm: Active Sprint (1) -> Not Started (0) -> Ended (2) -> Backlog
              const sortedGroupKeys = Object.keys(groups).sort((a, b) => {
                if (a === 'backlog') return 1;
                if (b === 'backlog') return -1;

                const statusA = groups[a].status ?? -1;
                const statusB = groups[b].status ?? -1;

                // Priority: InProgress (1) > NotStarted (0) > Ended (2)
                const priority = { 1: 0, 0: 1, 2: 2 };
                const pA = priority[statusA as keyof typeof priority] ?? 3;
                const pB = priority[statusB as keyof typeof priority] ?? 3;

                return pA - pB;
              });

              return sortedGroupKeys.map(groupKey => {
                const group = groups[groupKey];
                const sprintStatusInfo = groupKey === 'backlog' ? null : (
                  group.status === 1 ? { label: 'Đang diễn ra', class: 'active' } :
                    group.status === 0 ? { label: 'Chưa bắt đầu', class: 'upcoming' } :
                      { label: 'Đã kết thúc', class: 'ended' }
                );

                return (
                  <div key={groupKey} className="sprint-group">
                    <div className={`sprint-group-header ${groupKey}`}>
                      <div className="sprint-info">
                        <div className="module-icon-box">
                          <Flag size={20} />
                        </div>
                        <div className="module-title-box">
                          <span className="module-label">MODULE / SPRINT</span>
                          <span className="sprint-name">{group.name}</span>
                        </div>
                        {group.startDate && group.endDate && (
                          <div className="sprint-dates-header v2">
                             {new Date(group.startDate).toLocaleDateString('vi-VN')} - {new Date(group.endDate).toLocaleDateString('vi-VN')}
                          </div>
                        )}
                        {sprintStatusInfo && (
                          <span className={`sprint-status-tag v2 ${sprintStatusInfo.class}`}>
                            {sprintStatusInfo.label}
                          </span>
                        )}
                      </div>
                      <div className="module-stats">
                        <span className="task-count">{group.tasks.length} công việc</span>
                      </div>
                    </div>

                    <div className="sprint-tasks-container">
                      {[1, 0, 2, 3, 4].map(statusId => {
                        const statusTasks = group.tasks.filter(t => t.trangThai === statusId);
                        if (statusTasks.length === 0) return null;
                        const statusInfo = getStatusInfo(statusId);

                        return (
                          <div key={statusId} className="status-sub-group">
                            <div className={`status-sub-header ${statusInfo.class}`}>
                              <div className="status-label-box">
                                <div className={`status-dot ${statusInfo.class}`}></div>
                                <span>{statusInfo.label}</span>
                              </div>
                              <span className="status-count">{statusTasks.length} công việc</span>
                            </div>
                            <div className="status-tasks-list">
                              {statusTasks.map(task => {
                                const priority = getPriorityColor(task.doUuTien);
                                const status = getStatusInfo(task.trangThai);
                                const isWorkable = isTaskWorkable(task);
                                const isLocked = task.sprintId && !isWorkable;

                                return (
                                  <div key={task.id} className={`task-row-item ${isLocked ? 'locked' : ''}`}>
                                    <div className="task-main-info">
                                      <div className="task-status-icon">
                                        {isLocked ? <Lock size={20} color="#94a3b8" /> : (task.trangThai === 3 ? <CheckCircle2 size={20} color="#10b981" /> : <Clock size={20} color="#64748b" />)}
                                      </div>
                                      <div className="task-text">
                                        <div className="title-row">
                                          <h4>{task.tieuDe}</h4>
                                          {task.sprintId && task.sprintStatus === 0 && !isWorkable && <span className="locked-badge"><Lock size={12} /> Sprint chưa bắt đầu</span>}
                                          {task.sprintId && task.sprintStatus === 2 && <span className="locked-badge"><Lock size={12} /> Sprint đã kết thúc</span>}
                                        </div>
                                        <div className="task-progress-box">
                                          <div className="progress-bar-wrapper">
                                            <div
                                              className="progress-fill"
                                              style={{
                                                width: `${calculateProgress(task.thoiGianThucTe, task.thoiGianUocTinh)}%`,
                                                backgroundColor: calculateProgress(task.thoiGianThucTe, task.thoiGianUocTinh) >= 100 ? '#10b981' : '#6366f1'
                                              }}
                                            ></div>
                                          </div>
                                          <span className="progress-text">{calculateProgress(task.thoiGianThucTe, task.thoiGianUocTinh)}%</span>
                                        </div>
                                        {task.dependencies && task.dependencies.length > 0 && (
                                          <div className="task-row-dependencies">
                                            <Link size={12} />
                                            <div className="dep-list">
                                              {task.dependencies.map(dep => (
                                                <span key={dep.dependsOnTaskId} title={dep.dependsOnTaskTitle} className="dep-tag">
                                                  #{dep.dependsOnTaskId}
                                                </span>
                                              ))}
                                            </div>
                                          </div>
                                        )}
                                        <div className="task-meta">
                                          <span className="time-tracking">
                                            <strong>{task.thoiGianThucTe || 0}h</strong> / {task.thoiGianUocTinh}h
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
                                      <div className="status-quick-actions">
                                        {!isLocked && task.trangThai !== 1 && task.trangThai !== 3 && (
                                          <button
                                            className="action-btn start"
                                            onClick={() => handleOpenProgressModal(task, 1)}
                                            title="Bắt đầu làm"
                                          >
                                            Làm việc
                                          </button>
                                        )}
                                        {!isLocked && task.trangThai === 1 && (
                                          <button
                                            className="action-btn log"
                                            onClick={() => handleOpenProgressModal(task)}
                                            title="Cập nhật tiến độ"
                                          >
                                            Cập nhật
                                          </button>
                                        )}
                                        {task.trangThai === 2 && (
                                          <span className="awaiting-review">Chờ phê duyệt</span>
                                        )}
                                        {!isLocked && task.trangThai === 2 && (
                                          currentUser?.vaiTros?.some((r: string) => {
                                            const nr = r.toLowerCase().replace(/\s+/g, '');
                                            return nr === 'quanly' || nr === 'admin' || nr === 'quảnlý';
                                          }) || currentUser?.id === task.createdBy
                                        ) && (
                                          <button
                                            className="action-btn done"
                                            onClick={() => handleOpenProgressModal(task, 3)}
                                            title="Hoàn thành"
                                          >
                                            Xong
                                          </button>
                                        )}

                                        {task.sprintId && task.sprintStatus === 0 && !isWorkable && (
                                          <span className="locked-info" title="Sprint này chưa được quản lý bắt đầu.">Đang đợi bắt đầu</span>
                                        )}
                                        {task.sprintId && task.sprintStatus === 2 && (
                                          <span className="locked-info" title="Sprint đã kết thúc, bạn không thể cập nhật thêm.">Đã đóng</span>
                                        )}
                                      </div>
                                      <button className="icon-btn-ghost"><MoreVertical size={18} /></button>
                                    </div>
                                  </div>
                                );
                              })}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                );
              });
            })()
          ) : (
            <div className="empty-tasks">
              <AlertCircle size={48} color="#94a3b8" />
              <p>Không tìm thấy công việc nào thỏa mãn điều kiện.</p>
            </div>
          )}
        </div>
      </div>

      {/* Progress Update Modal */}
      {showProgressModal && selectedTask && (
        <div className="modal-overlay">
          <div className="modal-content progress-modal-v2">
            <div className="modal-header">
              <div className="header-title">
                <span className="task-id">TASK-{selectedTask.id}</span>
                <h3>Cập nhật tiến độ</h3>
              </div>
              <button className="close-btn" onClick={() => setShowProgressModal(false)}>
                <X size={20} />
              </button>
            </div>

            <div className="modal-body split-view">
              <div className="task-summary-side">
                <div className="summary-card">
                  <h4>{selectedTask.tieuDe}</h4>
                  <div className="time-stats">
                    <div className="stat-circle">
                      <svg width="100" height="100" viewBox="0 0 100 100">
                        <circle cx="50" cy="50" r="45" fill="none" stroke="#f1f5f9" strokeWidth="8" />
                        <circle
                          cx="50" cy="50" r="45" fill="none" stroke="#6366f1" strokeWidth="8"
                          strokeDasharray={`${calculateProgress(selectedTask.thoiGianThucTe, selectedTask.thoiGianUocTinh) * 2.827} 282.7`}
                          transform="rotate(-90 50 50)"
                          strokeLinecap="round"
                        />
                      </svg>
                      <div className="stat-value">
                        {calculateProgress(selectedTask.thoiGianThucTe, selectedTask.thoiGianUocTinh)}%
                      </div>
                    </div>
                    <div className="time-details">
                      <div className="detail-row">
                        <span>Đã làm:</span>
                        <strong>{selectedTask.thoiGianThucTe || 0}h</strong>
                      </div>
                      <div className="detail-row">
                        <span>Dự kiến:</span>
                        <strong>{selectedTask.thoiGianUocTinh}h</strong>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="current-status-info">
                  <label>Trạng thái hiện tại</label>
                  <span className={`status-pill ${getStatusInfo(selectedTask.trangThai).class}`}>
                    {getStatusInfo(selectedTask.trangThai).label}
                  </span>
                </div>

                {/* Phần hiển thị chi tiết mô tả công việc */}
                {selectedTask.moTa && (
                  <div className="task-description-side">
                    <label>Mô tả chi tiết</label>
                    <div className="description-box">
                      {selectedTask.moTa}
                    </div>
                  </div>
                )}

              </div>

              <div className="task-form-side">
                <div className="form-group status-update-group">
                  <label>Cập nhật trạng thái mới</label>
                  <div className="status-selector horizontal">
                    {[0, 1, 2, 3].map(s => {
                      // Phân quyền: Chỉ hiển thị nút Done (3) nếu là Admin/Quản lý hoặc là người tạo task
                      if (s === 3) {
                        const isAdmin = currentUser?.vaiTros?.some((r: string) => {
                          const nr = r.toLowerCase().replace(/\s+/g, '');
                          return nr === 'quanly' || nr === 'admin' || nr === 'quảnlý';
                        });
                        const isCreator = currentUser?.id === selectedTask?.createdBy;
                        if (!isAdmin && !isCreator) return null;
                      }

                      return (
                        <button
                          key={s}
                          className={`status-opt-v2 ${progressData.trangThai === s ? 'active' : ''}`}
                          onClick={() => setProgressData({ ...progressData, trangThai: s })}
                        >
                          <div className={`status-icon-dot ${getStatusInfo(s).class}`}></div>
                          {getStatusInfo(s).label}
                        </button>
                      );
                    })}
                  </div>
                </div>

                {/* Thảo luận & Phản hồi đã được gỡ bỏ */}
              </div>
            </div>

            <div className="modal-footer">
              <button className="btn-cancel-v2" onClick={() => setShowProgressModal(false)}>Hủy bỏ</button>
              <button className="btn-save-v2" onClick={handleUpdateProgressSubmit}>
                Xác nhận cập nhật
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Toast Notification */}
      {toast && (
        <div className={`toast-notification-v2 ${toast.type}`}>
          <div className="toast-icon">
            {toast.type === 'success' ? <CheckCircle2 size={24} /> : <AlertCircle size={24} />}
          </div>
          <div className="toast-content">
            <span className="toast-title">{toast.type === 'success' ? 'Thành công' : 'Cảnh báo'}</span>
            <span className="toast-msg">{toast.message}</span>
          </div>
          <button className="toast-close" onClick={() => setToast(null)}><X size={16} /></button>
        </div>
      )}
    </div>
  );
};

export default MyTasksPage;
