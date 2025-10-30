import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { managmentGroupRequest } from '@models/managment/groups/management-group-request.model';
import { managmentGroupResponse } from '@models/managment/groups/managment-group-response.model';
import { groupsResponse } from '@models/managment/groups/management-groups.model';
import { UserFilter } from '@models/managment/users/user-filter';
import { UserRequest } from '@models/managment/users/create-user-request.model';
import { UsersIdResult } from '@models/managment/users/user-id-response.model';
import { BaseResult } from '@models/base/base-result.model';
import { User } from '@models/account/user.model';
import { AppConfigService } from './app-config.service';
import { UsersResult } from '@models/managment/users/users-response';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  
   private groupsApi : string = "";
   private usersApi : string = "";

  constructor(private httpClient: HttpClient,
              private appConfigService: AppConfigService) {
              this.groupsApi = this.appConfigService.apiUrl + "/admins/groups";
              this.usersApi = this.appConfigService.apiUrl + "/admins/users";
 }

 public getAllGroups(){
   return this.httpClient.get<groupsResponse>(`${this.groupsApi}`);
 }

 public createNewGroup(grouprequest: managmentGroupRequest) {
    return this.httpClient.post<managmentGroupResponse>(`${this.groupsApi}`, grouprequest);
}

public updateGroup(grouprequest: managmentGroupRequest,groupId : string ){
  return this.httpClient.put<BaseResult>(`${this.groupsApi}/${groupId}`,grouprequest) ;
}

public deleteGroup(groupId : string ){
  return this.httpClient.delete<groupsResponse>(`${this.groupsApi}/${groupId}`) ;
}

public getUsers(userFilter: UserFilter) {
  return this.httpClient.get<UsersResult>(`${this.usersApi}?key=${userFilter.key}&` +
      `offset=${userFilter.offset}&limit=${userFilter.limit}`, { observe: "response" });
}

public createNewUser(newUser: UserRequest) {
  return this.httpClient.post<UsersIdResult>(`${this.usersApi}`, newUser);
}

public updateUser(userDetails: User) {
  return this.httpClient.put<BaseResult>(`${this.usersApi}/${userDetails.id}`, userDetails);
}
public deleteUser(userId) {
  return this.httpClient.delete<BaseResult>(`${this.usersApi}/${userId}`);
}

}
