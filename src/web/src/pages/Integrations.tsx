import { useState, useMemo, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { integrationsApi, reportsApi, propertiesApi, residentsApi } from '../services/api';
import type {
  AccountingSoftware,
  ClientAccountingConnection,
  ReportSchedule,
  CreateScheduleRequest,
  ScheduleFrequency,
  Property,
  BulkResidentInput,
  ResidentImportResponse,
} from '../types';

type TabType = 'schedules' | 'connections' | 'resident-import';
type ScheduleSortColumn = 'name' | 'reportName' | 'destination' | 'frequency' | 'lastRunAt' | 'nextRunAt' | 'status';
type SortDirection = 'asc' | 'desc';

export default function Integrations() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<TabType>('schedules');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState<ReportSchedule | null>(null);
  const [showHistoryModal, setShowHistoryModal] = useState(false);
  const [sortColumn, setSortColumn] = useState<ScheduleSortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  // Fetch data
  const { data: softwareData } = useQuery({
    queryKey: ['accounting-software'],
    queryFn: () => integrationsApi.listSoftware(),
  });

  const { data: connectionsData, isLoading: connectionsLoading } = useQuery({
    queryKey: ['connections'],
    queryFn: () => integrationsApi.listConnections(),
  });

  const { data: schedulesData, isLoading: schedulesLoading } = useQuery({
    queryKey: ['schedules'],
    queryFn: () => integrationsApi.listSchedules(),
  });

  const { data: reportsData } = useQuery({
    queryKey: ['reports', 'for-schedule'],
    queryFn: () => reportsApi.list({ pageSize: 100 }),
  });

  const { data: runHistoryData } = useQuery({
    queryKey: ['schedule-runs', selectedSchedule?.id],
    queryFn: () => selectedSchedule ? integrationsApi.listScheduleRuns(selectedSchedule.id) : null,
    enabled: !!selectedSchedule && showHistoryModal,
  });

  // Mutations
  const toggleMutation = useMutation({
    mutationFn: (id: string) => integrationsApi.toggleSchedule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] });
    },
  });

  const runNowMutation = useMutation({
    mutationFn: (id: string) => integrationsApi.triggerScheduleRun(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] });
      queryClient.invalidateQueries({ queryKey: ['schedule-runs'] });
    },
  });

  const deleteScheduleMutation = useMutation({
    mutationFn: (id: string) => integrationsApi.deleteSchedule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] });
    },
  });

  const testConnectionMutation = useMutation({
    mutationFn: (id: string) => integrationsApi.testConnection(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['connections'] });
    },
  });

  const software = softwareData?.software ?? [];
  const connections = connectionsData?.connections ?? [];
  const schedules = schedulesData?.schedules ?? [];
  const reports = reportsData?.reports ?? [];

  const formatFrequency = (schedule: ReportSchedule) => {
    const time = schedule.timeOfDay ? schedule.timeOfDay.substring(0, 5) : '00:00';
    switch (schedule.frequency) {
      case 'Daily':
        return `Daily at ${time}`;
      case 'Weekly':
        const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        return `${days[schedule.dayOfWeek ?? 0]} at ${time}`;
      case 'Monthly':
        return `${schedule.dayOfMonth}${getOrdinalSuffix(schedule.dayOfMonth ?? 1)} of month at ${time}`;
      case 'Once':
        return `Once on ${new Date(schedule.startDate).toLocaleDateString()}`;
      default:
        return schedule.frequency;
    }
  };

  const getOrdinalSuffix = (n: number) => {
    const s = ['th', 'st', 'nd', 'rd'];
    const v = n % 100;
    return s[(v - 20) % 10] || s[v] || s[0];
  };

  const formatDate = (dateStr?: string, schedule?: ReportSchedule) => {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    // If we have a schedule with timeOfDay, use that for display consistency
    // since timeOfDay represents the intended local time
    if (schedule?.timeOfDay) {
      const [hours, minutes] = schedule.timeOfDay.split(':').map(Number);
      const localDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), hours, minutes, 0);
      return localDate.toLocaleString();
    }
    return date.toLocaleString();
  };

  const getStatusBadge = (status?: string, isActive?: boolean) => {
    if (isActive === false) {
      return <span className="status-badge paused">Paused</span>;
    }
    if (!status) return <span className="status-badge pending">Pending</span>;
    const statusLower = status.toLowerCase();
    const className = statusLower === 'success' ? 'success' : statusLower === 'failed' ? 'error' : 'pending';
    return <span className={`status-badge ${className}`}>{status}</span>;
  };

  const handleSort = (column: ScheduleSortColumn) => {
    if (sortColumn === column) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  const SortIndicator = ({ column }: { column: ScheduleSortColumn }) => {
    if (sortColumn !== column) {
      return <span className="sort-indicator inactive">⇅</span>;
    }
    return <span className="sort-indicator active">{sortDirection === 'asc' ? '↑' : '↓'}</span>;
  };

  const sortedSchedules = useMemo(() => {
    if (!sortColumn) return schedules;
    return [...schedules].sort((a, b) => {
      let aVal: string | number | boolean | null;
      let bVal: string | number | boolean | null;
      switch (sortColumn) {
        case 'name':
          aVal = a.name.toLowerCase();
          bVal = b.name.toLowerCase();
          break;
        case 'reportName':
          aVal = (a.reportName || '').toLowerCase();
          bVal = (b.reportName || '').toLowerCase();
          break;
        case 'destination':
          aVal = (a.accountingSoftwareName || '').toLowerCase();
          bVal = (b.accountingSoftwareName || '').toLowerCase();
          break;
        case 'frequency':
          aVal = a.frequency.toLowerCase();
          bVal = b.frequency.toLowerCase();
          break;
        case 'lastRunAt':
          aVal = a.lastRunAt || '';
          bVal = b.lastRunAt || '';
          break;
        case 'nextRunAt':
          aVal = a.nextRunAt || '';
          bVal = b.nextRunAt || '';
          break;
        case 'status':
          aVal = a.isActive ? (a.lastRunStatus || 'Pending') : 'Paused';
          bVal = b.isActive ? (b.lastRunStatus || 'Pending') : 'Paused';
          break;
        default:
          return 0;
      }
      if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
      if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [schedules, sortColumn, sortDirection]);

  return (
    <div className="integrations-page">
      <div className="page-header">
        <h1>Integrations</h1>
        <p className="page-subtitle">Schedule reports to push to client accounting software</p>
      </div>

      <div className="tabs">
        <button
          className={`tab ${activeTab === 'schedules' ? 'active' : ''}`}
          onClick={() => setActiveTab('schedules')}
        >
          Scheduled Jobs ({schedules.length})
        </button>
        <button
          className={`tab ${activeTab === 'connections' ? 'active' : ''}`}
          onClick={() => setActiveTab('connections')}
        >
          Connections ({connections.length})
        </button>
        <button
          className={`tab ${activeTab === 'resident-import' ? 'active' : ''}`}
          onClick={() => setActiveTab('resident-import')}
        >
          Resident Import
        </button>
      </div>

      {activeTab === 'schedules' && (
        <div className="schedules-section">
          <div className="section-header">
            <h2>Report Schedules</h2>
            <button className="btn-primary" onClick={() => setShowCreateModal(true)}>
              + New Schedule
            </button>
          </div>

          {schedulesLoading ? (
            <div className="loading">Loading schedules...</div>
          ) : schedules.length === 0 ? (
            <div className="empty-state">
              <p>No scheduled jobs yet</p>
              <p className="empty-subtitle">Create a schedule to automatically push reports to accounting software</p>
            </div>
          ) : (
            <div className="schedules-table-container">
              <table className="schedules-table">
                <thead>
                  <tr>
                    <th className="sortable" onClick={() => handleSort('name')}>
                      Schedule Name <SortIndicator column="name" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('reportName')}>
                      Report <SortIndicator column="reportName" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('destination')}>
                      Destination <SortIndicator column="destination" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('frequency')}>
                      Frequency <SortIndicator column="frequency" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('lastRunAt')}>
                      Last Run <SortIndicator column="lastRunAt" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('nextRunAt')}>
                      Next Run <SortIndicator column="nextRunAt" />
                    </th>
                    <th className="sortable" onClick={() => handleSort('status')}>
                      Status <SortIndicator column="status" />
                    </th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedSchedules.map((schedule) => (
                    <tr key={schedule.id} className={!schedule.isActive ? 'inactive' : ''}>
                      <td className="schedule-name">
                        <span className="name">{schedule.name}</span>
                        {schedule.description && (
                          <span className="description">{schedule.description}</span>
                        )}
                      </td>
                      <td>{schedule.reportName}</td>
                      <td>
                        <span className="destination">
                          {schedule.accountingSoftwareName}
                          <span className="connection-name">{schedule.connectionName}</span>
                        </span>
                      </td>
                      <td>{formatFrequency(schedule)}</td>
                      <td>{formatDate(schedule.lastRunAt, schedule)}</td>
                      <td>{schedule.isActive ? formatDate(schedule.nextRunAt, schedule) : '-'}</td>
                      <td>{getStatusBadge(schedule.lastRunStatus, schedule.isActive)}</td>
                      <td className="actions">
                        <button
                          className={`btn-icon ${schedule.isActive ? 'active' : ''}`}
                          onClick={() => toggleMutation.mutate(schedule.id)}
                          title={schedule.isActive ? 'Pause' : 'Resume'}
                        >
                          {schedule.isActive ? '⏸' : '▶'}
                        </button>
                        <button
                          className="btn-icon"
                          onClick={() => runNowMutation.mutate(schedule.id)}
                          title="Run Now"
                          disabled={runNowMutation.isPending}
                        >
                          ⚡
                        </button>
                        <button
                          className="btn-icon"
                          onClick={() => {
                            setSelectedSchedule(schedule);
                            setShowHistoryModal(true);
                          }}
                          title="View History"
                        >
                          📋
                        </button>
                        <button
                          className="btn-icon danger"
                          onClick={() => {
                            if (confirm('Are you sure you want to delete this schedule?')) {
                              deleteScheduleMutation.mutate(schedule.id);
                            }
                          }}
                          title="Delete"
                        >
                          🗑
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {activeTab === 'connections' && (
        <div className="connections-section">
          <div className="section-header">
            <h2>Accounting Connections</h2>
          </div>

          {connectionsLoading ? (
            <div className="loading">Loading connections...</div>
          ) : connections.length === 0 ? (
            <div className="empty-state">
              <p>No connections configured</p>
              <p className="empty-subtitle">Contact your administrator to set up accounting software connections</p>
            </div>
          ) : (
            <div className="connections-grid">
              {connections.map((connection) => (
                <div key={connection.id} className={`connection-card ${!connection.isActive ? 'inactive' : ''}`}>
                  <div className="connection-header">
                    <div className="software-info">
                      <span className="software-name">{connection.accountingSoftwareName}</span>
                      <span className="connection-type">{connection.connectionType}</span>
                    </div>
                    <span className={`status-indicator ${connection.isActive ? 'active' : 'inactive'}`}>
                      {connection.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                  <div className="connection-body">
                    <h3>{connection.name}</h3>
                    <div className="test-status">
                      <span className="label">Last Test:</span>
                      <span className={`value ${connection.lastTestStatus?.toLowerCase()}`}>
                        {connection.lastTestStatus || 'Never'} {connection.lastTestedAt && `- ${formatDate(connection.lastTestedAt)}`}
                      </span>
                    </div>
                  </div>
                  <div className="connection-footer">
                    <button
                      className="btn-secondary"
                      onClick={() => testConnectionMutation.mutate(connection.id)}
                      disabled={testConnectionMutation.isPending}
                    >
                      Test Connection
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {software.length > 0 && (
            <div className="software-list">
              <h3>Supported Accounting Software</h3>
              <div className="software-grid">
                {software.map((sw) => (
                  <div key={sw.id} className="software-item">
                    <span className="software-name">{sw.name}</span>
                    <span className="software-type">{sw.connectionType}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {activeTab === 'resident-import' && (
        <ResidentImportTab />
      )}

      {/* Create Schedule Modal */}
      {showCreateModal && (
        <CreateScheduleModal
          reports={reports}
          connections={connections}
          onClose={() => setShowCreateModal(false)}
          onCreated={() => {
            setShowCreateModal(false);
            queryClient.invalidateQueries({ queryKey: ['schedules'] });
          }}
        />
      )}

      {/* History Modal */}
      {showHistoryModal && selectedSchedule && (
        <div className="modal-overlay" onClick={() => setShowHistoryModal(false)}>
          <div className="modal history-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Run History: {selectedSchedule.name}</h2>
              <button className="close-btn" onClick={() => setShowHistoryModal(false)}>&times;</button>
            </div>
            <div className="modal-body">
              {runHistoryData?.runs?.length === 0 ? (
                <div className="empty-state">
                  <p>No run history yet</p>
                </div>
              ) : (
                <table className="history-table">
                  <thead>
                    <tr>
                      <th>Started</th>
                      <th>Completed</th>
                      <th>Status</th>
                      <th>Records</th>
                      <th>Failed</th>
                      <th>Error</th>
                    </tr>
                  </thead>
                  <tbody>
                    {runHistoryData?.runs?.map((run) => (
                      <tr key={run.id}>
                        <td>{formatDate(run.startedAt)}</td>
                        <td>{formatDate(run.completedAt)}</td>
                        <td>{getStatusBadge(run.status)}</td>
                        <td>{run.recordsProcessed}</td>
                        <td>{run.recordsFailed}</td>
                        <td className="error-cell">{run.errorMessage || '-'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// Create Schedule Modal Component
function CreateScheduleModal({
  reports,
  connections,
  onClose,
  onCreated,
}: {
  reports: Array<{ id: string; name: string }>;
  connections: ClientAccountingConnection[];
  onClose: () => void;
  onCreated: () => void;
}) {
  const [formData, setFormData] = useState<CreateScheduleRequest>({
    reportDefinitionId: '',
    connectionId: '',
    name: '',
    description: '',
    frequency: 'Daily',
    timeOfDay: '06:00',
    exportFormat: 'csv',
    isActive: true,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateScheduleRequest) => integrationsApi.createSchedule(data),
    onSuccess: onCreated,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate(formData);
  };

  const frequencyOptions: ScheduleFrequency[] = ['Once', 'Daily', 'Weekly', 'Monthly'];

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal create-schedule-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Create Schedule</h2>
          <button className="close-btn" onClick={onClose}>&times;</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            <div className="form-group">
              <label>Schedule Name *</label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Daily Utility Push"
                required
              />
            </div>

            <div className="form-group">
              <label>Description</label>
              <input
                type="text"
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Automatically push utility bills to Yardi"
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Report *</label>
                <select
                  value={formData.reportDefinitionId}
                  onChange={(e) => setFormData({ ...formData, reportDefinitionId: e.target.value })}
                  required
                >
                  <option value="">Select a report...</option>
                  {reports.map((report) => (
                    <option key={report.id} value={report.id}>{report.name}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Connection *</label>
                <select
                  value={formData.connectionId}
                  onChange={(e) => setFormData({ ...formData, connectionId: e.target.value })}
                  required
                >
                  <option value="">Select a connection...</option>
                  {connections.filter(c => c.isActive).map((conn) => (
                    <option key={conn.id} value={conn.id}>
                      {conn.name} ({conn.accountingSoftwareName})
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Frequency *</label>
                <select
                  value={formData.frequency}
                  onChange={(e) => setFormData({ ...formData, frequency: e.target.value as ScheduleFrequency })}
                >
                  {frequencyOptions.map((freq) => (
                    <option key={freq} value={freq}>{freq}</option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Time of Day</label>
                <input
                  type="time"
                  value={formData.timeOfDay || '06:00'}
                  onChange={(e) => setFormData({ ...formData, timeOfDay: e.target.value })}
                />
              </div>
            </div>

            {formData.frequency === 'Weekly' && (
              <div className="form-group">
                <label>Day of Week</label>
                <select
                  value={formData.dayOfWeek ?? 1}
                  onChange={(e) => setFormData({ ...formData, dayOfWeek: parseInt(e.target.value) })}
                >
                  <option value={0}>Sunday</option>
                  <option value={1}>Monday</option>
                  <option value={2}>Tuesday</option>
                  <option value={3}>Wednesday</option>
                  <option value={4}>Thursday</option>
                  <option value={5}>Friday</option>
                  <option value={6}>Saturday</option>
                </select>
              </div>
            )}

            {formData.frequency === 'Monthly' && (
              <div className="form-group">
                <label>Day of Month</label>
                <input
                  type="number"
                  min={1}
                  max={28}
                  value={formData.dayOfMonth ?? 1}
                  onChange={(e) => setFormData({ ...formData, dayOfMonth: parseInt(e.target.value) })}
                />
              </div>
            )}

            {formData.frequency === 'Once' && (
              <div className="form-group">
                <label>Run Date</label>
                <input
                  type="date"
                  value={formData.startDate || ''}
                  onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                />
              </div>
            )}

            <div className="form-row">
              <div className="form-group">
                <label>Export Format</label>
                <select
                  value={formData.exportFormat}
                  onChange={(e) => setFormData({ ...formData, exportFormat: e.target.value })}
                >
                  <option value="csv">CSV</option>
                  <option value="xlsx">Excel</option>
                  <option value="json">JSON</option>
                </select>
              </div>

              <div className="form-group">
                <label>Destination Table (optional)</label>
                <input
                  type="text"
                  value={formData.destinationTable || ''}
                  onChange={(e) => setFormData({ ...formData, destinationTable: e.target.value })}
                  placeholder="UtilityCharges"
                />
              </div>
            </div>

            {createMutation.isError && (
              <div className="error-message">
                Failed to create schedule. Please try again.
              </div>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating...' : 'Create Schedule'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// Resident Import Tab Component
type ImportStep = 'select-property' | 'upload' | 'preview' | 'results';

function ResidentImportTab() {
  const queryClient = useQueryClient();
  const [step, setStep] = useState<ImportStep>('select-property');
  const [selectedProperty, setSelectedProperty] = useState<Property | null>(null);
  const [parsedData, setParsedData] = useState<BulkResidentInput[]>([]);
  const [importResult, setImportResult] = useState<ResidentImportResponse | null>(null);
  const [updateExisting, setUpdateExisting] = useState(true);
  const [parseErrors, setParseErrors] = useState<string[]>([]);
  const [propertySearch, setPropertySearch] = useState('');

  const { data: propertiesData, isLoading: propertiesLoading } = useQuery({
    queryKey: ['properties', 'for-import'],
    queryFn: () => propertiesApi.list({ pageSize: 500 }),
  });

  const importMutation = useMutation({
    mutationFn: residentsApi.import,
    onSuccess: (data) => {
      setImportResult(data);
      setStep('results');
      queryClient.invalidateQueries({ queryKey: ['residents'] });
    },
  });

  const pushMutation = useMutation({
    mutationFn: residentsApi.push,
  });

  const properties = propertiesData?.properties ?? [];

  // Filter properties based on search
  const filteredProperties = useMemo(() => {
    if (!propertySearch.trim()) return properties;
    const searchLower = propertySearch.toLowerCase();
    return properties.filter(p =>
      p.propertyName.toLowerCase().includes(searchLower) ||
      p.propertyAddress.toLowerCase().includes(searchLower) ||
      p.propertyClientId.toLowerCase().includes(searchLower) ||
      (p.propertyCity && p.propertyCity.toLowerCase().includes(searchLower)) ||
      (p.propertyCode && p.propertyCode.toLowerCase().includes(searchLower))
    );
  }, [properties, propertySearch]);

  const handlePropertySelect = (property: Property) => {
    setSelectedProperty(property);
    setStep('upload');
  };

  const parseCSV = useCallback((text: string): { data: BulkResidentInput[]; errors: string[] } => {
    const lines = text.split('\n').map(line => line.trim()).filter(line => line);
    if (lines.length < 2) {
      return { data: [], errors: ['File must have a header row and at least one data row'] };
    }

    const headers = lines[0].toLowerCase().split(',').map(h => h.trim().replace(/^["']|["']$/g, ''));
    const data: BulkResidentInput[] = [];
    const errors: string[] = [];

    // Map common header variations
    const headerMap: Record<string, keyof BulkResidentInput> = {
      'external_id': 'externalId',
      'externalid': 'externalId',
      'id': 'externalId',
      'first_name': 'firstName',
      'firstname': 'firstName',
      'first': 'firstName',
      'last_name': 'lastName',
      'lastname': 'lastName',
      'last': 'lastName',
      'middle_name': 'middleName',
      'middlename': 'middleName',
      'middle': 'middleName',
      'email': 'email',
      'phone': 'phone',
      'phone_number': 'phone',
      'alternate_phone': 'alternatePhone',
      'alternatephone': 'alternatePhone',
      'unit': 'unitNumber',
      'unit_number': 'unitNumber',
      'unitnumber': 'unitNumber',
      'apartment': 'unitNumber',
      'lease_start': 'leaseStart',
      'leasestart': 'leaseStart',
      'move_in': 'leaseStart',
      'movein': 'leaseStart',
      'lease_end': 'leaseEnd',
      'leaseend': 'leaseEnd',
      'move_out': 'leaseEnd',
      'moveout': 'leaseEnd',
      'monthly_rent': 'monthlyRent',
      'monthlyrent': 'monthlyRent',
      'rent': 'monthlyRent',
      'address': 'address',
      'city': 'city',
      'state': 'state',
      'zip': 'zipCode',
      'zip_code': 'zipCode',
      'zipcode': 'zipCode',
      'status': 'status',
      'type': 'type',
    };

    const columnIndexes: Record<keyof BulkResidentInput, number> = {} as Record<keyof BulkResidentInput, number>;
    headers.forEach((header, index) => {
      const mappedField = headerMap[header];
      if (mappedField) {
        columnIndexes[mappedField] = index;
      }
    });

    // Check required fields
    if (columnIndexes.externalId === undefined) {
      errors.push('Missing required column: external_id (or id)');
    }
    if (columnIndexes.firstName === undefined) {
      errors.push('Missing required column: first_name (or firstname)');
    }
    if (columnIndexes.lastName === undefined) {
      errors.push('Missing required column: last_name (or lastname)');
    }

    if (errors.length > 0) {
      return { data: [], errors };
    }

    // Parse data rows
    for (let i = 1; i < lines.length; i++) {
      const values = parseCSVLine(lines[i]);

      try {
        const resident: BulkResidentInput = {
          externalId: values[columnIndexes.externalId]?.trim() || '',
          firstName: values[columnIndexes.firstName]?.trim() || '',
          lastName: values[columnIndexes.lastName]?.trim() || '',
          middleName: columnIndexes.middleName !== undefined ? values[columnIndexes.middleName]?.trim() : undefined,
          email: columnIndexes.email !== undefined ? values[columnIndexes.email]?.trim() : undefined,
          phone: columnIndexes.phone !== undefined ? values[columnIndexes.phone]?.trim() : undefined,
          alternatePhone: columnIndexes.alternatePhone !== undefined ? values[columnIndexes.alternatePhone]?.trim() : undefined,
          unitNumber: columnIndexes.unitNumber !== undefined ? values[columnIndexes.unitNumber]?.trim() : undefined,
          leaseStart: columnIndexes.leaseStart !== undefined ? values[columnIndexes.leaseStart]?.trim() : undefined,
          leaseEnd: columnIndexes.leaseEnd !== undefined ? values[columnIndexes.leaseEnd]?.trim() : undefined,
          monthlyRent: columnIndexes.monthlyRent !== undefined ? parseFloat(values[columnIndexes.monthlyRent]) || undefined : undefined,
          address: columnIndexes.address !== undefined ? values[columnIndexes.address]?.trim() : undefined,
          city: columnIndexes.city !== undefined ? values[columnIndexes.city]?.trim() : undefined,
          state: columnIndexes.state !== undefined ? values[columnIndexes.state]?.trim() : undefined,
          zipCode: columnIndexes.zipCode !== undefined ? values[columnIndexes.zipCode]?.trim() : undefined,
          status: columnIndexes.status !== undefined ? values[columnIndexes.status]?.trim() : undefined,
          type: columnIndexes.type !== undefined ? values[columnIndexes.type]?.trim() : undefined,
        };

        if (!resident.externalId) {
          errors.push(`Row ${i + 1}: Missing external_id`);
          continue;
        }
        if (!resident.firstName) {
          errors.push(`Row ${i + 1}: Missing first_name`);
          continue;
        }
        if (!resident.lastName) {
          errors.push(`Row ${i + 1}: Missing last_name`);
          continue;
        }

        data.push(resident);
      } catch (e) {
        errors.push(`Row ${i + 1}: ${e instanceof Error ? e.message : 'Parse error'}`);
      }
    }

    return { data, errors };
  }, []);

  const handleFileUpload = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      const { data, errors } = parseCSV(text);
      setParsedData(data);
      setParseErrors(errors);
      if (data.length > 0) {
        setStep('preview');
      }
    };
    reader.readAsText(file);
  }, [parseCSV]);

  const handleImport = () => {
    if (!selectedProperty || parsedData.length === 0) return;

    importMutation.mutate({
      propertyId: selectedProperty.propertyId,
      residents: parsedData,
      updateExisting,
    });
  };

  const handlePushToConservice = () => {
    if (!selectedProperty || !importResult) return;
    pushMutation.mutate({ propertyId: selectedProperty.propertyId });
  };

  const resetImport = () => {
    setStep('select-property');
    setSelectedProperty(null);
    setParsedData([]);
    setImportResult(null);
    setParseErrors([]);
  };

  const downloadTemplate = () => {
    const headers = 'external_id,first_name,last_name,email,phone,unit_number,lease_start,lease_end,monthly_rent,status';
    const example = 'RES-001,John,Smith,john@email.com,555-1234,101A,2024-01-01,2025-01-01,1500,Active';
    const content = `${headers}\n${example}`;
    const blob = new Blob([content], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'resident_import_template.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="resident-import-section">
      <div className="section-header">
        <h2>Resident Import</h2>
        <p className="section-subtitle">Import resident data for an apartment complex</p>
      </div>

      {/* Progress Steps */}
      <div className="import-steps">
        <div className={`step ${step === 'select-property' ? 'active' : step !== 'select-property' ? 'completed' : ''}`}>
          <span className="step-number">1</span>
          <span className="step-label">Select Property</span>
        </div>
        <div className={`step ${step === 'upload' ? 'active' : ['preview', 'results'].includes(step) ? 'completed' : ''}`}>
          <span className="step-number">2</span>
          <span className="step-label">Upload File</span>
        </div>
        <div className={`step ${step === 'preview' ? 'active' : step === 'results' ? 'completed' : ''}`}>
          <span className="step-number">3</span>
          <span className="step-label">Preview</span>
        </div>
        <div className={`step ${step === 'results' ? 'active' : ''}`}>
          <span className="step-number">4</span>
          <span className="step-label">Results</span>
        </div>
      </div>

      {/* Step 1: Property Selection */}
      {step === 'select-property' && (
        <div className="import-content">
          <h3>Select Property</h3>
          <p>Choose the apartment complex to import residents for</p>

          {propertiesLoading ? (
            <div className="loading">Loading properties...</div>
          ) : properties.length === 0 ? (
            <div className="empty-state">
              <p>No properties found</p>
              <p className="empty-subtitle">Create a property first before importing residents</p>
            </div>
          ) : (
            <>
              <div className="property-search">
                <input
                  type="text"
                  placeholder="Search properties by name, address, city, or code..."
                  value={propertySearch}
                  onChange={(e) => setPropertySearch(e.target.value)}
                  className="property-search-input"
                />
                {propertySearch && (
                  <span className="search-results-count">
                    {filteredProperties.length} of {properties.length} properties
                  </span>
                )}
              </div>
              {filteredProperties.length === 0 ? (
                <div className="empty-state">
                  <p>No properties match your search</p>
                  <button className="btn-link" onClick={() => setPropertySearch('')}>Clear search</button>
                </div>
              ) : (
                <div className="property-select-grid">
                  {filteredProperties.map((property) => (
                    <div
                      key={property.propertyId}
                      className="property-select-card"
                      onClick={() => handlePropertySelect(property)}
                    >
                      <h4>{property.propertyName}</h4>
                      <p className="property-address">{property.propertyAddress}</p>
                      {property.propertyCity && (
                        <p className="property-location">
                          {property.propertyCity}, {property.propertyState} {property.propertyZipCode}
                        </p>
                      )}
                      <span className="property-type">{property.propertyType}</span>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      )}

      {/* Step 2: File Upload */}
      {step === 'upload' && selectedProperty && (
        <div className="import-content">
          <div className="selected-property-banner">
            <strong>{selectedProperty.propertyName}</strong>
            <button className="btn-link" onClick={() => setStep('select-property')}>Change</button>
          </div>

          <h3>Upload Resident File</h3>
          <p>Upload a CSV or Excel file with resident data</p>

          <div className="upload-area">
            <div className="upload-box">
              <input
                type="file"
                accept=".csv,.xlsx,.xls"
                onChange={handleFileUpload}
                id="resident-file-upload"
              />
              <label htmlFor="resident-file-upload">
                <span className="upload-icon">📁</span>
                <span className="upload-text">Drop file here or click to browse</span>
                <span className="upload-hint">Supports CSV and Excel files</span>
              </label>
            </div>

            <div className="upload-options">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={updateExisting}
                  onChange={(e) => setUpdateExisting(e.target.checked)}
                />
                Update existing residents (match by External ID)
              </label>
            </div>

            <button className="btn-secondary" onClick={downloadTemplate}>
              Download Template
            </button>
          </div>

          {parseErrors.length > 0 && (
            <div className="parse-errors">
              <h4>Parse Errors</h4>
              <ul>
                {parseErrors.slice(0, 10).map((error, i) => (
                  <li key={i}>{error}</li>
                ))}
                {parseErrors.length > 10 && (
                  <li>...and {parseErrors.length - 10} more errors</li>
                )}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* Step 3: Preview */}
      {step === 'preview' && selectedProperty && (
        <div className="import-content">
          <div className="selected-property-banner">
            <strong>{selectedProperty.propertyName}</strong>
            <span className="resident-count">{parsedData.length} residents to import</span>
          </div>

          <h3>Preview Import Data</h3>

          <div className="preview-table-container">
            <table className="preview-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>External ID</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Unit</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {parsedData.slice(0, 50).map((resident, i) => (
                  <tr key={i}>
                    <td>{i + 1}</td>
                    <td>{resident.externalId}</td>
                    <td>{resident.firstName} {resident.lastName}</td>
                    <td>{resident.email || '-'}</td>
                    <td>{resident.phone || '-'}</td>
                    <td>{resident.unitNumber || '-'}</td>
                    <td>{resident.status || 'Active'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {parsedData.length > 50 && (
              <p className="preview-note">Showing first 50 of {parsedData.length} records</p>
            )}
          </div>

          <div className="preview-actions">
            <button className="btn-secondary" onClick={() => setStep('upload')}>
              Back
            </button>
            <button
              className="btn-primary"
              onClick={handleImport}
              disabled={importMutation.isPending}
            >
              {importMutation.isPending ? 'Importing...' : `Import ${parsedData.length} Residents`}
            </button>
          </div>

          {importMutation.isError && (
            <div className="error-message">
              Import failed. Please check your data and try again.
            </div>
          )}
        </div>
      )}

      {/* Step 4: Results */}
      {step === 'results' && importResult && (
        <div className="import-content">
          <div className="import-results">
            <h3>Import Complete</h3>

            <div className="results-summary">
              <div className="result-stat success">
                <span className="stat-value">{importResult.created}</span>
                <span className="stat-label">Created</span>
              </div>
              <div className="result-stat info">
                <span className="stat-value">{importResult.updated}</span>
                <span className="stat-label">Updated</span>
              </div>
              <div className="result-stat error">
                <span className="stat-value">{importResult.failed}</span>
                <span className="stat-label">Failed</span>
              </div>
            </div>

            {importResult.errors.length > 0 && (
              <div className="import-errors">
                <h4>Errors</h4>
                <ul>
                  {importResult.errors.slice(0, 20).map((error, i) => (
                    <li key={i}>
                      Row {error.index + 1}{error.externalId && ` (${error.externalId})`}: {error.message}
                    </li>
                  ))}
                  {importResult.errors.length > 20 && (
                    <li>...and {importResult.errors.length - 20} more errors</li>
                  )}
                </ul>
              </div>
            )}

            <div className="results-actions">
              <button className="btn-secondary" onClick={resetImport}>
                Import More
              </button>
              <button
                className="btn-primary"
                onClick={handlePushToConservice}
                disabled={pushMutation.isPending}
              >
                {pushMutation.isPending ? 'Pushing...' : 'Push to Conservice'}
              </button>
            </div>

            {pushMutation.isSuccess && (
              <div className="success-message">
                Successfully pushed {pushMutation.data?.residentCount} residents to Conservice
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

// Helper function to parse CSV line with proper quote handling
function parseCSVLine(line: string): string[] {
  const result: string[] = [];
  let current = '';
  let inQuotes = false;

  for (let i = 0; i < line.length; i++) {
    const char = line[i];

    if (char === '"') {
      if (inQuotes && line[i + 1] === '"') {
        current += '"';
        i++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (char === ',' && !inQuotes) {
      result.push(current);
      current = '';
    } else {
      current += char;
    }
  }
  result.push(current);

  return result;
}
