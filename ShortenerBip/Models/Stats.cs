using ShortenerBip.Helper;
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
        [Key]
        public URLModel URLModel { get; set; }


        public static void SaveStats(URLModel model, DataContext _context)
        {
            Stats stats = _context.Stats.FirstOrDefault(x => x.URLModel == model);
            {
                if (stats == null)
                {
                    stats = new Stats
                    {
                        URLModel = model
                    };                    
                    stats.HitCount++;
                    _context.Stats.Add(stats);
                    _context.SaveChanges();
                }
                else
                {
                    stats.HitCount++;
                    _context.Update(stats);
                    _context.SaveChanges();
                }
            }
        }
    }
}
