export interface KyNangDto {
  id: number;
  tenKyNang: string;
  maKyNang: string;
  moTa: string | null;
}

export interface TaoKyNangDto {
  tenKyNang: string;
  maKyNang: string;
  moTa: string | null;
}

export interface CapNhatKyNangDto {
  tenKyNang: string;
  maKyNang: string;
  moTa: string | null;
}

export interface SkillQuery {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}
