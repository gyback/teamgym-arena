/// <reference types="vitest/config" />
import { fileURLToPath, URL } from 'node:url';

import tailwindcss from '@tailwindcss/vite';
import { tanstackRouter } from '@tanstack/router-plugin/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// The Aspire AppHost injects the API's address when it starts the dev server (ADR-0005). Outside Aspire,
// fall back to the API's own launch profile in src/Api/Properties/launchSettings.json.
const apiTarget = process.env.API_HTTPS ?? process.env.API_HTTP ?? 'https://localhost:7180';

// Requests the SPA sends to the same-origin BFF host. In production TeamGym.Arena.Api serves both.
const backendPaths = ['/api', '/bff', '/hubs'];

export default defineConfig({
  plugins: [
    // Must come before the React plugin.
    tanstackRouter({
      target: 'react',
      autoCodeSplitting: true,
      routesDirectory: './src/app/routes',
      generatedRouteTree: './src/app/routeTree.gen.ts',
    }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: Object.fromEntries(
      backendPaths.map((path) => [
        path,
        {
          target: apiTarget,
          // The local API uses the ASP.NET Core development certificate.
          secure: false,
          ws: path === '/hubs',
        },
      ]),
    ),
  },
  build: {
    sourcemap: true,
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
    restoreMocks: true,
    coverage: {
      provider: 'v8',
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.test.{ts,tsx}', 'src/test/**', 'src/app/routeTree.gen.ts', 'src/main.tsx'],
    },
  },
});
