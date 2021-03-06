// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;

namespace CoreWebApi.Middleware
{
    /// <summary>
    /// This default implementation of the ICookieAuthenticationEvents may be used if the 
    /// application only needs to override a few of the interface methods. This may be used as a base class
    /// or may be instantiated directly.
    /// </summary>
    public class CusCookieAuthenticationEvents : ICusCookieAuthenticationEvents
    {
        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<CusCookieValidatePrincipalContext, Task> OnValidatePrincipal { get; set; } = context => TaskCache.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<CusCookieSigningInContext, Task> OnSigningIn { get; set; } = context => TaskCache.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<CusCookieSignedInContext, Task> OnSignedIn { get; set; } = context => TaskCache.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<CusCookieSigningOutContext, Task> OnSigningOut { get; set; } = context => TaskCache.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        // public Func<CookieRedirectContext, Task> OnRedirectToLogin { get; set; } = context =>
        // {
        //     if (IsAjaxRequest(context.Request))
        //     {
        //         context.Response.Headers["Location"] = context.RedirectUri;
        //         context.Response.StatusCode = 401;
        //     }
        //     else
        //     {
        //         context.Response.Redirect(context.RedirectUri);
        //     }
        //     return TaskCache.CompletedTask;
        // };

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        // public Func<CookieRedirectContext, Task> OnRedirectToAccessDenied { get; set; } = context =>
        // {
        //     if (IsAjaxRequest(context.Request))
        //     {
        //         context.Response.Headers["Location"] = context.RedirectUri;
        //         context.Response.StatusCode = 403;
        //     }
        //     else
        //     {
        //         context.Response.Redirect(context.RedirectUri);
        //     }
        //     return TaskCache.CompletedTask;
        // };

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        // public Func<CookieRedirectContext, Task> OnRedirectToLogout { get; set; } = context =>
        // {
        //     if (IsAjaxRequest(context.Request))
        //     {
        //         context.Response.Headers["Location"] = context.RedirectUri;
        //     }
        //     else
        //     {
        //         context.Response.Redirect(context.RedirectUri);
        //     }
        //     return TaskCache.CompletedTask;
        // };

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        // public Func<CookieRedirectContext, Task> OnRedirectToReturnUrl { get; set; } = context =>
        // {
        //     if (IsAjaxRequest(context.Request))
        //     {
        //         context.Response.Headers["Location"] = context.RedirectUri;
        //     }
        //     else
        //     {
        //         context.Response.Redirect(context.RedirectUri);
        //     }
        //     return TaskCache.CompletedTask;
        // };

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }		

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task ValidatePrincipal(CusCookieValidatePrincipalContext context) => OnValidatePrincipal(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SigningIn(CusCookieSigningInContext context) => OnSigningIn(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SignedIn(CusCookieSignedInContext context) => OnSignedIn(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SigningOut(CusCookieSigningOutContext context) => OnSigningOut(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        // public virtual Task RedirectToLogout(CookieRedirectContext context) => OnRedirectToLogout(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        // public virtual Task RedirectToLogin(CookieRedirectContext context) => OnRedirectToLogin(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        // public virtual Task RedirectToReturnUrl(CookieRedirectContext context) => OnRedirectToReturnUrl(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context">Contains information about the event</param>
        // public virtual Task RedirectToAccessDenied(CookieRedirectContext context) => OnRedirectToAccessDenied(context);
    }
}