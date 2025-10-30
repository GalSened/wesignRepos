namespace DAL.Extensions
{
    using Common.Models;
    using DAL.DAOs.Groups;
    using System.Linq;

    public static class GroupExtentions
    {

        public static AdditionalGroupMapper ToAdditionalGroupMapper(this AdditionalGroupMapperDAO additionalGroupMapperDAO)
        {
            return additionalGroupMapperDAO == null ? null :
                new AdditionalGroupMapper()
                {
                    CompanyId = additionalGroupMapperDAO.CompanyId,
                    Id = additionalGroupMapperDAO.Id,
                    GroupId = additionalGroupMapperDAO.GroupId,
                    UserId = additionalGroupMapperDAO.UserId,
                };
        }

        public static Group ToGroup(this GroupDAO groupDAO)
        {
            return groupDAO == null ? null : new Group()
            {
                Id = groupDAO.Id,
                CompanyId = groupDAO.CompanyId,
                Name = groupDAO.Name,
                GroupStatus = groupDAO.GroupStatus,
                Users = groupDAO.Users?.Select(u => u.ToUser())
            };
        }
    }
}
