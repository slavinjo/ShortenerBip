using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShortenerBip.Helper;
using ShortenerBip.Middleware;
using ShortenerBip.Models;

namespace ShortenerBip.Controllers
{
    [Produces("application/json")]
    [Route("Statistic")]
    public class StatisticController : Controller
    {

        private DataContext _context;

        public StatisticController(DataContext context)
        {
            _context = context;
            
        }

        [MiddlewareFilter(typeof(HeaderAuthorizationPipeline))]
        [HttpGet("{id}")]
        public IActionResult Get(String accountId)
        {          
            try
            {
                string authHeader = HttpContext.Request.Headers["Authorization"]; 
                if (!string.IsNullOrEmpty(authHeader))
                {
                    bool isHeaderValid = HeaderAuthorizationMiddleware.ValidateCredentials(authHeader, _context);
                    if (isHeaderValid)
                    {
                        var user = _context.Users.SingleOrDefault(s => s.Token == authHeader);

                        //List<Stats> stats = _context.Stats.Where(x => x.URLModel.AccountId == accountId).ToList();


                        //foreach(Stats s in stats)
                        //{
                        //    var jsonResult = new { s.URLModel.RedirectURL, s.HitCount };                            
                        //}
                        var itemList = (from items in _context.Stats where items.URLModel.AccountId.Equals(accountId) select new {  items.URLModel.RedirectURL, value = items.HitCount}).ToList();

                        return Json(itemList);

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
                var jsonResult = new { error = "Authorization error!" };
                return new JsonResult(jsonResult);
            }


           
        }

    }
}