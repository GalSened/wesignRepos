
export class ApiError {
    //public key : string[] = [];
    //public errorMessage: string[] = [];
    public status: number;
    // public error: string[] = [];
    public errors: { [key: string]: string[] };
    public title: string = "";
}