using Microsoft.AspNetCore.Builder;
using ShortenerBip.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Extensions
{
    public static class HeaderAuthorizationMiddlewareExtension
    {
        public static IApplicationBuilder UseHeaderAuthorization(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<HeaderAuthorizationMiddleware>();
        }
    }
}