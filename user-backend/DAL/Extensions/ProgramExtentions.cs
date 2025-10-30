namespace DAL.Extensions
{
    using Common.Models;
    using Common.Models.License;
    using DAL.DAOs.Programs;

    public static class ProgramExtentions
    {
        public static Program ToProgram(this ProgramDAO programDAO)
        {
            return programDAO == null ? null : new Program()
            {
                Id = programDAO.Id,
                Name = programDAO.Name,
                Users = programDAO.Users,
                DocumentsPerMonth = programDAO.DocumentsPerMonth,
                Templates = programDAO.Templates,
                SmsPerMonth = programDAO.SmsPerMonth,
                VisualIdentificationsPerMonth = programDAO.VisualIdentificationsPerMonth,
                VideoConferencePerMonth = programDAO.VideoConferencePerMonth,
                SmartCard = programDAO.SmartCard,
                ServerSignature = programDAO.ServerSignature,
                Note = programDAO.Note,
                UIViewLicense = programDAO.ProgramUIView.ToUIViewLicense()            
            };
        }

        public static UIViewLicense ToUIViewLicense(this ProgramUIViewDAO programUIViewDAO)
        {
            return programUIViewDAO == null ? null : new UIViewLicense()
            {
                ShouldShowAddNewTemplate = programUIViewDAO.ShouldShowAddNewTemplate,
                ShouldShowContacts = programUIViewDAO.ShouldShowContacts,
                ShouldShowEditCheckboxField = programUIViewDAO.ShouldShowEditCheckboxField,
                ShouldShowEditDateField = programUIViewDAO.ShouldShowEditDateField,
                ShouldShowEditEmailField = programUIViewDAO.ShouldShowEditEmailField,
                ShouldShowEditListField = programUIViewDAO.ShouldShowEditListField,
                ShouldShowEditNumberField = programUIViewDAO.ShouldShowEditNumberField,
                ShouldShowEditPhoneField = programUIViewDAO.ShouldShowEditPhoneField,
                ShouldShowEditRadioField = programUIViewDAO.ShouldShowEditRadioField,
                ShouldShowEditSignatureField = programUIViewDAO.ShouldShowEditSignatureField,
                ShouldShowEditTextField = programUIViewDAO.ShouldShowEditTextField,
                ShouldShowGroupSign = programUIViewDAO.ShouldShowGroupSign,
                ShouldShowLiveMode = programUIViewDAO.ShouldShowLiveMode,
                ShouldShowSelfSign = programUIViewDAO.ShouldShowSelfSign,
                ShouldShowTemplates = programUIViewDAO.ShouldShowTemplates,
                ShouldShowDocuments = programUIViewDAO.ShouldShowDocuments,
                ShouldShowProfile = programUIViewDAO.ShouldShowProfile,
                ShouldShowUploadAndsign = programUIViewDAO.ShouldShowUploadAndsign,
                ShouldShowDistribution = programUIViewDAO.ShouldShowDistribution,
                ShouldShowMultilineText = programUIViewDAO.ShouldShowMultilineText
            };
        }
    }
}
