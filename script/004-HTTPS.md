dotnet new mvc -n HttpsSample -o HttpsSample


# 001 Show HTTPS by default

# 002 Copy Cert from scripts

# 003 use cert

~~~ csharp
.UseKestrel(options => 
{
	options.Listen(IPAddress.Loopback, 5000);
	options.Listen(IPAddress.Loopback, 5001, listenOptions =>
	{
		listenOptions.UseHttps("certificate.pfx", "topsecret");
	});
})
~~~

# 004 call http://localhost:5000/