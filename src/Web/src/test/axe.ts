import { configureAxe } from 'vitest-axe';

/**
 * axe-core for component tests. jsdom has no layout or canvas, so colour contrast cannot be measured here:
 * the Playwright suite in tests/E2E checks it in a real browser.
 */
export const axe = configureAxe({
  rules: {
    'color-contrast': { enabled: false },
  },
});
