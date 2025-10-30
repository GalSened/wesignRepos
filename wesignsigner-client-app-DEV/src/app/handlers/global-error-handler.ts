import { ActivatedRoute } from '@angular/router';
import { ErrorHandler, Injectable} from '@angular/core';
import { LogsApiService } from '../services/logs-api.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler{
    token: any;   
    
    constructor(private logsService: LogsApiService, private route: ActivatedRoute) {
        this.route.paramMap.subscribe(params => {
            this.token = params.get("id")
          });
    }
    
  handleError(error: any): void {
      this.logsService.error(error.message, this.token).subscribe();
      console.error("Error from global error handler", error);

  } 
}