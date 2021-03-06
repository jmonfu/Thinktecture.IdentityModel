﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Thinktecture.IdentityModel.WebApi
{
    [Obsolete("We recommend using the version based on OWIN middleware instead")]
    public class ScopeAuthorizeAttribute : AuthorizeAttribute
    {
        string[] _scopes;
        static string _scopeClaimType = "scope";

        public string[] Scopes
        {
            get { return _scopes;}
        }

        public static string ScopeClaimType 
        {
            get { return _scopeClaimType; }
            set { _scopeClaimType = value; }
        }

        public ScopeAuthorizeAttribute(params string[] scopes)
        {
            if (scopes == null)
            {
                throw new ArgumentNullException("scopes");
            }

            _scopes = scopes;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;

            if (principal == null)
            {
                return false;
            }

            var grantedScopes = principal.FindAll(_scopeClaimType).Select(c => c.Value).ToList();

            foreach (var scope in _scopes)
            {
                if (grantedScopes.Contains(scope, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "insufficient_scope");
            response.Headers.Add("WWW-Authenticate", "Bearer error=\"insufficient_scope\"");

            actionContext.Response = response;
        }
    }
}
