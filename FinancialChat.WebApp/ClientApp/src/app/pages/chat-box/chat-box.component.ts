import {Component, OnDestroy, OnInit} from '@angular/core';
import {HttpErrorResponse} from '@angular/common/http';
import {Subscription} from 'rxjs';
import {FormControl} from '@angular/forms';
import {AuthService} from '../../core/services/auth.service';
import {ChatUser} from '../../core/models/chat/chat-user.model';
import {ChatMessage} from '../../core/models/chat/chat-message.model';
import {MessagingService} from '../../core/services/messaging.service';

@Component({
  selector: 'app-chat-box',
  templateUrl: './chat-box.component.html',
  styleUrls: ['./chat-box.component.css']
})
export class ChatBoxComponent implements OnInit, OnDestroy {
  connectedClients: ChatUser[] = [];
  connectedClientsSubscription: Subscription;
  currentMessages: ChatMessage[] = [];
  currentMessagesSubscription: Subscription;
  newMessageSubscription: Subscription;
  currentUsername: string;
  message = new FormControl('');

  constructor(private messagingService: MessagingService,
              private authService: AuthService
  ) {
  }

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUsername = user.username;
    });
    this.subscribeToChatEvents();
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  subscribeToChatEvents() {
    this.connectedClientsSubscription = this.messagingService.connectedUsers.subscribe((connectedUsers: ChatUser[]) => {
      if (connectedUsers) {
        this.connectedClients = connectedUsers;
        console.log(connectedUsers)
      }
    });

    this.currentMessagesSubscription = this.messagingService.currentMessages.subscribe((currentMessages: ChatMessage[]) => {
      if (currentMessages) {
        this.currentMessages = currentMessages;
      }
    });

    this.newMessageSubscription = this.messagingService.newMessage
      .subscribe(async (newMessage: { chatMessage: ChatMessage, invokedCommand: boolean }) => {
      if (newMessage) {
        this.currentMessages.push(newMessage.chatMessage);

        if (newMessage.chatMessage.sentBy === this.messagingService.botName && newMessage.invokedCommand) {
          await this.messagingService.saveBotMessage(newMessage.chatMessage);
        }
      }
    });
  }

  getMessageClass(username: string) {
    switch (username) {
      case this.messagingService.botName:
        return 'bot';
      case this.currentUsername:
        return 'me';
      default:
        return 'other';
    }
  }

  send() {
    if (this.message.value.trim() === '') {
      return;
    }

    const newMessage: ChatMessage = {
      sentBy: this.currentUsername,
      sentOn: new Date(),
      message: this.message.value
    };

    this.messagingService.sendNewMessage(newMessage)
      .then(() => {
        this.message.setValue('');
      })
      .catch((error: HttpErrorResponse) => {
        console.log(error);
      });
  }

  changeRoom(room: string) {
    this.messagingService.joinRoom(room).then(() => {
      this.message.setValue('');
    })
    .catch((error: HttpErrorResponse) => {
      console.log(error);
    });
  }

  disconnect() {
    this.connectedClientsSubscription.unsubscribe();
    this.currentMessagesSubscription.unsubscribe();
    this.messagingService.closeConnectionForCurrentClient();
  }
}
