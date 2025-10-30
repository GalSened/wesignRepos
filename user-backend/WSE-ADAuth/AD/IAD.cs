using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.AD
{
    public interface IAD
    {
        void InItADHandlerForUser(string userName);
        bool IsUserInADGroup(string searchForGroup);
        bool IsEmailAddressExist();
        string GetUserEmail();
        string GetUserAdName();
        List<string> GetUserPhones();
    }
}
