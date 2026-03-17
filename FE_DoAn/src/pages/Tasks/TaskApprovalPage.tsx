import React, { useEffect, useState } from 'react';
import { 
  CheckCircle2, 
  XCircle, 
  Clock, 
  Search,
  User,
  ExternalLink
} from 'lucide-react';
import TaskService from '../../services/TaskService';
import type { CongViecDto } from '../../services/TaskService';
import './TaskApproval.css';

/**
 * Trang duyệt công việc (Dành cho Quản lý).
 */
const TaskApprovalPage: React.FC = () => {
  const [tasks, setTasks] = useState<CongViecDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchPendingTasks = async () => {
    try {
      setLoading(true);
      const data = await TaskService.getPendingReviews();
      setTasks(data || []);
    } catch (error) {
      console.error('Error fetching pending tasks:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingTasks();
  }, []);

  const handleAction = async (taskId: number, approve: boolean) => {
    try {
      // 3 là Done, 1 là InProgress (nếu bị bác bỏ)
      const targetStatus = approve ? 3 : 1;
      const success = await TaskService.updateStatus(taskId, targetStatus);
      if (success) {
        setTasks(prev => prev.filter(t => t.id !== taskId));
        alert(approve ? 'Đã phê duyệt công việc!' : 'Đã bác bỏ công việc!');
      }
    } catch (error) {
      console.error('Action failed:', error);
    }
  };

  const filteredTasks = tasks.filter(t => 
    t.tieuDe.toLowerCase().includes(searchTerm.toLowerCase()) || 
    t.assigneeName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) return <div className="loading-state">Đang tải danh sách chờ duyệt...</div>;

  return (
    <div className="approval-container">
      <header className="page-header">
        <div className="header-info">
          <h1>Duyệt công việc</h1>
          <p>Phê duyệt hoặc bác bỏ các công việc đã hoàn thành từ nhân viên.</p>
        </div>
        <div className="header-actions">
          <div className="search-box">
            <Search size={18} />
            <input 
              type="text" 
              placeholder="Tìm theo tên task hoặc người làm..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
      </header>

      <div className="approval-list">
        {filteredTasks.length > 0 ? (
          filteredTasks.map(task => (
            <div key={task.id} className="approval-card">
              <div className="card-left">
                <div className="task-info">
                  <span className="task-id">TASK-{task.id}</span>
                  <h3>{task.tieuDe}</h3>
                  <div className="task-meta">
                    <span className="meta-item">
                      <User size={14} />
                      {task.assigneeName || 'Chưa gán'}
                    </span>
                    <span className="dot">•</span>
                    <span className="meta-item">
                      <Clock size={14} />
                      Thực tế: {task.thoiGianThucTe || 0}h / Dự kiến: {task.thoiGianUocTinh}h
                    </span>
                  </div>
                </div>
              </div>
              
              <div className="card-right">
                <div className="action-buttons">
                  <button 
                    className="btn-reject" 
                    onClick={() => handleAction(task.id, false)}
                    title="Bác bỏ và yêu cầu làm lại"
                  >
                    <XCircle size={18} />
                    Bác bỏ
                  </button>
                  <button 
                    className="btn-approve" 
                    onClick={() => handleAction(task.id, true)}
                    title="Phê duyệt hoàn thành"
                  >
                    <CheckCircle2 size={18} />
                    Phê duyệt
                  </button>
                </div>
                <button className="btn-view-detail" title="Xem chi tiết">
                  <ExternalLink size={18} />
                </button>
              </div>
            </div>
          ))
        ) : (
          <div className="empty-reviews">
            <CheckCircle2 size={48} color="#10b981" />
            <p>Hiện không có công việc nào đang chờ duyệt.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default TaskApprovalPage;
