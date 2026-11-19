import enCommon from './locales/en/common.json';
import enTerminology from './locales/en/terminology.json';
import svCommon from './locales/sv/common.json';
import svTerminology from './locales/sv/terminology.json';

export const supportedLanguages = ['en', 'sv'] as const;

export type SupportedLanguage = (typeof supportedLanguages)[number];

export const fallbackLanguage: SupportedLanguage = 'en';

export const defaultNamespace = 'common';

/**
 * Every namespace for every language. `en` is the reference: its key set is the contract that `sv` must
 * match (see resources.test.ts), and it types the `t` function (see i18next.d.ts).
 */
export const resources = {
  en: { common: enCommon, terminology: enTerminology },
  sv: { common: svCommon, terminology: svTerminology },
} as const satisfies Record<SupportedLanguage, Record<string, unknown>>;

export function isSupportedLanguage(language: string): language is SupportedLanguage {
  return (supportedLanguages as readonly string[]).includes(language);
}
