# Overview

This is a demonstration of how nesting db context scopes can lead to objects being returned by queries on the outer context being inconsistent with the query. Sample output:

```console
$ dotnet run -c Release
documentary title in inner context is Documentary
documentary title in outer context is Action Movie
```
