import type { AxeMatchers } from 'vitest-axe/matchers';

// vitest-axe declares its matchers on the pre-1.0 `Vi` namespace. Current Vitest reads them from the
// `vitest` module instead.
declare module 'vitest' {
  // eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/no-explicit-any, @typescript-eslint/no-unused-vars -- must match Vitest's own declaration to merge with it.
  interface Assertion<T = any> extends AxeMatchers {}
  // eslint-disable-next-line @typescript-eslint/no-empty-object-type -- adds the matchers to `expect.not`.
  interface AsymmetricMatchersContaining extends AxeMatchers {}
}
