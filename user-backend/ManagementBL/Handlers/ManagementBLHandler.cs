using Common.Interfaces;
using Common.Interfaces.ManagementApp;

namespace ManagementBL.Handlers
{
    public class ManagementBLHandler : IManagementBL
    {
        public ILogs Logs { get; }

        public IPrograms Programs { get; }

        public Common.Interfaces.ManagementApp.IUsers Users { get; }

        public IAppConfigurations AppConfigurations { get; }
        
        public ICompanies Companies { get; }

        public IOTP OTP { get; }

        public Common.Interfaces.IActiveDirectory ActiveDirectory { get; }

        public IPayment Payment { get; }
        
        public IReport Reports{ get; }

        public ManagementBLHandler(ILogs logs, IPrograms programs, Common.Interfaces.ManagementApp.IUsers users, IAppConfigurations appConfigurations, ICompanies companies, IOTP otp, Common.Interfaces.IActiveDirectory activeDirectory, IPayment payment, IReport reports)
        {
            Logs = logs;
            Programs = programs;
            Users = users;
            AppConfigurations = appConfigurations;
            Companies = companies;
            OTP = otp;
            ActiveDirectory = activeDirectory;
            Payment = payment;
            Reports = reports;
        }
    }
}
