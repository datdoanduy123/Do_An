export interface UserSkillDto {
  kyNangId: number;
  tenKyNang: string;
  level: number;
  soNamKinhNghiem: number;
}

export interface GanKyNangDto {
  nguoiDungId: number;
  kyNangId: number;
  level: number;
  soNamKinhNghiem: number;
}

export interface NguoiDungDto {
  id: number;
  tenDangNhap: string;
  hoTen: string;
  email: string;
  dienThoai: string;
  vaiTros: string[];
  quyens?: string[];
  kyNangs: UserSkillDto[];
  createdAt: string;
}

export interface TaoNguoiDungDto {
  tenDangNhap: string;
  matKhau: string;
  hoTen: string;
  email: string;
  dienThoai: string;
}

export interface CapNhatNguoiDungDto {
  hoTen: string;
  email: string;
  dienThoai: string;
}

export interface NguoiDungQuery {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}
