import React, { useState } from 'react';
import { X, Send } from 'lucide-react';
import './RejectionModal.css';

interface RejectionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => void;
  taskTitle: string;
}

const RejectionModal: React.FC<RejectionModalProps> = ({ isOpen, onClose, onConfirm, taskTitle }) => {
  const [reason, setReason] = useState('');

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (reason.trim()) {
      onConfirm(reason);
      setReason('');
    }
  };

  return (
    <div className="rejection-modal-overlay" onClick={onClose}>
      <div className="rejection-modal-content" onClick={e => e.stopPropagation()}>
        <div className="rejection-modal-header">
          <div className="header-icon">⚠️</div>
          <div className="header-text">
            <h3>Từ chối Công việc</h3>
            <p>Task: <strong>{taskTitle}</strong></p>
          </div>
          <button className="close-btn" onClick={onClose}><X size={20} /></button>
        </div>
        
        <form onSubmit={handleSubmit} className="rejection-modal-body">
          <label>Vui lòng nhập lý do từ chối và yêu cầu sửa lại:</label>
          <textarea
            value={reason}
            onChange={e => setReason(e.target.value)}
            placeholder="Ví dụ: Giao diện chưa khớp mẫu thiết kế, thiếu logic xử lý lỗi..."
            autoFocus
            required
          />
          
          <div className="modal-actions">
            <button type="button" className="btn-cancel" onClick={onClose}>Hủy</button>
            <button type="submit" className="btn-confirm" disabled={!reason.trim()}>
              <Send size={16} />
              Xác nhận Từ chối
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default RejectionModal;
