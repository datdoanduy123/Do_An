import React, { useEffect, useState } from 'react';
import { 
  Mail, 
  Phone, 
  Shield, 
  Calendar, 
  Award, 
  Briefcase,
  Edit2,
  Camera
} from 'lucide-react';
import UserService from '../../services/UserService';
import type { NguoiDungDto } from '../../services/UserService';
import './ProfilePage.css';

/**
 * Trang thông tin cá nhân của người dùng.
 */
const ProfilePage: React.FC = () => {
  const [user, setUser] = useState<NguoiDungDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        const userData = await UserService.getProfile();
        setUser(userData);
      } catch (error) {
        console.error('Error fetching profile:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  if (loading) return <div className="loading-state">Đang tải thông tin cá nhân...</div>;
  if (!user) return <div className="error-state">Không thể tải thông tin người dùng.</div>;

  return (
    <div className="profile-container">
      <div className="profile-header-card">
        <div className="cover-photo" />
        <div className="header-content">
          <div className="avatar-wrapper">
            <img 
              src={`https://ui-avatars.com/api/?name=${encodeURIComponent(user.hoTen)}&background=6366f1&color=fff&size=200`} 
              alt="Avatar" 
              className="profile-avatar"
            />
            <button className="change-avatar-btn">
              <Camera size={18} />
            </button>
          </div>
          <div className="user-basic-info">
            <h1 className="user-fullname">{user.hoTen}</h1>
            <p className="user-username">@{user.tenDangNhap}</p>
            <div className="user-badges">
              {user.vaiTros.map((role, idx) => (
                <span key={idx} className="role-badge">
                  <Shield size={12} />
                  {role}
                </span>
              ))}
            </div>
          </div>
          <button className="edit-profile-btn">
            <Edit2 size={18} />
            <span>Chỉnh sửa hồ sơ</span>
          </button>
        </div>
      </div>

      <div className="profile-grid">
        {/* Cột trái: Thông tin liên hệ */}
        <div className="profile-card info-card">
          <div className="card-header">
            <h3>Thông tin liên hệ</h3>
          </div>
          <div className="card-body">
            <div className="info-item">
              <div className="info-icon"><Mail size={18} /></div>
              <div className="info-text">
                <span className="label">Email</span>
                <span className="value">{user.email}</span>
              </div>
            </div>
            <div className="info-item">
              <div className="info-icon"><Phone size={18} /></div>
              <div className="info-text">
                <span className="label">Số điện thoại</span>
                <span className="value">{user.dienThoai || 'Chưa cập nhật'}</span>
              </div>
            </div>
            <div className="info-item">
              <div className="info-icon"><Calendar size={18} /></div>
              <div className="info-text">
                <span className="label">Ngày tham gia</span>
                <span className="value">{new Date(user.createdAt).toLocaleDateString('vi-VN')}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Cột phải: Kỹ năng & Kinh nghiệm */}
        <div className="profile-card skills-card">
          <div className="card-header">
            <h3>Kỹ năng & Chuyên môn</h3>
          </div>
          <div className="card-body">
            {user.kyNangs && user.kyNangs.length > 0 ? (
              <div className="skills-grid">
                {user.kyNangs.map((skill, idx) => (
                  <div key={idx} className="skill-item">
                    <div className="skill-info">
                      <span className="skill-name">{skill.tenKyNang}</span>
                      <span className="skill-exp">{skill.soNamKinhNghiem} năm kinh nghiệm</span>
                    </div>
                    <div className="skill-level-bar">
                      <div 
                        className="level-fill" 
                        style={{ width: `${(skill.level / 10) * 100}%` }} 
                      />
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="empty-skills">
                <Award size={40} />
                <p>Chưa có thông tin kỹ năng</p>
              </div>
            )}
          </div>
        </div>

        {/* Cột dưới: Hoạt động gần đây hoặc dự án tham gia (Placeholder) */}
        <div className="profile-card projects-summary-card full-width">
          <div className="card-header">
            <h3>Dự án đang tham gia</h3>
          </div>
          <div className="card-body">
            <div className="project-placeholder-list">
              {[1, 2].map(i => (
                <div key={i} className="project-placeholder-item">
                  <div className="proj-icon"><Briefcase size={20} /></div>
                  <div className="proj-info">
                    <span className="proj-name">Dự án Mẫu {i}</span>
                    <span className="proj-role">Thành viên chính</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
