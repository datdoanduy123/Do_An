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

  const fetchData = async () => {
    if (!id) return;
    try {
      setLoading(true);
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
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [id]);

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
                  <Play size={14} fill="currentColor"/> Kích hoạt ngay
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
          <button className="create-task-btn">
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
                        title="Bác bỏ"
                      >
                        <XCircle size={14} />
                        Bác bỏ
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
    </div>
  );
};

export default SprintDetailPage;
