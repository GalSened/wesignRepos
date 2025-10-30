// Ignore Spelling: debenu

namespace PdfHandler.Tests
{
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models.Settings;
    using LazyCache;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Moq;
    using PdfHandler.Interfaces;
    using PdfHandler.pdf;
    using System;

    public class PdfStub : Pdf
    {
        
        public PdfStub(IOptions<GeneralSettings> generalSettings, 
            IDebenuPdfLibrary debenu,
            IMemoryCache memoryCacheMock, IFilesWrapper fileWrapperMock ) 
            : base(generalSettings, debenu, null, null,  memoryCacheMock, fileWrapperMock, null)
        {

        }

        public new bool Load(byte[] file)
        {
            return base.IsLoaded(file,Guid.NewGuid());
        }
    }
}
