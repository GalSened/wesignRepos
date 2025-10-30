namespace DAL.Extensions
{
    using Common.Models.Documents;
    using DAL.DAOs.Documents;

    public static class DocumentExtension
    {
        public static Document ToDocument(this DocumentDAO documentDAO)
        {
            return documentDAO == null ? null : new Document()
            {
                Id = documentDAO.Id,
                Name = documentDAO.Name,
                TemplateId = documentDAO.TemplateId
            };
        }
    }
}
