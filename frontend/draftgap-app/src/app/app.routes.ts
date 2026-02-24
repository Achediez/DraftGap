import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/pages/auth-page/auth-page.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AdminComponent } from './features/admin/admin.component';

// App routes (extend here as new features are added).
export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'auth' }, // Ahora redirige a login
	{ path: 'auth', component: AuthPageComponent },
	{ path: 'dashboard', component: DashboardComponent },
	{ path: 'admin', component: AdminComponent }
];
