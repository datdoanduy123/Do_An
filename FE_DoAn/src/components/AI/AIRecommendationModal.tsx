import React from 'react';
import { 
  X, 
  User, 
  Brain, 
  CheckCircle,
  Lightbulb,
  Loader2,
  Sparkles
} from 'lucide-react';
import './AIRecommendation.css';

export interface AIRecommendation {
  userId: number;
  hoTen: string;
  diemPhuHop: number;
  lyDo: string;
  kyNangPhuHop: string[];
}

interface Props {
  taskId: number;
  taskTitle: string;
  recommendations: AIRecommendation[];
  loading: boolean;
  onClose: () => void;
  onApply: (userId: number) => void;
}

const AIRecommendationModal: React.FC<Props> = ({
  taskTitle,
  recommendations,
  loading,
  onClose,
  onApply
}) => {
  return (
    <div className="ai-modal-overlay">
      <div className="ai-modal-content premium">
        <div className="ai-modal-header">
          <div className="header-title-group">
            <div className="ai-sparkle-bg">
              <Sparkles size={20} className="ai-rainbow-icon" />
            </div>
            <div>
              <h2>Trợ lý Giao việc AI</h2>
              <p className="task-ref">Công việc: <span>{taskTitle}</span></p>
            </div>
          </div>
          <button className="close-ai-btn" onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <div className="ai-modal-body">
          {loading ? (
            <div className="ai-loading-state">
              <div className="brain-animation">
                <Brain size={48} />
                <div className="pulse-ring"></div>
              </div>
              <h3>AI đang phân tích kỹ năng...</h3>
              <p>Chúng tôi đang đối chiếu yêu cầu công việc với hồ sơ nhân viên để tìm người phù hợp nhất.</p>
            </div>
          ) : recommendations.length > 0 ? (
            <div className="recommendations-container">
              <div className="recommendations-list">
                {recommendations.sort((a, b) => b.diemPhuHop - a.diemPhuHop).map((rec, index) => (
                  <div key={rec.userId} className={`rec-card-premium ${index === 0 ? 'best-match' : ''}`}>
                    {index === 0 && <span className="match-badge">Phù hợp nhất</span>}
                    
                    <div className="rec-card-main">
                      <div className="user-avatar-large">
                        {rec.hoTen.charAt(0)}
                      </div>
                      
                      <div className="rec-user-info">
                        <div className="name-score">
                          <h3>{rec.hoTen}</h3>
                          <div className="match-score-circle">
                            <span className="score-val">{rec.diemPhuHop}%</span>
                          </div>
                        </div>
                        
                        <div className="match-reason">
                          <Lightbulb size={14} />
                          <p>{rec.lyDo}</p>
                        </div>

                        <div className="skill-tags">
                          {rec.kyNangPhuHop.map((skill, sIdx) => (
                            <span key={sIdx} className="skill-pill">{skill}</span>
                          ))}
                        </div>
                      </div>

                      <button 
                        className="btn-apply-rec"
                        onClick={() => onApply(rec.userId)}
                      >
                        Giao việc
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="ai-empty-state">
              <User size={40} />
              <h3>Không tìm thấy ứng viên phù hợp</h3>
              <p>AI không tìm thấy nhân viên nào có kỹ năng phù hợp với yêu cầu của công việc này.</p>
            </div>
          )}
        </div>

        <div className="ai-modal-footer">
          <p className="ai-disclaimer">
            <Brain size={12} />
            Đề xuất dựa trên bóc tách kỹ năng từ tên và mô tả công việc.
          </p>
        </div>
      </div>
    </div>
  );
};

export default AIRecommendationModal;
