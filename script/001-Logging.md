dotnet new mvc -n LoggingSample -o LoggingSample

# 001 Configure Logging

~~~ csharp
.ConfigureLogging((hostingContext, logging) =>
{
    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
})
~~~

# 002 CustomLogger

~~~ csharp
public class ColoredConsoleLoggerConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    public int EventId { get; set; } = 0;
    public ConsoleColor Color { get; set; } = ConsoleColor.Yellow;
}
public class ColoredConsoleLoggerProvider : ILoggerProvider
{
    private readonly ColoredConsoleLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, ColoredConsoleLogger> _loggers = new ConcurrentDictionary<string, ColoredConsoleLogger>();

    public ColoredConsoleLoggerProvider(ColoredConsoleLoggerConfiguration config)
    {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ColoredConsoleLogger(name, _config));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
public class ColoredConsoleLogger : ILogger
{
	private static object _lock = new Object();
    private readonly string _name;
    private readonly ColoredConsoleLoggerConfiguration _config;

    public ColoredConsoleLogger(string name, ColoredConsoleLoggerConfiguration config)
    {
        _name = name;
        _config = config;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == _config.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        lock (_lock)
        {
            if (_config.EventId == 0 || _config.EventId == eventId.Id)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = _config.Color;
                Console.WriteLine($"{logLevel.ToString()} - {eventId.Id} - {_name} - {formatter(state, exception)}");
                Console.ForegroundColor = color;
            }
        }
    }
}
~~~

# 003 use the custom logger

~~~ csharp
logging.ClearProviders();
var config = new ColoredConsoleLoggerConfiguration
{
    LogLevel = LogLevel.Information,
    Color = ConsoleColor.Red
};
logging.AddProvider(new ColoredConsoleLoggerProvider(config));

~~~

# 004 ad NLog

==> NLog.Web.AspNetCore

~~~ csharp
using NLog.Extensions.Logging;
using NLog.Web;

// ...

hostingContext.HostingEnvironment.ConfigureNLog("NLog.Config");
logging.AddProvider(new NLogLoggerProvider());
~~~

# 005 NLog.Config

~~~ xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="C:\git\dotnetconf\001-logging\internal-nlog.txt">

  <!-- Load the ASP.NET Core plugin -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
  </extensions>

  <!-- the targets to write to -->
  <targets>
     <!-- write logs to file -->
     <target xsi:type="File" name="allfile" fileName="C:\git\dotnetconf\001-logging\nlog-all-${shortdate}.log"
                 layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message} ${exception}" />

   <!-- another file log, only own logs. Uses some ASP.NET core renderers -->
     <target xsi:type="File" name="ownFile-web" fileName="C:\git\dotnetconf\001-logging\nlog-own-${shortdate}.log"
             layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|  ${message} ${exception}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />

     <!-- write to the void aka just remove -->
    <target xsi:type="Null" name="blackhole" />
  </targets>

  <!-- rules to map from logger name to target -->
  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />

    <!--Skip Microsoft logs and so log only own logs-->
    <logger name="Microsoft.*" minlevel="Trace" writeTo="blackhole" final="true" />
    <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
  </rules>
</nlog>
~~~