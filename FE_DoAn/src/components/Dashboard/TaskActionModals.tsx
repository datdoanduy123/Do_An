import React, { useState } from 'react';
import { X, UserCheck, ArrowRightLeft } from 'lucide-react';
import type { CongViecDto } from '../../services/TaskTypes';
import { ProjectRole } from '../../services/ProjectTypes';
import type { ThanhVienDuAnDto } from '../../services/ProjectTypes';
import type { SprintDto } from '../../services/SprintTypes';
import TaskService from '../../services/TaskService';

// ============================================================
// MODAL GIAO LẠI TASK – Cho phép quản lý chuyển task sang
// một thành viên khác trong cùng dự án.
// ============================================================

interface AssignModalProps {
  /** Task đang được giao lại */
  task: CongViecDto;
  /** Danh sách thành viên trong dự án */
  members: ThanhVienDuAnDto[];
  /** Callback khi đóng modal */
  onClose: () => void;
  /** Callback khi giao việc thành công, truyền lại task đã cập nhật */
  onSuccess: () => void;
}

export const AssignModal: React.FC<AssignModalProps> = ({ task, members, onClose, onSuccess }) => {
  // State lưu ID người được chọn để nhận task
  const [selectedUserId, setSelectedUserId] = useState<number>(task.assigneeId ?? 0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Xử lý xác nhận giao việc:
   * Gọi API assignTask rồi thông báo thành công lên component cha.
   */
  const handleConfirm = async () => {
    if (!selectedUserId) return;
    try {
      setLoading(true);
      setError(null);
      await TaskService.assignTask({ congViecId: task.id, assigneeId: selectedUserId });
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'Không thể giao việc. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="task-action-overlay" onClick={onClose}>
      {/* Ngăn click bên trong modal lan ra overlay */}
      <div className="task-action-modal" onClick={e => e.stopPropagation()}>

        {/* Header */}
        <div className="tam-header">
          <div className="tam-header-left">
            <div className="tam-icon-badge assign">
              <UserCheck size={18} color="#6366f1" />
            </div>
            <div>
              <h3>Giao lại công việc</h3>
              <p>Chọn thành viên nhận task này</p>
            </div>
          </div>
          <button className="tam-close-btn" onClick={onClose} title="Đóng">
            <X size={16} />
          </button>
        </div>

        {/* Body */}
        <div className="tam-body">
          {/* Thông tin task đang xử lý */}
          <div className="tam-task-info">
            <span className="tam-task-info-icon">📋</span>
            <div className="tam-task-info-text">
              <div className="task-id-small">Task #{task.id}</div>
              <div className="task-title-small">{task.tieuDe}</div>
            </div>
          </div>

          {/* XAI: Giải thích quyết định của AI */}
          {task.aiReasoning && (
            <div style={{ marginTop: '0', marginBottom: '16px', padding: '10px 12px', backgroundColor: '#f5f3ff', borderRadius: '8px', border: '1px solid #ddd6fe', display: 'flex', gap: '8px', alignItems: 'flex-start' }}>
              <span style={{ fontSize: '1.1rem', marginTop: '-2px' }}>🤖</span>
              <div>
                <strong style={{ display: 'block', fontSize: '0.82rem', color: '#6d28d9', marginBottom: '4px' }}>Nhận định từ AI (XAI)</strong>
                <span style={{ fontSize: '0.82rem', color: '#4b5563', lineHeight: '1.4', display: 'block' }}>{task.aiReasoning}</span>
              </div>
            </div>
          )}

          {/* Dropdown chọn người nhận:
              - Hiển thị TẤT CẢ thành viên trong dự án
              - Loại bỏ những người có role PM (ProjectRole.PM = 4)
                vì PM là người điều phối, không nhận task thực thi */}
          <label className="tam-label">Giao cho thành viên</label>
          <select
            className="tam-select"
            value={selectedUserId}
            onChange={e => setSelectedUserId(Number(e.target.value))}
          >
            <option value={0}>-- Chọn thành viên --</option>
            {members
              .filter(m => m.vaiTro !== ProjectRole.PM)
              .map(m => {
                // Nhãn vai trò hiển thị kèm theo tên
                const roleLabel: Record<number, string> = {
                  [ProjectRole.Member]:    'Thành viên',
                  [ProjectRole.Developer]: 'Developer',
                  [ProjectRole.Tester]:    'Tester',
                  [ProjectRole.QA]:        'QA',
                  [ProjectRole.BA]:        'BA',
                };
                const role = m.vaiTro !== undefined ? roleLabel[m.vaiTro] ?? '' : '';
                const isCurrent = m.id === task.assigneeId;
                return (
                  <option key={m.id} value={m.id}>
                    {m.hoTen}{role ? ` – ${role}` : ''}{isCurrent ? ' ✓ (đang nhận)' : ''}
                  </option>
                );
              })
            }
          </select>

          {/* Hiển thị thông báo lỗi nếu có */}
          {error && (
            <p style={{ color: '#ef4444', fontSize: '0.82rem', marginTop: '10px', fontWeight: 600 }}>
              ⚠️ {error}
            </p>
          )}
        </div>

        {/* Footer */}
        <div className="tam-footer">
          <button className="tam-btn-cancel" onClick={onClose}>Hủy</button>
          <button
            className="tam-btn-confirm assign-confirm"
            onClick={handleConfirm}
            disabled={loading || !selectedUserId || selectedUserId === task.assigneeId}
          >
            <UserCheck size={14} />
            {loading ? 'Đang giao...' : 'Xác nhận giao'}
          </button>
        </div>
      </div>
    </div>
  );
};

// ============================================================
// MODAL CHUYỂN SPRINT – Cho phép quản lý di chuyển task sang
// một Sprint khác trong cùng dự án (trừ sprint hiện tại).
// ============================================================

interface MoveSprintModalProps {
  /** Task cần chuyển sprint */
  task: CongViecDto;
  /** Danh sách tất cả sprint trong dự án */
  sprints: SprintDto[];
  /** Callback khi đóng modal */
  onClose: () => void;
  /** Callback khi chuyển sprint thành công */
  onSuccess: () => void;
}

export const MoveSprintModal: React.FC<MoveSprintModalProps> = ({ task, sprints, onClose, onSuccess }) => {
  // Chỉ hiển thị các sprint KHÁC sprint hiện tại
  const availableSprints = sprints.filter(s => s.id !== task.sprintId);

  // State lưu sprint đích được chọn
  const [targetSprintId, setTargetSprintId] = useState<number>(
    availableSprints.length > 0 ? availableSprints[0].id : 0
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Xử lý xác nhận chuyển sprint:
   * Gọi TaskService.update() với sprintId mới, giữ nguyên các field khác.
   */
  const handleConfirm = async () => {
    if (!targetSprintId) return;
    try {
      setLoading(true);
      setError(null);

      // Cập nhật task với sprintId mới, giữ nguyên các trường khác
      await TaskService.update(task.id, {
        tieuDe: task.tieuDe,
        moTa: task.moTa,
        loaiCongViec: task.loaiCongViec,
        doUuTien: task.doUuTien,
        assigneeId: task.assigneeId,
        sprintId: targetSprintId,
        thoiGianUocTinh: task.thoiGianUocTinh,
      });
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Không thể chuyển sprint. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  // Tên sprint hiện tại của task
  const currentSprintName = task.tenSprint || 'Backlog';

  // Nhãn hiển thị trạng thái sprint
  const getSprintLabel = (sprint: SprintDto) => {
    const statusMap: Record<number, string> = { 0: '🆕 Mới', 1: '▶️ Đang chạy', 2: '✅ Hoàn thành' };
    return `${sprint.tenSprint} (${statusMap[sprint.trangThai] ?? ''})`;
  };

  return (
    <div className="task-action-overlay" onClick={onClose}>
      <div className="task-action-modal" onClick={e => e.stopPropagation()}>

        {/* Header */}
        <div className="tam-header">
          <div className="tam-header-left">
            <div className="tam-icon-badge sprint">
              <ArrowRightLeft size={18} color="#f59e0b" />
            </div>
            <div>
              <h3>Chuyển sang Sprint khác</h3>
              <p>Đang ở: <strong>{currentSprintName}</strong></p>
            </div>
          </div>
          <button className="tam-close-btn" onClick={onClose} title="Đóng">
            <X size={16} />
          </button>
        </div>

        {/* Body */}
        <div className="tam-body">
          {/* Thông tin task đang xử lý */}
          <div className="tam-task-info">
            <span className="tam-task-info-icon">📋</span>
            <div className="tam-task-info-text">
              <div className="task-id-small">Task #{task.id}</div>
              <div className="task-title-small">{task.tieuDe}</div>
            </div>
          </div>

          {/* Trường hợp không có sprint khác để chuyển */}
          {availableSprints.length === 0 ? (
            <p style={{ color: '#94a3b8', fontSize: '0.9rem', fontWeight: 600, textAlign: 'center', padding: '12px 0' }}>
              Không có Sprint nào khác để chuyển.
            </p>
          ) : (
            <>
              <label className="tam-label">Sprint đích</label>
              <select
                className="tam-select"
                value={targetSprintId}
                onChange={e => setTargetSprintId(Number(e.target.value))}
              >
                {availableSprints.map(s => (
                  <option key={s.id} value={s.id}>
                    {getSprintLabel(s)}
                  </option>
                ))}
              </select>
            </>
          )}

          {/* Hiển thị thông báo lỗi nếu có */}
          {error && (
            <p style={{ color: '#ef4444', fontSize: '0.82rem', marginTop: '10px', fontWeight: 600 }}>
              ⚠️ {error}
            </p>
          )}
        </div>

        {/* Footer */}
        <div className="tam-footer">
          <button className="tam-btn-cancel" onClick={onClose}>Hủy</button>
          {availableSprints.length > 0 && (
            <button
              className="tam-btn-confirm sprint-confirm"
              onClick={handleConfirm}
              disabled={loading || !targetSprintId}
            >
              <ArrowRightLeft size={14} />
              {loading ? 'Đang chuyển...' : 'Chuyển Sprint'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
