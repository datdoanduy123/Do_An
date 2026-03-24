/**
 * Định nghĩa các kiểu dữ liệu liên quan đến Dự án.
 */

export const TrangThaiDuAn = {
  Planning: 0,
  Active: 1,
  Completed: 2,
  Cancelled: 3
} as const;

export type TrangThaiDuAn = typeof TrangThaiDuAn[keyof typeof TrangThaiDuAn];

export interface DuAnDto {
  id: number;
  tenDuAn: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc?: string;
  trangThai: TrangThaiDuAn;
  tienDo: number;
  createdAt: string;
}

export interface ThanhVienDuAnDto {
  id: number;
  hoTen: string;
  email: string;
  vaiTro?: string;
  ngayThamGia: string;
  soCongViec: number;
  kyNang: string[];
}

export interface TaoDuAnDto {
  tenDuAn: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc?: string;
}

export interface CapNhatDuAnDto {
  tenDuAn: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc?: string;
  trangThai: TrangThaiDuAn;
}
