/**
 * Định nghĩa các kiểu dữ liệu liên quan đến Dự án.
 */

export const TrangThaiDuAn = {
  Moi: 0,
  DangThucHien: 1,
  TamDung: 2,
  HoanThanh: 3,
  Huy: 4
} as const;

export type TrangThaiDuAn = typeof TrangThaiDuAn[keyof typeof TrangThaiDuAn];

export interface DuAnDto {
  id: number;
  tenDuAn: string;
  moTa?: string;
  ngayBatDau: string;
  ngayKetThuc?: string;
  trangThai: TrangThaiDuAn;
  createdAt: string;
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
