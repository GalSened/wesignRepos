import { Observable } from 'rxjs';

export class Modal {
    public showModal: boolean = false;
    public title: string = "";
    public content: string = "";
    public confirmBtnText: string = "";
    public rejectBtnText: string = "";
    public confirmAction : Observable<any>;
    public confirmAction2 : Observable<any>;    

    constructor(values: Object = {}) {
        Object.assign(this, values);
      }
}
