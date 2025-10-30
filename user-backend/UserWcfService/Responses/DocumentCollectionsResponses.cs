using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserSoapService.HttpClientLogic;

namespace UserSoapService.Responses
{
    public class GetDocumentCollectionsResponse : BaseResult
    {
        public AllDocumentCollectionsResposneDTO DocumentCollections { get; set; }
    }
    public class GetDocumentCollectionDataResponse : BaseResult
    {
        public DocumentCollectionResposneDTO DocumentCollectionInfo { get; set; }
    }
    public class DownloadDocumentCollectionResponse : BaseResult
    {
        public DownloadFileResponse DownloadResponse { get; set; }
    }
    public class DownloadDocumentCollectionAttchmentResponse : BaseResult
    {
        public DownloadFileResponse DownloadAttachmentResponse { get; set; }
    }
    public class DownloadDocumentCollectionTraceResponse : BaseResult
    {
        public object DownloadTraceResponse { get; set; }
    }
    public class GetDocumentCollectionPagesCountResponse : BaseResult
    {
        public DocumentCountResponseDTO DocumentCollectionCount { get; set; }
    }
    public class GetDocumentCollectionPagesInfoResponse : BaseResult
    {
        public DocumentPagesRangeResponseDTO DocumentCollectionPagesRange { get; set; }
    }
    public class CreateDocumentCollectionResponse : BaseResult
    {
        public CreateDocumentCollectionResposneDTO CreateDocumentCollection { get; set; }
    }

    public class ExportDocumentCollectionPdfFieldsResponse : BaseResult
    {
        public DownloadFileResponse FieldsResponse { get; set; }
    }

    public class ExportDocumentCollectionResponse : BaseResult
    {
        public DownloadFileResponse ExportResponse { get; set; }
    }

    public class ResendDocumentCollectionResponse : BaseResult
    {
        public SignerLink SignerLinkResponse { get; set; }
    }
}