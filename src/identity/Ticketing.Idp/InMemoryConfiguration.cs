using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ticketing.Idp
{
    public static class InMemoryConfiguration
    {
        // test users
        public static List<TestUser> GetUsersFromIdentityRepository()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "7ad1db51-a99f-4f94-a102-c6467040f821",
                    Username = "Bob",
                    Password = "Mallo",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Bob"),
                        new Claim("family_name", "Mallo"),
                        new Claim("address", "Main Road 1"),
                        new Claim("role", "FreeUser"),
                        new Claim("subscriptionlevel", "FreeUser"),
                        new Claim("country", "nl")
                    }
                }
            };
        }

        // identity-related resources (scopes)
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(
                    "roles",
                    "Your role(s)",
                     new List<string>() { "role" }),
                new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>() { "country" }),
                new IdentityResource(
                    "subscriptionlevel",
                    "Your subscription level",
                    new List<string>() { "subscriptionlevel" })
            };
        }

        // api-related resources (scopes)
        public static IEnumerable<ApiResource> GetSecuredApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(
                        "api://ticketing-core",
                        "Ticketing Core API",
                        new List<string>() {"orders:read", "orders:write", "users:read", "users:write" } ),
                new ApiResource(
                        "api://ticketing-new",
                        "Ticketing New API",
                        new List<string>() {"orders:read", "users:read"} )
            };
        }


        public static IEnumerable<Client> GetAllowedOAuthClients()
        {
            return new List<Client>()
            {
                new Client
                {
                    ClientName = "TicketingTestApp",
                    ClientId = "ticketingtestapp",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "country",
                        "subscriptionlevel",
                        "concerts:read",
                        "concerts:modify",
                        "api://ticketing-core", // TODO scope not resources  
                        "api://ticketing-new" // TODO scope not resources
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                },
                new Client
                {
                    ClientName = "Ticketing-SPA",
                    ClientId = "ticketingspa",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44355/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:44355/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "country",
                        "subscriptionlevel",
                        "concerts:read",
                        "concerts:modify",
                        "users:read",
                        "users:modify"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                }
             };

        }
    }
}
