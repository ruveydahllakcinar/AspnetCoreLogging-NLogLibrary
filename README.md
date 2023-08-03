
# NLog in Asp.net Core

NLog is a flexible and free logging platform for various .NET platforms, including the .NET standard. NLog makes it easy to write to various targets (database, file, console) and change the log configuration on the fly.

## NLog Installation

1. First we need to install 2 packages.

 ``
  <PackageReference Include="NLog.Web.AspNetCore" Version="5.*" />
``
 ``
<PackageReference Include="NLog" Version="5.*" />
``
> Don't forget to build after installing the libraries! (You can quickly
> build with CTRL+B)
>
2.  `Creating the nlog file:`
*	I add a new item to the project and add a txt file and name it nlog.config. In this txt file, we add the codes specified on its site.

 ```
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Info"
      internalLogFile="c:\temp\internal-nlog-AspNetCore.txt">

  <!-- enable asp.net core layout renderers -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
  </extensions>

  <!-- the targets to write to -->
  <targets>
    <!-- File Target for all log messages with basic details -->
    <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-AspNetCore-all-${shortdate}.log"
            layout="${longdate}|${event-properties:item=EventId:whenEmpty=0}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}" />

    <!-- File Target for own log messages with extra web details using some ASP.NET core renderers -->
    <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-AspNetCore-own-${shortdate}.log"
            layout="${longdate}|${event-properties:item=EventId:whenEmpty=0}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />

    <!--Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection -->
    <target xsi:type="Console" name="lifetimeConsole" layout="${MicrosoftConsoleLayout}" />
  </targets>

  <!-- rules to map from logger name to target -->
  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />

    <!--Output hosting lifetime messages to console target for faster startup detection -->
    <logger name="Microsoft.Hosting.Lifetime" minlevel="Info" writeTo="lifetimeConsole, ownFile-web" final="true" />

    <!--Skip non-critical Microsoft logs and so log only own logs (BlackHole) -->
    <logger name="Microsoft.*" maxlevel="Info" final="true" />
    <logger name="System.Net.Http.*" maxlevel="Info" final="true" />
    
    <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
  </rules>
</nlog> 
```

### If we explain the codes briefly...
* In the Targets field, I specify the fields where the logs will be written. In the Layout section, I specify how the logging will be written. 

* In the other targets section, I only want to save your own custom logs, so we can write our optional codes in the second targets section.

 

    <targets>
        <!-- File Target for all log messages with basic details -->
        <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-AspNetCore-all-${shortdate}.log"
                layout="${longdate}|${event-properties:item=EventId:whenEmpty=0}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}" />
    
        <!-- File Target for own log messages with extra web details using some ASP.NET core renderers -->
        <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-AspNetCore-own-${shortdate}.log"
                layout="${longdate}|${event-properties:item=EventId:whenEmpty=0}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />
    
        <!--Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection -->
        <target xsi:type="Console" name="lifetimeConsole" layout="${MicrosoftConsoleLayout}" />
      </targets>
* The Rules field is the fields where we specify which level files will be written to which log, on which date they will be written.

* The `name="*"` field in the code writes all incoming logs.
````
    <rules>
        <!--All logs, including from Microsoft-->
        <logger name="*" minlevel="Trace" writeTo="allfile" />
    
        <!--Output hosting lifetime messages to console target for faster startup detection -->
        <logger name="Microsoft.Hosting.Lifetime" minlevel="Info" writeTo="lifetimeConsole, ownFile-web" final="true" />
    
        <!--Skip non-critical Microsoft logs and so log only own logs (BlackHole) -->
        <logger name="Microsoft.*" maxlevel="Info" final="true" />
        <logger name="System.Net.Http.*" maxlevel="Info" final="true" />
    
        <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
      </rules>
````

> ***Attention should be paid here.***

     <logger name="Microsoft.*" maxlevel="Info" final="true" />  In this area, we have added these layers to distinguish between our custom logs and the logs in the application.
     <logger name="System.Net.Http.*" maxlevel="Info" final="true" />

* Please note that the codes on your Appsettings.Development.json page are written as follows!!!
````
 {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
     }
 }
 ````

3. You must ensure that the nlog.config file we created for NLog is saved in the bin file.

* Right click on the nlog.config file and open the "Properties" page.

* Change the Copy if newer option => Copy Always. 

4. The Startup.cs page, a feature that came with Asp.net Core 6 and 7, has been removed from our projects. We do all our operations in Program.cs, so we need to add some code to Program.cs to introduce the NLog library to the project.
````
    builder.Services.AddControllersWithViews();
    Let's add these two codes just below the code.
````
````
builder.Logging.ClearProviders();
builder.Host.UseNLog();
````

5. After writing these codes, we add the codes that we will log into HomeContoller. After running it, we observe whether our log records are written to the txt file.

````
namespace NLogLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            _logger.LogInformation("Index sayfası açılıyor.");
            return View();
        }
    }
}
````

6. After running from the ISP, we go to the root folder and look at our log records saved in the bin folder there. 

 We can see all the logs coming into the file saved as   `nlog-AspNetCore-own-2023-08-03.log`

### Example Output:
````
2023-08-03 15:39:44.3650|0|INFO|Microsoft.Hosting.Lifetime|Application started. Press Ctrl+C to shut down. |url: |action: 
2023-08-03 15:39:44.3981|0|INFO|Microsoft.Hosting.Lifetime|Hosting environment: Development |url: |action: 
2023-08-03 15:39:44.3981|0|INFO|Microsoft.Hosting.Lifetime|Content root path: C:\Users\rhakc\source\repos\AspnetCoreLogging-NLogLibrary\NLogLibrary\NLogLibrary |url: |action: 
2023-08-03 15:39:44.7259|0|INFO|NLogLibrary.Controllers.HomeController|Index sayfası açılıyor. |url: https://localhost/|action: Index
````

## Writing Logs to Sql Server with NLog Library

1. First, we write the following code to Sql Server to create our database tables.

````
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
  CREATE TABLE [dbo].[Log] (
	  [Id] [int] IDENTITY(1,1) NOT NULL,
	  [MachineName] [nvarchar](50) NOT NULL,
	  [Logged] [datetime] NOT NULL,
	  [Level] [nvarchar](50) NOT NULL,
	  [Message] [nvarchar](max) NOT NULL,
	  [Logger] [nvarchar](250) NULL,
	  [Exception] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED ([Id] ASC)
      WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
  ) ON [PRIMARY]
````

2. Create new database
3. Create a database query.
4. After writing the above code, execute it
5. Our table is formed
6. We add the following code into the target field we added before. You must add your ConnectionString information.

````
<target name="database" xsi:type="Database">
  <connectionString>server=localhost;Database=*****;user id=****;password=*****</connectionString>
  <commandText>
    insert into dbo.Log (
      MachineName, Logged, Level, Message,
      Logger, Exception
    ) values (
      @MachineName, @Logged, @Level, @Message,
      @Logger, @Exception
    );
  </commandText>

  <parameter name="@MachineName" layout="${machinename}" />
  <parameter name="@Logged" layout="${date}" />
  <parameter name="@Level" layout="${level}" />
  <parameter name="@Message" layout="${message}" />
  <parameter name="@Logger" layout="${logger}" />
  <parameter name="@Exception" layout="${exception:tostring}" />
</target>
````

7. After doing these, do not forget to add the following code to the rules field
````
<logger name="*" minlevel="Info" writeTo="database" />
````

Your log records will now be saved in your database.
