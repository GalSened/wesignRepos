using Common.Enums;

namespace Common.Interfaces.FileGateScanner
{
    public interface IFileGateScannerProviderFactory
    {
        IFileGateScannerProvider ExecuteCreation(FileGateScannerProviderType providerType);
    }
}
