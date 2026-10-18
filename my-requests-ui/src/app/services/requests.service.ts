import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { NormalizedRequest } from '../models/normalized-request';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MyRequestsService {
  private readonly http = inject(HttpClient);

  getMyRequests(): Observable<NormalizedRequest[]> {
    return this.http.get<{ requests: NormalizedRequest[] }>(`${environment.apiUrl}/api/my-requests`)
      .pipe(map(res => res.requests));
  }
}
