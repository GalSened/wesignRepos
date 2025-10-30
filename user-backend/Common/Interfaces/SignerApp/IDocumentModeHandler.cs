using Common.Enums.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces.SignerApp
{
    public interface IDocumentModeHandler
    {
        IDocumentModeAction ExecuteCreation(DocumentMode documentMode);
    }
}

