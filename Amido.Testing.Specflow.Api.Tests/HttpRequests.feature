Feature: HttpRequests
	In order to quickly write API tests
	as a BA
	I want to be able to write simple specifications that are immediately available for execution

Scenario: Call Google
	When I execute the following http request:
	| Field    | Value   |
	| Endpoint | ?q=tree |
	| HttpVerb | GET     |
	Then the status code should be '200'
	And the body should include 'tree'


Scenario: Call Google using a context value
	When I execute the following http request:
	| Field    | Value   |
	| Endpoint | ?q={{Amido.Testing.Specflow.Api.Tests.TestModels.SearchModel->Term}} |
	| HttpVerb | GET     |
	Then the status code should be '200'
	And the body should include 'tree'

Scenario: Call Google using a context value via a shortKey
	When I execute the following http request:
	| Field    | Value   |
	| Endpoint | ?q={{SearchModel->Term}} |
	| HttpVerb | GET     |
	Then the status code should be '200'
	And the body should include 'tree'
	