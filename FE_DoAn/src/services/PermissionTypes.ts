export interface QuyenDto {
  id: number;
  tenQuyen: string;
  maQuyen: string;
  moTa: string;
  nhomQuyenId: number;
  tenNhomQuyen: string;
}

export interface NhomQuyenDto {
  id: number;
  tenNhom: string;
  moTa: string;
}

export interface PaginatedResult<T> {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: T[];
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PermissionQuery {
  pageIndex?: number;
  pageSize?: number;
  keyword?: string;
}
