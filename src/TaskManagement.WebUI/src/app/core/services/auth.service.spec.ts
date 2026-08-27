import { TestBed } from '@angular/core/testing';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { TOKEN_STORAGE } from '../tokens/injection.tokens';
import { ApiResponse, AuthResponse } from '../models';
import { ITokenStorage } from '../interfaces/token-storage.interface';

describe('AuthService', () => {
  let service: AuthService;
  let api: { post: ReturnType<typeof vi.fn> };
  let storage: ITokenStorage & {
    token: string | null;
    userJson: string | null;
  };

  const authResponse: AuthResponse = {
    token: 'jwt-token',
    userId: 'user-1',
    name: 'demo',
    email: 'demo@example.com',
    role: 'user'
  };

  const createStorage = (): typeof storage => ({
    token: null,
    userJson: null,
    getToken() {
      return this.token;
    },
    setToken(token: string) {
      this.token = token;
    },
    getUserJson() {
      return this.userJson;
    },
    setUserJson(userJson: string) {
      this.userJson = userJson;
    },
    clear() {
      this.token = null;
      this.userJson = null;
    }
  });

  const configure = (storageOverride: typeof storage = createStorage()) => {
    api = { post: vi.fn() };
    storage = storageOverride;

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: ApiService, useValue: api },
        { provide: TOKEN_STORAGE, useValue: storage }
      ]
    });

    service = TestBed.inject(AuthService);
  };

  beforeEach(() => {
    configure();
  });

  it('signIn persists token and marks authenticated', async () => {
    const envelope: ApiResponse<AuthResponse> = {
      success: true,
      data: authResponse,
      message: null,
      errors: []
    };
    api.post.mockResolvedValue(envelope);

    await service.signIn({ userName: 'demo', password: '@Demo123' });

    expect(service.isAuthenticated()).toBe(true);
    expect(service.getToken()).toBe('jwt-token');
    expect(service.currentUserName()).toBe('demo');
  });

  it('signIn throws when API returns failure', async () => {
    api.post.mockResolvedValue({
      success: false,
      data: null,
      message: 'Invalid credentials',
      errors: []
    });

    await expect(service.signIn({ userName: 'x', password: 'y' })).rejects.toThrow(
      'Invalid credentials'
    );
    expect(service.isAuthenticated()).toBe(false);
  });

  it('signIn throws when success response has no token', async () => {
    api.post.mockResolvedValue({
      success: true,
      data: { ...authResponse, token: null },
      message: null,
      errors: []
    });

    await expect(service.signIn({ userName: 'demo', password: '@Demo123' })).rejects.toThrow(
      'Sign-in succeeded but no token was returned.'
    );
  });

  it('signIn throws Request failed when failure has no message', async () => {
    api.post.mockResolvedValue({
      success: false,
      data: null,
      message: null,
      errors: []
    });

    await expect(service.signIn({ userName: 'x', password: 'y' })).rejects.toThrow('Request failed');
  });

  it('logout clears session', () => {
    storage.setToken('jwt-token');
    storage.setUserJson(
      JSON.stringify({
        userId: 'user-1',
        name: 'demo',
        email: 'demo@example.com',
        role: 'user'
      })
    );

    TestBed.resetTestingModule();
    configure(storage);

    expect(service.isAuthenticated()).toBe(true);

    service.logout();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.getToken()).toBeNull();
  });

  it('clears invalid persisted user JSON on startup', () => {
    const broken = createStorage();
    broken.token = 'jwt-token';
    broken.userJson = '{not-json';

    TestBed.resetTestingModule();
    configure(broken);

    expect(service.isAuthenticated()).toBe(false);
    expect(broken.token).toBeNull();
    expect(broken.userJson).toBeNull();
  });

  it('currentUserName falls back to null when persisted name is missing', () => {
    const seeded = createStorage();
    seeded.token = 'jwt-token';
    seeded.userJson = JSON.stringify({
      userId: 'user-1',
      email: 'demo@example.com',
      role: 'user'
    });

    TestBed.resetTestingModule();
    configure(seeded);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUserName()).toBeNull();
  });

  it('register returns payload without authenticating', async () => {
    api.post.mockResolvedValue({
      success: true,
      data: { ...authResponse, token: null },
      message: 'ok',
      errors: []
    });

    const data = await service.register({
      userName: 'alice',
      email: 'a@b.com',
      password: 'Secret1!'
    });

    expect(data.name).toBe('demo');
    expect(service.isAuthenticated()).toBe(false);
  });
});
