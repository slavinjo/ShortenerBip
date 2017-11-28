using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Models
{
    public class Stats
    {
        [Key]
        public int ID { get; set; }
        public int HitCount { get; set; } = 0;
        public URLModel URLModel;
    }
}
