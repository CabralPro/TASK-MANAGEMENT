import { TestBed } from '@angular/core/testing';
import { TokenStorageService } from './token-storage.service';

describe('TokenStorageService', () => {
  let service: TokenStorageService;
  const memory = new Map<string, string>();

  beforeEach(() => {
    memory.clear();
    Object.defineProperty(globalThis, 'localStorage', {
      configurable: true,
      value: {
        getItem: (key: string) => memory.get(key) ?? null,
        setItem: (key: string, value: string) => {
          memory.set(key, value);
        },
        removeItem: (key: string) => {
          memory.delete(key);
        },
        clear: () => memory.clear()
      }
    });

    TestBed.configureTestingModule({
      providers: [TokenStorageService]
    });
    service = TestBed.inject(TokenStorageService);
  });

  it('stores and reads token and user json', () => {
    service.setToken('abc');
    service.setUserJson('{"name":"demo"}');

    expect(service.getToken()).toBe('abc');
    expect(service.getUserJson()).toBe('{"name":"demo"}');
  });

  it('clear removes persisted values', () => {
    service.setToken('abc');
    service.setUserJson('{"name":"demo"}');

    service.clear();

    expect(service.getToken()).toBeNull();
    expect(service.getUserJson()).toBeNull();
  });
});
