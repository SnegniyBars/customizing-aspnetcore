dotnet new web -n MiddleWaresSample -o MiddleWaresSample

# 001 empty asp.net core

# 002 sample middlewares

~~~ csharp
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync(" ===");
    await next();
    await context.Response.WriteAsync("=== ");
});
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync(">>>>>> ");
    await next();
    await context.Response.WriteAsync(" <<<<<<");
});
~~~

# 003 stopwatch middleware

~~~ csharp
app.Use(async (context, next) =>
{
    var s = new Stopwatch();
    s.Start();
    await next();
    s.Stop();
    var result = s.ElapsedMilliseconds;
    await context.Response.WriteAsync($"Time needed: {result }");
});
~~~

# 004 Maps

~~~ csharp
app.Map("/map1", app1 =>
{
    app1.Run(async context =>
    {
        await context.Response.WriteAsync("Map Test 1");
    });
});

app.Map("/map2", app1 =>
{
    app1.Run(async context =>
    {
        await context.Response.WriteAsync("Map Test 2");
    });
});
~~~

# 005 StopwatchMiddleWare

~~~ csharp
public class StopwatchMiddleWare
{
    private readonly RequestDelegate _next;

    public StopwatchMiddleWare(RequestDelegate next)
    {
        _next = next;
    }

    public async  Task Invoke(HttpContext context)
    {
        var s = new Stopwatch();
        s.Start();
        await _next(context);
        s.Stop();
        var result = s.ElapsedMilliseconds;
        await context.Response.WriteAsync($" Time needed: {result }");
    }
}
~~~

# 007 Extension

~~~ csharp
public static class StopwatchMiddleWareExtension
{
	public static IApplicationBuilder UseStopwatch(this IApplicationBuilder app)
	{
		app.UseMiddleware<StopwatchMiddleWare>();
		return app;
	}
}
~~~

# 006 AddMvc() / UseMvc()