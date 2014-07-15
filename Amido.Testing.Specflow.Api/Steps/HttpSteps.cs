using System;
using System.Net;

using RestSharp;

using Should;

using TechTalk.SpecFlow;

namespace Amido.Testing.Specflow.Api.Steps
{
    [Binding]
    public class HttpSteps
    {
        [Then("the status code should be '(.*)'")]
        public void TheStatusCodeShouldEqual(int statusCode)
        {
            var lastResponse = ScenarioContextService.GetValue<IRestResponse>();
            lastResponse.StatusCode.ShouldEqual((HttpStatusCode)statusCode);
        }

        [Then("the response reason phrase should be '(.*)'")]
        public void TheReasonPhraseShouldEqual(string expectedReasonPhrase)
        {
            var lastResponse = ScenarioContextService.GetValue<IRestResponse>();
            lastResponse.StatusDescription.ShouldEqual(expectedReasonPhrase);
        }

        [Then(@"the body should include '(.*)'")]
        public void ThenTheBodyShouldInclude(string includedText)
        {
            var lastResponse = ScenarioContextService.GetValue<IRestResponse>();
            lastResponse.Content.ShouldNotBeNull("Body should not be null");
            lastResponse.Content.ShouldContain(includedText, StringComparison.OrdinalIgnoreCase);
        }

        [Then(@"the body should not include '(.*)'")]
        public void ThenTheBodyShouldNotInclude(string excludedText)
        {
            var lastResponse = ScenarioContextService.GetValue<IRestResponse>();
            lastResponse.Content.ShouldNotBeNull("Body should not be null");
            lastResponse.Content.ShouldNotContain(excludedText, StringComparison.OrdinalIgnoreCase);
        }
    }
}