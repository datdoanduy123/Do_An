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
  TrendingUp,
  FileText,
  Upload,
  Play,
  CheckCircle,
  Loader2,
  Sparkles
} from 'lucide-react';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto } from '../../services/ProjectService';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import { TrangThaiSprint as TrangThaiEnum } from '../../services/SprintService';
import DocumentService from '../../services/DocumentService';
import type { DocumentDto } from '../../services/DocumentService';
import './ProjectDetail.css';

/**
 * Trang chi tiết dự án - Hiển thị danh sách Sprint.
 */
const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<DuAnDto | null>(null);
  const [sprints, setSprints] = useState<SprintDto[]>([]);
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [docLoading, setDocLoading] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      try {
        setLoading(true);
        const [projData, sprintsData, docsData] = await Promise.all([
          ProjectService.getProjectById(Number(id)),
          SprintService.getByProjectId(Number(id)),
          DocumentService.getByProject(Number(id))
        ]);
        setProject(projData);
        setSprints(sprintsData || []);
        setDocuments(docsData || []);
      } catch (error) {
        console.error('Error fetching project details:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [id]);

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file || !id) return;

    try {
      setDocLoading(true);
      await DocumentService.upload(Number(id), file);
      const updatedDocs = await DocumentService.getByProject(Number(id));
      setDocuments(updatedDocs);
    } catch (error) {
      console.error('Upload failed:', error);
      alert('Tải lên tài liệu thất bại.');
    } finally {
      setDocLoading(false);
    }
  };

  const handleProcessAI = async (docId: number) => {
    if (!window.confirm('AI sẽ phân tích tài liệu để tự động bóc tách công việc và gán nhân sự. Quá trình này có thể mất vài giây. Tiếp tục?')) return;

    try {
      setDocLoading(true);
      const success = await DocumentService.processAI(docId);
      if (success) {
        alert('✨ Chúc mừng! Trợ lý AI đã bóc tách và phân bổ công việc thành công.');
        // Refresh everything to show new tasks and sprints
        const [sprintsData, docsData] = await Promise.all([
          SprintService.getByProjectId(Number(id)),
          DocumentService.getByProject(Number(id))
        ]);
        setSprints(sprintsData || []);
        setDocuments(docsData || []);
      }
    } catch (error) {
      console.error('AI processing failed:', error);
      alert('Có lỗi xảy ra khi AI đang phân tích. Vui lòng kiểm tra lại định dạng tài liệu.');
    } finally {
      setDocLoading(false);
    }
  };

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

      {/* AI & Smart Automation Section */}
      <div className="documents-section">
        <div className="section-header">
          <div className="section-title">
            <Sparkles size={22} className="ai-icon-title" />
            <h2>Trợ lý AI & Đặc tả Dự án</h2>
          </div>
          <div className="upload-wrapper">
            <input
              type="file"
              id="doc-upload"
              hidden
              onChange={handleFileUpload}
              accept=".doc,.docx"
              disabled={docLoading}
            />
            <label htmlFor="doc-upload" className={`upload-btn-premium ${docLoading ? 'loading' : ''}`}>
              {docLoading ? <Loader2 className="spin" size={18} /> : <Upload size={18} />}
              <span>{docLoading ? 'Đang tải lên...' : 'Tải lên đặc tả (.docx)'}</span>
            </label>
          </div>
        </div>

        <div className="ai-content-box">
          <div className="doc-grid">
            {documents.map((doc) => (
              <div key={doc.id} className="doc-card-premium">
                <div className="doc-type-icon">
                  <FileText size={32} />
                  <span className="file-ext">DOCX</span>
                </div>
                <div className="doc-main-info">
                  <h3>{doc.fileName}</h3>
                  <p className="doc-meta-text">
                    Đã tải lên: {new Date(doc.uploadAt).toLocaleDateString('vi-VN')}
                  </p>
                  <div className={`status-pill ${doc.isProcessed ? 'processed' : 'pending'}`}>
                    {doc.isProcessed ? (
                      <><CheckCircle size={14} /> <span>Đã hoàn thành bóc tách</span></>
                    ) : (
                      <><Clock size={14} /> <span>Chờ kích hoạt AI</span></>
                    )}
                  </div>
                </div>
                <div className="doc-card-actions">
                  {!doc.isProcessed ? (
                    <button
                      className="btn-activate-ai"
                      onClick={() => handleProcessAI(doc.id)}
                      disabled={docLoading}
                    >
                      <Play size={16} fill="currentColor" />
                      <span>Kích hoạt AI</span>
                    </button>
                  ) : (
                    <button className="btn-view-report">
                      <TrendingUp size={16} />
                      <span>Xem kết quả</span>
                    </button>
                  )}
                </div>
              </div>
            ))}

            {documents.length === 0 && !docLoading && (
              <div className="empty-ai-state">
                <div className="ai-pulse-icon">
                  <Sparkles size={40} />
                </div>
                <h3>Dự án chưa có bản đặc tả</h3>
                <p>Hãy tải lên file Word chứa mô tả nghiệp vụ để AI giúp bạn bóc tách công việc và tự động lập kế hoạch Sprint.</p>
                <div className="ai-features-list">
                  <div className="feature-item">✓ Tự động nhận diện Task</div>
                  <div className="feature-item">✓ Phân loại Sprint thông minh</div>
                  <div className="feature-item">✓ Đề xuất nhân sự phù hợp</div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProjectDetailPage;
