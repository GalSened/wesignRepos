import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LogLevel, LogMessage } from '@models/log-message.model';

import { AppConfigService } from './app-config.service';

@Injectable({
  providedIn: 'root'
})
export class LogsApiService {

 
  private logsApi : string = "";

  constructor(private httpClient: HttpClient, private appConfigService: AppConfigService) {
      this.logsApi = this.appConfigService.signerApiUrl + "/logs";
  }

  public info(message : string, token : string){    
    let logMessage = new LogMessage();
    logMessage.token = token;
    logMessage.message = message;
    logMessage.logLevel = LogLevel.Information;
    logMessage.applicationName = "User Frontend Client";
    logMessage.timeStamp = new Date();

   // console.log("info | "+message);
    //return this.httpClient.post(`${this.logsApi}`, logMessage);
  }

  public debug(message : string, token : string){    
    let logMessage = new LogMessage();
    logMessage.token = token;
    logMessage.message = message;
    logMessage.applicationName = "User Frontend Client";
    logMessage.timeStamp = new Date();

  // console.log("debug | "+message);
    //return this.httpClient.post(`${this.logsApi}`, logMessage);
  }

  public error(message : string, token : string){    
    let logMessage = new LogMessage();
    logMessage.token = token;
    logMessage.message = message;
    logMessage.logLevel = LogLevel.Error;
    logMessage.applicationName = "User Frontend Client";
    logMessage.timeStamp = new Date();

  //  console.log("error | "+message);
    //return this.httpClient.post(`${this.logsApi}`, logMessage);
  }

}