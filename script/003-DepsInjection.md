dotnet new mvc -n DiSample -o DiSample

# 001 Service

~~~ csharp
public interface IService
{
    IEnumerable<Person> AllPersons();
}
internal class MyService : IService
{
    public IEnumerable<Person> AllPersons()
    {
        return A.ListOf<Person>(25);
    }
}
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string EmailAddress { get; set; }
}
~~~

# 002 Controller

~~~ csharp
private IService _service;

public HomeController(IService service)
{
    _service = service;
}
~~~

# 003 Service

~~~ razor
<ul>
    @foreach(var person in Model)
    {
        <li>
            @person.FirstName @person.LastName (@person.Age)<br />
            @person.EmailAddress<br />
        </li>
    }
</ul>
~~~

# 004 Action

~~~ csharp
var persons = _service.AllPersons();
return View(persons);
~~~

# 005 IServiceProvider

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    return services.BuildServiceProvider();
}

~~~

# 006 nuget

Autofac.Extensions.DependencyInjection

# 007 replacing

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // create a Autofac container builder
    var builder = new ContainerBuilder();

    // read service collection to Autofac
    builder.Populate(services);

    // use and configure Autofac
    //builder.RegisterType<MyService>().As<IService>();

    // build the Autofac container
    ApplicationContainer = builder.Build();

    // creating the IServiceProvider out of the Autofac container
    return new AutofacServiceProvider(ApplicationContainer);
}

// IContainer instance in the Startup class 
public IContainer ApplicationContainer { get; private set; }
~~~