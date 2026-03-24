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
  X
} from 'lucide-react';
import ProjectService from '../../services/ProjectService';
import type { DuAnDto, TrangThaiDuAn, TaoDuAnDto } from '../../services/ProjectService';
import { TrangThaiDuAn as TrangThaiEnum } from '../../services/ProjectService';
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
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState<TaoDuAnDto>({
    tenDuAn: '',
    moTa: '',
    ngayBatDau: new Date().toISOString().split('T')[0],
    ngayKetThuc: ''
  });

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

  useEffect(() => {
    fetchProjects();
  }, []);

  const getStatusLabel = (status: TrangThaiDuAn) => {
    switch (status) {
      case TrangThaiEnum.Planning: return { text: 'Mới', class: 'status-new' };
      case TrangThaiEnum.Active: return { text: 'Đang thực hiện', class: 'status-inprogress' };
      case TrangThaiEnum.Completed: return { text: 'Hoàn thành', class: 'status-completed' };
      case TrangThaiEnum.Cancelled: return { text: 'Đã hủy', class: 'status-cancelled' };
      default: return { text: 'Không xác định', class: '' };
    }
  };

  const filteredProjects = projects.filter(p =>
    p.tenDuAn?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      const response = await ProjectService.createProject(formData);
      if (response.statusCode === 200) {
        alert('Tạo dự án thành công!');
        setIsModalOpen(false);
        setFormData({
          tenDuAn: '',
          moTa: '',
          ngayBatDau: new Date().toISOString().split('T')[0],
          ngayKetThuc: ''
        });
        fetchProjects();
      } else {
        alert('Có lỗi xảy ra: ' + response.message);
      }
    } catch (error: any) {
      console.error('Error creating project:', error);
      alert('Lỗi hệ thống khi tạo dự án.');
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
        <button className="primary-btn" onClick={() => setIsModalOpen(true)}>
          <Plus size={18} />
          <span>Thêm dự án mới</span>
        </button>
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
                <button
                  className="more-btn"
                  onClick={(e) => {
                    e.stopPropagation();
                    // Thêm logic more menu ở đây nếu cần
                  }}
                >
                  <MoreVertical size={18} />
                </button>
              </div>

              <div className="card-body">
                <h3 className="project-title">{project.tenDuAn}</h3>
                <p className="project-desc">{project.moTa || 'Không có mô tả cho dự án này.'}</p>

                <div className="project-meta">
                  <div className="meta-item">
                    <Calendar size={14} />
                    <span>{new Date(project.ngayBatDau).toLocaleDateString('vi-VN')}</span>
                  </div>
                  <span className={`status-tag ${getStatusLabel(project.trangThai).class}`}>
                    {getStatusLabel(project.trangThai).text}
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
              <h2>Tạo dự án mới</h2>
              <button className="close-btn" onClick={() => setIsModalOpen(false)}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCreateProject} className="modal-form">
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
                  onClick={() => setIsModalOpen(false)}
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="primary-btn"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Đang lưu...' : 'Tạo dự án'}
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
