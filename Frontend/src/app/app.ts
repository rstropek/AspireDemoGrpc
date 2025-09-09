import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <div class="app-container">
      <h1>Angular Frontend</h1>

      <!-- Navigation -->
      <nav class="navigation">
        <a
          routerLink="/"
          routerLinkActive="nav-link-active"
          [routerLinkActiveOptions]="{ exact: true }"
          class="nav-link"
        >
          Dashboard
        </a>
        <a routerLink="/users" routerLinkActive="nav-link-active" class="nav-link">
          User Management
        </a>
        <a routerLink="/messaging" routerLinkActive="nav-link-active" class="nav-link">
          MQTT Messaging
        </a>
      </nav>

      <router-outlet />
    </div>
  `,
  styles: [
    `
      .app-container {
        padding: 20px;
      }

      .navigation {
        margin: 20px 0;
        padding: 15px;
        background: #f8f9fa;
        border-radius: 5px;
      }

      .nav-link {
        text-decoration: none;
        color: #007bff;
        font-weight: bold;
        margin-right: 20px;
      }

      .nav-link-active {
        color: #28a745;
      }
    `,
  ],
})
export class App {}
