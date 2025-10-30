import { BaseResult } from "@models/base/base-result.model";

export class ProgramStatus extends BaseResult {
    public Name: string;
    public Users: number;
    public Templates: number;
    public Documents: number;
    public SMS: number;
}
