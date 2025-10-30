using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WSE_ADAuth.Models;

namespace WSE_ADAuth.Handler
{
    public interface INewUserGenerator
    {
        Task<User> CreateNewUser(LoginToClient loginToClient);
    }
}
