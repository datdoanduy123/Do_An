import React, { useEffect, useState } from 'react';
import {
  Clock,
  Calendar,
  Briefcase,
  Layers,
  User as UserIcon,
  CheckSquare,
  ArrowRight,
  AlertTriangle,
  Smile,
  Zap,
  Coffee,
  UserCheck,
  CheckCircle2,
  ArrowRightLeft,
  GitMerge // Icon dùng để hiển thị phụ thuộc task
} from 'lucide-react';
import DashboardService from '../../services/DashboardService';
import type { DashboardStats } from '../../services/DashboardService';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto, ThanhVienDuAnDto } from '../../services/ProjectTypes';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintTypes';
import TaskService from '../../services/TaskService';
import type { CongViecDto } from '../../services/TaskTypes';
import { TrangThaiCongViec } from '../../services/TaskTypes';
import UserService from '../../services/UserService';
import { AssignModal, MoveSprintModal } from '../../components/Dashboard/TaskActionModals';
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
  const [dashboardStats, setDashboardStats] = useState<DashboardStats | null>(null);

  // Danh sách thành viên của dự án đang chọn (dùng cho modal Giao lại)
  const [members, setMembers] = useState<ThanhVienDuAnDto[]>([]);

  // State quản lý 3 modal hành động trên task card
  // assignModal: task đang được giao lại
  const [assignModal, setAssignModal] = useState<CongViecDto | null>(null);
  // moveSprintModal: task đang được chuyển sprint
  const [moveSprintModal, setMoveSprintModal] = useState<CongViecDto | null>(null);
  // approveLoading: ID task đang được duyệt (để hiện spinner trên nút)
  const [approveLoading, setApproveLoading] = useState<number | null>(null);

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

  // Fetch Sprints, Tasks, Members và Dashboard Stats khi đổi Dự án
  useEffect(() => {
    const fetchBoardData = async () => {
      if (!selectedProjectId) return;
      try {
        setBoardLoading(true);
        // Fetch song song để tăng tốc độ tải
        const [sprintList, taskList, stats, memberList] = await Promise.all([
          SprintService.getByProjectId(Number(selectedProjectId)),
          TaskService.getByProjectId(Number(selectedProjectId)),
          DashboardService.getDashboardData(Number(selectedProjectId)),
          ProjectService.getMembers(Number(selectedProjectId))
        ]);
        setSprints(sprintList || []);
        setTasks(taskList || []);
        setDashboardStats(stats);
        setMembers(memberList || []);
      } catch (error) {
        console.error('Error fetching board data:', error);
      } finally {
        setBoardLoading(false);
      }
    };
    fetchBoardData();
  }, [selectedProjectId]);

  /**
   * Refresh danh sách task sau khi thực hiện hành động thành công.
   * Đóng modal và tải lại task list từ API.
   */
  const refreshTasks = async () => {
    if (!selectedProjectId) return;
    try {
      const taskList = await TaskService.getByProjectId(Number(selectedProjectId));
      setTasks(taskList || []);
    } catch (err) {
      console.error('Không thể refresh tasks:', err);
    }
    // Đóng tất cả modal sau khi refresh
    setAssignModal(null);
    setMoveSprintModal(null);
  };

  /**
   * Xử lý duyệt task (Review → Done):
   * Gọi API updateStatus với trạng thái Done (3).
   * Chỉ hiển thị nút này khi task đang ở trạng thái Review.
   */
  const handleApproveTask = async (task: CongViecDto) => {
    try {
      setApproveLoading(task.id);
      await TaskService.updateStatus(task.id, TrangThaiCongViec.Done);
      // Refresh task list sau khi duyệt thành công
      const taskList = await TaskService.getByProjectId(Number(selectedProjectId));
      setTasks(taskList || []);
    } catch (err: any) {
      alert(err.message || 'Không thể duyệt task. Vui lòng thử lại.');
    } finally {
      setApproveLoading(null);
    }
  };



  const getSprintStatusLabel = (status: number, tienDo: number, hasActiveTasks: boolean) => {
    if (status === 2) return { text: 'Hoàn thành', class: 'bg-emerald-100 text-emerald-700' };
    if (status === 1 || tienDo > 0 || hasActiveTasks) return { text: 'Đang chạy', class: 'bg-blue-100 text-blue-700' };
    return { text: 'Mới tạo', class: 'bg-slate-100 text-slate-700' };
  };

  // Render bảng theo dõi tải công việc nhân sự trong Sprint
  const renderWorkloadRegistry = () => {
    if (!dashboardStats?.sprintWorkload || dashboardStats.sprintWorkload.length === 0) return null;

    return (
      <div className="sprint-workload-registry mt-6 mb-8">
        <div className="registry-header">
          <div className="registry-main-header">
            <div className="zap-icon-wrapper">
              <Zap size={20} fill="currentColor" />
            </div>
            <h3>Cân bằng nguồn lực Sprint</h3>
          </div>

          {dashboardStats.activeSprintName && (
            <div className="sprint-name-sub">
              <span>Đang chạy</span> {dashboardStats.activeSprintName}
            </div>
          )}

          <p className="registry-desc">Giám sát tải công việc thời gian thực của các thành viên.</p>
        </div>

        <div className="workload-grid">
          {dashboardStats.sprintWorkload.map((user: any) => {
            let statusColor = '#10b981'; // Green
            let statusIcon = <Smile size={16} />;
            let statusText = 'Đang rảnh / Ổn định';

            if (user.status === 'Overloaded') {
              statusColor = '#ef4444'; // Red
              statusIcon = <AlertTriangle size={16} />;
              statusText = 'Đang quá tải';
            } else if (user.status === 'Warning') {
              statusColor = '#f59e0b'; // Amber
              statusIcon = <AlertTriangle size={16} />;
              statusText = 'Sắp đầy tải';
            } else if (user.status === 'Under-load') {
              statusColor = '#64748b'; // Slate (Under-load)
              statusIcon = <Coffee size={16} />;
              statusText = 'Dư thừa NL / Rảnh';
            }

            return (
              <div key={user.userId} className={`workload-card ${user.status.toLowerCase()}`}>
                <div className="user-info-row">
                  <div className="user-avatar-small">{user.fullName.charAt(0)}</div>
                  <div className="user-name-box">
                    <span className="name">{user.fullName}</span>
                    <span className="tasks">{user.totalTasks} task chưa xong</span>
                  </div>
                  <div className={`workload-status-badge ${user.status.toLowerCase()}`}>
                    {statusIcon}
                    <span>{statusText}</span>
                  </div>
                </div>

                <div className="progress-container">
                  <div className="progress-labels">
                    <span>Khối lượng: <strong>{user.totalHours}h</strong> / {user.capacity}h</span>
                    <span>{user.loadFactor}%</span>
                  </div>
                  <div className="workload-progress-bg">
                    <div
                      className={`workload-progress-fill ${user.status.toLowerCase()}`}
                      style={{ width: `${Math.min(user.loadFactor, 100)}%`, backgroundColor: statusColor }}
                    ></div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  // --- Giao diện Operations (Kanban Board cũ) ---
  const renderOperations = () => (
    <div className="sprint-swimlanes fade-in">
      {renderWorkloadRegistry()}
      {sprints.map(sprint => {
        const sprintTasks = tasks.filter(t => t.sprintId === sprint.id);
        const hasActive = sprintTasks.some(t => t.trangThai > 0 && t.trangThai < 3);
        const statusStyle = getSprintStatusLabel(sprint.trangThai!, sprint.tienDo || 0, hasActive);

        return (
          <div key={sprint.id} className="sprint-lane">
            <div className="sprint-lane-header">
              <div className="sprint-lane-title">
                <h2><Layers size={20} /> {sprint.tenSprint}</h2>
                <span className={`sprint-badge ${statusStyle.class}`}>{statusStyle.text}</span>
                <span className="sprint-dates">
                  <Calendar size={14} />
                  {new Date(sprint.ngayBatDau).toLocaleDateString()} <ArrowRight size={12} /> {new Date(sprint.ngayKetThuc).toLocaleDateString()}
                </span>
              </div>
              <div className="sprint-lane-progress">
                <span>Tiến độ: <strong>{Math.round(sprint.tienDo || 0)}%</strong></span>
                <div className="progress-bar-bg-mini">
                  <div className="progress-bar-fill-mini" style={{ width: `${Math.round(sprint.tienDo || 0)}%` }}></div>
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
                            {/* Header: ID task và mức ưu tiên */}
                            <div className="k-task-header">
                              <span className="k-task-id">#{task.id}</span>
                              {task.doUuTien === 2 ? <span className="k-prio high">Cao</span> :
                                task.doUuTien === 1 ? <span className="k-prio medium">Vừa</span> :
                                  <span className="k-prio low">Thấp</span>}
                            </div>

                            {/* Tên task */}
                            <h5 className="k-task-title" title={task.tieuDe}>{task.tieuDe}</h5>

                            {/* Phần hiển thị danh sách task phụ thuộc (dependencies) */}
                            {task.dependencies && task.dependencies.length > 0 && (
                              <div className="k-dependencies">
                                <div className="k-dep-label">
                                  <GitMerge size={11} />
                                  <span>Phụ thuộc ({task.dependencies.length})</span>
                                </div>
                                <div className="k-dep-list">
                                  {task.dependencies.map(dep => (
                                    <span key={dep.dependsOnTaskId} className="k-dep-badge" title={dep.dependsOnTaskTitle}>
                                      #{dep.dependsOnTaskId}
                                      {dep.dependsOnTaskTitle && (
                                        <span className="k-dep-title"> – {dep.dependsOnTaskTitle}</span>
                                      )}
                                    </span>
                                  ))}
                                </div>
                              </div>
                            )}

                            {/* Footer: Assignee và thời gian ước tính */}
                            <div className="k-task-footer">
                              <div className="k-assignee">
                                <div className="k-avatar">
                                  {task.assigneeName ? task.assigneeName.charAt(0).toUpperCase() : '?'}
                                </div>
                                <span className="k-name">{task.assigneeName || 'Chưa giao'}</span>
                              </div>
                              <div className="k-time">
                                <Clock size={12} /> {task.thoiGianUocTinh}h
                              </div>
                            </div>

                            {/* Nhóm 3 nút hành động theo ngữ cảnh */}
                            <div className="k-task-actions">
                              {/* Nút Giao lại – luôn hiển thị */}
                              <button
                                className="k-action-btn assign"
                                title="Giao lại task cho người khác"
                                onClick={() => setAssignModal(task)}
                              >
                                <UserCheck size={12} /> Giao lại
                              </button>

                              {/* Nút Duyệt – chỉ hiện khi task đang ở trạng thái Review */}
                              {task.trangThai === TrangThaiCongViec.Review && (
                                <button
                                  className="k-action-btn approve"
                                  title="Duyệt – chuyển sang Hoàn thành"
                                  onClick={() => handleApproveTask(task)}
                                  disabled={approveLoading === task.id}
                                >
                                  <CheckCircle2 size={12} />
                                  {approveLoading === task.id ? 'Đang...' : 'Duyệt'}
                                </button>
                              )}

                              {/* Nút Chuyển Sprint – ẩn nếu chỉ có 1 sprint */}
                              {sprints.length > 1 && (
                                <button
                                  className="k-action-btn move-sprint"
                                  title="Chuyển task sang Sprint khác"
                                  onClick={() => setMoveSprintModal(task)}
                                >
                                  <ArrowRightLeft size={12} /> Sprint
                                </button>
                              )}
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

  /**
   * Render 2 modal hành động (Giao lại & Chuyển Sprint).
   * Chỉ hiển thị khi có task đang được chọn xử lý.
   */
  const renderActionModals = () => (
    <>
      {/* Modal Giao lại task */}
      {assignModal && (
        <AssignModal
          task={assignModal}
          members={members}
          onClose={() => setAssignModal(null)}
          onSuccess={refreshTasks}
        />
      )}

      {/* Modal Chuyển Sprint */}
      {moveSprintModal && (
        <MoveSprintModal
          task={moveSprintModal}
          sprints={sprints}
          onClose={() => setMoveSprintModal(null)}
          onSuccess={refreshTasks}
        />
      )}
    </>
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
            {userProfile?.hoTen?.charAt(0) || <UserIcon size={24} />}
          </div>
          <div className="welcome-text">
            <h1>Tổng quan điều hành dự án</h1>
            <p>Xin chào <strong>{userProfile?.hoTen || 'Quản lý'}</strong>. Hệ thống AI đã sẵn sàng điều hành.</p>
          </div>
        </div>

        <div className="project-selector-wrapper">
          <label><Briefcase size={16} /> Chọn Dự Án Điều Hành:</label>
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
        <button className="dash-tab active">
          <Layers size={18} />
          <span>Bảng điều hành Sprint</span>
        </button>
      </nav>

      {/* 3. Main Master Board Area */}
      {selectedProjectId ? (
        <div className="master-board-content">
          {boardLoading ? (
            <div className="board-loading">Đang cập nhật luồng dữ liệu thời gian thực...</div>
          ) : sprints.length > 0 ? (
            renderOperations()
          ) : (
            <div className="empty-master-board">
              <CheckSquare size={48} className="empty-icon" />
              <h3>Dự án này chưa có Sprint nào!</h3>
              <p>Vui lòng cập nhật tài liệu hoặc tạo Sprint mới để bắt đầu theo dõi tiến độ.</p>
            </div>
          )}
        </div>
      ) : (
        <div className="empty-master-board">Vui lòng chọn một dự án để bắt đầu điều hành.</div>
      )}

      {/* Render các modal hành động (Giao lại / Chuyển Sprint) */}
      {renderActionModals()}
    </div>
  );
};

export default DashboardPage;
