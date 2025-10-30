import { SplitSignProcessStep } from "src/app/enums/split-sign-process-step.enum";

export class SplitDocumentProcessResult {
    url: string;
    processStep: SplitSignProcessStep;
}