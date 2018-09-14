dotnet new mvc -n ConfigureSample -o ConfigureSample

# 001 ConfigureAppConfiguration

~~~ csharp
.ConfigureAppConfiguration((builderContext, config) =>
{
    var env = builderContext.HostingEnvironment;

    config.SetBasePath(env.ContentRootPath);
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
})
~~~

# 002 appsettings

~~~ json
,
"AppSettings": {
    "Foo": 123,
    "Bar": "Bar"
}
~~~

# 003 AppSettings

~~~ csharp
public class AppSettings
{
    public int Foo { get; set; }
    public string Bar { get; set; }
}
~~~

# 004 register config

~~~ csharp
services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
~~~

# 005 using the settings

~~~ csharp
private readonly AppSettings _options;

public HomeController(IOptions<AppSettings> options)
{
    _options = options.Value;
}

public IActionResult Index()
{
    ViewData["Message"] = _options.Bar;
    return View();
}
~~~
~~~ html
<div class="row">
    <div class="col-md-12">
        <h1>@ViewData["Title"]</h1>
        <h2>@ViewData["Message"]</h2>
    </div>
</div>
~~~

# 006 ini file

~~~ ini
[AppSettings]
Bar="FooBar"
~~~

