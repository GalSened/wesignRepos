namespace Common.Models.License
{
    public class UIViewLicense
    {
        public bool ShouldShowSelfSign { get; set; }
        public bool ShouldShowGroupSign { get; set; }
        public bool ShouldShowLiveMode { get; set; }
        public bool ShouldShowContacts { get; set; }
        public bool ShouldShowTemplates { get; set; }
        public bool ShouldShowDocuments { get; set; }
        public bool ShouldShowProfile { get; set; }
        public bool ShouldShowUploadAndsign { get; set; }
        public bool ShouldShowAddNewTemplate { get; set; }
       
        public bool ShouldShowEditTextField { get; set; }
        public bool ShouldShowEditSignatureField { get; set; }
        public bool ShouldShowEditEmailField { get; set; }
        public bool ShouldShowEditPhoneField { get; set; }
        public bool ShouldShowEditDateField { get; set; }
        public bool ShouldShowEditNumberField { get; set; }
        public bool ShouldShowEditListField { get; set; }
        public bool ShouldShowEditCheckboxField { get; set; }
        public bool ShouldShowEditRadioField { get; set; }
        public bool ShouldShowDistribution { get; set; }
        public bool ShouldShowMultilineText { get; set; }

        public UIViewLicense() { }
        public UIViewLicense(UIViewLicense uiViewLicense) 
        {
            ShouldShowContacts = uiViewLicense?.ShouldShowContacts ?? true;
            ShouldShowGroupSign = uiViewLicense?.ShouldShowGroupSign ?? true;
            ShouldShowLiveMode = uiViewLicense?.ShouldShowLiveMode ?? true;
            ShouldShowSelfSign = uiViewLicense?.ShouldShowSelfSign ?? true;
            ShouldShowTemplates = uiViewLicense?.ShouldShowTemplates ?? true;
            ShouldShowAddNewTemplate = uiViewLicense?.ShouldShowAddNewTemplate ?? true;
            ShouldShowDocuments = uiViewLicense?.ShouldShowDocuments ?? true;
            ShouldShowProfile = uiViewLicense?.ShouldShowProfile ?? true;
            ShouldShowUploadAndsign = uiViewLicense?.ShouldShowUploadAndsign ?? true;
            ShouldShowDistribution = uiViewLicense?.ShouldShowDistribution ?? false;

            ShouldShowEditTextField = uiViewLicense?.ShouldShowEditTextField ?? true;
            ShouldShowEditSignatureField = uiViewLicense?.ShouldShowEditSignatureField ?? true;
            ShouldShowEditEmailField = uiViewLicense?.ShouldShowEditEmailField ?? true;
            ShouldShowEditPhoneField = uiViewLicense?.ShouldShowEditPhoneField ?? true;
            ShouldShowEditDateField = uiViewLicense?.ShouldShowEditDateField ?? true;
            ShouldShowEditNumberField = uiViewLicense?.ShouldShowEditNumberField ?? true;
            ShouldShowEditListField = uiViewLicense?.ShouldShowEditListField ?? true;
            ShouldShowEditCheckboxField = uiViewLicense?.ShouldShowEditCheckboxField ?? true;
            ShouldShowEditRadioField = uiViewLicense?.ShouldShowEditRadioField ?? true;
            ShouldShowMultilineText = uiViewLicense?.ShouldShowMultilineText ?? true;
        }
    }
}
