import { createRootRouteWithContext } from '@tanstack/react-router';

import { ErrorPage } from '@/app/layout/ErrorPage';
import { NotFoundPage } from '@/app/layout/NotFoundPage';
import { RootLayout } from '@/app/layout/RootLayout';
import type { RouterContext } from '@/app/router';

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
  notFoundComponent: NotFoundPage,
  errorComponent: ErrorPage,
});
