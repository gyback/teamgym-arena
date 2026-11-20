import { useRouter, type ErrorComponentProps } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';

import { Button } from '@/shared/components/ui/button';

// The error itself is not shown: it may contain details that are not meant for the user.
export function ErrorPage({ reset }: ErrorComponentProps) {
  const { t } = useTranslation();
  const router = useRouter();

  return (
    <section role="alert" className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">{t('error.title')}</h1>
      <p className="text-muted-foreground">{t('error.description')}</p>
      <Button
        type="button"
        size="lg"
        onClick={() => {
          reset();
          void router.invalidate();
        }}
      >
        {t('error.retry')}
      </Button>
    </section>
  );
}
