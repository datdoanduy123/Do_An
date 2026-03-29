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
  Key,
  ShieldAlert
} from 'lucide-react';
import RoleService, { 
  type VaiTroDto
} from '../../services/RoleService';
import type { PaginatedResult } from '../../services/PermissionTypes';
import PermissionService, { type QuyenDto } from '../../services/PermissionService';
import './Roles.css';

interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

/**
 * Trang Quản lý Vai trò (Roles).
 */
const RolesPage: React.FC = () => {
  const [data, setData] = useState<PaginatedResult<VaiTroDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [keyword, setKeyword] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // States for Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [selectedItem, setSelectedItem] = useState<Partial<VaiTroDto>>({});
  const [submitting, setSubmitting] = useState(false);

  // States for Custom Confirm
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [confirmData, setConfirmData] = useState<{id: number, name: string} | null>(null);

  // States for Toasts
  const [toasts, setToasts] = useState<Toast[]>([]);

  // States for Assign Permissions Modal
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [assigningRole, setAssigningRole] = useState<VaiTroDto | null>(null);
  const [allPermissions, setAllPermissions] = useState<QuyenDto[]>([]);
  const [rolePermissions, setRolePermissions] = useState<number[]>([]);
  const [isLoadingPermissions, setIsLoadingPermissions] = useState(false);

  useEffect(() => {
    fetchData();
  }, [pageIndex, pageSize]);

  const fetchData = async (kw = keyword) => {
    setLoading(true);
    try {
      const result = await RoleService.getRoles({
        pageIndex,
        pageSize,
        keyword: kw
      });
      setData(result);
    } catch (error) {
      showToast('Lỗi khi tải danh sách vai trò', 'error');
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

  const openModal = (mode: 'create' | 'edit' | 'view', item: Partial<VaiTroDto> = {}) => {
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
        const res = await RoleService.createRole(selectedItem as any);
        if (res.statusCode === 200) {
          showToast('Thêm mới vai trò thành công!');
        } else {
          showToast(res.message || 'Lỗi khi tạo vai trò', 'error');
        }
      } else {
        const res = await RoleService.updateRole(selectedItem.id!, selectedItem as any);
        if (res.statusCode === 200) {
          showToast('Cập nhật vai trò thành công!');
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

  const askDelete = (item: VaiTroDto) => {
    setConfirmData({ id: item.id, name: item.tenVaiTro });
    setIsConfirmOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!confirmData) return;
    try {
      const success = await RoleService.deleteRole(confirmData.id);
      if (success) {
        showToast('Đã xóa vai trò thành công!');
        fetchData();
      } else {
        showToast('Không thể xóa vai trò này.', 'error');
      }
    } catch (error) {
      showToast('Lỗi kết nối khi xóa vai trò.', 'error');
    } finally {
      setIsConfirmOpen(false);
      setConfirmData(null);
    }
  };

  const openAssignModal = async (role: VaiTroDto) => {
    setAssigningRole(role);
    setIsAssignModalOpen(true);
    setIsLoadingPermissions(true);
    
    try {
      const [allPermsResponse, rolePerms] = await Promise.all([
        PermissionService.getPermissions({ pageSize: 1000 }),
        RoleService.getPermissionsByRole(role.id)
      ]);
      setAllPermissions(allPermsResponse.items || []);
      setRolePermissions((rolePerms || []).map((p: any) => p.id));
    } catch (error) {
      showToast('Lỗi tải danh sách quyền', 'error');
    } finally {
      setIsLoadingPermissions(false);
    }
  };

  const closeAssignModal = () => {
    setIsAssignModalOpen(false);
    setAssigningRole(null);
  };

  const togglePermission = async (quyenId: number, isChecked: boolean) => {
    if (!assigningRole?.id) return;
    
    if (isChecked) {
      setRolePermissions(prev => [...prev, quyenId]);
    } else {
      setRolePermissions(prev => prev.filter(id => id !== quyenId));
    }

    try {
      if (isChecked) {
        await RoleService.assignPermission(assigningRole.id, quyenId);
      } else {
        await RoleService.removePermission(assigningRole.id, quyenId);
      }
      showToast('Đã cập nhật quyền thành công!');
    } catch (error: any) {
      if (isChecked) {
        setRolePermissions(prev => prev.filter(id => id !== quyenId));
      } else {
        setRolePermissions(prev => [...prev, quyenId]);
      }
      showToast(error?.response?.data?.message || 'Lỗi cập nhật quyền', 'error');
    }
  };

  const groupedPermissions: Record<string, QuyenDto[]> = {};
  allPermissions.forEach(p => {
    const groupName = p.tenNhomQuyen || 'Quyền Hệ Thống';
    if (!groupedPermissions[groupName]) {
      groupedPermissions[groupName] = [];
    }
    groupedPermissions[groupName].push(p);
  });

  return (
    <div className="roles-container">
      {/* Header section with search and add button */}
      <div className="roles-header">
        <div className="search-controls">
          <form className="search-input-wrapper" onSubmit={handleSearch}>
            <Search size={18} className="search-icon" />
            <input
              type="text"
              placeholder="Tìm kiếm theo mã, tên vai trò..."
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
                <th>Mã vai trò</th>
                <th>Tên vai trò</th>
                <th>Mô tả</th>
                <th style={{ textAlign: 'right', width: '150px' }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map((item, idx) => (
                  <tr key={item.id}>
                    <td>{(pageIndex - 1) * pageSize + idx + 1}</td>
                    <td style={{ fontWeight: 600 }}>
                       <span className="role-badge">{item.maVaiTro}</span>
                    </td>
                    <td>{item.tenVaiTro}</td>
                    <td>{item.moTa || <span style={{ color: '#cbd5e1' }}>Không có mô tả</span>}</td>
                    <td>
                      <div className="action-buttons">
                        <button className="action-btn assign" title="Phân quyền" onClick={() => openAssignModal(item)}>
                          <Key size={16} />
                        </button>
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
                  <td colSpan={5} style={{ textAlign: 'center', padding: '48px' }}>
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
                {modalMode === 'create' ? 'Thêm mới Vai trò' : 
                 modalMode === 'edit' ? 'Chỉnh sửa Vai trò' : 'Chi tiết Vai trò'}
              </h3>
              <button className="close-btn" onClick={closeModal}><X size={20} /></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                <div className="form-group">
                  <label>Tên vai trò</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.tenVaiTro || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, tenVaiTro: e.target.value})}
                    placeholder="Nhập tên vai trò..."
                  />
                </div>
                <div className="form-group">
                  <label>Mã vai trò</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view' || modalMode === 'edit'} // Thường mã không nên đổi sau khi tạo
                    value={selectedItem.maVaiTro || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, maVaiTro: e.target.value})}
                    placeholder="Ví dụ: ADMIN, MANAGER..."
                  />
                </div>
                <div className="form-group">
                  <label>Mô tả</label>
                  <textarea 
                    rows={4} 
                    disabled={modalMode === 'view'}
                    value={selectedItem.moTa || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, moTa: e.target.value})}
                    placeholder="Ghi chú thêm về vai trò này..."
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
                Bạn có chắc chắn muốn xóa vai trò <b>{confirmData?.name}</b>? 
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

      {/* Assign Permissions Modal */}
      {isAssignModalOpen && assigningRole && (
        <div className="modal-overlay">
          <div className="modal-content permissions-modal">
            <div className="modal-header">
              <div className="modal-title-wrap">
                <h3>Phân quyền: <span className="role-highlight">{assigningRole.tenVaiTro}</span></h3>
                {assigningRole.maVaiTro === 'QUAN_LY' && (
                  <div className="warning-badge">
                    <ShieldAlert size={14} /> 
                    <span>Cảnh báo: Vai trò Quản lý rất nhạy cảm</span>
                  </div>
                )}
              </div>
              <button className="close-btn" onClick={closeAssignModal}><X size={20} /></button>
            </div>
            
            <div className="modal-body permissions-body">
              {isLoadingPermissions ? (
                <div style={{ padding: '40px', textAlign: 'center' }}>Đang tải cấu trúc quyền...</div>
              ) : (
                <div className="permissions-grid">
                  {Object.keys(groupedPermissions).map(groupName => (
                    <div key={groupName} className="permission-group-card">
                      <h4 className="group-title">{groupName}</h4>
                      <div className="permission-list">
                        {groupedPermissions[groupName].map(p => {
                          const isChecked = rolePermissions.includes(p.id);
                          return (
                            <label key={p.id} className="permission-item">
                              <div className="switch-container">
                                <input 
                                  type="checkbox" 
                                  checked={isChecked}
                                  onChange={(e) => togglePermission(p.id, e.target.checked)}
                                />
                                <span className="slider round"></span>
                              </div>
                              <div className="permission-info">
                                <span className="perm-name">{p.tenQuyen}</span>
                                <span className="perm-code">{p.maQuyen}</span>
                              </div>
                            </label>
                          );
                        })}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
            <div className="modal-footer">
              <span style={{flex: 1, fontSize: '13px', color: '#64748b'}}>* Thay đổi sẽ được lưu tự động (Real-time).</span>
              <button type="button" className="btn-primary" onClick={closeAssignModal}>Hoàn tất</button>
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

export default RolesPage;
