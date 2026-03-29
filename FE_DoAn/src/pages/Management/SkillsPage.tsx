import React, { useState, useEffect } from 'react';
import {
  Search,
  Plus,
  Eye,
  Edit,
  Trash2,
  ChevronLeft,
  ChevronRight,
  MoreHorizontal,
  X,
  CheckCircle2,
  AlertCircle,
  Info,
  AlertTriangle,
  Zap
} from 'lucide-react';
import SkillService, { 
  type KyNangDto
} from '../../services/SkillService';
import type { PaginatedResult } from '../../services/PermissionTypes';
import './Skills.css';

interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

/**
 * Trang Quản lý Kỹ năng (Skills).
 */
const SkillsPage: React.FC = () => {
  const [data, setData] = useState<PaginatedResult<KyNangDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [keyword, setKeyword] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // States for Hierarchy
  const [nhoms, setNhoms] = useState<any[]>([]);
  const [allCongNghes, setAllCongNghes] = useState<any[]>([]);
  const [selectedNhomId, setSelectedNhomId] = useState<number | null>(null);

  // States for Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [selectedItem, setSelectedItem] = useState<Partial<KyNangDto>>({});
  const [submitting, setSubmitting] = useState(false);

  // Computed
  const filteredCongNghes = allCongNghes.filter(c => c.nhomKyNangId === selectedNhomId);

  // States for Custom Confirm
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [confirmData, setConfirmData] = useState<{id: number, name: string} | null>(null);

  // States for Toasts
  const [toasts, setToasts] = useState<Toast[]>([]);

  useEffect(() => {
    fetchData();
    fetchHierarchyData();
  }, [pageIndex, pageSize]);

  const fetchHierarchyData = async () => {
    try {
      const h = await SkillService.getHierarchy();
      setNhoms(h);
      // Flatten techs for easier filtering
      const techs: any[] = [];
      h.forEach(n => {
        if (n.congNghes) {
          n.congNghes.forEach(c => techs.push(c));
        }
      });
      setAllCongNghes(techs);
    } catch (error) {
      console.error('Failed to load hierarchy', error);
    }
  };

  const fetchData = async (kw = keyword) => {
    setLoading(true);
    try {
      const result = await SkillService.getSkills({
        pageIndex,
        pageSize,
        keyword: kw
      });
      setData(result);
    } catch (error) {
      showToast('Lỗi khi tải danh sách kỹ năng', 'error');
    } finally {
      setLoading(false);
    }
  };

  const showToast = (message: string, type: 'success' | 'error' | 'info' = 'success') => {
    const id = Date.now();
    setToasts(prev => [...prev, { id, message, type }]);
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 3000);
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    fetchData();
  };

  const openModal = (mode: 'create' | 'edit' | 'view', item: Partial<KyNangDto> = {}) => {
    setModalMode(mode);
    setSelectedItem(item);
    
    // Set parent group if editing
    if (item.congNgheId) {
      const tech = allCongNghes.find(c => c.id === item.congNgheId);
      if (tech) setSelectedNhomId(tech.nhomKyNangId);
    } else {
      setSelectedNhomId(null);
    }
    
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedItem({});
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (modalMode === 'view') return;

    setSubmitting(true);
    try {
      if (modalMode === 'create') {
        const res = await SkillService.createSkill(selectedItem as any);
        if (res.statusCode === 200) {
          showToast('Thêm mới kỹ năng thành công!');
        } else {
          showToast(res.message || 'Lỗi khi tạo kỹ năng', 'error');
        }
      } else {
        const res = await SkillService.updateSkill(selectedItem.id!, selectedItem as any);
        if (res.statusCode === 200) {
          showToast('Cập nhật kỹ năng thành công!');
        } else {
          showToast(res.message || 'Lỗi khi cập nhật', 'error');
        }
      }
      fetchData();
      closeModal();
    } catch (error) {
      showToast('Đã có lỗi xảy ra trong quá trình xử lý.', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const askDelete = (item: KyNangDto) => {
    setConfirmData({ id: item.id, name: item.tenKyNang });
    setIsConfirmOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!confirmData) return;
    try {
      const success = await SkillService.deleteSkill(confirmData.id);
      if (success) {
        showToast('Đã xóa kỹ năng thành công!');
        fetchData();
      } else {
        showToast('Không thể xóa kỹ năng này.', 'error');
      }
    } catch (error) {
      showToast('Lỗi kết nối khi xóa kỹ năng.', 'error');
    } finally {
      setIsConfirmOpen(false);
      setConfirmData(null);
    }
  };

  return (
    <div className="skills-container">
      <div className="skills-header">
        <div className="search-controls">
          <form className="search-input-wrapper" onSubmit={handleSearch}>
            <Search size={18} className="search-icon" />
            <input
              type="text"
              placeholder="Tìm kiếm tên kỹ năng..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </form>
        </div>

        <div className="header-actions">
          <button className="btn-add" onClick={() => openModal('create')}>
            <Plus size={18} />
            <span>Thêm kỹ năng</span>
          </button>
        </div>
      </div>

      <div className="table-card">
        {loading ? (
          <div style={{ padding: '40px', textAlign: 'center' }}>Đang tải dữ liệu...</div>
        ) : (
          <table className="custom-table">
            <thead>
              <tr>
                <th style={{ width: '60px' }}>STT</th>
                <th>Kỹ năng</th>
                <th>Lĩnh vực / Công nghệ</th>
                <th>Mô tả</th>
                <th style={{ textAlign: 'right', width: '150px' }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map((item, idx) => (
                  <tr key={item.id}>
                    <td>{(pageIndex - 1) * pageSize + idx + 1}</td>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                        <div style={{ backgroundColor: '#f1f5f9', padding: '8px', borderRadius: '8px', color: '#6366f1' }}>
                          <Zap size={16} />
                        </div>
                        <span style={{ fontWeight: 600 }}>{item.tenKyNang}</span>
                      </div>
                    </td>
                    <td>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                        <span className="badge-group">{item.tenNhomKyNang || 'Chưa phân nhóm'}</span>
                        <span className="badge-tech">{item.tenCongNghe || 'Chưa rõ công nghệ'}</span>
                      </div>
                    </td>
                    <td style={{ color: '#64748b', fontSize: '0.9rem' }}>
                      {item.moTa || <i style={{ color: '#cbd5e1' }}>Không có mô tả</i>}
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button className="action-btn view" title="Xem" onClick={() => openModal('view', item)}>
                          <Eye size={16} />
                        </button>
                        <button className="action-btn edit" title="Sửa" onClick={() => openModal('edit', item)}>
                          <Edit size={16} />
                        </button>
                        <button className="action-btn delete" title="Xóa" onClick={() => askDelete(item)}>
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={4} style={{ textAlign: 'center', padding: '48px' }}>
                    Không tìm thấy kỹ năng nào.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalCount > 0 && (
        <div className="pagination-container">
          <div className="pagination-info">
            Hiển thị <b>{((pageIndex - 1) * pageSize) + 1}</b> đến <b>{Math.min(pageIndex * pageSize, data.totalCount)}</b> trong tổng số <b>{data.totalCount}</b> bản ghi
          </div>

          <div className="pagination-controls">
            <button
              className="page-btn"
              disabled={!data.hasPreviousPage}
              onClick={() => setPageIndex(pageIndex - 1)}
            >
              <ChevronLeft size={16} />
            </button>

            {[...Array(data.totalPages)].map((_, i) => {
              const p = i + 1;
              if (p === 1 || p === data.totalPages || Math.abs(p - pageIndex) <= 1) {
                return (
                  <button
                    key={p}
                    className={`page-btn ${pageIndex === p ? 'active' : ''}`}
                    onClick={() => setPageIndex(p)}
                  >
                    {p}
                  </button>
                );
              } else if (Math.abs(p - pageIndex) === 2) {
                return <span key={p} style={{ padding: '0 4px' }}><MoreHorizontal size={14} /></span>;
              }
              return null;
            })}

            <button
              className="page-btn"
              disabled={!data.hasNextPage}
              onClick={() => setPageIndex(pageIndex + 1)}
            >
              <ChevronRight size={16} />
            </button>
          </div>

          <div className="page-size-selector">
            <span>Dòng trên trang:</span>
            <select value={pageSize} onChange={(e) => {
              setPageIndex(1);
              setPageSize(Number(e.target.value));
            }}>
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
        </div>
      )}

      {/* Skills Modal */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3>
                {modalMode === 'create' ? 'Thêm mới Kỹ năng' : 
                 modalMode === 'edit' ? 'Chỉnh sửa Kỹ năng' : 'Chi tiết Kỹ năng'}
              </h3>
              <button className="close-btn" onClick={closeModal}><X size={20} /></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                <div className="form-row">
                  <div className="form-group">
                    <label>Lĩnh vực (Cấp 1)</label>
                    <select 
                      className="form-control"
                      disabled={modalMode === 'view'}
                      value={selectedNhomId || ''}
                      onChange={(e) => {
                        const id = Number(e.target.value);
                        setSelectedNhomId(id);
                        // Reset tech when group changes
                        setSelectedItem({...selectedItem, congNgheId: 0});
                      }}
                    >
                      <option value="">-- Chọn lĩnh vực --</option>
                      {nhoms.map(n => <option key={n.id} value={n.id}>{n.tenNhom}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Công nghệ (Cấp 2)</label>
                    <select 
                      className="form-control"
                      disabled={modalMode === 'view' || !selectedNhomId}
                      value={selectedItem.congNgheId || ''}
                      onChange={(e) => setSelectedItem({...selectedItem, congNgheId: Number(e.target.value)})}
                    >
                      <option value="">-- Chọn công nghệ --</option>
                      {filteredCongNghes.map(c => <option key={c.id} value={c.id}>{c.tenCongNghe}</option>)}
                    </select>
                  </div>
                </div>

                <div className="form-group">
                  <label>Kỹ năng chi tiết (Cấp 3)</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.tenKyNang || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, tenKyNang: e.target.value})}
                    placeholder="Ví dụ: Entity Framework, React Hook..."
                  />
                </div>
                <div className="form-group">
                  <label>Mô tả kỹ năng</label>
                  <textarea 
                    rows={3}
                    disabled={modalMode === 'view'}
                    value={selectedItem.moTa || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, moTa: e.target.value})}
                    placeholder="Mô tả năng lực cần có..."
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn-secondary" onClick={closeModal}>Hủy</button>
                {modalMode !== 'view' && (
                  <button type="submit" className="btn-primary" disabled={submitting}>
                    {submitting ? 'Đang lưu...' : 'Lưu thông tin'}
                  </button>
                )}
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Confirmation Dialog */}
      {isConfirmOpen && (
        <div className="modal-overlay">
          <div className="modal-content" style={{maxWidth: '400px'}}>
            <div className="confirm-box">
              <div className="confirm-icon">
                <AlertTriangle size={32} />
              </div>
              <h3 className="confirm-title">Xác nhận xóa</h3>
              <p className="confirm-text">
                Bạn có chắc chắn muốn xóa kỹ năng <b>{confirmData?.name}</b>? 
              </p>
              <div className="confirm-footer">
                <button className="btn-secondary" onClick={() => setIsConfirmOpen(false)}>Hủy bỏ</button>
                <button className="btn-danger" onClick={handleConfirmDelete}>Xác nhận xóa</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Toasts */}
      <div className="toast-container">
        {toasts.map(toast => (
          <div key={toast.id} className={`toast ${toast.type}`}>
            <div className="toast-icon">
              {toast.type === 'success' && <CheckCircle2 size={20} color="#10b981" />}
              {toast.type === 'error' && <AlertCircle size={20} color="#ef4444" />}
              {toast.type === 'info' && <Info size={20} color="#3b82f6" />}
            </div>
            <div className="toast-content">
              <div className="toast-message">{toast.message}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default SkillsPage;
