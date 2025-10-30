import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { LogsApiService } from '@services/logs-api.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  //constructor(private errorDialogService: ErrorDialogService, private zone: NgZone) {}
  public storage: Storage = sessionStorage || localStorage;
  private readonly JWT_TOKEN: string = 'JWT_TOKEN';
  
  constructor(private logsService: LogsApiService, private zone: NgZone) {}

  handleError(error: Error) {

    /*
    Even if the error is thrown outside the ngZone.
    This is for example the case if an error occurs in a lifecycle hook like the ngOnInit function in a component.
     */
    this.zone.run(() =>

    //this.logsService.error(error.message, this.accessToken()).subscribe()
    this.logsService.error(error.message, this.accessToken())

    //   this.errorDialogService.openDialog(
    //     error.message || "Undefined client error")
    );

    console.error("Error from global error handler", error);
  }

   accessToken(): string {
    return this.storage.getItem(this.JWT_TOKEN) || localStorage.getItem(this.JWT_TOKEN);
}
}
