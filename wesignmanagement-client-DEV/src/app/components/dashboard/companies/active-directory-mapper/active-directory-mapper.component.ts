import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { ActiveDirectoryService } from 'src/app/services/active-directory.service';
import { Company } from 'src/app/models/company.model';
import { GroupADMapper } from 'src/app/models/group-ad-mapper';

@Component({
  selector: 'app-active-directory-mapper',
  templateUrl: './active-directory-mapper.component.html',
  styleUrls: ['./active-directory-mapper.component.css']
})
export class ActiveDirectoryMapperComponent implements OnInit {

  @Output() public hideGroupForm = new EventEmitter<number>();
  dataModel  = [];
  groups  = [];
  isError :boolean = false;
  errorMessage:string = "";
  @Input() company :Company;
  @Input() selectedgroupGroup:string = "";
  @Input() selectedADUser:string = "";
  @Input() selectedADContacts:string = "";
  

  constructor(private activeDirectoryService:ActiveDirectoryService) { }

  back()
  {
    this.isError = false;
    this.hideGroupForm.emit(1);
  }
  ngOnInit(): void {
    this.activeDirectoryService.ReadAddADGroups().subscribe(
      (data) => {
        this.dataModel =  data.activeDirectoryGroups;
        this.company.groups.forEach(x=> this.groups.push(x[0]));
      }
    );
  }



  public CollectionToOptions(companyAdminUsers: [string, string][]) {
    return companyAdminUsers.reduce((prev, curr) => {
      if( curr["item2"] == undefined)
      {
        prev[curr[0]] = curr[0];
      }
      else
      {
        prev[curr["item2"]] = curr["item2"];
      }
      return prev;
    }, {});
  }

  public dropDownUpdate(newValue) {
    this.isError = false;
      this.selectedgroupGroup = newValue;

  }

  update(){
    this.isError = false;
    if((this.selectedgroupGroup == "") ){
      this.errorMessage = "Group is mandatory field";
      this.isError = true;
      return;
    }

    let groupExist = false
    this.company.groupsADMapper.forEach(x => {if(x.groupName ==this.selectedgroupGroup)
    {
      groupExist = true;
    } });

  if(groupExist){
    this.errorMessage = `Group ${this.selectedgroupGroup} allready Mapped`;
    this.isError = true;
    return;
  }

this.company.groupsADMapper.forEach(x => {if(x.activeDirectoryUsersGroupName ==this.selectedADContacts)
  {    
    groupExist = true;
  } });

  if(groupExist){
    this.errorMessage = `Contact Active Directory ${this.selectedADContacts} allready Mapped`;
    this.isError = true;
    return;
  }


  


let newMapper = new GroupADMapper();
newMapper.activeDirectoryContactsGroupName = this.selectedADContacts;
newMapper.groupName = this.selectedgroupGroup;
newMapper.activeDirectoryUsersGroupName = this.selectedADUser;
this.company.groupsADMapper.push(newMapper);
this.selectedgroupGroup = "";
this.selectedADUser = "";
this.selectedADContacts = "";

this.back();

  }


}
