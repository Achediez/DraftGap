import { Component } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Panel de administración para gestión de usuarios.
 * Solo accesible para usuarios con rol de administrador.
 */
@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
/**
 * Componente de administración: permite gestionar usuarios (solo admin).
 * Incluye botón para volver al dashboard principal.
 */
export class AdminComponent {
  constructor(private router: Router) {
    // Si no es admin, redirige al dashboard
    const isAdmin = localStorage.getItem('isAdmin');
    if (isAdmin !== '1' && isAdmin !== 'true') {
      this.router.navigate(['/dashboard']);
    }
  }

  /**
   * Redirige al dashboard principal.
   */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
  // Aquí se gestionarán los usuarios: listar, añadir, borrar, etc.
  // TODO: Implementar lógica real de gestión de usuarios.
}
