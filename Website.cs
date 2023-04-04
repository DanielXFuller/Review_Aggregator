using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Review_Aggregator
{
    public class Website
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Multiplier { get; set; }
    }
}
