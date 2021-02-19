# MIW-RazorPages-AAD
#### A proof of concept for Azure AD authentication with Razor Pages and multiple APIs

This test solution was set up in preparation for a change in our larger project to move to Azure AD (AAD). The larger project is a collection of separate but related applications with Razor Pages web clients and API backends (all .NET Core).

Each web client has its own API but there are a couple of APIs that are shared across multiple clients.

`Microsoft.Identity.Web` is used with AAD to authenticate users. Authorization will be handled by the application, not AAD.

Initial challenges were calling two different APIs from the one web client. Exceptions were throw when more than one API resource (NB: resource, not scope) was supplied in `EnableTokenAcquisitionToCallDownstreamApi`.

`MsalServiceException: AADSTS28000: Provided value for the input parameter scope is not valid because it contains more than one resource.`

As we were planning to use wrappers for API calls this wasn't a big problem, but was confusing initially.

Then using a wrapper around API calls so the API service could be used by other applications caused authentication problems.

`MicrosoftIdentityWebChallengeUserException: IDW10502: An MsalUiRequiredException was thrown due to a challenge for the user.`

If a token could not be found or had expired, the call to silenty refresh the token failed because we were in a class library, not a Razor Page or MVC controller (using the `AuthorizeForScopes` attribute).

Injecting `MicrosoftIdentityConsentAndConditionalAccessHandler` in to the DI container helped solve that issue as it allowed us to handle the exception in the API wrapper service.

Many thanks to [jpda](https://github.com/jpda/) for his help getting this working.
