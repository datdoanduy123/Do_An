import React from 'react';
import { AlertTriangle, X } from 'lucide-react';
import './ConfirmModal.css';

interface ConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'info';
}

const ConfirmModal: React.FC<ConfirmModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  title = 'Xác nhận hành động',
  message,
  confirmText = 'Đồng ý',
  cancelText = 'Hủy bỏ',
  type = 'warning'
}) => {
  if (!isOpen) return null;

  return (
    <div className="confirm-modal-overlay" onClick={onClose}>
      <div className="confirm-modal-content" onClick={e => e.stopPropagation()}>
        <div className="confirm-modal-header">
          <div className={`icon-box ${type}`}>
            <AlertTriangle size={24} />
          </div>
          <button className="confirm-close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>
        
        <div className="confirm-modal-body">
          <h3>{title}</h3>
          <p>{message}</p>
        </div>

        <div className="confirm-modal-footer">
          <button className="btn-confirm-cancel" onClick={onClose}>
            {cancelText}
          </button>
          <button 
            className={`btn-confirm-submit ${type}`} 
            onClick={() => {
              onConfirm();
              onClose();
            }}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmModal;
