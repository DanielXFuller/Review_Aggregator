using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Review_Aggregator
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(450)]
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Review> Reviews { get; set; }
        public string ImageUrl { get; set; }
        //public override string ToString()
        //
        //    return System.Text.Json.JsonSerializer.Serialize(this);
        //}

    }
}
