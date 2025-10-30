namespace Common.Models.Emails
{
    using Common.Enums;
    using System.Collections.Generic;
    using System;

    public class HtmlBody
    {
        public string TemplateText { get; set; }
        public string Logo
        {
            get { return PlaceHoldersToData["[LOGO_URL]"]; }
            set { PlaceHoldersToData["[LOGO_URL]"] = value; }
        }
        public string ClientName
        {
            get { return PlaceHoldersToData["[CLIENT_NAME_TEXT]"]; }
            set { PlaceHoldersToData["[CLIENT_NAME_TEXT]"] = value; }
        }
        public string Link
        {
            get { return PlaceHoldersToData["[DOCUMENT_LINK_URL]"]; }
            set { PlaceHoldersToData["[DOCUMENT_LINK_URL]"] = value; }
        }
        public string LinkText
        {
            get { return PlaceHoldersToData["[LINK_TEXT]"]; }
            set { PlaceHoldersToData["[LINK_TEXT]"] = value; }
        }
        public string Date
        {
            get { return PlaceHoldersToData["[DATE_TEXT]"]; }
            set { PlaceHoldersToData["[DATE_TEXT]"] = value; }
        }
        public TextDirection TextDirection
        {
            get { Enum.TryParse(PlaceHoldersToData["[DIRECTION_TEXT]"], out TextDirection textDirection); return textDirection; }
            set { PlaceHoldersToData["[DIRECTION_TEXT]"] = value.ToString(); }
        }
        public string AttentionText
        {
            get { return PlaceHoldersToData["[ATTENTION_TEXT]"]; }
            set { PlaceHoldersToData["[ATTENTION_TEXT]"] = value; }
        }
        public string AttachmentText
        {
            get { return PlaceHoldersToData["[ATTACHMENT_TEXT]"]; }
            set { PlaceHoldersToData["[ATTACHMENT_TEXT]"] = value; }
        }
        public string AttachmentSpace
        {
            get { return PlaceHoldersToData["[ATTACHMENT_SPACE]"]; }
            set { PlaceHoldersToData["[ATTACHMENT_SPACE]"] = value; }
        }
        public string AttachmentDoNotReply
        {
            get { return PlaceHoldersToData["ATTACHMENT_DO_NOT_REPLY]"]; }
            set { PlaceHoldersToData["[ATTACHMENT_DO_NOT_REPLY]"] = value; }
        }
        public string CopyrightText
        {
            get { return PlaceHoldersToData["[COPYRIGHT_TEXT]"]; }
            set { PlaceHoldersToData["[COPYRIGHT_TEXT]"] = value; }
        }
        public string DigitalText
        {
            get { return PlaceHoldersToData["[DIGITAL_TEXT]"]; }
            set { PlaceHoldersToData["[DIGITAL_TEXT]"] = value; }
        }
        public string VisitText
        {
            get { return PlaceHoldersToData["[VISIT_TEXT]"]; }
            set { PlaceHoldersToData["[VISIT_TEXT]"] = value; }
        }
        public string EmailText
        {
            get { return PlaceHoldersToData["[EMAIL_TEXT]"]; }
            set { PlaceHoldersToData["[EMAIL_TEXT]"] = value; }
        }
        private IDictionary<string, string> PlaceHoldersToData { get; set; }

        public HtmlBody()
        {
            PlaceHoldersToData = new Dictionary<string, string>
            {
                { "[LOGO_URL]", null },
                { "[CLIENT_NAME_TEXT]", null },
                { "[DOCUMENT_LINK_URL]", null },
                { "[LINK_TEXT]", null },
                { "[DATE_TEXT]", null },
                { "[DIRECTION_TEXT]", null },
                { "[ATTENTION_TEXT]", string.Empty },
                { "[ATTACHMENT_TEXT]", string.Empty },
                { "[ATTACHMENT_SPACE]", string.Empty },
                { "[ATTACHMENT_DO_NOT_REPLY]", string.Empty },
                { "[COPYRIGHT_TEXT]", null },
                { "[DIGITAL_TEXT]", null },
                { "[VISIT_TEXT]", null },
                { "[EMAIL_TEXT]", null }
            };
        }

        public override string ToString()
        {
            foreach (var item in PlaceHoldersToData)
            {
                if (item.Value != null && !string.IsNullOrWhiteSpace(TemplateText) && TemplateText.Contains(item.Key))
                {
                    TemplateText = TemplateText.Replace(item.Key, item.Value);
                }
            }
            return TemplateText;
        }
    }
}
    