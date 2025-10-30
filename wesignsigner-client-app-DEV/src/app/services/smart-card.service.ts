import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr'; 
import { Subject } from 'rxjs';
import { AppState } from '../models/state/app-state.model';
import { AppConfigService } from './app-config.service';
import { StateService } from './state.service';

@Injectable({
  providedIn: 'root'
})
export class SmartCardService {
  private connection: any;
  public alertError = new Subject();
  public state: AppState;
  private smartCardSigningApi: string;
  private isInitiated = false;
  private loggingToken: string;
  private sentClientConnected = false;

  constructor(private appConfigService: AppConfigService,
      private stateService: StateService) {        
          this.stateService.state$.subscribe(
              (data)=>{
                  this.state = data;
              }
          )
      this.smartCardSigningApi = this.appConfigService.apiUrl + "/smartcard/";
      // mapping to the smartcardsocket as in startup.cs
     
  }

  public Init(loggingToken: string)
  {
      this.loggingToken = loggingToken;
      if(!this.isInitiated){
        this.connection = new signalR.HubConnectionBuilder().withUrl(this.appConfigService.hubUrl + "/smartcardsocket").withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
        this.connection.serverTimeoutInMilliseconds = 1000 * 60 * 10; // 1 second * 60 * 10 = 10 minutes.
        this.sentClientConnected = false;
        this.start();
        this.isInitiated = true;
      }
  }
  
  // Strart the connection
  private async start() {
      try {
          await this.connection.start().catch((err)=>{
   //           console.log("Error in start connection to hub : " + err);
          });
         // console.log("connected");
          
      } catch (err) {
          console.log(err);
          setTimeout(() => this.start(), 5000);
      }
  }

  public sign(documentCollectionId : string, token : string, isLastDocumentInCollection: boolean) {    
      let roomId : string;
      this.connection.on("GetRoomId", (id) => { 
          roomId = id;
          //exe local app with roomid and host 
          //smartCardDesktopClient:/{roomId}_{host}        
       
          let exeLocalApp = `smartCardDesktopClient:/${roomId}_${this.appConfigService.hubUrl}`;          
          window.open( exeLocalApp,"_blank").focus();    
          
      });
      this.connection.on("DesktopClientJoin", (desktoClientId,  inputRoomId) => {         
        if(roomId == inputRoomId && !this.sentClientConnected)
        {  
          this.sentClientConnected = true;
          this.connection.invoke("SendHashToDesktopClient", roomId);     
        }
       
          
      });
      this.connection.on("GetSmartCardSigningResult", (isSuccess, downloadLink)=>{
          this.emitSmartCardSigningResultEvent(isSuccess, downloadLink);
      });
      this.connection.on("GetMessage", (errorMessage : string, inputRoomId)=>{     
        if(roomId == inputRoomId)
        {
            if(! errorMessage.toUpperCase().includes("SUCCESS"))        
            {
                this.alertError.next(errorMessage);
            }
        }
        });
      this.connection.onclose(error => {
          this.alertError.next(error);

      });
      this.connection.invoke("CreateGroup", documentCollectionId, token, isLastDocumentInCollection).catch((err: any) => console.error("CreateGroup error "+ err));      
    }    


    getSmartCardResult: EventEmitter<any> = new EventEmitter();
    emitSmartCardSigningResultEvent(isSuccess, downloadLink) {
      this.getSmartCardResult.emit({isSuccess: isSuccess, downloadLink: downloadLink});
    }
    getSmartCardSigningResultEvent() {
      return this.getSmartCardResult;
    }


}
