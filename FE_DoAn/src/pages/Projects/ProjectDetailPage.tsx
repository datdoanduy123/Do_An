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
  Sparkles,
  UserPlus,
  Trash2,
  BarChart3,
  Users as UsersIcon,
  CheckCircle,
  Loader2,
  Zap,
  Shield,
  Award,
  Activity,
  Play
} from 'lucide-react';
import { 
  Radar, 
  RadarChart, 
  PolarGrid, 
  PolarAngleAxis, 
  PolarRadiusAxis, 
  ResponsiveContainer,
  Legend,
  Tooltip
} from 'recharts';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto } from '../../services/ProjectService';
import UserService from '../../services/UserService';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import { TrangThaiSprint as TrangThaiEnum } from '../../services/SprintService';
import DocumentService from '../../services/DocumentService';
import type { TaiLieuDuAnDto } from '../../services/DocumentService';
import './ProjectDetail.css';

/**
 * Trang chi tiết dự án - Hiển thị danh sách Sprint.
 */
const ProjectDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<DuAnDto | null>(null);
  const [sprints, setSprints] = useState<SprintDto[]>([]);
  const [documents, setDocuments] = useState<TaiLieuDuAnDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [docLoading, setDocLoading] = useState(false);
  const [members, setMembers] = useState<any[]>([]);
  const [allAvailableUsers, setAllAvailableUsers] = useState<any[]>([]);
  const [skillCoverage, setSkillCoverage] = useState<any[]>([]);
  const [showAddMember, setShowAddMember] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [dashboardLoading, setDashboardLoading] = useState(false);
  const [showCreateSprintModal, setShowCreateSprintModal] = useState(false);
  const [newSprintData, setNewSprintData] = useState({
    tenSprint: '',
    mucTieuStoryPoints: 0,
    ngayBatDau: '',
    ngayKetThuc: ''
  });
  const [isCreatingSprint, setIsCreatingSprint] = useState(false);

  const [activeSprintMenuId, setActiveSprintMenuId] = useState<number | null>(null);
  const [showEditSprintModal, setShowEditSprintModal] = useState(false);
  const [editSprintData, setEditSprintData] = useState<SprintDto | null>(null);
  const [isEditingSprint, setIsEditingSprint] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      try {
        setLoading(true);
        setDashboardLoading(true);
        const [projData, sprintsData, docsData, membersData, usersData, coverageData] = await Promise.all([
          ProjectService.getProjectById(Number(id)),
          SprintService.getByProjectId(Number(id)),
          DocumentService.getByProject(Number(id)),
          ProjectService.getMembers(Number(id)),
          UserService.getUsers({ pageSize: 100 }),
          ProjectService.getSkillCoverage(Number(id))
        ]);
        setProject(projData);
        setSprints(sprintsData || []);
        setDocuments(docsData || []);
        setMembers(membersData || []);
        setAllAvailableUsers(usersData.items || []);
        setSkillCoverage(coverageData || []);
      } catch (error) {
        console.error('Error fetching project details:', error);
      } finally {
        setLoading(false);
        setDashboardLoading(false);
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

  const handleAddMember = async () => {
    if (!selectedUserId || !id) return;
    try {
      setDocLoading(true);
      await ProjectService.addMember(Number(id), Number(selectedUserId));
      const [updatedMembers, updatedCoverage] = await Promise.all([
        ProjectService.getMembers(Number(id)),
        ProjectService.getSkillCoverage(Number(id))
      ]);
      setMembers(updatedMembers);
      setSkillCoverage(updatedCoverage);
      setShowAddMember(false);
      setSelectedUserId('');
    } catch (error) {
      console.error('Add member failed:', error);
      alert('Thêm thành viên thất bại.');
    } finally {
      setDocLoading(false);
    }
  };

  const handleRemoveMember = async (userId: number) => {
    if (!id || !window.confirm('Bạn có chắc chắn muốn xóa thành viên này khỏi dự án?')) return;
    try {
      setDocLoading(true);
      await ProjectService.removeMember(Number(id), userId);
      const [updatedMembers, updatedCoverage] = await Promise.all([
        ProjectService.getMembers(Number(id)),
        ProjectService.getSkillCoverage(Number(id))
      ]);
      setMembers(updatedMembers);
      setSkillCoverage(updatedCoverage);
    } catch (error) {
      console.error('Remove member failed:', error);
      alert('Xóa thành viên thất bại.');
    } finally {
      setDocLoading(false);
    }
  };

  const handleAutoAssign = async () => {
    if (!id || !window.confirm('AI sẽ tự động phân bổ toàn bộ công việc chưa có người nhận cho các thành viên trong team. Bạn có chắc chắn?')) return;
    try {
      setDocLoading(true);
      await ProjectService.autoAssignProject(Number(id));
      // Tải lại dữ liệu dự án và sprint
      const [updatedProj, updatedSprints] = await Promise.all([
        ProjectService.getProjectById(Number(id)),
        SprintService.getByProjectId(Number(id))
      ]);
      setProject(updatedProj);
      setSprints(updatedSprints);
      alert('AI đã hoàn thành việc giao việc tự động!');
    } catch (error) {
      console.error('Auto assign failed:', error);
      alert('Giao việc tự động thất bại.');
    } finally {
      setDocLoading(false);
    }
  };

  const handleCreateSprint = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !newSprintData.tenSprint) return;
    
    try {
      setIsCreatingSprint(true);
      await SprintService.create({
        duAnId: Number(id),
        tenSprint: newSprintData.tenSprint,
        mucTieuStoryPoints: newSprintData.mucTieuStoryPoints || 0,
        ngayBatDau: newSprintData.ngayBatDau || new Date().toISOString(),
        ngayKetThuc: newSprintData.ngayKetThuc || new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString()
      });
      
      const updatedSprints = await SprintService.getByProjectId(Number(id));
      setSprints(updatedSprints);
      
      setShowCreateSprintModal(false);
      setNewSprintData({ tenSprint: '', mucTieuStoryPoints: 0, ngayBatDau: '', ngayKetThuc: '' });
    } catch (error) {
      console.error('Create sprint failed:', error);
      alert('Tạo Sprint thất bại.');
    } finally {
      setIsCreatingSprint(false);
    }
  };

  const handleDeleteSprint = async (sprintId: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa Sprint này không? Các công việc (nếu có) có thể bị ảnh hưởng.')) return;
    try {
      await SprintService.delete(sprintId);
      setSprints(sprints.filter(s => s.id !== sprintId));
    } catch (error) {
      console.error('Delete sprint failed:', error);
      alert('Xóa Sprint thất bại.');
    }
  };

  if (loading || dashboardLoading) return <div className="loading-state">Đang tải thông tin dự án...</div>;
  if (!project) return <div className="error-state">Không tìm thấy dự án.</div>;

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="custom-chart-tooltip">
          <p className="label">{payload[0].payload.Skill}</p>
          <p className="val required">Yêu cầu: {payload[0].payload.Required}</p>
          <p className="val available">Hiện có: {payload[0].payload.Available}</p>
          <p className="val percent">Đáp ứng: {Math.round(payload[0].payload.CoveragePercent)}%</p>
        </div>
      );
    }
    return null;
  };

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
            <span className="stat-value">{Math.round(project.tienDo || 0)}%</span>
          </div>
          <button 
            className="auto-assign-btn-premium" 
            onClick={handleAutoAssign}
            disabled={docLoading}
          >
            <Zap size={18} fill={docLoading ? "#cbd5e1" : "#fbbf24"} color="#fbbf24" strokeWidth={0} />
            <span>AI Auto-Assign Team</span>
          </button>
        </div>
      </div>

      {/* Sprints Section */}
      <div className="sprints-section">
        <div className="section-header">
          <div className="section-title">
            <TrendingUp size={22} className="title-icon" />
            <h2>Danh sách Sprints</h2>
          </div>
          <button className="add-sprint-btn" onClick={() => setShowCreateSprintModal(true)}>
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
                    <span>{Math.round(sprint.tienDo || 0)}%</span>
                  </div>
                  <div className="progress-bar-bg">
                    <div className="progress-bar-fill" style={{ width: `${Math.round(sprint.tienDo || 0)}%` }} />
                  </div>
                </div>

                <div className="sprint-actions">
                  <button className="view-sprint-btn">
                    <span>Chi tiết</span>
                    <ChevronRight size={16} />
                  </button>
                  <div className="sprint-menu-wrapper" style={{ position: 'relative' }}>
                    <button 
                      className="icon-btn"
                      onClick={(e) => {
                        e.stopPropagation();
                        setActiveSprintMenuId(activeSprintMenuId === sprint.id ? null : sprint.id);
                      }}
                    >
                      <MoreVertical size={18} />
                    </button>
                    {activeSprintMenuId === sprint.id && (
                      <div className="sprint-dropdown-menu fade-in">
                        <button className="menu-item" onClick={(e) => { e.stopPropagation(); setShowEditSprintModal(true); setEditSprintData(sprint); setActiveSprintMenuId(null); }}>
                           <span>Chỉnh sửa Sprint</span>
                        </button>
                        <button className="menu-item danger" onClick={(e) => { e.stopPropagation(); handleDeleteSprint(sprint.id); setActiveSprintMenuId(null); }}>
                           <span>Xóa Sprint</span>
                        </button>
                      </div>
                    )}
                  </div>
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

      {/* Team Skill Insights Dashboard */}
      <div className="dashboard-section">
        <div className="section-header">
          <div className="section-title">
            <BarChart3 size={22} className="title-icon chart-icon" />
            <h2>Phân tích Năng lực Team</h2>
          </div>
          <div className="dashboard-badges">
            <span className="badge-ai">Phân tích bởi AI</span>
          </div>
        </div>

        <div className="dashboard-content">
          <div className="chart-container-radar">
            {skillCoverage.length >= 3 ? (
              <ResponsiveContainer width="100%" height={350}>
                <RadarChart cx="50%" cy="50%" outerRadius="80%" data={skillCoverage}>
                  <PolarGrid stroke="#e2e8f0" />
                  <PolarAngleAxis dataKey="Skill" tick={{ fill: '#64748b', fontSize: 12 }} />
                  <PolarRadiusAxis angle={30} domain={[0, 'auto']} tick={{ fill: '#94a3b8', fontSize: 10 }} />
                  <Radar
                    name="Hiện có"
                    dataKey="Available"
                    stroke="#6366f1"
                    fill="#6366f1"
                    fillOpacity={0.6}
                  />
                  <Radar
                    name="Yêu cầu"
                    dataKey="Required"
                    stroke="#fbbf24"
                    fill="#fbbf24"
                    fillOpacity={0.3}
                  />
                  <Tooltip content={<CustomTooltip />} />
                  <Legend />
                </RadarChart>
              </ResponsiveContainer>
            ) : (
              <div className="chart-placeholder">
                <p>Cần ít nhất 3 kỹ năng để hiển thị biểu đồ Radar.</p>
                <div className="mini-bars">
                  {skillCoverage.map(s => (
                    <div key={s.Skill} className="mini-bar-item">
                      <span className="s-name">{s.Skill}</span>
                      <div className="s-track">
                        <div className="s-fill" style={{ width: `${s.CoveragePercent}%`, background: s.CoveragePercent >= 100 ? '#10b981' : '#6366f1' }} />
                      </div>
                      <span className="s-val">{Math.round(s.CoveragePercent)}%</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          <div className="dashboard-summary">
            <h3>Chi tiết Đáp ứng Kỹ năng</h3>
            <div className="skill-stats-list">
              {skillCoverage.map((s) => (
                <div key={s.Skill} className="skill-stat-card">
                  <div className="skill-head">
                    <span className="skill-name">{s.Skill}</span>
                    <span className={`skill-percent ${s.CoveragePercent >= 100 ? 'good' : 'warning'}`}>
                      {Math.round(s.CoveragePercent)}%
                    </span>
                  </div>
                  <div className="skill-bar-bg">
                    <div 
                      className="skill-bar-fill" 
                      style={{ 
                        width: `${s.CoveragePercent}%`,
                        background: s.CoveragePercent >= 100 ? '#10b981' : 'linear-gradient(90deg, #6366f1, #fbbf24)'
                      }} 
                    />
                  </div>
                  <div className="skill-counts">
                    <span>Yêu cầu: <strong>{s.Required}</strong></span>
                    <span>Team hiện có: <strong>{s.Available}</strong></span>
                  </div>
                </div>
              ))}
              {skillCoverage.length === 0 && (
                <div className="empty-dashboard-text">
                  Chưa có dữ liệu phân tích kỹ năng. Vui lòng gán Task có yêu cầu kỹ năng và thêm thành viên.
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
      
      {/* AI Project Analyst Section */}
      <div className="ai-analyst-section">
        <div className="section-header">
          <div className="section-title">
            <Sparkles size={22} className="title-icon ai-icon" />
            <h2>Trợ lý AI bóc tách tài liệu</h2>
          </div>
          <div className="upload-controls">
            <input 
              type="file" 
              id="doc-upload" 
              hidden 
              accept=".docx,.pdf"
              onChange={handleFileUpload}
            />
            <label htmlFor="doc-upload" className="upload-label-btn">
              {docLoading ? <Loader2 size={18} className="animate-spin" /> : <Upload size={18} />}
              <span>Tải tài liệu Word (.docx)</span>
            </label>
          </div>
        </div>

        <div className="ai-content-card">
          {documents.length > 0 ? (
            <div className="document-list">
              {documents.map((doc) => (
                <div key={doc.id} className="document-item">
                  <div className="doc-info">
                    <FileText size={20} className="doc-icon" />
                    <div className="doc-details">
                      <span className="doc-name">{doc.fileName}</span>
                      <span className="doc-meta">
                        {new Date(doc.uploadAt).toLocaleString('vi-VN')} • {doc.fileType}
                      </span>
                    </div>
                  </div>
                  <div className="doc-actions">
                    {doc.isProcessed ? (
                      <span className="processed-tag">
                        <CheckCircle size={14} />
                        Đã xử lý
                      </span>
                    ) : (
                      <button 
                        className="process-ai-btn"
                        onClick={() => handleProcessAI(doc.id)}
                        disabled={docLoading}
                      >
                        <Zap size={16} />
                        <span>AI Bóc tách Task</span>
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="ai-empty-state">
              <div className="ai-illustration">
                <Sparkles size={40} />
              </div>
              <p>Chưa có tài liệu dự án nào được tải lên.</p>
              <span className="ai-hint">Tải lên file Word (.docx) chứa bảng mô tả công việc để AI tự động bóc tách.</span>
            </div>
          )}
        </div>
      </div>
 Broadway
      {/* Project Members Section */}
      <div className="members-section">
        <div className="section-header">
          <div className="section-title">
            <UsersIcon size={22} className="title-icon members-icon" />
            <h2>Thành viên Dự án</h2>
          </div>
          <button 
            className="add-member-btn"
            onClick={() => setShowAddMember(!showAddMember)}
          >
            <UserPlus size={18} />
            <span>Thêm thành viên</span>
          </button>
        </div>

        {showAddMember && (
          <div className="add-member-overlay">
            <select 
              className="user-select" 
              value={selectedUserId} 
              onChange={(e) => setSelectedUserId(e.target.value)}
            >
              <option value="">Chọn nhân viên...</option>
              {allAvailableUsers
                .filter(u => !members.some(m => m.nguoiDungId === u.id))
                .map(u => (
                  <option key={u.id} value={u.id}>{u.hoTen} ({u.tenDangNhap})</option>
                ))
              }
            </select>
            <button className="confirm-add-btn" onClick={handleAddMember} disabled={docLoading}>
              {docLoading ? '...' : 'Xác nhận'}
            </button>
            <button className="cancel-add-btn" onClick={() => setShowAddMember(false)}>Hủy</button>
          </div>
        )}

        <div className="member-grid">
          {members.map((member) => (
            <div key={member.id} className="member-card">
              <div className="member-avatar">
                {member.hoTen.charAt(0)}
              </div>
              <div className="member-info">
                <div className="member-name-row">
                  <h4>{member.hoTen}</h4>
                  {member.vaiTro === 'PM' && <Shield size={14} className="pm-icon" fill="#6366f1" color="#6366f1" />}
                </div>
                <p className="member-mail">{member.email}</p>
                
                <div className="member-skills-mini">
                  {member.kyNang?.map((sk: string, idx: number) => (
                    <span key={idx} className="mini-skill-tag">{sk}</span>
                  ))}
                  {(!member.kyNang || member.kyNang.length === 0) && (
                    <span className="no-skill-text">Chưa có kỹ năng</span>
                  )}
                </div>

                <div className="member-stats-bottom">
                  <div className="member-stat-item">
                    <Activity size={12} />
                    <span>{member.soCongViec} Tasks</span>
                  </div>
                  <div className="member-role-tag">{member.vaiTro || 'Member'}</div>
                </div>
              </div>
              <button 
                className="remove-member-btn"
                onClick={() => handleRemoveMember(member.id)}
              >
                <Trash2 size={16} />
              </button>
            </div>
          ))}
          {members.length === 0 && (
            <div className="empty-members">Chưa có thành viên nào tham gia dự án.</div>
          )}
        </div>
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

      {showCreateSprintModal && (
        <div className="sprint-modal-overlay" onClick={() => setShowCreateSprintModal(false)}>
          <div className="sprint-modal-content" onClick={e => e.stopPropagation()}>
            <div className="sprint-modal-header">
              <h2>Tạo Sprint Mới</h2>
              <button className="close-btn" onClick={() => setShowCreateSprintModal(false)}>✕</button>
            </div>
            <form onSubmit={handleCreateSprint} className="sprint-modal-body">
              <div className="form-group">
                <label>Tên Sprint <span className="required">*</span></label>
                <input 
                  type="text" 
                  value={newSprintData.tenSprint}
                  onChange={e => setNewSprintData({...newSprintData, tenSprint: e.target.value})}
                  placeholder="VD: Sprint 1"
                  required
                />
              </div>
              <div className="form-group">
                <label>Mục tiêu Story Points</label>
                <input 
                  type="number" 
                  min={0}
                  value={newSprintData.mucTieuStoryPoints}
                  onChange={e => setNewSprintData({...newSprintData, mucTieuStoryPoints: parseInt(e.target.value) || 0})}
                  placeholder="VD: 40"
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Ngày bắt đầu</label>
                  <input 
                    type="date" 
                    value={newSprintData.ngayBatDau}
                    onChange={e => setNewSprintData({...newSprintData, ngayBatDau: e.target.value})}
                  />
                </div>
                <div className="form-group">
                  <label>Ngày kết thúc</label>
                  <input 
                    type="date" 
                    value={newSprintData.ngayKetThuc}
                    onChange={e => setNewSprintData({...newSprintData, ngayKetThuc: e.target.value})}
                  />
                </div>
              </div>
              <div className="sprint-modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowCreateSprintModal(false)}>
                  Hủy
                </button>
                <button type="submit" className="btn-submit" disabled={isCreatingSprint || !newSprintData.tenSprint}>
                  {isCreatingSprint ? <Loader2 className="animate-spin" size={16} /> : 'Tạo Sprint'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDetailPage;
