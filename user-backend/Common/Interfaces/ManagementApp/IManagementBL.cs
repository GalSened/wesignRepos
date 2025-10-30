namespace Common.Interfaces.ManagementApp
{
    public interface IManagementBL
    {
        ILogs Logs { get; }
        IPrograms Programs { get;}
        IUsers Users { get; }
        IAppConfigurations AppConfigurations { get; }
        ICompanies Companies { get; }
        IOTP OTP { get; }
        IActiveDirectory ActiveDirectory { get; }
        IPayment Payment { get; }
        IReport Reports { get; }
    }
}
