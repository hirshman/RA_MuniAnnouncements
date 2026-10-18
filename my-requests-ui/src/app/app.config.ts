import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './auth.interceptor';
import { from } from 'rxjs';
import { map } from 'rxjs/operators';
import { AUTH_TOKEN } from './auth-token.token';
import { environment } from '../environments/environment';

// סביבת dev: מביא טוקן מה-API. בסביבת prod – האזור האישי מחליף ספק זה.
const devTokenProvider = {
  provide: AUTH_TOKEN,
  useValue: from(
    fetch(`${environment.apiUrl}/api/dev-token?idNumber=012345678`)
      .then(r => r.json())
  ).pipe(map((res: { token: string }) => res.token))
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor])),
    devTokenProvider
  ]
};
