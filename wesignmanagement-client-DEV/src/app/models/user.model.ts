import { UserType } from '../enums/user-type.enum';

export class User{
    public id : string ="";
    public name : string ="";
    public email : string ="";
    public username : string ="";
    public creationTime : Date;
    public language : number = 0 ;
    public type : UserType ;
    public companyName : string ="" ;
    public groupName : string ="" ;
    public programName : string ="" ;
}