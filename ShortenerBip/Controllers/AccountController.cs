using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShortenerBip.Models;
using ShortenerBip.Helper;
using ShortenerBip.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.Extensions.Options;

namespace ShortenerBip.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class AccountController : Controller
    {

        private IUserInterface _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        //private readonly IConfiguration _configuration;


        public AccountController(
        IUserInterface userService,
        IMapper mapper,
        IOptions<AppSettings> appSettings, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
        }


        //ovo se nemre overloadat, kaze covjek
        //https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
        //ovo je ako hocemo slati json, ja koristim form poziv dolje
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Post([FromBody]User userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);
            return SaveUser(user);
        }

        //https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult RegisterModel(User user)
        {
            return SaveUser(user);
        }

        private JsonResult SaveUser(User userModel)
        {
            User regUser = null;
            try
            {
                var user = Json(userModel);
                //save
                regUser = _userService.Create(userModel);
                Task<IActionResult> rez = Authenticate(regUser); /*todo*/
                //return rez;
                //return RedirectToAction("UserRegistered", "Account", userModel);
                var jsonResult = new { Success = true, /*AccountId = regUser.AccountId,*/ Password = regUser.Password, Description = "Your account is opened." };
                return new JsonResult(jsonResult);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                var jsonResult = new { Success = false, /*AccountId = regUser.AccountId, Password = regUser.Password,*/ Description = ex.Message };
                return new JsonResult(jsonResult);
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]User userDto)
        {
            var user = _userService.Authenticate(userDto.AccountId, userDto.Password);

            if (user == null)
                return Unauthorized();
            try
            {
                //var result = await _userManager.CreateAsync(user, user.AccountId);

                //if (result.Succeeded)
                //{
                    await _signInManager.SignInAsync(user, false);
                    
                    //return token;
                //}
                //else
                //{
                //    throw new ApplicationException("Autentifikacija nije uspjela!");
                //}
            }
            catch (Exception e)
            {
                throw new ApplicationException("Autentifikacija nije uspjela!");
            }


            return Ok(new
            {
                Id = user.Id,
                Username = user.AccountId,
                Password = user.Password,
                Token = user.Password
            });
        }



    }
}