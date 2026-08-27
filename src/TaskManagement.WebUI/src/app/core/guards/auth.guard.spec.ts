import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard, guestGuard } from './auth.guard';
import { AUTH_SERVICE } from '../interfaces/auth.service.interface';

describe('auth guards', () => {
  const isAuthenticated = signal(false);
  const router = {
    createUrlTree: vi.fn((commands: string[]) => ({ commands }) as unknown as UrlTree)
  };

  beforeEach(() => {
    isAuthenticated.set(false);
    router.createUrlTree.mockClear();

    TestBed.configureTestingModule({
      providers: [
        { provide: AUTH_SERVICE, useValue: { isAuthenticated } },
        { provide: Router, useValue: router }
      ]
    });
  });

  it('authGuard allows authenticated users', () => {
    isAuthenticated.set(true);
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBe(true);
  });

  it('authGuard redirects guests to login', () => {
    isAuthenticated.set(false);
    TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  it('guestGuard allows guests', () => {
    isAuthenticated.set(false);
    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toBe(true);
  });

  it('guestGuard redirects authenticated users to tasks', () => {
    isAuthenticated.set(true);
    TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(router.createUrlTree).toHaveBeenCalledWith(['/tasks']);
  });
});
