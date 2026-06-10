interface PaginationProps {
  currentPage: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  pageSizeOptions?: number[];
  isLoading?: boolean;
}

export default function Pagination({
  currentPage,
  totalCount,
  pageSize,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 25, 50, 100],
  isLoading = false,
}: PaginationProps) {
  const totalPages = Math.ceil(totalCount / pageSize);
  const startItem = totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  return (
    <div className="pagination">
      <button
        className="btn-page"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage <= 1 || isLoading}
      >
        Previous
      </button>

      <span className="page-info">
        {totalCount > 0 ? (
          <>
            Showing {startItem}-{endItem} of {totalCount}
            {totalPages > 1 && ` (Page ${currentPage} of ${totalPages})`}
          </>
        ) : (
          'No results'
        )}
      </span>

      <button
        className="btn-page"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage >= totalPages || isLoading}
      >
        Next
      </button>

      {onPageSizeChange && (
        <select
          className="page-size-select"
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          disabled={isLoading}
        >
          {pageSizeOptions.map((size) => (
            <option key={size} value={size}>
              {size} per page
            </option>
          ))}
        </select>
      )}
    </div>
  );
}
