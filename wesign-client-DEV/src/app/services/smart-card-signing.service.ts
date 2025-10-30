import { EventEmitter, Injectable } from '@angular/core';
import { AppConfigService } from './app-config.service';
import * as signalR from '@microsoft/signalr';
import { LogsApiService } from './logs-api.service';
import { SharedService } from './shared.service';
import { Errors } from '@models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class SmartCardSigningService {

  private connection: any;
  private smartCardSigningApi: string;
  private isInitiated = false;
  private loggingToken: string;
  private roomId: string;
  private sentClientConnected = false;
  fieldNameToImage: { [fieldName: string]: string; };

  constructor(private appConfigService: AppConfigService, private translate: TranslateService, private logsApiService: LogsApiService, private sharedService: SharedService) {
    this.smartCardSigningApi = this.appConfigService.hubapi + "/smartcard/";
  }

  public Init(loggingToken: string) {
    this.loggingToken = loggingToken;
    // mapping to the smartcardsocket as in startup.cs
    if (!this.isInitiated) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.appConfigService.hubapi + "/smartcardsocket")
        .configureLogging(signalR.LogLevel.Information)
        .build();
      this.connection.serverTimeoutInMilliseconds = 1000 * 60 * 10; // 1 second * 60 * 10 = 10 minutes.

      this.start();
      this.isInitiated = true;
      this.sentClientConnected = false;
    }
  }
  // Start the connection
  public async start() {
    try {
      await this.connection.start().catch((err) => {
        //  this.logsApiService.error("Error in start connection to hub : " + err, this.loggingToken).subscribe();        
        this.logsApiService.error("Error in start connection to hub : " + err, this.loggingToken);
      });;
      //this.logsApiService.info("connected", this.loggingToken).subscribe();      
      this.logsApiService.info("connected", this.loggingToken);
    } catch (err) {
      //this.logsApiService.error("Error in start connection to hub : " + err, this.loggingToken).subscribe();
      this.logsApiService.error("Error in start connection to hub : " + err, this.loggingToken);
      setTimeout(() => this.start(), 5000);
    }
  }

  public sign(documentId: string, token: string, isLastDocumentInCollection: boolean) {
    //this.fieldNameToImage = fieldNameToImage;
    this.connection.on("GetRoomId", (id) => {
      if (this.roomId != id) {

        this.roomId = id;
        this.sentClientConnected = false;
        //exe local app with roomid and host 
        //smartCardDesktopClient:/{roomId}_{host}      
        //let exeLocalApp:string = 'smartCardDesktopClient:/' + this.roomId + "_https://wesign3.comda.co.il:443/userapi/v3"; // + 
        let exeLocalApp: string = `smartCardDesktopClient:/${this.roomId}_${this.appConfigService.hubapi}`
        // 'smartCardDesktopClient:/' + this.roomId + "_" + + this.appConfigService.hubapi; // + 

        //this.appConfigService.hubapi;

        window.open(exeLocalApp, "_blank").focus();
      }
    });

    this.connection.on("DesktopClientJoin", (desktoClientId, roomId) => {
      if (roomId == this.roomId && !this.sentClientConnected) {
        this.sentClientConnected = true;
        this.connection.invoke("SendHashToDesktopClient", this.roomId);
      }
    });

    this.connection.on("GetMessage", (errorMessage, roomId) => {
      if (this.roomId == roomId) {
        this.sharedService.setBusy(false);
        this.sharedService.setErrorAlert(errorMessage);
      }
    });

    this.connection.on("GetSmartCardSigningResult", (isSuccess, downloadLink) => {
      this.emitSmartCardSigningResultEvent(isSuccess, downloadLink);
    });

    // this.connection.on("GetSmartCardGovSigningResult", (isSuccess, xmlContent,fileName) => {
    //   this.emitSmartCardGovSigningResultEvent(isSuccess, xmlContent,fileName);
    // });

    this.connection.onclose(error => {
      this.logsApiService.error("Error on close = " + error, this.loggingToken);
      this.sharedService.setBusy(false);
      let er = new Errors();
      er.errorCode = 500;
      this.sharedService.setErrorAlert(er);
    });

    this.connection.invoke("CreateGroup", documentId, token, isLastDocumentInCollection)
      .catch((err: any) => {
        console.log(err);
        this.sharedService.setBusy(false);
        this.sharedService.setErrorAlert(this.translate.instant("ERROR.OPERATION.3"));
        setTimeout(() => this.start(), 5000);
      });
  }

  getSmartCardResult: EventEmitter<any> = new EventEmitter();

  emitSmartCardSigningResultEvent(isSuccess, downloadLink) {
    this.getSmartCardResult.emit({ isSuccess: isSuccess, downloadLink: downloadLink });
  }
  // emitSmartCardGovSigningResultEvent(isSuccess, xmlContent, fileName) {
  //   this.getSmartCardResult.emit({ isSuccess: isSuccess, xml: xmlContent ,fileName: fileName});
  // }

  getSmartCardSigningResultEvent() {
    return this.getSmartCardResult;
  }
}