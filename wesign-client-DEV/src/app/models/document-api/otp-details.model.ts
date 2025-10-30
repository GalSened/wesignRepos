import { OtpMode } from '@models/enums/otp-mode.enum';

export class OtpDetails {
    public identification: string;
    public code: string;
    public expirationTime: Date;
    public mode: OtpMode;
}