using Common.Models.License;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DAL.DAOs.Programs
{
    [Table("ProgramsUIView")]
    public class ProgramUIViewDAO
    {
        [Key]
        public Guid ProgramId { get; set; }
        public bool ShouldShowSelfSign { get; set; }
        public bool ShouldShowGroupSign { get; set; }
        public bool ShouldShowLiveMode { get; set; }
        public bool ShouldShowContacts { get; set; }
        public bool ShouldShowTemplates { get; set; }
        public bool ShouldShowDocuments { get; set; }
        public bool ShouldShowProfile { get; set; }
        public bool ShouldShowUploadAndsign { get; set; }
        public bool ShouldShowAddNewTemplate { get; set; }
        public bool ShouldShowDistribution { get; set; }
        
        public bool ShouldShowEditTextField { get; set; }
        public bool ShouldShowEditSignatureField { get; set; }
        public bool ShouldShowEditEmailField { get; set; }
        public bool ShouldShowEditPhoneField { get; set; }
        public bool ShouldShowEditDateField { get; set; }
        public bool ShouldShowEditNumberField { get; set; }
        public bool ShouldShowEditListField { get; set; }
        public bool ShouldShowEditCheckboxField { get; set; }
        public bool ShouldShowEditRadioField { get; set; }
        public bool ShouldShowMultilineText { get; set; }
        public virtual ProgramDAO Program { get; set; }

        public ProgramUIViewDAO()
        {
            ShouldShowContacts = true;
            ShouldShowGroupSign = true;
            ShouldShowLiveMode = true;
            ShouldShowSelfSign = true;
            ShouldShowTemplates = true;
            ShouldShowDocuments = true;
            ShouldShowProfile = true;
            ShouldShowUploadAndsign = true;
            ShouldShowAddNewTemplate = true;
            ShouldShowEditTextField = true;
            ShouldShowEditSignatureField = true;
            ShouldShowEditEmailField = true;
            ShouldShowEditPhoneField = true;
            ShouldShowEditDateField = true;
            ShouldShowEditNumberField = true;
            ShouldShowEditListField = true;
            ShouldShowEditCheckboxField = true;
            ShouldShowEditRadioField = true;
            ShouldShowDistribution = false;
            ShouldShowMultilineText = true;
        }

        public ProgramUIViewDAO(UIViewLicense uiViewLicense)
        {
            ShouldShowContacts = uiViewLicense?.ShouldShowContacts ?? true;
            ShouldShowGroupSign = uiViewLicense?.ShouldShowGroupSign ?? true;
            ShouldShowLiveMode = uiViewLicense?.ShouldShowLiveMode ?? true;
            ShouldShowSelfSign = uiViewLicense?.ShouldShowSelfSign ?? true;
            ShouldShowTemplates = uiViewLicense?.ShouldShowTemplates ?? true;
            ShouldShowDocuments = uiViewLicense?.ShouldShowDocuments ?? true;
            ShouldShowProfile = uiViewLicense?.ShouldShowProfile ?? true;
            ShouldShowUploadAndsign = uiViewLicense?.ShouldShowUploadAndsign ?? true;
            ShouldShowAddNewTemplate = uiViewLicense?.ShouldShowAddNewTemplate ?? true;
            ShouldShowEditTextField = uiViewLicense?.ShouldShowEditTextField ?? true;
            ShouldShowEditSignatureField = uiViewLicense?.ShouldShowEditSignatureField ?? true;
            ShouldShowEditEmailField = uiViewLicense?.ShouldShowEditEmailField ?? true;
            ShouldShowEditPhoneField = uiViewLicense?.ShouldShowEditPhoneField ?? true;
            ShouldShowEditDateField = uiViewLicense?.ShouldShowEditDateField ?? true;
            ShouldShowEditNumberField = uiViewLicense?.ShouldShowEditNumberField ?? true;
            ShouldShowEditListField = uiViewLicense?.ShouldShowEditListField ?? true;
            ShouldShowEditCheckboxField = uiViewLicense?.ShouldShowEditCheckboxField ?? true;
            ShouldShowEditRadioField = uiViewLicense?.ShouldShowEditRadioField ?? true;
            ShouldShowDistribution = uiViewLicense?.ShouldShowDistribution ?? false;
            ShouldShowMultilineText = uiViewLicense?.ShouldShowMultilineText ?? false;
        }


    }
}
