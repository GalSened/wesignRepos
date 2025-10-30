import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AutoReportsNotificationService {
    private newReportNotificationSource = new Subject<void>();
    private deletedReportNotificationSource = new Subject<void>();
    newReportNotification$ = this.newReportNotificationSource.asObservable();
    deletedReportNotification$ = this.deletedReportNotificationSource.asObservable();

    onAddedReport() {
        this.newReportNotificationSource.next();
    }

    onDeletedReport() {
        this.deletedReportNotificationSource.next();
    }
}