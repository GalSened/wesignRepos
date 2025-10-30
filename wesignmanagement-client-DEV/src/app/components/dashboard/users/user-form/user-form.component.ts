import value from '*.json';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Console } from 'console';
import { element } from 'protractor';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Operation } from 'src/app/enums/operaion.enum';
import { UserType } from 'src/app/enums/user-type.enum';
import { AddUser, UpdateUserModel } from 'src/app/models/add-user';
import { Errors } from 'src/app/models/error/errors.model';
import { Filter } from 'src/app/models/filter.model';
import { CompanyResult } from 'src/app/models/results/companany-result.model';

import { CompaniesResult } from 'src/app/models/results/companies-result.model';
import { User } from 'src/app/models/user.model';
import { CompaniesApiService } from 'src/app/services/companies-api.service';
import { SharedService } from 'src/app/services/shared.service';
import { UsersApiService } from 'src/app/services/users-api.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnInit {

  @Input() mode: Operation = Operation.Update;
  @Input() user: User;
  @Output() bannerAlertEvent = new EventEmitter<string>();
  @Output() public hideUserForm = new EventEmitter<number>();
  public submited: boolean = false;
  public isBusy: boolean = false;
  public isError: boolean = false;
  public companiesResult: CompanyResult[];
  public filter: Filter = new Filter();
  public addUserModel = new AddUser();
  public errorMessage: string = "";
  private options: Array<{ item1: string, item2: string }> = [];
  private groupsOptions: [string, string][] = [];
  public userTypesOptions = { 1: "Basic", 2: "Editor", 3: "CompanyAdmin", 4: "SystemAdmin" };
  public selectedGroup: string = "";
  public selectedCompant: string = "";
  public passMinLengthMsg: string;
  constructor(private companiesApi: CompaniesApiService, private usersApiService: UsersApiService, private sharedService: SharedService) { }
  public selectedUserType = 2;
  public isSystemAdmin: boolean = false;
  public sysPassword: string = "";
  public submitText: string = "Create";
  public userEmail: string = "";
  public userUsername: string = "";
  public userName: string = "";

  ngOnInit(): void {
    this.filter.limit = -1;
    this.filter.offset = 0;
    this.companiesApi.Read(this.filter).subscribe(
      (result) => {
        this.companiesResult = result.body.companies;
        this.companiesResult.forEach(x => this.options.push({ item1: x.name, item2: x.id }));
      }
    )
  }

  public cancel() {
    this.isSystemAdmin = false;
    this.addUserModel = new AddUser();
    this.selectedUserType = 2
    this.hideUserForm.emit(0);

  }

  public CollectionToGroupsOptions() {
    let goptions = this.groupsOptions.reduce((prev, curr) => {
      if (curr["item1"]) {
        prev[curr["item1"]] = curr["item2"];
      }
      if (curr["0"]) {
        prev[curr["0"]] = curr["1"];
      }
      return prev;
    }, {});

    return goptions;
  }

  dropDownUpdateGroup($event) {

    this.addUserModel.GroupId = $event;
  }

  public CollectionToUserTypeOptions() {
    return this.userTypesOptions;
  }

  dropDownUpdateCompany($event, comp, group) {
    this.groupsOptions = [];
    this.addUserModel.GroupId = "";
    let companyName = this.options[$event];
    this.addUserModel.CompanyId = companyName.item2;
    this.companiesApi.ReadCompany(companyName.item2, companyName.item2).subscribe(
      (result) => {
        this.groupsOptions = result.body.groups;

      }
    )


  }

  public dropDownUpdateUserType($event) {
    this.selectedUserType = $event;
    if (this.selectedUserType == 4) {
      this.isSystemAdmin = true;
    }
    else {
      this.isSystemAdmin = false;
    }
  }
  public CollectionToOptions() {

    return this.options.map(x => x.item1);

  }

  public submit() {
    this.isError = false;

    if (this.selectedUserType != 4 && this.userName == "") {
      this.errorMessage = "Missing User Name";
      this.isError = true;
      return;
    }
    if (this.userEmail == "") {
      this.errorMessage = "Missing User Email";
      this.isError = true;
      return;
    }
    if (this.userName != null) {
      if (this.userName.length > 0) {
        if (this.hasHebrew(this.userUsername)) {
          this.errorMessage = "Username cannot consists Hebrew letters";
          this.isError = true;
          return;
        }
      }
    }

    if (this.mode == Operation.Create) {
      this.create();
    }
    else {
      this.update();
    }
  }

  private hasHebrew(username: string): boolean {
    let hebrewChars = new RegExp("[\u0590-\u05FF]");
    if (hebrewChars.test(username))
      return true;
    return false;
  }
  public create() {

    if (this.selectedUserType != 4 && this.addUserModel.GroupId == null) {
      this.errorMessage = "Plaese select Group";
      this.isError = true;
      return;
    }
    if (this.selectedUserType != 4 && this.addUserModel.CompanyId == null) {
      this.errorMessage = "Please select Company";
      this.isError = true;
      return;
    }
    this.addUserModel.UserType = this.selectedUserType;
    this.addUserModel.userEmail = this.userEmail;
    this.addUserModel.userUsername = this.userUsername;
    this.addUserModel.userName = this.userName;
    this.isBusy = true;
    this.usersApiService.create(this.addUserModel, this.sysPassword).subscribe(
      () => {
        this.isBusy = false;
        this.addUserModel = new AddUser();
        this.selectedUserType = 2
        this.isSystemAdmin = false;
        this.hideUserForm.emit(1);
      },
      (err) => {
        this.isBusy = false;
        let errors = new Errors(err.error);
        let passMinLength = this.sharedService.getMinimumPasswordLengthFromError("Password", errors);
        if (passMinLength == null) {
          this.errorMessage = this.sharedService.getErrorMessage(errors);
          this.isError = true;
        }
        else {
          this.passMinLengthMsg = `Password should contain at least one digit, one special character and at least ${passMinLength} characters long`;
        }
      });

  }

  update() {
    let updateUserModel = new UpdateUserModel();
    updateUserModel.Name = this.userName;
    updateUserModel.Email = this.userEmail;
    updateUserModel.Username = this.userUsername;
    updateUserModel.UserType = this.selectedUserType;
    this.isBusy = true;
    this.usersApiService.update(this.user.id, updateUserModel).subscribe(
      () => {
        this.isBusy = false;
        this.addUserModel = new AddUser();
        this.selectedUserType = 2
        this.isSystemAdmin = false;
        this.hideUserForm.emit(1);
      },
      (err) => {
        this.isBusy = false;
        this.errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
        this.isError = true;
      });
  }

  UpdateFormUI(mode: Operation, user: User) {
    this.mode = mode;
    this.user = user;
    this.submitText = Operation[mode];
    
    this.userEmail = mode == Operation.Update ? user.email : "";
    this.userUsername = mode == Operation.Update ? user.username : "";
    this.userName = mode == Operation.Update ? user.name : "";
    this.selectedUserType = mode == Operation.Update ? user.type : 2;
    this.isSystemAdmin = user?.type == UserType.SystemAdmin;
  }
}
