import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  ArrowLeft, 
  Search, 
  Plus, 
  Filter,
  MoreHorizontal,
  Clock,
  Sparkles,
  AlertCircle,
  CheckCircle2,
  Calendar,
  XCircle,
  CheckCircle
} from 'lucide-react';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import TaskService from '../../services/TaskService';
import UserService from '../../services/UserService';
import type { CongViecDto } from '../../services/TaskService';
import { TrangThaiCongViec as StatusEnum } from '../../services/TaskService';
import GiaoViecAIService from '../../services/GiaoViecAIService';
import type { AIRecommendation } from '../../services/GiaoViecAIService';
import AIRecommendationModal from '../../components/AI/AIRecommendationModal';
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

  // AI State
  const [showAIModal, setShowAIModal] = useState(false);
  const [aiSelectedTask, setAiSelectedTask] = useState<CongViecDto | null>(null);
  const [aiRecommendations, setAiRecommendations] = useState<AIRecommendation[]>([]);
  const [aiLoading, setAiLoading] = useState(false);

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

  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      try {
        setLoading(true);
        const sprintData = await SprintService.getById(Number(id));
        setSprint(sprintData);
        
        // Lấy tất cả task của dự án và lọc theo sprintId
        if (sprintData) {
          const allTasks = await TaskService.getByProjectId(sprintData.duAnId);
          const sprintTasks = allTasks.filter(t => t.sprintId === Number(id));
          setTasks(sprintTasks);
        }
      } catch (error) {
        console.error('Error fetching sprint details:', error);
      } finally {
        setLoading(false);
      }
    };
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
    try {
      // 3 là Done, 1 là InProgress
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

  const handleAIRecommend = async (task: CongViecDto) => {
    setAiSelectedTask(task);
    setShowAIModal(true);
    setAiLoading(true);
    try {
      const recs = await GiaoViecAIService.getRecommendations(task.id);
      setAiRecommendations(recs);
    } catch (error) {
      console.error('AI Recommend failed:', error);
    } finally {
      setAiLoading(false);
    }
  };

  const handleApplyAI = async (userId: number) => {
    if (!aiSelectedTask) return;
    try {
      const success = await TaskService.assignTask({
        congViecId: aiSelectedTask.id,
        nguoiDuocGiaoId: userId
      });
      if (success) {
        setTasks(prev => prev.map(t =>
          t.id === aiSelectedTask.id ? { ...t, assigneeId: userId, assigneeName: aiRecommendations.find(r => r.userId === userId)?.hoTen || t.assigneeName } : t
        ));
        setShowAIModal(false);
      }
    } catch (error) {
      console.error('Apply AI failed:', error);
    }
  };

  if (loading) return <div className="loading-state">Đang tải chi tiết Sprint...</div>;
  if (!sprint) return <div className="error-state">Không tìm thấy Sprint.</div>;

  return (
    <div className="sprint-detail-container">
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
          <div className="search-tasks">
            <Search size={18} />
            <input type="text" placeholder="Tìm công việc..." />
          </div>
          <button className="filter-btn">
            <Filter size={18} />
            <span>Lọc</span>
          </button>
          <button className="create-task-btn">
            <Plus size={18} />
            <span>Tạo công việc</span>
          </button>
        </div>
      </div>

      {/* Kanban Board */}
      <div className="kanban-board">
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
                  {col.id === StatusEnum.Review && isAdmin && (
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
                    <div className="assignee">
                      {task.assigneeName ? (
                        <div className="avatar">
                          {task.assigneeName.charAt(0)}
                        </div>
                      ) : (
                        <div className="avatar empty" onClick={() => isAdmin && handleAIRecommend(task)} title="Gợi ý bằng AI">
                           <Sparkles size={14} color="#fbbf24" strokeWidth={3} />
                        </div>
                      )}
                      <span>{task.assigneeName || 'Chưa giao'}</span>
                      {!task.assigneeName && isAdmin && (
                        <button className="ai-mini-btn" onClick={() => handleAIRecommend(task)}>✨</button>
                      )}
                    </div>

                    <div className="story-points">
                      <span>{task.storyPoints} SP</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>

      {showAIModal && aiSelectedTask && (
        <AIRecommendationModal
          taskId={aiSelectedTask.id}
          taskTitle={aiSelectedTask.tieuDe}
          recommendations={aiRecommendations}
          loading={aiLoading}
          onClose={() => setShowAIModal(false)}
          onApply={handleApplyAI}
        />
      )}
    </div>
  );
};

export default SprintDetailPage;
