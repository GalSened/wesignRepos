import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { Modal } from '@models/modal/modal.model'
import { ModalService } from '@services/modal.service'
import { Subscription } from 'rxjs';

@Component({
    selector: 'sgn-modal-component',
    templateUrl: 'modal.component.html'
})

export class ModalComponent implements OnInit {
    public data: Modal;
    private subscription: Subscription;

    public confirmFunction(): void {
        this.data.showModal = false;
        if(this.data.confirmBtnText.toLowerCase()  =="submit" || this.data.confirmBtnText=="אישור"){
            this.modalService.applyConfirm2(this.data.confirmAction2);
        }
        else{
            this.modalService.applyConfirm(this.data.confirmAction);
        }
    }
    
    public cancelFunction(): void {
        this.data.showModal = false;
    }

    constructor(private modalService: ModalService) { }

    ngOnInit() {
        this.subscription = this.modalService.modalData.subscribe(a => this.data = a);
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

}

