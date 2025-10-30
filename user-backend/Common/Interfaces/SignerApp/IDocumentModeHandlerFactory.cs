using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.SignerApp
{
    public interface IDocumentModeHandlerFactory
    {
        IDocumentModeAction Create();
    }
}
