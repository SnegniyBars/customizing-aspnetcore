dotnet new mvc -n TagHelperSample -o TagHelperSample


# 001 GreeterTagHelper

~~~ csharp
public class GreeterTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public string Name { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.Content.SetContent($"Hello {Name}");
    }
}
~~~

# 002 register

@addTagHelper *, TagHelperSample

# 003 GenFu

# 004 services

~~~ csharp

public interface IService
{
    IEnumerable<Person> AllPersons();
}
internal class PersonService : IService
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

# 005 services in controller injecten

services.AddTransient<IService, PersonService>();


# 006 taghelper

~~~ csharp
public class PersonGridTagHelper : TagHelper
{
    [HtmlAttributeName("persons")]
    public IEnumerable<Person> Persons { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "table";
        output.Attributes.Add("class", "table");
        output.Content.AppendHtml("<tr>");
        output.Content.AppendHtml("<th>First name</th>");
        output.Content.AppendHtml("<th>Last name</th>");
        output.Content.AppendHtml("<th>Age</th>");
        output.Content.AppendHtml("<th>Email address</th>");
        output.Content.AppendHtml("</tr>");

        foreach (var person in Persons)
        {
            output.Content.AppendHtml("<tr>");
            output.Content.AppendHtml($"<td>{person.FirstName}</td>");
            output.Content.AppendHtml($"<td>{person.LastName}</td>");
            output.Content.AppendHtml($"<td>{person.Age}</td>");
            output.Content.AppendHtml($"<td>{person.EmailAddress}</td>");
            output.Content.AppendHtml("</tr>");
        }
    }
}
~~~

#007 tag helper benutzen

public class AboutViewModel
{
	public IEnumerable<Person> Persons { get; set; }
}
<person-grid persons="Model.Persons"></person-grid>
