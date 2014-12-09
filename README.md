
# Automation : Web Hosting

This library is designed to allow the hosting of web applications (or sites) via a programatic interface. It utilizes **[IIS Express](http://www.iis.net/learn/extensions/introduction-to-iis-express/iis-express-overview)** to spin up dedicated instances of your applications with support for both **HTTP** and **HTTPS**. We mainly developed this utility to aid in **[integration testing](http://en.wikipedia.org/wiki/Integration_testing)** but it can certainly be used in other scenarios where web hosting is desired.

## Prerequisites

You will need IIS Express installed on your host machine. You can [download it here](http://www.iis.net/downloads) or use the Web Platform Installer.

## Quick Start

You can install this package via [Nuget](http://nuget.org) under the package if of [RimDev.Automation.WebHosting](http://www.nuget.org/packages/RimDev.Automation.WebHosting/).

```
PM> Install-Package RimDev.Automation.WebHosting
```

Once the package is installed, you can use the following code to start any web application.

```csharp
var pathToWebsite = "C:\\mysite";
var siteName = "Contoso";

// async Task<IisExpress>
var instance = await IisExpress.CreateServer(pathToWebsite, siteName);

// sync
var instance = IisExpress.CreateServer(pathToWebsite, siteName).Result;
```

You can then access your site via the ports express in the **IisExpress** class instance. Those properties are described below.

## Properties

Every **IisExpress** class instance has properties that make working with your new server instance easier.

- **AppConfigPath** : The application host configuration where your site will be registered. This is where your bindings and site name are injected.
- **HttpPort** : The **HTTP** port your application is listening on.
- **HttpsPort** : The **HTTPS** port your application is listening on. To utilize the IIS Express development certificate HTTPS ports are required to be between *43300* and *43399*.
- **ProcessId** : The process in which your IIS Express instance is running in.

## Constants / Members

The IisExpress class also has a two important members: **DefaultIisExpressPath** and **DefaultAppConfigPath**.

- **DefaultIisExpressPath** : The assumed installation path for IisExpress.
- **defaultAppConfigPath** : The assumed path to the applicationHost.config template used to construct your sites IIS Express configuration.

## Start Up

When first initializing an **IisExpress** instance, a few steps are taken to insure you get a running web server.

1. An available **HTTP** port is retrieved
2. An available **HTTPS** port is retrieved
3. The site unique applicationHost.config is created with the ports and site name, then written to disk.
4. The IIS Express host process is started, and passed the config

## Clean up

The **IisExpress** implementation implements the **IDisposable** interface. On Dispose, the process running your web application is stopped and the applicationHost.config file that was created is deleted.

## Logging

All process logging is done using **Trace**. There are methods to access this via your favorite logging frameworks.

## Thanks

Thanks to [Ritter IM](http://ritterim.com) for supporting OSS.
