import { Component } from '@angular/core';
import { MyRequestsComponent } from './components/my-requests/my-requests.component';

@Component({
  selector: 'app-root',
  imports: [MyRequestsComponent],
  template: '<app-my-requests />'
})
export class App {}
