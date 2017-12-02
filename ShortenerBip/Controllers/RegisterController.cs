using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShortenerBip.Models;
using ShortenerBip.Helper;
using Microsoft.AspNetCore.Authorization;
using ShortenerBip.Middleware;
using Microsoft.Extensions.Options;

namespace ShortenerBip.Controllers
{
    [Produces("application/json")]
    [Route("Register")]
    public class RegisterController : Controller
    {
        private DataContext _context;
        private readonly AppSettings _settings;
        URLModel _model = new URLModel();      
        CheckURL _check = new CheckURL();
        ShorterURL _shorter = new ShorterURL();

        public static String getPath(HttpContext con)
        {
            return con.Request.Scheme + "://" + con.Request.Host.ToString() + "/";
        }

        public RegisterController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _settings = appSettings.Value;
        }


        [HttpGet]
        public string Get()
        {
            return "URL Shortener Service Active";
        }


        /*prebaci na Home*/
        //[HttpGet("/{id}")]
        //public IActionResult Get(String id)
        //{
        //    try
        //    {
        //        var result = _context.URL.Where(x => x.ShortCode == _settings.AppUrl + id).FirstOrDefault();
        //        if (result.RedirectType == 302)
        //            return Redirect(result.RedirectURL);
        //        else
        //            return RedirectPermanent(result.RedirectURL);
        //    }
        //    catch (Exception)
        //    {
        //        return NotFound();
        //    }
        //}



        //[Authorize]
        [MiddlewareFilter(typeof(HeaderAuthorizationPipeline))]
        [HttpPost]
        public async Task<JsonResult> Post([FromBody]URLModel value)
        {
           
            if (ModelState.IsValid)
            {
                string accountID = "";
                try
                {
                    string authHeader = HttpContext.Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        bool isHeaderValid = HeaderAuthorizationMiddleware.ValidateCredentials(authHeader, _context);
                        if (isHeaderValid)
                        {
                            var user = _context.Users.SingleOrDefault(s => s.Token == authHeader);
                            _model.AccountId = user.AccountId;
                            accountID = user.AccountId;
                        }
                        else
                        {
                            var jsonResult = new { error = "Invalid user!" };
                            return new JsonResult(jsonResult);
                        }

                    }
                    else
                    {
                        var jsonResult = new { error = "Unautorized user!" };
                        return new JsonResult(jsonResult);
                    }
                }
                catch (Exception e)
                {
                    var jsonResult = new { error = "Authorization error! " + e.Message };
                    return new JsonResult(jsonResult);
                }



                value.RedirectURL = CheckURL.FormatUrl(value.RedirectURL, true);
                if (value.RedirectURL.Length > 5)
                {
                    var ExistingURL =_context.URL.SingleOrDefault(s => s.RedirectURL == value.RedirectURL && s.AccountId == accountID );
                    if (ExistingURL != null)
                    {
                        var jsonResult = new { shortUrl = ExistingURL.ShortCode };
                        return new JsonResult(jsonResult);
                    }

                    var i = await _check.Check(value.RedirectURL);
                    if (i == true)
                    {
                        try
                        {
                            int nextId = 1;
               
                            int? next = _context.URL.Max(u => (int?)u.ID);
                            if (next != null)
                                nextId = (int)next + 1;

                            //_model.ShortCode = _settings.AppUrl + ShorterURL.Encode(nextId);
                            _model.ShortCode = getPath(HttpContext) + ShorterURL.Encode(nextId);
                            _model.RedirectURL = value.RedirectURL;
                            _model.RedirectType = value.RedirectType;
                      
                            _context.URL.Add(_model);
                            if (_context.SaveChanges() > 0)
                            {
                                var jsonResult = new { shortUrl = _model.ShortCode };
                                return new JsonResult(jsonResult);
                            }
                            else
                            {
                                var jsonResult = new { error = "Save model error" };
                                return new JsonResult(jsonResult);
                            }
                        }
                        catch
                        {
                            var jsonResult = new { error = "Save model error" };
                            return new JsonResult(jsonResult);
                        }
                    }
                    else
                    {
                        var jsonResult = new { error = "URL Verification Error" };
                        return new JsonResult(jsonResult);                      
                    }
                }

                var jsonRErroresult = new { error = "Invalid or Nonexistent RedirectURL Error" };
                return new JsonResult(jsonRErroresult);
            }
            else
            {
                var jsonResult = new { error = "Invalid model Error" };
                return new JsonResult(jsonResult);
            }
        }

    }
}