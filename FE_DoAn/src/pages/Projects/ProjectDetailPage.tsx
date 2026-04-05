import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  ArrowLeft, 
  Calendar, 
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
  Users as UsersIcon,
  Layers,
  Loader2,
  Shield,
  Activity,
  Play,
  Search
} from 'lucide-react';

import ProjectService, { ProjectRole } from '../../services/ProjectService';
import type { DuAnDto } from '../../services/ProjectService';
import UserService from '../../services/UserService';
import SprintService from '../../services/SprintService';
import type { SprintDto } from '../../services/SprintService';
import { TrangThaiSprint as TrangThaiEnum } from '../../services/SprintService';
import DocumentService from '../../services/DocumentService';
import type { TaiLieuDuAnDto } from '../../services/DocumentService';
import Toast from '../../components/Common/Toast';
import ConfirmModal from '../../components/Common/ConfirmModal';
import { CheckCircle } from 'lucide-react';
import './ProjectDetail.css';

/**
 * Lấy nhãn hiển thị cho ProjectRole enum.
 */
const getRoleLabel = (role?: ProjectRole) => {
  switch (role) {
    case ProjectRole.PM: return 'PM';
    case ProjectRole.Developer: return 'Developer';
    case ProjectRole.Tester: return 'Tester';
    case ProjectRole.QA: return 'QA';
    case ProjectRole.BA: return 'BA';
    default: return 'Member';
  }
};

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
  const [showAddMember, setShowAddMember] = useState(false);
  const [selectedUserIds, setSelectedUserIds] = useState<number[]>([]);
  const [userSearchTerm, setUserSearchTerm] = useState<string>('');
  const [selectedRole, setSelectedRole] = useState<ProjectRole>(ProjectRole.Member);
  const [allAvailableUsers, setAllAvailableUsers] = useState<any[]>([]);
  const [currentUser, setCurrentUser] = useState<any>(null);

  const [dashboardLoading, setDashboardLoading] = useState(false);
  const [showCreateSprintModal, setShowCreateSprintModal] = useState(false);
  const [newSprintData, setNewSprintData] = useState({
    tenSprint: '',
    ngayBatDau: '',
    ngayKetThuc: ''
  });
  const [isCreatingSprint, setIsCreatingSprint] = useState(false);

  const [activeSprintMenuId, setActiveSprintMenuId] = useState<number | null>(null);
  const [showEditSprintModal, setShowEditSprintModal] = useState(false);
  const [editSprintData, setEditSprintData] = useState<SprintDto | null>(null);
  const [isEditingSprint, setIsEditingSprint] = useState(false);

  // Toast State
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
  };

  // Confirm Modal State
  const [confirmConfig, setConfirmConfig] = useState<{ 
    isOpen: boolean; 
    message: string; 
    onConfirm: () => void;
    title?: string;
    type?: 'danger' | 'warning' | 'info'
  }>({
    isOpen: false,
    message: '',
    onConfirm: () => {},
  });

  useEffect(() => {
    const fetchData = async () => {
      if (!id) return;
      try {
        setLoading(true);
        setDashboardLoading(true);
        const [projData, sprintsData, docsData, membersData, usersData, profileData] = await Promise.all([
          ProjectService.getProjectById(Number(id)),
          SprintService.getByProjectId(Number(id)),
          DocumentService.getByProject(Number(id)),
          ProjectService.getMembers(Number(id)),
          UserService.getUsers({ pageSize: 100 }),
          UserService.getProfile()
        ]);
        setProject(projData);
        setSprints(sprintsData || []);
        setDocuments(docsData || []);
        setMembers(membersData || []);
        setAllAvailableUsers(usersData.items || []);
        setCurrentUser(profileData);
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
      showToast('Tải lên tài liệu thất bại.', 'error');
    } finally {
      setDocLoading(false);
    }
  };

  const handleProcessAI = async (docId: number) => {
    setConfirmConfig({
      isOpen: true,
      title: 'Kích hoạt Trợ lý AI',
      message: 'AI sẽ phân tích tài liệu để tự động bóc tách công việc và gán nhân sự dựa trên kỹ năng. Quá trình này có thể mất vài giây. Tiếp tục?',
      type: 'info',
      onConfirm: async () => {
        try {
          setDocLoading(true);
          const success = await DocumentService.processAI(docId);
          if (success) {
            showToast('✨ Chúc mừng! Trợ lý AI đã bóc tách và phân bổ công việc thành công.');
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
          showToast('Có lỗi xảy ra khi AI đang phân tích. Vui lòng kiểm tra lại định dạng tài liệu.', 'error');
        } finally {
          setDocLoading(false);
        }
      }
    });
  };

  const getSprintStatusLabel = (sprint: SprintDto) => {
    if (sprint.tienDo === 100 && sprint.trangThai !== TrangThaiEnum.Finished) {
      return { text: 'Chờ đóng (Xong)', class: 'status-finished-pending' };
    }

    if (sprint.trangThai === TrangThaiEnum.Finished) return { text: 'Đã kết thúc', class: 'status-finished' };
    if (sprint.trangThai === TrangThaiEnum.InProgress || sprint.tienDo! > 0) return { text: 'Đang thực hiện', class: 'status-inprogress' };
    
    return { text: 'Mới', class: 'status-new' };
  };

  const handleAddMembers = async () => {
    if (selectedUserIds.length === 0 || !id) return;
    try {
      setDocLoading(true);
      
      // Gọi API thêm từng người một cách song song
      await Promise.all(
        selectedUserIds.map(uid => 
          ProjectService.addMember(Number(id), uid, selectedRole)
        )
      );

      showToast(`Đã thêm thành công ${selectedUserIds.length} thành viên vào dự án.`);
      
      const updatedMembers = await ProjectService.getMembers(Number(id));
      setMembers(updatedMembers);
      
      // Reset state
      setShowAddMember(false);
      setSelectedUserIds([]);
      setUserSearchTerm('');
      setSelectedRole(ProjectRole.Member);
    } catch (error) {
      console.error('Add members failed:', error);
      showToast('Có lỗi xảy ra khi thêm thành viên.', 'error');
    } finally {
      setDocLoading(false);
    }
  };

  const toggleUserSelection = (userId: number) => {
    setSelectedUserIds(prev => 
      prev.includes(userId) 
        ? prev.filter(id => id !== userId) 
        : [...prev, userId]
    );
  };

  const handleSelectAll = (users: any[]) => {
    if (selectedUserIds.length === users.length) {
      setSelectedUserIds([]);
    } else {
      setSelectedUserIds(users.map(u => u.id));
    }
  };

  const handleUpdateMemberRole = async (userId: number, newRole: ProjectRole) => {
    try {
      setDocLoading(true);
      await ProjectService.updateMemberRole(Number(id), userId, newRole);
      const updatedMembers = await ProjectService.getMembers(Number(id));
      setMembers(updatedMembers);
    } catch (error) {
      console.error('Update role failed:', error);
      showToast('Cập nhật chức danh thất bại.', 'error');
    } finally {
      setDocLoading(false);
    }
  };

  const handleRemoveMember = async (userId: number) => {
    setConfirmConfig({
      isOpen: true,
      title: 'Xóa thành viên',
      message: 'Bạn có chắc chắn muốn xóa thành viên này khỏi dự án? Thao tác này không thể hoàn tác.',
      type: 'danger',
      onConfirm: async () => {
        try {
          setDocLoading(true);
          await ProjectService.removeMember(Number(id), userId);
          const updatedMembers = await ProjectService.getMembers(Number(id));
          setMembers(updatedMembers);
          showToast('Đã xóa thành viên thành công.');
        } catch (error) {
          console.error('Remove member failed:', error);
          showToast('Xóa thành viên thất bại.', 'error');
        } finally {
          setDocLoading(false);
        }
      }
    });
  };



  const handleCreateSprint = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !newSprintData.tenSprint) return;
    
    try {
      setIsCreatingSprint(true);
      await SprintService.create({
        duAnId: Number(id),
        tenSprint: newSprintData.tenSprint,
        ngayBatDau: newSprintData.ngayBatDau || new Date().toISOString(),
        ngayKetThuc: newSprintData.ngayKetThuc || new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString()
      });
      
      const updatedSprints = await SprintService.getByProjectId(Number(id));
      setSprints(updatedSprints);
      
      setShowCreateSprintModal(false);
      setNewSprintData({ tenSprint: '', ngayBatDau: '', ngayKetThuc: '' });
    } catch (error) {
      console.error('Create sprint failed:', error);
      showToast('Tạo Sprint thất bại.', 'error');
    } finally {
      setIsCreatingSprint(false);
    }
  };

  const handleEditSprint = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editSprintData || !id) return;
    try {
      setIsEditingSprint(true);
      await SprintService.update(editSprintData.id, {
        tenSprint: editSprintData.tenSprint,
        ngayBatDau: editSprintData.ngayBatDau || new Date().toISOString(),
        ngayKetThuc: editSprintData.ngayKetThuc || new Date().toISOString(),
        trangThai: editSprintData.trangThai
      });
      const updatedSprints = await SprintService.getByProjectId(Number(id));
      setSprints(updatedSprints);
      setShowEditSprintModal(false);
    } catch (error) {
      console.error('Update sprint failed:', error);
      showToast('Cập nhật Sprint thất bại.', 'error');
    } finally {
      setIsEditingSprint(false);
    }
  };


  const handleDeleteSprint = async (sprintId: number) => {
    setConfirmConfig({
      isOpen: true,
      title: 'Xóa Sprint',
      message: 'Bạn có chắc chắn muốn xóa Sprint này không? Các công việc (nếu có) có thể bị ảnh hưởng.',
      type: 'danger',
      onConfirm: async () => {
        try {
          await SprintService.delete(sprintId);
          setSprints(sprints.filter(s => s.id !== sprintId));
          showToast('Đã xóa Sprint thành công.');
        } catch (error) {
          console.error('Delete sprint failed:', error);
          showToast('Xóa Sprint thất bại.', 'error');
        }
      }
    });
  };

  if (loading || dashboardLoading) return <div className="loading-state">Đang tải thông tin dự án...</div>;
  if (!project) return <div className="error-state">Không tìm thấy dự án.</div>;

  const isAdmin = currentUser?.vaiTros?.includes('QUAN_LY') || false;
  const canManageMembers = currentUser?.quyens?.includes('PROJECT_UPDATE') || isAdmin;
  const canManageSprints = currentUser?.quyens?.includes('SPRINT_CREATE') || currentUser?.quyens?.includes('SPRINT_UPDATE') || isAdmin;
  const canUploadDocs = currentUser?.quyens?.includes('DOC_CREATE') || isAdmin;
  const canProcessAI = currentUser?.quyens?.includes('TASK_CREATE') || isAdmin;



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
          <div className="stat-card">
            <div className="stat-icon-wrapper indigo">
              <Layers size={22} />
            </div>
            <div className="stat-details">
              <span className="stat-label">Tổng Sprint</span>
              <span className="stat-value">{sprints.length}</span>
            </div>
          </div>
          
          <div className="stat-card">
            <div className="stat-icon-wrapper emerald">
              <Activity size={22} />
            </div>
            <div className="stat-details">
              <span className="stat-label">Tiến độ dự án</span>
              <span className="stat-value">{Math.round(project.tienDo || 0)}%</span>
            </div>
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
          {canManageSprints && (
            <button className="add-sprint-btn" onClick={() => setShowCreateSprintModal(true)}>
              <Plus size={18} />
              <span>Tạo Sprint mới</span>
            </button>
          )}
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
                    <span className={`sprint-status ${getSprintStatusLabel(sprint).class}`}>
                      {getSprintStatusLabel(sprint).text}
                    </span>
                  </div>
                  <div className="sprint-meta">
                    <div className="meta-item">
                      <Calendar size={14} />
                      <span>{new Date(sprint.ngayBatDau).toLocaleDateString('vi-VN')} - {new Date(sprint.ngayKetThuc).toLocaleDateString('vi-VN')}</span>
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
                  {canManageSprints && (
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
                  )}
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



      {/* Project Members Section */}
      <div className="members-section">
        <div className="section-header">
          <div className="section-title">
            <UsersIcon size={22} className="title-icon members-icon" />
            <h2>Thành viên Dự án</h2>
          </div>
          {canManageMembers && (
            <button 
              className="add-member-btn"
              onClick={() => setShowAddMember(!showAddMember)}
            >
              <UserPlus size={18} />
              <span>Thêm thành viên</span>
            </button>
          )}
        </div>

        {showAddMember && (
          <div className="add-member-panel-modern fade-in">
            <div className="modern-panel-header">
              <div className="header-text">
                <h3>Thêm thành viên mới</h3>
                <p>Tìm kiếm và chọn các nhân sự phù hợp để đưa vào dự án</p>
              </div>
              <div className="search-box-modern">
                <Search size={18} />
                <input 
                  type="text" 
                  placeholder="Tìm theo tên hoặc email..." 
                  value={userSearchTerm}
                  onChange={(e) => setUserSearchTerm(e.target.value)}
                />
              </div>
            </div>

            <div className="user-selection-area">
              <div className="selection-controls">
                <label className="select-all-label">
                  <input 
                    type="checkbox" 
                    checked={
                      selectedUserIds.length > 0 && 
                      selectedUserIds.length === allAvailableUsers.filter(u => 
                        !members.some(m => m.id === u.id) &&
                        (u.hoTen?.toLowerCase().includes(userSearchTerm.toLowerCase()) || 
                         u.email?.toLowerCase().includes(userSearchTerm.toLowerCase()))
                      ).length
                    }
                    onChange={() => handleSelectAll(
                      allAvailableUsers.filter(u => 
                        !members.some(m => m.id === u.id) &&
                        (u.hoTen?.toLowerCase().includes(userSearchTerm.toLowerCase()) || 
                         u.email?.toLowerCase().includes(userSearchTerm.toLowerCase()))
                      )
                    )}
                  />
                  <span>Chọn tất cả kết quả</span>
                </label>
                <span className="selection-count">Đã chọn: <strong>{selectedUserIds.length}</strong></span>
              </div>

              <div className="available-users-list">
                {allAvailableUsers
                  .filter(u => 
                    !members.some(m => m.id === u.id) &&
                    (u.hoTen?.toLowerCase().includes(userSearchTerm.toLowerCase()) || 
                     u.email?.toLowerCase().includes(userSearchTerm.toLowerCase()))
                  )
                  .map(u => (
                  <div 
                    key={u.id} 
                    className={`user-selection-item ${selectedUserIds.includes(u.id) ? 'selected' : ''}`}
                    onClick={() => toggleUserSelection(u.id)}
                  >
                    <div className="checkbox-wrapper">
                      <input 
                        type="checkbox" 
                        readOnly 
                        checked={selectedUserIds.includes(u.id)} 
                      />
                    </div>
                    <div className="user-mini-avatar">
                      {u.hoTen?.charAt(0)}
                    </div>
                    <div className="user-main-info">
                      <span className="u-name">{u.hoTen}</span>
                      <span className="u-email">{u.email}</span>
                    </div>
                    <div className="user-skills-preview">
                      {u.kyNang?.slice(0, 2).map((s: string, i: number) => (
                        <span key={i} className="tiny-skill">{s}</span>
                      ))}
                    </div>
                  </div>
                ))}
                {allAvailableUsers.filter(u => !members.some(m => m.id === u.id)).length === 0 && (
                  <div className="empty-search-state">Tất cả nhân sự hệ thống đều đã tham gia dự án.</div>
                )}
              </div>
            </div>

            <div className="modern-panel-footer">
              <div className="role-batch-selector">
                <label>Vai trò chung:</label>
                <select
                  value={selectedRole}
                  onChange={(e) => setSelectedRole(Number(e.target.value) as ProjectRole)}
                >
                  <option value={ProjectRole.Member}>Member</option>
                  <option value={ProjectRole.Developer}>Developer</option>
                  <option value={ProjectRole.Tester}>Tester</option>
                  <option value={ProjectRole.QA}>QA</option>
                  <option value={ProjectRole.BA}>BA</option>
                </select>
              </div>
              <div className="footer-actions">
                <button className="btn-close-modern" onClick={() => setShowAddMember(false)}>Đóng</button>
                <button 
                  className="btn-add-batch" 
                  onClick={handleAddMembers}
                  disabled={selectedUserIds.length === 0 || docLoading}
                >
                  {docLoading ? 'Đang xử lý...' : `Thêm ${selectedUserIds.length} thành viên`}
                </button>
              </div>
            </div>
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
                  {canManageMembers ? (
                    <select
                      className="member-role-tag editable"
                      value={member.vaiTro ?? ProjectRole.Member}
                      onChange={(e) => handleUpdateMemberRole(member.id, Number(e.target.value) as ProjectRole)}
                    >
                      <option value={ProjectRole.Member}>Member</option>
                      <option value={ProjectRole.Developer}>Developer</option>
                      <option value={ProjectRole.Tester}>Tester</option>
                      <option value={ProjectRole.QA}>QA</option>
                      <option value={ProjectRole.PM}>PM</option>
                      <option value={ProjectRole.BA}>BA</option>
                    </select>
                  ) : (
                    <div className="member-role-tag">{getRoleLabel(member.vaiTro)}</div>
                  )}
                </div>
              </div>
              {canManageMembers && (
                <button 
                  className="remove-member-btn"
                  onClick={() => handleRemoveMember(member.id)}
                >
                  <Trash2 size={16} />
                </button>
              )}
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
          {canUploadDocs && (
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
          )}
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
                  {canProcessAI && !doc.isProcessed ? (
                    <button
                      className="btn-activate-ai"
                      onClick={() => handleProcessAI(doc.id)}
                      disabled={docLoading}
                    >
                      <Play size={16} fill="currentColor" />
                      <span>Kích hoạt AI</span>
                    </button>
                  ) : doc.isProcessed ? (
                    <button className="btn-view-report">
                      <TrendingUp size={16} />
                      <span>Xem kết quả</span>
                    </button>
                  ) : null}
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

      {/* Edit Sprint Modal */}
      {showEditSprintModal && editSprintData && (
        <div className="sprint-modal-overlay" onClick={() => setShowEditSprintModal(false)}>
          <div className="sprint-modal-content" onClick={e => e.stopPropagation()}>
            <div className="sprint-modal-header">
              <h2>Chỉnh sửa Sprint</h2>
              <button className="close-btn" onClick={() => setShowEditSprintModal(false)}>✕</button>
            </div>
            <form onSubmit={handleEditSprint} className="sprint-modal-body">
              <div className="form-group">
                <label>Tên Sprint <span className="required">*</span></label>
                <input 
                  type="text" 
                  value={editSprintData.tenSprint}
                  onChange={e => setEditSprintData({...editSprintData, tenSprint: e.target.value})}
                  required
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Ngày bắt đầu</label>
                  <input 
                    type="date" 
                    value={editSprintData.ngayBatDau ? editSprintData.ngayBatDau.split('T')[0] : ''}
                    onChange={e => setEditSprintData({...editSprintData, ngayBatDau: e.target.value})}
                  />
                </div>
                <div className="form-group">
                  <label>Ngày kết thúc</label>
                  <input 
                    type="date" 
                    value={editSprintData.ngayKetThuc ? editSprintData.ngayKetThuc.split('T')[0] : ''}
                    onChange={e => setEditSprintData({...editSprintData, ngayKetThuc: e.target.value})}
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Trạng thái</label>
                <select 
                  value={editSprintData.trangThai}
                  onChange={e => setEditSprintData({...editSprintData, trangThai: parseInt(e.target.value) as any})}
                  style={{ padding: '12px 16px', borderRadius: '12px', border: '1px solid #e2e8f0', outline: 'none' }}
                >
                  <option value={0}>Mới</option>
                  <option value={1}>Đang thực hiện</option>
                  <option value={2}>Đã kết thúc</option>
                </select>
              </div>
              <div className="sprint-modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowEditSprintModal(false)}>
                  Hủy
                </button>
                <button type="submit" className="btn-submit" disabled={isEditingSprint || !editSprintData.tenSprint}>
                  {isEditingSprint ? <Loader2 className="animate-spin" size={16} /> : 'Lưu Thay Đổi'}
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

export default ProjectDetailPage;
