import { apiRequest, API_BASE_URL } from '../../api/http';

// Mock ../api (token getters/setters)
jest.mock('../../api', () => {
  return {
    __esModule: true,
    getAuthToken: jest.fn(),
    setAuthToken: jest.fn(),
  };
});

const { getAuthToken, setAuthToken } = jest.requireMock('../../api');

describe('http.ts apiRequest', () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    jest.resetAllMocks();
    (global as any).fetch = jest.fn();
  });

  afterAll(() => {
    (global as any).fetch = originalFetch;
  });

  it('exposes API_BASE_URL with /api suffix', () => {
    expect(API_BASE_URL.endsWith('/api')).toBe(true);
  });

  it('adds Authorization header when token exists', async () => {
    (getAuthToken as jest.Mock).mockReturnValue('abc123');
    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ success: true, data: { ok: true }, message: '' }),
    });

    await apiRequest('/health/simple', { method: 'GET' });

    expect(global.fetch).toHaveBeenCalledTimes(1);
    const [calledUrl, config] = (global.fetch as jest.Mock).mock.calls[0];
    expect(calledUrl).toBe(`${API_BASE_URL}/health/simple`);
    expect((config.headers as any).Authorization).toBe('Bearer abc123');
  });

  it('clears token and redirects on 401, then throws', async () => {
    (getAuthToken as jest.Mock).mockReturnValue('expired');

    const setHref = jest.fn((href: string) => {
      (window as any).location.href = href;
    });
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { href: 'http://localhost', assign: setHref },
    });

    (global.fetch as jest.Mock).mockResolvedValue({
      ok: false,
      status: 401,
      statusText: 'Unauthorized',
      json: async () => ({ success: false, message: 'Unauthorized' }),
    });

    await expect(apiRequest('/anything')).rejects.toThrow('Session expired');

    expect(setAuthToken).toHaveBeenCalledWith(null);
    expect(window.location.href).toBe('/login');
  });

  it('throws with server message on non-OK, non-401 responses', async () => {
    (getAuthToken as jest.Mock).mockReturnValue(undefined);
    (global.fetch as jest.Mock).mockResolvedValue({
      ok: false,
      status: 400,
      statusText: 'Bad Request',
      json: async () => ({ success: false, message: 'Invalid input' }),
    });

    await expect(apiRequest('/bad')).rejects.toThrow('Invalid input');
  });
});
