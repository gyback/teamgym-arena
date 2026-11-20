import { Link, Outlet } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';

import { LanguageSwitcher } from './LanguageSwitcher';

const mainContentId = 'main-content';

export function RootLayout() {
  const { t } = useTranslation();

  return (
    <div className="flex min-h-dvh flex-col">
      <a
        href={`#${mainContentId}`}
        className="sr-only rounded-md bg-background px-3 py-2 font-medium focus:not-sr-only focus:fixed focus:top-2 focus:left-2 focus:z-50"
      >
        {t('layout.skipToContent')}
      </a>
      <header className="border-b">
        <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center justify-between gap-2 px-4 py-3 sm:px-6">
          <Link to="/" aria-label={t('layout.homeLink')} className="rounded-md text-lg font-semibold tracking-tight">
            {t('app.name')}
          </Link>
          <LanguageSwitcher />
        </div>
      </header>
      <main id={mainContentId} tabIndex={-1} className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:px-6">
        <Outlet />
      </main>
    </div>
  );
}
