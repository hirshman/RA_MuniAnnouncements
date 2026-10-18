import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

// בסביבת prod – האזור האישי מספק Observable<string> עם הטוקן העדכני
export const AUTH_TOKEN = new InjectionToken<Observable<string>>('AUTH_TOKEN');
