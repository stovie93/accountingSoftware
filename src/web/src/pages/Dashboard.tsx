import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { itemsApi, entitiesApi, mockSyncApi, propertiesApi, providersApi, billsApi, utilitiesApi } from '../services/api';
import { usePagination } from '../hooks/usePagination';
import { useDebounce } from '../hooks/useDebounce';
import { useEntityTypesWithEntities } from '../hooks/useEntityTypes';
import Pagination from '../components/Pagination';
import VirtualizedItemTable from '../components/VirtualizedItemTable';
import type { EntityTypeWithEntities, Entity, Item, EntityTag, Property, Provider, Bill, Utility, BillRelationships, PropertyRelationships, ProviderRelationships, UtilityRelationships } from '../types';

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

interface EntityWithType extends Entity {
  entityTypeName: string;
  entityTypeColor?: string;
}

// Unified search result type
interface SearchResult {
  id: string;
  name: string;
  description?: string;
  type: 'property' | 'vendor' | 'bill' | 'utility' | 'entity';
  typeLabel: string;
  typeColor: string;
  data: Property | Provider | Bill | Utility | EntityWithType;
}

export default function Dashboard() {
  const { user } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const [searchQuery, setSearchQuery] = useState('');
  const [filteredEntities, setFilteredEntities] = useState<EntityWithType[]>([]);
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [selectedEntity, setSelectedEntity] = useState<EntityWithType | null>(null);
  const [selectedEntityType, setSelectedEntityType] = useState<EntityTypeWithEntities | null>(null);
  const [linkedItems, setLinkedItems] = useState<Item[]>([]);
  const [properties, setProperties] = useState<Property[]>([]);
  const [providers, setProviders] = useState<Provider[]>([]);
  const [bills, setBills] = useState<Bill[]>([]);
  const [utilities, setUtilities] = useState<Utility[]>([]);
  const [selectedProperty, setSelectedProperty] = useState<Property | null>(null);
  const [selectedProvider, setSelectedProvider] = useState<Provider | null>(null);
  const [selectedBill, setSelectedBill] = useState<Bill | null>(null);
  const [selectedUtility, setSelectedUtility] = useState<Utility | null>(null);
  const [expandedPropertyRows, setExpandedPropertyRows] = useState<Set<string>>(new Set());
  const [expandedProviderRows, setExpandedProviderRows] = useState<Set<string>>(new Set());
  const [expandedBillRows, setExpandedBillRows] = useState<Set<string>>(new Set());
  const [expandedUtilityRows, setExpandedUtilityRows] = useState<Set<string>>(new Set());
  const [propertyRelationshipsMap, setPropertyRelationshipsMap] = useState<Record<string, PropertyRelationships>>({});
  const [providerRelationshipsMap, setProviderRelationshipsMap] = useState<Record<string, ProviderRelationships>>({});
  const [billRelationshipsMap, setBillRelationshipsMap] = useState<Record<string, BillRelationships>>({});
  const [utilityRelationshipsMap, setUtilityRelationshipsMap] = useState<Record<string, UtilityRelationships>>({});
  const [loadingExpandedRow, setLoadingExpandedRow] = useState<string | null>(null);
  const [isLoadingItems, setIsLoadingItems] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [selectedItem, setSelectedItem] = useState<Item | null>(null);
  const [focusedItemId, setFocusedItemId] = useState<string | null>(null);
  const [focusedEntity, setFocusedEntity] = useState<EntityTag | null>(null);
  const [totalItemCount, setTotalItemCount] = useState(0);
  const [isSyncing, setIsSyncing] = useState(false);
  const [syncMessage, setSyncMessage] = useState<string | null>(null);
  const { page, pageSize, setPage, setPageSize, resetPagination } = usePagination();
  const searchRef = useRef<HTMLDivElement>(null);

  // Debounce search query to avoid excessive API calls
  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  // Use React Query for entity types (cached and deduplicated with Layout)
  const { data: entityTypes = [], isLoading: isLoadingEntities } = useEntityTypesWithEntities();

  // Flatten entities with their type info
  const allEntities: EntityWithType[] = entityTypes.flatMap((type) =>
    type.entities.map((entity) => ({
      ...entity,
      entityTypeName: type.name,
      entityTypeColor: type.color,
    }))
  );

  const entityIdFromUrl = searchParams.get('entity');
  const entityTypeIdFromUrl = searchParams.get('entityType');
  const showAllEntities = searchParams.get('entities') === 'all';

  // Select entity from URL param
  useEffect(() => {
    if (entityIdFromUrl && allEntities.length > 0) {
      const entity = allEntities.find(e => e.id === entityIdFromUrl);
      if (entity && (!selectedEntity || selectedEntity.id !== entityIdFromUrl)) {
        setSelectedEntity(entity);
        setSelectedEntityType(null);
        setSearchQuery(entity.name);
        setSelectedItem(null);
                resetPagination();
      }
    }
  }, [entityIdFromUrl, allEntities]);

  // Select entity type from URL param
  useEffect(() => {
    if (entityTypeIdFromUrl && entityTypes.length > 0) {
      const entityType = entityTypes.find(t => t.id === entityTypeIdFromUrl);
      if (entityType && (!selectedEntityType || selectedEntityType.id !== entityTypeIdFromUrl)) {
        setSelectedEntityType(entityType);
        setSelectedEntity(null);
        setSearchQuery('');
        setSelectedItem(null);
                resetPagination();
      }
    } else if (!entityTypeIdFromUrl && !entityIdFromUrl) {
      setSelectedEntityType(null);
    }
  }, [entityTypeIdFromUrl, entityTypes]);

  // Load data for selected entity type (properties, providers, or items based on type)
  useEffect(() => {
    if (!selectedEntityType) return;

    const loadEntityTypeData = async () => {
      setIsLoadingItems(true);
      setProperties([]);
      setProviders([]);
      setBills([]);
      setUtilities([]);
      setLinkedItems([]);

      try {
        if (selectedEntityType.code === 'property') {
          // Load properties for Property entity type
          const response = await propertiesApi.list({ page, pageSize });
          setProperties(response.properties);
          setTotalItemCount(response.totalCount);
        } else if (selectedEntityType.code === 'vendor') {
          // Load providers for Vendor entity type
          const response = await providersApi.list({ page, pageSize });
          setProviders(response.providers);
          setTotalItemCount(response.totalCount);
        } else if (selectedEntityType.code === 'bill') {
          // Load bills for Bill entity type
          const response = await billsApi.list({ page, pageSize });
          setBills(response.bills);
          setTotalItemCount(response.totalCount);
        } else if (selectedEntityType.code === 'utility-type') {
          // Load utilities for Utility Type entity type
          const response = await utilitiesApi.list({ page, pageSize });
          setUtilities(response.utilities);
          setTotalItemCount(response.totalCount);
        } else {
          // Load items for other entity types
          const entityIds = selectedEntityType.entities.map(e => e.id);
          const response = await itemsApi.list({
            entityIds,
            page,
            pageSize,
          });
          setLinkedItems(response.items);
          setTotalItemCount(response.totalCount);
        }
      } catch (error) {
        console.error('Failed to load data:', error);
      } finally {
        setIsLoadingItems(false);
      }
    };
    loadEntityTypeData();
  }, [selectedEntityType, page, pageSize]);

  // Load all items when showing all entities
  useEffect(() => {
    if (!showAllEntities) return;

    setSelectedEntity(null);
    setSelectedEntityType(null);
    setSearchQuery('');

    const loadAllItems = async () => {
      setIsLoadingItems(true);
      try {
        const response = await itemsApi.list({ page, pageSize });
        setLinkedItems(response.items);
        setTotalItemCount(response.totalCount);
      } catch (error) {
        console.error('Failed to load items:', error);
      } finally {
        setIsLoadingItems(false);
      }
    };
    loadAllItems();
  }, [showAllEntities, page, pageSize]);

  // Server-side search across all domain tables (debounced)
  useEffect(() => {
    if (debouncedSearchQuery.trim() === '') {
      setSearchResults([]);
      setFilteredEntities([]);
      setShowDropdown(false);
      return;
    }

    const searchAll = async () => {
      setIsSearching(true);
      try {
        // Search all domain tables in parallel
        const [propertiesRes, providersRes, billsRes, utilitiesRes] = await Promise.all([
          propertiesApi.list({ search: debouncedSearchQuery, pageSize: 5 }),
          providersApi.list({ search: debouncedSearchQuery, pageSize: 5 }),
          billsApi.list({ search: debouncedSearchQuery, pageSize: 5 }),
          utilitiesApi.list({ search: debouncedSearchQuery, pageSize: 5 }),
        ]);

        const results: SearchResult[] = [];

        // Add properties
        propertiesRes.properties.forEach(p => results.push({
          id: p.propertyId,
          name: p.propertyName,
          description: p.propertyAddress,
          type: 'property',
          typeLabel: 'Property',
          typeColor: '#3B82F6',
          data: p,
        }));

        // Add vendors/providers
        providersRes.providers.forEach(p => results.push({
          id: p.providerId,
          name: p.providerName,
          description: p.providerType || undefined,
          type: 'vendor',
          typeLabel: 'Vendor',
          typeColor: '#F59E0B',
          data: p,
        }));

        // Add bills
        billsRes.bills.forEach(b => results.push({
          id: b.billId,
          name: b.billReferenceId,
          description: `${b.utilityName || b.billCategory || ''} - ${formatCurrency(b.billAmount)}`,
          type: 'bill',
          typeLabel: 'Bill',
          typeColor: '#EF4444',
          data: b,
        }));

        // Add utilities
        utilitiesRes.utilities.forEach(u => results.push({
          id: u.utilityId,
          name: u.utilityName,
          description: u.utilityType,
          type: 'utility',
          typeLabel: 'Utility',
          typeColor: '#10B981',
          data: u,
        }));

        setSearchResults(results);
        setShowDropdown(true);
      } catch (error) {
        console.error('Failed to search:', error);
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    };
    searchAll();
  }, [debouncedSearchQuery]);

  // Load linked items when entity is selected
  useEffect(() => {
    if (!selectedEntity) {
      setLinkedItems([]);
      setTotalItemCount(0);
      return;
    }

    const loadItems = async () => {
      setIsLoadingItems(true);
      try {
        const response = await itemsApi.list({
          entityId: selectedEntity.id,
          page,
          pageSize,
        });
        setLinkedItems(response.items);
        setTotalItemCount(response.totalCount);
      } catch (error) {
        console.error('Failed to load items:', error);
      } finally {
        setIsLoadingItems(false);
      }
    };
    loadItems();
  }, [selectedEntity, page, pageSize]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleEntitySelect = (entity: EntityWithType) => {
    setSelectedEntity(entity);
    setSearchQuery(entity.name);
    setShowDropdown(false);
    setSelectedItem(null);
    resetPagination();
    setSearchParams({ entity: entity.id });
  };

  const handleSearchResultSelect = (result: SearchResult) => {
    setSearchQuery(result.name);
    setShowDropdown(false);
    setSearchResults([]);
    resetPagination();

    // Find the entity type and navigate to it with the item selected
    const entityType = entityTypes.find(et =>
      (result.type === 'property' && et.code === 'property') ||
      (result.type === 'vendor' && et.code === 'vendor') ||
      (result.type === 'bill' && et.code === 'bill') ||
      (result.type === 'utility' && et.code === 'utility-type')
    );

    if (entityType) {
      setSelectedEntityType(entityType);
      setSearchParams({ entityType: entityType.id });
    }
  };

  const handleClearSearch = () => {
    setSearchQuery('');
    setSelectedEntity(null);
    setLinkedItems([]);
    setFilteredEntities([]);
    setSearchResults([]);
    setSelectedItem(null);
    resetPagination();
    setSearchParams({});
  };

  const handleEntityClick = (entity: EntityTag, item: Item, e: React.MouseEvent) => {
    e.stopPropagation();
    // Filter grid to show only items that have this entity
    setFocusedEntity(entity);
    setFocusedItemId(null);
    setSelectedItem(null);
  };

  const handleClearFocus = () => {
    setFocusedItemId(null);
    setFocusedEntity(null);
  };

  // Navigation handlers for clicking on related entities in expanded rows
  // These navigate to the entity type view without opening the detail panel
  const navigateToProvider = (provider: Provider) => {
    const vendorType = entityTypes.find(t => t.code === 'vendor');
    if (vendorType) {
      setSelectedEntityType(vendorType);
      setSelectedEntity(null);
      setSearchParams({ entityType: vendorType.id });
    }
    // Clear all selections to avoid opening the detail panel
    setSelectedProvider(null);
    setSelectedProperty(null);
    setSelectedBill(null);
    setSelectedUtility(null);
  };

  const navigateToUtility = (utility: Utility) => {
    const utilityType = entityTypes.find(t => t.code === 'utility-type');
    if (utilityType) {
      setSelectedEntityType(utilityType);
      setSelectedEntity(null);
      setSearchParams({ entityType: utilityType.id });
    }
    // Clear all selections to avoid opening the detail panel
    setSelectedUtility(null);
    setSelectedProperty(null);
    setSelectedProvider(null);
    setSelectedBill(null);
  };

  const navigateToBill = (bill: Bill) => {
    const billType = entityTypes.find(t => t.code === 'bill');
    if (billType) {
      setSelectedEntityType(billType);
      setSelectedEntity(null);
      setSearchParams({ entityType: billType.id });
    }
    // Clear all selections to avoid opening the detail panel
    setSelectedBill(null);
    setSelectedProperty(null);
    setSelectedProvider(null);
    setSelectedUtility(null);
  };

  const navigateToProperty = (property: Property) => {
    const propertyType = entityTypes.find(t => t.code === 'property');
    if (propertyType) {
      setSelectedEntityType(propertyType);
      setSelectedEntity(null);
      setSearchParams({ entityType: propertyType.id });
    }
    // Clear all selections to avoid opening the detail panel
    setSelectedProperty(null);
    setSelectedProvider(null);
    setSelectedBill(null);
    setSelectedUtility(null);
  };

  const totalAmount = linkedItems.reduce((sum, item) => sum + item.amount, 0);

  const handleMockSync = async () => {
    setIsSyncing(true);
    setSyncMessage(null);
    try {
      const response = await mockSyncApi.triggerSync();
      setSyncMessage(`Synced ${response.itemCount} new items`);
      // Refresh the current view if showing items
      if (selectedEntity) {
        const itemsResponse = await itemsApi.list({
          entityId: selectedEntity.id,
          page,
          pageSize,
        });
        setLinkedItems(itemsResponse.items);
        setTotalItemCount(itemsResponse.totalCount);
      } else if (selectedEntityType) {
        const entityIds = selectedEntityType.entities.map(e => e.id);
        const itemsResponse = await itemsApi.list({
          entityIds,
          page,
          pageSize,
        });
        setLinkedItems(itemsResponse.items);
        setTotalItemCount(itemsResponse.totalCount);
      }
      // Clear message after 3 seconds
      setTimeout(() => setSyncMessage(null), 3000);
    } catch (error) {
      console.error('Mock sync failed:', error);
      setSyncMessage('Sync failed');
      setTimeout(() => setSyncMessage(null), 3000);
    } finally {
      setIsSyncing(false);
    }
  };

  // Toggle expand for property row to show linked entities
  const togglePropertyExpand = async (property: Property, e: React.MouseEvent) => {
    e.stopPropagation();
    const propertyId = property.propertyId;

    if (expandedPropertyRows.has(propertyId)) {
      // Collapse
      setExpandedPropertyRows(prev => {
        const next = new Set(prev);
        next.delete(propertyId);
        return next;
      });
    } else {
      // Expand and load relationships if not already loaded
      setExpandedPropertyRows(prev => new Set(prev).add(propertyId));

      if (!propertyRelationshipsMap[propertyId]) {
        setLoadingExpandedRow(propertyId);
        try {
          const relationships = await propertiesApi.getRelationships(propertyId);
          setPropertyRelationshipsMap(prev => ({
            ...prev,
            [propertyId]: relationships
          }));
        } catch (error) {
          console.error('Failed to load property relationships:', error);
        } finally {
          setLoadingExpandedRow(null);
        }
      }
    }
  };

  // Toggle expand for provider row to show linked entities
  const toggleProviderExpand = async (provider: Provider, e: React.MouseEvent) => {
    e.stopPropagation();
    const providerId = provider.providerId;

    if (expandedProviderRows.has(providerId)) {
      // Collapse
      setExpandedProviderRows(prev => {
        const next = new Set(prev);
        next.delete(providerId);
        return next;
      });
    } else {
      // Expand and load relationships if not already loaded
      setExpandedProviderRows(prev => new Set(prev).add(providerId));

      if (!providerRelationshipsMap[providerId]) {
        setLoadingExpandedRow(providerId);
        try {
          const relationships = await providersApi.getRelationships(providerId);
          setProviderRelationshipsMap(prev => ({
            ...prev,
            [providerId]: relationships
          }));
        } catch (error) {
          console.error('Failed to load provider relationships:', error);
        } finally {
          setLoadingExpandedRow(null);
        }
      }
    }
  };

  // Toggle expand for utility row to show linked entities
  const toggleUtilityExpand = async (utility: Utility, e: React.MouseEvent) => {
    e.stopPropagation();
    const utilityId = utility.utilityId;

    if (expandedUtilityRows.has(utilityId)) {
      // Collapse
      setExpandedUtilityRows(prev => {
        const next = new Set(prev);
        next.delete(utilityId);
        return next;
      });
    } else {
      // Expand and load relationships if not already loaded
      setExpandedUtilityRows(prev => new Set(prev).add(utilityId));

      if (!utilityRelationshipsMap[utilityId]) {
        setLoadingExpandedRow(utilityId);
        try {
          const relationships = await utilitiesApi.getRelationships(utilityId);
          setUtilityRelationshipsMap(prev => ({
            ...prev,
            [utilityId]: relationships
          }));
        } catch (error) {
          console.error('Failed to load utility relationships:', error);
        } finally {
          setLoadingExpandedRow(null);
        }
      }
    }
  };

  // Toggle expand for bill row to show linked entities
  const toggleBillExpand = async (bill: Bill, e: React.MouseEvent) => {
    e.stopPropagation();
    const billId = bill.billId;

    if (expandedBillRows.has(billId)) {
      // Collapse
      setExpandedBillRows(prev => {
        const next = new Set(prev);
        next.delete(billId);
        return next;
      });
    } else {
      // Expand and load relationships if not already loaded
      setExpandedBillRows(prev => new Set(prev).add(billId));

      if (!billRelationshipsMap[billId]) {
        setLoadingExpandedRow(billId);
        try {
          const relationships = await billsApi.getRelationships(billId);
          setBillRelationshipsMap(prev => ({
            ...prev,
            [billId]: relationships
          }));
        } catch (error) {
          console.error('Failed to load bill relationships:', error);
        } finally {
          setLoadingExpandedRow(null);
        }
      }
    }
  };

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <div className="dashboard-header-left">
          <h1>Dashboard</h1>
          <p className="welcome-text">Welcome, {user?.firstName || user?.email}</p>
        </div>
        <div className="dashboard-header-right">
          {syncMessage && (
            <span className={`sync-message ${syncMessage.includes('failed') ? 'error' : 'success'}`}>
              {syncMessage}
            </span>
          )}
          <button
            className="btn-sync"
            onClick={handleMockSync}
            disabled={isSyncing}
          >
            {isSyncing ? 'Syncing...' : 'Mock data sync'}
          </button>
        </div>
      </div>

      <div className="search-section" ref={searchRef}>
        <div className="search-container">
          <input
            type="text"
            className="dashboard-search"
            placeholder="Search properties, vendors, bills, utilities..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchQuery.trim() && searchResults.length > 0 && setShowDropdown(true)}
          />
          {(searchQuery || selectedEntity) && (
            <button className="search-clear" onClick={handleClearSearch}>
              &times;
            </button>
          )}
        </div>

        {showDropdown && isSearching && (
          <div className="search-dropdown">
            <div className="search-dropdown-loading">Searching...</div>
          </div>
        )}

        {showDropdown && !isSearching && searchResults.length > 0 && (
          <div className="search-dropdown">
            {searchResults.map((result) => (
              <div
                key={`${result.type}-${result.id}`}
                className="search-dropdown-item"
                onClick={() => handleSearchResultSelect(result)}
              >
                <span
                  className="entity-type-badge"
                  style={{ backgroundColor: result.typeColor }}
                >
                  {result.typeLabel}
                </span>
                <span className="entity-name">{result.name}</span>
                {result.description && (
                  <span className="entity-description">{result.description}</span>
                )}
              </div>
            ))}
          </div>
        )}

        {showDropdown && !isSearching && debouncedSearchQuery.trim() && searchResults.length === 0 && (
          <div className="search-dropdown">
            <div className="search-dropdown-empty">No results found matching "{debouncedSearchQuery}"</div>
          </div>
        )}
      </div>

      {selectedEntity && (
        <div className="search-results">
          <div className="results-header">
            <div className="selected-entity-info">
              <span
                className="entity-type-badge large"
                style={{ backgroundColor: selectedEntity.entityTypeColor || '#6b7280' }}
              >
                {selectedEntity.entityTypeName}
              </span>
              <h2>{selectedEntity.name}</h2>
              {selectedEntity.description && (
                <p className="entity-description">{selectedEntity.description}</p>
              )}
            </div>
            <div className="results-summary">
              <div className="summary-stat">
                <span className="stat-value">{linkedItems.length}</span>
                <span className="stat-label">Items</span>
              </div>
              <div className="summary-stat">
                <span className="stat-value">{formatCurrency(totalAmount)}</span>
                <span className="stat-label">Total</span>
              </div>
            </div>
          </div>

          {isLoadingItems ? (
            <div className="loading">Loading linked items...</div>
          ) : linkedItems.length === 0 ? (
            <div className="empty-state">
              <p>No items linked to this entity.</p>
            </div>
          ) : (
            <div className="data-grid-container">
              <VirtualizedItemTable
                items={linkedItems}
                selectedItem={selectedItem}
                focusedItemId={focusedItemId}
                focusedEntity={focusedEntity}
                onItemSelect={setSelectedItem}
                onEntityClick={handleEntityClick}
                onClearFocus={handleClearFocus}
                showStatusColumn={selectedEntity.entityTypeCode === 'utility-type'}
              />
              <Pagination
                currentPage={page}
                totalCount={totalItemCount}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
                isLoading={isLoadingItems}
              />
            </div>
          )}
        </div>
      )}

      {selectedItem && (
        <div className="item-detail-panel">
          <div className="panel-header">
            <h3>Item Details</h3>
            <button onClick={() => setSelectedItem(null)} className="btn-close">&times;</button>
          </div>
          <div className="panel-content">
            <div className="detail-row">
              <label>ID</label>
              <span>{selectedItem.id}</span>
            </div>
            <div className="detail-row">
              <label>External ID</label>
              <span>{selectedItem.externalId || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Date</label>
              <span>{formatDate(selectedItem.date)}</span>
            </div>
            <div className="detail-row">
              <label>Entities</label>
              <div className="panel-entities">
                {selectedItem.entities && selectedItem.entities.length > 0 ? (
                  selectedItem.entities.map((entity) => (
                    <div
                      key={entity.id}
                      className="panel-entity-card clickable"
                      style={{ borderLeftColor: entity.color || '#6b7280' }}
                      onClick={(e) => handleEntityClick(entity, e)}
                      title={`View all items for ${entity.name}`}
                    >
                      <span
                        className="entity-type-indicator"
                        style={{ backgroundColor: entity.color || '#6b7280' }}
                      >
                        {entity.entityTypeCode}
                      </span>
                      <span className="panel-entity-name">{entity.name}</span>
                    </div>
                  ))
                ) : (
                  <span className="no-entities-text">No entities linked</span>
                )}
              </div>
            </div>
            <div className="detail-row">
              <label>Amount</label>
              <span className="amount-value">{formatCurrency(selectedItem.amount)}</span>
            </div>
            {selectedItem.quantity && (
              <div className="detail-row">
                <label>Quantity</label>
                <span>{selectedItem.quantity.toLocaleString()}</span>
              </div>
            )}
            <div className="detail-row">
              <label>Description</label>
              <span>{selectedItem.description || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Source</label>
              <span className={`source-badge source-${selectedItem.source.toLowerCase()}`}>
                {selectedItem.source}
              </span>
            </div>
            {selectedItem.attributes && Object.keys(selectedItem.attributes).length > 0 && (
              <div className="attributes-section">
                <h4>Attributes</h4>
                <div className="attributes-grid">
                  {Object.entries(selectedItem.attributes).map(([key, value]) => (
                    <div key={key} className="attribute-item">
                      <label>{key}</label>
                      <span>{String(value)}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
            <div className="detail-row">
              <label>Created</label>
              <span>{new Date(selectedItem.createdAt).toLocaleString()}</span>
            </div>
            <div className="detail-row">
              <label>Updated</label>
              <span>{new Date(selectedItem.updatedAt).toLocaleString()}</span>
            </div>
          </div>
        </div>
      )}

      {selectedEntityType && !selectedEntity && (
        <div className="search-results">
          <div className="results-header">
            <div className="selected-entity-info">
              <span
                className="entity-type-badge large"
                style={{ backgroundColor: selectedEntityType.color || '#6b7280' }}
              >
                {selectedEntityType.name}
              </span>
              <h2>{selectedEntityType.name}</h2>
              {selectedEntityType.description && (
                <p className="entity-description">{selectedEntityType.description}</p>
              )}
            </div>
            <div className="results-summary">
              {selectedEntityType.code === 'bill' ? (
                <>
                  <div className="summary-stat">
                    <span className="stat-value">{totalItemCount}</span>
                    <span className="stat-label">Bills</span>
                  </div>
                  <div className="summary-stat warning">
                    <span className="stat-value">{bills.filter(b => b.billStatus === 'Needs approval').length}</span>
                    <span className="stat-label">Needs Approval</span>
                  </div>
                  <div className="summary-stat">
                    <span className="stat-value">{formatCurrency(bills.reduce((sum, b) => sum + b.billAmount, 0))}</span>
                    <span className="stat-label">Total Amount</span>
                  </div>
                </>
              ) : (
                <>
                  <div className="summary-stat">
                    <span className="stat-value">{totalItemCount}</span>
                    <span className="stat-label">
                      {selectedEntityType.code === 'property' ? 'Properties' :
                       selectedEntityType.code === 'vendor' ? 'Vendors' :
                       selectedEntityType.code === 'utility-type' ? 'Utilities' : 'Items'}
                    </span>
                  </div>
                  {selectedEntityType.code !== 'property' && selectedEntityType.code !== 'vendor' && selectedEntityType.code !== 'utility-type' && (
                    <div className="summary-stat">
                      <span className="stat-value">{formatCurrency(totalAmount)}</span>
                      <span className="stat-label">Total</span>
                    </div>
                  )}
                </>
              )}
            </div>
          </div>

          {isLoadingItems ? (
            <div className="loading">Loading...</div>
          ) : selectedEntityType.code === 'property' ? (
            properties.length === 0 ? (
              <div className="empty-state">
                <p>No properties found.</p>
              </div>
            ) : (
              <div className="data-grid-container">
                <table className="data-grid expandable-grid">
                  <thead>
                    <tr>
                      <th className="expand-col"></th>
                      <th>Name</th>
                      <th>Address</th>
                      <th>City</th>
                      <th>State</th>
                      <th>Type</th>
                      <th>Units</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {properties.map((property) => {
                      const isExpanded = expandedPropertyRows.has(property.propertyId);
                      const relationships = propertyRelationshipsMap[property.propertyId];
                      const isLoading = loadingExpandedRow === property.propertyId;
                      const hasRelationships = relationships &&
                        (relationships.providers.length > 0 ||
                         relationships.utilities.length > 0 ||
                         relationships.bills.length > 0);

                      return (
                        <React.Fragment key={property.propertyId}>
                          <tr
                            className={`clickable-row ${isExpanded ? 'expanded' : ''} ${selectedProperty?.propertyId === property.propertyId ? 'selected' : ''}`}
                            onClick={() => setSelectedProperty(property)}
                          >
                            <td className="expand-col">
                              <button
                                className="expand-btn"
                                onClick={(e) => togglePropertyExpand(property, e)}
                                title={isExpanded ? 'Collapse' : 'Expand to see linked entities'}
                              >
                                <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>
                                  &#9658;
                                </span>
                              </button>
                            </td>
                            <td>{property.propertyName}</td>
                            <td>{property.propertyAddress}</td>
                            <td>{property.propertyCity || '-'}</td>
                            <td>{property.propertyState || '-'}</td>
                            <td>{property.propertyType}</td>
                            <td>{property.propertyUnitCount || '-'}</td>
                            <td>
                              <span className={`status-badge ${property.propertyIsActive ? 'active' : 'inactive'}`}>
                                {property.propertyIsActive ? 'Active' : 'Inactive'}
                              </span>
                            </td>
                          </tr>
                          {isExpanded && (
                            <tr className="expanded-row">
                              <td colSpan={8}>
                                <div className="expanded-content">
                                  {isLoading ? (
                                    <div className="loading-inline">Loading...</div>
                                  ) : !hasRelationships ? (
                                    <div className="no-entities-text">No linked entities found</div>
                                  ) : (
                                    <div className="bill-relationships">
                                      {relationships.providers.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Providers:</div>
                                          <div className="expanded-entities">
                                            {relationships.providers.map((provider) => (
                                              <div
                                                key={provider.providerId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#10B981' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProvider(provider); }}
                                                title={`View ${provider.providerName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#10B981' }}>
                                                  provider
                                                </span>
                                                <span className="entity-name">{provider.providerName}</span>
                                                {provider.providerType && <span className="entity-detail">{provider.providerType}</span>}
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.utilities.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Utilities:</div>
                                          <div className="expanded-entities">
                                            {relationships.utilities.map((utility) => (
                                              <div
                                                key={utility.utilityId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#F59E0B' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToUtility(utility); }}
                                                title={`View ${utility.utilityName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#F59E0B' }}>
                                                  utility
                                                </span>
                                                <span className="entity-name">{utility.utilityName}</span>
                                                <span className="entity-detail">{utility.utilityType}</span>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.bills.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Bills ({relationships.bills.length}):</div>
                                          <div className="expanded-entities">
                                            {relationships.bills.slice(0, 5).map((bill) => (
                                              <div
                                                key={bill.billId}
                                                className="expanded-entity-card clickable"
                                                onClick={(e) => { e.stopPropagation(); navigateToBill(bill); }}
                                                title={`View ${bill.billReferenceId}`}
                                                style={{ borderLeftColor: '#8B5CF6' }}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#8B5CF6' }}>
                                                  bill
                                                </span>
                                                <span className="entity-name">{bill.billReferenceId}</span>
                                                <span className="entity-detail">{formatCurrency(bill.billAmount)}</span>
                                              </div>
                                            ))}
                                            {relationships.bills.length > 5 && (
                                              <div className="expanded-entity-card" style={{ borderLeftColor: '#6b7280' }}>
                                                <span className="entity-name">+{relationships.bills.length - 5} more</span>
                                              </div>
                                            )}
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </div>
                              </td>
                            </tr>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>
                <Pagination
                  currentPage={page}
                  totalCount={totalItemCount}
                  pageSize={pageSize}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  isLoading={isLoadingItems}
                />
              </div>
            )
          ) : selectedEntityType.code === 'vendor' ? (
            providers.length === 0 ? (
              <div className="empty-state">
                <p>No vendors found.</p>
              </div>
            ) : (
              <div className="data-grid-container">
                <table className="data-grid expandable-grid">
                  <thead>
                    <tr>
                      <th className="expand-col"></th>
                      <th>Name</th>
                      <th>Account ID</th>
                      <th>Type</th>
                      <th>Phone</th>
                      <th>Email</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {providers.map((provider) => {
                      const isExpanded = expandedProviderRows.has(provider.providerId);
                      const relationships = providerRelationshipsMap[provider.providerId];
                      const isLoading = loadingExpandedRow === provider.providerId;
                      const hasRelationships = relationships &&
                        (relationships.properties.length > 0 ||
                         relationships.utilities.length > 0 ||
                         relationships.bills.length > 0);

                      return (
                        <React.Fragment key={provider.providerId}>
                          <tr
                            className={`clickable-row ${isExpanded ? 'expanded' : ''} ${selectedProvider?.providerId === provider.providerId ? 'selected' : ''}`}
                            onClick={() => setSelectedProvider(provider)}
                          >
                            <td className="expand-col">
                              <button
                                className="expand-btn"
                                onClick={(e) => toggleProviderExpand(provider, e)}
                                title={isExpanded ? 'Collapse' : 'Expand to see linked entities'}
                              >
                                <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>
                                  &#9658;
                                </span>
                              </button>
                            </td>
                            <td>{provider.providerName}</td>
                            <td>{provider.providerAccountId}</td>
                            <td>{provider.providerType || '-'}</td>
                            <td>{provider.providerPhone || '-'}</td>
                            <td>{provider.providerEmail || '-'}</td>
                            <td>
                              <span className={`status-badge ${provider.providerIsActive ? 'active' : 'inactive'}`}>
                                {provider.providerIsActive ? 'Active' : 'Inactive'}
                              </span>
                            </td>
                          </tr>
                          {isExpanded && (
                            <tr className="expanded-row">
                              <td colSpan={7}>
                                <div className="expanded-content">
                                  {isLoading ? (
                                    <div className="loading-inline">Loading...</div>
                                  ) : !hasRelationships ? (
                                    <div className="no-entities-text">No linked entities found</div>
                                  ) : (
                                    <div className="bill-relationships">
                                      {relationships.properties.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Properties:</div>
                                          <div className="expanded-entities">
                                            {relationships.properties.map((property) => (
                                              <div
                                                key={property.propertyId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#3B82F6' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProperty(property); }}
                                                title={`View ${property.propertyName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#3B82F6' }}>
                                                  property
                                                </span>
                                                <span className="entity-name">{property.propertyName}</span>
                                                {property.propertyAddress && <span className="entity-detail">{property.propertyAddress}</span>}
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.utilities.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Utilities:</div>
                                          <div className="expanded-entities">
                                            {relationships.utilities.map((utility) => (
                                              <div
                                                key={utility.utilityId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#F59E0B' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToUtility(utility); }}
                                                title={`View ${utility.utilityName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#F59E0B' }}>
                                                  utility
                                                </span>
                                                <span className="entity-name">{utility.utilityName}</span>
                                                <span className="entity-detail">{utility.utilityType}</span>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.bills.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Bills ({relationships.bills.length}):</div>
                                          <div className="expanded-entities">
                                            {relationships.bills.slice(0, 5).map((bill) => (
                                              <div
                                                key={bill.billId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#8B5CF6' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToBill(bill); }}
                                                title={`View ${bill.billReferenceId}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#8B5CF6' }}>
                                                  bill
                                                </span>
                                                <span className="entity-name">{bill.billReferenceId}</span>
                                                <span className="entity-detail">{formatCurrency(bill.billAmount)}</span>
                                              </div>
                                            ))}
                                            {relationships.bills.length > 5 && (
                                              <div className="expanded-entity-card" style={{ borderLeftColor: '#6b7280' }}>
                                                <span className="entity-name">+{relationships.bills.length - 5} more</span>
                                              </div>
                                            )}
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </div>
                              </td>
                            </tr>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>
                <Pagination
                  currentPage={page}
                  totalCount={totalItemCount}
                  pageSize={pageSize}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  isLoading={isLoadingItems}
                />
              </div>
            )
          ) : selectedEntityType.code === 'bill' ? (
            bills.length === 0 ? (
              <div className="empty-state">
                <p>No bills found.</p>
              </div>
            ) : (
              <div className="data-grid-container">
                <table className="data-grid expandable-grid">
                  <thead>
                    <tr>
                      <th className="expand-col"></th>
                      <th>Reference</th>
                      <th>Amount</th>
                      <th>Due Date</th>
                      <th>Utility Type</th>
                      <th>Vendor</th>
                      <th>Period</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {bills.map((bill) => {
                      const isExpanded = expandedBillRows.has(bill.billId);
                      const relationships = billRelationshipsMap[bill.billId];
                      const isLoading = loadingExpandedRow === bill.billId;
                      const hasRelationships = relationships &&
                        (relationships.properties.length > 0 ||
                         relationships.providers.length > 0 ||
                         relationships.utilities.length > 0);

                      return (
                        <React.Fragment key={bill.billId}>
                          <tr
                            className={`clickable-row ${isExpanded ? 'expanded' : ''} ${selectedBill?.billId === bill.billId ? 'selected' : ''}`}
                            onClick={() => setSelectedBill(bill)}
                          >
                            <td className="expand-col">
                              <button
                                className="expand-btn"
                                onClick={(e) => toggleBillExpand(bill, e)}
                                title={isExpanded ? 'Collapse' : 'Expand to see linked entities'}
                              >
                                <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>
                                  &#9658;
                                </span>
                              </button>
                            </td>
                            <td>{bill.billReferenceId}</td>
                            <td>{formatCurrency(bill.billAmount)}</td>
                            <td>{bill.billDueDate ? formatDate(bill.billDueDate) : '-'}</td>
                            <td>{bill.utilityName || bill.billCategory || '-'}</td>
                            <td>{bill.providerName || '-'}</td>
                            <td>
                              {bill.billPeriodStart && bill.billPeriodEnd
                                ? `${bill.billPeriodStart} - ${bill.billPeriodEnd}`
                                : '-'}
                            </td>
                            <td>
                              <span className={`status-badge ${bill.billStatus?.toLowerCase().replace(/\s+/g, '-') || 'pending'}`}>
                                {bill.billStatus || 'Pending'}
                              </span>
                            </td>
                          </tr>
                          {isExpanded && (
                            <tr className="expanded-row">
                              <td colSpan={8}>
                                <div className="expanded-content">
                                  {isLoading ? (
                                    <div className="loading-inline">Loading...</div>
                                  ) : !hasRelationships ? (
                                    <div className="no-entities-text">No linked entities found</div>
                                  ) : (
                                    <div className="bill-relationships">
                                      {relationships.properties.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Property:</div>
                                          <div className="expanded-entities">
                                            {relationships.properties.map((prop) => (
                                              <div
                                                key={prop.propertyId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#3B82F6' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProperty(prop); }}
                                                title={`View ${prop.propertyName}`}
                                              >
                                                <span
                                                  className="entity-type-indicator"
                                                  style={{ backgroundColor: '#3B82F6' }}
                                                >
                                                  property
                                                </span>
                                                <span className="entity-name">{prop.propertyName}</span>
                                                <span className="entity-detail">{prop.propertyAddress}</span>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.providers.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Provider:</div>
                                          <div className="expanded-entities">
                                            {relationships.providers.map((prov) => (
                                              <div
                                                key={prov.providerId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#F59E0B' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProvider(prov); }}
                                                title={`View ${prov.providerName}`}
                                              >
                                                <span
                                                  className="entity-type-indicator"
                                                  style={{ backgroundColor: '#F59E0B' }}
                                                >
                                                  vendor
                                                </span>
                                                <span className="entity-name">{prov.providerName}</span>
                                                {prov.providerType && (
                                                  <span className="entity-detail">{prov.providerType}</span>
                                                )}
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.utilities.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Utility:</div>
                                          <div className="expanded-entities">
                                            {relationships.utilities.map((util) => (
                                              <div
                                                key={util.utilityId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#10B981' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToUtility(util); }}
                                                title={`View ${util.utilityName}`}
                                              >
                                                <span
                                                  className="entity-type-indicator"
                                                  style={{ backgroundColor: '#10B981' }}
                                                >
                                                  utility
                                                </span>
                                                <span className="entity-name">{util.utilityName}</span>
                                                <span className="entity-detail">{util.utilityType}</span>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </div>
                              </td>
                            </tr>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>
                <Pagination
                  currentPage={page}
                  totalCount={totalItemCount}
                  pageSize={pageSize}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  isLoading={isLoadingItems}
                />
              </div>
            )
          ) : selectedEntityType.code === 'utility-type' ? (
            utilities.length === 0 ? (
              <div className="empty-state">
                <p>No utilities found.</p>
              </div>
            ) : (
              <div className="data-grid-container">
                <table className="data-grid expandable-grid">
                  <thead>
                    <tr>
                      <th className="expand-col"></th>
                      <th>Name</th>
                      <th>Type</th>
                      <th>Code</th>
                      <th>Description</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {utilities.map((utility) => {
                      const isExpanded = expandedUtilityRows.has(utility.utilityId);
                      const relationships = utilityRelationshipsMap[utility.utilityId];
                      const isLoading = loadingExpandedRow === utility.utilityId;
                      const hasRelationships = relationships &&
                        (relationships.properties.length > 0 ||
                         relationships.providers.length > 0 ||
                         relationships.bills.length > 0);

                      return (
                        <React.Fragment key={utility.utilityId}>
                          <tr
                            className={`clickable-row ${isExpanded ? 'expanded' : ''} ${selectedUtility?.utilityId === utility.utilityId ? 'selected' : ''}`}
                            onClick={() => setSelectedUtility(utility)}
                          >
                            <td className="expand-col">
                              <button
                                className="expand-btn"
                                onClick={(e) => toggleUtilityExpand(utility, e)}
                                title={isExpanded ? 'Collapse' : 'Expand to see linked entities'}
                              >
                                <span className={`expand-icon ${isExpanded ? 'expanded' : ''}`}>
                                  &#9658;
                                </span>
                              </button>
                            </td>
                            <td>{utility.utilityName}</td>
                            <td>{utility.utilityType}</td>
                            <td>{utility.utilityCode || '-'}</td>
                            <td>{utility.utilityDescription || '-'}</td>
                            <td>
                              <span className={`status-badge ${utility.utilityIsActive ? 'active' : 'inactive'}`}>
                                {utility.utilityIsActive ? 'Active' : 'Inactive'}
                              </span>
                            </td>
                          </tr>
                          {isExpanded && (
                            <tr className="expanded-row">
                              <td colSpan={6}>
                                <div className="expanded-content">
                                  {isLoading ? (
                                    <div className="loading-inline">Loading...</div>
                                  ) : !hasRelationships ? (
                                    <div className="no-entities-text">No linked entities found</div>
                                  ) : (
                                    <div className="bill-relationships">
                                      {relationships.properties.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Properties:</div>
                                          <div className="expanded-entities">
                                            {relationships.properties.map((property) => (
                                              <div
                                                key={property.propertyId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#3B82F6' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProperty(property); }}
                                                title={`View ${property.propertyName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#3B82F6' }}>
                                                  property
                                                </span>
                                                <span className="entity-name">{property.propertyName}</span>
                                                {property.propertyAddress && <span className="entity-detail">{property.propertyAddress}</span>}
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.providers.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Providers:</div>
                                          <div className="expanded-entities">
                                            {relationships.providers.map((provider) => (
                                              <div
                                                key={provider.providerId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#10B981' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToProvider(provider); }}
                                                title={`View ${provider.providerName}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#10B981' }}>
                                                  provider
                                                </span>
                                                <span className="entity-name">{provider.providerName}</span>
                                                {provider.providerType && <span className="entity-detail">{provider.providerType}</span>}
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      )}
                                      {relationships.bills.length > 0 && (
                                        <div className="relationship-section">
                                          <div className="relationship-label">Bills ({relationships.bills.length}):</div>
                                          <div className="expanded-entities">
                                            {relationships.bills.slice(0, 5).map((bill) => (
                                              <div
                                                key={bill.billId}
                                                className="expanded-entity-card clickable"
                                                style={{ borderLeftColor: '#8B5CF6' }}
                                                onClick={(e) => { e.stopPropagation(); navigateToBill(bill); }}
                                                title={`View ${bill.billReferenceId}`}
                                              >
                                                <span className="entity-type-indicator" style={{ backgroundColor: '#8B5CF6' }}>
                                                  bill
                                                </span>
                                                <span className="entity-name">{bill.billReferenceId}</span>
                                                <span className="entity-detail">{formatCurrency(bill.billAmount)}</span>
                                              </div>
                                            ))}
                                            {relationships.bills.length > 5 && (
                                              <div className="expanded-entity-card" style={{ borderLeftColor: '#6b7280' }}>
                                                <span className="entity-name">+{relationships.bills.length - 5} more</span>
                                              </div>
                                            )}
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </div>
                              </td>
                            </tr>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </tbody>
                </table>
                <Pagination
                  currentPage={page}
                  totalCount={totalItemCount}
                  pageSize={pageSize}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  isLoading={isLoadingItems}
                />
              </div>
            )
          ) : linkedItems.length === 0 ? (
            <div className="empty-state">
              <p>No items linked to entities in this category.</p>
            </div>
          ) : (
            <div className="data-grid-container">
              <VirtualizedItemTable
                items={linkedItems}
                selectedItem={selectedItem}
                focusedItemId={focusedItemId}
                focusedEntity={focusedEntity}
                onItemSelect={setSelectedItem}
                onEntityClick={handleEntityClick}
                onClearFocus={handleClearFocus}
              />
              <Pagination
                currentPage={page}
                totalCount={totalItemCount}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
                isLoading={isLoadingItems}
              />
            </div>
          )}
        </div>
      )}

      {showAllEntities && !selectedEntity && !selectedEntityType && (
        <div className="search-results">
          <div className="results-header">
            <div className="selected-entity-info">
              <span className="entity-type-badge large" style={{ backgroundColor: '#003366' }}>
                All
              </span>
              <h2>All Entities</h2>
              <p className="entity-description">Showing all items across all entities</p>
            </div>
            <div className="results-summary">
              <div className="summary-stat">
                <span className="stat-value">{allEntities.length}</span>
                <span className="stat-label">Entities</span>
              </div>
              <div className="summary-stat">
                <span className="stat-value">{linkedItems.length}</span>
                <span className="stat-label">Items</span>
              </div>
              <div className="summary-stat">
                <span className="stat-value">{formatCurrency(totalAmount)}</span>
                <span className="stat-label">Total</span>
              </div>
            </div>
          </div>

          {isLoadingItems ? (
            <div className="loading">Loading items...</div>
          ) : linkedItems.length === 0 ? (
            <div className="empty-state">
              <p>No items found.</p>
            </div>
          ) : (
            <div className="data-grid-container">
              <VirtualizedItemTable
                items={linkedItems}
                selectedItem={selectedItem}
                focusedItemId={focusedItemId}
                focusedEntity={focusedEntity}
                onItemSelect={setSelectedItem}
                onEntityClick={handleEntityClick}
                onClearFocus={handleClearFocus}
                showStatusColumn={true}
              />
              <Pagination
                currentPage={page}
                totalCount={totalItemCount}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
                isLoading={isLoadingItems}
              />
            </div>
          )}
        </div>
      )}

      {!showAllEntities && !selectedEntity && !selectedEntityType && (
        <>
          <div className="dashboard-grid">
            <div className="dashboard-card insights-card">
              <div className="card-header">
                <h3>Insights</h3>
              </div>
              <div className="card-content">
                <div className="insight-item">
                  <span className="insight-icon trend-up">↑</span>
                  <div className="insight-text">
                    <strong>12% increase</strong> in utility costs this month compared to last month
                  </div>
                </div>
                <div className="insight-item">
                  <span className="insight-icon info">i</span>
                  <div className="insight-text">
                    <strong>3 properties</strong> have pending invoices over 30 days
                  </div>
                </div>
                <div className="insight-item">
                  <span className="insight-icon trend-down">↓</span>
                  <div className="insight-text">
                    <strong>8% decrease</strong> in water usage across all properties
                  </div>
                </div>
                <div className="insight-item">
                  <span className="insight-icon info">i</span>
                  <div className="insight-text">
                    <strong>{allEntities.length} entities</strong> tracked across {entityTypes.length} categories
                  </div>
                </div>
              </div>
            </div>

            <div className="dashboard-card actions-card">
              <div className="card-header">
                <h3>Actions Needed</h3>
                <span className="action-count">5</span>
              </div>
              <div className="card-content">
                <div className="action-item priority-high">
                  <span className="priority-indicator"></span>
                  <div className="action-text">
                    <strong>Review invoice</strong>
                    <span>Pinecrest - Electric bill requires approval</span>
                  </div>
                  <button className="action-btn">Review</button>
                </div>
                <div className="action-item priority-high">
                  <span className="priority-indicator"></span>
                  <div className="action-text">
                    <strong>Reconcile payment</strong>
                    <span>Oak Manor - Payment discrepancy detected</span>
                  </div>
                  <button className="action-btn">Resolve</button>
                </div>
                <div className="action-item priority-medium">
                  <span className="priority-indicator"></span>
                  <div className="action-text">
                    <strong>Update vendor info</strong>
                    <span>City Water Co - Contact information outdated</span>
                  </div>
                  <button className="action-btn">Update</button>
                </div>
                <div className="action-item priority-low">
                  <span className="priority-indicator"></span>
                  <div className="action-text">
                    <strong>Schedule review</strong>
                    <span>Quarterly utility audit due next week</span>
                  </div>
                  <button className="action-btn">Schedule</button>
                </div>
                <div className="action-item priority-low">
                  <span className="priority-indicator"></span>
                  <div className="action-text">
                    <strong>Export report</strong>
                    <span>Monthly summary ready for download</span>
                  </div>
                  <button className="action-btn">Export</button>
                </div>
              </div>
            </div>
          </div>

          <div className="entity-types-section">
            <div className="entity-types-header">
              <h2>Browse by Category</h2>
            </div>
          {isLoadingEntities ? (
            <div className="loading">Loading...</div>
          ) : (
            <div className="entity-types-grid">
              {entityTypes.map((entityType) => (
                <div
                  key={entityType.id}
                  className="entity-type-card"
                  style={{ borderTopColor: entityType.color || '#6b7280' }}
                >
                  <div className="entity-type-card-header">
                    <h3 style={{ color: entityType.color || '#6b7280' }}>{entityType.name}</h3>
                    <span className="entity-count-badge">{entityType.entities.length}</span>
                  </div>
                  {entityType.description && (
                    <p className="entity-type-description">{entityType.description}</p>
                  )}
                  <div className="entity-list">
                    {entityType.entities.slice(0, 5).map((entity) => (
                      <button
                        key={entity.id}
                        className="entity-list-item"
                        onClick={() => handleEntitySelect({
                          ...entity,
                          entityTypeName: entityType.name,
                          entityTypeColor: entityType.color,
                        })}
                      >
                        {entity.name}
                      </button>
                    ))}
                    {entityType.entities.length > 5 && (
                      <span className="more-entities">+{entityType.entities.length - 5} more</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
          </div>
        </>
      )}

      {/* Property Detail Panel */}
      {selectedProperty && (
        <div className="item-detail-panel">
          <div className="panel-header">
            <h3>Property Details</h3>
            <button onClick={() => setSelectedProperty(null)} className="btn-close">&times;</button>
          </div>
          <div className="panel-content">
            <div className="detail-row">
              <label>Name</label>
              <span>{selectedProperty.propertyName}</span>
            </div>
            <div className="detail-row">
              <label>Client ID</label>
              <span>{selectedProperty.propertyClientId}</span>
            </div>
            <div className="detail-row">
              <label>Address</label>
              <span>{selectedProperty.propertyAddress}</span>
            </div>
            <div className="detail-row">
              <label>City</label>
              <span>{selectedProperty.propertyCity || '-'}</span>
            </div>
            <div className="detail-row">
              <label>State</label>
              <span>{selectedProperty.propertyState || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Zip Code</label>
              <span>{selectedProperty.propertyZipCode || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Type</label>
              <span>{selectedProperty.propertyType}</span>
            </div>
            <div className="detail-row">
              <label>Units</label>
              <span>{selectedProperty.propertyUnitCount || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Square Footage</label>
              <span>{selectedProperty.propertySquareFootage?.toLocaleString() || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Status</label>
              <span className={`status-badge ${selectedProperty.propertyIsActive ? 'active' : 'inactive'}`}>
                {selectedProperty.propertyIsActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          </div>
        </div>
      )}

      {/* Provider/Vendor Detail Panel */}
      {selectedProvider && (
        <div className="item-detail-panel">
          <div className="panel-header">
            <h3>Vendor Details</h3>
            <button onClick={() => setSelectedProvider(null)} className="btn-close">&times;</button>
          </div>
          <div className="panel-content">
            <div className="detail-row">
              <label>Name</label>
              <span>{selectedProvider.providerName}</span>
            </div>
            <div className="detail-row">
              <label>Account ID</label>
              <span>{selectedProvider.providerAccountId}</span>
            </div>
            <div className="detail-row">
              <label>Type</label>
              <span>{selectedProvider.providerType || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Phone</label>
              <span>{selectedProvider.providerPhone || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Email</label>
              <span>{selectedProvider.providerEmail || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Website</label>
              <span>{selectedProvider.providerWebsite || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Contact</label>
              <span>{selectedProvider.providerContactName || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Address</label>
              <span>
                {selectedProvider.providerAddress ? (
                  <>
                    {selectedProvider.providerAddress}<br />
                    {selectedProvider.providerCity}, {selectedProvider.providerState} {selectedProvider.providerZipCode}
                  </>
                ) : '-'}
              </span>
            </div>
            <div className="detail-row">
              <label>Status</label>
              <span className={`status-badge ${selectedProvider.providerIsActive ? 'active' : 'inactive'}`}>
                {selectedProvider.providerIsActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          </div>
        </div>
      )}

      {/* Bill Detail Panel */}
      {selectedBill && (
        <div className="item-detail-panel">
          <div className="panel-header">
            <h3>Bill Details</h3>
            <button onClick={() => setSelectedBill(null)} className="btn-close">&times;</button>
          </div>
          <div className="panel-content">
            <div className="detail-row">
              <label>Reference ID</label>
              <span>{selectedBill.billReferenceId}</span>
            </div>
            <div className="detail-row">
              <label>Amount</label>
              <span className="amount-value">{formatCurrency(selectedBill.billAmount)}</span>
            </div>
            <div className="detail-row">
              <label>Status</label>
              <span className={`status-badge ${selectedBill.billStatus?.toLowerCase().replace(/\s+/g, '-') || 'pending'}`}>
                {selectedBill.billStatus || 'Pending'}
              </span>
            </div>
            <div className="detail-row">
              <label>Utility Type</label>
              <span>{selectedBill.utilityName || selectedBill.billCategory || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Vendor</label>
              <span>{selectedBill.providerName || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Bill Date</label>
              <span>{selectedBill.billDate ? formatDate(selectedBill.billDate) : '-'}</span>
            </div>
            <div className="detail-row">
              <label>Due Date</label>
              <span>{selectedBill.billDueDate ? formatDate(selectedBill.billDueDate) : '-'}</span>
            </div>
            {selectedBill.billPaidDate && (
              <div className="detail-row">
                <label>Paid Date</label>
                <span>{formatDate(selectedBill.billPaidDate)}</span>
              </div>
            )}
            <div className="detail-row">
              <label>Period</label>
              <span>
                {selectedBill.billPeriodStart && selectedBill.billPeriodEnd
                  ? `${selectedBill.billPeriodStart} - ${selectedBill.billPeriodEnd}`
                  : '-'}
              </span>
            </div>
            {selectedBill.billQuantity && (
              <div className="detail-row">
                <label>Usage</label>
                <span>{selectedBill.billQuantity.toLocaleString()} {selectedBill.billUnit || ''}</span>
              </div>
            )}
            {selectedBill.billRate && (
              <div className="detail-row">
                <label>Rate</label>
                <span>{formatCurrency(selectedBill.billRate)} / {selectedBill.billUnit || 'unit'}</span>
              </div>
            )}
            {selectedBill.billDescription && (
              <div className="detail-row">
                <label>Description</label>
                <span>{selectedBill.billDescription}</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Utility Detail Panel */}
      {selectedUtility && (
        <div className="item-detail-panel">
          <div className="panel-header">
            <h3>Utility Details</h3>
            <button onClick={() => setSelectedUtility(null)} className="btn-close">&times;</button>
          </div>
          <div className="panel-content">
            <div className="detail-row">
              <label>Name</label>
              <span>{selectedUtility.utilityName}</span>
            </div>
            <div className="detail-row">
              <label>Type</label>
              <span>{selectedUtility.utilityType}</span>
            </div>
            <div className="detail-row">
              <label>Code</label>
              <span>{selectedUtility.utilityCode || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Description</label>
              <span>{selectedUtility.utilityDescription || '-'}</span>
            </div>
            <div className="detail-row">
              <label>Status</label>
              <span className={`status-badge ${selectedUtility.utilityIsActive ? 'active' : 'inactive'}`}>
                {selectedUtility.utilityIsActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
