dotnet new mvc -n OutputFormatterSample -o OutputFormatterSample


# 001 Genfu / CsvHelper

# 002 API controller 

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

# 003 Person

~~~ csharp
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string EmailAddress { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
}
~~~

# 004 Action

~~~ csharp
// GET api/values
[HttpGet]
public ActionResult<IEnumerable<Person>> Get()
{
	var persons = A.ListOf<Person>(25);
	return persons;
}
~~~

# 005 formatters

~~~ csharp
services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.OutputFormatters.Add(new VcardOutputFormatter()); 
        options.OutputFormatters.Add(new CsvOutputFormatter()); 
        options.FormatterMappings.SetMediaTypeMappingForFormat(
            "vcard", MediaTypeHeaderValue.Parse("text/vcard"));
        options.FormatterMappings.SetMediaTypeMappingForFormat(
            "csv", MediaTypeHeaderValue.Parse("text/csv"));
    });
~~~

# 006 VcardOutputFormatter

~~~ csharp
public class VcardOutputFormatter : TextOutputFormatter
{
    public string ContentType { get; }

    public VcardOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/vcard"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    // optional, but makes sense to restrict to a specific condition
    protected override bool CanWriteType(Type type)
    {
        if (typeof(Person).IsAssignableFrom(type) 
            || typeof(IEnumerable<Person>).IsAssignableFrom(type))
        {
            return base.CanWriteType(type);
        }
        return false;
    }

    // this needs to be overwritten
    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var logger = serviceProvider.GetService(typeof(ILogger<VcardOutputFormatter>)) as ILogger;

        var response = context.HttpContext.Response;

        var buffer = new StringBuilder();
        if (context.Object is IEnumerable<Person>)
        {
            foreach (var person in context.Object as IEnumerable<Person>)
            {
                FormatVcard(buffer, person, logger);
            }
        }
        else
        {
            var person = context.Object as Person;
            FormatVcard(buffer, person, logger);
        }
        return response.WriteAsync(buffer.ToString());
    }

    private static void FormatVcard(StringBuilder buffer, Person person, ILogger logger)
    {
		buffer.AppendLine("BEGIN:VCARD");
		buffer.AppendLine("VERSION:2.1");
		buffer.AppendLine($"FN:{person.FirstName} {person.LastName}");
		buffer.AppendLine($"N:{person.LastName};{person.FirstName}");
		buffer.AppendLine($"EMAIL:{person.EmailAddress}");
		buffer.AppendLine($"TEL;TYPE=VOICE,HOME:{person.Phone}");
		buffer.AppendLine($"ADR;TYPE=home:;;{person.Address};{person.City}");            
		buffer.AppendLine($"UID:{person.Id}");
		buffer.AppendLine("END:VCARD");
		logger.LogInformation($"Writing {person.FirstName} {person.LastName}");
    }
}
~~~

# 007 CsvOutputFormatter

~~~ csharp
public class CsvOutputFormatter : TextOutputFormatter
{
    public string ContentType { get; }

    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    // optional, but makes sense to restrict to a specific condition
    protected override bool CanWriteType(Type type)
    {
        if (typeof(Person).IsAssignableFrom(type)
            || typeof(IEnumerable<Person>).IsAssignableFrom(type))
        {
            return base.CanWriteType(type);
        }
        return false;
    }

    // this needs to be overwritten
    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var logger = serviceProvider.GetService(typeof(ILogger<CsvOutputFormatter>)) as ILogger;

        var response = context.HttpContext.Response;

        var csv = new CsvWriter(new StreamWriter(response.Body));

        if (context.Object is IEnumerable<Person>)
        {
            var persons = context.Object as IEnumerable<Person>;
            csv.WriteRecords(persons);
        }
        else
        {
            var person = context.Object as Person;
            csv.WriteRecord<Person>(person);
        }

        return Task.CompletedTask;
    }
}
~~~
