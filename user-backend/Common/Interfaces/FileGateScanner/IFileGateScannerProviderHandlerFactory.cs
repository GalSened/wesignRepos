using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.FileGateScanner
{
    public interface IFileGateScannerProviderHandlerFactory
    {
        IFileGateScannerProvider Create();
    }
}
