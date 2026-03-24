/**
 * Định nghĩa các kiểu dữ liệu liên quan đến Tài liệu dự án.
 */

export interface TaiLieuDuAnDto {
  id: number;
  duAnId: number;
  fileName: string;
  filePath: string;
  fileType: string;
  uploadedBy: number;
  uploadAt: string;
  isProcessed: boolean;
}
