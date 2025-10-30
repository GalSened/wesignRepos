using Common.Models;
using DAL.DAOs.Users;

namespace DAL.Extensions
{
    public static class UserPasswordHistoryExtensions
    {
        public static UserPasswordHistory ToUserPasswordHistory(this UserPasswordHistoryDAO userPasswordHistoryDAO)
        {
            return userPasswordHistoryDAO == null ? null : new UserPasswordHistory()
            {
                UserId = userPasswordHistoryDAO.UserId,
                Password = userPasswordHistoryDAO.Password,
                CreationTime = userPasswordHistoryDAO.CreationTime
            };
        }
    }
}
