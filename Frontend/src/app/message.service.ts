import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../environments/environment';

export interface MessageData {
  message: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private http = inject(HttpClient);
  private messageSubject = new Subject<MessageData>();
  private eventSource: EventSource | null = null;

  public messages$ = this.messageSubject.asObservable();

  sendMessage(message: string): Observable<unknown> {
    return this.http.post(`${environment.apiBaseUrl}/sendMessage`, 
      { message }, 
      { headers: { 'Content-Type': 'application/json' } }
    );
  }

  startMessageStream(): void {
    if (this.eventSource) {
      return; // Already connected
    }

    this.eventSource = new EventSource(`${environment.apiBaseUrl}/messages`);
    
    this.eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data) as MessageData;
        this.messageSubject.next(data);
      } catch (error) {
        console.error('Error parsing SSE message:', error);
      }
    };

    this.eventSource.onerror = (error) => {
      console.error('SSE connection error:', error);
    };
  }

  stopMessageStream(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
  }
}
