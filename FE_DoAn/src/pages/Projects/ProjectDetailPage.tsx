import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  ArrowLeft, 
  Calendar, 
  Target, 
  Clock, 
  MoreVertical, 
  Plus,
  ChevronRight,
  TrendingUp
} from 'lucide-react';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto } from '../../services/ProjectService';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import { TrangThaiSprint as TrangThaiEnum } from '../../services/SprintService';
import './ProjectDetail.css';

/**
 * Trang chi tiết dự án - Hiển thị danh sách Sprint.
 */
const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<DuAnDto | null>(null);
  const [sprints, setSprints] = useState<SprintDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      try {
        setLoading(true);
        const [projData, sprintsData] = await Promise.all([
          ProjectService.getProjectById(Number(id)),
          SprintService.getByProjectId(Number(id))
        ]);
        setProject(projData);
        setSprints(sprintsData || []);
      } catch (error) {
        console.error('Error fetching project details:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [id]);

  const getSprintStatusLabel = (status: number) => {
    switch (status) {
      case TrangThaiEnum.New: return { text: 'Mới', class: 'status-new' };
      case TrangThaiEnum.InProgress: return { text: 'Đang thực hiện', class: 'status-inprogress' };
      case TrangThaiEnum.Finished: return { text: 'Đã kết thúc', class: 'status-finished' };
      default: return { text: 'Không xác định', class: '' };
    }
  };

  if (loading) return <div className="loading-state">Đang tải thông tin dự án...</div>;
  if (!project) return <div className="error-state">Không tìm thấy dự án.</div>;

  return (
    <div className="project-detail-container">
      {/* Breadcrumb & Header */}
      <div className="detail-header">
        <button className="back-btn" onClick={() => navigate('/projects')}>
          <ArrowLeft size={20} />
          <span>Quay lại</span>
        </button>
        
        <div className="project-title-section">
          <div className="title-wrapper">
            <h1>{project.tenDuAn}</h1>
            <span className="project-id">#PRJ-{project.id}</span>
          </div>
          <p className="project-desc">{project.moTa || 'Không có mô tả cho dự án này.'}</p>
        </div>

        <div className="header-stats">
          <div className="stat-box">
            <span className="stat-label">Tổng Sprint</span>
            <span className="stat-value">{sprints.length}</span>
          </div>
          <div className="stat-box">
            <span className="stat-label">Tiến độ</span>
            <span className="stat-value">65%</span>
          </div>
        </div>
      </div>

      {/* Sprints Section */}
      <div className="sprints-section">
        <div className="section-header">
          <div className="section-title">
            <TrendingUp size={22} className="title-icon" />
            <h2>Danh sách Sprints</h2>
          </div>
          <button className="add-sprint-btn">
            <Plus size={18} />
            <span>Tạo Sprint mới</span>
          </button>
        </div>

        {sprints.length > 0 ? (
          <div className="sprint-list">
            {sprints.map((sprint) => (
              <div 
                key={sprint.id} 
                className="sprint-item-card clickable" 
                onClick={() => navigate(`/sprints/${sprint.id}`)}
              >
                <div className="sprint-info">
                  <div className="sprint-name-row">
                    <h3>{sprint.tenSprint}</h3>
                    <span className={`sprint-status ${getSprintStatusLabel(sprint.trangThai).class}`}>
                      {getSprintStatusLabel(sprint.trangThai).text}
                    </span>
                  </div>
                  <div className="sprint-meta">
                    <div className="meta-item">
                      <Calendar size={14} />
                      <span>{new Date(sprint.ngayBatDau).toLocaleDateString('vi-VN')} - {new Date(sprint.ngayKetThuc).toLocaleDateString('vi-VN')}</span>
                    </div>
                    <div className="meta-item">
                      <Target size={14} />
                      <span>{sprint.mucTieuStoryPoints} Story Points</span>
                    </div>
                  </div>
                </div>

                <div className="sprint-progress">
                  <div className="progress-text">
                    <span>Tiến độ công việc</span>
                    <span>70%</span>
                  </div>
                  <div className="progress-bar-bg">
                    <div className="progress-bar-fill" style={{ width: '70%' }} />
                  </div>
                </div>

                <div className="sprint-actions">
                  <button className="view-sprint-btn">
                    <span>Chi tiết</span>
                    <ChevronRight size={16} />
                  </button>
                  <button 
                    className="icon-btn"
                    onClick={(e) => {
                      e.stopPropagation();
                      // Logic more actions
                    }}
                  >
                    <MoreVertical size={18} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="empty-sprints">
            <Clock size={48} />
            <h3>Chưa có Sprint nào</h3>
            <p>Dự án này hiện chưa bắt đầu bất kỳ Sprint nào. Hãy tạo Sprint đầu tiên để bắt đầu công việc.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProjectDetailPage;
