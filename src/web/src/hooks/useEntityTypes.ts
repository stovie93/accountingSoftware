import { useQuery } from '@tanstack/react-query';
import { entityTypesApi } from '../services/api';
import type { EntityTypeWithEntities, EntityType } from '../types';

export const entityTypesKeys = {
  all: ['entityTypes'] as const,
  list: () => [...entityTypesKeys.all, 'list'] as const,
  withEntities: () => [...entityTypesKeys.all, 'withEntities'] as const,
  detail: (id: string) => [...entityTypesKeys.all, 'detail', id] as const,
};

export function useEntityTypes() {
  return useQuery<EntityType[]>({
    queryKey: entityTypesKeys.list(),
    queryFn: () => entityTypesApi.list(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useEntityTypesWithEntities() {
  return useQuery<EntityTypeWithEntities[]>({
    queryKey: entityTypesKeys.withEntities(),
    queryFn: () => entityTypesApi.listWithEntities(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
