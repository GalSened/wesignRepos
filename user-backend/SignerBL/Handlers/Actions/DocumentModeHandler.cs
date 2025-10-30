using Common.Enums.Documents;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.PDF;
using Common.Interfaces.SignerApp;
using Serilog;
using System;
using System.Collections.Generic;
using ISignerValidator = Common.Interfaces.SignerApp.ISignerValidator;

namespace SignerBL.Handlers.Actions
{
    public class DocumentModeHandler : IDocumentModeHandler
    {
      

        private readonly IDictionary<DocumentMode, IDocumentModeHandlerFactory> _factories;

        public DocumentModeHandler(ILogger logger, ICompanyConnector companyConnector, IDocumentCollectionConnector documentCollectionConnector,
             IConfiguration configuration, ISignerValidator validator, ISender sender, IDater dater, IDoneActionsHelper actionsHelper)
        {
            _factories = new Dictionary<DocumentMode, IDocumentModeHandlerFactory>();
            foreach (DocumentMode documentMode in Enum.GetValues(typeof(DocumentMode)))
            {
                var parameters = new object[8];
                parameters[0] = logger;
                parameters[1] = companyConnector;
                parameters[2] = documentCollectionConnector;
                parameters[3] = configuration;
                parameters[4] = validator;
                parameters[5] = sender;
                parameters[6] = dater;
                parameters[7] = actionsHelper;
                if (documentMode != DocumentMode.SelfSign )
                {
                    var type = Type.GetType($"SignerBL.Handlers.Actions.{Enum.GetName(typeof(DocumentMode), documentMode)}HandlerFactory");
                    var factory = (IDocumentModeHandlerFactory)Activator.CreateInstance(type, parameters);
                    _factories.Add(documentMode, factory);
                }
            }
        }

        public IDocumentModeAction ExecuteCreation(DocumentMode documentMode) => _factories[documentMode].Create();

    }
}




