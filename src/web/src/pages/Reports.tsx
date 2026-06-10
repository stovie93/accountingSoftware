import React, { useState, useEffect, useMemo } from 'react';
import { reportsApi } from '../services/api';
import { usePagination } from '../hooks/usePagination';
import { useDebounce } from '../hooks/useDebounce';
import Pagination from '../components/Pagination';
import ReportEditor from '../components/ReportEditor';
import type { Report, ReportType, ReportConfiguration, ReportSourceType } from '../types';

// Sample data for preview (same as ReportEditor)
const SAMPLE_DATA: Record<ReportSourceType, Record<string, string | number>[]> = {
  bills: [
    { billId: 'b001', billReferenceId: 'BILL-000001', billExternalId: 'EXT-001', billAmount: 1250.00, billTotalAmount: 1312.50, billStatus: 'Pending', billDate: '2026-01-15', billDueDate: '2026-02-15', billPaidDate: '', billDescription: 'Electric bill for Oakwood', billQuantity: 1042, billUnit: 'kWh', billRate: 0.12, billPeriodStart: 'Jan 1', billPeriodEnd: 'Jan 31', billCategory: 'Electric', billTax: 62.50, billNotes: '', 'property.propertyName': 'Oakwood Apartments', 'property.propertyCode': 'OAK', 'property.propertyAddress': '123 Oak Street', 'provider.providerName': 'Denver Power & Light', 'provider.providerCode': 'DPL', 'utility.utilityName': 'Electric', 'utility.utilityType': 'Electric' },
    { billId: 'b002', billReferenceId: 'BILL-000002', billExternalId: 'EXT-002', billAmount: 485.00, billTotalAmount: 509.25, billStatus: 'Paid', billDate: '2026-01-15', billDueDate: '2026-02-15', billPaidDate: '2026-01-20', billDescription: 'Water bill for Maplegrove', billQuantity: 12125, billUnit: 'gallons', billRate: 0.004, billPeriodStart: 'Jan 1', billPeriodEnd: 'Jan 31', billCategory: 'Water', billTax: 24.25, billNotes: '', 'property.propertyName': 'Maplegrove Complex', 'property.propertyCode': 'MAP', 'property.propertyAddress': '456 Maple Ave', 'provider.providerName': 'Denver Water Authority', 'provider.providerCode': 'DWA', 'utility.utilityName': 'Water', 'utility.utilityType': 'Water' },
    { billId: 'b003', billReferenceId: 'BILL-000003', billExternalId: 'EXT-003', billAmount: 890.00, billTotalAmount: 934.50, billStatus: 'Needs approval', billDate: '2026-01-15', billDueDate: '2026-02-15', billPaidDate: '', billDescription: 'Gas bill for Pinecrest', billQuantity: 742, billUnit: 'therms', billRate: 1.2, billPeriodStart: 'Jan 1', billPeriodEnd: 'Jan 31', billCategory: 'Gas', billTax: 44.50, billNotes: 'High usage month', 'property.propertyName': 'Pinecrest Towers', 'property.propertyCode': 'PIN', 'property.propertyAddress': '789 Pine Blvd', 'provider.providerName': 'Colorado Natural Gas', 'provider.providerCode': 'CNG', 'utility.utilityName': 'Natural Gas', 'utility.utilityType': 'Gas' },
  ],
  properties: [
    { propertyId: 'p001', propertyClientId: 'OAK-001', propertyName: 'Oakwood Apartments', propertyAddress: '123 Oak Street', propertyCity: 'Denver', propertyState: 'CO', propertyZipCode: '80202', propertyCountry: 'USA', propertyType: 'Residential', propertyCode: 'OAK', propertyDescription: '24-unit apartment complex', propertyUnitCount: 24, propertySquareFootage: 18000 },
    { propertyId: 'p002', propertyClientId: 'MAP-001', propertyName: 'Maplegrove Complex', propertyAddress: '456 Maple Ave', propertyCity: 'Denver', propertyState: 'CO', propertyZipCode: '80203', propertyCountry: 'USA', propertyType: 'Residential', propertyCode: 'MAP', propertyDescription: '36-unit complex with pool', propertyUnitCount: 36, propertySquareFootage: 27000 },
    { propertyId: 'p003', propertyClientId: 'PIN-001', propertyName: 'Pinecrest Towers', propertyAddress: '789 Pine Blvd', propertyCity: 'Denver', propertyState: 'CO', propertyZipCode: '80204', propertyCountry: 'USA', propertyType: 'Residential', propertyCode: 'PIN', propertyDescription: '60-unit high-rise', propertyUnitCount: 60, propertySquareFootage: 45000 },
  ],
  providers: [
    { providerId: 'pr001', providerName: 'Denver Power & Light', providerCode: 'DPL', providerType: 'Electric' },
    { providerId: 'pr002', providerName: 'Denver Water Authority', providerCode: 'DWA', providerType: 'Water' },
    { providerId: 'pr003', providerName: 'Colorado Natural Gas', providerCode: 'CNG', providerType: 'Gas' },
  ],
  utilities: [
    { utilityId: 'u001', utilityName: 'Electric', utilityType: 'Electric', utilityCode: 'ELEC' },
    { utilityId: 'u002', utilityName: 'Water', utilityType: 'Water', utilityCode: 'WTR' },
    { utilityId: 'u003', utilityName: 'Natural Gas', utilityType: 'Gas', utilityCode: 'GAS' },
  ],
  items: [
    { itemId: 'i001', externalId: 'INV-OAK-ELE-202601', date: '2026-01-01', amount: 1250.00, quantity: 1042, description: 'Electric bill for oakwood-apts' },
    { itemId: 'i002', externalId: 'INV-MAP-WTR-202601', date: '2026-01-01', amount: 485.00, quantity: 12125, description: 'Water bill for maplegrove' },
    { itemId: 'i003', externalId: 'INV-PIN-GAS-202601', date: '2026-01-01', amount: 890.00, quantity: 742, description: 'Gas bill for pinecrest' },
  ],
};

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

function parseConfig(configStr: string): ReportConfiguration | null {
  try {
    return JSON.parse(configStr);
  } catch {
    return null;
  }
}

type SortColumn = 'name' | 'source' | 'columns' | 'createdAt';
type SortDirection = 'asc' | 'desc';

export default function Reports() {
  const [reports, setReports] = useState<Report[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showEditor, setShowEditor] = useState(false);
  const [editingReport, setEditingReport] = useState<Report | null>(null);
  const [isExporting, setIsExporting] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [exportMenuOpen, setExportMenuOpen] = useState<string | null>(null);
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [sortColumn, setSortColumn] = useState<SortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  const { page, pageSize, setPage, setPageSize } = usePagination();
  const debouncedSearch = useDebounce(searchQuery, 300);

  const handleSort = (column: SortColumn) => {
    if (sortColumn === column) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  const SortIndicator = ({ column }: { column: SortColumn }) => {
    if (sortColumn !== column) {
      return <span className="sort-indicator inactive">⇅</span>;
    }
    return <span className="sort-indicator active">{sortDirection === 'asc' ? '↑' : '↓'}</span>;
  };

  // Generate preview data for selected report
  const previewData = useMemo(() => {
    if (!selectedReport) return null;

    const config = parseConfig(selectedReport.configuration);
    if (!config) return null;

    const sourceType = config.sourceType || 'bills';
    const sampleRows = SAMPLE_DATA[sourceType] || [];
    const headers: string[] = [];
    const rows: (string | number)[][] = [];

    // Build column order
    type ColumnDef =
      | { type: 'auto'; data: NonNullable<ReportConfiguration['autoIncrement']> }
      | { type: 'static'; data: NonNullable<ReportConfiguration['staticColumns']>[number] }
      | { type: 'data'; data: NonNullable<ReportConfiguration['columns']>[number] };

    const columnOrder: ColumnDef[] = [];

    if (config.outputOrder && config.outputOrder.length > 0) {
      config.outputOrder.forEach(orderKey => {
        if (orderKey === 'auto' && config.autoIncrement) {
          columnOrder.push({ type: 'auto', data: config.autoIncrement });
        } else if (orderKey.startsWith('static:') && config.staticColumns) {
          const idx = parseInt(orderKey.split(':')[1], 10);
          if (config.staticColumns[idx]) {
            columnOrder.push({ type: 'static', data: config.staticColumns[idx] });
          }
        } else if (orderKey.startsWith('data:') && config.columns) {
          const idx = parseInt(orderKey.split(':')[1], 10);
          if (config.columns[idx]) {
            columnOrder.push({ type: 'data', data: config.columns[idx] });
          }
        }
      });
    } else {
      // Default order
      if (config.autoIncrement) {
        columnOrder.push({ type: 'auto', data: config.autoIncrement });
      }
      config.staticColumns?.forEach(sc => columnOrder.push({ type: 'static', data: sc }));
      config.columns?.forEach(col => columnOrder.push({ type: 'data', data: col }));
    }

    // Build headers
    columnOrder.forEach(col => {
      if (col.type === 'auto') {
        headers.push(col.data.header || 'Line');
      } else if (col.type === 'static') {
        headers.push(col.data.header);
      } else {
        headers.push(col.data.header);
      }
    });

    // Build rows
    sampleRows.forEach((sample, rowIndex) => {
      const row: (string | number)[] = [];
      columnOrder.forEach(col => {
        if (col.type === 'auto') {
          const lineNum = (col.data.startAt || 1) + rowIndex;
          row.push(`${col.data.prefix || ''}${lineNum}`);
        } else if (col.type === 'static') {
          row.push(col.data.value);
        } else {
          const value = sample[col.data.source];
          row.push(value !== undefined ? value : '');
        }
      });
      rows.push(row);
    });

    return { headers, rows, sourceType };
  }, [selectedReport]);

  // Load reports
  useEffect(() => {
    const loadReports = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await reportsApi.list({
          page,
          pageSize,
          search: debouncedSearch || undefined,
        });
        setReports(response.reports as Report[]);
        setTotalCount(response.totalCount);
      } catch (err) {
        setError('Failed to load reports');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    loadReports();
  }, [page, pageSize, debouncedSearch]);

  // Close export menu when clicking outside
  useEffect(() => {
    const handleClickOutside = () => setExportMenuOpen(null);
    if (exportMenuOpen) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [exportMenuOpen]);

  const handleCreate = () => {
    setEditingReport(null);
    setShowEditor(true);
  };

  const handlePreview = (report: Report, e?: React.MouseEvent) => {
    e?.stopPropagation();
    setSelectedReport(report);
  };

  const handleEdit = (report: Report, e?: React.MouseEvent) => {
    e?.stopPropagation();
    setEditingReport(report);
    setShowEditor(true);
  };

  const handleSave = async (data: { name: string; type: ReportType; configuration: string; isShared: boolean }) => {
    try {
      if (editingReport) {
        await reportsApi.update(editingReport.id, data);
      } else {
        await reportsApi.create(data);
      }
      setShowEditor(false);
      setEditingReport(null);
      // Reload reports
      const response = await reportsApi.list({ page, pageSize, search: debouncedSearch || undefined });
      setReports(response.reports as Report[]);
      setTotalCount(response.totalCount);
    } catch (err) {
      console.error('Failed to save report:', err);
      throw err;
    }
  };

  const handleDelete = async (report: Report, e?: React.MouseEvent) => {
    e?.stopPropagation();
    if (!confirm(`Are you sure you want to delete "${report.name}"?`)) {
      return;
    }
    try {
      await reportsApi.delete(report.id);
      // Reload reports
      const response = await reportsApi.list({ page, pageSize, search: debouncedSearch || undefined });
      setReports(response.reports as Report[]);
      setTotalCount(response.totalCount);
    } catch (err) {
      console.error('Failed to delete report:', err);
      setError('Failed to delete report');
    }
  };

  const handleDuplicate = async (report: Report, e?: React.MouseEvent) => {
    e?.stopPropagation();
    try {
      await reportsApi.duplicate(report.id);
      // Reload reports
      const response = await reportsApi.list({ page, pageSize, search: debouncedSearch || undefined });
      setReports(response.reports as Report[]);
      setTotalCount(response.totalCount);
    } catch (err) {
      console.error('Failed to duplicate report:', err);
      setError('Failed to duplicate report');
    }
  };

  const handleExport = async (report: Report, format: 'csv' | 'xlsx', e?: React.MouseEvent) => {
    e?.stopPropagation();
    setExportMenuOpen(null);
    setIsExporting(report.id);
    try {
      const blob = await reportsApi.export(report.id, format);

      // Create download link
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${report.name.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().split('T')[0]}.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Failed to export report:', err);
      setError('Failed to export report');
    } finally {
      setIsExporting(null);
    }
  };

  const toggleExportMenu = (reportId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setExportMenuOpen(exportMenuOpen === reportId ? null : reportId);
  };

  const getSourceTypeLabel = (config: ReportConfiguration): string => {
    switch (config.sourceType) {
      case 'bills': return 'Bills';
      case 'properties': return 'Properties';
      case 'providers': return 'Providers';
      case 'utilities': return 'Utilities';
      case 'items': return 'Items';
      default: return config.sourceType || 'Unknown';
    }
  };

  const getColumnCount = (config: ReportConfiguration | null): number => {
    if (!config) return 0;
    let count = config.columns?.length || 0;
    count += config.staticColumns?.length || 0;
    if (config.autoIncrement) count += 1;
    return count;
  };

  const sortedReports = useMemo(() => {
    if (!sortColumn) return reports;
    return [...reports].sort((a, b) => {
      let aVal: string | number;
      let bVal: string | number;
      switch (sortColumn) {
        case 'name':
          aVal = a.name.toLowerCase();
          bVal = b.name.toLowerCase();
          break;
        case 'source':
          const aConfig = parseConfig(a.configuration);
          const bConfig = parseConfig(b.configuration);
          aVal = aConfig ? getSourceTypeLabel(aConfig).toLowerCase() : '';
          bVal = bConfig ? getSourceTypeLabel(bConfig).toLowerCase() : '';
          break;
        case 'columns':
          aVal = getColumnCount(parseConfig(a.configuration));
          bVal = getColumnCount(parseConfig(b.configuration));
          break;
        case 'createdAt':
          aVal = a.createdAt;
          bVal = b.createdAt;
          break;
        default:
          return 0;
      }
      if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
      if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [reports, sortColumn, sortDirection]);

  return (
    <div className="reports-page">
      <div className="reports-header">
        <div className="reports-header-left">
          <h1>Reports</h1>
          <span className="reports-count">{totalCount} report{totalCount !== 1 ? 's' : ''}</span>
        </div>
        <div className="reports-header-right">
          <div className="search-box">
            <input
              type="text"
              placeholder="Search reports..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="search-input"
            />
          </div>
          <button className="btn-primary" onClick={handleCreate}>
            + New Report
          </button>
        </div>
      </div>

      {error && (
        <div className="error-banner">
          {error}
          <button onClick={() => setError(null)} className="btn-close">&times;</button>
        </div>
      )}

      <div className="reports-table-container">
        {isLoading ? (
          <div className="loading">Loading reports...</div>
        ) : reports.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">&#128196;</div>
            <h3>No reports yet</h3>
            <p>Create your first report template to export data</p>
            <button className="btn-primary" onClick={handleCreate}>
              + Create Report
            </button>
          </div>
        ) : (
          <>
            <table className="reports-table">
              <thead>
                <tr>
                  <th className="sortable" onClick={() => handleSort('name')}>
                    Name <SortIndicator column="name" />
                  </th>
                  <th className="sortable" onClick={() => handleSort('source')}>
                    Source <SortIndicator column="source" />
                  </th>
                  <th className="sortable" onClick={() => handleSort('columns')}>
                    Columns <SortIndicator column="columns" />
                  </th>
                  <th className="sortable" onClick={() => handleSort('createdAt')}>
                    Created <SortIndicator column="createdAt" />
                  </th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {sortedReports.map((report) => {
                  const config = parseConfig(report.configuration);
                  return (
                    <tr key={report.id}>
                      <td className="report-name-cell">
                        <span className="report-name">{report.name}</span>
                        {report.isShared && <span className="shared-badge">Shared</span>}
                      </td>
                      <td>
                        <span className="source-badge">{config ? getSourceTypeLabel(config) : '-'}</span>
                      </td>
                      <td>{getColumnCount(config)}</td>
                      <td className="date-cell">{formatDate(report.createdAt)}</td>
                      <td className="actions-cell">
                        <div className="action-buttons">
                          <button
                            className="btn-icon"
                            onClick={(e) => handlePreview(report, e)}
                            title="Preview"
                          >
                            &#128065;
                          </button>
                          <div className="export-dropdown">
                            <button
                              className="btn-run"
                              onClick={(e) => toggleExportMenu(report.id, e)}
                              disabled={isExporting === report.id}
                            >
                              {isExporting === report.id ? 'Running...' : 'Run'}
                              <span className="dropdown-arrow">&#9662;</span>
                            </button>
                            {exportMenuOpen === report.id && (
                              <div className="export-menu">
                                <button onClick={(e) => handleExport(report, 'csv', e)}>
                                  Export as CSV
                                </button>
                                <button onClick={(e) => handleExport(report, 'xlsx', e)}>
                                  Export as Excel
                                </button>
                              </div>
                            )}
                          </div>
                          <button
                            className="btn-edit"
                            onClick={(e) => handleEdit(report, e)}
                            title="Edit report"
                          >
                            Edit
                          </button>
                          <button
                            className="btn-icon"
                            onClick={(e) => handleDuplicate(report, e)}
                            title="Duplicate report"
                          >
                            &#128464;
                          </button>
                          <button
                            className="btn-icon btn-icon-danger"
                            onClick={(e) => handleDelete(report, e)}
                            title="Delete report"
                          >
                            &#128465;
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            <Pagination
              currentPage={page}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isLoading={isLoading}
            />
          </>
        )}
      </div>

      {/* Preview Modal */}
      {selectedReport && previewData && (
        <div className="modal-overlay" onClick={() => setSelectedReport(null)}>
          <div className="modal preview-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>{selectedReport.name}</h2>
                <span className="preview-subtitle">Preview with sample data</span>
              </div>
              <button className="close-btn" onClick={() => setSelectedReport(null)}>&times;</button>
            </div>
            <div className="modal-body">
              <div className="preview-table-wrapper">
                <table className="preview-data-table">
                  <thead>
                    <tr>
                      {previewData.headers.map((header, i) => (
                        <th key={i}>{header}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {previewData.rows.map((row, rowIndex) => (
                      <tr key={rowIndex}>
                        {row.map((cell, cellIndex) => (
                          <td key={cellIndex}>{cell}</td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn-secondary" onClick={() => setSelectedReport(null)}>
                Close
              </button>
              <button className="btn-edit" onClick={() => handleEdit(selectedReport)}>
                Edit Report
              </button>
              <div className="export-dropdown">
                <button
                  className="btn-primary"
                  onClick={(e) => toggleExportMenu(selectedReport.id, e)}
                  disabled={isExporting === selectedReport.id}
                >
                  {isExporting === selectedReport.id ? 'Exporting...' : 'Run Export'}
                  <span className="dropdown-arrow">&#9662;</span>
                </button>
                {exportMenuOpen === selectedReport.id && (
                  <div className="export-menu">
                    <button onClick={(e) => handleExport(selectedReport, 'csv', e)}>
                      Export as CSV
                    </button>
                    <button onClick={(e) => handleExport(selectedReport, 'xlsx', e)}>
                      Export as Excel
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {showEditor && (
        <ReportEditor
          report={editingReport}
          onSave={handleSave}
          onClose={() => {
            setShowEditor(false);
            setEditingReport(null);
          }}
        />
      )}
    </div>
  );
}
