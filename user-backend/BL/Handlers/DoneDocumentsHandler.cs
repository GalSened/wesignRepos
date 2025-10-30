using Comda.Authentication.Models;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BL.Handlers
{
    public class DoneDocumentsHandler : IDoneDocuments
    {
        private readonly IDater _dater;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IFilesWrapper _filesWrapper;

        public DoneDocumentsHandler( IDater dater, IDocumentCollectionConnector documentCollectionConnector,
            IFilesWrapper filesWrapper)
        {
            _dater = dater;
            _documentCollectionConnector = documentCollectionConnector;


            _filesWrapper = filesWrapper;
        }

        public async Task<string> DoneProcess(DocumentCollection dbDocumentCollection, Signer dbSigner)
        {
            string downloadLink = "";
            await UpdateSignedStatusInDB(dbDocumentCollection, dbSigner);
            
            return downloadLink;
        }
        public byte[] DownloadSmartCardDesktopClientInstaller()
        {
            return _filesWrapper.Signers.GetSmartCardDesktopClientInstaller();
            
        }

        private  Task UpdateSignedStatusInDB(DocumentCollection dbDcumentCollection, Signer dbSigner)
        {
            dbSigner.Status = SignerStatus.Signed;
            dbSigner.TimeSigned = _dater.UtcNow();
            if (AreAllSignersSigned(dbDcumentCollection.Signers))
            {
                dbDcumentCollection.DocumentStatus = DocumentStatus.Signed;
                dbDcumentCollection.SignedTime = _dater.UtcNow();
            }
            return _documentCollectionConnector.Update(dbDcumentCollection);
        }

        private bool AreAllSignersSigned(IEnumerable<Signer> signers)
        {
            foreach (var signer in signers)
            {
                if (signer?.Status != SignerStatus.Signed)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
