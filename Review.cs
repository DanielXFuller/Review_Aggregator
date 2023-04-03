using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Review_Aggregator
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Movie))]
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string Website { get; set; }
        public string Source { get; set; }
        public decimal Rating { get; set; }

    }
}