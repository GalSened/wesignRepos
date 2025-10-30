export class ApiError{
    public status : number;
    public errors : {[key:string] : string[]}
    public title : string = "";
}