using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Review_Aggregator
{
    internal class MoviesDbContext:DbContext
    {
        public DbSet<Movie> Movies { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Website> Websites { get; set; }


        public MoviesDbContext() : base(new DbContextOptionsBuilder<MoviesDbContext>().UseSqlite("Data Source=Movies.db").Options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>().HasIndex(a => a.Title);

            modelBuilder.Entity<Review>().HasIndex(a => a.Rating);

            modelBuilder.Entity<Review>().HasIndex(a => new { a.MovieId, a.Rating });



        }
    }
}
