using Microsoft.AspNetCore.Builder;
using ShortenerBip.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Middleware
{
    public class  HeaderAuthorizationPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseHeaderAuthorization();
        }
    }
}