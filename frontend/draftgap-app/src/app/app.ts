
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AdminComponent } from './features/admin/admin.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DashboardComponent, AdminComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  // Root component intentionally minimal.
  onActivate(event: any) {
    // Si el componente activado es AdminComponent, forzar recarga
    if (event && event.constructor && event.constructor.name === 'AdminComponent') {
      if (typeof event.loadUsers === 'function') {
        event.loadUsers();
      }
    }
  }
}
