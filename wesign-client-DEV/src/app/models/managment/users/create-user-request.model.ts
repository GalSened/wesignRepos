import { userType } from '@models/enums/user-type.enum';

export class UserRequest { 

   public name:string;
   public email:string;
   public type:userType;
   public groupId:string;
   public additionalGroupsIds :string[];
}


