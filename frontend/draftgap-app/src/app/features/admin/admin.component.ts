import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { AdminApiService } from './admin-api.service';

/**
 * Panel de administración para gestión de usuarios.
 * Solo accesible para usuarios con rol de administrador.
 */
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  showAddUser = false;
  users: any[] = [];
  selectedUser: any = null;
  loading = false;
  error: string | null = null;
  newUser: any = { email: '', riotId: '', region: '', password: '', isAdmin: false };
  addUserError: string | null = null;
  addUserSuccess = false;

  constructor(private router: Router, private adminApi: AdminApiService, private cdr: ChangeDetectorRef) {
    // Si no es admin, redirige al dashboard
    const isAdmin = localStorage.getItem('isAdmin');
    if (isAdmin !== '1' && isAdmin !== 'true') {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnInit() {
    this.loadUsers();
  }

  /**
   * Añadir usuario nuevo
   */
  addUser() {
    this.addUserError = null;
    this.addUserSuccess = false;
    // Validación mínima
    if (!this.newUser.email || !this.newUser.riotId || !this.newUser.region || !this.newUser.password) {
      this.addUserError = 'Todos los campos son obligatorios';
      return;
    }
    if (this.newUser.password.length < 6) {
      this.addUserError = 'La contraseña debe tener al menos 6 caracteres';
      return;
    }
    this.adminApi.addUser(this.newUser).subscribe({
      next: () => {
        this.addUserSuccess = true;
        this.newUser = { email: '', riotId: '', region: '', password: '', isAdmin: false };
        this.loadUsers(); // Refresca la lista tras crear
        setTimeout(() => {
          this.showAddUser = false;
          this.addUserSuccess = false;
        }, 1200);
      },
      error: err => {
        if (err?.error?.error) {
          this.addUserError = err.error.error;
        } else {
          this.addUserError = 'Error al añadir usuario';
        }
      }
    });
  }

  /**
   * Cargar usuarios desde el backend
   */
  loadUsers() {
    console.log('[Admin] loadUsers() called');
    this.loading = true;
    this.users = [];
    this.adminApi.getUsers().subscribe({
      next: users => {
        console.log('[Admin] getUsers() response:', users);
        this.users = users;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Error al cargar usuarios';
        this.loading = false;
        console.error('[Admin] getUsers() error:', err);
      }
    });
  }

  /**
   * Ver perfil de usuario (abre modal o muestra detalles)
   */
  viewUser(user: any) {
    this.selectedUser = null;
    this.cdr.detectChanges();
    this.adminApi.getUserById(user.userId).subscribe({
      next: data => {
        this.selectedUser = data;
        this.cdr.detectChanges();
      },
      error: () => alert('Error al cargar el perfil')
    });
  }

  /**
   * Eliminar usuario (excepto prueba2@draftgap.local)
   */
  deleteUser(user: any) {
    if (user.email === 'prueba2@draftgap.local') return;
    if (confirm('¿Seguro que quieres eliminar a ' + user.email + '?')) {
      this.adminApi.deleteUser(user.userId).subscribe({
        next: () => this.loadUsers(),
        error: () => alert('Error al eliminar usuario')
      });
    }
  }

  /**
   * Redirige al dashboard principal.
   */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
