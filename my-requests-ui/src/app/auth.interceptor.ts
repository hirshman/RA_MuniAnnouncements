import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { switchMap } from 'rxjs/operators';
import { AUTH_TOKEN } from './auth-token.token';

export const authInterceptor: HttpInterceptorFn = (req, next) =>
  inject(AUTH_TOKEN).pipe(
    switchMap(token => next(req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    })))
  );
