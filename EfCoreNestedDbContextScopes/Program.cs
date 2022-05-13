using Microsoft.EntityFrameworkCore;

using var outerContext = new MovieDbContext();

outerContext.Database.EnsureDeleted();
outerContext.Database.EnsureCreated();

outerContext.Movies.Add(new("Action Movie"));
outerContext.SaveChanges();

using (var innerWriteContext = new MovieDbContext())
{
    innerWriteContext.Movies.Single().Title = "Documentary";
    innerWriteContext.SaveChanges();
}

using (var innerReadContext = new MovieDbContext())
{
    var innerDocumentary = innerReadContext.Movies.Single(movie => movie.Title == "Documentary");
    Console.WriteLine($"documentary title in inner context is {innerDocumentary.Title}");
}

var outerDocumentary = outerContext.Movies.Single(movie => movie.Title == "Documentary");
Console.WriteLine($"documentary title in outer context is {outerDocumentary.Title}");

outerContext.Database.EnsureDeleted();

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
