export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

// Entity model - flexible, non-hierarchical objects linked via bridge tables
export interface EntityType {
  id: string;
  name: string;
  code: string;
  description?: string;
  color?: string;
  icon?: string;
  sortOrder: number;
  isSystem: boolean;
  entityCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Entity {
  id: string;
  entityTypeId: string;
  entityTypeName: string;
  entityTypeCode: string;
  entityTypeColor?: string;
  name: string;
  code: string;
  description?: string;
  attributes?: Record<string, unknown>;
  sortOrder: number;
  isActive: boolean;
  isSystem: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface EntityTypeWithEntities extends EntityType {
  entities: Entity[];
  totalEntityCount: number;
}

export interface EntityLink {
  id: string;
  sourceEntityId: string;
  sourceEntityName: string;
  sourceEntityCode: string;
  targetEntityId: string;
  targetEntityName: string;
  targetEntityCode: string;
  linkType: string;
  description?: string;
  createdAt: string;
}

export interface EntityWithLinks extends Entity {
  links: EntityLink[];
}

export interface CreateEntityTypeRequest {
  name: string;
  code: string;
  description?: string;
  color?: string;
  icon?: string;
  sortOrder?: number;
}

export interface CreateEntityRequest {
  entityTypeId: string;
  name: string;
  code: string;
  description?: string;
  attributes?: Record<string, unknown>;
  sortOrder?: number;
}

export interface CreateEntityLinkRequest {
  sourceEntityId: string;
  targetEntityId: string;
  linkType: string;
  description?: string;
}

export interface EntitySearchFilter extends PaginationParams {
  search?: string;
  entityTypeId?: string;
  includeInactive?: boolean;
}

export interface EntitySearchResponse {
  entities: Entity[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Entity tag for displaying on items
export interface EntityTag {
  id: string;
  name: string;
  code: string;
  entityTypeName: string;
  entityTypeCode: string;
  color?: string;
}

// Item with linked entities
export interface Item {
  id: string;
  entities: EntityTag[];
  externalId?: string;
  date: string;
  amount: number;
  quantity?: number;
  description?: string;
  attributes?: Record<string, unknown>;
  source: 'Manual' | 'Import' | 'Pipeline';
  batchId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ItemListResponse {
  items: Item[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ItemsFilter {
  page?: number;
  pageSize?: number;
  startDate?: string;
  endDate?: string;
  entityId?: string;
  entityIds?: string[];
  entityMatch?: 'any' | 'all';
  search?: string;
}

export interface CreateItemRequest {
  externalId?: string;
  date: string;
  amount: number;
  quantity?: number;
  description?: string;
  attributes?: Record<string, unknown>;
  entityIds?: string[];
}

// Pagination types
export interface PaginationParams {
  page?: number;
  pageSize?: number;
}

// Domain Entities
export interface Property {
  propertyId: string;
  propertyClientId: string;
  propertyName: string;
  propertyAddress: string;
  propertyCity?: string;
  propertyState?: string;
  propertyZipCode?: string;
  propertyCountry?: string;
  propertyType: string;
  propertyCode?: string;
  propertyDescription?: string;
  propertyUnitCount?: number;
  propertySquareFootage?: number;
  propertyIsActive: boolean;
  propertyCreatedAt: string;
  propertyUpdatedAt: string;
}

export interface CreatePropertyRequest {
  propertyClientId: string;
  propertyName: string;
  propertyAddress: string;
  propertyCity?: string;
  propertyState?: string;
  propertyZipCode?: string;
  propertyCountry?: string;
  propertyType: string;
  propertyCode?: string;
  propertyDescription?: string;
  propertyUnitCount?: number;
  propertySquareFootage?: number;
}

export interface PropertiesFilter extends PaginationParams {
  type?: string;
  search?: string;
  includeInactive?: boolean;
}

export interface PropertyListResponse {
  properties: Property[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Bill {
  billId: string;
  billReferenceId: string;
  billAmount: number;
  billStatus: string;
  billDescription?: string;
  billDate?: string;
  billDueDate?: string;
  billPaidDate?: string;
  billPeriodStart?: string;
  billPeriodEnd?: string;
  billQuantity?: number;
  billUnit?: string;
  billRate?: number;
  billTax?: number;
  billTotalAmount?: number;
  billCurrency?: string;
  billNotes?: string;
  billCategory?: string;
  billExternalId?: string;
  billIsActive: boolean;
  billCreatedAt: string;
  billUpdatedAt: string;
  utilityName?: string;
  providerName?: string;
}

export interface CreateBillRequest {
  billReferenceId: string;
  billAmount: number;
  billStatus: string;
  billDescription?: string;
  billDate?: string;
  billDueDate?: string;
  billPeriodStart?: string;
  billPeriodEnd?: string;
  billQuantity?: number;
  billUnit?: string;
  billRate?: number;
  billTax?: number;
  billTotalAmount?: number;
  billCurrency?: string;
  billNotes?: string;
  billExternalId?: string;
}

export interface BillsFilter extends PaginationParams {
  status?: string;
  fromDate?: string;
  toDate?: string;
  search?: string;
  includeInactive?: boolean;
}

export interface BillListResponse {
  bills: Bill[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Provider {
  providerId: string;
  providerAccountId: string;
  providerName: string;
  providerCode?: string;
  providerDescription?: string;
  providerType?: string;
  providerAddress?: string;
  providerCity?: string;
  providerState?: string;
  providerZipCode?: string;
  providerCountry?: string;
  providerPhone?: string;
  providerEmail?: string;
  providerWebsite?: string;
  providerContactName?: string;
  providerAccountNumber?: string;
  providerIsActive: boolean;
  providerCreatedAt: string;
  providerUpdatedAt: string;
}

export interface CreateProviderRequest {
  providerAccountId: string;
  providerName: string;
  providerCode?: string;
  providerDescription?: string;
  providerType?: string;
  providerAddress?: string;
  providerCity?: string;
  providerState?: string;
  providerZipCode?: string;
  providerCountry?: string;
  providerPhone?: string;
  providerEmail?: string;
  providerWebsite?: string;
  providerContactName?: string;
  providerAccountNumber?: string;
}

export interface ProvidersFilter extends PaginationParams {
  type?: string;
  search?: string;
  includeInactive?: boolean;
}

export interface ProviderListResponse {
  providers: Provider[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Utility {
  utilityId: string;
  utilityName: string;
  utilityType: string;
  utilityCode?: string;
  utilityDescription?: string;
  utilityIsActive: boolean;
  utilityCreatedAt: string;
  utilityUpdatedAt: string;
}

export interface CreateUtilityRequest {
  utilityName: string;
  utilityType: string;
  utilityCode?: string;
  utilityDescription?: string;
}

export interface UtilitiesFilter extends PaginationParams {
  type?: string;
  search?: string;
  includeInactive?: boolean;
}

export interface UtilityListResponse {
  utilities: Utility[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Bill Relationship types
export interface BillRelatedProperty {
  propertyId: string;
  propertyName: string;
  propertyAddress: string;
  propertyType: string;
}

export interface BillRelatedProvider {
  providerId: string;
  providerName: string;
  providerType?: string;
}

export interface BillRelatedUtility {
  utilityId: string;
  utilityName: string;
  utilityType: string;
}

export interface BillRelationships {
  properties: BillRelatedProperty[];
  providers: BillRelatedProvider[];
  utilities: BillRelatedUtility[];
}

// Property Relationship types
export interface PropertyRelatedProvider {
  providerId: string;
  providerName: string;
  providerType?: string;
}

export interface PropertyRelatedUtility {
  utilityId: string;
  utilityName: string;
  utilityType: string;
}

export interface PropertyRelatedBill {
  billId: string;
  billReferenceId: string;
  billAmount: number;
  billStatus: string;
  billCategory?: string;
}

export interface PropertyRelationships {
  providers: PropertyRelatedProvider[];
  utilities: PropertyRelatedUtility[];
  bills: PropertyRelatedBill[];
}

// Provider Relationship types
export interface ProviderRelatedProperty {
  propertyId: string;
  propertyName: string;
  propertyAddress?: string;
  propertyType?: string;
}

export interface ProviderRelatedUtility {
  utilityId: string;
  utilityName: string;
  utilityType: string;
}

export interface ProviderRelatedBill {
  billId: string;
  billReferenceId: string;
  billAmount: number;
  billStatus: string;
  billCategory?: string;
}

export interface ProviderRelationships {
  properties: ProviderRelatedProperty[];
  utilities: ProviderRelatedUtility[];
  bills: ProviderRelatedBill[];
}

// Utility Relationship types
export interface UtilityRelatedProperty {
  propertyId: string;
  propertyName: string;
  propertyAddress?: string;
  propertyType?: string;
}

export interface UtilityRelatedProvider {
  providerId: string;
  providerName: string;
  providerType?: string;
}

export interface UtilityRelatedBill {
  billId: string;
  billReferenceId: string;
  billAmount: number;
  billStatus: string;
  billCategory?: string;
}

export interface UtilityRelationships {
  properties: UtilityRelatedProperty[];
  providers: UtilityRelatedProvider[];
  bills: UtilityRelatedBill[];
}

// Report Types
export type ReportType = 'Summary' | 'Detail' | 'Trend' | 'Export';

export interface Report {
  id: string;
  name: string;
  type: ReportType;
  configuration: string;
  isShared: boolean;
  createdById: string;
  createdByName?: string;
  createdAt: string;
}

export interface CreateReportRequest {
  name: string;
  type: ReportType;
  configuration: string;
  isShared?: boolean;
}

export interface UpdateReportRequest {
  name: string;
  type: ReportType;
  configuration: string;
  isShared: boolean;
}

export interface ReportsFilter extends PaginationParams {
  type?: ReportType;
  search?: string;
  includeShared?: boolean;
}

export interface ReportListResponse {
  reports: Report[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Report Configuration for export templates
export type ReportSourceType = 'bills' | 'properties' | 'providers' | 'utilities' | 'items';

export interface ReportColumnMapping {
  source: string;
  header: string;
  format?: string;
}

export interface ReportStaticColumn {
  header: string;
  value: string;
}

export interface ReportAutoIncrement {
  header: string;
  startAt: number;
  prefix?: string;
}

export interface ReportConfiguration {
  sourceType: ReportSourceType;
  filters?: Record<string, unknown>;
  columns: ReportColumnMapping[];
  staticColumns?: ReportStaticColumn[];
  autoIncrement?: ReportAutoIncrement;
  sortBy?: string;
  sortDescending?: boolean;
  outputOrder?: string[];
}

// Integration types
export interface AccountingSoftware {
  id: string;
  name: string;
  code: string;
  description?: string;
  connectionType: string;
  logoUrl?: string;
  isActive: boolean;
}

export interface AccountingSoftwareListResponse {
  software: AccountingSoftware[];
  totalCount: number;
}

export interface ClientAccountingConnection {
  id: string;
  accountingSoftwareId: string;
  accountingSoftwareName: string;
  name: string;
  connectionType: string;
  isActive: boolean;
  lastTestedAt?: string;
  lastTestStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ClientAccountingConnectionDetail {
  id: string;
  accountingSoftwareId: string;
  accountingSoftwareName: string;
  name: string;
  databaseServer?: string;
  databaseName?: string;
  databaseUsername?: string;
  hasDatabasePassword: boolean;
  apiEndpoint?: string;
  hasApiKey: boolean;
  hasApiSecret: boolean;
  sftpHost?: string;
  sftpPort?: number;
  sftpUsername?: string;
  hasSftpPassword: boolean;
  sftpPath?: string;
  isActive: boolean;
  lastTestedAt?: string;
  lastTestStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateConnectionRequest {
  accountingSoftwareId: string;
  name: string;
  connectionString?: string;
  databaseName?: string;
  databaseServer?: string;
  databaseUsername?: string;
  databasePassword?: string;
  apiEndpoint?: string;
  apiKey?: string;
  apiSecret?: string;
  sftpHost?: string;
  sftpPort?: number;
  sftpUsername?: string;
  sftpPassword?: string;
  sftpPath?: string;
  isActive?: boolean;
}

export interface ConnectionListResponse {
  connections: ClientAccountingConnection[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TestConnectionResult {
  success: boolean;
  message: string;
  testedAt: string;
}

export type ScheduleFrequency = 'Once' | 'Daily' | 'Weekly' | 'BiWeekly' | 'Monthly' | 'Quarterly' | 'Custom';
export type ScheduleRunStatus = 'Pending' | 'Running' | 'Success' | 'Failed' | 'Cancelled';

export interface ReportSchedule {
  id: string;
  reportDefinitionId: string;
  reportName: string;
  connectionId: string;
  connectionName: string;
  accountingSoftwareName: string;
  name: string;
  description?: string;
  frequency: ScheduleFrequency;
  cronExpression?: string;
  timeOfDay: string;
  dayOfWeek?: number;
  dayOfMonth?: number;
  startDate: string;
  endDate?: string;
  exportFormat: string;
  destinationTable?: string;
  destinationPath?: string;
  isActive: boolean;
  lastRunAt?: string;
  lastRunStatus?: string;
  nextRunAt?: string;
  createdAt: string;
}

export interface CreateScheduleRequest {
  reportDefinitionId: string;
  connectionId: string;
  name: string;
  description?: string;
  frequency: ScheduleFrequency;
  cronExpression?: string;
  timeOfDay?: string;
  dayOfWeek?: number;
  dayOfMonth?: number;
  startDate?: string;
  endDate?: string;
  exportFormat?: string;
  destinationTable?: string;
  destinationPath?: string;
  isActive?: boolean;
}

export interface ScheduleListResponse {
  schedules: ReportSchedule[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ReportScheduleRun {
  id: string;
  scheduleId: string;
  status: ScheduleRunStatus;
  startedAt?: string;
  completedAt?: string;
  recordsProcessed: number;
  recordsFailed: number;
  errorMessage?: string;
  outputFilePath?: string;
  fileSizeBytes?: number;
}

export interface ScheduleRunListResponse {
  runs: ReportScheduleRun[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Resident Types
export interface Resident {
  residentId: string;
  residentExternalId: string;
  residentFirstName: string;
  residentLastName: string;
  residentMiddleName?: string;
  residentEmail?: string;
  residentPhone?: string;
  residentAlternatePhone?: string;
  residentUnitNumber?: string;
  residentLeaseStart?: string;
  residentLeaseEnd?: string;
  residentMonthlyRent?: number;
  residentAddress?: string;
  residentCity?: string;
  residentState?: string;
  residentZipCode?: string;
  residentStatus: string;
  residentType?: string;
  residentAttributes?: Record<string, unknown>;
  residentBatchId?: string;
  residentIsActive: boolean;
  residentCreatedAt: string;
  residentUpdatedAt: string;
}

export interface CreateResidentRequest {
  residentExternalId: string;
  residentFirstName: string;
  residentLastName: string;
  residentMiddleName?: string;
  residentEmail?: string;
  residentPhone?: string;
  residentAlternatePhone?: string;
  residentUnitNumber?: string;
  residentLeaseStart?: string;
  residentLeaseEnd?: string;
  residentMonthlyRent?: number;
  residentAddress?: string;
  residentCity?: string;
  residentState?: string;
  residentZipCode?: string;
  residentStatus?: string;
  residentType?: string;
  residentAttributes?: Record<string, unknown>;
  propertyId?: string;
}

export interface ResidentsFilter extends PaginationParams {
  propertyId?: string;
  status?: string;
  search?: string;
  includeInactive?: boolean;
}

export interface ResidentListResponse {
  residents: Resident[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Bulk Import Types
export interface BulkResidentInput {
  externalId: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  email?: string;
  phone?: string;
  alternatePhone?: string;
  unitNumber?: string;
  leaseStart?: string;
  leaseEnd?: string;
  monthlyRent?: number;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  status?: string;
  type?: string;
  attributes?: Record<string, unknown>;
}

export interface BulkImportResidentsRequest {
  propertyId: string;
  residents: BulkResidentInput[];
  updateExisting?: boolean;
}

export interface ResidentImportResponse {
  batchId: string;
  propertyId: string;
  totalRequested: number;
  created: number;
  updated: number;
  failed: number;
  errors: ResidentImportError[];
}

export interface ResidentImportError {
  index: number;
  externalId?: string;
  message: string;
}

// Push to Conservice Types
export interface ResidentPushRequest {
  propertyId: string;
  residentIds?: string[];
}

export interface ResidentPushResponse {
  pushId: string;
  propertyId: string;
  residentCount: number;
  status: string;
  pushedAt: string;
}
