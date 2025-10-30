using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Extensions
{
    public static class ContactsExtenstions
    {
        public static bool IsValidPhone(this string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            try
            {
                //Regex rgx = new Regex("^[0-9\\-\\+]{9,15}$");
                Regex rgx = new Regex("^[0-9\\-]{9,15}$");
                return rgx.IsMatch(phone.ToString());
            }
            catch (Exception )
            {
                return false;
            }
        }


        public static bool IsValidPhoneExtension(string phoneExtention)
        {
            if (string.IsNullOrWhiteSpace(phoneExtention))
            {
                return false;
            }
            try
            {
                Regex rgx = new Regex(@"\+(9[976]\d|8[987530]\d|6[987]\d|5[90]\d|42\d|3[875]\d|2[98654321]\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)");
                return rgx.IsMatch(phoneExtention);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidPhoneWithExtention(this string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            try
            {
                Regex rgx = new Regex(@"(\+\d{1,3}\s?)?((\(\d{3}\)\s?)|(\d{3})(\s|-?))(\d{3}(\s|-?))(\d{4})");
                return rgx.IsMatch(phone);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool IsValidEmail(this string email)
        {
            //Regex rgx = new Regex("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,20}$");
            //return rgx.IsMatch(email);
            if(string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            string trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidEmail_CheckWithMailAddress(this string emailAddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailAddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
