import axios from 'axios';
import type {
  LoginResponse,
  Item,
  ItemListResponse,
  ItemsFilter,
  CreateItemRequest,
  EntityType,
  Entity,
  EntityTypeWithEntities,
  EntityWithLinks,
  CreateEntityTypeRequest,
  CreateEntityRequest,
  CreateEntityLinkRequest,
  EntityLink,
  EntitySearchFilter,
  EntitySearchResponse,
  Property,
  CreatePropertyRequest,
  PropertiesFilter,
  PropertyListResponse,
  PropertyRelationships,
  Bill,
  CreateBillRequest,
  BillsFilter,
  BillListResponse,
  BillRelationships,
  Provider,
  CreateProviderRequest,
  ProvidersFilter,
  ProviderListResponse,
  ProviderRelationships,
  Utility,
  CreateUtilityRequest,
  UtilitiesFilter,
  UtilityListResponse,
  UtilityRelationships,
  Resident,
  CreateResidentRequest,
  ResidentsFilter,
  ResidentListResponse,
  BulkImportResidentsRequest,
  ResidentImportResponse,
  ResidentPushRequest,
  ResidentPushResponse
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5141/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', { email, password });
    return response.data;
  },

  me: async () => {
    const response = await api.get('/auth/me');
    return response.data;
  },
};

// Entity Types API
export const entityTypesApi = {
  list: async (): Promise<EntityType[]> => {
    const response = await api.get<EntityType[]>('/entity-types');
    return response.data;
  },

  listWithEntities: async (): Promise<EntityTypeWithEntities[]> => {
    const response = await api.get<EntityTypeWithEntities[]>('/entity-types/with-entities');
    return response.data;
  },

  get: async (id: string): Promise<EntityType> => {
    const response = await api.get<EntityType>(`/entity-types/${id}`);
    return response.data;
  },

  create: async (data: CreateEntityTypeRequest): Promise<EntityType> => {
    const response = await api.post<EntityType>('/entity-types', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateEntityTypeRequest>): Promise<EntityType> => {
    const response = await api.put<EntityType>(`/entity-types/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/entity-types/${id}`);
  },
};

// Entities API
export const entitiesApi = {
  list: async (entityTypeId?: string, includeInactive = false): Promise<Entity[]> => {
    const params = new URLSearchParams();
    if (entityTypeId) params.append('entityTypeId', entityTypeId);
    if (includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<Entity[]>(`/entities?${params.toString()}`);
    return response.data;
  },

  search: async (filters: EntitySearchFilter = {}): Promise<EntitySearchResponse> => {
    const params = new URLSearchParams();
    if (filters.search) params.append('search', filters.search);
    if (filters.entityTypeId) params.append('entityTypeId', filters.entityTypeId);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<EntitySearchResponse>(`/entities/search?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<EntityWithLinks> => {
    const response = await api.get<EntityWithLinks>(`/entities/${id}`);
    return response.data;
  },

  create: async (data: CreateEntityRequest): Promise<Entity> => {
    const response = await api.post<Entity>('/entities', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateEntityRequest>): Promise<Entity> => {
    const response = await api.put<Entity>(`/entities/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/entities/${id}`);
  },

  getLinks: async (id: string): Promise<EntityLink[]> => {
    const response = await api.get<EntityLink[]>(`/entities/${id}/links`);
    return response.data;
  },

  createLink: async (id: string, data: CreateEntityLinkRequest): Promise<EntityLink> => {
    const response = await api.post<EntityLink>(`/entities/${id}/links`, data);
    return response.data;
  },

  deleteLink: async (linkId: string): Promise<void> => {
    await api.delete(`/entities/links/${linkId}`);
  },
};

// Items API - now using entity-based filtering
export const itemsApi = {
  list: async (filters: ItemsFilter = {}): Promise<ItemListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.startDate) params.append('startDate', filters.startDate);
    if (filters.endDate) params.append('endDate', filters.endDate);
    if (filters.entityId) params.append('entityId', filters.entityId);
    if (filters.entityIds && filters.entityIds.length > 0) {
      filters.entityIds.forEach(id => params.append('entityIds', id));
    }
    if (filters.entityMatch) params.append('entityMatch', filters.entityMatch);
    if (filters.search) params.append('search', filters.search);
    const response = await api.get<ItemListResponse>(`/items?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Item> => {
    const response = await api.get<Item>(`/items/${id}`);
    return response.data;
  },

  create: async (data: CreateItemRequest): Promise<Item> => {
    const response = await api.post<Item>('/items', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateItemRequest>): Promise<Item> => {
    const response = await api.put<Item>(`/items/${id}`, data);
    return response.data;
  },

  updateEntities: async (id: string, entityIds: string[]): Promise<Item> => {
    const response = await api.put<Item>(`/items/${id}/entities`, { entityIds });
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/items/${id}`);
  },
};

// Properties API
export const propertiesApi = {
  list: async (filters: PropertiesFilter = {}): Promise<PropertyListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.type) params.append('type', filters.type);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<PropertyListResponse>(`/properties?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Property> => {
    const response = await api.get<Property>(`/properties/${id}`);
    return response.data;
  },

  create: async (data: CreatePropertyRequest): Promise<Property> => {
    const response = await api.post<Property>('/properties', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreatePropertyRequest> & { propertyIsActive?: boolean }): Promise<Property> => {
    const response = await api.put<Property>(`/properties/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/properties/${id}`);
  },

  getRelationships: async (id: string): Promise<PropertyRelationships> => {
    const response = await api.get<PropertyRelationships>(`/properties/${id}/relationships`);
    return response.data;
  },
};

// Bills API
export const billsApi = {
  list: async (filters: BillsFilter = {}): Promise<BillListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.status) params.append('status', filters.status);
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<BillListResponse>(`/bills?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Bill> => {
    const response = await api.get<Bill>(`/bills/${id}`);
    return response.data;
  },

  create: async (data: CreateBillRequest): Promise<Bill> => {
    const response = await api.post<Bill>('/bills', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateBillRequest> & { billIsActive?: boolean }): Promise<Bill> => {
    const response = await api.put<Bill>(`/bills/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/bills/${id}`);
  },

  getRelationships: async (id: string): Promise<BillRelationships> => {
    const response = await api.get<BillRelationships>(`/bills/${id}/relationships`);
    return response.data;
  },
};

// Providers API
export const providersApi = {
  list: async (filters: ProvidersFilter = {}): Promise<ProviderListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.type) params.append('type', filters.type);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<ProviderListResponse>(`/providers?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Provider> => {
    const response = await api.get<Provider>(`/providers/${id}`);
    return response.data;
  },

  create: async (data: CreateProviderRequest): Promise<Provider> => {
    const response = await api.post<Provider>('/providers', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateProviderRequest> & { providerIsActive?: boolean }): Promise<Provider> => {
    const response = await api.put<Provider>(`/providers/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/providers/${id}`);
  },

  getRelationships: async (id: string): Promise<ProviderRelationships> => {
    const response = await api.get<ProviderRelationships>(`/providers/${id}/relationships`);
    return response.data;
  },
};

// Utilities API
export const utilitiesApi = {
  list: async (filters: UtilitiesFilter = {}): Promise<UtilityListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.type) params.append('type', filters.type);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<UtilityListResponse>(`/utilities?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Utility> => {
    const response = await api.get<Utility>(`/utilities/${id}`);
    return response.data;
  },

  create: async (data: CreateUtilityRequest): Promise<Utility> => {
    const response = await api.post<Utility>('/utilities', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateUtilityRequest> & { utilityIsActive?: boolean }): Promise<Utility> => {
    const response = await api.put<Utility>(`/utilities/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/utilities/${id}`);
  },

  getRelationships: async (id: string): Promise<UtilityRelationships> => {
    const response = await api.get<UtilityRelationships>(`/utilities/${id}/relationships`);
    return response.data;
  },
};

// Mock Sync API
export interface MockSyncResponse {
  itemCount: number;
  batchId: string;
  syncedAt: string;
  message: string;
}

// Reports API
export const reportsApi = {
  list: async (filters: {
    page?: number;
    pageSize?: number;
    type?: string;
    search?: string;
    includeShared?: boolean;
  } = {}): Promise<{
    reports: Array<{
      id: string;
      name: string;
      type: string;
      configuration: string;
      isShared: boolean;
      createdById: string;
      createdByName?: string;
      createdAt: string;
    }>;
    totalCount: number;
    page: number;
    pageSize: number;
  }> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.type) params.append('type', filters.type);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeShared !== undefined) params.append('includeShared', filters.includeShared.toString());
    const response = await api.get(`/reports?${params.toString()}`);
    return response.data;
  },

  get: async (id: string) => {
    const response = await api.get(`/reports/${id}`);
    return response.data;
  },

  create: async (data: {
    name: string;
    type: string;
    configuration: string;
    isShared?: boolean;
  }) => {
    const response = await api.post('/reports', data);
    return response.data;
  },

  update: async (id: string, data: {
    name: string;
    type: string;
    configuration: string;
    isShared: boolean;
  }) => {
    const response = await api.put(`/reports/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/reports/${id}`);
  },

  duplicate: async (id: string) => {
    const response = await api.post(`/reports/${id}/duplicate`);
    return response.data;
  },

  export: async (id: string, format: 'csv' | 'xlsx' = 'csv'): Promise<Blob> => {
    const response = await api.post(`/reports/${id}/export?format=${format}`, null, {
      responseType: 'blob',
    });
    return response.data;
  },
};

export const mockSyncApi = {
  triggerSync: async (): Promise<MockSyncResponse> => {
    const response = await api.post<MockSyncResponse>('/mock-sync');
    return response.data;
  },
};

// Integrations API
import type {
  AccountingSoftware,
  AccountingSoftwareListResponse,
  ClientAccountingConnection,
  ClientAccountingConnectionDetail,
  CreateConnectionRequest,
  ConnectionListResponse,
  TestConnectionResult,
  ReportSchedule,
  CreateScheduleRequest,
  ScheduleListResponse,
  ReportScheduleRun,
  ScheduleRunListResponse,
} from '../types';

export const integrationsApi = {
  // Accounting Software
  listSoftware: async (activeOnly = true): Promise<AccountingSoftwareListResponse> => {
    const params = new URLSearchParams();
    if (activeOnly) params.append('activeOnly', 'true');
    const response = await api.get<AccountingSoftwareListResponse>(`/integrations/software?${params.toString()}`);
    return response.data;
  },

  getSoftware: async (id: string): Promise<AccountingSoftware> => {
    const response = await api.get<AccountingSoftware>(`/integrations/software/${id}`);
    return response.data;
  },

  // Connections
  listConnections: async (filters: {
    page?: number;
    pageSize?: number;
    softwareId?: string;
    isActive?: boolean;
  } = {}): Promise<ConnectionListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.softwareId) params.append('softwareId', filters.softwareId);
    if (filters.isActive !== undefined) params.append('isActive', filters.isActive.toString());
    const response = await api.get<ConnectionListResponse>(`/integrations/connections?${params.toString()}`);
    return response.data;
  },

  getConnection: async (id: string): Promise<ClientAccountingConnectionDetail> => {
    const response = await api.get<ClientAccountingConnectionDetail>(`/integrations/connections/${id}`);
    return response.data;
  },

  createConnection: async (data: CreateConnectionRequest): Promise<ClientAccountingConnectionDetail> => {
    const response = await api.post<ClientAccountingConnectionDetail>('/integrations/connections', data);
    return response.data;
  },

  updateConnection: async (id: string, data: Partial<CreateConnectionRequest> & { isActive?: boolean }): Promise<ClientAccountingConnectionDetail> => {
    const response = await api.put<ClientAccountingConnectionDetail>(`/integrations/connections/${id}`, data);
    return response.data;
  },

  deleteConnection: async (id: string): Promise<void> => {
    await api.delete(`/integrations/connections/${id}`);
  },

  testConnection: async (id: string): Promise<TestConnectionResult> => {
    const response = await api.post<TestConnectionResult>(`/integrations/connections/${id}/test`);
    return response.data;
  },

  // Schedules
  listSchedules: async (filters: {
    page?: number;
    pageSize?: number;
    reportId?: string;
    connectionId?: string;
    isActive?: boolean;
  } = {}): Promise<ScheduleListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.reportId) params.append('reportId', filters.reportId);
    if (filters.connectionId) params.append('connectionId', filters.connectionId);
    if (filters.isActive !== undefined) params.append('isActive', filters.isActive.toString());
    const response = await api.get<ScheduleListResponse>(`/integrations/schedules?${params.toString()}`);
    return response.data;
  },

  getSchedule: async (id: string): Promise<ReportSchedule> => {
    const response = await api.get<ReportSchedule>(`/integrations/schedules/${id}`);
    return response.data;
  },

  createSchedule: async (data: CreateScheduleRequest): Promise<ReportSchedule> => {
    const response = await api.post<ReportSchedule>('/integrations/schedules', data);
    return response.data;
  },

  updateSchedule: async (id: string, data: Partial<CreateScheduleRequest> & { isActive?: boolean }): Promise<ReportSchedule> => {
    const response = await api.put<ReportSchedule>(`/integrations/schedules/${id}`, data);
    return response.data;
  },

  deleteSchedule: async (id: string): Promise<void> => {
    await api.delete(`/integrations/schedules/${id}`);
  },

  toggleSchedule: async (id: string): Promise<ReportSchedule> => {
    const response = await api.post<ReportSchedule>(`/integrations/schedules/${id}/toggle`);
    return response.data;
  },

  triggerScheduleRun: async (id: string, force = false): Promise<ReportScheduleRun> => {
    const response = await api.post<ReportScheduleRun>(`/integrations/schedules/${id}/run`, { force });
    return response.data;
  },

  // Schedule Runs
  listScheduleRuns: async (scheduleId: string, filters: {
    page?: number;
    pageSize?: number;
  } = {}): Promise<ScheduleRunListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    const response = await api.get<ScheduleRunListResponse>(`/integrations/schedules/${scheduleId}/runs?${params.toString()}`);
    return response.data;
  },

  getRun: async (id: string): Promise<ReportScheduleRun> => {
    const response = await api.get<ReportScheduleRun>(`/integrations/runs/${id}`);
    return response.data;
  },
};

// Residents API
export const residentsApi = {
  list: async (filters: ResidentsFilter = {}): Promise<ResidentListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.propertyId) params.append('propertyId', filters.propertyId);
    if (filters.status) params.append('status', filters.status);
    if (filters.search) params.append('search', filters.search);
    if (filters.includeInactive) params.append('includeInactive', 'true');
    const response = await api.get<ResidentListResponse>(`/residents?${params.toString()}`);
    return response.data;
  },

  get: async (id: string): Promise<Resident> => {
    const response = await api.get<Resident>(`/residents/${id}`);
    return response.data;
  },

  create: async (data: CreateResidentRequest): Promise<Resident> => {
    const response = await api.post<Resident>('/residents', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateResidentRequest> & { residentIsActive?: boolean }): Promise<Resident> => {
    const response = await api.put<Resident>(`/residents/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/residents/${id}`);
  },

  import: async (data: BulkImportResidentsRequest): Promise<ResidentImportResponse> => {
    const response = await api.post<ResidentImportResponse>('/residents/import', data);
    return response.data;
  },

  getBatch: async (batchId: string, filters: { page?: number; pageSize?: number } = {}): Promise<ResidentListResponse> => {
    const params = new URLSearchParams();
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    const response = await api.get<ResidentListResponse>(`/residents/batch/${batchId}?${params.toString()}`);
    return response.data;
  },

  push: async (data: ResidentPushRequest): Promise<ResidentPushResponse> => {
    const response = await api.post<ResidentPushResponse>('/residents/push', data);
    return response.data;
  },
};

export default api;
