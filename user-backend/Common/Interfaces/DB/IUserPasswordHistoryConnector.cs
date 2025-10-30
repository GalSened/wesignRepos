using Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.DB
{
    public interface IUserPasswordHistoryConnector
    {
        Task Create(UserPasswordHistory uph);
        IEnumerable<UserPasswordHistory> ReadAllByUserId(Guid userId);
        Task DeleteOldestPasswordsByUserId(Guid userId, int count);
        Task DeleteAllByUserId(Guid userId);
    }
}
