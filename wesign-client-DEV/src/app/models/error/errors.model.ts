import { ApiError } from '@models/error/error-message.model';

export class Errors {
    public errors: ApiError;
    public errorCode: number = 0;

    constructor(errorObject?: Errors) { // TODO - add 400 status error parsing from server
        if (errorObject) {
            this.errors = Object.assign(errorObject);
            if (this.errors.status) {
                this.errorCode = this.errors.status;
            }
        }
    }

    public toString(): string {
        let _errors = "";
        Object.keys(this.errors.errors).forEach(key => {
            _errors += key + ": " + this.errors.errors[key].join(", ") + " ;\n";
        });

        return _errors;
    }
    // public resultCode() {
    //     return this.errors.status;
    // }
}