import React, { useEffect, useState } from 'react';
import { 
  Cpu, 
  Settings, 
  Save, 
  RotateCcw, 
  Info, 
  ToggleLeft, 
  ToggleRight,
  Sparkles,
  Search,
  CheckCircle2,
  XCircle
} from 'lucide-react';
import AiRuleService, { type AiRuleDto } from '../../services/AiRuleService';
import './AiRules.css';

/**
 * Trang quản lý cấu hình Quy tắc AI cho người quản lý.
 */
const AiRulesPage: React.FC = () => {
  const [rules, setRules] = useState<AiRuleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchRules();
  }, []);

  const fetchRules = async () => {
    try {
      setLoading(true);
      const data = await AiRuleService.getAll();
      setRules(data);
    } catch (error) {
      console.error('Error fetching AI rules:', error);
      alert('Không thể tải danh sách quy tắc AI.');
    } finally {
      setLoading(false);
    }
  };

  const handleEditClick = (rule: AiRuleDto) => {
    setEditingId(rule.id);
    setEditValue(rule.giaTri);
    setIsActive(rule.isActive);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditValue('');
  };

  const handleSave = async (id: number) => {
    try {
      setSaving(true);
      await AiRuleService.update(id, { 
        giaTri: editValue,
        isActive: isActive
      });
      
      // Update local state instead of refetching everything
      setRules(prev => prev.map(r => 
        r.id === id ? { ...r, giaTri: editValue, isActive: isActive } : r
      ));
      
      setEditingId(null);
      alert('Cập nhật quy tắc thành công!');
    } catch (error) {
      console.error('Update failed:', error);
      alert('Cập nhật thất bại. Vui lòng thử lại.');
    } finally {
      setSaving(false);
    }
  };

  const filteredRules = rules.filter(r => 
    r.maQuyTac.toLowerCase().includes(searchTerm.toLowerCase()) || 
    r.moTa?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) return (
    <div className="rules-loading">
      <div className="spinner-modern"></div>
      <p>Đang tải cấu hình AI...</p>
    </div>
  );

  return (
    <div className="ai-rules-container">
      <div className="rules-header">
        <div className="header-left">
          <div className="icon-badge-ai">
            <Cpu size={24} />
          </div>
          <div>
            <h1>Cấu hình Quy tắc AI</h1>
            <p>Điều chỉnh các trọng số và tham số logic cho trợ lý AI bóc tách công việc.</p>
          </div>
        </div>
        <div className="header-right">
          <div className="search-box-rules">
            <Search size={18} />
            <input 
              type="text" 
              placeholder="Tìm kiếm quy tắc..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
      </div>

      <div className="ai-notice-card">
        <div className="notice-icon">
          <Sparkles size={24} />
        </div>
        <div className="notice-content">
          <h4>Về các quy tắc này</h4>
          <p>AI sử dụng các tham số bên dưới để tính toán khoảng cách KNN khi giao việc và xác định quy trình bóc tách. Việc thay đổi giá trị có thể ảnh hưởng trực tiếp đến kết quả phân bổ nhân sự.</p>
        </div>
      </div>

      <div className="rules-grid">
        {filteredRules.map((rule) => (
          <div key={rule.id} className={`rule-card ${editingId === rule.id ? 'is-editing' : ''}`}>
            <div className="rule-card-header">
              <div className="rule-code-wrapper">
                <span className="rule-code">{rule.maQuyTac}</span>
                <span className={`data-type-tag ${rule.loaiDuLieu.toLowerCase()}`}>{rule.loaiDuLieu}</span>
              </div>
              <div className="rule-status-indicator">
                {rule.isActive ? <CheckCircle2 className="txt-success" size={16} /> : <XCircle className="txt-muted" size={16} />}
              </div>
            </div>

            <div className="rule-card-body">
              {editingId === rule.id ? (
                <div className="edit-form-inline">
                  <div className="input-field-edit">
                    <label>Giá trị mới:</label>
                    <input 
                      type="text" 
                      value={editValue} 
                      onChange={(e) => setEditValue(e.target.value)}
                      autoFocus
                    />
                  </div>
                  <div className="toggle-switch-wrapper" onClick={() => setIsActive(!isActive)}>
                    <label>Kích hoạt:</label>
                    {isActive ? <ToggleRight className="txt-indigo" size={32} /> : <ToggleLeft className="txt-muted" size={32} />}
                  </div>
                </div>
              ) : (
                <div className="value-display">
                  <span className="value-label">Giá trị hiện tại:</span>
                  <span className="value-text">{rule.giaTri}</span>
                </div>
              )}
              <p className="rule-desc">{rule.moTa || 'Không có mô tả cho quy tắc này.'}</p>
            </div>

            <div className="rule-card-footer">
              {editingId === rule.id ? (
                <>
                  <button className="btn-cancel-edit" onClick={cancelEdit} disabled={saving}>
                    <RotateCcw size={16} />
                    <span>Hủy</span>
                  </button>
                  <button className="btn-save-edit" onClick={() => handleSave(rule.id)} disabled={saving || !editValue}>
                    {saving ? <div className="spinner-mini"></div> : <Save size={16} />}
                    <span>Lưu cấu hình</span>
                  </button>
                </>
              ) : (
                <button className="btn-trigger-edit" onClick={() => handleEditClick(rule)}>
                  <Settings size={16} />
                  <span>Điều chỉnh</span>
                </button>
              )}
            </div>
          </div>
        ))}

        {filteredRules.length === 0 && (
          <div className="empty-rules-state">
            <Info size={48} />
            <p>Không tìm thấy quy tắc nào khớp với từ khóa tìm kiếm.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default AiRulesPage;
