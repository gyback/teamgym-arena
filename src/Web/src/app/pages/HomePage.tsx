import { useTranslation } from 'react-i18next';

export function HomePage() {
  const { t } = useTranslation();

  return (
    <section className="max-w-prose space-y-4">
      <h1 className="text-3xl font-semibold tracking-tight">{t('home.title')}</h1>
      <p className="text-lg text-muted-foreground">{t('app.tagline')}</p>
      <p>{t('home.intro')}</p>
    </section>
  );
}
