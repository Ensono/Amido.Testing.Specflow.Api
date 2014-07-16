using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using Amido.Testing.Specflow.Api.Controllers;
using Amido.Testing.Specflow.Api.Models;

using RestSharp;

using Should;

using TechTalk.SpecFlow;

namespace Amido.Testing.Specflow.Api.Steps
{
    [Binding]
    public class HttpSteps
    {
        private readonly HttpController httpController;

        private const string VariablesPattern = @"{{[A-Za-z0-9.\->]+?}}";

        private static Dictionary<string, string> shortKeys = new Dictionary<string, string>();

        public HttpSteps(HttpController httpController)
        {
            this.httpController = httpController;
        }

        public static void AddVariableLookupShortKey(string shortKey, string className)
        {
            shortKeys.Add(shortKey, className);
        }

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

        [When(@"I execute the following http request:")]
        public void WhenICallTheFollowingEndpointWithTheFollowingPostBody(Table table)
        {
            var httpRequestItems = table.Rows.Select(tableRow => 
                new HttpRequestItem { Type = tableRow[0], Value = tableRow[1] }).ToList();

            if (httpRequestItems.All(x => x.Type != "Endpoint"))
            {
                throw new ArgumentException("You must include an endpoint");
            }

            if (httpRequestItems.All(x => x.Type != "HttpVerb"))
            {
                throw new ArgumentException("You must include a http verb");
            }

            if (httpRequestItems.First(x => x.Type == "HttpVerb").Value == "POST" || 
                httpRequestItems.First(x => x.Type == "HttpVerb").Value == "PUT" || 
                httpRequestItems.First(x => x.Type == "HttpVerb").Value == "PATCH")
            {
                if (httpRequestItems.All(x => x.Type != "ContentType"))
                {
                    throw new ArgumentException("You must include a content type");
                }

                if (httpRequestItems.All(x => x.Type != "Body"))
                {
                    throw new ArgumentException("You must include a body");
                }
            }

            var endpoint = UpdateVariables(httpRequestItems.First(x => x.Type == "Endpoint").Value);
            if (httpRequestItems.FirstOrDefault(x => x.Type == "Body") != null)
            {
                var body = UpdateVariables(httpRequestItems.First(x => x.Type == "Body").Value);
                httpRequestItems.First(x => x.Type == "Body").Value = body;
            }

            foreach (var header in httpRequestItems.Where(x => x.Type == "Header"))
            {
                header.Value = UpdateVariables(header.Value);
            }
            
            httpRequestItems.First(x => x.Type == "Endpoint").Value = endpoint;

            httpController.Execute(httpRequestItems);
        }

        private static string UpdateVariables(string value)
        {
            var matches = Regex.Matches(value, VariablesPattern);

            foreach (Match match in matches)
            {
                var parts = match.Value.Replace("{{", "").Replace("}}", "").Split(new[]{"->"},StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    throw new ArgumentException("The variable " + match.Value + " does not seem to point to a property");
                }
                var objectName = parts[0];

                if (shortKeys.ContainsKey(objectName))
                {
                    objectName = shortKeys[objectName];
                }

                var instance = TryGetInstanceFromContexts(objectName);
                
                if (instance == null)
                {
                    throw new ArgumentException("The " + objectName + " class is not currently supported, update the ApiSteps");
                }

                value = value.Replace(match.Value, GetPropertyValueAsString(instance, parts, 0, parts.Length - 1));
            }

            return value;
        }

        private static object TryGetInstanceFromContexts(string objectName)
        {
            try
            {
                var scenarioObject = ScenarioContextService.GetValue<object>(objectName);
                return scenarioObject;
            }
            catch
            {
                // swallow
            }

            try
            {
                var featureObject = FeatureContextService.GetValue<object>(objectName);
                return featureObject;
            }
            catch
            {
                // swallow
            }

            try
            {
                var globalObject = GlobalContextService.GetValue<object>(objectName);
                return globalObject;
            }
            catch
            {
                // swallow
            }

            return null;
        }

        private static string GetPropertyValueAsString(object parent, string[] parts, int currentPart, int maxParts)
        {
            if (currentPart == maxParts - 1)
            {
                return parent.GetType().GetProperty(parts[currentPart + 1]).GetValue(parent).ToString();
            }

            return GetPropertyValueAsString(
                parent.GetType().GetProperty(parts[currentPart + 1]).GetValue(parent),
                parts,
                currentPart + 1,
                maxParts);
        }
    }
}