import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject, Observable } from 'rxjs'
import { Modal } from '@models/modal/modal.model'

@Injectable({ providedIn: 'root' })
export class ModalService {

    private modalSource = new BehaviorSubject<Modal>(new Modal());
    public modalData = this.modalSource.asObservable();

    // private confirmAction = new Subject<boolean>();
    private confirmAction = new Subject<Observable<any>>();
    private confirmAction2 = new Subject<Observable<any>>();

    constructor() { }

    public showModal(data: Modal) {
        this.modalSource.next(data);
    }

    // public applyConfirm(isConfirmed: boolean) {
    public applyConfirm(action: Observable<any>) {
        this.confirmAction.next(action);       
    }
    public applyConfirm2(action: Observable<any>) {
        this.confirmAction2.next(action);       
    }

    public checkConfirm(): Observable<any> {
        return this.confirmAction.asObservable();
    }
    public checkConfirm2(): Observable<any> {
        return this.confirmAction2.asObservable();
    }
}