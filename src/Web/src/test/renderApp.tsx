import { createMemoryHistory } from '@tanstack/react-router';
import { act, render } from '@testing-library/react';

import { App } from '@/app/App';
import { createQueryClient } from '@/app/queryClient';
import { createAppRouter } from '@/app/router';

/** Renders the whole app, router included, at the given path and waits for the route to load. */
export async function renderApp(path = '/') {
  const queryClient = createQueryClient();
  const router = createAppRouter({ queryClient }, createMemoryHistory({ initialEntries: [path] }));

  const result = render(<App queryClient={queryClient} router={router} />);
  await act(() => router.load());

  return { ...result, router };
}
