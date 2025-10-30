import { ApiError } from "./api-error.model";

export class Errors{
    public errors: ApiError;
    public errorCode: number = 0;

    constructor(errorObject?: Errors ){
        if(errorObject){
            this.errors = Object.assign(errorObject);
            if(this.errors.status){
                this.errorCode = this.errors.status;
            }
        }
    }
    
    public toString(): string{
        let _errors = "";
        Object.keys(this.errors.errors).forEach(
            x=>{
                _errors += x + ": "+ this.errors.errors[x].join(", ")+" ;\n";
            }
        )

        return _errors;
    }
}