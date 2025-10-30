using Common.Models.ActiveDirectory;
using Common.Models.Configurations;
using DAL.DAOs.ActiveDirectory;


namespace DAL.Extensions
{
    public static class ActiveDirectoryExtensions
    {
        public static ActiveDirectoryConfiguration ToActiveDirectoryConfiguration(this ActiveDirectoryConfigDAO activeDirectoryConfigDAO)
        {
            return activeDirectoryConfigDAO == null ? null : new ActiveDirectoryConfiguration
            {
                Id = activeDirectoryConfigDAO.Id,
                Container = activeDirectoryConfigDAO.Container,
                Host = activeDirectoryConfigDAO.Host,
                User = activeDirectoryConfigDAO.User,
                Password = activeDirectoryConfigDAO.Password,
                Port = activeDirectoryConfigDAO.Port,
                Domain = activeDirectoryConfigDAO.Domain
            };
        }

        public static ActiveDirectoryGroup ToGroup(this ActiveDirectoryGroupDAO activeDirectoryGroupDAO)
        {
            return activeDirectoryGroupDAO == null ? null : new ActiveDirectoryGroup
            {
                Id = activeDirectoryGroupDAO.Id,
                GroupId = activeDirectoryGroupDAO.GroupId,
                ActiveDirectoryContactsGroupName = activeDirectoryGroupDAO.ActiveDirectoryContactsGroupName,
                ActiveDirectoryUsersGroupName = activeDirectoryGroupDAO.ActiveDirectoryUsersGroupName,
                Group = activeDirectoryGroupDAO.Group?.ToGroup(),
                GroupName = activeDirectoryGroupDAO.Group.Name
            };
        }
    }
}
