import { useRef, useState, useCallback, useMemo } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import type { Item, EntityTag } from '../types';

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

type SortColumn = 'date' | 'description' | 'externalId' | 'amount' | 'quantity' | 'source' | 'status';
type SortDirection = 'asc' | 'desc';

function getItemStatus(item: Item): 'Paid' | 'Approval Needed' {
  // Mock synced items have "Synced" in their description
  if (item.description?.includes('Synced')) {
    return 'Approval Needed';
  }
  return 'Paid';
}

interface VirtualizedItemTableProps {
  items: Item[];
  selectedItem: Item | null;
  focusedItemId: string | null;
  focusedEntity: EntityTag | null;
  onItemSelect: (item: Item) => void;
  onEntityClick: (entity: EntityTag, item: Item, e: React.MouseEvent) => void;
  onClearFocus: () => void;
  showStatusColumn?: boolean;
}

const ROW_HEIGHT = 48;
const EXPANDED_ROW_HEIGHT = 100;

export default function VirtualizedItemTable({
  items,
  selectedItem,
  focusedItemId,
  focusedEntity,
  onItemSelect,
  onEntityClick,
  onClearFocus,
  showStatusColumn = false,
}: VirtualizedItemTableProps) {
  const parentRef = useRef<HTMLDivElement>(null);
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [sortColumn, setSortColumn] = useState<SortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  const handleSort = useCallback((column: SortColumn) => {
    if (sortColumn === column) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  }, [sortColumn]);

  // Filter items if focusedItemId or focusedEntity is set
  const filteredItems = useMemo(() => {
    if (focusedEntity) {
      // Filter to show only items that have this entity
      return items.filter(item =>
        item.entities.some(e => e.id === focusedEntity.id)
      );
    }
    if (focusedItemId) {
      return items.filter(item => item.id === focusedItemId);
    }
    return items;
  }, [items, focusedItemId, focusedEntity]);

  const sortedItems = useMemo(() => {
    if (!sortColumn) return filteredItems;

    return [...filteredItems].sort((a, b) => {
      let aVal: string | number | null;
      let bVal: string | number | null;

      switch (sortColumn) {
        case 'date':
          aVal = a.date;
          bVal = b.date;
          break;
        case 'description':
          aVal = a.description?.toLowerCase() || '';
          bVal = b.description?.toLowerCase() || '';
          break;
        case 'externalId':
          aVal = a.externalId?.toLowerCase() || '';
          bVal = b.externalId?.toLowerCase() || '';
          break;
        case 'amount':
          aVal = a.amount;
          bVal = b.amount;
          break;
        case 'quantity':
          aVal = a.quantity ?? 0;
          bVal = b.quantity ?? 0;
          break;
        case 'source':
          aVal = a.source.toLowerCase();
          bVal = b.source.toLowerCase();
          break;
        case 'status':
          aVal = getItemStatus(a);
          bVal = getItemStatus(b);
          break;
        default:
          return 0;
      }

      if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
      if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [filteredItems, sortColumn, sortDirection]);

  const toggleRowExpansion = useCallback((itemId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    setExpandedRows(prev => {
      const next = new Set(prev);
      if (next.has(itemId)) {
        next.delete(itemId);
      } else {
        next.add(itemId);
      }
      return next;
    });
  }, []);

  const getItemSize = useCallback((index: number) => {
    const item = sortedItems[index];
    if (expandedRows.has(item.id) && item.entities && item.entities.length > 0) {
      return ROW_HEIGHT + EXPANDED_ROW_HEIGHT;
    }
    return ROW_HEIGHT;
  }, [sortedItems, expandedRows]);

  const virtualizer = useVirtualizer({
    count: sortedItems.length,
    getScrollElement: () => parentRef.current,
    estimateSize: getItemSize,
    overscan: 5,
  });

  const virtualItems = virtualizer.getVirtualItems();

  const SortIndicator = ({ column }: { column: SortColumn }) => {
    if (sortColumn !== column) {
      return <span className="sort-indicator inactive">⇅</span>;
    }
    return (
      <span className="sort-indicator active">
        {sortDirection === 'asc' ? '↑' : '↓'}
      </span>
    );
  };

  return (
    <div className="virtualized-table-container">
      {(focusedItemId || focusedEntity) && (
        <div className="focus-filter-banner">
          <span>
            {focusedEntity
              ? `Filtering by entity: ${focusedEntity.name} (${filteredItems.length} item${filteredItems.length !== 1 ? 's' : ''})`
              : 'Showing focused item only'}
          </span>
          <button className="clear-focus-btn" onClick={onClearFocus}>
            Show All
          </button>
        </div>
      )}
      <div className="virtualized-table-header">
        <table className="data-grid expandable-grid">
          <thead>
            <tr>
              <th className="expand-col"></th>
              <th className="sortable" onClick={() => handleSort('date')}>
                Date <SortIndicator column="date" />
              </th>
              <th className="sortable" onClick={() => handleSort('description')}>
                Description <SortIndicator column="description" />
              </th>
              <th className="sortable" onClick={() => handleSort('externalId')}>
                External ID <SortIndicator column="externalId" />
              </th>
              <th className="amount-col sortable" onClick={() => handleSort('amount')}>
                Amount <SortIndicator column="amount" />
              </th>
              <th className="qty-col sortable" onClick={() => handleSort('quantity')}>
                Qty <SortIndicator column="quantity" />
              </th>
              <th className="sortable" onClick={() => handleSort('source')}>
                Source <SortIndicator column="source" />
              </th>
              {showStatusColumn && (
                <th className="sortable" onClick={() => handleSort('status')}>
                  Status <SortIndicator column="status" />
                </th>
              )}
            </tr>
          </thead>
        </table>
      </div>
      <div
        ref={parentRef}
        className="virtualized-table-body"
        style={{ height: '500px', overflow: 'auto' }}
      >
        <div
          style={{
            height: `${virtualizer.getTotalSize()}px`,
            width: '100%',
            position: 'relative',
          }}
        >
          {virtualItems.map((virtualRow) => {
            const item = sortedItems[virtualRow.index];
            const isExpanded = expandedRows.has(item.id);
            const hasEntities = item.entities && item.entities.length > 0;

            return (
              <div
                key={item.id}
                data-index={virtualRow.index}
                ref={virtualizer.measureElement}
                style={{
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  width: '100%',
                  transform: `translateY(${virtualRow.start}px)`,
                  zIndex: isExpanded ? 10 : 1,
                }}
              >
                <table className="data-grid expandable-grid virtual-row-table">
                  <tbody>
                    <tr
                      onClick={() => onItemSelect(item)}
                      className={`${selectedItem?.id === item.id ? 'selected' : ''} ${isExpanded ? 'expanded' : ''}`}
                    >
                      <td className="expand-col">
                        {hasEntities && (
                          <button
                            className="expand-btn"
                            onClick={(e) => toggleRowExpansion(item.id, e)}
                            title={isExpanded ? 'Collapse' : 'Expand to see entities'}
                          >
                            <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>
                              &#9658;
                            </span>
                            <span className="entity-count">{item.entities.length}</span>
                          </button>
                        )}
                      </td>
                      <td>{formatDate(item.date)}</td>
                      <td className="description-cell">{item.description || '-'}</td>
                      <td className="external-id-cell">{item.externalId || '-'}</td>
                      <td className="amount-col">{formatCurrency(item.amount)}</td>
                      <td className="qty-col">{item.quantity?.toLocaleString() || '-'}</td>
                      <td>
                        <span className={`source-badge source-${item.source.toLowerCase()}`}>
                          {item.source}
                        </span>
                      </td>
                      {showStatusColumn && (
                        <td>
                          <span className={`status-badge status-${getItemStatus(item) === 'Paid' ? 'paid' : 'approval'}`}>
                            {getItemStatus(item)}
                          </span>
                        </td>
                      )}
                    </tr>
                    {isExpanded && hasEntities && (
                      <tr className="expanded-row">
                        <td colSpan={showStatusColumn ? 8 : 7}>
                          <div className="expanded-content">
                            <div className="expanded-label">Linked Entities:</div>
                            <div className="expanded-entities">
                              {item.entities.map((entity) => (
                                <div
                                  key={entity.id}
                                  className="expanded-entity-card clickable"
                                  style={{ borderLeftColor: entity.color || '#6b7280' }}
                                  onClick={(e) => onEntityClick(entity, item, e)}
                                  title={`Focus on this item`}
                                >
                                  <span
                                    className="entity-type-indicator"
                                    style={{ backgroundColor: entity.color || '#6b7280' }}
                                  >
                                    {entity.entityTypeCode}
                                  </span>
                                  <span className="entity-name">{entity.name}</span>
                                </div>
                              ))}
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
