import { Injectable } from '@angular/core';
import { Errors } from '../models/error/errors.model';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class SharedService {

    constructor(private httpClient: HttpClient){

    }

    public static EnumToArrayHelper<E>(_enum: any): any {
        let values = Object.values(_enum).filter(value => isNaN(Number(value)) === true);
        let keys = Object.values(_enum).filter(value => isNaN(Number(value)) === false);
        return values.reduce((prev, curr, i) => {
            prev[keys[i] as string] = curr;
            return prev;
        }, {});
    }

    public getErrorMessage(result: Errors) {
        if (result.errorCode == 400 && result.errors?.status == 400) {
            return result?.errors?.title + " " + result.toString();
        }
        return result?.errors?.errors?.error?.toString();
    }

    public getToolTipsInfo() {
        return this.httpClient.get('./assets/tool-tip.json');
    }

    public getMinimumPasswordLengthFromError(keyToSearch: string, errors: Errors) {
        const pattern = /Password should contain at least one digit, one special character and at least (\d+) characters long/;
        if (errors.errors.status == 400) {
                let validationErrorValues = errors.errors.errors[keyToSearch];
                if (validationErrorValues != null) {
                    for (const message of validationErrorValues) {
                        const match = message.match(pattern);
                        if (match) {
                            return match[1];
                        }
                    }
                }
        }
        return null;
    }
}