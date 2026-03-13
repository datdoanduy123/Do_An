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
  AlertTriangle
} from 'lucide-react';
import PermissionService, { 
  type QuyenDto, 
  type PaginatedResult, 
  type NhomQuyenDto 
} from '../../services/PermissionService';
import './Permissions.css';

interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

/**
 * Trang Quản lý Quyền (Permissions).
 */
const PermissionsPage: React.FC = () => {
  const [data, setData] = useState<PaginatedResult<QuyenDto> | null>(null);
  const [groups, setGroups] = useState<NhomQuyenDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [keyword, setKeyword] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // States for Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [selectedItem, setSelectedItem] = useState<Partial<QuyenDto>>({});
  const [submitting, setSubmitting] = useState(false);

  // States for Custom Confirm
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [confirmData, setConfirmData] = useState<{id: number, name: string} | null>(null);

  // States for Toasts
  const [toasts, setToasts] = useState<Toast[]>([]);

  useEffect(() => {
    fetchData();
    fetchGroups();
  }, [pageIndex, pageSize]);

  const fetchData = async (kw = keyword) => {
    setLoading(true);
    try {
      const result = await PermissionService.getPermissions({
        pageIndex,
        pageSize,
        keyword: kw
      });
      setData(result);
    } catch (error) {
      showToast('Lỗi khi tải danh sách quyền', 'error');
    } finally {
      setLoading(false);
    }
  };

  const fetchGroups = async () => {
    try {
      const result = await PermissionService.getGroups({ pageSize: 100 });
      setGroups(result.items);
    } catch (error) {
      console.error('Error fetching groups:', error);
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

  const openModal = (mode: 'create' | 'edit' | 'view', item: Partial<QuyenDto> = {}) => {
    setModalMode(mode);
    setSelectedItem(item);
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
        const res = await PermissionService.createPermission(selectedItem);
        if (res.statusCode === 200) {
          showToast('Thêm mới quyền thành công!');
        } else {
          showToast(res.message || 'Lỗi khi tạo quyền', 'error');
        }
      } else {
        const res = await PermissionService.updatePermission(selectedItem.id!, selectedItem);
        if (res.statusCode === 200) {
          showToast('Cập nhật quyền thành công!');
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

  const askDelete = (item: QuyenDto) => {
    setConfirmData({ id: item.id, name: item.tenQuyen });
    setIsConfirmOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!confirmData) return;
    try {
      const success = await PermissionService.deletePermission(confirmData.id);
      if (success) {
        showToast('Đã xóa quyền thành công!');
        fetchData();
      } else {
        showToast('Không thể xóa quyền này.', 'error');
      }
    } catch (error) {
      showToast('Lỗi kết nối khi xóa quyền.', 'error');
    } finally {
      setIsConfirmOpen(false);
      setConfirmData(null);
    }
  };

  return (
    <div className="permissions-container">
      {/* Header section with search and add button */}
      <div className="permissions-header">
        <div className="search-controls">
          <form className="search-input-wrapper" onSubmit={handleSearch}>
            <Search size={18} className="search-icon" />
            <input
              type="text"
              placeholder="Tìm kiếm theo mã, tên quyền..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </form>
        </div>

        <div className="header-actions">
          <button className="btn-add" onClick={() => openModal('create')}>
            <Plus size={18} />
            <span>Thêm mới</span>
          </button>
        </div>
      </div>

      {/* Main data table */}
      <div className="table-card">
        {loading ? (
          <div style={{ padding: '40px', textAlign: 'center' }}>Đang tải dữ liệu...</div>
        ) : (
          <table className="custom-table">
            <thead>
              <tr>
                <th style={{ width: '60px' }}>STT</th>
                <th>Mã quyền</th>
                <th>Tên quyền</th>
                <th>Mô tả</th>
                <th>Nhóm quyền</th>
                <th style={{ textAlign: 'right', width: '150px' }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map((item, idx) => (
                  <tr key={item.id}>
                    <td>{(pageIndex - 1) * pageSize + idx + 1}</td>
                    <td style={{ fontWeight: 600 }}>{item.maQuyen}</td>
                    <td>{item.tenQuyen}</td>
                    <td>{item.moTa || <span style={{ color: '#cbd5e1' }}>Không có mô tả</span>}</td>
                    <td>
                      <span className="group-badge">{item.tenNhomQuyen}</span>
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
                  <td colSpan={6} style={{ textAlign: 'center', padding: '48px' }}>
                    Không tìm thấy dữ liệu phù hợp.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Pagination controls */}
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
            <select value={pageSize} onChange={(e) => setPageSize(Number(e.target.value))}>
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
        </div>
      )}

      {/* Modal for Create/Edit/View */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3>
                {modalMode === 'create' ? 'Thêm mới Quyền' : 
                 modalMode === 'edit' ? 'Chỉnh sửa Quyền' : 'Chi tiết Quyền'}
              </h3>
              <button className="close-btn" onClick={closeModal}><X size={20} /></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                <div className="form-group">
                  <label>Tên quyền</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.tenQuyen || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, tenQuyen: e.target.value})}
                    placeholder="Nhập tên quyền..."
                  />
                </div>
                <div className="form-group">
                  <label>Mã quyền</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.maQuyen || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, maQuyen: e.target.value})}
                    placeholder="Ví dụ: USER_CREATE"
                  />
                </div>
                <div className="form-group">
                  <label>Nhóm quyền</label>
                  <select 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.nhomQuyenId || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, nhomQuyenId: Number(e.target.value)})}
                  >
                    <option value="">-- Chọn nhóm quyền --</option>
                    {groups.map(g => (
                      <option key={g.id} value={g.id}>{g.tenNhom}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Mô tả</label>
                  <textarea 
                    rows={3} 
                    disabled={modalMode === 'view'}
                    value={selectedItem.moTa || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, moTa: e.target.value})}
                    placeholder="Ghi chú thêm về quyền này..."
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

      {/* Custom Confirmation Modal */}
      {isConfirmOpen && (
        <div className="modal-overlay">
          <div className="modal-content" style={{maxWidth: '400px'}}>
            <div className="confirm-box">
              <div className="confirm-icon">
                <AlertTriangle size={32} />
              </div>
              <h3 className="confirm-title">Xác nhận xóa</h3>
              <p className="confirm-text">
                Bạn có chắc chắn muốn xóa quyền <b>{confirmData?.name}</b>? 
                Hành động này không thể hoàn tác.
              </p>
              <div className="confirm-footer">
                <button className="btn-secondary" onClick={() => setIsConfirmOpen(false)}>Hủy bỏ</button>
                <button className="btn-danger" onClick={handleConfirmDelete}>Xác nhận xóa</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Toast Notifications */}
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

export default PermissionsPage;
