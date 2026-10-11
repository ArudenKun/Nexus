using Dapper.SimpleSqlBuilder;

Console.WriteLine("Hello, World!");
var name = "testName";
var shorBio = "";
var sqlBuilder = SimpleBuilder
    .CreateFluent(reuseParameters: true)
    .Select($"*")
    .From($"Author")
    .WhereFilter($"Name = {name}")
    .WithFilter($"{shorBio} IS NOT NULL")
    .WithFilter($"{shorBio} <> ''");

Console.WriteLine(sqlBuilder.Sql);
