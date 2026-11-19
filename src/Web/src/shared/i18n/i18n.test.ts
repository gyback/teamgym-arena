import { z } from 'zod';

import { i18n } from '.';

describe('i18n', () => {
  it('falls back to English for an unsupported language', async () => {
    await i18n.changeLanguage('de-DE');

    expect(i18n.resolvedLanguage).toBe('en');
    expect(i18n.t('notFound.title')).toBe('Page not found');
    expect(document.documentElement.lang).toBe('en');
  });

  it('resolves a regional Swedish locale to sv', async () => {
    await i18n.changeLanguage('sv-SE');

    expect(i18n.resolvedLanguage).toBe('sv');
    expect(i18n.t('notFound.title')).toBe('Sidan hittades inte');
  });

  it('sets the document language when the language changes', async () => {
    await i18n.changeLanguage('sv');

    expect(document.documentElement.lang).toBe('sv');
  });

  it('translates TeamGym terms from the terminology namespace', async () => {
    await i18n.changeLanguage('sv');

    expect(i18n.t('discipline.Floor', { ns: 'terminology' })).toBe('Fristående');
    expect(i18n.t('section.Open', { ns: 'terminology' })).toBe('Öppen');
  });

  it('shows Zod validation messages in the active language', async () => {
    const schema = z.string().min(3);

    await i18n.changeLanguage('sv');
    const swedish = schema.safeParse('a').error?.issues[0]?.message;
    await i18n.changeLanguage('en');
    const english = schema.safeParse('a').error?.issues[0]?.message;

    expect(swedish).toMatch(/förväntade/);
    expect(english).toMatch(/expected/i);
  });

  it('remembers the chosen language for the next visit', async () => {
    await i18n.changeLanguage('sv');

    expect(localStorage.getItem('i18nextLng')).toBe('sv');
  });
});
