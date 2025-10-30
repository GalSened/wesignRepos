using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;


using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Common.Handlers.Files.Local
{
    public class LocalContactFileWrapperHandler : IContactFileWrapper
    {
        private readonly FolderSettings _folderSettings;
        private readonly IFileSystem _fileSystem;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly ILogger _logger;
 

        private readonly int NUMBER_OF_SIGNATURES = 6;

        public LocalContactFileWrapperHandler(IOptions<FolderSettings> folderSettings, IFileSystem fileSystem,
           ILogger logger , IDataUriScheme dataUriScheme)
        {
            _folderSettings = folderSettings.Value;
            _fileSystem = fileSystem;
            _dataUriScheme = dataUriScheme;
            _logger = logger;
        }

        public void DeleteSeals(Contact contact)
        {
            if(contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }
            string contactSealsFolder = _fileSystem.Path.Combine(_folderSettings.ContactSeals,
                                                                 contact.UserId.ToString(),
                                                                 contact.Id.ToString());
            if (_fileSystem.Directory.Exists(contactSealsFolder))
            {
                _fileSystem.Directory.Delete(contactSealsFolder, recursive: true);
            }
        }

        public void SaveSeals(Contact contact)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_folderSettings.ContactSeals, contact.UserId.ToString(), contact.Id.ToString()));
            foreach (var image in contact.Seals ?? Enumerable.Empty<Seal>())
            {
                string filePath = GetImagePath(contact, image);
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    byte[] content = _dataUriScheme.GetBytes(image.Base64Image);
                    _fileSystem.File.WriteAllBytes(filePath, content);
                }
            }            
        }


        public void SaveCertificate(Contact contact, byte[] cert)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }
            var certPath = _fileSystem.Path.Combine(_folderSettings.ContactCertificates, $"{contact.Id}.pfx");
            _fileSystem.File.WriteAllBytes(certPath, cert);
        }

        public void SetSealsData(Contact contact)
        {
            if (contact == null)
            {
                return ;
            }
            foreach (var image in contact.Seals ?? Enumerable.Empty<Seal>())
            {

                string filePath = GetImagePath(contact, image);
                if (string.IsNullOrWhiteSpace(filePath) || !_fileSystem.File.Exists(filePath))
                {
                    _logger.Error("Image does not exist, cannot get content from image [{FilePath}]", filePath);
                    continue;
                }
                var base64Content = Convert.ToBase64String(_fileSystem.File.ReadAllBytes(filePath));
                image.Base64Image = $"{image.Base64Image}{base64Content}";
            }
        }


        public List<string> ReadSeals(Contact contact)
        {
            if(contact == null)
            {
                return null;
            }
            var result = new List<string>();
            string folderPath = _fileSystem.Path.Combine(_folderSettings.ContactSignatureImages, contact.Id.ToString());
            if (_fileSystem.Directory.Exists(folderPath))
            {
                var images = _fileSystem.Directory.GetFiles(folderPath).Where(x => x.ToLower().EndsWith(".jpeg") || x.ToLower().EndsWith(".jpg") || x.ToLower().EndsWith(".png") || x.ToLower().EndsWith(".jpe"));


                foreach (var image in images)
                {
                    var imageFromDirectory = Convert.ToBase64String(_fileSystem.File.ReadAllBytes(image));
                    if (!string.IsNullOrWhiteSpace(imageFromDirectory))
                    {
                        result.Add($"data:image/{_fileSystem.Path.GetExtension(image).Replace(".", "")};base64,{Convert.ToBase64String(_fileSystem.File.ReadAllBytes(image))}");
                    }
                }
            }
            if (result.Count > NUMBER_OF_SIGNATURES)
            {
                result = result.Take(NUMBER_OF_SIGNATURES).ToList();
            }
            return result;
        }

        public void UpdateSeals(Contact contact, List<string> signaturesImages)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }

            string folderPath = _fileSystem.Path.Combine(_folderSettings.ContactSignatureImages, contact.Id.ToString());
            if (!_fileSystem.Directory.Exists(folderPath))
            {
                _fileSystem.Directory.CreateDirectory(folderPath);
            }

            foreach (var signatureImage in signaturesImages)
            {
                if (_dataUriScheme.IsValidSignatureImageType(signatureImage, out SignatureImageType imageType))
                {
                    var bytes = _dataUriScheme.GetBytes(signatureImage);
                    if (imageType == SignatureImageType.GIF || imageType == SignatureImageType.BMP)
                    {

                        bytes = ConvertToPNG(bytes);
                        imageType = SignatureImageType.PNG;

                    }
                    // save only 6 

                    var fileList = _fileSystem.Directory.EnumerateFiles(folderPath).Where(x =>
                    x.ToLower().EndsWith(".jpeg") ||
                    x.ToLower().EndsWith(".png") ||
                    x.ToLower().EndsWith(".jpg") ||
                    x.ToLower().EndsWith(".gif") ||
                    x.ToLower().EndsWith(".bmp"));

                    string newFileName = _fileSystem.Path.Combine(folderPath, $"{Guid.NewGuid()}.{imageType}");
                    if (fileList.Any() )
                    {

                        TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

                        using (Bitmap bitmap1 = (Bitmap)tc.ConvertFrom(bytes))
                        {
                            FileSystemInfo fileInfo = null;
                            if (fileList.Count() >= NUMBER_OF_SIGNATURES)
                            {
                                fileInfo = new DirectoryInfo(folderPath).GetFileSystemInfos()
                            .OrderBy(fi => fi.CreationTime).First();
                            }
                            bool needToRewriteImage = true;
                            // need to check if the file exist..
                            foreach (var file in fileList)
                            {
                                using (var bitmap = Bitmap.FromFile(file))
                                {
                                    if (bitmap.Equals(bitmap1))
                                    {
                                        needToRewriteImage = false;
                                        newFileName = file;
                                        break;
                                    }
                                }

                            }

                            if (fileInfo != null && needToRewriteImage)
                            {
                                _fileSystem.File.Delete(fileInfo.FullName);
                                newFileName = _fileSystem.Path.Combine(folderPath, $"{Guid.NewGuid()}.{imageType}");
                            }

                        }

                    }

                    _fileSystem.File.WriteAllBytes(newFileName, bytes);
                }
                else
                {
                    throw new InvalidOperationException(ResultCode.NotSupportedImageFormat.GetNumericString());
                }


                var fileListafterSave = _fileSystem.Directory.EnumerateFiles(folderPath).Where(x =>
                    x.ToLower().EndsWith(".jpeg") ||
                    x.ToLower().EndsWith(".png") ||
                    x.ToLower().EndsWith(".jpg") ||
                    x.ToLower().EndsWith(".gif") ||
                    x.ToLower().EndsWith(".bmp"));

                if (fileListafterSave.Count() > NUMBER_OF_SIGNATURES)
                {
                    for (int i = fileListafterSave.Count(); i > NUMBER_OF_SIGNATURES; --i)
                    {
                        FileSystemInfo fileInfo = new DirectoryInfo(folderPath).GetFileSystemInfos()
                          .OrderBy(fi => fi.CreationTime).First();
                        _fileSystem.File.Delete(fileInfo.FullName);
                    }
                }

            }
        }

        public bool IsCertificateExist(Contact contact)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }

            var contactCertificate = _fileSystem.Path.Combine(_folderSettings.ContactCertificates, $"{contact.Id}.pfx");
            return _fileSystem.File.Exists(contactCertificate);
        }

        public byte[] ReadCertificate(Contact contact, CompanyConfiguration companyConfiguration)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }
            var certificatePath = _fileSystem.Path.Combine(_folderSettings.ContactCertificates, $"{contact.Id}.pfx");          
            return _fileSystem.File.ReadAllBytes(certificatePath);
        }

        public void DeleteCertificate(Contact contact)
        {
            if (contact == null)
            {
                throw new InvalidOperationException(ResultCode.InsufficientContactData.GetNumericString());
            }

            var contactCertificate = _fileSystem.Path.Combine(_folderSettings.ContactCertificates, $"{contact.Id}.pfx");
            if (_fileSystem.File.Exists(contactCertificate))
            {              
                _fileSystem.File.Delete(contactCertificate);
            }
        }

        private string GetImagePath(Contact contact, Seal image)
        {
            try
            {
                var dataType = image.Base64Image?.Split(new char[] { ',' }).FirstOrDefault();
                int length = dataType.IndexOf(';') - dataType.IndexOf('/') - 1;
                string imageType = dataType?.Substring(dataType.IndexOf('/') + 1, length);
                string imageName = image.Name.EndsWith($".{imageType}") ? image.Name : $"{image.Name}.{imageType}";
                return _fileSystem.Path.Combine(_folderSettings.ContactSeals, contact.UserId.ToString(), contact.Id.ToString(), imageName);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        private byte[] ConvertToPNG(byte[] image)
        {
            using (MemoryStream memstr = new MemoryStream(image))
            {
                using (var bmp = System.Drawing.Image.FromStream(memstr))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }

            }
        }

      
    }
}
