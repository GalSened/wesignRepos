using Common.Models;
using Common.Models.Configurations;
using Common.Models.Users;
using DAL.DAOs.Configurations;
using DAL.DAOs.Users;
using System.Linq;

    namespace DAL.Extensions
    {
        public static class UserExtensions
        {
            public static User ToUser(this UserDAO userDAO)
            {
                if (userDAO == null)
                {
                    return null;
                }

                var user = new User()
                {
                    Id = userDAO.Id,
                    CompanyId = userDAO.CompanyId,
                    Name = userDAO.Name,
                    Email = userDAO.Email,
                    Password = userDAO.Password,
                    PasswordSetupTime = userDAO.PasswordSetupTime,
                    Type = userDAO.Type,
                    ProgramUtilizationId = userDAO.ProgramUtilizationId,
                    Status = userDAO.Status,
                    CreationTime = userDAO.CreationTime,
                    GroupId = userDAO.GroupId,
                    ProgramUtilization = userDAO.ProgramUtilization.ToProgramUtilization(),
                    UserConfiguration = userDAO.UserConfiguration.ToUserConfiguration(),
                    UserTokens = userDAO.UserTokens.ToUserTokens(),
                    CreationSource = userDAO.CreationSource,
                    LastSeen = userDAO.LastSeen,
                    Username = userDAO.Username,
                    Phone = userDAO.Phone

                };
                if (userDAO.AdditionalGroupsMapper != null)
                {
                    user.AdditionalGroupsMapper = userDAO.AdditionalGroupsMapper.Select(x => x.ToAdditionalGroupMapper()).ToList();
                }
                return user;
            }

            public static UserConfiguration ToUserConfiguration(this UserConfigurationDAO userConfigurationDAO)
            {
                return userConfigurationDAO == null ? null : new UserConfiguration()
                {
                    ShouldSendSignedDocument = userConfigurationDAO.ShouldSendSignedDocument,
                    ShouldNotifyWhileSignerSigned = userConfigurationDAO.ShouldNotifyWhileSignerSigned,
                    ShouldNotifyWhileSignerViewed = userConfigurationDAO.shouldNotifyWhileSignerViewed,
                    ShouldDisplayNameInSignature = userConfigurationDAO.ShouldDisplayNameInSignature,
                    ShouldNotifySignReminder = userConfigurationDAO.ShouldNotifySignReminder,
                    SignReminderFrequencyInDays = userConfigurationDAO.SignReminderFrequencyInDays,
                    SignatureColor = userConfigurationDAO.SignatureColor,
                    Language = userConfigurationDAO.Language
                };
        }

        public static UserOtpDetails ToUserOtpDetails(this UserOtpDetailsDAO userOtpDetailsDAO)
        {
            return userOtpDetailsDAO == null ? null : new UserOtpDetails
            {
                Code = userOtpDetailsDAO.Code,
                ExpirationTime = userOtpDetailsDAO.ExpirationTime,
                Id = userOtpDetailsDAO.Id,
                OtpMode = userOtpDetailsDAO.OtpMode,
                UserId = userOtpDetailsDAO.UserId,
                AdditionalInfo = userOtpDetailsDAO.AdditionalInfo

            };
        }
    }
    }