using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Models
{
    public class User : IdentityUser
    {
        //public int Id { get; set; }
        //public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string AccountId { get; set; }
    }
}
