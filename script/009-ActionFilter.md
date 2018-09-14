dotnet new mvc -n ActionFilterSample -o ActionFilterSample

# 001 Sample

~~~ csharp
public class SampleActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // do something before the action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // do something after the action executes
    }
}

public class SampleAsyncActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // do something before the action executes
        var resultContext = await next();
        // do something after the action executes; resultContext.Result will be set
    }
}

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
~~~

~~~ csharp

.AddMvcOptions(options =>
{
	options.Filters.Add(new SampleActionFilter());
	options.Filters.Add(new SampleAsyncActionFilter());
});
~~~

# 002 Person

~~~ csharp
public class Person
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}
~~~

# 003 API controller

~~~ csharp
using Microsoft.AspNetCore.Mvc;

namespace OutputFormatterSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
    }
}
~~~


# 004 Controller Action

~~~ csharp
[HttpPost]
[ValidateModel]
public ActionResult<Person> Post([FromBody] Person model)
{
	return model;
}
~~~

# 005 JSON
{
	"FirstName":"",
	"LastName": "",
	"EmailAddress": ""
}