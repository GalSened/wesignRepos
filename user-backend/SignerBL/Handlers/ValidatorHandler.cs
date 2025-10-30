using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.FileGateScanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class SignerValidatorHandler : Common.Interfaces.SignerApp.ISignerValidator
    {
        
        private readonly IJWT _jwt;
        private readonly IDocumentPdf _documentPdf;
        private readonly IFileGateScannerProviderFactory _fileGateScannerProviderHandler;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly IConfigurationConnector _configurationConnector;

        public SignerValidatorHandler(ISignerTokenMappingConnector signerTokenMappingConnector, IConfigurationConnector configurationConnector , IJWT jwt, IDocumentPdf documentPdf, IFileGateScannerProviderFactory fileGateScannerProviderHandler)
        {
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _configurationConnector = configurationConnector;
            _jwt = jwt;
            _documentPdf = documentPdf;
            _fileGateScannerProviderHandler = fileGateScannerProviderHandler;
        }

        public bool AreAllFieldsBelongToSigner(Signer dbSigner, Signer signer, DocumentCollection inputDocumentCollection)
        {
            Dictionary<Guid, List<Common.Models.Files.PDF.TextField>> memory = new Dictionary<Guid, List<Common.Models.Files.PDF.TextField>>();
            var docs = signer.SignerFields.Select(x => x.DocumentId);
            foreach (var doc in docs)
            {
                if (!memory.ContainsKey(doc))
                {
                    memory[doc] = _documentPdf.GetAllFields(false).TextFields;
                }
              
            }
            
            foreach (var signerField in signer?.SignerFields ?? Enumerable.Empty<SignerField>())
            {
                if (!IsFieldBelongToSigner(dbSigner, signerField, inputDocumentCollection , memory))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AreAllFieldsExistsInDocuments(DocumentCollection documentCollection)
        {
            foreach (var document in documentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                _documentPdf.Load(document.Id);
                var fields = _documentPdf.GetAllFields(false);
                var textFields = fields.TextFields;
                foreach (var field in document.Fields.TextFields)
                {
                    if (textFields.FirstOrDefault(x => x.Name == field.Name) == null)
                        return false;
                }
                var checkBoxFields = fields.CheckBoxFields;
                foreach (var field in document.Fields.CheckBoxFields)
                {
                    if (checkBoxFields.FirstOrDefault(x => x.Name == field.Name) == null)
                        return false;
                }
                var choiceFields = fields.ChoiceFields;
                foreach (var field in document.Fields.ChoiceFields)
                {
                    if (choiceFields.FirstOrDefault(x => x.Name == field.Name) == null)
                        return false;
                }

                var sigFields = fields.SignatureFields;
                foreach (var field in document.Fields.SignatureFields)
                {
                    if (sigFields.FirstOrDefault(x => x.Name == field.Name) == null)
                        return false;
                }

                var radioGroupFields = fields.RadioGroupFields;
                foreach (var field in document.Fields.RadioGroupFields)
                {
                    if (radioGroupFields.FirstOrDefault(x => x.Name == field.Name) == null)
                        return false;
                }
            }
            return true;
        }

        public bool AreAllMandatoryFieldsFilledIn(Signer dbSigner, Signer signer)
        {
            var mandatoryFields = dbSigner?.SignerFields?.Where(x => x.IsMandatory);
            //TODO Load mandatory attribute to fields
            foreach (var mandatoryField in mandatoryFields)
            {
                var signerField = signer.SignerFields?.FirstOrDefault(x => x.FieldName == mandatoryField.FieldName);
                if (signerField == null || string.IsNullOrWhiteSpace(signerField?.FieldValue))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AreDocumentsBelongToDocumentCollection(DocumentCollection dbDocumentCollection, DocumentCollection inputDocumentCollection)
        {
            foreach (var document in inputDocumentCollection?.Documents ?? Enumerable.Empty<Document>())
            {
                var doc = dbDocumentCollection.Documents.FirstOrDefault(x => x.Id == document.Id);
                if (doc == null)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<(Signer, Guid documentCollectionId)> ValidateSignerToken(SignerTokenMapping signerTokenMapping)
        {
            Guid documentCollectionId;
            var dbSignerTokenMapping =await _signerTokenMappingConnector.Read(signerTokenMapping);
            var signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);          
            documentCollectionId = dbSignerTokenMapping.DocumentCollectionId;
            if(string.IsNullOrWhiteSpace( signerTokenMapping.AuthToken) && !string.IsNullOrWhiteSpace(dbSignerTokenMapping.AuthToken) )
            {
                signerTokenMapping.AuthToken = dbSignerTokenMapping.AuthToken;
            }
            return (signer , documentCollectionId);
        }

        public bool AreAllSignersSigned(IEnumerable<Signer> signers)
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

        public async Task<FileGateScanResult> ValidateIsCleanFile(string base64string)
        {
            var fileGateScannerConfiguration = (await _configurationConnector.Read())?.FileGateScannerConfiguration;
            var providerType = fileGateScannerConfiguration?.Provider ?? FileGateScannerProviderType.None;
            var fileGateScannerProviderHandler = _fileGateScannerProviderHandler.ExecuteCreation(providerType);
            var fileGateScan = new FileGateScan { Base64 = base64string };
            var result = fileGateScannerProviderHandler.Scan(fileGateScan);
            if (!result?.IsValid ?? false)
            {
                throw new InvalidOperationException(ResultCode.InvalidFileContent.GetNumericString());
            }

            return result;
        }

        #region Private Functions

        private bool IsFieldBelongToSigner(Signer dbSigner, SignerField signerField, DocumentCollection inputDocumentCollection, Dictionary<Guid, List<Common.Models.Files.PDF.TextField>> memory)
        {
            bool result = false;
            //TODO: Need to replace where with firstOrDefault and fix memory to contains all type of fields 
            result = memory[signerField.DocumentId].Where(x => x.Name.ToLower() == signerField.FieldName || x.Description.ToLower() == signerField.FieldName) != null;

            if (!result)
            {
                result = dbSigner.SignerFields.FirstOrDefault(x => x.FieldName.ToLower() == signerField.FieldName.ToLower() || x.FieldName.ToLower() == signerField.FieldValue?.ToLower()) != null;
                if (!result)
                {
                    var group = inputDocumentCollection.Documents.FirstOrDefault(x => x.Id == signerField.DocumentId) // Verify not null
                                        .Fields?.RadioGroupFields.FirstOrDefault(x => x.Name.ToLower() == signerField.FieldName.ToLower());
                    result = group != null;
                }
            }

            return result; 
        }

        #endregion
    }
}