import { Component, OnInit, Output, EventEmitter, ViewChild, Input } from '@angular/core';
import { AdminApiService } from '@services/admin-api.service';
import { group } from '@models/managment/groups/management-groups.model';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { SharedService } from '@services/shared.service';
import { TranslateService } from '@ngx-translate/core';
import { Errors } from '@models/error/errors.model';
import { NgForm } from '@angular/forms';
import { managmentGroupRequest } from '@models/managment/groups/management-group-request.model';
import { formatBaseResult } from '@models/base/base-result.model';
import { GroupAssignService } from '@services/group-assign.service';
import { Modal } from '@models/modal/modal.model';

@Component({
  selector: 'sgn-manage-gourp',
  templateUrl: './manage-gourp.component.html',
  styles: []
})
export class ManageGourpComponent implements OnInit {

  constructor(private adminApiService: AdminApiService, private sharedService: SharedService,
    private translate: TranslateService,) { }
    public currentGroupId :string;
  @Input() public groups: group[];
  
  @Output()
  public cancel = new EventEmitter<void>();
  //public groupName: string = "";
  public disableAddClick: boolean = false;
  public error: string = "";

  public deleteGroupPopupData: Modal = new Modal();
  @ViewChild('newGroupForm') newGroupForm: NgForm;
  public back() {
   // this.groupName = "";
    this.cancel.emit();

  }
  public groupNAmeKeyPress() {
    this.error = "";
  }
  public addGroup() {
    let groupName = this.groups[this.groups.length - 1].name;
    if (!groupName || groupName == "") {
      return;
    }
    this.sharedService.setBusy(true, "CONTACTS.LOADING");
    let newGroupRequest = new managmentGroupRequest();
    newGroupRequest.name = groupName
    this.adminApiService.createNewGroup(newGroupRequest).subscribe((x) => {
      
      
      let addedGroup = this.groups.find(x=>x.name==groupName);
      addedGroup.groupId = x.groupId;
      this.groups.push(new group());

    }, (errorResult) => {
      let result = new Errors(errorResult.error);
      this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
        (msg) => {
          this.error = msg;
        });


    },
      () => {
        this.sharedService.setBusy(false);
        
        this.disableAddClick = false;
      });



  }
  ngOnInit() {   
    this.groups.push(new group());
  }

public doDeleteGroupEvent()
{
  this.sharedService.setBusy(true, "DOCUMENT.DELETING");
    this.adminApiService.deleteGroup(this.currentGroupId).subscribe(
      (x) => {
        this.groups = x.groups;

        
      },
      (errorResult) => {
        let result = new Errors(errorResult.error);
        this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
          (msg) => {
            this.error = msg;

          });
          this.sharedService.setBusy(false);
        this.currentGroupId = "";
        this.deleteGroupPopupData.showModal = false;
      },

      () => {
        this.sharedService.setBusy(false);
        this.currentGroupId = "";
        this.deleteGroupPopupData.showModal = false;
      });
}
  public deleteGroup($event: any, groupId: string, groupName : string) {

    this.currentGroupId = groupId;
    $event.preventDefault();
    $event.stopPropagation();
    this.deleteGroupPopupData.showModal = true;
    this.translate.get(['MANAGEMENT.DELETE_GROUP', 'MANAGEMENT.MODALTEXT', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
      .subscribe((res: object) => {
        let keys = Object.keys(res);
        this.deleteGroupPopupData.title = res[keys[0]];
        this.deleteGroupPopupData.content = (res[keys[1]] as string).replace("{#*#}", groupName);
        this.deleteGroupPopupData.rejectBtnText = res[keys[2]];
        this.deleteGroupPopupData.confirmBtnText = res[keys[3]];
      });
  }

  public editGroup(event: any, groupId: string, groupName : string){
    this.sharedService.setBusy(true, "TEMPLATE.SAVING");

    let req = new managmentGroupRequest();
    req.name = groupName;
    this.adminApiService.updateGroup(req, groupId).subscribe(
      x => {
       
      },
      (errorResult) => {
        let result = new Errors(errorResult.error);
        this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
          (msg) => {
            this.error = msg;
          });
      }
      ,

      () => {
        this.sharedService.setBusy(false);
      }

      );
  }
}
