﻿using System;
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
        public async Task<IActionResult> Post([FromBody]User userDto)
        {
            // map dto to entity
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<User>(userDto);
                return await SaveUser(user);
            }
            else
            {
                var jsonResult = new { Success = false, Description = "Invalid data." };
                return new JsonResult(jsonResult);
            }
        }

        ////https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
        //[AllowAnonymous]
        //[HttpPost("register")]
        //public IActionResult RegisterModel(User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return SaveUser(user);
        //    }
        //    return NotFound();
        //}

        private async Task<JsonResult> SaveUser(User userModel)
        {
            try
            {
                var user = Json(userModel);
                //save
                //  regUser = _userService.Create(userModel);
                String password = Password.GeneratePassword(8, 0);
                userModel.Password = password;
                userModel.UserName = userModel.AccountId;
                userModel.Token = userModel.Password;

                ObjectResult rez = await Authenticate(userModel); /*todo*/
                if (rez.StatusCode != 400)
                {
                    var jsonResultOK = new { Success = true, /*AccountId = regUser.AccountId,*/ Password = userModel.Password, Description = "Your account is opened." };
                    return new JsonResult(jsonResultOK);
                }

                //return rez;
                //return RedirectToAction("UserRegistered", "Account", userModel);
                var jsonResult = new { Success = false, Description = rez.Value.ToString() };
                return new JsonResult(jsonResult);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                var jsonResult = new { Success = false, /*AccountId = regUser.AccountId, Password = regUser.Password,*/ Description = ex.Message };
                return new JsonResult(jsonResult);
            }
        }

        private async Task<IActionResult> SaveUserResult(User userModel)
        {
            try
            {
                //regUser = _userService.Create(userModel); /*todo*/
                //await Authenticate(regUser); /*todo*/
                String password = Password.GeneratePassword(8, 0);
                userModel.Password = password;
                userModel.UserName = userModel.AccountId;
                userModel.Token = userModel.Password;
                var result = await Authenticate(userModel);
                if (result.StatusCode != 400)
                    return Redirect("/Home/UserScreen/?" + userModel.Token);
                else
                    return View(userModel);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return NotFound("Error" + ex.Message);
            }
        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ObjectResult> Authenticate([FromBody]User userDto)
        {
            //var user = _userService.Authenticate(userDto.AccountId, userDto.Password);

            //if (user == null)
            //    return Unauthorized();

            try
            {
                // var result = await _userManager.CreateAsync(user); /*todo*/
                var result = await _userManager.CreateAsync(userDto);

                if (result.Succeeded)
                {
                    //await _signInManager.SignInAsync(userDto, false);
                    await _signInManager.SignInAsync(userDto, false);
                }
                else
                {
                    //throw new ApplicationException("Autentifikacija nije uspjela!");
                    return BadRequest(result.Errors.ElementAt(0).Description);
                }
            }
            catch (Exception e)
            {
                //throw new ApplicationException("Autentifikacija nije uspjela!");
                return BadRequest(e.Message);
            }


            //return Ok(new
            //{
            //    Id = user.Id,
            //    Username = user.AccountId,
            //    Password = user.Password,
            //    Token = user.Password
            //});

            return Ok(new
            {
                Id = userDto.Id,
                Username = userDto.AccountId,
                Password = userDto.Password,
                Token = userDto.Password
            });
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register(User model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                return await SaveUserResult(model);           
            }
            return View();
        }

        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }


    }
        
}