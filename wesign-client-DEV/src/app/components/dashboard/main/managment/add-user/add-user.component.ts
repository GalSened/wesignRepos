import { Component, OnInit, Input, Output, EventEmitter, ViewChild, AfterViewInit } from '@angular/core';
import { User } from '@models/account/user.model';
import { NgForm } from '@angular/forms';
import { group } from '@models/managment/groups/management-groups.model';
import { SharedService } from '@services/shared.service';
import { userType } from '@models/enums/user-type.enum';
import { TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'sgn-add-user',
  templateUrl: './add-user.component.html',
  styles: []
})
export class AddUserComponent implements OnInit, AfterViewInit {

  public submitted: boolean = false;

  @Input()
  public user: User;


  @Input()
  public currentUser: User;



  @Input()
  public isUpdateMode: boolean;

  @Input()
  public groups: group[];

  @Input()
  public groupsNames: string[];

  @Output()
  public accept = new EventEmitter<User>();
  @Output()
  public update = new EventEmitter<User>();
  public selectedGroupsId: any;
  @Output()
  public cancel = new EventEmitter<void>();
  selectedUserType: any;
  selectedUserGroup: any;

  userNameBeforeUpdate: string;

  public groupForDropDown: group[];


  public userTypestring: string;
  @Input() public errorMsg: string = "";

  public get UsersTypeOptions() {
    return SharedService.EnumToArrayHelper<userType>(userType)
  };

  @ViewChild('newUserForm') newUserForm: NgForm;
  @ViewChild('userType') userType: any;
  @ViewChild('userGroup') userGroup: any;

  public selectedGroup: string;
  public errorsArr: string[] = [];
  public get groupsOptions() {
    return this.groupsNames;// ["aaa","bbb", "ccc"];//this.groups$.subscribe((x) => x);  
  }

  public usersTypes;

  public dropDownUserTypeUpdate(value: any) {
    if (value) {
      this.user.type = value;
    }
  }

  public dropDownUserGroupUpdate(value: any) {
    if (value) {
      this.groups.forEach(e => {
        if (e.name.toLowerCase().trim() == this.groupsNames[value].toLowerCase()) {
          this.user.groupId = e.groupId;

        }

      });

    }
  }
  constructor(private sharedService: SharedService,
    private translate: TranslateService,) { }
  ngAfterViewInit(): void {
    this.userTypestring = this.user.type.toString();
  }

  ngOnInit() {
    let types = Object.values(SharedService.EnumToArrayHelper<userType>(userType));
    types = types.filter(x => x != "SystemAdmin" && x != "Unknown");
    this.usersTypes = types;
    this.userNameBeforeUpdate = this.user?.username;
  }


  public save() {


    if (this.user.additionalGroupsIds) {
      this.user.additionalGroupsIds = this.user.additionalGroupsIds.filter(x => x != this.user.groupId);
    }
    this.submitted = true;
    this.errorMsg = '';
    if (!this.isUpdateMode && (!this.selectedUserType || this.selectedUserType == "")) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.INCOMPLETE_FORM`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });
      return;
    }

    if (!this.isUpdateMode && (!this.selectedUserGroup || this.selectedUserGroup == "")) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.INCOMPLETE_FORM`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });
      return;
    }

    if (!this.isUpdateMode && (!this.user.email || this.user.email == "")) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.INCOMPLETE_FORM`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });
      return;
    }

    if (!this.isUpdateMode && !this.containsEmailAddress(this.user.email)) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.EMAIL_IS_NOT_VALID`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        })
      return;
    }

    if (!this.isUpdateMode && (!this.user.name || this.user.name == "")) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.INCOMPLETE_FORM`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });
      return;
    }

    if (this.userNameBeforeUpdate != this.user.username) {

      if (this.user.username && this.user.username.trim() != "") {
        if ((this.user.username.length < 6 || this.user.username.length > 15)) {
          this.submitted = false;
          this.translate.get(`MANAGEMENT.USER_NAME_TOO_LONG_OR_SHORT`).subscribe(
            (msg) => {
              this.errorMsg = msg;
            });

          return;
        }

        if (this.containsHebrewLetters(this.user.username)) {
          this.submitted = false;
          this.translate.get(`MANAGEMENT.USER_NAME_CANNOT_CONTAINNS_HEB_LETTERS`).subscribe(
            (msg) => {
              this.errorMsg = msg;
            });

          return;
        }

        if (this.containsEmailAddress(this.user.username)) {
          this.submitted = false;
          this.translate.get(`MANAGEMENT.USER_NAME_CANNOT_CONTAIN_EMAIL_ADDRESS`).subscribe(
            (msg) => {
              this.errorMsg = msg;
            });

          return;
        }

      }
    }

    if (this.currentUser.id === this.user.id && 
      this.user.additionalGroupsIds && 
      !this.user.additionalGroupsIds.includes(this.currentUser.groupId) && this.user.groupId != this.currentUser.groupId) {
      this.submitted = false;
      this.translate.get(`MANAGEMENT.USER_CANNOT_REMOVE_CONNECTED_GROUP`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });
      return;
    }

    if (this.isUpdateMode) {
      this.update.emit(this.user);

    } else {
      this.accept.emit(this.user);
    }
    this.submitted = false;

  }


  public containsHebrewLetters(str): boolean {

    for (var i = 0; i < str.length; i++) {
      if (/^[[\u0590-\u05FF]*$/.test(str.charAt(i))) {
        return true;
      }
    }
    return false;

  }

  public containsEmailAddress(str): boolean {
    if (/^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{1,})$/.test(str)) {
      return true;
    }
    return false
  }


  public onUserTypeChanged(event, user: User) {
    if (event.target.selectedIndex > 0) {
      this.selectedUserType = event.target.selectedIndex;
      this.user.type = this.selectedUserType;
    }
    else {
      event.target.selectedIndex = this.selectedUserType;
    }
  }

  public onUserGroupChanged(event, user: User) {
    this.selectedUserGroup = event.target.selectedIndex;
    if (this.groups[this.selectedUserGroup - 1]) {
      this.user.groupId = this.groups[this.selectedUserGroup - 1].groupId;
    }
  }

  getGroupName(user) {
    if (user.groupId && this.groups.length > 0) {
      let group = this.groups.find(x => x.groupId == user.groupId);
      if (group) {
        return group.name;
      }
    }
  }


}
export class groupToSelect extends group {
  public Checked: boolean;
}
