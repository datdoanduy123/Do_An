import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Search,
  Calendar,
  MoreVertical,
  Filter,
  LayoutGrid,
  List as ListIcon,
  Briefcase,
  X,
  Edit,
  Trash2
} from 'lucide-react';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto, TaoDuAnDto } from '../../services/ProjectService';
import { TrangThaiDuAn as TrangThaiEnum } from '../../services/ProjectService';
import UserService from '../../services/UserService';
import './Projects.css';

/**
 * Trang quản lý danh sách dự án.
 */
const ProjectsPage: React.FC = () => {
  const navigate = useNavigate();
  const [projects, setProjects] = useState<DuAnDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');


  // State cho Modal thêm dự án
  const [hasCreatePermission, setHasCreatePermission] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState<TaoDuAnDto>({
    tenDuAn: '',
    moTa: '',
    ngayBatDau: new Date().toISOString().split('T')[0],
    ngayKetThuc: ''
  });

  // State cho Menu dropdown và Edit
  const [activeMenuId, setActiveMenuId] = useState<number | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingProjectId, setEditingProjectId] = useState<number | null>(null);

  const fetchProjects = async () => {
    try {
      setLoading(true);
      const data = await ProjectService.getProjects();
      setProjects(data || []);
    } catch (error) {
      console.error('Error fetching projects:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchUser = async () => {
    try {
      const profile = await UserService.getProfile();
      setHasCreatePermission(profile.quyens?.includes('PROJECT_CREATE') || profile.vaiTros?.includes('QUAN_LY') || false);
    } catch (error) {
      console.error('Error fetching user:', error);
    }
  };

  useEffect(() => {
    fetchProjects();
    fetchUser();

    // Đóng dropdown khi click ra ngoài
    const handleClickOutside = (e: MouseEvent) => {
      if (!(e.target as Element).closest('.more-menu-container')) {
        setActiveMenuId(null);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const getStatusLabel = (project: DuAnDto) => {
    if (project.trangThai === TrangThaiEnum.Completed) return { text: 'Hoàn thành', class: 'status-completed' };
    if (project.trangThai === TrangThaiEnum.Cancelled) return { text: 'Đã hủy', class: 'status-cancelled' };
    if (project.trangThai === TrangThaiEnum.Active || project.tienDo! > 0) return { text: 'Đang thực hiện', class: 'status-inprogress' };
    return { text: 'Mới', class: 'status-new' };
  };

  const filteredProjects = projects.filter(p =>
    p.tenDuAn?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const resetForm = () => {
    setFormData({
      tenDuAn: '',
      moTa: '',
      ngayBatDau: new Date().toISOString().split('T')[0],
      ngayKetThuc: ''
    });
    setIsEditMode(false);
    setEditingProjectId(null);
  };

  const handleEditClick = (project: DuAnDto) => {
    setFormData({
      tenDuAn: project.tenDuAn,
      moTa: project.moTa || '',
      ngayBatDau: new Date(project.ngayBatDau).toISOString().split('T')[0],
      ngayKetThuc: project.ngayKetThuc ? new Date(project.ngayKetThuc).toISOString().split('T')[0] : ''
    });
    setIsEditMode(true);
    setEditingProjectId(project.id);
    setIsModalOpen(true);
    setActiveMenuId(null);
  };

  const handleDeleteProject = async (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa dự án này? Thao tác này không thể hoàn tác.')) return;
    try {
      const success = await ProjectService.deleteProject(id);
      if (success) {
        alert('Xóa dự án thành công!');
        fetchProjects();
      } else {
        alert('Xóa dự án thất bại.');
      }
    } catch (error) {
      console.error('Error deleting project:', error);
      alert('Lỗi hệ thống khi xóa dự án.');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      let response;
      
      if (isEditMode && editingProjectId) {
        // Cập nhật dự án
        response = await ProjectService.updateProject(editingProjectId, {
          ...formData,
          trangThai: projects.find(p => p.id === editingProjectId)?.trangThai || TrangThaiEnum.Planning 
        });
      } else {
        // Tạo dự án mới
        response = await ProjectService.createProject(formData);
      }

      if (response.statusCode === 200) {
        alert(isEditMode ? 'Cập nhật dự án thành công!' : 'Tạo dự án thành công!');
        setIsModalOpen(false);
        resetForm();
        fetchProjects();
      } else {
        alert('Có lỗi xảy ra: ' + response.message);
      }
    } catch (error: any) {
      console.error('Error submitting project:', error);
      alert('Lỗi hệ thống khi lưu dự án.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="projects-container">
      {/* Header Section */}
      <div className="page-header">
        <div className="header-info">
          <h1>Danh sách Dự án</h1>
          <p>Quản lý và theo dõi tiến độ các dự án của bạn.</p>
        </div>
        {hasCreatePermission && (
          <div className="header-actions">
            <button className="primary-btn" onClick={() => setIsModalOpen(true)}>
              <Plus size={18} />
              <span>Thêm dự án mới</span>
            </button>
          </div>
        )}
      </div>

      {/* Toolbar Section */}
      <div className="projects-toolbar">
        <div className="search-box">
          <Search size={18} />
          <input
            type="text"
            placeholder="Tìm kiếm dự án..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <div className="toolbar-actions">
          <button className="icon-btn-outline">
            <Filter size={18} />
            <span>Lọc</span>
          </button>

          <div className="view-toggle">
            <button
              className={`toggle-btn ${viewMode === 'grid' ? 'active' : ''}`}
              onClick={() => setViewMode('grid')}
            >
              <LayoutGrid size={18} />
            </button>
            <button
              className={`toggle-btn ${viewMode === 'list' ? 'active' : ''}`}
              onClick={() => setViewMode('list')}
            >
              <ListIcon size={18} />
            </button>
          </div>
        </div>
      </div>

      {/* Projects Content */}
      {loading ? (
        <div className="loading-state">Đang tải danh sách dự án...</div>
      ) : filteredProjects.length > 0 ? (
        <div className={`projects-content ${viewMode}-view`}>
          {filteredProjects.map((project) => (
            <div
              key={project.id}
              className="project-card clickable"
              onClick={() => navigate(`/projects/${project.id}`)}
            >
              <div className="card-top">
                <div className="project-icon-box">
                  <Briefcase size={20} />
                </div>
                <div className="more-menu-container">
                  <button
                    className="more-btn"
                    onClick={(e) => {
                      e.stopPropagation();
                      setActiveMenuId(activeMenuId === project.id ? null : project.id);
                    }}
                  >
                    <MoreVertical size={18} />
                  </button>

                  {activeMenuId === project.id && (
                    <div className="project-dropdown-menu">
                      <button 
                        className="dropdown-item" 
                        onClick={(e) => { e.stopPropagation(); handleEditClick(project); }}
                      >
                        <Edit size={16} />
                        <span>Sửa dự án</span>
                      </button>
                      <div className="dropdown-divider"></div>
                      <button 
                        className="dropdown-item delete" 
                        onClick={(e) => { e.stopPropagation(); handleDeleteProject(project.id); }}
                      >
                        <Trash2 size={16} />
                        <span>Xóa dự án</span>
                      </button>
                    </div>
                  )}
                </div>
              </div>

              <div className="card-body">
                <h3 className="project-title">{project.tenDuAn}</h3>
                <p className="project-desc">{project.moTa || 'Không có mô tả cho dự án này.'}</p>

                <div className="project-meta">
                  <div className="meta-item">
                    <Calendar size={14} />
                    <span>{new Date(project.ngayBatDau).toLocaleDateString('vi-VN')}</span>
                  </div>
                  <span className={`status-tag ${getStatusLabel(project).class}`}>
                    {getStatusLabel(project).text}
                  </span>
                </div>
              </div>

              <div className="card-footer">
                <div className="progress-section">
                  <div className="progress-info">
                    <span>Tiến độ</span>
                    <span>{Math.round(project.tienDo || 0)}%</span>
                  </div>
                  <div className="progress-bar-bg">
                    <div
                      className="progress-bar-fill"
                      style={{ width: `${Math.round(project.tienDo || 0)}%` }}
                    />
                  </div>
 Broadway                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="empty-state">
          <Briefcase size={48} />
          <h3>Chưa có dự án nào</h3>
          <p>Bắt đầu bằng cách tạo dự án đầu tiên của bạn.</p>
        </div>
      )}

      {/* Modal Thêm dự án mới */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h2>{isEditMode ? 'Chỉnh sửa dự án' : 'Tạo dự án mới'}</h2>
              <button className="close-btn" onClick={() => { setIsModalOpen(false); resetForm(); }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="modal-form">
              <div className="form-group">
                <label>Tên dự án *</label>
                <input
                  type="text"
                  name="tenDuAn"
                  value={formData.tenDuAn}
                  onChange={handleInputChange}
                  placeholder="Nhập tên dự án..."
                  required
                />
              </div>

              <div className="form-group">
                <label>Mô tả</label>
                <textarea
                  name="moTa"
                  value={formData.moTa}
                  onChange={handleInputChange}
                  placeholder="Mô tả ngắn gọn về dự án..."
                  rows={3}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Ngày bắt đầu *</label>
                  <input
                    type="date"
                    name="ngayBatDau"
                    value={formData.ngayBatDau}
                    onChange={handleInputChange}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Ngày kết thúc (dự kiến)</label>
                  <input
                    type="date"
                    name="ngayKetThuc"
                    value={formData.ngayKetThuc}
                    onChange={handleInputChange}
                  />
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="secondary-btn"
                  onClick={() => { setIsModalOpen(false); resetForm(); }}
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="primary-btn"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Đang lưu...' : (isEditMode ? 'Cập nhật dự án' : 'Tạo dự án')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectsPage;
