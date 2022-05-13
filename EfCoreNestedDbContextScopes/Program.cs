using Microsoft.EntityFrameworkCore;

using var outerContext = new MovieDbContext();

outerContext.Database.EnsureDeleted();
outerContext.Database.EnsureCreated();

outerContext.Movies.Add(new("Action Movie"));
outerContext.SaveChanges();

using (var innerContext = new MovieDbContext())
{
    innerContext.Movies.Single().Title = "Documentary";
    innerContext.SaveChanges();
}

var documentary = outerContext.Movies.Single(movie => movie.Title == "Documentary");
Console.WriteLine($"{nameof(documentary)} title is {documentary.Title}");

class MovieDbContext : DbContext
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=movies.db");
    }
}

class Movie
{
    public int Id { get; set; }

    public string Title { get; set; }

    public Movie(string title) => Title = title;
}