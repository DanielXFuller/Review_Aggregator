using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Review_Aggregator
{
    internal class Program
    {
        static void Main(string[] args)
        {
     
            // Declare a variable to track whether the user wants to exit the program
            bool exitProgram = false;

            // Use a while loop to keep the program running until the user chooses to exit
            while (!exitProgram)
            {
                // Display the four options for the user, plus an option to exit the program
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1. Add a new Movie to the database");
                Console.WriteLine("2. Read Movies from the database");
                Console.WriteLine("3. Update existing Movie in the database");
                Console.WriteLine("4. Delete Movie from the database");
                Console.WriteLine("5. Exit the program");


                Console.Write("Enter the number of the action you would like to perform: ");

                if (int.TryParse(Console.ReadLine(), out int linkNumber))
                {
                    switch (linkNumber)
                    {
                        case 1:
                            Create();
                            break;
                        case 2:
                            Read();
                            break;
                        case 3:
                            Console.WriteLine("Enter the ID of the movie to edit:");
                            int id = int.Parse(Console.ReadLine());
                            Update(id);
                            break;
                        case 4:
                            Console.WriteLine("Enter the ID of the movie to Delete:");
                            id = int.Parse(Console.ReadLine());
                            Delete(id);
                            break;
                        case 5:
                            exitProgram = true;
                            Console.WriteLine("Exiting the program...");
                            break;
                        default:
                            Console.WriteLine("Invalid option selected. Please choose a number between 1 and 5.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input entered. Please enter a number between 1 and 5.");
                }

                // Add a blank line for spacing
                Console.WriteLine();
            }

            // Once the while loop exits, print a message to indicate that the program has ended
            Console.WriteLine("Program has ended.");





        }

        public static void Create()
        {
            using (MoviesDbContext db = new MoviesDbContext())
            {
                db.Database.EnsureCreated();

                Console.Write("Enter the name of the movie: ");
                string movieName = Console.ReadLine();
                Console.Write("Enter the name of the website (IMDb or Rotten Tomatoes): ");
                string websiteName = Console.ReadLine();

                ChromeOptions options = new ChromeOptions();
                options.AddExcludedArgument("enable-logging");

                string chromedriverPath = @"C:\path\to\chromedriver.exe";

                ChromeDriverService service = ChromeDriverService.CreateDefaultService(chromedriverPath);
                ChromeDriver driver = new ChromeDriver(service, options);

                try
                {
                    // Search for movie URL on the specified website
                    string url = GetMovieUrl(movieName, websiteName, driver);

                    // Extract structured data from movie page and output to console
                    if (url != null)
                    {
                        Movie movie = GetStructuredDataJson(url, websiteName, driver);
                        int index = 0;
                        foreach (var review in movie.Reviews)
                        {
                            movie.Reviews[index].Movie = movie;
                            db.Reviews.Add(movie.Reviews[index]);
                            index++;
                        }
                        db.Movies.Add(movie);
                    }
                    else
                    {
                        Console.WriteLine($"No results found for {movieName} on {websiteName}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    driver.Quit();
                    db.SaveChanges();
                }
            }
        }


        public static void Read() 
        {
            using (MoviesDbContext db = new MoviesDbContext())
            {
                // Ask the user whether to read all movies or search for a movie
                Console.WriteLine("1. Read All Movies");
                Console.WriteLine("2. Search for specific Movie/s");
                Console.Write("Enter the number of the action you would like to perform: ");

                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        // Retrieve all movies from the database
                        var movies = db.Movies.Include(a => a.Reviews).ToList();
                        //Console.WriteLine(check);

                        if (movies.Any())
                        {
                            // Display all movies to the user
                            Console.WriteLine("List of Movies:");
                            foreach (var movie in movies)
                            {
                                Console.WriteLine($"ID: {movie.Id}, Title: {movie.Title}, Description: {movie.Description}");
                                Console.WriteLine("Current reviews:");
                                foreach (var review in movie.Reviews)
                                {
                                    Console.WriteLine($"Source: {review.Source}, Rating: {review.Rating}, Website: {review.Website}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No movies found.");
                        }
                        break;

                    case 2:
                        // Ask the user whether to search by ID or title                                                  
                        Console.WriteLine("1. Search By Id");
                        Console.WriteLine("2. Search By Title");
                        Console.Write("Enter the number of the action you would like to perform: ");
                        int searchChoice = int.Parse(Console.ReadLine());

                        switch (searchChoice)
                        {
                            case 1:
                                // Search by movie ID
                                Console.WriteLine("Enter movie ID:");
                                int id = int.Parse(Console.ReadLine());

                                var movie = db.Movies.Find(id);
                                var reviews = db.Reviews.Where(a => a.MovieId == id);
                                if (movie != null)
                                {
                                    Console.WriteLine($"ID: {movie.Id}, Title: {movie.Title}, Description: {movie.Description}");
                                    Console.WriteLine("Current reviews:");
                                    foreach (var review in reviews)
                                    {   
                                        Console.WriteLine($"Source: {review.Source}, Rating: {review.Rating}, Website: {review.Website}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Movie not found.");
                                }
                                break;

                            case 2:
                                // Search by movie title
                                Console.WriteLine("Enter search string:");
                                string searchString = Console.ReadLine();

                                var searchResults = db.Movies.Where(m => m.Title.Contains(searchString)).ToList();

                                if (searchResults.Any())
                                {
                                    // Display search results to the user
                                    Console.WriteLine($"Search results for '{searchString}':");
                                    foreach (var result in searchResults)
                                    {
                                        var reviews2 = db.Reviews.Where(a => a.MovieId == result.Id);
                                        Console.WriteLine($"ID: {result.Id}, Title: {result.Title}, Description: {result.Description}");
                                        Console.WriteLine("Current reviews:");
                                        foreach (var review in reviews2)
                                        {
                                            Console.WriteLine($"Source: {review.Source}, Rating: {review.Rating}, Website: {review.Website}");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No movies found.");
                                }
                                break;

                            default:
                                Console.WriteLine("Invalid choice.");
                                break;
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        public static void Update(int id)
        {
            using (MoviesDbContext db = new MoviesDbContext())
            {

                // Find the movie with the specified ID
                var movie = db.Movies.Include(a => a.Reviews).First(a => a.Id == id);


                if (movie == null)
                {
                    // If the movie is not found, notify the user and exit the method
                    Console.WriteLine("Movie not found.");
                    return;
                }

                // Display the current movie information to the user
                Console.WriteLine($"Current title: {movie.Title}");
                Console.WriteLine($"Current description: {movie.Description}");
                Console.WriteLine($"Current image URL: {movie.ImageUrl}");

                // Ask the user to enter the new values for the movie
                Console.WriteLine("Enter new title (leave blank to keep current value):");
                string newTitle = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newTitle))
                {
                    movie.Title = newTitle;
                }

                Console.WriteLine("Enter new description (leave blank to keep current value):");
                string newDescription = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newDescription))
                {
                    movie.Description = newDescription;
                }

                Console.WriteLine("Enter new image URL (leave blank to keep current value):");
                string newImageUrl = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newImageUrl))
                {
                    movie.ImageUrl = newImageUrl;
                }

                Console.WriteLine("Current reviews:");
                foreach (var review in movie.Reviews)
                {
                    Console.WriteLine($"Id: {review.Id} Source: {review.Source}, Rating: {review.Rating}, Website: {review.Website}");
                }

                Console.WriteLine("1. Add A Review");
                Console.WriteLine("2. Edit A Review");
                Console.WriteLine("3. Delete A Review");
                Console.WriteLine("4. Exit");
                Console.Write("Enter the number of the action you would like to perform: ");


                if (int.TryParse(Console.ReadLine(), out int option))
                {
                    switch (option)
                    {
                        case 1:
                            // Add a new review for the movie
                            Console.WriteLine("Enter the source of the review:");
                            string source = Console.ReadLine();

                            Console.WriteLine("Enter the rating for the review:");
                            decimal rating = decimal.Parse(Console.ReadLine());

                            Console.WriteLine("Enter the website of the review:");
                            string website = Console.ReadLine();

                            var newReview = new Review { Source = source, Rating = rating, Website = website, Movie = movie };
                            movie.Reviews.Add(newReview);
                            break;

                        case 2:
                            Console.WriteLine("Current reviews:");
                            foreach (var review in movie.Reviews)
                            {
                                Console.WriteLine($"Id: {review.Id}, Source: {review.Source}, Rating: {review.Rating}, Website: {review.Website}");
                            }

                            Console.WriteLine("Enter the Id of the review you want to edit:");
                            int reviewId = int.Parse(Console.ReadLine());

                            var reviewToEdit = movie.Reviews.FirstOrDefault(r => r.Id == reviewId);

                            if (reviewToEdit != null)
                            {
                                // Display the details of the review
                                Console.WriteLine($"Current details for review {reviewId}:");
                                Console.WriteLine($"Source: {reviewToEdit.Source}, Rating: {reviewToEdit.Rating}, Website: {reviewToEdit.Website}");

                                // Ask the user for the updated details
                                Console.WriteLine("Enter the updated source of the review (press enter to keep the current value):");
                                string newSource = Console.ReadLine();
                                if (!string.IsNullOrEmpty(newSource))
                                {
                                    reviewToEdit.Source = newSource;
                                }

                                Console.WriteLine("Enter the updated rating for the review (press enter to keep the current value):");
                                string newRatingString = Console.ReadLine();
                                if (!string.IsNullOrEmpty(newRatingString))
                                {
                                    decimal newRating = decimal.Parse(newRatingString);
                                    reviewToEdit.Rating = newRating;
                                }

                                Console.WriteLine("Enter the updated website of the review (press enter to keep the current value):");
                                string newWebsite = Console.ReadLine();
                                if (!string.IsNullOrEmpty(newWebsite))
                                {
                                    reviewToEdit.Website = newWebsite;
                                }

                                Console.WriteLine("Review updated successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"Review with Id {reviewId} not found.");
                            }
                            break;

                        case 3:
                            Console.WriteLine("Enter the Id of the review you want to Delete:");
                            reviewId = int.Parse(Console.ReadLine());

                            var reviewToDelete = movie.Reviews.FirstOrDefault(r => r.Id == reviewId);

                            if (reviewToDelete != null)
                            {
                                movie.Reviews.Remove(reviewToDelete);
                                Console.WriteLine($"Review with Id {reviewId} has been deleted.");
                            }
                            else
                            {
                                Console.WriteLine($"Review with Id {reviewId} does not exist.");
                            }
                            break;

                        case 4:
                            break;

                        default:
                            Console.WriteLine("Invalid option selected. Please choose a number between 1 and 4.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input entered. Please enter a number between 1 and 4.");
                }

                db.SaveChanges();

            }
        }

        public static void Delete(int id)
        {
            using (MoviesDbContext db = new MoviesDbContext())
            {
                object item = db.Movies.Include(m => m.Reviews).FirstOrDefault(m => m.Id == id);

                if (item == null)
                {
                    Console.WriteLine($"Item with Id {id} not found.");
                    return;
                }

                if (item is Movie movieToDelete)
                {
                    Console.WriteLine($"Found movie: Id: {movieToDelete.Id} Title: {movieToDelete.Title} Description: {movieToDelete.Description}");
                }
                else if (item is Review reviewToDelete)
                {
                    Console.WriteLine($"Found review for movie {reviewToDelete.Movie.Title}: Source: {reviewToDelete.Source}, Rating: {reviewToDelete.Rating}, Id: {reviewToDelete.Id}");
                }

                Console.WriteLine("Are you sure you want to delete this item? (y/n)");
                string confirmation = Console.ReadLine();
                if (confirmation.ToLower() == "y")
                {
                    // Remove the item from the database and save changes
                    db.Remove(item);
                    db.SaveChanges();
                    Console.WriteLine("Item deleted successfully.");
                } else 
                {
                    return;
                }
            }
        }


        public static string GetMovieUrl(string movieName, string websiteName, ChromeDriver driver)
        {

            string website;
            string url;
            if (websiteName.ToLower() == "imdb")
            {
                website = "imdb";
            }
            else if (websiteName.ToLower() == "rotten tomatoes")
            {
                website = "rotten tomatoes";
            }
            else
            {
                throw new ArgumentException($"Website name {websiteName} is not supported.");
            }
            string baseUrl = "http://www.google.com/search?q=";
            driver.Navigate().GoToUrl(baseUrl + website + " " + movieName);



            IList<IWebElement> titles = driver.FindElements(By.CssSelector("h3.LC20lb.MBeuO.DKV0Md"));
            var length = 0;

            for (int i = 0; i < titles.Count; i++)
            {
                if (string.IsNullOrEmpty(titles[i].Text))
                {
                    break;
                }

                Console.WriteLine((i + 1) + ". " + titles[i].Text);
                length += 1;
            }

            IList<IWebElement> links = driver.FindElements(By.CssSelector("div.yuRUbf a"));
            if (links.Count > length)
            {
                links = links.Take(length).ToList();
            }

            while (true)
            {
                Console.Write("Enter the number of the link you want to select: ");
                if (int.TryParse(Console.ReadLine(), out int linkNumber))
                {
                    if (linkNumber > 0 && linkNumber <= links.Count)
                    {
                        url = links[linkNumber - 1].GetAttribute("href");
                        return url;
                    }
                }
                Console.WriteLine("Invalid link number. Please try again.");
            }

        }

        public static Movie GetStructuredDataJson(string url, string website, ChromeDriver driver)
        {
            decimal multi;
            if (website.ToLower() == "imdb")
            {
                website = "imdb";
                multi = 1;
            }
            else if (website.ToLower() == "rotten tomatoes")
            {
                website = "rotten tomatoes";
                multi = 0.1m;
            } else
            {
                throw new NotSupportedException("Website Invalid");
            }
            driver.Navigate().GoToUrl(url);
            var obj = driver.ExecuteScript("return document.querySelector('script[type=\"application/ld+json\"]').text");
            var obj2 = System.Text.Json.JsonSerializer.Deserialize<JsonNode>(obj.ToString());
            var reviewObject = obj2["aggregateRating"]["ratingValue"].ToString();
            decimal review;
            review = decimal.Parse(reviewObject);
            review = review * multi;
            string description = obj2["description"]?.ToString() ?? obj2["name"].ToString();
            description = WebUtility.HtmlDecode(description);

            return new Movie
            {
                Title = obj2["name"].ToString(),
                Description = description,
                ImageUrl = obj2["image"].ToString(),
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Rating = review,
                        Source = url,
                        Website = website
                    }
                }
            };
        }
    }
}