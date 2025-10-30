import { BaseResult } from '@models/base/base-result.model';
import { UserRequest } from './create-user-request.model';
import { User } from '@models/account/user.model';

export class UsersResult extends BaseResult{
    public users:User[];
}




