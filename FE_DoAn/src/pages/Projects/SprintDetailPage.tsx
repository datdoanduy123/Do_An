import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Search,
  Plus,
  MoreHorizontal,
  MoreVertical,
  Clock,
  Sparkles,
  AlertCircle,
  CheckCircle2,
  Calendar,
  XCircle,
  CheckCircle,
  Play,
  Edit,
  Trash2,
  CheckSquare,
  AlertTriangle,
  Link,
  Activity,
  MessageSquare,
  Send
} from 'lucide-react';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import TaskService from '../../services/TaskService';
import UserService from '../../services/UserService';
import type { CongViecDto } from '../../services/TaskService';
import { TrangThaiCongViec as StatusEnum } from '../../services/TaskService';
import ProjectService, { ProjectRole } from '../../services/ProjectService';
import RejectionModal from '../../components/Tasks/RejectionModal';
import type { ThanhVienDuAnDto } from '../../services/ProjectTypes';
import SignalRService from '../../services/SignalRService';
import ConfirmModal from '../../components/Common/ConfirmModal';
import Toast from '../../components/Common/Toast';
import './SprintDetail.css';

/**
 * Trang chi tiết Sprint - Hiển thị Kanban Board.
 */
const SprintDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [sprint, setSprint] = useState<SprintDto | null>(null);
  const [tasks, setTasks] = useState<CongViecDto[]>([]);
  const [allSprints, setAllSprints] = useState<SprintDto[]>([]);
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
    thoiGianUocTinh: 0,
    assigneeId: null as number | null
  });

  const [activeMenuTaskId, setActiveMenuTaskId] = useState<number | null>(null);
  const [showEditTaskModal, setShowEditTaskModal] = useState(false);
  const [editingTask, setEditingTask] = useState<CongViecDto | null>(null);

  // Rejection Modal State
  const [isRejectionModalOpen, setIsRejectionModalOpen] = useState(false);
  const [taskToReject, setTaskToReject] = useState<CongViecDto | null>(null);

  // New Comment State
  const [newComment, setNewComment] = useState('');
  const [isSendingComment, setIsSendingComment] = useState(false);

  // New states for Tester View
  const [viewMode, setViewMode] = useState<'kanban' | 'tester'>('kanban');
  const [selectedTestTaskId, setSelectedTestTaskId] = useState<number | null>(null);

  // Toast State
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  // Confirm Modal State
  const [confirmConfig, setConfirmConfig] = useState<{ 
    isOpen: boolean; 
    message: string; 
    onConfirm: () => void;
    title?: string;
    type?: 'danger' | 'warning'
  }>({
    isOpen: false,
    message: '',
    onConfirm: () => {},
  });

  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
  };

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

        // Auto-switch view for Tester/QA
        const isUserTester = members.some(m => m.id === currentUser?.id && (m.vaiTro === ProjectRole.Tester || m.vaiTro === ProjectRole.QA));
        if (isUserTester && viewMode === 'kanban') {
          setViewMode('tester');
        }
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

  useEffect(() => {
    if (viewMode === 'tester' && tasks.length > 0 && !selectedTestTaskId) {
      const firstReviewTask = tasks.find(t => t.trangThai === StatusEnum.Review);
      if (firstReviewTask) {
        setSelectedTestTaskId(firstReviewTask.id);
      }
    }
  }, [viewMode, tasks, selectedTestTaskId]);

  useEffect(() => {
    if (sprint?.duAnId) {
      const fetchSprints = async () => {
        try {
          const sprints = await SprintService.getByProjectId(sprint.duAnId);
          setAllSprints(sprints);
        } catch (e) {
          console.error("Failed to fetch all sprints", e);
        }
      };
      fetchSprints();
    }
  }, [sprint?.duAnId]);

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
      default: return { text: 'Vừa', class: 'p-medium' };
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    setConfirmConfig({
      isOpen: true,
      title: 'Xóa Công việc',
      message: 'Bạn có chắc chắn muốn xóa công việc này không? Thao tác này không thể hoàn tác.',
      type: 'danger',
      onConfirm: async () => {
        try {
          const result = await TaskService.delete(taskId);
          if (result) {
            setTasks(tasks.filter(t => t.id !== taskId));
            setActiveMenuTaskId(null);
            showToast('Đã xóa công việc thành công!');
          }
        } catch (error) {
          console.error('Delete task failed:', error);
          showToast('Xóa công việc thất bại.', 'error');
        }
      }
    });
  };

  const handleEditClick = (task: CongViecDto) => {
    setEditingTask(task);
    setNewTaskData({
      tieuDe: task.tieuDe,
      moTa: task.moTa || '',
      loaiCongViec: task.loaiCongViec,
      doUuTien: task.doUuTien,
      thoiGianUocTinh: task.thoiGianUocTinh,
      assigneeId: task.assigneeId || null
    });
    setShowEditTaskModal(true);
    setActiveMenuTaskId(null);
  };

  const handleUpdateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask) return;
    try {
      const result = await TaskService.update(editingTask.id, newTaskData);
      if (result.statusCode === 200) {
        setShowEditTaskModal(false);
        setEditingTask(null);
        setNewTaskData({
          tieuDe: '', moTa: '', loaiCongViec: 0, doUuTien: 1, thoiGianUocTinh: 0, assigneeId: null
        });
        // Reload tasks will happen via SignalR or manual fetch
        const updated = await TaskService.getByProjectId(sprint!.duAnId);
        setTasks(updated);
        showToast('Cập nhật công việc thành công!');
      }
    } catch (error) {
      console.error('Update task failed:', error);
      showToast('Cập nhật thất bại.', 'error');
    }
  };

  const handleTaskAction = async (taskId: number, approve: boolean) => {
    if (!isSprintActive) {
      showToast('Chỉ có thể phê duyệt công việc khi Sprint đang hoạt động.', 'error');
      return;
    }
    
    if (!approve) {
      // Nếu là từ chối -> Mở Modal nhập lý do
      const task = tasks.find(t => t.id === taskId);
      if (task) {
        setTaskToReject(task);
        setIsRejectionModalOpen(true);
      }
      return;
    }

    try {
      const targetStatus = 3; // Done
      const success = await TaskService.updateStatus(taskId, targetStatus);
      if (success) {
        setTasks(prev => prev.map(t => t.id === taskId ? { ...t, trangThai: targetStatus as any } : t));
        showToast('Đã phê duyệt công việc.');
      }
    } catch (error: any) {
      console.error('Action failed:', error);
      showToast(error.message || 'Thao tác thất bại.', 'error');
    }
  };

  const handleConfirmRejection = async (reason: string) => {
    if (!taskToReject) return;
    try {
      const success = await TaskService.reject(taskToReject.id, reason);
      if (success) {
        setIsRejectionModalOpen(false);
        setTaskToReject(null);
        showToast('Đã từ chối công việc và gửi yêu cầu sửa lại.');
        fetchData(true); // Refresh
      }
    } catch (error: any) {
      showToast(error.message || 'Không thể từ chối.', 'error');
    }
  };

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask || !newComment.trim()) return;

    try {
      setIsSendingComment(true);
      const comment = await TaskService.addComment(editingTask.id, {
        noiDung: newComment,
        loai: 0 // Thảo luận
      });
      
      // Update local state
      setEditingTask({
        ...editingTask,
        traoLois: [...(editingTask.traoLois || []), comment]
      });
      setNewComment('');
    } catch (error) {
      showToast('Không thể gửi bình luận.', 'error');
    } finally {
      setIsSendingComment(false);
    }
  };

  const isAdmin = currentUser?.vaiTros?.some((r: string) => {
    const normalized = r.toLowerCase().replace(/\s+/g, '');
    return normalized === 'quanly' || normalized === 'admin' || normalized === 'quảnlý';
  });

  const userProjectMember = projectMembers.find(m => m.id === currentUser?.id);
  const isProjectPM = userProjectMember?.vaiTro === ProjectRole.PM;

  const isSprintActive = !!sprint && sprint.trangThai !== 2;

  // Quyền quản lý Task (Giao việc, chỉnh sửa)
  // Cho phép Admin, PM dự án, hoặc người tạo Task
  // Và Sprint phải ở trạng thái New (0) hoặc InProgress (1)
  const canManageTask = (task: CongViecDto) => {
    if (!sprint || sprint.trangThai === 2) return false; // Không cho sửa khi đã hoàn thành
    
    // Admin hoặc PM dự án có quyền quản lý mọi task
    if (isAdmin || isProjectPM) return true;
    
    // Người tạo task có quyền quản lý task của mình
    if (currentUser?.id === task.createdBy) return true;

    return false;
  };

  const handleUpdateSprintStatus = async (status: number) => {
    if (!sprint) return;
    try {
      const success = await SprintService.update(sprint.id, {
        tenSprint: sprint.tenSprint,
        ngayBatDau: sprint.ngayBatDau,
        ngayKetThuc: sprint.ngayKetThuc,
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
        tieuDe: '', moTa: '', loaiCongViec: 0, doUuTien: 1, thoiGianUocTinh: 0, assigneeId: null
      });
      showToast('Đã tạo công việc mới thành công!');
    } catch (error) {
      console.error('Create task failed:', error);
      showToast('Tạo công việc thất bại.', 'error');
    } finally {
      setIsCreatingTask(false);
    }
  };

  const handleMoveToSprint = async (taskId: number, targetSprintId: number) => {
    const task = tasks.find(t => t.id === taskId);
    if (!task) return;

    try {
      const payload: any = {
        tieuDe: task.tieuDe,
        moTa: task.moTa,
        loaiCongViec: task.loaiCongViec,
        doUuTien: task.doUuTien,
        assigneeId: task.assigneeId,
        thoiGianUocTinh: task.thoiGianUocTinh,
        sprintId: targetSprintId
      };
      
      const res = await TaskService.update(taskId, payload);
      if (res.statusCode === 200) {
        showToast(`Đã chuyển công việc sang Sprint mới.`);
        // Reload danh sách task của sprint hiện tại (nó sẽ biến mất khỏi bảng này)
        setTasks(prev => prev.filter(t => t.id !== taskId));
        setActiveMenuTaskId(null);
      }
    } catch (error: any) {
      showToast(error.message || 'Chuyển Sprint thất bại.', 'error');
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
        showToast(`Đã gán việc cho ${assigneeName || 'người mới'}`);
      }
    } catch (error) {
      console.error('Manual assign failed:', error);
      showToast('Không thể cập nhật người thực hiện. Vui lòng thử lại.', 'error');
    }
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (!(e.target as Element).closest('.assignee-wrapper')) {
        setActiveDropdownTaskId(null);
      }
      if (!(e.target as Element).closest('.more-menu-container')) {
        setActiveMenuTaskId(null);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  if (loading) return <div className="loading-state">Đang tải chi tiết Sprint...</div>;
  if (!sprint) return <div className="error-state">Không tìm thấy Sprint.</div>;

  return (
    <div className="sprint-detail-container">
      {/* Sprint Status Warning Banner (Chỉ hiện khi đã kết thúc) */}
      {sprint.trangThai === 2 && (
        <div className="sprint-status-banner finished">
          <AlertCircle size={18} />
          <span>Sprint này đã kết thúc. Bạn không thể thay đổi tiến độ công việc.</span>
        </div>
      )}

      {/* Banner Sprint đang chạy - Admin/PM có thể chốt kết quả */}
      {(isAdmin || isProjectPM) && sprint.trangThai === 1 && (
        <div className="sprint-status-banner in-progress-status">
          <Activity size={18} />
          <span>Sprint đang chạy. Có thể chốt kết quả khi hoàn thành tất cả task.</span>
          <button className="btn-finish-sprint-inline ml-auto" style={{ background: '#10b981' }} onClick={() => handleUpdateSprintStatus(2)}>
            <CheckSquare size={14} /> Hoàn thành Sprint
          </button>
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
          {(isAdmin || isProjectPM || [ProjectRole.Tester, ProjectRole.QA].includes(userProjectMember?.vaiTro as any)) && (
            <div className="view-mode-toggle">
              <button 
                className={`toggle-btn ${viewMode === 'kanban' ? 'active' : ''}`}
                onClick={() => setViewMode('kanban')}
                title="Bảng Kanban"
              >
                <CheckSquare size={18} />
              </button>
              <button 
                className={`toggle-btn ${viewMode === 'tester' ? 'active' : ''}`}
                onClick={() => setViewMode('tester')}
                title="Màn hình Kiểm thử"
              >
                <Play size={18} />
              </button>
            </div>
          )}
          {isAdmin && sprint && (sprint.trangThai === 1 || (sprint.trangThai === 0 && tasks.length > 0 && tasks.every(t => t.trangThai === StatusEnum.Done))) && (
            <button className="btn-finish-sprint-inline" onClick={() => handleUpdateSprintStatus(2)}>
              <CheckSquare size={18} />
              <span>{sprint.trangThai === 0 ? "Chốt & Hoàn thành" : "Hoàn thành Sprint"}</span>
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

      {/* Main View Area */}
      {viewMode === 'kanban' ? (
        <div className={`kanban-board ${sprint.trangThai === 2 ? 'read-only' : ''}`}>
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
                  <div key={task.id} className="task-card" onClick={() => handleEditClick(task)}>
                    <div className="task-card-header">
                      <span className={`priority-tag ${getPriorityLabel(task.doUuTien).class}`}>
                        {getPriorityLabel(task.doUuTien).text}
                      </span>
                      {task.soLanBiTuChoi > 0 && (
                        <div className={`rejection-badge ${task.soLanBiTuChoi >= 3 ? 'urgent' : ''}`} title={`Bị từ chối ${task.soLanBiTuChoi} lần`}>
                          <AlertTriangle size={12} />
                          <span>{task.soLanBiTuChoi} lần</span>
                        </div>
                      )}
                      <div className="task-card-actions">
                        <span className="task-id">#{task.id}</span>
                        {canManageTask(task) && (
                          <div className="more-menu-container">
                            <button 
                              className="more-action-btn"
                              onClick={(e) => {
                                e.stopPropagation();
                                setActiveMenuTaskId(activeMenuTaskId === task.id ? null : task.id);
                              }}
                            >
                              <MoreVertical size={14} />
                            </button>
                            {activeMenuTaskId === task.id && (
                              <div className="task-dropdown-menu fade-in">
                                <button onClick={(e) => { e.stopPropagation(); handleEditClick(task); }}>
                                  <Edit size={12} /> Sửa
                                </button>
                                
                                <div className="menu-divider"></div>
                                <div className="menu-label">Di chuyển sang:</div>
                                {allSprints.filter(s => s.id !== Number(id) && s.trangThai !== 2).map(s => (
                                  <button 
                                    key={s.id} 
                                    className="move-btn"
                                    onClick={(e) => { e.stopPropagation(); handleMoveToSprint(task.id, s.id); }}
                                  >
                                    <Play size={12} /> {s.tenSprint}
                                  </button>
                                ))}

                                <div className="menu-divider"></div>
                                <button 
                                  className="delete-btn" 
                                  onClick={(e) => { e.stopPropagation(); handleDeleteTask(task.id); }}
                                >
                                  <Trash2 size={12} /> Xóa
                                </button>
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    </div>

                    <h4 className="task-title">{task.tieuDe}</h4>

                    {task.dependencies && task.dependencies.length > 0 && (
                      <div className="task-card-dependencies">
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

                    {/* Quick Approval Actions for Managers & Testers */}
                    {col.id === StatusEnum.Review && (
                      isAdmin || isProjectPM || [ProjectRole.Tester, ProjectRole.QA].includes(userProjectMember?.vaiTro as any)
                    ) && isSprintActive && (
                      <div className="task-approval-actions">
                        <button
                          className="btn-reject-small"
                          onClick={(e) => { e.stopPropagation(); handleTaskAction(task.id, false); }}
                          title="Từ chối"
                        >
                          <XCircle size={14} />
                          Từ chối
                        </button>
                        <button
                          className="btn-approve-small"
                          onClick={(e) => { e.stopPropagation(); handleTaskAction(task.id, true); }}
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
                          className={`assignee ${canManageTask(task) ? 'editable' : ''}`}
                          onClick={(e) => {
                            if (canManageTask(task)) {
                              e.stopPropagation();
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
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="tester-view-container fade-in">
          <div className="tester-sidebar">
            <div className="sidebar-header">
              <h3>Công việc cần Test</h3>
              <span className="count-badge">{tasks.filter(t => t.trangThai === StatusEnum.Review).length}</span>
            </div>
            <div className="tester-task-list">
              {tasks.filter(t => t.trangThai === StatusEnum.Review).map(task => (
                <div 
                  key={task.id} 
                  className={`tester-task-card ${selectedTestTaskId === task.id ? 'active' : ''}`}
                  onClick={() => setSelectedTestTaskId(task.id)}
                >
                  <div className="card-top">
                    <span className="task-id">#{task.id}</span>
                    <span className={`priority-dot ${getPriorityLabel(task.doUuTien).class}`}></span>
                  </div>
                  <h4 className="task-title">{task.tieuDe}</h4>
                  <div className="card-bottom">
                    <div className="assignee">
                      <div className="avatar micro">{task.assigneeName?.charAt(0) || '?'}</div>
                      <span>{task.assigneeName}</span>
                    </div>
                    {task.soLanBiTuChoi > 0 && <span className="rejection-count">{task.soLanBiTuChoi} lần từ chối</span>}
                  </div>
                </div>
              ))}
              {tasks.filter(t => t.trangThai === StatusEnum.Review).length === 0 && (
                <div className="empty-state">
                  <CheckCircle size={32} />
                  <p>Tuyệt vời! Không còn công việc nào cần test trong Sprint này.</p>
                </div>
              )}
            </div>
          </div>
          
          <div className="tester-main-content">
            {selectedTestTaskId ? (
              (() => {
                const task = tasks.find(t => t.id === selectedTestTaskId);
                if (!task) return null;
                return (
                  <div className="tester-detail-panel fade-in">
                    <div className="detail-header">
                      <div className="title-area">
                        <div className="id-badge">#{task.id}</div>
                        <h2>{task.tieuDe}</h2>
                      </div>
                      <div className="action-area">
                        <button className="btn-reject-large" onClick={() => handleTaskAction(task.id, false)}>
                          <XCircle size={20} /> Từ chối
                        </button>
                        <button className="btn-approve-large" onClick={() => handleTaskAction(task.id, true)}>
                          <CheckCircle size={20} /> Phê duyệt
                        </button>
                      </div>
                    </div>

                    <div className="detail-body">
                      <div className="info-section">
                        <div className="info-group">
                          <label>Người thực hiện</label>
                          <div className="value-box">
                            <div className="avatar small">{task.assigneeName?.charAt(0)}</div>
                            <span>{task.assigneeName}</span>
                          </div>
                        </div>
                        <div className="info-group">
                          <label>Độ ưu tiên</label>
                          <span className={`priority-badge ${getPriorityLabel(task.doUuTien).class}`}>
                            {getPriorityLabel(task.doUuTien).text}
                          </span>
                        </div>
                        <div className="info-group">
                          <label>Ước tính</label>
                          <div className="value-box"><Clock size={14} /> {task.thoiGianUocTinh}h</div>
                        </div>
                      </div>

                      <div className="description-section">
                        <label>Mô tả công việc</label>
                        <div className="description-box">
                          {task.moTa || <span className="no-data">Không có mô tả chi tiết.</span>}
                        </div>
                      </div>

                      <div className="discussion-section">
                        <div className="section-header">
                          <MessageSquare size={18} />
                          <h3>Trao đổi & Phản hồi</h3>
                        </div>
                        <div className="tester-comment-area">
                          <div className="comment-list">
                            {!task.traoLois || task.traoLois.length === 0 ? (
                              <div className="empty-comments">Chưa có thảo luận nào.</div>
                            ) : (
                              task.traoLois.map(c => (
                                <div key={c.id} className={`comment-item ${c.loai === 1 ? 'rejection-reason' : ''}`}>
                                  <div className="comment-header">
                                    <span className="author">{c.tenNguoiTao}</span>
                                    <span className="time">{new Date(c.createdAt).toLocaleString('vi-VN')}</span>
                                    {c.loai === 1 && <span className="type-tag">Lý do từ chối</span>}
                                  </div>
                                  <div className="comment-body">{c.noiDung}</div>
                                </div>
                              ))
                            )}
                          </div>
                          <form className="comment-input-area" onSubmit={async (e) => {
                            e.preventDefault();
                            if (!newComment.trim()) return;
                            try {
                              setIsSendingComment(true);
                              const comment = await TaskService.addComment(task.id, { noiDung: newComment, loai: 0 });
                              setTasks(prev => prev.map(t => t.id === task.id ? { ...t, traoLois: [...(t.traoLois || []), comment] } : t));
                              setNewComment('');
                            } catch (e) {
                              showToast('Không thể gửi bình luận.', 'error');
                            } finally {
                              setIsSendingComment(false);
                            }
                          }}>
                            <textarea 
                              placeholder="Nhập nội dung phản hồi..." 
                              value={newComment}
                              onChange={e => setNewComment(e.target.value)}
                              disabled={isSendingComment}
                            />
                            <button type="submit" disabled={isSendingComment || !newComment.trim()}>
                              <Send size={16} />
                            </button>
                          </form>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })()
            ) : (
              <div className="tester-no-selection">
                <Search size={48} />
                <p>Chọn một công việc từ danh sách bên trái để bắt đầu kiểm thử.</p>
              </div>
            )}
          </div>
        </div>
      )}

      {showEditTaskModal && (
        <div className="sprint-modal-overlay" onClick={() => { setShowEditTaskModal(false); setEditingTask(null); }}>
          <div className="sprint-modal-content" onClick={e => e.stopPropagation()}>
            <div className="sprint-modal-header">
              <h2>Chỉnh sửa Công việc</h2>
              <button className="close-btn" onClick={() => { setShowEditTaskModal(false); setEditingTask(null); }}>✕</button>
            </div>
            <div className="sprint-modal-scroll-area">
              <form onSubmit={handleUpdateTask} className="sprint-modal-body">
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
                  />
                </div>
                <div className="form-row">
                  <div className="form-group" style={{ flex: 2 }}>
                    <label>Người thực hiện</label>
                    <select 
                      value={newTaskData.assigneeId || ''}
                      onChange={e => setNewTaskData({...newTaskData, assigneeId: e.target.value ? parseInt(e.target.value) : null})}
                      style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
                    >
                      <option value="">-- Chưa giao --</option>
                      {projectMembers.map(member => (
                        <option key={member.id} value={member.id}>
                          {member.hoTen}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group" style={{ flex: 1 }}>
                    <label>Loại công việc</label>
                    <select 
                      value={newTaskData.loaiCongViec}
                      onChange={e => setNewTaskData({...newTaskData, loaiCongViec: parseInt(e.target.value)})}
                      style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
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
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Độ ưu tiên</label>
                    <select 
                      value={newTaskData.doUuTien}
                      onChange={e => setNewTaskData({...newTaskData, doUuTien: parseInt(e.target.value)})}
                      style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
                    >
                      <option value={0}>Thấp</option>
                      <option value={1}>Vừa</option>
                      <option value={2}>Cao</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Ước tính (giờ)</label>
                    <input 
                      type="number" 
                      value={newTaskData.thoiGianUocTinh}
                      onChange={e => setNewTaskData({...newTaskData, thoiGianUocTinh: parseInt(e.target.value)})}
                      min="0"
                    />
                  </div>
                </div>
                <div className="modal-actions">
                  <button type="button" className="btn-cancel" onClick={() => { setShowEditTaskModal(false); setEditingTask(null); }}>Đóng</button>
                  {editingTask && canManageTask(editingTask) && (
                    <button type="submit" className="btn-save">Lưu Thay đổi</button>
                  )}
                </div>
              </form>

              {/* Comment Section in Detail Modal */}
              {editingTask && (
                <div className="task-discussion-section">
                  <h3><MessageSquare size={18} /> Thảo luận & Phản hồi</h3>
                  <div className="comment-list">
                    {!editingTask.traoLois || editingTask.traoLois.length === 0 ? (
                      <div className="empty-comments">Chưa có thảo luận nào cho công việc này.</div>
                    ) : (
                      editingTask.traoLois.map(c => (
                        <div key={c.id} className={`comment-item ${c.loai === 1 ? 'rejection-reason' : ''}`}>
                          <div className="comment-header">
                            <span className="author">{c.tenNguoiTao}</span>
                            <span className="time"><Clock size={12} /> {new Date(c.createdAt).toLocaleString('vi-VN')}</span>
                            {c.loai === 1 && <span className="type-tag">Lý do từ chối</span>}
                          </div>
                          <div className="comment-body">{c.noiDung}</div>
                        </div>
                      ))
                    )}
                  </div>
                  <form className="comment-input-area" onSubmit={handleAddComment}>
                    <textarea 
                      placeholder="Nhập nội dung trao đổi..." 
                      value={newComment}
                      onChange={e => setNewComment(e.target.value)}
                      disabled={isSendingComment}
                    />
                    <button type="submit" disabled={isSendingComment || !newComment.trim()}>
                      <Send size={16} />
                    </button>
                  </form>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Rejection Modal */}
      <RejectionModal
        isOpen={isRejectionModalOpen}
        onClose={() => { setIsRejectionModalOpen(false); setTaskToReject(null); }}
        onConfirm={handleConfirmRejection}
        taskTitle={taskToReject?.tieuDe || ''}
      />

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
                <div className="form-group" style={{ flex: 2 }}>
                  <label>Người thực hiện</label>
                  <select 
                    value={newTaskData.assigneeId || ''}
                    onChange={e => setNewTaskData({...newTaskData, assigneeId: e.target.value ? parseInt(e.target.value) : null})}
                    style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
                  >
                    <option value="">-- Chưa giao --</option>
                    {projectMembers.map(member => (
                      <option key={member.id} value={member.id}>
                        {member.hoTen}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group" style={{ flex: 1 }}>
                  <label>Loại công việc</label>
                  <select 
                    value={newTaskData.loaiCongViec}
                    onChange={e => setNewTaskData({...newTaskData, loaiCongViec: parseInt(e.target.value)})}
                    style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
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
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Độ ưu tiên</label>
                  <select 
                    value={newTaskData.doUuTien}
                    onChange={e => setNewTaskData({...newTaskData, doUuTien: parseInt(e.target.value)})}
                    style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none', width: '100%', fontSize: '0.9rem' }}
                  >
                    <option value={0}>Thấp</option>
                    <option value={1}>Vừa</option>
                    <option value={2}>Cao</option>
                  </select>
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

      {/* Confirm Modal */}
      <ConfirmModal
        isOpen={confirmConfig.isOpen}
        title={confirmConfig.title}
        message={confirmConfig.message}
        type={confirmConfig.type}
        onConfirm={confirmConfig.onConfirm}
        onClose={() => setConfirmConfig({ ...confirmConfig, isOpen: false })}
      />

      {/* Toast Notification */}
      {toast && (
        <Toast 
          message={toast.message} 
          type={toast.type} 
          onClose={() => setToast(null)} 
        />
      )}
    </div>
  );
};

export default SprintDetailPage;
