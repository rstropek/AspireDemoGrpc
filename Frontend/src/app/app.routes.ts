import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: 'users', 
    loadComponent: () => import('./components/user-management.component').then(m => m.UserManagementComponent) 
  },
  { 
    path: 'messaging', 
    loadComponent: () => import('./components/mqtt-messaging.component').then(m => m.MqttMessagingComponent) 
  },
  { 
    path: '', 
    loadComponent: () => import('./components/dashboard.component').then(m => m.DashboardComponent),
    pathMatch: 'full' 
  }
];
