import { BaseResult } from "@models/base/base-result.model";

export class ProgramInfo extends BaseResult {
    public Users: number;
    public Templates: number;
    public Documents: number;
    public SMS: number;
    public OnlineMode: boolean;
    public SmartCard: boolean;
    public ServerSignature: boolean;
}
