# Overview

This is a demonstration of how nesting db context scopes can lead to objects being returned by queries on the outer context being inconsistent with the query. Sample output:

```console
$ dotnet run -c Release
documentary title in inner context is Documentary
documentary title in outer context is Action Movie
```

This produced by executing the following code (relevant excerpt, for full example see `Program.cs`):

```csharp
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
```
