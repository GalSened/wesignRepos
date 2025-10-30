using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Documents.SplitSignature;
using CTInterfaces;
using CTXmlSigner;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Common.Hubs
{
    public class SmartCardSigningProcessHandler : ISmartCardSigningProcess
    {
        private ILogger _logger;


        private readonly IDataUriScheme _dataUriScheme;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IDoneDocuments _doneDocuments;
        private readonly IPdfPackage _pdfService;
        private readonly IMemoryCache _memoryCache;
        private readonly IFilesWrapper _filesWrapper;
        private Action<string, SmartCardInput> _itemRemoveEvent = null;
        private readonly string memCachePrimer = "SmartCardSigning";

        public SmartCardSigningProcessHandler(ILogger logger, IDataUriScheme dataUriScheme, IDocumentCollectionConnector documentCollectionConnector,
            IDoneDocuments doneDocuments, IPdfPackage pdfService,
            IMemoryCache memoryCache, IFilesWrapper filesWrapper)
        {

            _logger = logger;

            _dataUriScheme = dataUriScheme;
            _documentCollectionConnector = documentCollectionConnector;
            _doneDocuments = doneDocuments;
            _pdfService = pdfService;
            _memoryCache = memoryCache;
            _filesWrapper = filesWrapper;
        }

        public void SetItemRemoveEvent(Action<string, SmartCardInput> itemRemoveEvent)
        {
            _itemRemoveEvent = itemRemoveEvent;
        }

        public void CreateGroup(string roomToken, SmartCardInput smartCardInput)
        {
            _logger.Debug("SmartCardSigningHub - **********************Create Group for UI and Desktop communication***********,{RoomToken}", roomToken);
            UpdateSmartCardInput(roomToken, smartCardInput);

        }

        public async Task<string> CloseSigningProcess(string roomToken)
        {
            var smartCardInput = GetSmartCardProcessInputByToken(roomToken);
            if (smartCardInput != null)
            {
                var signerTokenMapping = smartCardInput.SignerTokenMapping;
                var documentCollection = await _documentCollectionConnector.Read(new DocumentCollection { Id = signerTokenMapping.DocumentCollectionId });
                var signer = documentCollection.Signers.FirstOrDefault(x => x.Id == signerTokenMapping.SignerId);
                return await _doneDocuments.DoneProcess(documentCollection, signer);
            }
            return "";
        }

        public bool EmbadeSignatureInDoc(string roomToken, byte[] signedHash, string fieldName)
        {
            var smartCardInput = GetSmartCardProcessInputByToken(roomToken);
            if (smartCardInput != null)
            {
                var document = smartCardInput.Documents.FirstOrDefault();
                var signatureField = document.SignatureFields.FirstOrDefault(x => x.Name == fieldName);
                SetResponse response = _pdfService.SetSignature(signatureField.PrepareSignaturePdfResult, signedHash);
                if (response.Result.ToString() == CTResult.SUCCESS.ToString())
                {
                    _filesWrapper.Documents.SaveDocument(Enums.DocumentType.Document, document.Id, response.PDF);

                    document.SignatureFields.Remove(signatureField);
                    if (document.SignatureFields.Count == 0)
                    {
                        smartCardInput.Documents.Remove(document);
                    }
                    UpdateSmartCardInput(roomToken, smartCardInput);
                    return true;
                }
            }
            return false;
        }

        public void UpdateProcessInputByToken(string oldRoomToken, string newRoomToken)
        {
            UpdateSmartCardInput(newRoomToken, GetSmartCardProcessInputByToken(oldRoomToken));
            _memoryCache.Remove(oldRoomToken);
        }

        public SmartCardInput GetSmartCardProcessInputByToken(string roomToken)
        {
            return _memoryCache.Get<SmartCardInput>($"{memCachePrimer}_{roomToken}");
        }

        public void UpdateSmartCardInput(string roomId, SmartCardInput smartCardProcessInput)
        {
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions().
                SetSlidingExpiration(TimeSpan.FromMinutes(3)).
                RegisterPostEvictionCallback(ItemRemoved);


            _memoryCache.Set($"{memCachePrimer}_{roomId}", smartCardProcessInput, memoryCacheEntryOptions);


        }

        private void ItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (reason == EvictionReason.Expired)
            {
                var smartCardInput = (value as SmartCardInput);
                if (smartCardInput != null)
                {
                    if (_itemRemoveEvent != null)
                    {
                        var items = key.ToString().Split("_")[1];
                        if (items.Count() == 2)
                        {
                            _itemRemoveEvent.Invoke(items[1].ToString(), smartCardInput);
                        }
                    }
                }
            }
        }

        public List<SignatureFieldData> PrepareSignatureNextField(string roomToken)
        {
            var smartCardProcessInput = _memoryCache.Get<SmartCardInput>($"{memCachePrimer}_{roomToken}");
            if (smartCardProcessInput != null)
            {
                List<SignatureFieldData> signatureFields = GetNextSignatureField(smartCardProcessInput);
                if (signatureFields != null)
                {
                    PrepResponse res = PrepareSignatureForField(smartCardProcessInput.Documents.FirstOrDefault(), signatureFields);
                    signatureFields.FirstOrDefault().Hash = res.Hash;
                    signatureFields.FirstOrDefault().PrepareSignaturePdfResult = res.PDF;
                    UpdateSmartCardInput(roomToken, smartCardProcessInput);
                    return signatureFields;
                }
            }

            return null;
        }

        public PrepResponse PrepareSignatureForField(DocumentSplitSignatureDataProcessInput smartCardProcessInput, List<SignatureFieldData> signatureFields)
        {
            var base64image = _dataUriScheme.Getbase64Content(signatureFields.FirstOrDefault().Image);

            byte[] bytes = _filesWrapper.Documents.ReadDocument(Enums.DocumentType.Document, smartCardProcessInput.Id);
            var fieldNames = signatureFields.Select(x => x.Name).ToArray();
            PrepResponse res = _pdfService.PrepareSignatureForField(fieldNames, Convert.FromBase64String(base64image), bytes);

            if (res.Result.ToString() != CTResult.SUCCESS.ToString())
            {
                throw new Exception($"SmartCardSigningHub - Failed to PrepareSignatureForField - {res.Result}");
            }
            return res;
        }

        private List<SignatureFieldData> GetNextSignatureField(SmartCardInput smartCardInput)
        {
            var doc = smartCardInput.Documents.FirstOrDefault();

            if (doc != null)
            {
                return doc.SignatureFields;
            }
            return null;

        }


    }
}
