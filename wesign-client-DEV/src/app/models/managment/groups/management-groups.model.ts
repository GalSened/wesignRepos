import { BaseResult } from '@models/base/base-result.model';

export class groupsResponse extends BaseResult{
    public groups:group[];
}

export class group {
    public groupId:string;
    public name:string;
    constructor(){
        
    }
}