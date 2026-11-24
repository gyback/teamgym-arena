import createClient, { type ClientOptions, type Middleware } from 'openapi-fetch';

import type { components, paths } from './schema';

/** An RFC 9457 problem details response, which the API returns for every error (ADR-0005). */
export type ProblemDetails = components['schemas']['ProblemDetails'];

/** Problem details with field-level validation errors, keyed by field name. */
export type ValidationProblemDetails = components['schemas']['HttpValidationProblemDetails'];

/**
 * The header that the BFF requires on every state-changing request (ADR-0008). A cross-site page cannot send it,
 * because the API configures no CORS policy.
 */
export const csrfHeader = { name: 'X-CSRF', value: '1' } as const;

const safeMethods = new Set(['GET', 'HEAD', 'OPTIONS', 'TRACE']);

const csrfMiddleware: Middleware = {
  onRequest({ request }) {
    if (!safeMethods.has(request.method.toUpperCase())) {
      request.headers.set(csrfHeader.name, csrfHeader.value);
    }
    return request;
  },
};

/**
 * Creates a typed client for the same-origin API. The browser sends the BFF session cookie with every request and
 * never sees a token (ADR-0008). Use {@link apiClient}; this factory exists so tests can pass their own `fetch`.
 */
export function createApiClient<Paths extends object = paths>(options: Pick<ClientOptions, 'baseUrl' | 'fetch'> = {}) {
  const client = createClient<Paths>({ ...options, credentials: 'same-origin' });
  client.use(csrfMiddleware);
  return client;
}

/** The client for the TeamGym Arena API. Its paths and types come from the generated `schema.d.ts`. */
export const apiClient = createApiClient();

/**
 * Reads the problem details from a failed call's `error` and `response`. A response without a problem details body
 * (for example a proxy error page) becomes problem details with the response's status, so callers handle one shape.
 */
export function toProblemDetails(error: unknown, response: Response): ProblemDetails {
  if (isRecord(error)) {
    return { ...error, status: typeof error.status === 'number' ? error.status : response.status };
  }
  return { status: response.status, title: response.statusText || null };
}

/** Whether the problem details carry field-level validation errors, as a `400 Bad Request` from a form does. */
export function isValidationProblem(problem: ProblemDetails): problem is ValidationProblemDetails {
  const { errors } = problem as { errors?: unknown };
  return isRecord(errors) && Object.values(errors).every((messages) => Array.isArray(messages));
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
