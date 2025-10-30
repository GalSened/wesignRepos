import * as selectActions from "@state/actions/selection.actions";
import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Signer } from '@models/document-api/signer.model';
import { Store } from '@ngrx/store';
import { IAppState } from '@state/app-state.interface';
import { UiViewLicense } from '@models/program/user-program.model';

@Component({
  selector: 'sgn-add-fileds-sidebar',
  templateUrl: './add-fileds-sidebar.component.html',
  styles: []
})
export class AddFiledsSidebarComponent implements OnInit, AfterViewInit {

  @Input() public signers: Signer[];
  public selectedSigner: Signer;

  @Input() showtemplateName : boolean;
  state$: any;
  stateSubscription: any;
  uiViewLicense: UiViewLicense;
  @Input()
  public get templateName() {
    return this.name;
  }

  public set templateName(val) {
    this.name = val;
    this.templateNameChange.emit(this.name);
  }

  @Output() public templateNameChange = new EventEmitter<string>();
  private name: string;

  constructor(
    private store: Store<IAppState>,
    private changeDetectorRef: ChangeDetectorRef,
  ) { }

  ngAfterViewInit(): void {
    this.changeDetectorRef.detectChanges();
  }

  ngOnInit() {
    this.state$ = this.store.select<any>('appstate');
    this.stateSubscription = this.state$.subscribe(a => {
      this.uiViewLicense = a.program.uiViewLicense;
    });
    if (this.signers && this.signers.length > 0){
      this.selectedSigner = this.signers[0];
      this.store.dispatch(new selectActions.SelectSignerClassId({classId : this.selectedSigner.ClassId }));
    }
    else{
      this.selectedSigner = new Signer();
    }
  }

  changeSignerClassId(){
    this.store.dispatch(new selectActions.SelectSignerClassId({classId : this.selectedSigner.ClassId }));
  }
}
