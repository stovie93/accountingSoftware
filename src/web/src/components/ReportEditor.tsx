import React, { useState, useEffect, useMemo } from 'react';
import type {
  Report,
  ReportType,
  ReportSourceType,
  ReportConfiguration,
  ReportColumnMapping,
  ReportStaticColumn,
  ReportAutoIncrement,
} from '../types';

interface ReportEditorProps {
  report: Report | null;
  onSave: (data: { name: string; type: ReportType; configuration: string; isShared: boolean }) => Promise<void>;
  onClose: () => void;
}

// Sample data for preview
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
    { providerId: 'pr001', providerAccountId: 'DPL-001', providerName: 'Denver Power & Light', providerCode: 'DPL', providerDescription: 'Electric utility provider', providerType: 'Electric', providerAddress: '123 Power Way', providerCity: 'Denver', providerState: 'CO', providerZipCode: '80202', providerCountry: 'USA', providerPhone: '(303) 555-0100', providerEmail: 'billing@denverpower.com', providerWebsite: 'https://denverpower.com', providerContactName: 'John Smith', providerAccountNumber: 'ACC-DPL-12345' },
    { providerId: 'pr002', providerAccountId: 'DWA-001', providerName: 'Denver Water Authority', providerCode: 'DWA', providerDescription: 'Municipal water provider', providerType: 'Water', providerAddress: '456 Water St', providerCity: 'Denver', providerState: 'CO', providerZipCode: '80203', providerCountry: 'USA', providerPhone: '(303) 555-0200', providerEmail: 'service@denverwater.gov', providerWebsite: 'https://denverwater.gov', providerContactName: 'Jane Doe', providerAccountNumber: 'ACC-DWA-67890' },
    { providerId: 'pr003', providerAccountId: 'CNG-001', providerName: 'Colorado Natural Gas', providerCode: 'CNG', providerDescription: 'Natural gas provider', providerType: 'Gas', providerAddress: '789 Gas Blvd', providerCity: 'Denver', providerState: 'CO', providerZipCode: '80204', providerCountry: 'USA', providerPhone: '(303) 555-0300', providerEmail: 'support@coloradogas.com', providerWebsite: 'https://coloradogas.com', providerContactName: 'Bob Wilson', providerAccountNumber: 'ACC-CNG-11111' },
  ],
  utilities: [
    { utilityId: 'u001', utilityName: 'Electric', utilityType: 'Electric', utilityCode: 'ELEC', utilityDescription: 'Electricity service' },
    { utilityId: 'u002', utilityName: 'Water', utilityType: 'Water', utilityCode: 'WTR', utilityDescription: 'Water service' },
    { utilityId: 'u003', utilityName: 'Natural Gas', utilityType: 'Gas', utilityCode: 'GAS', utilityDescription: 'Natural gas service' },
  ],
  items: [
    { itemId: 'i001', externalId: 'INV-OAK-ELE-202601', date: '2026-01-01', amount: 1250.00, quantity: 1042, description: 'Electric bill for oakwood-apts - January 2026', source: 'Pipeline', entities: 'Oakwood Apartments, Electric, Denver Power & Light' },
    { itemId: 'i002', externalId: 'INV-MAP-WTR-202601', date: '2026-01-01', amount: 485.00, quantity: 12125, description: 'Water bill for maplegrove - January 2026', source: 'Pipeline', entities: 'Maplegrove Complex, Water, Denver Water Authority' },
    { itemId: 'i003', externalId: 'INV-PIN-GAS-202601', date: '2026-01-01', amount: 890.00, quantity: 742, description: 'Gas bill for pinecrest - January 2026', source: 'Pipeline', entities: 'Pinecrest Towers, Gas, Colorado Natural Gas' },
  ],
};

// Available source fields for each data type
const SOURCE_FIELDS: Record<ReportSourceType, { value: string; label: string }[]> = {
  bills: [
    { value: 'billId', label: 'Bill ID' },
    { value: 'billReferenceId', label: 'Reference ID' },
    { value: 'billExternalId', label: 'External ID' },
    { value: 'billAmount', label: 'Amount' },
    { value: 'billTotalAmount', label: 'Total Amount' },
    { value: 'billStatus', label: 'Status' },
    { value: 'billDate', label: 'Bill Date' },
    { value: 'billDueDate', label: 'Due Date' },
    { value: 'billPaidDate', label: 'Paid Date' },
    { value: 'billDescription', label: 'Description' },
    { value: 'billQuantity', label: 'Quantity' },
    { value: 'billUnit', label: 'Unit' },
    { value: 'billRate', label: 'Rate' },
    { value: 'billPeriodStart', label: 'Period Start' },
    { value: 'billPeriodEnd', label: 'Period End' },
    { value: 'billCategory', label: 'Category' },
    { value: 'billTax', label: 'Tax' },
    { value: 'billNotes', label: 'Notes' },
    { value: 'property.propertyName', label: 'Property Name' },
    { value: 'property.propertyCode', label: 'Property Code' },
    { value: 'property.propertyAddress', label: 'Property Address' },
    { value: 'provider.providerName', label: 'Provider Name' },
    { value: 'provider.providerCode', label: 'Provider Code' },
    { value: 'utility.utilityName', label: 'Utility Name' },
    { value: 'utility.utilityType', label: 'Utility Type' },
  ],
  properties: [
    { value: 'propertyId', label: 'Property ID' },
    { value: 'propertyClientId', label: 'Client ID' },
    { value: 'propertyName', label: 'Name' },
    { value: 'propertyAddress', label: 'Address' },
    { value: 'propertyCity', label: 'City' },
    { value: 'propertyState', label: 'State' },
    { value: 'propertyZipCode', label: 'Zip Code' },
    { value: 'propertyCountry', label: 'Country' },
    { value: 'propertyType', label: 'Type' },
    { value: 'propertyCode', label: 'Code' },
    { value: 'propertyDescription', label: 'Description' },
    { value: 'propertyUnitCount', label: 'Unit Count' },
    { value: 'propertySquareFootage', label: 'Square Footage' },
  ],
  providers: [
    { value: 'providerId', label: 'Provider ID' },
    { value: 'providerAccountId', label: 'Account ID' },
    { value: 'providerName', label: 'Name' },
    { value: 'providerCode', label: 'Code' },
    { value: 'providerDescription', label: 'Description' },
    { value: 'providerType', label: 'Type' },
    { value: 'providerAddress', label: 'Address' },
    { value: 'providerCity', label: 'City' },
    { value: 'providerState', label: 'State' },
    { value: 'providerZipCode', label: 'Zip Code' },
    { value: 'providerCountry', label: 'Country' },
    { value: 'providerPhone', label: 'Phone' },
    { value: 'providerEmail', label: 'Email' },
    { value: 'providerWebsite', label: 'Website' },
    { value: 'providerContactName', label: 'Contact Name' },
    { value: 'providerAccountNumber', label: 'Account Number' },
  ],
  utilities: [
    { value: 'utilityId', label: 'Utility ID' },
    { value: 'utilityName', label: 'Name' },
    { value: 'utilityType', label: 'Type' },
    { value: 'utilityCode', label: 'Code' },
    { value: 'utilityDescription', label: 'Description' },
  ],
  items: [
    { value: 'itemId', label: 'Item ID' },
    { value: 'externalId', label: 'External ID' },
    { value: 'date', label: 'Date' },
    { value: 'amount', label: 'Amount' },
    { value: 'quantity', label: 'Quantity' },
    { value: 'description', label: 'Description' },
    { value: 'source', label: 'Source' },
    { value: 'entities', label: 'Entities' },
  ],
};

export default function ReportEditor({ report, onSave, onClose }: ReportEditorProps) {
  const [name, setName] = useState('');
  const [type, setType] = useState<ReportType>('Export');
  const [isShared, setIsShared] = useState(false);
  const [sourceType, setSourceType] = useState<ReportSourceType>('bills');
  const [columns, setColumns] = useState<ReportColumnMapping[]>([]);
  const [staticColumns, setStaticColumns] = useState<ReportStaticColumn[]>([]);
  const [autoIncrement, setAutoIncrement] = useState<ReportAutoIncrement | null>(null);
  const [sortBy, setSortBy] = useState('');
  const [sortDescending, setSortDescending] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize from existing report
  useEffect(() => {
    if (report) {
      setName(report.name);
      setType(report.type);
      setIsShared(report.isShared);

      try {
        const config: ReportConfiguration = JSON.parse(report.configuration);
        setSourceType(config.sourceType || 'bills');
        setColumns(config.columns || []);
        setStaticColumns(config.staticColumns || []);
        setAutoIncrement(config.autoIncrement || null);
        setSortBy(config.sortBy || '');
        setSortDescending(config.sortDescending || false);
      } catch {
        // Invalid config, use defaults
      }
    }
  }, [report]);

  const handleAddColumn = () => {
    const fields = SOURCE_FIELDS[sourceType];
    const usedSources = new Set(columns.map(c => c.source));
    const nextField = fields.find(f => !usedSources.has(f.value));

    if (nextField) {
      setColumns([...columns, { source: nextField.value, header: nextField.label }]);
    }
  };

  const handleUpdateColumn = (index: number, field: 'source' | 'header' | 'format', value: string) => {
    const newColumns = [...columns];
    newColumns[index] = { ...newColumns[index], [field]: value };

    // Auto-update header when source changes
    if (field === 'source') {
      const fieldDef = SOURCE_FIELDS[sourceType].find(f => f.value === value);
      if (fieldDef) {
        newColumns[index].header = fieldDef.label;
      }
    }

    setColumns(newColumns);
  };

  const handleRemoveColumn = (index: number) => {
    setColumns(columns.filter((_, i) => i !== index));
    // Update saved order: remove entry and decrement higher indices
    if (savedOutputOrder) {
      setSavedOutputOrder(
        savedOutputOrder
          .filter(key => key !== `data:${index}`)
          .map(key => {
            if (key.startsWith('data:')) {
              const idx = parseInt(key.split(':')[1], 10);
              if (idx > index) return `data:${idx - 1}`;
            }
            return key;
          })
      );
    }
  };

  const handleAddStaticColumn = () => {
    setStaticColumns([...staticColumns, { header: 'New Column', value: '' }]);
  };

  const handleUpdateStaticColumn = (index: number, field: 'header' | 'value', value: string) => {
    const newCols = [...staticColumns];
    newCols[index] = { ...newCols[index], [field]: value };
    setStaticColumns(newCols);
  };

  const handleRemoveStaticColumn = (index: number) => {
    setStaticColumns(staticColumns.filter((_, i) => i !== index));
    // Update saved order: remove entry and decrement higher indices
    if (savedOutputOrder) {
      setSavedOutputOrder(
        savedOutputOrder
          .filter(key => key !== `static:${index}`)
          .map(key => {
            if (key.startsWith('static:')) {
              const idx = parseInt(key.split(':')[1], 10);
              if (idx > index) return `static:${idx - 1}`;
            }
            return key;
          })
      );
    }
  };

  const handleToggleAutoIncrement = () => {
    if (autoIncrement) {
      setAutoIncrement(null);
      // Remove 'auto' from saved order
      if (savedOutputOrder) {
        setSavedOutputOrder(savedOutputOrder.filter(key => key !== 'auto'));
      }
    } else {
      setAutoIncrement({ header: 'Line Number', startAt: 1, prefix: '' });
    }
  };

  // Unified column order for drag-and-drop
  type UnifiedColumn =
    | { type: 'auto'; data: ReportAutoIncrement }
    | { type: 'static'; index: number; data: ReportStaticColumn }
    | { type: 'data'; index: number; data: ReportColumnMapping };

  const [columnOrder, setColumnOrder] = useState<UnifiedColumn[]>([]);
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  // Track saved/current output order for column arrangement
  const [savedOutputOrder, setSavedOutputOrder] = useState<string[] | null>(null);

  // Initialize from existing report - also load saved output order
  useEffect(() => {
    if (report) {
      try {
        const config: ReportConfiguration = JSON.parse(report.configuration);
        if (config.outputOrder) {
          setSavedOutputOrder(config.outputOrder);
        }
      } catch {
        // Invalid config
      }
    }
  }, [report]);

  // Rebuild column order when columns change
  useEffect(() => {
    // If we have a saved/current order, use it to build the column order
    if (savedOutputOrder && savedOutputOrder.length > 0) {
      const newOrder: UnifiedColumn[] = [];
      savedOutputOrder.forEach(orderKey => {
        if (orderKey === 'auto' && autoIncrement) {
          newOrder.push({ type: 'auto', data: autoIncrement });
        } else if (orderKey.startsWith('static:')) {
          const idx = parseInt(orderKey.split(':')[1], 10);
          if (staticColumns[idx]) {
            newOrder.push({ type: 'static', index: idx, data: staticColumns[idx] });
          }
        } else if (orderKey.startsWith('data:')) {
          const idx = parseInt(orderKey.split(':')[1], 10);
          if (columns[idx]) {
            newOrder.push({ type: 'data', index: idx, data: columns[idx] });
          }
        }
      });
      // Add any new columns that weren't in the saved order
      if (autoIncrement && !savedOutputOrder.includes('auto')) {
        newOrder.push({ type: 'auto', data: autoIncrement });
      }
      staticColumns.forEach((sc, i) => {
        if (!savedOutputOrder.includes(`static:${i}`)) {
          newOrder.push({ type: 'static', index: i, data: sc });
        }
      });
      columns.forEach((col, i) => {
        if (!savedOutputOrder.includes(`data:${i}`)) {
          newOrder.push({ type: 'data', index: i, data: col });
        }
      });
      setColumnOrder(newOrder);
    } else {
      // Default order: auto -> static -> data
      const newOrder: UnifiedColumn[] = [];
      if (autoIncrement) {
        newOrder.push({ type: 'auto', data: autoIncrement });
      }
      staticColumns.forEach((sc, i) => newOrder.push({ type: 'static', index: i, data: sc }));
      columns.forEach((col, i) => newOrder.push({ type: 'data', index: i, data: col }));
      setColumnOrder(newOrder);
    }
  }, [autoIncrement, staticColumns, columns, savedOutputOrder]);

  // Handle drag start
  const handleDragStart = (index: number) => {
    setDraggedIndex(index);
  };

  // Handle drag over
  const handleDragOver = (e: React.DragEvent, index: number) => {
    e.preventDefault();
    if (draggedIndex !== null && draggedIndex !== index) {
      setDragOverIndex(index);
    }
  };

  // Handle drop - reorder columns
  const handleDrop = (e: React.DragEvent, dropIndex: number) => {
    e.preventDefault();
    if (draggedIndex === null || draggedIndex === dropIndex) {
      setDraggedIndex(null);
      setDragOverIndex(null);
      return;
    }

    const newOrder = [...columnOrder];
    const [draggedItem] = newOrder.splice(draggedIndex, 1);
    newOrder.splice(dropIndex, 0, draggedItem);

    // Rebuild the separate arrays based on new order
    const newAutoIncrement = newOrder.find(c => c.type === 'auto')?.data as ReportAutoIncrement | undefined;
    const newStaticColumns: ReportStaticColumn[] = newOrder
      .filter(c => c.type === 'static')
      .map(c => c.data as ReportStaticColumn);
    const newColumns: ReportColumnMapping[] = newOrder
      .filter(c => c.type === 'data')
      .map(c => c.data as ReportColumnMapping);

    // Build the output order keys from the new order to preserve arrangement
    const newOutputOrder: string[] = newOrder.map((col, idx) => {
      if (col.type === 'auto') return 'auto';
      if (col.type === 'static') {
        // Find the new index of this static column in the filtered static array
        const staticIdx = newOrder.slice(0, idx + 1).filter(c => c.type === 'static').length - 1;
        return `static:${staticIdx}`;
      }
      // data column
      const dataIdx = newOrder.slice(0, idx + 1).filter(c => c.type === 'data').length - 1;
      return `data:${dataIdx}`;
    });

    // Save the output order so useEffect preserves the arrangement
    setSavedOutputOrder(newOutputOrder);

    // Only update if auto-increment state changed
    if ((autoIncrement && !newAutoIncrement) || (!autoIncrement && newAutoIncrement)) {
      setAutoIncrement(newAutoIncrement || null);
    }
    setStaticColumns(newStaticColumns);
    setColumns(newColumns);

    setDraggedIndex(null);
    setDragOverIndex(null);
  };

  const handleDragEnd = () => {
    setDraggedIndex(null);
    setDragOverIndex(null);
  };

  // Generate preview data using unified column order
  const previewData = useMemo(() => {
    const sampleRows = SAMPLE_DATA[sourceType];
    const headers: { text: string; type: string }[] = [];
    const rows: (string | number)[][] = [];

    // Build headers from column order
    columnOrder.forEach(col => {
      if (col.type === 'auto') {
        headers.push({ text: col.data.header || 'Line', type: 'auto' });
      } else if (col.type === 'static') {
        headers.push({ text: col.data.header, type: 'static' });
      } else {
        headers.push({ text: col.data.header, type: 'data' });
      }
    });

    // Build data rows
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

    return { headers, rows };
  }, [sourceType, columnOrder]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError('Name is required');
      return;
    }

    if (columns.length === 0) {
      setError('At least one column is required');
      return;
    }

    // Build column order for export
    const outputOrder = columnOrder.map(col => {
      if (col.type === 'auto') return 'auto';
      if (col.type === 'static') return `static:${staticColumns.indexOf(col.data as ReportStaticColumn)}`;
      return `data:${columns.indexOf(col.data as ReportColumnMapping)}`;
    });

    const configuration: ReportConfiguration = {
      sourceType,
      columns,
      staticColumns: staticColumns.length > 0 ? staticColumns : undefined,
      autoIncrement: autoIncrement || undefined,
      sortBy: sortBy || undefined,
      sortDescending: sortDescending || undefined,
      outputOrder: outputOrder.length > 0 ? outputOrder : undefined,
    };

    setIsSaving(true);
    try {
      await onSave({
        name: name.trim(),
        type,
        configuration: JSON.stringify(configuration),
        isShared,
      });
    } catch (err) {
      setError('Failed to save report');
      console.error(err);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content report-editor-modal report-editor-wide" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{report ? 'Edit Report' : 'Create Report'}</h2>
          <button className="btn-close" onClick={onClose}>&times;</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="modal-body report-editor-body">
            <div className="report-editor-config">
            {error && <div className="error-message">{error}</div>}

            <div className="config-card">
              <div className="config-card-header">
                <h3>Report Settings</h3>
              </div>
              <div className="config-card-body">
                <div className="form-grid">
                  <div className="form-row">
                    <label htmlFor="name">Report Name</label>
                    <input
                      id="name"
                      type="text"
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      placeholder="Enter report name"
                      required
                    />
                  </div>

                  <div className="form-row">
                    <label htmlFor="sourceType">Data Source</label>
                    <select
                      id="sourceType"
                      value={sourceType}
                      onChange={(e) => {
                        setSourceType(e.target.value as ReportSourceType);
                        setColumns([]);
                        setSortBy('');
                      }}
                    >
                      <option value="bills">Bills</option>
                      <option value="properties">Properties</option>
                      <option value="providers">Providers</option>
                      <option value="utilities">Utilities</option>
                      <option value="items">Items</option>
                    </select>
                  </div>
                </div>

                <div className="form-row-inline">
                  <div className="form-row inline">
                    <label htmlFor="sortBy">Sort by</label>
                    <select
                      id="sortBy"
                      value={sortBy}
                      onChange={(e) => setSortBy(e.target.value)}
                    >
                      <option value="">No sorting</option>
                      {SOURCE_FIELDS[sourceType].map((field) => (
                        <option key={field.value} value={field.value}>
                          {field.label}
                        </option>
                      ))}
                    </select>
                  </div>

                  {sortBy && (
                    <div className="form-row inline">
                      <label htmlFor="sortDir">Direction</label>
                      <select
                        id="sortDir"
                        value={sortDescending ? 'desc' : 'asc'}
                        onChange={(e) => setSortDescending(e.target.value === 'desc')}
                      >
                        <option value="asc">Ascending</option>
                        <option value="desc">Descending</option>
                      </select>
                    </div>
                  )}
                </div>

                <div className="form-row checkbox-row">
                  <label>
                    <input
                      type="checkbox"
                      checked={isShared}
                      onChange={(e) => setIsShared(e.target.checked)}
                    />
                    Share with other users
                  </label>
                </div>
              </div>
            </div>

            <div className="config-card">
              <div className="config-card-header">
                <h3>Data Columns</h3>
                <button type="button" className="btn-add" onClick={handleAddColumn}>
                  + Add
                </button>
              </div>
              <div className="config-card-body">
                {columns.length === 0 ? (
                  <p className="hint-text">Add columns to define what data to include in the export.</p>
                ) : (
                  <div className="columns-list">
                    {columns.map((col, index) => (
                      <div key={index} className="column-row">
                        <select
                          value={col.source}
                          onChange={(e) => handleUpdateColumn(index, 'source', e.target.value)}
                        >
                          {SOURCE_FIELDS[sourceType].map((field) => (
                            <option key={field.value} value={field.value}>
                              {field.label}
                            </option>
                          ))}
                        </select>
                        <input
                          type="text"
                          value={col.header}
                          onChange={(e) => handleUpdateColumn(index, 'header', e.target.value)}
                          placeholder="Header"
                        />
                        <input
                          type="text"
                          value={col.format || ''}
                          onChange={(e) => handleUpdateColumn(index, 'format', e.target.value)}
                          placeholder="Format"
                          className="format-input"
                          title="Format pattern, e.g. MM/dd/yyyy for dates"
                        />
                        <button
                          type="button"
                          className="btn-remove"
                          onClick={() => handleRemoveColumn(index)}
                          title="Remove column"
                        >
                          &times;
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="config-card">
              <div className="config-card-header">
                <h3>Static Columns</h3>
                <button type="button" className="btn-add" onClick={handleAddStaticColumn}>
                  + Add
                </button>
              </div>
              <div className="config-card-body">
                {staticColumns.length === 0 ? (
                  <p className="hint-text">Add fixed values that repeat on every row (GL codes, etc.)</p>
                ) : (
                  <div className="columns-list">
                    {staticColumns.map((col, index) => (
                      <div key={index} className="column-row">
                        <input
                          type="text"
                          value={col.header}
                          onChange={(e) => handleUpdateStaticColumn(index, 'header', e.target.value)}
                          placeholder="Header"
                        />
                        <input
                          type="text"
                          value={col.value}
                          onChange={(e) => handleUpdateStaticColumn(index, 'value', e.target.value)}
                          placeholder="Value"
                        />
                        <button
                          type="button"
                          className="btn-remove"
                          onClick={() => handleRemoveStaticColumn(index)}
                          title="Remove column"
                        >
                          &times;
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="config-card">
              <div className="config-card-header">
                <h3>Line Numbers</h3>
                <label className="toggle-switch">
                  <input
                    type="checkbox"
                    checked={autoIncrement !== null}
                    onChange={handleToggleAutoIncrement}
                  />
                  <span className="toggle-slider"></span>
                </label>
              </div>
              {autoIncrement && (
                <div className="config-card-body">
                  <div className="auto-increment-grid">
                    <div className="form-row compact">
                      <label htmlFor="aiHeader">Header</label>
                      <input
                        id="aiHeader"
                        type="text"
                        value={autoIncrement.header}
                        onChange={(e) => setAutoIncrement({ ...autoIncrement, header: e.target.value })}
                      />
                    </div>
                    <div className="form-row compact">
                      <label htmlFor="aiStart">Start</label>
                      <input
                        id="aiStart"
                        type="number"
                        value={autoIncrement.startAt}
                        onChange={(e) => setAutoIncrement({ ...autoIncrement, startAt: parseInt(e.target.value) || 1 })}
                        min={1}
                      />
                    </div>
                    <div className="form-row compact">
                      <label htmlFor="aiPrefix">Prefix</label>
                      <input
                        id="aiPrefix"
                        type="text"
                        value={autoIncrement.prefix || ''}
                        onChange={(e) => setAutoIncrement({ ...autoIncrement, prefix: e.target.value })}
                        placeholder="e.g. LINE-"
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
            </div>

            <div className="report-editor-preview">
              <div className="preview-header">
                <h3>Preview</h3>
                <span className="preview-hint">Drag column headers to reorder</span>
              </div>
              {previewData.headers.length === 0 ? (
                <div className="preview-empty">
                  <p>Add columns to see a preview of your export</p>
                </div>
              ) : (
                <div className="preview-table-container">
                  <table className="preview-table">
                    <thead>
                      <tr>
                        {previewData.headers.map((header, i) => (
                          <th
                            key={i}
                            draggable
                            onDragStart={() => handleDragStart(i)}
                            onDragOver={(e) => handleDragOver(e, i)}
                            onDrop={(e) => handleDrop(e, i)}
                            onDragEnd={handleDragEnd}
                            className={`draggable-header ${draggedIndex === i ? 'dragging' : ''} ${dragOverIndex === i ? 'drag-over' : ''} column-type-${header.type}`}
                          >
                            <span className="drag-handle">&#9776;</span>
                            {header.text}
                          </th>
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
              )}
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-secondary" onClick={onClose} disabled={isSaving}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={isSaving}>
              {isSaving ? 'Saving...' : (report ? 'Save Changes' : 'Create Report')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
