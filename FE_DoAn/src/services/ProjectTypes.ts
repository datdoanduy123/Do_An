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

export const ProjectRole = {
  Member: 0,
  Developer: 1,
  Tester: 2,
  QA: 3,
  PM: 4,
  BA: 5
} as const;

export type ProjectRole = typeof ProjectRole[keyof typeof ProjectRole];

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
  vaiTro?: ProjectRole;
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
