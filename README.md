# C# Mongo QueryBuilder

The intent of this repo is to provide a simple way to define Mongo DAL "conventions" in C#, which can be used to construct queries directly and fluently. I am optimizing for sanity of the developer interface.`MongoQueryBuilder` makes use of `Castle.Core` Proxies to accomplish this goal.

# What do I have to do?

### Step One: Define some Conventions

To define a convention, you must implement the following interface:

```csharp
public interface IQueryBuilderMethodConvention
{
    bool Matches(Type type, MethodInfo method);
    IMongoQuery GenerateQueryComponent(IInvocation invocation);
    UpdateBuilder GenerateUpdateComponent(IInvocation invocation);
}
```

`bool Matches(Type type, MethodInfo method)` is a function that will be called to validate that the method you defined in your QueryBuilder interface is actually valid for the DAL model type. For example, a `By{PropertyName}` convention must validate that there exists that property name in the DAL model. It may also do type checking, or anything else you want to do.

Here is an example definition for `By{PropertyName}` convention that I farted out.

```csharp
public class ByConvention : IQueryBuilderMethodConvention
{
    public Func<Type, MethodInfo, bool>[] Criteria =
    {
      (t,m) => m.Name.StartsWith("By"),
      (t,m) => m.GetParameters().Length == 1,
      // Make sure the DAL model has a property with that name
      (t,m) => t.GetProperties()
          .Any(p => p.Name == ExtractPropertyName(m.Name)),
      // Type check the QueryBuilder parameter with the DAL model property type
      (t,m) => t.GetProperties()
        .First(p => p.Name == ExtractPropertyName(m.Name))
        .PropertyType == m.GetParameters().First().ParameterType
    };

    public bool Matches(Type type, MethodInfo method)
    {
        return this.Criteria.All(i => i(type, method));
    }

    public IMongoQuery GenerateQueryComponent(IInvocation invocation)
    {
        return Query.EQ(
            ByConvention.ExtractPropertyName(invocation.Method.Name),
            BsonValue.Create(invocation.Arguments[0]));
    }
    public UpdateBuilder GenerateUpdateComponent(IInvocation invocation)
    {
        return null;
    }
    public static string ExtractPropertyName(string methodName)
    {
        return Regex.Replace(methodName, "^By", "");
    }
}

```

### Okay, I have a convention. How I mine for data?

Most of the hard work should be done at this point. Celebrate. Now all you need to do is define your DAL model, and your QueryBuilder interface (NOT an implementation to go along with it! :D)

Here's an example model:

```csharp
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

And here's an example QueryBuilder interface:

```csharp
public interface ICompanyQueryBuilder : IQueryBuilder<Company>
{
    ICompanyQueryBuilder ById(int id);
    ICompanyQueryBuilder ByName(string name);
    ICompanyQueryBuilder ByEmail(string email);
}
```

### Ta da!

Of course, this is a full-fledged repository. So you'll have to do a little bit of configuration -- but hopefully this boilerplate is stuffed into Ninject or Unity configuration (instead of a static class, like I use in the Tests project).

```csharp
public static IRepository<Company,ICompanyQueryBuilder> CreateRepo(RepositoryConfiguration config = null)
{
  var provider = new StandardRepositoryProvider();
  return provider.CreateRepository<Company, ICompanyQueryBuilder>(
      config ?? new RepositoryConfiguration
      {
          CollectionName = "companies",
          DatabaseName = "testdata",
          ConnectionString = "mongodb://localhost",
          SafeModeSetting = SafeMode.True
      }, typeof(ICompanyQueryBuilder), typeof(ByConvention));
}
```

That's right -- did you catch that? You must inject the types of your `QueryBuilder<T>` and Conventions into the `CreateRepository` method. This should not be a problem. You can also inject `Assembly` instances, and we'll happily crawl them for appropriate types for you. The support for individual types is just hyper-useful for testing.

### And, we're off!

Seriously, that's some boilerplate. But let's start using it!

```csharp
var repo = CompanyRepo.CreateRepo();
repo.Collection.Drop();
repo.Save(new Company
{
    Id = 1,
    Name = "Test One",
    Email = "test@one.com"
});
Assert.AreEqual(1, repo.Builder()
  .ByEmail("test@one.com")
  .ByName("Test One")
  .GetAll()
  .Count);
```

### But I like IQueryable<T>! 

I figured. Fortunately, your friendly neighborhood squirrel hears your pleas! Seriously though, it's a pretty solid requirement to do aggregation operations in the DB query.

```csharp
// Why not expose it directly?
repo.Queryable(q => q.Count()); 

// Why not chain it off the QueryBuilder stuff?
repo.Builder()
  .ByName("foo")
  .Queryable(q => q.Count())
  
// And don't worry, we preserve the good-old Mongo exceptions, instead of TargetInvocationException nonsense
Assert.Throws<NotSupportedException>(() => repo.Builder()
  .Queryable(q => q
      .Where(i => i.Name.Split(',').Count() == 0))
  .Count());
```

### Okay. I'm almost convinced. But I need more features!

So I have a use case where, because AWS is AWS, pretty much every database call needs to be wrapped in some kind of Retry semantics. But I didn't want to complect this library to my Retry library of choice (or my own implementation!). So I added a semi-hidden hook for those of you that need this support. Besides, you may not want to Retry at all, but you may want to have a simple way to log __every damn db call__

```csharp
public class RepositoryConfiguration
{
    public RepositoryConfiguration()
    {
        this.CustomWrapper = action => action();
    }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
    public SafeMode SafeModeSetting { get; set; }
    public Action<Action> CustomWrapper { get; set; }
}
```

This innocuous little `CustomWrapper` hook means you can provide a function that gets called every time a database call is intended to be made. The actual database call is the inner Action. A demonstration might make more sense...

```csharp
var wrapperCalls = 0;
Action<Action> wrapper = dbcall =>
{
    wrapperCalls++;
    dbcall();
    dbcall();
};
var repo = CompanyRepo.CreateRepo(new RepositoryConfiguration
{
    CollectionName = "companies",
    DatabaseName = "testdata",
    ConnectionString = "mongodb://localhost",
    SafeModeSetting = SafeMode.True,
    CustomWrapper = wrapper
});
var queryableCalls = 0;
var enumerableCalls = 0;
repo
    .Builder()
    .ByName("bar")
    .Queryable(c =>
    {
        queryableCalls++;
        return c;
    })
    .Where(i => i.Id == 1)
    .Select(i =>
    {
        enumerableCalls++;
        return i;
    })
    .ToList();
Assert.AreEqual(1, wrapperCalls);
Assert.AreEqual(2, queryableCalls);
Assert.AreEqual(1, enumerableCalls);
```

This has the fancy little side-effect that you can now easily segregate your database filtering from your in-memory filtering. You may not care, but having written several DALs myself, love this bacon.

### Okay, but this is slow, right?

Not as bad as you might think. Yes, it's Reflection, it's Proxies, and by God it's enough magic to make most developers insta-wretch. But it's not that bad. Most everything is computed ahead of time and cached. All the method matching is done at bootstrap-time (rather than query execution time). I tried my mostly hardest to not place any obvious performance hits inside this codebase.

There is a sanity test that keeps me honest here. It runs faster than 10ms total per database call (400ms total on my machine). If it turns out this thing is way slow for you, let me know! Just be sure to attach enough information for me to fix it -- or submit the fix yourself in a PR. DALs are typically somewhere you cannot afford slowness.

```csharp
[TestFixture]
public class PerformanceTest
{
  [Test]
  public void ItQueriesAHundredTimesInLessThanASecond()
  {
      var repo = CompanyRepo.CreateRepo();
  
      repo.Collection.Drop();
      repo.Save(new Company
      {
          Id = 1,
          Name = "bar"
      });
      repo.Save(new Company
      {
          Id = 2,
          Name = "foo",
      });
      repo.Save(new Company
      {
          Id = 3,
          Name = "bar",
      });
  
      var totalWatch = Stopwatch.StartNew();
  
      Enumerable.Range(0,100).ToList().ForEach(_ => 
          Assert.True(repo.Builder()
              .ByName("foo")
              .Queryable(q => q)
              .Any()));
  
      totalWatch.Stop();
      Assert.LessOrEqual(totalWatch.ElapsedMilliseconds, 1000, 
          "Either your machine is slow, or we need a different performance expectation.");
    }
  }
}
```

### I'm still not convinced.

`IRepository<T,K>` is threadsafe. Have fun.

### One last question: Why complect to Mongo Csharp 1.6.1?

After 1.6.1, the Mongo C# driver drops __heavily__ in performance. We observe insane database locking and 2x-10x response times in production when we upgrade, so we've frozen the version. I expect the Mongo C# team will address this.

That being said, this is MIT software. Fork and upgrade if you care that much, then box it up and sell it for thousands. Whatever.
