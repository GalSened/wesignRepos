using ManagementBL.CleanDb;
using ManagementBL.CleanDb.Handlers;
using System;
using System.Collections.Generic;
using Serilog;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementBL
{
    public class CleanDBManager : ICleanDBManager
    {
        private readonly string @namespace = "ManagementBL.CleanDb.Handlers";
        private readonly ICleanDBFactory _cleanDBFactory;
        private readonly ILogger _logger;
        private readonly List<Type> _cleanDBTypes = new List<Type>();
        public CleanDBManager(ICleanDBFactory cleanDBFactory, ILogger logger)
        {
            _cleanDBFactory = cleanDBFactory;
            _logger = logger;


            _cleanDBTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.IsClass && t.Namespace == @namespace && !t.IsNested)
    .ToList();
        }
        
        public async Task StartCleanDBProcess()
        {
            _logger.Debug("Start Clean DB Process");
            foreach (var item in _cleanDBTypes)
            {
                
                _logger.Debug("Clean DB Process item {Item}", item);
                IDeleter deleter = _cleanDBFactory.GetDeleter(item);
                await deleter.DeleteProcess();
            }            
        }
    }
}
