import { HttpRequest, HttpHandlerFn, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AUTH_SERVICE } from '../interfaces/auth.service.interface';
import { authInterceptor } from '../interceptors/auth.interceptor';

describe('authInterceptor', () => {
  it('passes through when there is no token', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: AUTH_SERVICE, useValue: { getToken: () => null } }]
    });

    const next = vi.fn<HttpHandlerFn>((req) => of(new HttpResponse({ status: 200, body: req })));

    TestBed.runInInjectionContext(() => {
      authInterceptor(new HttpRequest('GET', '/api/v1/tasks'), next).subscribe();
    });

    expect(next).toHaveBeenCalledOnce();
    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.has('Authorization')).toBe(false);
  });

  it('adds bearer token when available', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: AUTH_SERVICE, useValue: { getToken: () => 'jwt-token' } }]
    });

    const next = vi.fn<HttpHandlerFn>((req) => of(new HttpResponse({ status: 200, body: req })));

    TestBed.runInInjectionContext(() => {
      authInterceptor(new HttpRequest('GET', '/api/v1/tasks'), next).subscribe();
    });

    const forwarded = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwarded.headers.get('Authorization')).toBe('Bearer jwt-token');
  });
});
