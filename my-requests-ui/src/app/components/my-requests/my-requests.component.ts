import { Component, OnInit, inject, signal } from '@angular/core';
import { MyRequestsService } from '../../services/requests.service';
import { NormalizedRequest } from '../../models/normalized-request';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  templateUrl: './my-requests.component.html',
  styleUrl: './my-requests.component.css'
})
export class MyRequestsComponent implements OnInit {
  private readonly service = inject(MyRequestsService);

  requests = signal<NormalizedRequest[]>([]);
  loading = signal(true);
  error = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(false);

    this.service.getMyRequests().subscribe({
      next: (res) => { this.requests.set(res); this.loading.set(false); },
      error: () => { this.error.set(true); this.loading.set(false); }
    });
  }
}
