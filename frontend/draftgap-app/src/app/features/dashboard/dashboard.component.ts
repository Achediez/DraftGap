import { Component, ElementRef, HostListener, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthApiService } from '../auth/data/auth-api.service';
import { HttpErrorResponse } from '@angular/common/http';

/**
 * Página principal tras el login/registro.
 * Inspirada en DPM, u.gg, Lolalytics, etc.
 * Muestra estadísticas y acceso a admin si el usuario es administrador.
 */
/**
 * Componente principal del dashboard de usuario.
 * Muestra información del perfil, estadísticas, partidas recientes y menú de usuario.
 * Inspirado en la estética de DPM/u.gg/Lolalytics.
 *
 * - Si el usuario es admin, muestra acceso a gestión.
 * - El menú de usuario permite cerrar sesión y acceder a gestión (si es admin).
 * - Los datos son simulados para entorno demo.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  /**
   * Indica si el usuario es administrador.
   * Se actualizará con el valor real del backend.
   */
  isAdmin = false;

  /**
   * Objeto con los datos del usuario logueado.
   * Inicialmente vacío, se rellenará tras la petición al backend.
   */
  user: any = {};

  /**
   * Estadísticas principales del usuario para mostrar en tarjetas.
   * Se actualizarán tras obtener los datos reales.
   */
  stats: any[] = [];

  /**
   * Partidas recientes simuladas para la tabla (puedes adaptar esto a datos reales si el backend lo permite).
   */
  recentMatches = [
    { champion: 'Jinx', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Jinx.png', result: 'Victoria', kda: '12/3/8', date: '2026-02-17' },
    { champion: 'Thresh', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Thresh.png', result: 'Derrota', kda: '1/7/14', date: '2026-02-16' },
    { champion: 'Ahri', championImg: 'https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Ahri.png', result: 'Victoria', kda: '8/2/5', date: '2026-02-15' }
  ];

  /**
   * Estado de apertura/cierre del menú de usuario.
   */
  menuOpen = false;

  /**
   * Controla la pestaña activa del dashboard (datos, partidas, estadísticas, etc.).
   * Inspirado en la navegación de op.gg, u.gg, dpm.
   */
  activeTab: 'datos' | 'partidas' | 'stats' = 'datos';

  /**
   * Constructor: comprueba autenticación y obtiene los datos reales del usuario.
   * @param elRef Referencia al elemento raíz del componente (para detectar clics fuera)
   * @param authApi Servicio para acceder a la API de autenticación
   */
  constructor(private elRef: ElementRef, private authApi: AuthApiService, private cdr: ChangeDetectorRef) {
    // Si no hay token, redirige a login
    if (!localStorage.getItem('draftgap_token')) {
      window.location.href = '/auth';
      return;
    }

    // Llama al backend para obtener los datos reales del usuario logueado
    this.authApi.getCurrentUser().subscribe({
      next: (data) => {
        /**
         * data contiene los datos reales del usuario devueltos por el backend:
         * - email
         * - riotId
         * - isAdmin
         * - lastSync
         * - createdAt
         */
        this.user = data;
        this.isAdmin = data.isAdmin;
        // Actualiza las estadísticas principales con datos reales si están disponibles
        this.stats = [
          // Puedes adaptar estos campos según lo que devuelva el backend
          { label: 'Riot ID', value: data.riotId || '-', color: '#00bba3' },
          { label: 'Email', value: data.email, color: '#3fa7ff' },
          { label: 'Creado', value: data.createdAt ? new Date(data.createdAt).toLocaleDateString() : '-', color: '#ffe156' },
          { label: 'Última sync', value: data.lastSync ? new Date(data.lastSync).toLocaleDateString() : '-', color: '#ff6f61' }
        ];
        this.cdr.detectChanges();
      },
      error: (err: HttpErrorResponse) => {
        // Si hay error (token inválido, expirado, etc.), redirige a login
        window.location.href = '/auth';
      }
    });
  }

  /**
   * Cierra el menú si se hace clic fuera del menú de usuario.
   */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.menuOpen = false;
    }
  }

  /**
   * Abre/cierra el menú de usuario al hacer clic en el icono.
   */
  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  /**
   * Cierra sesión: borra datos de localStorage y redirige a login.
   */
  logout(event: Event) {
    event.stopPropagation();
    this.menuOpen = false;
    setTimeout(() => {
      localStorage.removeItem('draftgap_token');
      localStorage.removeItem('isAdmin');
      window.location.href = '/auth';
    }, 100);
  }

  /**
   * Acceso a la gestión de usuarios (solo admin).
   */
  goToAdmin(event: Event) {
    event.stopPropagation();
    this.menuOpen = false;
    setTimeout(() => {
      window.location.href = '/admin';
    }, 100);
  }
  // TODO: Cargar datos reales del usuario y estadísticas aquí.
}
