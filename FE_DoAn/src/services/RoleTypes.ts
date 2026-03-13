export interface VaiTroDto {
  id: number;
  tenVaiTro: string;
  maVaiTro: string;
  moTa: string;
}

export interface TaoVaiTroDto {
  tenVaiTro: string;
  maVaiTro: string;
  moTa: string;
}

export interface CapNhatVaiTroDto {
  tenVaiTro: string;
  moTa: string;
}

export interface VaiTroQuery {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}
