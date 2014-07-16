using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Amido.Testing.Specflow.Api.Controllers;
using Amido.Testing.Specflow.Api.Steps;
using Amido.Testing.Specflow.Api.Tests.TestModels;

using TechTalk.SpecFlow;

namespace Amido.Testing.Specflow.Api.Tests
{
    [Binding]
    public class StartUpHooks
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks

        [BeforeFeature]
        public static void BeforeFeature()
        {
            HttpController.Init(() => "http://www.bing.com");
            HttpSteps.AddVariableLookupShortKey("SearchModel", typeof(SearchModel).FullName);
            var searchModel = new SearchModel
                                  {
                                      Term = "tree"
                                  };
            FeatureContext.Current.Add(searchModel.GetType().FullName, searchModel);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            //TODO: implement logic that has to run after executing each scenario
        }
    }
}
