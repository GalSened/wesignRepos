using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.CleanDb.Handlers
{
    class TemplatesDeleter : IDeleter
    {
        
        private readonly ILogger _logger;
        
        private readonly ITemplatePdf _templatePdf;
        private readonly IServiceScopeFactory _scopeFactory;

        public TemplatesDeleter( ILogger logger, 
            ITemplatePdf  templatePdf, IServiceScopeFactory scopeFactory)
        {
            
            _logger = logger;            
            _templatePdf = templatePdf;
            _scopeFactory = scopeFactory;
        }
        public async Task<bool> DeleteProcess()
        {
            using var scope = _scopeFactory.CreateScope();
            ITemplateConnector templateConnector = scope.ServiceProvider.GetService<ITemplateConnector>();
          
            
            IEnumerable<Template> templates = templateConnector.ReadDeletedTemplatesWithoutConnectedDocuments();

            if (templates.Any())
            {
                try
                {
                    using (var innerScope = _scopeFactory.CreateScope())
                    {
                        ITemplateConnector dependencyService = scope.ServiceProvider.GetService<ITemplateConnector>();
                        await dependencyService.Delete(templates.ToList(), DeleteTemplatesFromFS);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to clean templates form DB ");
                }
            }

            templates = templateConnector.ReadOneTimeTemplatesWithoutConnectedDocuments();
            if (templates.Any())
            {
                try
                {
                    using (var innerScope = _scopeFactory.CreateScope())
                    {
                        ITemplateConnector dependencyService = scope.ServiceProvider.GetService<ITemplateConnector>();
                       await dependencyService.Delete(templates.ToList(), DeleteTemplatesFromFS);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to clean templates form DB ");
                }
            }

            return true;
        }
  

        private void DeleteTemplatesFromFS(List<Template> templates)
        {
            foreach (var template in templates)
            {
                try
                {
                    _templatePdf.SetId(template.Id);
                    _templatePdf.Delete();
                    _logger.Debug("template {TemplateId} deleted successfully", template.Id);
                }
                catch 
                {
                    // do nothing
                }
            }


        }
    }
}
