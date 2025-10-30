using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Dashboard;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Dashboard;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BL.Handlers
{
    public class DashboardHandler : IDashboard
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUsers _users;
        private readonly IMemoryCache _memoryCache;

        public DashboardHandler(IServiceScopeFactory scopeFactory, IUsers users, IMemoryCache memoryCache)
        {
            _scopeFactory = scopeFactory;
            _users = users;
            _memoryCache = memoryCache;
        }

        public async Task<DashboardView> GetDashboardView()
        {
            (User user, _) = await _users.GetUser();
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.UserNotExist.GetNumericString());
            }
            DashboardView dashboardView = _memoryCache.Get<DashboardView>($"dashboardview_{user.GroupId}");
            if (dashboardView == null)
            {
                using var scope = _scopeFactory.CreateScope();
                IDashboardConnector dashboardConnector = scope.ServiceProvider.GetService<IDashboardConnector>();
                dashboardView = dashboardConnector.ReadDashboardView(user.GroupId);
                _memoryCache.Set($"dashboardview_{user.GroupId}", dashboardView, TimeSpan.FromMinutes(2));
            }
            return dashboardView;
        }
    }
}
