using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShortenerBip.Models;
using ShortenerBip.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using ShortenerBip.Helper;
using Microsoft.Extensions.Options;
using ShortenerBip.Middleware;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace ShortenerBip.Controllers
{
    public class HomeController : Controller
    {

        private IUserInterface _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
      //  private User user;
        private DataContext _context;
        IHostingEnvironment _env;

        // public UserManager<User> UserManager => _userManager;

        //private readonly IConfiguration _configuration;


        public HomeController(IUserInterface userService, IMapper mapper,IOptions<AppSettings> appSettings, SignInManager<User> signInManager, UserManager<User> userManager, DataContext context, IHostingEnvironment e)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _env = e;
        }

        [HttpGet("{id}")]
        public IActionResult Index(String id)
        {
            try
            {
                var result = _context.URL.Where(x => x.ShortCode == _appSettings.AppUrl + id).FirstOrDefault();

                if (result == null)
                    return NotFound();

                if (result != null)
                    Stats.SaveStats(result, _context);

                if (result.RedirectType == 302)
                    return Redirect(result.RedirectURL);
                else
                    return RedirectPermanent(result.RedirectURL);
                
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }


        public IActionResult Index()
        {
            //return File(System.IO.File.OpenRead(Path.Combine(_env.WebRootPath + "/help.html")), "text/html");
            return View();
        }

        //[MiddlewareFilter(typeof(HeaderAuthorizationPipeline))]
        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        

        [Authorize]
        public IActionResult UserScreen(User model)
        {           
            try
            {
                //String password = Request.QueryString.Value;
                //user = _signInManager.UserManager.Users.SingleOrDefault(x => x.Password == password.Substring(1, password.Length - 1));
                String password = model.Password;
                User user = _signInManager.UserManager.Users.SingleOrDefault(x => x.Password == model.Password);
                //ViewData["Message"] = "Your application description page.";
                if (user == null)
                {
                   String usero = HttpContext.User.Claims.First().Value;
                   user = _signInManager.UserManager.Users.SingleOrDefault(x => x.Id == usero);
                }

     

                return View(user);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        public IActionResult Contact(User mod)
        {
            ViewData["Message"] = "Your contact page.";

            return View(mod);
        }

        [Route("help")]
        public IActionResult Help()
        {
            ViewData["Message"] = "Help";

            return View();
            //return File(System.IO.File.OpenRead(Path.Combine(_env.WebRootPath + "/help.html")), "text/html");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
