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
  User,
  ShieldCheck,
  ShieldAlert,
  Zap,
  Star,
  Award,
  History
} from 'lucide-react';
import UserService, { 
  type NguoiDungDto
} from '../../services/UserService';
import RoleService, { type VaiTroDto } from '../../services/RoleService';
import SkillService, { type KyNangDto } from '../../services/SkillService';
import type { PaginatedResult } from '../../services/PermissionTypes';
import './Users.css';

import type { UserSkillDto } from '../../services/UserTypes';

interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

/**
 * Trang Quản lý Người dùng (Users).
 */
const UsersPage: React.FC = () => {
  const [data, setData] = useState<PaginatedResult<NguoiDungDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [keyword, setKeyword] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // States for Modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit' | 'view'>('create');
  const [selectedItem, setSelectedItem] = useState<Partial<NguoiDungDto>>({});
  const [formPassword, setFormPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);

  // States for Role Assignment Modal
  const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);
  const [allRoles, setAllRoles] = useState<VaiTroDto[]>([]);
  const [userRoles, setUserRoles] = useState<string[]>([]);
  const [targetUser, setTargetUser] = useState<NguoiDungDto | null>(null);
  const [roleMatrixLoading, setRoleMatrixLoading] = useState(false);

  // States for Skill Assignment Modal
  const [isSkillModalOpen, setIsSkillModalOpen] = useState(false);
  const [allSkills, setAllSkills] = useState<KyNangDto[]>([]);
  const [userSkills, setUserSkills] = useState<UserSkillDto[]>([]);
  const [skillLoading, setSkillLoading] = useState(false);
  const [editingSkill, setEditingSkill] = useState<{
    kyNangId: number;
    level: number;
    soNamKinhNghiem: number;
  } | null>(null);

  // States for Custom Confirm
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [confirmData, setConfirmData] = useState<{id: number, name: string} | null>(null);

  // States for Toasts
  const [toasts, setToasts] = useState<Toast[]>([]);

  useEffect(() => {
    fetchData();
  }, [pageIndex, pageSize]);

  const fetchData = async (kw = keyword) => {
    setLoading(true);
    try {
      const result = await UserService.getUsers({
        pageIndex,
        pageSize,
        keyword: kw
      });
      setData(result);
    } catch (error) {
      showToast('Lỗi khi tải danh sách người dùng', 'error');
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

  const openModal = (mode: 'create' | 'edit' | 'view', item: Partial<NguoiDungDto> = {}) => {
    setModalMode(mode);
    setSelectedItem(item);
    setFormPassword('');
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedItem({});
    setFormPassword('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (modalMode === 'view') return;

    setSubmitting(true);
    try {
      if (modalMode === 'create') {
        const payload = { ...selectedItem, matKhau: formPassword };
        const res = await UserService.createUser(payload as any);
        if (res.statusCode === 200) {
          showToast('Thêm mới người dùng thành công!');
        } else {
          showToast(res.message || 'Lỗi khi tạo người dùng', 'error');
        }
      } else {
        const res = await UserService.updateUser(selectedItem.id!, selectedItem as any);
        if (res.statusCode === 200) {
          showToast('Cập nhật thông tin thành công!');
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

  const askDelete = (item: NguoiDungDto) => {
    setConfirmData({ id: item.id, name: item.hoTen });
    setIsConfirmOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!confirmData) return;
    try {
      const success = await UserService.deleteUser(confirmData.id);
      if (success) {
        showToast('Đã xóa người dùng thành công!');
        fetchData();
      } else {
        showToast('Không thể xóa người dùng này.', 'error');
      }
    } catch (error) {
      showToast('Lỗi kết nối khi xóa người dùng.', 'error');
    } finally {
      setIsConfirmOpen(false);
      setConfirmData(null);
    }
  };

  // Role Assignment Logic
  const openRoleModal = async (user: NguoiDungDto) => {
    setTargetUser(user);
    setUserRoles(user.vaiTros || []);
    setIsRoleModalOpen(true);
    setRoleMatrixLoading(true);
    try {
      // Get all roles for the checklist
      const res = await RoleService.getRoles({ pageSize: 100 });
      setAllRoles(res.items);
    } catch (error: any) {
      showToast('Không thể tải danh sách vai trò', 'error');
    } finally {
      setRoleMatrixLoading(false);
    }
  };

  const handleToggleRole = async (role: VaiTroDto) => {
    if (!targetUser) return;
    
    const hasRole = userRoles.includes(role.tenVaiTro);
    try {
      if (hasRole) {
        await UserService.removeRole(targetUser.id, role.id);
        setUserRoles(prev => prev.filter(r => r !== role.tenVaiTro));
        showToast(`Đã gỡ vai trò ${role.tenVaiTro}`);
      } else {
        await UserService.assignRole(targetUser.id, role.id);
        setUserRoles(prev => [...prev, role.tenVaiTro]);
        showToast(`Đã gán vai trò ${role.tenVaiTro}`);
      }
      // Refresh user data to update badges in the table
      fetchData();
    } catch (error: any) {
      showToast(error.response?.data?.message || 'Thao tác thất bại', 'error');
    }
  };

  // Skill Assignment Logic
  const openSkillModal = async (user: NguoiDungDto) => {
    setTargetUser(user);
    setUserSkills(user.kyNangs || []);
    setIsSkillModalOpen(true);
    setSkillLoading(true);
    try {
      const res = await SkillService.getSkills({ pageSize: 100 });
      setAllSkills(res.items);
    } catch (error) {
      showToast('Không thể tải danh sách kỹ năng', 'error');
    } finally {
      setSkillLoading(false);
    }
  };

  const handleUpdateSkill = async (kyNangId: number, level: number, years: number) => {
    if (!targetUser) return;
    
    try {
      await UserService.assignSkill({
        nguoiDungId: targetUser.id,
        kyNangId: kyNangId,
        level: level,
        soNamKinhNghiem: years
      });
      
      // Update local state
      const updatedUser = await UserService.getUserById(targetUser.id);
      setUserSkills(updatedUser.kyNangs || []);
      fetchData();
      showToast('Cập nhật kỹ năng thành công');
      setEditingSkill(null);
    } catch (error: any) {
      showToast(error.response?.data?.message || 'Thao tác thất bại', 'error');
    }
  };

  const handleRemoveSkill = async (kyNangId: number) => {
    if (!targetUser) return;
    
    try {
      await UserService.removeSkill({
        nguoiDungId: targetUser.id,
        kyNangId: kyNangId
      });
      
      setUserSkills(prev => prev.filter(s => s.kyNangId !== kyNangId));
      fetchData();
      showToast('Đã gỡ kỹ năng');
    } catch (error: any) {
      showToast(error.response?.data?.message || 'Thao tác thất bại', 'error');
    }
  };

  return (
    <div className="users-container">
      <div className="users-header">
        <div className="search-controls">
          <form className="search-input-wrapper" onSubmit={handleSearch}>
            <Search size={18} className="search-icon" />
            <input
              type="text"
              placeholder="Tìm kiếm tên, email, tài khoản..."
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

      <div className="table-card">
        {loading ? (
          <div style={{ padding: '40px', textAlign: 'center' }}>Đang tải dữ liệu...</div>
        ) : (
          <table className="custom-table">
            <thead>
              <tr>
                <th style={{ width: '60px' }}>STT</th>
                <th>Người dùng</th>
                <th>Tài khoản</th>
                <th>Điện thoại</th>
                <th>Vai trò</th>
                <th style={{ textAlign: 'right', width: '150px' }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map((item, idx) => (
                  <tr key={item.id}>
                    <td>{(pageIndex - 1) * pageSize + idx + 1}</td>
                    <td>
                      <div className="user-cell">
                        <div className="user-avatar-small">
                          <User size={16} />
                        </div>
                        <div className="user-info-text">
                          <span className="user-fullname">{item.hoTen}</span>
                          <span className="user-email">{item.email}</span>
                        </div>
                      </div>
                    </td>
                    <td style={{ fontWeight: 500 }}>{item.tenDangNhap}</td>
                    <td>{item.dienThoai || <span style={{ color: '#cbd5e1' }}>--</span>}</td>
                    <td>
                      <div className="role-badges">
                        {item.vaiTros && item.vaiTros.length > 0 ? (
                          item.vaiTros.map((r, ri) => (
                            <span key={ri} className="role-badge">{r}</span>
                          ))
                        ) : (
                          <span style={{ color: '#94a3b8', fontSize: '0.8rem' }}>Chưa có vai trò</span>
                        )}
                      </div>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button className="action-btn shield" title="Phân vai trò" onClick={() => openRoleModal(item)}>
                          <ShieldCheck size={16} />
                        </button>
                        <button className="action-btn zap" title="Quản lý kỹ năng" onClick={() => openSkillModal(item)}>
                          <Zap size={16} />
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
                  <td colSpan={6} style={{ textAlign: 'center', padding: '48px' }}>
                    Không tìm thấy người dùng nào.
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

      {/* User Edit/Create Modal */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3>
                {modalMode === 'create' ? 'Thêm mới Người dùng' : 
                 modalMode === 'edit' ? 'Chỉnh sửa Người dùng' : 'Thông tin Người dùng'}
              </h3>
              <button className="close-btn" onClick={closeModal}><X size={20} /></button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className="modal-body">
                <div className="form-group">
                  <label>Họ và tên</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.hoTen || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, hoTen: e.target.value})}
                    placeholder="Nhập họ và tên..."
                  />
                </div>
                <div className="form-group">
                  <label>Tên đăng nhập (Tài khoản)</label>
                  <input 
                    type="text" 
                    required 
                    disabled={modalMode !== 'create'}
                    value={selectedItem.tenDangNhap || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, tenDangNhap: e.target.value})}
                    placeholder="Nhập tên tài khoản..."
                  />
                </div>
                {modalMode === 'create' && (
                  <div className="form-group">
                    <label>Mật khẩu</label>
                    <input 
                      type="password" 
                      required 
                      value={formPassword}
                      onChange={(e) => setFormPassword(e.target.value)}
                      placeholder="Nhập mật khẩu..."
                    />
                  </div>
                )}
                <div className="form-group">
                  <label>Email</label>
                  <input 
                    type="email" 
                    required 
                    disabled={modalMode === 'view'}
                    value={selectedItem.email || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, email: e.target.value})}
                    placeholder="example@gmail.com"
                  />
                </div>
                <div className="form-group">
                  <label>Số điện thoại</label>
                  <input 
                    type="text" 
                    disabled={modalMode === 'view'}
                    value={selectedItem.dienThoai || ''}
                    onChange={(e) => setSelectedItem({...selectedItem, dienThoai: e.target.value})}
                    placeholder="Nhập số điện thoại..."
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
                Bạn có chắc chắn muốn xóa người dùng <b>{confirmData?.name}</b>? 
              </p>
              <div className="confirm-footer">
                <button className="btn-secondary" onClick={() => setIsConfirmOpen(false)}>Hủy bỏ</button>
                <button className="btn-danger" onClick={handleConfirmDelete}>Xác nhận xóa</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Role Assignment Modal */}
      {isRoleModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <ShieldAlert size={24} color="#6366f1" />
                <h3>Phân vai trò cho <b>{targetUser?.hoTen}</b></h3>
              </div>
              <button className="close-btn" onClick={() => setIsRoleModalOpen(false)}><X size={20} /></button>
            </div>
            <div className="modal-body">
              {roleMatrixLoading ? (
                <div style={{ padding: '30px', textAlign: 'center' }}>Đang tải danh sách vai trò...</div>
              ) : (
                <div className="role-assignment-list">
                  {allRoles.length > 0 ? (
                    allRoles.map(role => {
                      const isActive = userRoles.includes(role.tenVaiTro);
                      return (
                        <div 
                          key={role.id} 
                          className={`role-item ${isActive ? 'active' : ''}`}
                          onClick={() => handleToggleRole(role)}
                        >
                          <div className="role-item-checkbox">
                            <input 
                              type="checkbox" 
                              checked={isActive} 
                              readOnly 
                              className="custom-checkbox"
                            />
                          </div>
                          <div className="role-item-info">
                            <span className="role-item-name">{role.tenVaiTro}</span>
                            <span className="role-item-code">{role.maVaiTro}</span>
                          </div>
                        </div>
                      );
                    })
                  ) : (
                    <div style={{ textAlign: 'center', color: '#94a3b8', padding: '20px' }}>
                      Không có vai trò nào trong hệ thống.
                    </div>
                  )}
                </div>
              )}
            </div>
            <div className="modal-footer">
              <button className="btn-primary" onClick={() => setIsRoleModalOpen(false)}>Hoàn tất</button>
            </div>
          </div>
        </div>
      )}

      {/* Skill Assignment Modal */}
      {isSkillModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content" style={{ maxWidth: '800px' }}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Zap size={24} color="#f59e0b" />
                <h3>Quản lý kỹ năng: <b>{targetUser?.hoTen}</b></h3>
              </div>
              <button className="close-btn" onClick={() => setIsSkillModalOpen(false)}><X size={20} /></button>
            </div>
            <div className="modal-body">
              {skillLoading ? (
                <div style={{ padding: '30px', textAlign: 'center' }}>Đang tải...</div>
              ) : (
                <div className="skill-assignment-container">
                  <div className="skill-available-list">
                    <h4>Danh sách kỹ năng</h4>
                    <div className="skill-grid">
                      {allSkills.map(skill => {
                        const userSkill = userSkills.find(s => s.kyNangId === skill.id);
                        return (
                          <div key={skill.id} className={`skill-card ${userSkill ? 'assigned' : ''}`}>
                            <div className="skill-card-info">
                              <span className="skill-name">{skill.tenKyNang}</span>
                              <span className="skill-code">{skill.maKyNang}</span>
                            </div>
                            
                            {userSkill ? (
                              <div className="skill-card-actions">
                                <div className="skill-stats">
                                  <span>Lvl: {userSkill.level}</span>
                                  <span>Exp: {userSkill.soNamKinhNghiem}y</span>
                                </div>
                                <div style={{ display: 'flex', gap: '4px' }}>
                                  <button 
                                    className="skill-icon-btn edit" 
                                    onClick={() => setEditingSkill({
                                      kyNangId: skill.id,
                                      level: userSkill.level,
                                      soNamKinhNghiem: userSkill.soNamKinhNghiem
                                    })}
                                  >
                                    <Edit size={14} />
                                  </button>
                                  <button className="skill-icon-btn delete" onClick={() => handleRemoveSkill(skill.id)}>
                                    <Trash2 size={14} />
                                  </button>
                                </div>
                              </div>
                            ) : (
                              <button 
                                className="skill-add-btn"
                                onClick={() => setEditingSkill({ kyNangId: skill.id, level: 1, soNamKinhNghiem: 0 })}
                              >
                                <Plus size={14} /> Gán
                              </button>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  </div>

                  {editingSkill && (
                    <div className="skill-edit-panel">
                      <div className="edit-panel-header">
                        <Award size={18} />
                        <span>Thiết lập: <b>{allSkills.find(s => s.id === editingSkill.kyNangId)?.tenKyNang}</b></span>
                      </div>
                      <div className="edit-panel-body">
                        <div className="form-group-inline">
                          <label><Star size={14} /> Cấp độ (1-5):</label>
                          <input 
                            type="number" 
                            min="1" max="5" 
                            value={editingSkill.level}
                            onChange={(e) => setEditingSkill({...editingSkill, level: parseInt(e.target.value)})}
                          />
                        </div>
                        <div className="form-group-inline">
                          <label><History size={14} /> Kinh nghiệm (năm):</label>
                          <input 
                            type="number" 
                            min="0"
                            value={editingSkill.soNamKinhNghiem}
                            onChange={(e) => setEditingSkill({...editingSkill, soNamKinhNghiem: parseInt(e.target.value)})}
                          />
                        </div>
                        <div className="edit-panel-footer">
                          <button className="btn-cancel" onClick={() => setEditingSkill(null)}>Hủy</button>
                          <button 
                            className="btn-save" 
                            onClick={() => handleUpdateSkill(editingSkill.kyNangId, editingSkill.level, editingSkill.soNamKinhNghiem)}
                          >
                            Xác nhận
                          </button>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
            <div className="modal-footer">
              <button className="btn-primary" onClick={() => setIsSkillModalOpen(false)}>Hoàn tất</button>
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

export default UsersPage;
