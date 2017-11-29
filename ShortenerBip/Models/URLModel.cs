using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Models
{
    public class URLModel
    {       
        [Key]
        public int ID { get; set; }
        public string ShortCode { get; set; }
        [Required]
        public string RedirectURL { get; set; }
        public int RedirectType { get; set; } = 302;
        public string AccountId { get; set; }

    }
}
