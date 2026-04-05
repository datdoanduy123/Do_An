import React, { useEffect, useState } from 'react';
import { 
  Clock, 
  Calendar,
  Briefcase,
  Layers,
  User as UserIcon,
  CheckSquare,
  ArrowRight,
  BarChart2,
  PieChart,
  Activity,
  TrendingUp,
  Target,
  Zap,
  ChevronRight,
  Plus
} from 'lucide-react';
import {
  ResponsiveContainer,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
  PieChart as RePieChart,
  Pie,
  Cell,
  BarChart as ReBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as ReTooltip,
  Legend
} from 'recharts';

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
  const [skillCoverage, setSkillCoverage] = useState<any[]>([]);
  const [activeTab, setActiveTab] = useState<'overview' | 'operations'>('overview');
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

  // Fetch Sprints, Tasks, và Skill Coverage khi đổi Dự án
  useEffect(() => {
    const fetchBoardData = async () => {
      if (!selectedProjectId) return;
      try {
        setBoardLoading(true);
        const [sprintList, taskList, skillData] = await Promise.all([
          SprintService.getByProjectId(Number(selectedProjectId)),
          TaskService.getByProjectId(Number(selectedProjectId)),
          ProjectService.getSkillCoverage(Number(selectedProjectId))
        ]);
        setSprints(sprintList || []);
        setTasks(taskList || []);
        setSkillCoverage(skillData || []);
      } catch (error) {
        console.error('Error fetching board data:', error);
      } finally {
        setBoardLoading(false);
      }
    };
    fetchBoardData();
  }, [selectedProjectId]);

  // --- Logic tính toán số liệu Thống kê ---
  
  // 1. Thống kê trạng thái Công việc (Pie Chart)
  const statusData = [
    { name: 'Chưa làm', value: tasks.filter(t => t.trangThai === TrangThaiCongViec.Todo).length, color: '#94a3b8' },
    { name: 'Đang làm', value: tasks.filter(t => t.trangThai === TrangThaiCongViec.InProgress).length, color: '#3b82f6' },
    { name: 'Chờ duyệt', value: tasks.filter(t => t.trangThai === TrangThaiCongViec.Review).length, color: '#f59e0b' },
    { name: 'Hoàn thành', value: tasks.filter(t => t.trangThai === TrangThaiCongViec.Done).length, color: '#10b981' },
  ].filter(d => d.value > 0);

  // 2. Thống kê khối lượng công việc theo Thành viên (Bar Chart)
  const memberWorkload = Array.from(new Set(tasks.map(t => t.assigneeName || 'Chưa giao'))).map(name => ({
    name,
    total: tasks.filter(t => (t.assigneeName || 'Chưa giao') === name).length,
    done: tasks.filter(t => (t.assigneeName || 'Chưa giao') === name && t.trangThai === TrangThaiCongViec.Done).length
  }));

  // 3. Tính toán các KPI tổng quát
  const stats = {
    totalProjects: projects.length,
    activeSprints: sprints.filter(s => s.trangThai === 1).length,
    totalTasks: tasks.length,
    overallProgress: tasks.length > 0 ? (tasks.filter(t => t.trangThai === TrangThaiCongViec.Done).length / tasks.length) * 100 : 0
  };

  const getSprintStatusLabel = (status: number, tienDo: number, hasActiveTasks: boolean) => {
    if (status === 2) return { text: 'Hoàn thành', class: 'bg-emerald-100 text-emerald-700' };
    if (status === 1 || tienDo > 0 || hasActiveTasks) return { text: 'Đang chạy', class: 'bg-blue-100 text-blue-700' };
    return { text: 'Mới tạo', class: 'bg-slate-100 text-slate-700' };
  };

  // --- Giao diện Overview (Charts & KPIs) ---
  const renderOverview = () => (
    <div className="dashboard-overview-tab fade-in">
      {/* 3.1. KPI Cards Row */}
      <div className="kpi-grid">
        <div className="kpi-card gradient-blue">
          <div className="kpi-icon"><Briefcase size={24}/></div>
          <div className="kpi-info">
            <span className="kpi-label">Tổng Dự Án</span>
            <span className="kpi-value">{stats.totalProjects}</span>
          </div>
          <div className="kpi-mini-chart"><Activity size={16}/> +2 tháng này</div>
        </div>
        <div className="kpi-card gradient-purple">
          <div className="kpi-icon"><Zap size={24}/></div>
          <div className="kpi-info">
            <span className="kpi-label">Sprint Đang Chạy</span>
            <span className="kpi-value">{stats.activeSprints}</span>
          </div>
          <div className="kpi-trend">Đang vận hành</div>
        </div>
        <div className="kpi-card gradient-emerald">
          <div className="kpi-icon"><CheckSquare size={24}/></div>
          <div className="kpi-info">
            <span className="kpi-label">Tổng Công Việc</span>
            <span className="kpi-value">{stats.totalTasks}</span>
          </div>
          <div className="kpi-stat">Trong dự án hiện tại</div>
        </div>
        <div className="kpi-card gradient-orange">
          <div className="kpi-icon"><Target size={24}/></div>
          <div className="kpi-info">
            <span className="kpi-label">Hoàn Thành Mục Tiêu</span>
            <span className="kpi-value">{Math.round(stats.overallProgress)}%</span>
          </div>
          <div className="kpi-progress-bg">
            <div className="kpi-progress-fill" style={{width: `${stats.overallProgress}%`}}></div>
          </div>
        </div>
      </div>

      {/* 3.2. Charts Section */}
      <div className="charts-main-grid">
        <div className="chart-container large-chart">
          <div className="chart-header">
            <h3><TrendingUp size={18}/> Khối lượng công việc theo Thành viên</h3>
          </div>
          <div className="chart-body" style={{ height: 300 }}>
            <ResponsiveContainer width="100%" height="100%">
              <ReBarChart data={memberWorkload} margin={{ top: 20, right: 30, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{fill: '#64748b', fontSize: 12}} />
                <YAxis axisLine={false} tickLine={false} tick={{fill: '#64748b', fontSize: 12}} />
                <ReTooltip 
                  cursor={{fill: '#f8fafc'}}
                  contentStyle={{borderRadius: '12px', border: 'none', boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)'}}
                />
                <Legend iconType="circle" wrapperStyle={{paddingTop: '20px'}} />
                <Bar dataKey="total" name="Tổng cộng" fill="#6366f1" radius={[4, 4, 0, 0]} barSize={30} />
                <Bar dataKey="done" name="Đã xong" fill="#10b981" radius={[4, 4, 0, 0]} barSize={30} />
              </ReBarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="chart-container">
          <div className="chart-header">
            <h3><PieChart size={18}/> Trạng thái công việc</h3>
          </div>
          <div className="chart-body" style={{ height: 300 }}>
            <ResponsiveContainer width="100%" height="100%">
              <RePieChart>
                <Pie
                  data={statusData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  paddingAngle={5}
                  dataKey="value"
                >
                  {statusData.map((entry: any, index: number) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <ReTooltip />
                <Legend verticalAlign="bottom" height={36}/>
              </RePieChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="chart-container">
          <div className="chart-header">
            <h3><Activity size={18}/> Độ phủ Kỹ năng (Team Radar)</h3>
          </div>
          <div className="chart-body" style={{ height: 300 }}>
            <ResponsiveContainer width="100%" height="100%">
              <RadarChart cx="50%" cy="50%" outerRadius="80%" data={skillCoverage}>
                <PolarGrid stroke="#e2e8f0" />
                <PolarAngleAxis dataKey="skillName" tick={{fill: '#64748b', fontSize: 11}} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} axisLine={false} tick={false} />
                <Radar
                  name="Độ phủ kỹ năng"
                  dataKey="coveragePercent"
                  stroke="#8b5cf6"
                  fill="#8b5cf6"
                  fillOpacity={0.4}
                />
                <ReTooltip />
              </RadarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );

  // --- Giao diện Operations (Kanban Board cũ) ---
  const renderOperations = () => (
    <div className="sprint-swimlanes fade-in">
      {sprints.map(sprint => {
        const sprintTasks = tasks.filter(t => t.sprintId === sprint.id);
        const hasActive = sprintTasks.some(t => t.trangThai > 0 && t.trangThai < 3);
        const statusStyle = getSprintStatusLabel(sprint.trangThai!, sprint.tienDo || 0, hasActive);
        
        return (
          <div key={sprint.id} className="sprint-lane">
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
  );

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
      {/* 1. Header Select */}
      <header className="master-header">
        <div className="welcome-part">
          <div className="master-avatar">
            {userProfile?.hoTen?.charAt(0) || <UserIcon size={24}/>}
          </div>
          <div className="welcome-text">
            <h1>Tổng quan điều hành dự án</h1>
            <p>Xin chào <strong>{userProfile?.hoTen || 'Quản lý'}</strong>. Hệ thống AI đã sẵn sàng điều hành.</p>
          </div>
        </div>
        
        <div className="project-selector-wrapper">
          <label><Briefcase size={16}/> Chọn Dự Án Điều Hành:</label>
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

      {/* 2. Tab Navigation */}
      <nav className="dashboard-tabs">
        <button 
          className={`dash-tab ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveTab('overview')}
        >
          <BarChart2 size={18}/>
          <span>Tổng quan số liệu</span>
        </button>
        <button 
          className={`dash-tab ${activeTab === 'operations' ? 'active' : ''}`}
          onClick={() => setActiveTab('operations')}
        >
          <Layers size={18}/>
          <span>Bảng điều hành Sprint</span>
        </button>
      </nav>

      {/* 3. Main Master Board Area */}
      {selectedProjectId ? (
        <div className="master-board-content">
          {boardLoading ? (
            <div className="board-loading">Đang cập nhật luồng dữ liệu thời gian thực...</div>
          ) : activeTab === 'overview' ? (
            renderOverview()
          ) : sprints.length > 0 ? (
            renderOperations()
          ) : (
            <div className="empty-master-board">
              <CheckSquare size={48} className="empty-icon"/>
                <h3>Dự án này chưa có Sprint nào!</h3>
                <p>Vui lòng cập nhật tài liệu hoặc tạo Sprint mới để bắt đầu theo dõi tiến độ.</p>
             </div>
          )}
        </div>
      ) : (
         <div className="empty-master-board">Vui lòng chọn một dự án để bắt đầu điều hành.</div>
      )}
    </div>
  );
};

export default DashboardPage;
