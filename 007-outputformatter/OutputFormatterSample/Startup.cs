using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OutputFormatterSample.Controllers;

namespace OutputFormatterSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });


            services.AddMvc()
                .AddMvcOptions(options =>
                {
                    options.OutputFormatters.Add(new VcardOutputFormatter());
                    options.OutputFormatters.Add(new CsvOutputFormatter());

                    options.FormatterMappings.SetMediaTypeMappingForFormat(
                        "vcard", MediaTypeHeaderValue.Parse("text/vcard"));
                    options.FormatterMappings.SetMediaTypeMappingForFormat(
                        "csv", MediaTypeHeaderValue.Parse("text/csv"));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

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
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context,
            Encoding selectedEncoding)
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
}
