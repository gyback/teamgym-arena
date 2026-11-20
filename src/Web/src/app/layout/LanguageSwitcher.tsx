import { useTranslation } from 'react-i18next';

import { Button } from '@/shared/components/ui/button';
import { supportedLanguages } from '@/shared/i18n/resources';

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();

  return (
    <div role="group" aria-label={t('language.label')} className="flex gap-1">
      {supportedLanguages.map((language) => {
        const isActive = i18n.resolvedLanguage === language;
        return (
          <Button
            key={language}
            type="button"
            size="lg"
            variant={isActive ? 'secondary' : 'ghost'}
            lang={language}
            aria-pressed={isActive}
            onClick={() => void i18n.changeLanguage(language)}
          >
            {t(`language.${language}`)}
          </Button>
        );
      })}
    </div>
  );
}
