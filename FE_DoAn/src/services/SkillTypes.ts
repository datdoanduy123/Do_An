export interface KyNangDto {
  id: number;
  tenKyNang: string;
  moTa: string | null;
  congNgheId: number;
  tenCongNghe?: string;
  tenNhomKyNang?: string;
}

export interface TaoKyNangDto {
  tenKyNang: string;
  moTa: string | null;
  congNgheId: number;
}

export interface CapNhatKyNangDto {
  tenKyNang: string;
  moTa: string | null;
  congNgheId: number;
}

export interface SkillQuery {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}

export interface NhomKyNangDto {
  id: number;
  tenNhom: string;
  moTa?: string;
  congNghes?: CongNgheDto[];
}

export interface CongNgheDto {
  id: number;
  tenCongNghe: string;
  moTa?: string;
  nhomKyNangId: number;
  tenNhom?: string;
  kyNangs?: KyNangDto[];
}

export interface TaoNhomKyNangDto {
  tenNhom: string;
  moTa?: string;
}

export interface TaoCongNgheDto {
  tenCongNghe: string;
  moTa?: string;
  nhomKyNangId: number;
}
