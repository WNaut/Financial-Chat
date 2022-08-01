import {HttpErrorResponse} from '@angular/common/http';
import {EventEmitter, Inject, Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {ToastrService} from 'ngx-toastr';
import {AuthService} from './auth.service';
import {ChatUser} from '../models/chat/chat-user.model';
import {ChatMessage} from '../models/chat/chat-message.model';

@Injectable({
  providedIn: 'root'
})
export class MessagingService {
  private connection: signalR.HubConnection;
  connectedUsers = new EventEmitter<ChatUser[]>();
  currentMessages = new EventEmitter<ChatMessage[]>();
  newMessage = new EventEmitter<{ chatMessage: ChatMessage, invokedCommand: boolean }>();
  join = new EventEmitter<{ room: string, invokedCommand: boolean }>();
  commandFormat = '/stock=';
  botName = 'Financial Bot';
  invokedCommand = false;

  constructor(
    @Inject('BASE_URL') private baseUrl: string,
    private toastr: ToastrService,
    private authService: AuthService) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}hub/chat`, {
        accessTokenFactory: () => this.authService.accessToken
      }).build();
    this.startConnection();
  }

  private startConnection() {
    this.connection.serverTimeoutInMilliseconds = 36000000;
    this.connection.keepAliveIntervalInMilliseconds = 1800000;

    this.connection.start().then(() => {
      this.receiveConnectedUsers();
      this.receiveCurrentMessages();
      this.receiveMessage();
    }).catch((error: HttpErrorResponse) => {
      this.toastr.error(error.error, 'Error connecting to the chat');
    });
  }

  private receiveMessage() {
    this.connection.on('NewMessage', (message: ChatMessage) => {
      this.newMessage.emit({chatMessage: message, invokedCommand: this.invokedCommand});

      if (message.sentBy === this.botName && this.invokedCommand) {
        this.invokedCommand = false;
      }
    });
  }

  private receiveConnectedUsers() {
    this.connection.on('ChatUsersChanged', (response: ChatUser[]) => {
      this.connectedUsers.emit(response);
    });
  }

  private receiveCurrentMessages() {
    this.connection.on('CurrentMessages', (messages: ChatMessage[]) => {
      this.currentMessages.emit(messages);
    });
  }

  closeConnectionForCurrentClient() {
    let username = '';

    this.authService.currentUser$.subscribe(user => {
      username = user.username;
    });

    this.connection.invoke('Disconnect', username).then(() => {
      this.authService.logout();
    }).catch(() => {
      this.toastr.error('An error occurred while logging out.', 'Error');
    });
  }

  sendNewMessage(message: ChatMessage) {
    this.invokedCommand = message.message.includes(this.commandFormat);

    return this.connection.invoke('SendMessage', message);
  }

 joinRoom(message: string) {
  return this.connection.invoke('Join', message);
 }

  saveBotMessage(message: ChatMessage) {
    return this.connection.invoke('SendBotMessage', message);
  }
}
