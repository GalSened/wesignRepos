using Common.Models.FileGateScanner;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.FileGateScanner
{
    public interface IFileGateScannerProvider
    {
        FileGateScanResult Scan(FileGateScan base64string);
    }
}
