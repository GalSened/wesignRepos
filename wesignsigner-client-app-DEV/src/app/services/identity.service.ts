import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AppConfigService } from './app-config.service';

@Injectable({
  providedIn: 'root'
})
export class IdentityService {
  private connection: signalR.HubConnection;
  private isConnected = false;
  public processDoneAdSubject = new Subject();
  public reconnectSubject = new Subject();
  roomId: string;
  public needToReconnect = false;

  constructor(private appConfigService: AppConfigService) {
    this.connection = new signalR.HubConnectionBuilder().withUrl(this.appConfigService.hubUrl + "/identitsocket").withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.connection.serverTimeoutInMilliseconds = 1000 * 60 * 10; // 1 second * 60 * 10 = 10 minutes.
    this.connection.keepAliveIntervalInMilliseconds = 15000
    this.connection.onclose((err) => {
      //   console.log("onclose conncction start reconnecting again");

      this.isConnected = false;
      this.reconnectSubject.next();
    })

    this.connection.onreconnecting((err) => {
      this.isConnected = false;
      //   console.log("Reconnecting : err " + err);
    });

    this.connection.onreconnected((connectionId) => {
      //    console.log("in onreconnected Connecting to room " + this.roomId);
      this.connect(this.roomId);
      this.listenToonIdentityDone();
    });

    this.start();

  }

  connect(id: string) {
    //"in connect to agent - connection state : " + this.connection.state); 
    if (this.connection.state == signalR.HubConnectionState.Connected && !this.isConnected) {
      this.roomId = id;
      //  console.log("Connecting to room " + id);
      this.connection.invoke("Connect", id);
      this.isConnected = true;
    }
  }


  processDone(signerToken: string, identityToken: string) {
    this.connection.invoke("SignerDone", signerToken, identityToken);
  }

  listenToonIdentityDone() {
    this.connection.on("onIdentityDone", (token) => {
      //     console.log("listenToOnLinkChange link  :  " + link);
      this.processDoneAdSubject.next(token);
    });
  }

  private async start() {
    try {
      await this.connection.start().catch(
        (err) => {
          console.log("Error in start connection to hub : " + err);
        });
    }
    catch (err) {
      //  console.log(err);
      setTimeout(() => this.start(), 5000);
    }
  }
}