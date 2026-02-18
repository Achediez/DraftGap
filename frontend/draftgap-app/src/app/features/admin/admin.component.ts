import { Component } from '@angular/core';

/**
 * Panel de administración para gestión de usuarios.
 * Solo accesible para usuarios con rol de administrador.
 */
@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent {
  constructor() {
    // Si no es admin, redirige al dashboard
    if (localStorage.getItem('isAdmin') !== '1') {
      window.location.href = '/dashboard';
    }
  }
  // Aquí se gestionarán los usuarios: listar, añadir, borrar, etc.
  // TODO: Implementar lógica real de gestión de usuarios.
}
