using Comda.License.DAL;
using Common.Interfaces.License;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net;
using System.Reflection;
using System.Text;

namespace Common.Models.License
{
    public class WeSignLicense : IWeSignLicense
    {
        public DateTime ExpirationTime { get; set; }
        public LicenseCounters LicenseCounters { get; set; }
        public UIViewLicense UIViewLicense { get; set; }

        public WeSignLicense(IEnumerable<LicensePropertyDB> licenseProperties, IFileSystem fileSystem)
        {
            UIViewLicense = new UIViewLicense();
            LicenseCounters = new LicenseCounters();
            string currentFolder = fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string jsonPath = fileSystem.Path.Combine(currentFolder, "Resources", "LmsLicenseParams.json");
            string json = WebUtility.HtmlDecode(fileSystem.File.ReadAllText(jsonPath, Encoding.UTF8));
            LmsLicenseParams license = JsonConvert.DeserializeObject<LmsLicenseParams>(json);

            foreach (var licensePropertie in licenseProperties)
            {
                if (licensePropertie.Key == license.Templates && licensePropertie.Value is int)
                {
                    LicenseCounters.Templates = (int)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.Users && licensePropertie.Value is int)
                {
                    LicenseCounters.Users = (int)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.DocumentsPerMonth && licensePropertie.Value is int)
                {
                    LicenseCounters.DocumentsPerMonth = (int)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.SmsPerMonth && licensePropertie.Value is int)
                {
                    LicenseCounters.SmsPerMonth = (int)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.VisualIdentificationsPerMonth && licensePropertie.Value is int)
                {
                    LicenseCounters.VisualIdentificationsPerMonth = (int)licensePropertie.Value;
                    continue;
                } 
                if (licensePropertie.Key == license.ShouldShowSelfSign && licensePropertie.Value is bool)
                {
                    UIViewLicense.ShouldShowSelfSign = (bool)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.ShouldShowGroupSign && licensePropertie.Value is bool)
                {
                    UIViewLicense.ShouldShowGroupSign = (bool)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.ShouldShowLiveMode && licensePropertie.Value is bool)
                {
                    UIViewLicense.ShouldShowLiveMode = (bool)licensePropertie.Value;
                    continue;
                }
                if (licensePropertie.Key == license.ShouldShowContacts && licensePropertie.Value is bool)
                {
                    UIViewLicense.ShouldShowContacts = (bool)licensePropertie.Value;
                }
                if (licensePropertie.Key == license.UseActiveDirectory && licensePropertie.Value is bool)
                {
                    LicenseCounters.UseActiveDirectory = (bool)licensePropertie.Value;
                }


            }

        }

    }
}
