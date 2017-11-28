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

namespace ShortenerBip.Controllers
{
    public class HomeController : Controller
    {

        private IUserInterface _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private User user;
        private DataContext _context;

       // public UserManager<User> UserManager => _userManager;

        //private readonly IConfiguration _configuration;


        public HomeController(IUserInterface userService, IMapper mapper,IOptions<AppSettings> appSettings, SignInManager<User> signInManager, UserManager<User> userManager, DataContext context)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> IndexAsync(String id)
        {
            try
            {
                var result = _context.URL.Where(x => x.ShortCode == _appSettings.AppUrl + id).FirstOrDefault();

                if (result == null)
                    return NotFound();

                if (result != null)
                    SaveStats(result);

                if (result.RedirectType == 302)
                    return Redirect(result.RedirectURL);
                else
                    return RedirectPermanent(result.RedirectURL);
                
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        private void SaveStats(URLModel model)
        {
            Stats stats = _context.Stats.FirstOrDefault(x => x.URLModel == model);
            {
                if (stats == null)
                {
                    stats = new Stats();
                    stats.URLModel = model;
                    stats.HitCount++;
                    _context.Stats.Add(stats);
                    _context.SaveChanges();
                }
                else
                {
                    stats.HitCount++;
                    _context.Update(stats);
                }
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        [MiddlewareFilter(typeof(HeaderAuthorizationPipeline))]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult UserScreen()
        {
            try
            {
                String password = Request.QueryString.Value;
                user = _signInManager.UserManager.Users.SingleOrDefault(x => x.Password == password.Substring(1, password.Length - 1));
                //ViewData["Message"] = "Your application description page.";
                return View(user);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
