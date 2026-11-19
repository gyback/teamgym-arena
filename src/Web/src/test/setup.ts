import '@testing-library/jest-dom/vitest';

import { cleanup } from '@testing-library/react';
import * as axeMatchers from 'vitest-axe/matchers';

import { i18n } from '@/shared/i18n';

expect.extend(axeMatchers);

// jsdom does not implement scrolling; the router's scroll restoration calls it on every navigation.
window.scrollTo = () => undefined;

afterEach(async () => {
  cleanup();
  localStorage.clear();
  await i18n.changeLanguage('en');
});
