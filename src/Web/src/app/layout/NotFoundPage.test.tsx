import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { axe } from '@/test/axe';
import { renderApp } from '@/test/renderApp';

describe('NotFoundPage', () => {
  it('is shown for an unknown route, with a link back to the home page', async () => {
    const user = userEvent.setup();
    const { router } = await renderApp('/does-not-exist');

    expect(screen.getByRole('heading', { level: 1, name: 'Page not found' })).toBeInTheDocument();

    await user.click(screen.getByRole('link', { name: 'Go to the home page' }));

    expect(await screen.findByRole('heading', { level: 1, name: 'Welcome to TeamGym Arena' })).toBeInTheDocument();
    expect(router.state.location.pathname).toBe('/');
  });

  it('has no detectable accessibility violations', async () => {
    const { container } = await renderApp('/does-not-exist');

    expect(await axe(container)).toHaveNoViolations();
  });
});
