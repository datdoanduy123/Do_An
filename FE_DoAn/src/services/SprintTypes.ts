/**
 * Định nghĩa các kiểu dữ liệu liên quan đến Sprint.
 */

export const TrangThaiSprint = {
  New: 0,
  InProgress: 1,
  Finished: 2
} as const;

export type TrangThaiSprint = typeof TrangThaiSprint[keyof typeof TrangThaiSprint];

export interface SprintDto {
  id: number;
  duAnId: number;
  tenSprint: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  mucTieuStoryPoints: number;
  trangThai: TrangThaiSprint;
  tienDo: number;
}

export interface TaoSprintDto {
  duAnId: number;
  tenSprint: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  mucTieuStoryPoints: number;
}

export interface CapNhatSprintDto {
  tenSprint: string;
  ngayBatDau: string;
  ngayKetThuc: string;
  mucTieuStoryPoints: number;
  trangThai: TrangThaiSprint;
}
