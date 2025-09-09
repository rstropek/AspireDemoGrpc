import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MessageService, MessageData } from '../message.service';

@Component({
  selector: 'app-mqtt-messaging',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './mqtt-messaging.component.html',
  styleUrls: ['./mqtt-messaging.component.css']
})
export class MqttMessagingComponent implements OnInit, OnDestroy {
  private messageService = inject(MessageService);

  messages = signal<MessageData[]>([]);
  streaming = signal<boolean>(false);
  sending = signal<boolean>(false);
  
  messageText = '';

  ngOnInit(): void {
    // Subscribe to incoming messages
    this.messageService.messages$.subscribe(message => {
      this.messages.update(current => [...current, message]);
    });
  }

  ngOnDestroy(): void {
    this.messageService.stopMessageStream();
  }

  sendMessage(): void {
    if (!this.messageText.trim()) return;
    
    this.sending.set(true);
    this.messageService.sendMessage(this.messageText).subscribe({
      next: (response) => {
        console.log('Message sent:', response);
        this.messageText = '';
        this.sending.set(false);
      },
      error: (error) => {
        console.error('Error sending message:', error);
        this.sending.set(false);
      }
    });
  }

  toggleMessageStream(): void {
    if (this.streaming()) {
      this.messageService.stopMessageStream();
      this.streaming.set(false);
    } else {
      this.messageService.startMessageStream();
      this.streaming.set(true);
    }
  }
}
