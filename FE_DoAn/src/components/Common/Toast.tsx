import React, { useEffect } from 'react';
import { CheckCircle, AlertCircle, X } from 'lucide-react';
import './Toast.css';

export interface ToastProps {
  message: string;
  type: 'success' | 'error';
  onClose: () => void;
  duration?: number;
}

const Toast: React.FC<ToastProps> = ({ message, type, onClose, duration = 5200 }) => {
  useEffect(() => {
    const timer = setTimeout(() => {
      onClose();
    }, duration);
    return () => clearTimeout(timer);
  }, [onClose, duration]);

  return (
    <div className={`toast-notification-v2 ${type}`}>
      <div className="toast-icon">
        {type === 'success' ? <CheckCircle size={24} /> : <AlertCircle size={24} />}
      </div>
      <div className="toast-content">
        <span className="toast-title">{type === 'success' ? 'Thành công' : 'Cảnh báo'}</span>
        <span className="toast-msg">{message}</span>
      </div>
      <button className="toast-close" onClick={onClose}>
        <X size={16} />
      </button>
    </div>
  );
};

export default Toast;
