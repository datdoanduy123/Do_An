import React, { useEffect, useState } from 'react';
import { 
  Clock, 
  Calendar,
  Briefcase,
  Layers,
  User as UserIcon,
  CheckSquare,
  ArrowRight
} from 'lucide-react';


import ProjectService from '../../services/ProjectService';
import type { DuAnDto } from '../../services/ProjectTypes';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintTypes';
import TaskService from '../../services/TaskService';
import type { CongViecDto } from '../../services/TaskTypes';
import { TrangThaiCongViec } from '../../services/TaskTypes';
import UserService from '../../services/UserService';
import './Dashboard.css';

const DashboardPage: React.FC = () => {
  const [userProfile, setUserProfile] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  
  // Data States
  const [projects, setProjects] = useState<DuAnDto[]>([]);
  const [selectedProjectId, setSelectedProjectId] = useState<number | ''>('');
  
  const [sprints, setSprints] = useState<SprintDto[]>([]);
  const [tasks, setTasks] = useState<CongViecDto[]>([]);
  const [boardLoading, setBoardLoading] = useState(false);

  // Khởi tạo: Lấy profile và danh sách dự án
  useEffect(() => {
    const initData = async () => {
      try {
        setLoading(true);
        const [profile, projList] = await Promise.all([
          UserService.getProfile(),
          ProjectService.getProjects()
        ]);
        setUserProfile(profile);
        setProjects(projList || []);
        
        // Mặc định chọn dự án đầu tiên nếu có
        if (projList && projList.length > 0) {
          setSelectedProjectId(projList[0].id);
        }
      } catch (error) {
        console.error('Error init dashboard:', error);
      } finally {
        setLoading(false);
      }
    };
    initData();
  }, []);

  // Fetch Sprints và Tasks khi đổi Dự án
  useEffect(() => {
    const fetchBoardData = async () => {
      if (!selectedProjectId) return;
      try {
        setBoardLoading(true);
        const [sprintList, taskList] = await Promise.all([
          SprintService.getByProjectId(Number(selectedProjectId)),
          TaskService.getByProjectId(Number(selectedProjectId))
        ]);
        setSprints(sprintList || []);
        setTasks(taskList || []);
      } catch (error) {
        console.error('Error fetching board data:', error);
      } finally {
        setBoardLoading(false);
      }
    };
    fetchBoardData();
  }, [selectedProjectId]);


  const getSprintStatusLabel = (status: number, tienDo: number, hasActiveTasks: boolean) => {
    if (status === 2) return { text: 'Hoàn thành', class: 'bg-emerald-100 text-emerald-700' };
    if (status === 1 || tienDo > 0 || hasActiveTasks) return { text: 'Đang chạy', class: 'bg-blue-100 text-blue-700' };
    return { text: 'Mới tạo', class: 'bg-slate-100 text-slate-700' };
  };

  if (loading) {
    return (
      <div className="loading-container-full">
        <div className="modern-loader"></div>
        <p style={{ fontWeight: 600, color: '#6366f1' }}>Đang khởi động Master Board...</p>
      </div>
    );
  }

  return (
    <div className="dashboard-master-board">
      {/* Header Select */}
      <header className="master-header">
        <div className="welcome-part">
          <div className="master-avatar">
            {userProfile?.hoTen?.charAt(0) || <UserIcon size={24}/>}
          </div>
          <div className="welcome-text">
            <h1>Tổng quan điều hành dự án</h1>
            <p>Xin chào <strong>{userProfile?.hoTen || 'Quản lý'}</strong>. Bạn đang xem báo cáo trực tiếp toàn bộ dự án.</p>
          </div>
        </div>
        
        <div className="project-selector-wrapper">
          <label><Briefcase size={16}/> Chọn Dự Án Theo Dõi:</label>
          <div className="select-container">
            <select 
              className="master-project-select"
              value={selectedProjectId}
              onChange={(e) => setSelectedProjectId(e.target.value ? Number(e.target.value) : '')}
            >
              {projects.length === 0 && <option value="">Không có dự án nào</option>}
              {projects.map(p => (
                <option key={p.id} value={p.id}>
                  🏷️ {p.tenDuAn} (Mã: #{p.id})
                </option>
              ))}
            </select>
          </div>
        </div>
      </header>

      {/* Main Board Area */}
      {selectedProjectId ? (
        <div className="master-board-content">
          {boardLoading ? (
             <div className="board-loading">Đang tải cấu trúc dữ liệu...</div>
          ) : sprints.length > 0 ? (
             <div className="sprint-swimlanes">
               {sprints.map(sprint => {
                 // Lấy các task thuộc sprint này
                 const sprintTasks = tasks.filter(t => t.sprintId === sprint.id);
                 const hasActive = sprintTasks.some(t => t.trangThai > 0 && t.trangThai < 3);
                 const statusStyle = getSprintStatusLabel(sprint.trangThai!, sprint.tienDo || 0, hasActive);
                 
                 return (
                   <div key={sprint.id} className="sprint-lane">
                     {/* Băng rôn (Header) của Sprint */}
                     <div className="sprint-lane-header">
                       <div className="sprint-lane-title">
                         <h2><Layers size={20}/> {sprint.tenSprint}</h2>
                         <span className={`sprint-badge ${statusStyle.class}`}>{statusStyle.text}</span>
                         <span className="sprint-dates">
                           <Calendar size={14}/> 
                           {new Date(sprint.ngayBatDau).toLocaleDateString()} <ArrowRight size={12}/> {new Date(sprint.ngayKetThuc).toLocaleDateString()}
                         </span>
                       </div>
                       <div className="sprint-lane-progress">
                         <span>Tiến độ: <strong>{Math.round(sprint.tienDo || 0)}%</strong></span>
                         <div className="progress-bar-bg-mini">
                           <div className="progress-bar-fill-mini" style={{width: `${Math.round(sprint.tienDo || 0)}%`}}></div>
                         </div>
                       </div>
                     </div>

                     {/* Kanban Board (Nhúng) */}
                     <div className="sprint-kanban-board">
                       {[
                         { status: TrangThaiCongViec.Todo, title: 'Chưa bắt đầu', className: 'col-todo' },
                         { status: TrangThaiCongViec.InProgress, title: 'Đang làm', className: 'col-inprog' },
                         { status: TrangThaiCongViec.Review, title: 'Chờ duyệt', className: 'col-review' },
                         { status: TrangThaiCongViec.Done, title: 'Hoàn thành', className: 'col-done' }
                       ].map(col => {
                         const colTasks = sprintTasks.filter(t => t.trangThai === col.status);
                         return (
                           <div key={col.status} className={`kanban-column ${col.className}`}>
                             <div className="k-col-header">
                               <h4>{col.title}</h4>
                               <span className="k-count">{colTasks.length}</span>
                             </div>
                             <div className="k-col-body">
                               {colTasks.length === 0 ? (
                                 <div className="empty-slot">Trống</div>
                               ) : (
                                 colTasks.map(task => (
                                   <div key={task.id} className="k-task-card">
                                     <div className="k-task-header">
                                       <span className="k-task-id">#{task.id}</span>
                                       {task.doUuTien === 2 ? <span className="k-prio high">Nguy cấp</span> :
                                        task.doUuTien === 1 ? <span className="k-prio medium">Ưu tiên</span> :
                                        null}
                                     </div>
                                     <h5 className="k-task-title" title={task.tieuDe}>{task.tieuDe}</h5>
                                     <div className="k-task-footer">
                                       <div className="k-assignee">
                                          <div className="k-avatar">
                                            {task.assigneeName ? task.assigneeName.charAt(0).toUpperCase() : '?'}
                                          </div>
                                          <span className="k-name">{task.assigneeName || 'Chưa giao'}</span>
                                       </div>
                                       <div className="k-time">
                                         <Clock size={12}/> {task.thoiGianUocTinh}h
                                       </div>
                                     </div>
                                   </div>
                                 ))
                               )}
                             </div>
                           </div>
                         );
                       })}
                     </div>
                   </div>
                 );
               })}
             </div>
          ) : (
             <div className="empty-master-board">
                <CheckSquare size={48} className="empty-icon"/>
                <h3>Dự án này chưa có Sprint nào!</h3>
                <p>Vui lòng cập nhật tài liệu hoặc tạo Sprint mới để bắt đầu theo dõi tiến độ.</p>
             </div>
          )}
        </div>
      ) : (
         <div className="empty-master-board">Vui lòng chọn một dự án để xem tổng quan.</div>
      )}
    </div>
  );
};

export default DashboardPage;

