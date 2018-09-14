dotnet new mvc -n ModelBinderSample -o ModelBinderSample

# 001 CsvHelper

# 002 API controller generieren

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

# 004 Action

~~~ csharp
[HttpPost]
public ActionResult<object> Post([ModelBinder(binderType: typeof(PersonsCsvBinder))] IEnumerable<Person> persons)
{
	return new
	{
		ItemsRead = persons.Count(),
		Persons = persons
	};
}
~~~

# 005 Person

~~~ csharp
[ModelBinder(BinderType = typeof(PersonsCsvBinder))]
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

# 006 PersonsCsvBinder

~~~ csharp
public class PersonsCsvBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        // Specify a default argument name if none is set by ModelBinderAttribute
        var modelName = bindingContext.BinderModelName;
        if (String.IsNullOrEmpty(modelName))
        {
            modelName = "persons";
        }

        // Try to fetch the value of the argument by name
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        // Check if the argument value is null or empty
        if (String.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var stringReader = new StringReader(value);
        var reader = new CsvReader(stringReader);

        var model = reader.GetRecords<Person>().ToList();
        bindingContext.Result = ModelBindingResult.Success(model);

        return Task.CompletedTask;
    }
}
~~~

# 007 CSV
persons=Id,FirstName,LastName,Age,EmailAddress,Address,City,Phone
43,Patrick,Verstraete,58,Beulah.Diaz@hotmail.com,"680 Strauss Street ",Ontario,(633) 495-7238
78,Isabel,Wright,3,Carlos.Perry@telus.net,"6323 Park Avenue ",Colwood,(462) 445-5772
52,Leslie,MacKenzie,61,Mackenzie.Daeninck@live.com,"2456 Estate Road ",Yorba Linda,(594) 719-6189
65,Alexandra,Kelly,44,Katelyn.White@att.com,"8188 Burnett Street ",Whitney,(497) 600-6435
47,Anthony,Hayes,56,Cassandra.Adams@gmail.com,"6455 Monroe Street ",Pacifica,(241) 771-1426
26,Alyssa,Griffin,19,Cole.Roberts@rogers.ca,"5674 Bay 16th Street ",Sebastian,(244) 588-1031
81,Claire,Patterson,38,Isaac.Hall@live.com,"2276 Paerdegat 2nd Street ",Garfield,(609) 242-5209
77,Colby,Baker,24,John.Roberts@live.com,"5036 St Nicholas Avenue ",Cookville,(651) 694-1091
86,Rebecca,Murphy,83,Makayla.Cook@gmail.com,"5487 11th Avenue ",Soledad,(303) 243-1134
50,Brianna,Morgan,53,Sydney.Rogers@hotmail.com,"7407 Ferry Place ",East Crockett,(210) 361-5761
21,Jenna,Gonzalez,84,Robert.Lewis@gmx.com,"6534 Church Avenue ",Lyford,(230) 402-5565
83,Zachary,Pearson,58,Alexandria.Timms@att.com,"8711 Caton Place ",Daingerfield,(355) 657-4390
67,Silvia,Adams,29,Leslie.Martinez@att.com,"5697 Malcolm X Boulevard ",FresnoCounty seat,(542) 638-1524
28,Haley,Morris,65,Makayla.Rogers@rogers.ca,"4622 Onderdonk Avenue ",Lac-Saint-Joseph,(619) 529-7068
70,Alexander,Price,20,Carlos.Perry@rogers.ca,"859 101st Avenue ",L'Assomption,(444) 739-7931
57,Alyssa,Johnson,8,Carlie.Harris@att.com,"523 Stillwell Avenue ",Primera,(366) 251-7977
10,Caitlin,Collins,64,Briana.Adams@live.com,"1022 Delevan Street ",Pomona,(287) 224-5185
63,Victoria,Martinez,86,Sebastian.Barnes@microsoft.com,"5068 Avenue L  ",Amsterdam,(556) 456-1377
30,Abigail,Lewis,85,Mary.Bryant@gmail.com,"1179 Apollo Street ",Arborg,(247) 331-7833
95,John,Jones,31,Brittany.Getzlaff@shaw.ca,"8795 Jay Street ",Bécancour,(515) 475-3856
10,Jada,Baker,34,Bailey.Tellies@live.com,"2327 Bushwick Place ",Austwell-Tivoli,(399) 569-4144