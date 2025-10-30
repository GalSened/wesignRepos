namespace Common.Models
{
    using Common.Models.License;
    using System;

    public class Program
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Users { get; set; }
        public long Templates { get; set; }
        public long DocumentsPerMonth { get; set; }
        public long SmsPerMonth { get; set; }
        public long VisualIdentificationsPerMonth { get; set; }
        public long VideoConferencePerMonth { get; set; }
        public bool ServerSignature { get; set; }
        public bool SmartCard { get; set; }
        public string Note { get; set; }
        public UIViewLicense UIViewLicense { get; set; }

        public Program()
        {
            UIViewLicense = new UIViewLicense();
        }
    }
}
