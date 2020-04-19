# MongoQuerryBuilder


this project allows .net developers to build filter definition,sort definition and pagination from the http query parameters.
in this project we are used [Microsoft REST API Guidelines] to build queries for [Mongodbdriver]


# How to use
add the Builder class in your IOC container
  ```csharp
  
  services.AddScoped(typeof(QuerryBuilder<>));
  
```

Inject the builder in your class

  ```csharp
  
public YourClass(QuerryBuilder<T> querryBuilder)
{
    this.querryBuilder = querryBuilder;
}
// T is the entity related to your collection
  

```

You can also instantiate it manually

```csharp

var queryBuilder = new QuerryBuilder<YourEntity>(httpContextAccessor)

// httpContextAccessor is of type IHttpContextAccessor

```
if you want to use the builder

 ```csharp
querryBuilder.build();

```

**querryBuilder.Sort** contain a value of type [SortDefintion]

**querryBuilder.Filter** contain a value of type [FilterDefinition]

**querryBuilder.Top** contain the take value

**querryBuilder.Skip** contain the skip value


# example


```csharp
collection.Find(querryBuilder.Filter).
Sort(querryBuilder.Sort).
Skip(querryBuilder.Skip).
Limit(querryBuilder.Top);

```

   [Microsoft REST API Guidelines]: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#96-sorting-collections
   [mongodbDriver]: https://mongodb.github.io/mongo-csharp-driver/2.10/reference/driver/definitions/
   [SortDefintion]: https://mongodb.github.io/mongo-csharp-driver/2.10/reference/driver/definitions/#sort-definition-builder
   [FilterDefinition]: https://mongodb.github.io/mongo-csharp-driver/2.10/reference/driver/definitions/#filters

