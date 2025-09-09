import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient);

  pingResponse = signal<string>('');
  grpcResponse = signal<string>('');
  rustyResponse = signal<string>('');

  ngOnInit(): void {
    this.http.get<{ message: string }>(`${environment.apiBaseUrl}/ping`).subscribe(response => {
      this.pingResponse.set(response.message);
    });

    this.http.get<{ message: string }>(`${environment.apiBaseUrl}/callGreeter`).subscribe(response => {
      this.grpcResponse.set(response.message);
    });

    this.http.get<{ message: string }>(`${environment.apiBaseUrl}/rusty`).subscribe(response => {
      this.rustyResponse.set(response.message);
    });
  }
}
