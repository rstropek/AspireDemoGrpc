import { Component } from '@angular/core';

import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-root',
    imports: [],
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Aspire Demo Angular Frontend';
  
  pingResult = '';
  addResult = '';
  postgresResult = '';
  mqttMessages: string[] = [];
  
  private readonly apiBaseUrl = 'https://localhost:7054';

  constructor(private http: HttpClient) {}

  async ping() {
    try {
      const response = await this.http.get(`${this.apiBaseUrl}/ping`, { responseType: 'text' }).toPromise();
      this.pingResult = response || '';
    } catch (error) {
      this.pingResult = 'Error: ' + error;
    }
  }

  async add() {
    try {
      const response = await this.http.get<{sum: number}>(`${this.apiBaseUrl}/add`).toPromise();
      this.addResult = response?.sum?.toString() || '';
    } catch (error) {
      this.addResult = 'Error: ' + error;
    }
  }

  async postgres() {
    try {
      const response = await this.http.get<{answer: number}>(`${this.apiBaseUrl}/answer-from-db`).toPromise();
      this.postgresResult = response?.answer?.toString() || '';
    } catch (error) {
      this.postgresResult = 'Error: ' + error;
    }
  }

  async publishMqttMessage() {
    const message = prompt('Enter message to publish:');
    if (message) {
      try {
        await this.http.get(`http://localhost:5001/publish/${encodeURIComponent(message)}`).toPromise();
        alert('Message published successfully!');
      } catch (error) {
        alert('Error publishing message: ' + error);
      }
    }
  }

  async getMqttMessages() {
    try {
      const response = await this.http.get<{messages: string[]}>(`http://localhost:5002/messages`).toPromise();
      this.mqttMessages = response?.messages || [];
    } catch (error) {
      this.mqttMessages = ['Error: ' + error];
    }
  }

  async clearMqttMessages() {
    try {
      await this.http.get(`http://localhost:5002/clear`).toPromise();
      this.mqttMessages = [];
    } catch (error) {
      alert('Error clearing messages: ' + error);
    }
  }
}
