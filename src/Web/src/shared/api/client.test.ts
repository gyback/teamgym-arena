import { createApiClient, csrfHeader, isValidationProblem, toProblemDetails } from './client';

// The generated schema has no endpoints yet, so these tests describe their own.
interface TestPaths {
  '/api/v1/teams': {
    get: { responses: { 200: { content: { 'application/json': { name: string }[] } } } };
    post: {
      requestBody: { content: { 'application/json': { name: string } } };
      responses: {
        201: { content: { 'application/json': { id: string } } };
        400: { content: { 'application/problem+json': unknown } };
      };
    };
  };
}

const baseUrl = 'https://arena.test';

function stubFetch(response: Response) {
  const requests: Request[] = [];
  const fetch = vi.fn((input: Request) => {
    requests.push(input);
    return Promise.resolve(response);
  });
  return { client: createApiClient<TestPaths>({ baseUrl, fetch }), requests };
}

function json(body: unknown, status: number, contentType = 'application/json') {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': contentType } });
}

describe('apiClient', () => {
  it('sends the session cookie to the same origin only', async () => {
    const { client, requests } = stubFetch(json([], 200));

    await client.GET('/api/v1/teams');

    expect(requests[0]?.credentials).toBe('same-origin');
  });

  it('adds the CSRF header to state-changing requests', async () => {
    const { client, requests } = stubFetch(json({ id: '1' }, 201));

    await client.POST('/api/v1/teams', { body: { name: 'Team A' } });

    expect(requests[0]?.headers.get(csrfHeader.name)).toBe(csrfHeader.value);
  });

  it('does not add the CSRF header to safe requests', async () => {
    const { client, requests } = stubFetch(json([], 200));

    await client.GET('/api/v1/teams');

    expect(requests[0]?.headers.has(csrfHeader.name)).toBe(false);
  });

  it('returns the problem details of a failed request as its error', async () => {
    const problem = { title: 'One or more validation errors occurred.', status: 400, errors: { name: ['Required.'] } };
    const { client } = stubFetch(json(problem, 400, 'application/problem+json'));

    const { data, error, response } = await client.POST('/api/v1/teams', { body: { name: '' } });

    expect(data).toBeUndefined();
    const details = toProblemDetails(error, response);
    expect(details).toEqual(problem);
    expect(isValidationProblem(details) && details.errors).toEqual({ name: ['Required.'] });
  });
});

describe('toProblemDetails', () => {
  it('takes the status from the response when the body has none', () => {
    const details = toProblemDetails({ title: 'Not Found' }, new Response(null, { status: 404 }));

    expect(details).toEqual({ title: 'Not Found', status: 404 });
  });

  it('describes a response without a problem details body by its status', () => {
    const response = new Response('<html>Bad Gateway</html>', { status: 502, statusText: 'Bad Gateway' });

    expect(toProblemDetails('<html>Bad Gateway</html>', response)).toEqual({ status: 502, title: 'Bad Gateway' });
  });
});

describe('isValidationProblem', () => {
  it('is false for problem details without field errors', () => {
    expect(isValidationProblem({ status: 403, title: 'Forbidden' })).toBe(false);
  });

  it('is false when the errors are not lists of messages per field', () => {
    expect(isValidationProblem({ status: 400, errors: { name: 'Required.' } } as never)).toBe(false);
  });
});
