using System;
using System.Collections.Generic;
using System.Linq;

using Amido.Testing.Specflow.Api.Models;

using RestSharp;

namespace Amido.Testing.Specflow.Api.Controllers
{
    public class HttpController
    {
        private static Func<string> apiBaseUrl;

        private static Func<string, IRestClient> apiRestClientFactory;

        private static Func<Method, IRestRequest> apiRestRequestFactory;

        public static void Init(Func<string> baseUrl, Func<string, IRestClient> restClientFactory = null, Func<Method, IRestRequest> restRequestFactory = null)
        {
            apiBaseUrl = baseUrl;
            apiRestClientFactory = restClientFactory;
            apiRestRequestFactory = restRequestFactory;
        }

        public void Execute(List<HttpRequestItem> httpRequestItems)
        {
            if (apiBaseUrl == null)
            {
                throw new ArgumentException("You must call Init and set the baseUrl");    
            }

            var restClient = apiRestClientFactory != null 
                                         ? apiRestClientFactory(apiBaseUrl()) 
                                         : new RestClient(apiBaseUrl());
            

            var verb = GetMethod(httpRequestItems.First(x => x.Type == "HttpVerb").Value);

            var restRequest = apiRestRequestFactory != null
                                           ? apiRestRequestFactory(verb)
                                           : new RestRequest(verb);

            restRequest.Resource = httpRequestItems.First(x => x.Type == "Endpoint").Value;

            if (verb == Method.POST || verb == Method.PUT || verb == Method.PATCH)
            {
                var contentType = httpRequestItems.First(x => x.Type == "ContentType").Value;
                var body = httpRequestItems.First(x => x.Type == "Body").Value;
                restRequest.AddHeader("content-type", contentType);
                restRequest.AddParameter(contentType, body, ParameterType.RequestBody);
            }

            foreach (var header in httpRequestItems.Where(x => x.Type == "Header"))
            {
                restRequest.AddHeader(header.Type, header.Value);
            }

            var response = restClient.Execute(restRequest);
            ScenarioContextService.SaveValue(response);
        }

        private static Method GetMethod(string httpRequestVerb)
        {
            switch (httpRequestVerb)
            {
                case "GET":
                    return Method.GET;
                case "POST":
                    return Method.POST;
                case "PUT":
                    return Method.PUT;
                case "DELETE":
                    return Method.DELETE;
                case "HEAD":
                    return Method.HEAD;
                case "PATCH":
                    return Method.PATCH;
                case "OPTIONS":
                    return Method.OPTIONS;
            }
            throw new ArgumentException(string.Format("The verb {0} is not supported.", httpRequestVerb));
        }
    }
}