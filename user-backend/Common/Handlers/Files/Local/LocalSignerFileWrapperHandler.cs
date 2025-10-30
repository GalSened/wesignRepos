using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;


namespace Common.Handlers.Files.Local
{
    public class LocalSignerFileWrapperHandler : ISignerFileWrapper
    {
        private readonly FolderSettings _folderSettings;
        private readonly IFileSystem _fileSystem;
        private readonly GeneralSettings _generalSettings;
        private readonly IDataUriScheme _dataUriScheme;
        public LocalSignerFileWrapperHandler(IOptions<FolderSettings> folderSettings, IFileSystem fileSystem,
            IOptions<GeneralSettings> generalSettings, IDataUriScheme dataUriScheme)
        {
            _fileSystem = fileSystem;
            _folderSettings = folderSettings.Value;
            _generalSettings = generalSettings.Value;
            _dataUriScheme = dataUriScheme;

        }

        public void CreateSharedAppendices(DocumentCollection documentCollection, Appendix appendix)
        {
            ValidateAppendix(appendix, out FileType fileType);
            var bytes = Convert.FromBase64String(_dataUriScheme.Getbase64Content(appendix.Base64File));
            string folderPath = _fileSystem.Path.Combine(_folderSettings?.Appendices, documentCollection.Id.ToString());

            string path = _fileSystem.Path.Combine(folderPath, $"{appendix.Name}.{fileType.ToString().ToLower()}");
            SaveAppndix(bytes, folderPath, path);
        }

        public void CreateSignerAppendices(Guid documentCollectionId, Signer signer, Appendix appendix)
        {
            ValidateAppendix(appendix, out FileType fileType);
            var bytes = Convert.FromBase64String(_dataUriScheme.Getbase64Content(appendix.Base64File));
            string folderPath = _fileSystem.Path.Combine(_folderSettings.Appendices, documentCollectionId.ToString(), signer.Id.ToString());

            string path = _fileSystem.Path.Combine(folderPath, $"{appendix.Name}.{fileType.ToString().ToLower()}");
            SaveAppndix(bytes, folderPath, path);
        }



        public byte[] GetSmartCardDesktopClientInstaller()
        {
            string installerPath = _generalSettings.SmartCardDesktopClientInstallerPath;
            if (!_fileSystem.File.Exists(installerPath))
            {
                throw new Exception($"SmartCardDesktopClient installer not found at [{installerPath}]");
            }
            return _fileSystem.File.ReadAllBytes(installerPath);
        }

        public Dictionary<string, (FileType, byte[])> ReadAttachments(Signer dbSigner)
        {
            Dictionary<string, (FileType, byte[])> result = new Dictionary<string, (FileType, byte[])>();
            string folderPath = _fileSystem.Path.Combine(_folderSettings.Attachments, dbSigner.Id.ToString());
            if (_fileSystem.Directory.Exists(folderPath) && _fileSystem.Directory.EnumerateFiles(folderPath).Any())
            {

                foreach (var item in _fileSystem.Directory.EnumerateFiles(folderPath))
                {
                    string fileName = _fileSystem.Path.GetFileNameWithoutExtension(item);
                    string extension = _fileSystem.Path.GetExtension(item).Replace(".", "");

                    if (dbSigner.SignerAttachments.FirstOrDefault(x => x.Id.ToString().ToLower() == fileName.ToString().ToLower()) != null &&
                        !string.IsNullOrWhiteSpace(extension) && Enum.TryParse(extension, true, out FileType fileType))
                    {
                        string currentItemName = GetUniqueItemName(fileName, dbSigner, result);
                        result.Add(currentItemName, (fileType, _fileSystem.File.ReadAllBytes(item)));

                    }

                }

            }
            return result;
        }
        public bool IsAttachmentExist(Signer signer)
        {
            return _fileSystem.Directory.Exists(_fileSystem.Path.Combine(_folderSettings.Attachments, signer.Id.ToString()));
        }
        public IEnumerable<Appendix> ReadSharedAppendices(Guid documentCollectionId)
        {
            string documentAppendicesFolder = _fileSystem.Path.Combine(_folderSettings.Appendices, documentCollectionId.ToString());
            return ReadAppendicesFromFolderFiles(documentAppendicesFolder);
        }

        public IEnumerable<Appendix> ReadSignerAppendices(Guid documentCollectionId, Guid signerId)
        {
            string signerAppendicesFolder = _fileSystem.Path.Combine(_folderSettings.Appendices, documentCollectionId.ToString(), signerId.ToString());
            return ReadAppendicesFromFolderFiles(signerAppendicesFolder);
        }

        public IEnumerable<Attachment> ReadSharedAppendicesAsAttachment(Guid documentCollectionId)
        {
            string documentAppendicesFolder = _fileSystem.Path.Combine(_folderSettings.Appendices, documentCollectionId.ToString());

            return ReadAttachmentsFromFolderFiles(documentAppendicesFolder);
        }

        public IEnumerable<Attachment> ReadSignerAppendicesAsAttachment(Guid documentCollectionId, Guid signerId)
        {
            string signerAppendicesFolder = _fileSystem.Path.Combine(_folderSettings.Appendices, documentCollectionId.ToString(), signerId.ToString());
            return ReadAttachmentsFromFolderFiles(signerAppendicesFolder);
        }

        public IEnumerable<Attachment> ReadSignerAttachments(Signer signer)
        {
            string folderPath = _fileSystem.Path.Combine(_folderSettings.Attachments, signer.Id.ToString());
            return ReadAttachmentsFromFolderFiles(folderPath);
        }

        public void SaveSignerAttachment(Signer signer, SignerAttachment signerAttachment)
        {
            if (signerAttachment == null || signer == null || string.IsNullOrWhiteSpace(signerAttachment.Base64File))
            {
                return;
            }
            string folderPath = _fileSystem.Path.Combine(_folderSettings.Attachments, signer.Id.ToString());
            _fileSystem.Directory.CreateDirectory(folderPath);
            bool isValid = _dataUriScheme.IsValidFileType(signerAttachment.Base64File, out FileType fileType);
            if (!isValid)
            {
                throw new InvalidOperationException(ResultCode.InvalidFileType.GetNumericString());
            }
            string filePath = _fileSystem.Path.Combine(folderPath, $"{signerAttachment.Id}.{fileType.ToString().ToLower()}");
            _fileSystem.File.WriteAllBytes(filePath, _dataUriScheme.GetBytes(signerAttachment.Base64File));
        }

        private void SaveAppndix(byte[] bytes, string folderPath, string path)
        {
            _fileSystem.Directory.CreateDirectory(folderPath);
            _fileSystem.File.WriteAllBytes(path, bytes);
        }


        private void ValidateAppendix(Appendix appendix, out FileType fileType)
        {
            if (!_dataUriScheme.IsValidFileType(appendix.Base64File, out fileType))
            {
                throw new InvalidOperationException(ResultCode.InvalidFileType.GetNumericString());
            }
            if (string.IsNullOrEmpty(appendix.Name))
            {
                throw new InvalidOperationException(ResultCode.FieldNameNotExist.GetNumericString());
            }
        }


        private IEnumerable<Attachment> ReadAttachmentsFromFolderFiles(string folder)
        {
            List<Attachment> result = new List<Attachment>();
            if (_fileSystem.Directory.Exists(folder))
            {
                var filesPaths = _fileSystem.Directory.EnumerateFiles(folder);
                foreach (var filePath in filesPaths ?? Enumerable.Empty<string>())
                {
                    
                    ContentType contentType = new ContentType
                    {
                        MediaType = MimeTypes.Auto(filePath),
                        Name = _fileSystem.Path.GetFileName(filePath),
                    };
                    


                    byte[] attachmentBytes = _fileSystem.File.ReadAllBytes(filePath);
                    Stream stream = new MemoryStream(attachmentBytes);
                    var attachment = new Attachment(stream, contentType);
                    result.Add(attachment);
                }
            }

            return result;
        }

       

        private IEnumerable<Appendix> ReadAppendicesFromFolderFiles(string folder)
        {
            List<Appendix> result = new List<Appendix>();
            if (_fileSystem.Directory.Exists(folder))
            {
                var filesPaths = _fileSystem.Directory.EnumerateFiles(folder);
                foreach (var filePath in filesPaths ?? Enumerable.Empty<string>())
                {
                    var appendix = new Appendix
                    {
                        Name = _fileSystem.Path.GetFileNameWithoutExtension(filePath),
                        FileExtention = _fileSystem.Path.GetExtension(filePath).ToLower(),
                        FileContent = _fileSystem.File.ReadAllBytes(filePath)
                    };
                    result.Add(appendix);
                }
            }

            return result;
        }

        private string GetUniqueItemName(string fileName, Signer dbSigner, Dictionary<string, (FileType, byte[])> result)
        {
            var attchment = dbSigner.SignerAttachments.FirstOrDefault(x => x.Id.ToString().ToLower() == fileName.ToString());
            string attchmentName = ReplaceInvalidChars(attchment.Name);
            string selectedName = attchmentName;
            bool unique = false;
            int index = 1;
            do
            {
                if (!result.ContainsKey(selectedName))
                {
                    unique = true;
                }
                else
                {
                    selectedName = $"{attchmentName}_{index++}";
                }

            } while (!unique);


            return selectedName;
        }


        private string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(_fileSystem.Path.GetInvalidFileNameChars()));
        }

        
    }
}
