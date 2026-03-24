import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Search,
  Plus,
  MoreHorizontal,
  Clock,
  Sparkles,
  AlertCircle,
  CheckCircle2,
  Calendar,
  XCircle,
  CheckCircle,
  Play,
  CheckSquare
} from 'lucide-react';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import TaskService from '../../services/TaskService';
import UserService from '../../services/UserService';
import type { CongViecDto } from '../../services/TaskService';
import { TrangThaiCongViec as StatusEnum } from '../../services/TaskService';
import ProjectService from '../../services/ProjectService';
import type { ThanhVienDuAnDto } from '../../services/ProjectTypes';
import SignalRService from '../../services/SignalRService';
import './SprintDetail.css';

/**
 * Trang chi tiết Sprint - Hiển thị Kanban Board.
 */
const SprintDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [sprint, setSprint] = useState<SprintDto | null>(null);
  const [tasks, setTasks] = useState<CongViecDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentUser, setCurrentUser] = useState<any>(null);

  // Manual Assignment State
  const [projectMembers, setProjectMembers] = useState<ThanhVienDuAnDto[]>([]);
  const [activeDropdownTaskId, setActiveDropdownTaskId] = useState<number | null>(null);

  const [showCreateTaskModal, setShowCreateTaskModal] = useState(false);
  const [isCreatingTask, setIsCreatingTask] = useState(false);
  const [newTaskData, setNewTaskData] = useState({
    tieuDe: '',
    moTa: '',
    loaiCongViec: 0,
    doUuTien: 1,
    storyPoints: 0,
    thoiGianUocTinh: 0,
    assigneeId: null as number | null
  });

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const profile = await UserService.getProfile();
        setCurrentUser(profile);
      } catch (e) {
        console.error('Failed to fetch profile', e);
      }
    };
    fetchProfile();
  }, []);

  const fetchData = async (background: boolean = false) => {
    if (!id) return;
    try {
      if (!background) setLoading(true);
      const sprintData = await SprintService.getById(Number(id));
      setSprint(sprintData);

      if (sprintData) {
        const allTasks = await TaskService.getByProjectId(sprintData.duAnId);
        const sprintTasks = allTasks.filter(t => t.sprintId === Number(id));
        setTasks(sprintTasks);

        const members = await ProjectService.getMembers(sprintData.duAnId);
        setProjectMembers(members);
      }
    } catch (error) {
      console.error('Error fetching sprint details:', error);
    } finally {
      if (!background) setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();

    // Thay thế Polling bằng SignalR Realtime
    const setupSignalR = async () => {
      await SignalRService.startConnection();
      if (sprint) {
        await SignalRService.joinProject(sprint.duAnId);
      }
      
      // Lắng nghe sự kiện cập nhật công việc
      SignalRService.on('TaskUpdated', (projectId: number) => {
        if (sprint && projectId === sprint.duAnId) {
          console.log('Realtime update received for project:', projectId);
          fetchData(true); // Refresh data ngầm
        }
      });
    };

    if (sprint) {
      setupSignalR();
    }

    return () => {
      if (sprint) {
        SignalRService.leaveProject(sprint.duAnId);
      }
      SignalRService.off('TaskUpdated');
    };
  }, [id, sprint?.duAnId]);

  const columns = [
    { id: StatusEnum.Todo, title: 'Cần làm', icon: <AlertCircle size={18} />, color: '#64748b' },
    { id: StatusEnum.InProgress, title: 'Đang làm', icon: <Clock size={18} />, color: '#6366f1' },
    { id: StatusEnum.Review, title: 'Đang kiểm tra', icon: <Search size={18} />, color: '#f59e0b' },
    { id: StatusEnum.Done, title: 'Hoàn thành', icon: <CheckCircle2 size={18} />, color: '#10b981' },
  ];

  const getPriorityLabel = (priority: number) => {
    switch (priority) {
      case 0: return { text: 'Thấp', class: 'p-low' };
      case 1: return { text: 'Vừa', class: 'p-medium' };
      case 2: return { text: 'Cao', class: 'p-high' };
      case 3: return { text: 'Khẩn cấp', class: 'p-urgent' };
      default: return { text: 'Vừa', class: 'p-medium' };
    }
  };

  const handleTaskAction = async (taskId: number, approve: boolean) => {
    if (!isSprintActive) {
      alert('Chỉ có thể phê duyệt công việc khi Sprint đang hoạt động.');
      return;
    }
    try {
      const targetStatus = approve ? 3 : 1;
      const success = await TaskService.updateStatus(taskId, targetStatus);
      if (success) {
        setTasks(prev => prev.map(t => t.id === taskId ? { ...t, trangThai: targetStatus as any } : t));
      }
    } catch (error) {
      console.error('Action failed:', error);
    }
  };

  const isAdmin = currentUser?.vaiTros?.some((r: string) => {
    const normalized = r.toLowerCase().replace(/\s+/g, '');
    return normalized === 'quanly' || normalized === 'admin' || normalized === 'quảnlý';
  });

  const isSprintActive = !!sprint && (
    sprint.trangThai === 1 ||
    (sprint.trangThai === 0 && new Date() >= new Date(sprint.ngayBatDau) && new Date() <= new Date(sprint.ngayKetThuc))
  );

  const handleUpdateSprintStatus = async (status: number) => {
    if (!sprint) return;
    try {
      const success = await SprintService.update(sprint.id, {
        tenSprint: sprint.tenSprint,
        ngayBatDau: sprint.ngayBatDau,
        ngayKetThuc: sprint.ngayKetThuc,
        mucTieuStoryPoints: sprint.mucTieuStoryPoints,
        trangThai: status as any
      });
      if (success) {
        fetchData(); // Reload
      }
    } catch (e) {
      console.error('Update status failed', e);
    }
  }

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sprint || !newTaskData.tieuDe) return;
    
    try {
      setIsCreatingTask(true);
      const payload = {
        ...newTaskData,
        duAnId: sprint.duAnId,
        sprintId: sprint.id
      };
      await TaskService.create(payload as any);
      
      const allTasks = await TaskService.getByProjectId(sprint.duAnId);
      const sprintTasks = allTasks.filter(t => t.sprintId === Number(id));
      setTasks(sprintTasks);
      
      setShowCreateTaskModal(false);
      setNewTaskData({
        tieuDe: '', moTa: '', loaiCongViec: 0, doUuTien: 1, storyPoints: 0, thoiGianUocTinh: 0, assigneeId: null
      });
    } catch (error) {
      console.error('Create task failed:', error);
      alert('Tạo công việc thất bại.');
    } finally {
      setIsCreatingTask(false);
    }
  };

  const handleManualAssign = async (taskId: number, userId: number | null) => {
    try {
      let assigneeName = '';
      if (userId !== null) {
        const member = projectMembers.find(m => m.id === userId);
        if (member) assigneeName = member.hoTen;
      }

      // API call to re-assign or un-assign
      const success = await TaskService.assignTask({
        congViecId: taskId,
        assigneeId: userId || 0 // Pass 0 if null for unassigning
      });

      if (success) {
        setTasks(prev => prev.map(t =>
          t.id === taskId ? {
            ...t,
            assigneeId: userId !== null ? userId : undefined,
            assigneeName: assigneeName
          } : t
        ));
        setActiveDropdownTaskId(null);
      }
    } catch (error) {
      console.error('Manual assign failed:', error);
      alert('Không thể cập nhật người thực hiện. Vui lòng thử lại.');
    }
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (!(e.target as Element).closest('.assignee-wrapper')) {
        setActiveDropdownTaskId(null);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  if (loading) return <div className="loading-state">Đang tải chi tiết Sprint...</div>;
  if (!sprint) return <div className="error-state">Không tìm thấy Sprint.</div>;

  return (
    <div className="sprint-detail-container">
      {/* Sprint Status Warning Banner */}
      {!isSprintActive && (
        <div className={`sprint-status-banner ${sprint.trangThai === 2 ? 'finished' : 'new'}`}>
          <AlertCircle size={18} />
          <span>
            {sprint.trangThai === 0 ? 'Sprint này chưa được bắt đầu (hoặc ngoài khung thời gian). Quản lý cần nhấn "Bắt đầu Sprint".' : 'Sprint này đã kết thúc. Bạn không thể thay đổi tiến độ công việc.'}
          </span>
          {isAdmin && sprint.trangThai === 0 && (
            <button className="btn-start-sprint" onClick={() => handleUpdateSprintStatus(1)}>
              <Play size={14} fill="currentColor" /> Kích hoạt ngay
            </button>
          )}
          {isAdmin && sprint.trangThai === 1 && (
            <button className="btn-finish-sprint" onClick={() => handleUpdateSprintStatus(2)}>
              <CheckSquare size={14} /> Hoàn thành Sprint
            </button>
          )}
        </div>
      )}

      {/* Header Section */}
      <div className="sprint-header">
        <div className="header-left">
          <button className="back-link" onClick={() => navigate(`/projects/${sprint.duAnId}`)}>
            <ArrowLeft size={20} />
            <span>Quay lại dự án</span>
          </button>
          <div className="sprint-title-box">
            <h1>{sprint.tenSprint}</h1>
            <div className="sprint-dates">
              <Calendar size={14} />
              <span>{new Date(sprint.ngayBatDau).toLocaleDateString('vi-VN')} - {new Date(sprint.ngayKetThuc).toLocaleDateString('vi-VN')}</span>
            </div>
          </div>
        </div>

        <div className="header-right">
          {isAdmin && sprint.trangThai === 1 && (
            <button className="btn-finish-sprint-inline" onClick={() => handleUpdateSprintStatus(2)}>
              <CheckSquare size={18} />
              <span>Hoàn thành Sprint</span>
            </button>
          )}
          <div className="search-tasks">
            <Search size={18} />
            <input type="text" placeholder="Tìm công việc..." />
          </div>
          <button className="create-task-btn" onClick={() => setShowCreateTaskModal(true)}>
            <Plus size={18} />
            <span>Tạo công việc</span>
          </button>
        </div>
      </div>

      {/* Kanban Board */}
      <div className={`kanban-board ${!isSprintActive ? 'read-only' : ''}`}>
        {columns.map(col => (
          <div key={col.id} className="kanban-column">
            <div className="column-header" style={{ borderTopColor: col.color }}>
              <div className="title-group">
                {col.icon}
                <h3>{col.title}</h3>
                <span className="count">{tasks.filter(t => t.trangThai === col.id).length}</span>
              </div>
              <button className="more-btn"><MoreHorizontal size={18} /></button>
            </div>

            <div className="task-list">
              {tasks.filter(t => t.trangThai === col.id).map(task => (
                <div key={task.id} className="task-card">
                  <div className="task-card-header">
                    <span className={`priority-tag ${getPriorityLabel(task.doUuTien).class}`}>
                      {getPriorityLabel(task.doUuTien).text}
                    </span>
                    <span className="task-id">#{task.id}</span>
                  </div>

                  <h4 className="task-title">{task.tieuDe}</h4>

                  {/* Quick Approval Actions for Managers */}
                  {col.id === StatusEnum.Review && isAdmin && isSprintActive && (
                    <div className="task-approval-actions">
                      <button
                        className="btn-reject-small"
                        onClick={() => handleTaskAction(task.id, false)}
                        title="Từ chối"
                      >
                        <XCircle size={14} />
                        Từ chối
                      </button>
                      <button
                        className="btn-approve-small"
                        onClick={() => handleTaskAction(task.id, true)}
                        title="Duyệt"
                      >
                        <CheckCircle size={14} />
                        Duyệt
                      </button>
                    </div>
                  )}

                  <div className="task-card-footer">
                    <div className="assignee-wrapper">
                      <div
                        className={`assignee ${isAdmin && isSprintActive ? 'editable' : ''}`}
                        onClick={() => {
                          if (isAdmin && isSprintActive) {
                            setActiveDropdownTaskId(activeDropdownTaskId === task.id ? null : task.id);
                          }
                        }}
                      >
                        {task.assigneeName ? (
                          <div className="avatar">
                            {task.assigneeName.charAt(0)}
                          </div>
                        ) : (
                          <div className="avatar empty" title="Chưa giao">
                            <Sparkles size={14} color="#94a3b8" />
                          </div>
                        )}
                        <span>{task.assigneeName || 'Chưa giao'}</span>
                      </div>

                      {/* Assignee Selection Dropdown */}
                      {activeDropdownTaskId === task.id && (
                        <div className="assignee-dropdown fade-in">
                          <div className="dropdown-search">
                            <Search size={14} color="#94a3b8" />
                            <input type="text" placeholder="Tìm thành viên..." onClick={e => e.stopPropagation()} />
                          </div>
                          <div className="dropdown-list">
                            <div className="dropdown-item clear-assign" onClick={(e) => { e.stopPropagation(); handleManualAssign(task.id, null); }}>
                              <div className="avatar empty"><XCircle size={14} /></div>
                              <span>Gỡ phân công</span>
                            </div>
                            <div className="dropdown-divider"></div>
                            {projectMembers.map(member => (
                              <div
                                key={member.id}
                                className={`dropdown-item ${task.assigneeId === member.id ? 'selected' : ''}`}
                                onClick={(e) => { e.stopPropagation(); handleManualAssign(task.id, member.id); }}
                              >
                                <div className="avatar">{member.hoTen.charAt(0)}</div>
                                <div className="member-info">
                                  <span className="name">{member.hoTen}</span>
                                  <span className="email">{member.email}</span>
                                </div>
                                {task.assigneeId === member.id && <CheckCircle2 size={16} color="#10b981" className="ml-auto" />}
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>

                    <div className="task-metrics">
                      <div className="task-hours" title="Thời gian ước tính">
                        <Clock size={12} />
                        <span>{task.thoiGianUocTinh}h</span>
                      </div>
                      <div className="story-points">
                        <span>{task.storyPoints} SP</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>

      {showCreateTaskModal && (
        <div className="sprint-modal-overlay" onClick={() => setShowCreateTaskModal(false)}>
          <div className="sprint-modal-content" onClick={e => e.stopPropagation()}>
            <div className="sprint-modal-header">
              <h2>Tạo Công việc Mới</h2>
              <button className="close-btn" onClick={() => setShowCreateTaskModal(false)}>✕</button>
            </div>
            <form onSubmit={handleCreateTask} className="sprint-modal-body">
              <div className="form-group">
                <label>Tiêu đề <span className="required">*</span></label>
                <input 
                  type="text" 
                  value={newTaskData.tieuDe}
                  onChange={e => setNewTaskData({...newTaskData, tieuDe: e.target.value})}
                  placeholder="Nhập tiêu đề công việc"
                  required
                />
              </div>
              <div className="form-group">
                <label>Mô tả</label>
                <textarea 
                  value={newTaskData.moTa}
                  onChange={e => setNewTaskData({...newTaskData, moTa: e.target.value})}
                  placeholder="Mô tả chi tiết công việc..."
                  rows={3}
                  style={{ width: '100%', padding: '10px 14px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', resize: 'vertical' }}
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Loại công việc</label>
                  <select 
                    value={newTaskData.loaiCongViec}
                    onChange={e => setNewTaskData({...newTaskData, loaiCongViec: parseInt(e.target.value)})}
                    style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%' }}
                  >
                    <option value={0}>Backend</option>
                    <option value={1}>Frontend</option>
                    <option value={2}>Fullstack</option>
                    <option value={3}>Mobile</option>
                    <option value={4}>DevOps</option>
                    <option value={5}>Tester</option>
                    <option value={6}>UI/UX</option>
                    <option value={7}>BA</option>
                  </select>
                </div>
                <div className="form-group">
                  <label>Độ ưu tiên</label>
                  <select 
                    value={newTaskData.doUuTien}
                    onChange={e => setNewTaskData({...newTaskData, doUuTien: parseInt(e.target.value)})}
                    style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%' }}
                  >
                    <option value={0}>Thấp</option>
                    <option value={1}>Vừa</option>
                    <option value={2}>Cao</option>
                    <option value={3}>Khẩn cấp</option>
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>Người thực hiện</label>
                <select 
                  value={newTaskData.assigneeId || ''}
                  onChange={e => setNewTaskData({...newTaskData, assigneeId: e.target.value ? parseInt(e.target.value) : null})}
                  style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%' }}
                >
                  <option value="">-- Để trống (Chưa giao) --</option>
                  {projectMembers.map(member => (
                    <option key={member.id} value={member.id}>
                      {member.hoTen} ({member.email})
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Story Points</label>
                  <input 
                    type="number" 
                    min={0}
                    value={newTaskData.storyPoints}
                    onChange={e => setNewTaskData({...newTaskData, storyPoints: parseInt(e.target.value) || 0})}
                  />
                </div>
                <div className="form-group">
                  <label>Thời gian ước tính (Giờ)</label>
                  <input 
                    type="number" 
                    min={0}
                    value={newTaskData.thoiGianUocTinh}
                    onChange={e => setNewTaskData({...newTaskData, thoiGianUocTinh: parseInt(e.target.value) || 0})}
                  />
                </div>
              </div>
              <div className="sprint-modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowCreateTaskModal(false)}>
                  Hủy
                </button>
                <button type="submit" className="btn-submit" disabled={isCreatingTask || !newTaskData.tieuDe}>
                  {isCreatingTask ? '...' : 'Lưu Công việc'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default SprintDetailPage;
