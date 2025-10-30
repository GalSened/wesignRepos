using System;

namespace Common.Consts
{
    public class Consts
    {

        public static readonly Guid SYSTEM_ADMIN_ID = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        public static readonly Guid PAYMENT_ADMIN_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
        public static readonly Guid GHOST_USER_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5);
        public static readonly Guid FREE_ACCOUNTS_COMPANY_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        public static readonly Guid GHOST_USERS_COMPANY_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5);
        public static readonly Guid DEV_USER_ADMIN_ID = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6);

        public static readonly string EXPIRED_PASSWORD = "EXPIRED_PASSWORD";
        public const int UNLIMITED = -1;
        public const int NEVER = -1;
        public const string EMPTY = "";

        public const string DATE_FORMAT = "MMMM dd, yyyy";
        internal static readonly Guid EMPTY_GUID = Guid.Empty;

        public static readonly Guid PROGRAM_TRAIAL = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        public static readonly Guid PROGRAM_UNLIMITED = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
        public static readonly Guid PROGRAM_BASIC = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

        public const string JPEG = "jpeg";
        public static readonly string SAML_SEPARATOR = "###Separator###";

        public const string symmetricKey = "b14ca5898a4e4133bbce2ea2315a1916";
    }
}
