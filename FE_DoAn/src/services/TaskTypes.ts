/**
 * Định nghĩa các kiểu dữ liệu liên quan đến Công việc (Task).
 */

export const TrangThaiCongViec = {
  Todo: 0,
  InProgress: 1,
  Review: 2,
  Done: 3,
  Cancelled: 4
} as const;

export type TrangThaiCongViec = typeof TrangThaiCongViec[keyof typeof TrangThaiCongViec];

export const DoUuTien = {
  Low: 0,
  Medium: 1,
  High: 2,
  Urgent: 3
} as const;

export type DoUuTien = typeof DoUuTien[keyof typeof DoUuTien];

export const LoaiCongViec = {
  Backend: 0,
  Frontend: 1,
  Fullstack: 2,
  Mobile: 3,
  DevOps: 4,
  Tester: 5,
  UIUX: 6,
  BA: 7
} as const;

export type LoaiCongViec = typeof LoaiCongViec[keyof typeof LoaiCongViec];

export interface CongViecDto {
  id: number;
  duAnId: number;
  sprintId?: number;
  sprintStatus?: number;
  tieuDe: string;
  moTa?: string;
  loaiCongViec: LoaiCongViec;
  doUuTien: DoUuTien;
  trangThai: TrangThaiCongViec;
  assigneeId?: number;
  assigneeName?: string;
  thoiGianUocTinh: number;
  thoiGianThucTe?: number;
  ngayBatDauDuKien?: string;
  ngayKetThucDuKien?: string;
  ngayBatDauThucTe?: string;
  ngayKetThucThucTe?: string;
  ngayBatDauSprint?: string;
  ngayKetThucSprint?: string;
  soLanBiTuChoi: number;
  createdBy?: number;
  dependencies?: {
    dependsOnTaskId: number;
    dependsOnTaskTitle?: string;
  }[];
}

export interface TaoCongViecDto {
  duAnId: number;
  sprintId?: number;
  tieuDe: string;
  moTa?: string;
  loaiCongViec: LoaiCongViec;
  doUuTien: DoUuTien;
  thoiGianUocTinh: number;
  assigneeId?: number;
  thoiGianThucTe?: number;
}

export interface CapNhatCongViecDto {
  tieuDe: string;
  moTa?: string;
  loaiCongViec: LoaiCongViec;
  doUuTien: DoUuTien;
  assigneeId?: number;
  sprintId?: number;
  thoiGianUocTinh: number;
}
