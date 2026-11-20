import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { axe } from '@/test/axe';
import { renderApp } from '@/test/renderApp';

describe('RootLayout', () => {
  it('renders the home page inside the main landmark', async () => {
    await renderApp('/');

    const main = screen.getByRole('main');
    expect(within(main).getByRole('heading', { level: 1, name: 'Welcome to TeamGym Arena' })).toBeInTheDocument();
  });

  it('has no detectable accessibility violations', async () => {
    const { container } = await renderApp('/');

    expect(await axe(container)).toHaveNoViolations();
  });

  it('offers a skip link to the main content as the first focusable element', async () => {
    const user = userEvent.setup();
    await renderApp('/');

    await user.tab();

    const skipLink = screen.getByRole('link', { name: 'Skip to main content' });
    expect(skipLink).toHaveFocus();
    expect(skipLink).toHaveAttribute('href', `#${screen.getByRole('main').id}`);
  });

  it('switches the interface to Swedish', async () => {
    const user = userEvent.setup();
    await renderApp('/');

    await user.click(screen.getByRole('button', { name: 'Svenska' }));

    expect(screen.getByRole('heading', { level: 1, name: 'Välkommen till TeamGym Arena' })).toBeInTheDocument();
    expect(screen.getByRole('group', { name: 'Språk' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Svenska' })).toHaveAttribute('aria-pressed', 'true');
    expect(screen.getByRole('button', { name: 'English' })).toHaveAttribute('aria-pressed', 'false');
    expect(document.documentElement.lang).toBe('sv');
  });

  it('marks each language option with its own language', async () => {
    await renderApp('/');

    expect(screen.getByRole('button', { name: 'English' })).toHaveAttribute('lang', 'en');
    expect(screen.getByRole('button', { name: 'Svenska' })).toHaveAttribute('lang', 'sv');
  });
});
