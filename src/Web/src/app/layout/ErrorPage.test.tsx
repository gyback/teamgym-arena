import {
  createMemoryHistory,
  createRootRoute,
  createRoute,
  createRouter,
  Outlet,
  RouterProvider,
} from '@tanstack/react-router';
import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { ErrorPage } from './ErrorPage';

function renderFailingRoute(loader: () => string) {
  const rootRoute = createRootRoute({ component: Outlet });
  const pageRoute = createRoute({
    getParentRoute: () => rootRoute,
    path: '/',
    loader,
    component: () => <p>{pageRoute.useLoaderData()}</p>,
    errorComponent: ErrorPage,
  });
  const router = createRouter({
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
