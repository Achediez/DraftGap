import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/pages/auth-page/auth-page.component';

// App routes (extend here as new features are added).
export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'auth' },
	{ path: 'auth', component: AuthPageComponent }
];
