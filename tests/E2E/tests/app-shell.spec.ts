import { expect, test } from '@playwright/test';

import { expectNoAccessibilityViolations } from './accessibility';

test.describe('app shell', () => {
  test('home page meets WCAG 2.2 AA', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('heading', { level: 1, name: 'Welcome to TeamGym Arena' })).toBeVisible();
    await expectNoAccessibilityViolations(page);
  });

  test('switching to Swedish is remembered after a reload', async ({ page }) => {
    await page.goto('/');

    await page.getByRole('button', { name: 'Svenska' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Välkommen till TeamGym Arena' })).toBeVisible();
    await expect(page.locator('html')).toHaveAttribute('lang', 'sv');
    await expectNoAccessibilityViolations(page);

    await page.reload();
    await expect(page.getByRole('heading', { level: 1, name: 'Välkommen till TeamGym Arena' })).toBeVisible();
  });

  test('skip link is visible on focus and moves to the main content', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    await page.keyboard.press('Tab');
    const skipLink = page.getByRole('link', { name: 'Skip to main content' });
    await expect(skipLink).toBeFocused();
    await expect(skipLink).toBeVisible();

    await page.keyboard.press('Enter');
    await expect(page.getByRole('main')).toBeFocused();
  });

  test('a deep link to an unknown page shows the not-found page', async ({ page }) => {
    await page.goto('/competitions/does-not-exist');

    await expect(page.getByRole('heading', { level: 1, name: 'Page not found' })).toBeVisible();
    await expectNoAccessibilityViolations(page);

    await page.getByRole('link', { name: 'Go to the home page' }).click();
    await expect(page).toHaveURL('/');
  });
});
