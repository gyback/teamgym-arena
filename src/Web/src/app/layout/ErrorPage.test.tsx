import {
  createMemoryHistory,
  createRootRoute,
  createRoute,
  createRouter,
  RouterProvider,
} from '@tanstack/react-router';
import { act, render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { routerDefaults } from '@/app/router';

import { RootLayout } from './RootLayout';

// The page route has no errorComponent of its own, as in the app: its errors reach ErrorPage through the
// router's defaultErrorComponent.
function renderFailingRoute(loader: () => string) {
  const rootRoute = createRootRoute({ component: RootLayout });
  const pageRoute = createRoute({
    getParentRoute: () => rootRoute,
    path: '/',
    loader,
    component: () => <p>{pageRoute.useLoaderData()}</p>,
  });
  const router = createRouter({
    ...routerDefaults,
    routeTree: rootRoute.addChildren([pageRoute]),
    history: createMemoryHistory({ initialEntries: ['/'] }),
  });

  render(<RouterProvider router={router} />);
  return act(() => router.load());
}

describe('ErrorPage', () => {
  it('shows a translated message without the error details', async () => {
    await renderFailingRoute(() => {
      throw new Error('Connection string: Host=db;Password=secret');
    });

    expect(screen.getByRole('alert')).toHaveTextContent('Something went wrong');
    expect(screen.queryByText(/secret/)).not.toBeInTheDocument();
  });

  it('keeps the app layout around a failing page', async () => {
    await renderFailingRoute(() => {
      throw new Error('Temporary failure');
    });

    expect(within(screen.getByRole('main')).getByRole('alert')).toBeInTheDocument();
    expect(screen.getByRole('banner')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Skip to main content' })).toBeInTheDocument();
    expect(screen.getByRole('group', { name: 'Language' })).toBeInTheDocument();
  });

  it('loads the route again when the user retries', async () => {
    const user = userEvent.setup();
    let attempts = 0;
    await renderFailingRoute(() => {
      attempts += 1;
      if (attempts === 1) {
        throw new Error('Temporary failure');
      }
      return 'Loaded';
    });

    await user.click(screen.getByRole('button', { name: 'Try again' }));

    expect(await screen.findByText('Loaded')).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
