import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AUTH_SERVICE } from '../interfaces/auth.service.interface';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AUTH_SERVICE);
  const token = authService.getToken();

  if (!token) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
  );
};
