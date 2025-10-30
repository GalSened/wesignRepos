import { AuthMode } from '@models/enums/auth-mode.enum';
import { OtpDetails } from './otp-details.model';

export class SignerAuthentication {
    otpDetails: OtpDetails;
    authenticationMode: AuthMode;
}