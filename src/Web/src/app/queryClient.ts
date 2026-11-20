import { QueryClient } from '@tanstack/react-query';

export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Avoid refetching on every mount and focus during a competition day; mutations invalidate
        // what they change.
        staleTime: 30_000,
      },
    },
  });
}
