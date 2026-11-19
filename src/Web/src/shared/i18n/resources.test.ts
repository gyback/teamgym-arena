import { resources, supportedLanguages } from './resources';

function keyPaths(value: unknown, prefix = ''): string[] {
  if (typeof value !== 'object' || value === null) {
    return [prefix];
  }
  return Object.entries(value).flatMap(([key, child]) => keyPaths(child, prefix ? `${prefix}.${key}` : key));
}

function leafValues(value: unknown): unknown[] {
  if (typeof value !== 'object' || value === null) {
    return [value];
  }
  return Object.values(value).flatMap(leafValues);
}

const namespaces = Object.keys(resources.en) as (keyof typeof resources.en)[];
const translatedLanguages = supportedLanguages.filter((language) => language !== 'en');

describe('translation resources', () => {
  describe.each(translatedLanguages)('%s', (language) => {
    it.each(namespaces)('has exactly the English keys in the %s namespace', (namespace) => {
      expect(keyPaths(resources[language][namespace]).sort()).toEqual(keyPaths(resources.en[namespace]).sort());
    });
  });

  describe.each(supportedLanguages)('%s', (language) => {
    it.each(namespaces)('has only non-empty strings in the %s namespace', (namespace) => {
      for (const value of leafValues(resources[language][namespace])) {
        expect(typeof value === 'string' && value.trim() !== '').toBe(true);
      }
    });

    // Mirrors TeamGym.Arena.Common.Discipline and Section, so every value the API returns has a label.
    it('names every discipline and section', () => {
      const { discipline, section } = resources[language].terminology;

      expect(Object.keys(discipline)).toEqual(['Floor', 'Tumble', 'Trampet']);
      expect(Object.keys(section)).toEqual(['Women', 'Men', 'Mixed', 'Open']);
    });
  });

  it('uses the European Gymnastics TeamGym terms in en', () => {
    expect(resources.en.terminology).toEqual({
      discipline: { Floor: 'Floor', Tumble: 'Tumble', Trampet: 'Trampet' },
      section: { Women: 'Women', Men: 'Men', Mixed: 'Mixed', Open: 'Open' },
    });
  });

  it('uses the Swedish TeamGym terms in sv', () => {
    expect(resources.sv.terminology).toEqual({
      discipline: { Floor: 'Fristående', Tumble: 'Tumbling', Trampet: 'Trampett' },
      section: { Women: 'Dam', Men: 'Herr', Mixed: 'Mixed', Open: 'Öppen' },
    });
  });
});
