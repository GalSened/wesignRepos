import { OtpMode } from '@models/enums/otp-mode.enum';
import { Notes } from './notes.model';
import { ReplaceSignerRequest } from './replace-signer-request';
import { AuthMode } from '@models/enums/auth-mode.enum';

export class ReplaceSignerWithDetailsRequest extends ReplaceSignerRequest {
    public newNotes: Notes;
    public newOtpMode: OtpMode;
    public newOtpIdentification: string;
    public newAuthenticationMode: AuthMode;
}