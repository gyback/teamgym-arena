import type { QueryClient } from '@tanstack/react-query';
import { createRouter, type RouterHistory } from '@tanstack/react-router';

import { ErrorPage } from './layout/ErrorPage';
import { routeTree } from './routeTree.gen';

export interface RouterContext {
  queryClient: QueryClient;
}

/** Router options that do not depend on the route tree, shared with tests that build their own tree. */
export const routerDefaults = {
  // Child routes without their own error component render this one inside the root layout's <Outlet />, so
  // the header, the language switcher and the main landmark stay. The root route's errorComponent only
  // handles errors in the layout itself.
  defaultErrorComponent: ErrorPage,
  // Route loaders read through TanStack Query, so the router itself never caches loader data.
  defaultPreload: 'intent',
  defaultPreloadStaleTime: 0,
  scrollRestoration: true,
} as const;

export function createAppRouter(context: RouterContext, history?: RouterHistory) {
  return createRouter({
    ...routerDefaults,
    routeTree,
    context,
    ...(history ? { history } : {}),
  });
}

declare module '@tanstack/react-router' {
  interface Register {
    router: ReturnType<typeof createAppRouter>;
  }
}
