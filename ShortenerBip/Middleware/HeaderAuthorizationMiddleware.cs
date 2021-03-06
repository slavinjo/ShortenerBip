﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShortenerBip.Helper;
using ShortenerBip.Interfaces;
using ShortenerBip.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortenerBip.Middleware
{
    public class HeaderAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private DataContext _dataContext;

        

        public HeaderAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
            
        }

        public async Task Invoke(HttpContext context,  DataContext dbContext)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                _dataContext = dbContext;
                bool isHeaderValid = ValidateCredentials(authHeader,_dataContext);
                if (isHeaderValid)
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            //Reject request if there is no authorization header or if it is not valid
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");

        }


        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;
            request.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body = body;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        public static bool ValidateCredentials(string token, DataContext con)
        {
            if (con.Users.Any(x => x.Token == token))
                return true;
            return false;
        }


        public static bool ValidateCredentialsForStats(string token,string AccountId, DataContext con)
        {
            if (con.Users.Any(x => x.Token == token && x.AccountId == AccountId))
                return true;
            return false;
        }

    }
}