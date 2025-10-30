using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Dictionaries
{
    public class LanguageDictionary
    {
        public static readonly Dictionary<Language, Dictionary<string, string>> languageDictionary = new Dictionary<Language, Dictionary<string, string>>
        {
            { Language.en,
                new Dictionary<string, string>
                {
                    {"id", "id" },
                    {"Document Name", "Document Name" },
                    {"DocumentAmount", "Document Amount" },
                    {"ContactName", "Contact Name" },
                    {"Sender", "Sender" },
                    {"Creation Time", "Creation Time" },
                    {"EmailPhone", "Email/Phone" },
                    {"Sent To", "Sent To" },
                    {"Time Sent", "Time Sent" },
                    {"Time Viewed", "Time Viewed" },
                    {"Time Signed", "Time Signed" },
                    {"Time Rejected", "Time Rejected" },
                    {"CompanyName", "Company Name" },
                    {"GroupId", "Group Id" },
                    {"GroupName", "Group Name" },
                    {"SentDocumentsCount", "Sent" },
                    {"PendingDocumentsCount", "Pending" },
                    {"SignedDocumentsCount", "Signed" },
                    {"DeclinedDocumentsCount", "Declined" },
                    {"CanceledDocumentsCount", "Canceled" },
                    {"DeletedDocumentsCount", "Deleted" },
                    {"DistributionDocumentsCount", "Distribution" },
                }
            },
            {
                Language.he,
                new Dictionary<string, string>
                {
                    {"id", "מזהה מסמך" },
                    {"Document Name", "שם מסמך" },
                    {"Document Amount", "סך כל המסמכים" },
                    {"ContactName", "איש קשר" },
                    {"Sender", "שולח" },
                    {"Creation Time", "זמן יצירה" },
                    {"EmailPhone", "דוא\"ל/טלפון" },
                    {"Sent To", "נמען" },
                    {"Time Sent", "זמן שליחה" },
                    {"Time Viewed", "זמן צפייה" },
                    {"Time Signed", "זמן חתימה" },
                    {"Time Rejected", "זמן דחייה" },
                    {"CompanyName", "שם החברה" },
                    {"GroupId", "מספר קבוצה" },
                    {"GroupName", "שם הקבוצה" },
                    {"SentDocumentsCount", "נשלחו" },
                    {"PendingDocumentsCount", "בהמתנה" },
                    {"SignedDocumentsCount", "נחתמו" },
                    {"DeclinedDocumentsCount", "נדחו" },
                    {"CanceledDocumentsCount", "בוטלו" },
                    {"DeletedDocumentsCount", "נמחקו" },
                    {"DistributionDocumentsCount", "בהפצה" },
                }
            }
        };
    }
}