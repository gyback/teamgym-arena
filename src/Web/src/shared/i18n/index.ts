import i18n from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';
import { z } from 'zod';

import {
  defaultNamespace,
  fallbackLanguage,
  isSupportedLanguage,
  resources,
  supportedLanguages,
  type SupportedLanguage,
} from './resources';

const zodLocales: Record<SupportedLanguage, () => z.core.$ZodConfig> = {
  en: z.locales.en,
  sv: z.locales.sv,
};

/**
 * Keeps the rest of the page in step with the active language: the document language for assistive
 * technology (WCAG 3.1.1) and the language of Zod's built-in validation messages.
 */
function applyLanguage(language: string) {
  const active = isSupportedLanguage(language) ? language : fallbackLanguage;
  document.documentElement.lang = active;
  z.config(zodLocales[active]());
}

i18n.on('languageChanged', applyLanguage);

await i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    ns: Object.keys(resources.en),
    defaultNS: defaultNamespace,
    fallbackLng: fallbackLanguage,
    supportedLngs: supportedLanguages,
    load: 'languageOnly',
    // The resources are bundled, so initialisation completes before the first render.
    initAsync: false,
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
    interpolation: {
      // React already escapes rendered values.
      escapeValue: false,
    },
  });

export { i18n };
