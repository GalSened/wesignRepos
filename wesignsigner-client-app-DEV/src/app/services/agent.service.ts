import { ElementRef, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppConfigService } from './app-config.service';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AgentService {
  private connection: signalR.HubConnection;
  private isConnected: boolean = false;
  public linkChangeSubject = new Subject();
  public moveToAdSubject = new Subject();
  public reconnectSubject = new Subject();
  roomId : string;
  public needToReconnect = false;
  constructor(private appConfigService: AppConfigService,  private router: Router) {
    this.connection = new signalR.HubConnectionBuilder().withUrl(this.appConfigService.hubUrl + "/agentsocket").withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.connection.serverTimeoutInMilliseconds = 1000 * 60 * 10; // 1 second * 60 * 10 = 10 minutes.
    this.connection.keepAliveIntervalInMilliseconds = 15000
    this.connection.onclose((err) => {
   //   console.log("onclose conncction start reconnecting again");
      
      this.isConnected = false;
      this.reconnectSubject.next();
    })
    
    this.connection.onreconnecting((err) =>{
      this.isConnected = false;
   //   console.log("Reconnecting : err " + err);
    }
    );
    this.connection.onreconnected((connectionId) => {
  //    console.log("in onreconnected Connecting to room " + this.roomId);
      this.connect(this.roomId);
      this.listenToImAlive();
      this.listenToOnLinkChange();
      this.listenToOnMoveToAd();
      
  });

    this.start();
  }
  
  reconnect(){
    this.connect(this.roomId);
      this.listenToImAlive();
      this.listenToOnLinkChange();
      this.listenToOnMoveToAd();;
  }



  private async start() {
    try {
      await this.connection.start().catch(
        (err) => {
   //       console.log("Error in start connection to hub : " + err);
        });
    }
    catch (err) {
    //  console.log(err);
      setTimeout(() => this.start(), 5000);
    }
  }

  connect(id: string) {
    //"in connect to agent - connection state : " + this.connection.state); 
    if ( this.connection.state  == signalR.HubConnectionState.Connected && !this.isConnected) {
      this.roomId = id;
    //  console.log("Connecting to room " + id);
      this.connection.invoke("Connect", id);
      this.isConnected = true;
    }
  }

  imAlive(id: string)
  {
   // console.log("send ping to hub "+ id);
    setTimeout(() => {
      if(this.needToReconnect)
    {
      this.isConnected = false;
      this.reconnect();
    }
    },3000)
      this.needToReconnect = true;
    this.connection.invoke("Ping", id);
  }

  listenToImAlive() {
    this.connection.on("Ping", () => {
      this.needToReconnect = false;
    //  console.log("listenToImAlive get call from hub ");
      
    });
  }
  listenToOnLinkChange() {
    this.connection.on("onLinkChange", (link) => {
 //     console.log("listenToOnLinkChange link  :  " + link);
      this.linkChangeSubject.next(link);
    });
  }

  listenToOnMoveToAd() {
    this.connection.on("onMoveToAd", () => {
      this.moveToAdSubject.next();
    });
  }


}
