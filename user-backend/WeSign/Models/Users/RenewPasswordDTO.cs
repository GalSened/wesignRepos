namespace WeSign.Models.Users
{
    using System;

    public class RenewPasswordDTO
    {
        public string RenewPasswordToken { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}
