import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
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
/**
 * Componente de administración: permite gestionar usuarios (solo admin).
 * Incluye botón para volver al dashboard principal.
 */
  users: any[] = [];
  selectedUser: any = null;
  loading = false;
  error: string | null = null;
  newUser: any = { email: '', riotId: '', region: '', password: '', isAdmin: false };
  addUserError: string | null = null;
  addUserSuccess = false;
  /**
   * Añadir usuario nuevo
   */
  addUser() {
    this.addUserError = null;
    @Component({
      selector: 'app-admin',
      templateUrl: './admin.component.html',
      styleUrls: ['./admin.component.scss']
    })
    export class AdminComponent {
      users: any[] = [];
      selectedUser: any = null;
      loading = false;
      error: string | null = null;
      newUser: any = { email: '', riotId: '', region: '', password: '', isAdmin: false };
      addUserError: string | null = null;
      addUserSuccess = false;

      constructor(private router: Router, private adminApi: AdminApiService) {
        // Si no es admin, redirige al dashboard
        const isAdmin = localStorage.getItem('isAdmin');
        if (isAdmin !== '1' && isAdmin !== 'true') {
          this.router.navigate(['/dashboard']);
        }
        this.loadUsers();
      }

      /**
       * Añadir usuario nuevo
       */
      addUser() {
        this.addUserError = null;
        this.addUserSuccess = false;
        this.adminApi.addUser(this.newUser).subscribe({
          next: () => {
            this.addUserSuccess = true;
            this.newUser = { email: '', riotId: '', region: '', password: '', isAdmin: false };
            this.loadUsers();
          },
          error: err => {
            this.addUserError = 'Error al añadir usuario';
          }
        });
      }

      /**
       * Cargar usuarios desde el backend
       */
      loadUsers() {
        this.loading = true;
        this.adminApi.getUsers().subscribe({
          next: users => {
            this.users = users;
            this.loading = false;
          },
          error: err => {
            this.error = 'Error al cargar usuarios';
            this.loading = false;
          }
        });
      }

      /**
       * Ver perfil de usuario (abre modal o muestra detalles)
       */
      viewUser(user: any) {
        this.selectedUser = null;
        this.adminApi.getUserById(user.user_id || user.id).subscribe({
          next: data => {
            this.selectedUser = data;
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
          this.adminApi.deleteUser(user.user_id || user.id).subscribe({
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
