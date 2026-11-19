import { defineConfig, devices } from '@playwright/test';

// Set E2E_BASE_URL to run against an app that is already running, for example the Vite URL that
// `dotnet run --project src/AppHost` prints in the Aspire dashboard. Without it, Playwright builds the SPA
// and serves it with `vite preview`, which needs no backend.
const externalBaseUrl = process.env.E2E_BASE_URL;
const previewPort = 4173;
const isCi = !!process.env.CI;

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: isCi,
  retries: isCi ? 2 : 0,
  ...(isCi ? { workers: 1 } : {}),
  reporter: isCi ? [['github'], ['html', { open: 'never' }]] : [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: externalBaseUrl ?? `http://localhost:${previewPort}`,
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] } },
    // Coaches and officials use phones on the competition floor.
    { name: 'mobile', use: { ...devices['Pixel 7'] } },
  ],
  ...(externalBaseUrl
    ? {}
    : {
        webServer: {
          command: `npm run build && npm run preview -- --port ${previewPort} --strictPort`,
          cwd: '../../src/Web',
          url: `http://localhost:${previewPort}`,
          reuseExistingServer: !isCi,
          timeout: 120_000,
        },
      }),
});
