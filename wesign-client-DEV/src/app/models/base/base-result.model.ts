export function formatBaseResult(msg: string, result: BaseResult): string {

    if (result.ResultParams) {
        for (let index = 0; index < result.ResultParams.length; index ++) {
            msg = msg.replace(`{${index}}`, result.ResultParams[index]);
        }
    }
    return msg;
}

export class BaseResult {
    public ResultCode: number;
    public ResultMessage: string;
    public ResultParams: string[];
}
