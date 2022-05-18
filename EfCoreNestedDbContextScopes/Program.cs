using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using var outerContext = new MovieDbContext();

outerContext.Database.EnsureDeleted();
outerContext.Database.EnsureCreated();

// See - https://github.com/dotnet/efcore/issues/12260
//     - https://stackoverflow.com/questions/52684458/updating-entity-in-ef-core-application-with-sqlite-gives-dbupdateconcurrencyexce
//     - https://elanderson.net/2018/12/entity-framework-core-sqlite-concurrency-checks/
outerContext.Database.ExecuteSqlRaw(@"
CREATE TRIGGER SetMoviesTimestampOnInsert
AFTER INSERT ON Movies
BEGIN
    UPDATE Movies
    SET Timestamp = randomblob(8)
    WHERE rowid = NEW.rowid;
END
");

outerContext.Database.ExecuteSqlRaw(@"
CREATE TRIGGER SetMoviesTimestampOnUpdate
AFTER UPDATE ON Movies
BEGIN
    UPDATE Movies
    SET Timestamp = randomblob(8)
    WHERE rowid = NEW.rowid;
END
");


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

outerDocumentary.Title = "Thriller";
outerContext.SaveChanges();

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

    [Timestamp] public byte[]? Timestamp { get; set; }

    public Movie(string title) => Title = title;
}
