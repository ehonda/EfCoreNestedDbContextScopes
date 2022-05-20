# Some experiments with parallel DB access

## Demo with undetected changes on read

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

## Demo with detected changes on write

This is sample output ran from the commit that introduced the `Timestamp` property

```console
$ dotnet run -c Release
documentary title in inner context is Documentary
documentary title in outer context is Action Movie
Unhandled exception. Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded. See http://go.microsoft.com/fwlink/?LinkId=527962 for information on understanding and handling optimistic concurrency exceptions.
   at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ThrowAggregateUpdateConcurrencyException(Int32 commandIndex, Int32 expectedRowsAffected, Int32 rowsAffected)
   at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetWithPropagation(Int32 commandIndex, RelationalDataReader reader)
   at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.Consume(RelationalDataReader reader)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.Execute(IRelationalConnection connection)
   at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.Execute(IEnumerable`1 commandBatches, IRelationalConnection connection)
   at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChanges(IList`1 entries)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChanges(IList`1 entriesToSave)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChanges(StateManager stateManager, Boolean acceptAllChangesOnSuccess)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<>c.<SaveChanges>b__104_0(DbContext _, ValueTuple`2 t)
   at Microsoft.EntityFrameworkCore.Storage.NonRetryingExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
   at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChanges(Boolean acceptAllChangesOnSuccess)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChanges(Boolean acceptAllChangesOnSuccess)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChanges()
   at Program.<Main>$(String[] args) in ...\Program.cs:line 52
```
