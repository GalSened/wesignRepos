export class AddUser{
    public userName : string = "";
    public userEmail : string = "";
    public userUsername : string = "";
    public CompanyId : string = null;
    public GroupId : string = null;
    public UserType : number  = 2;
}

export class UpdateUserModel{
    public UserType : number  = 2;
    public Name : string = "";
    public Email : string = "";
    public Username : string = "";
}