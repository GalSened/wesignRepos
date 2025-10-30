import { BaseResult } from "@models/base/base-result.model";
import { Contact } from "./contact.model";

export class ContactsResult extends BaseResult {
    public contacts: Contact[];
}
