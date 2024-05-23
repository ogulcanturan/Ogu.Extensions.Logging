# <img src="logo/ogu-logo.png" alt="Header" width="24"/> Ogu.Extensions.Logging 

| **Build Status** | **Ogu.Extensions.Logging.HttpClient** | **Ogu.Extensions.Logging.HttpMiddleware** | **Ogu.Extensions.Logging.Abstractions** |
|------------------|---------------------------------------|------------------------------------------|------------------------------------------|
| [![.NET Core Desktop](https://github.com/ogulcanturan/Ogu.Extensions.Logging/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ogulcanturan/Ogu.Extensions.Logging/actions/workflows/dotnet.yml) | [![NuGet](https://img.shields.io/nuget/v/Ogu.Extensions.Logging.HttpClient.svg?color=1ecf18)](https://nuget.org/packages/Ogu.Extensions.Logging.HttpClient) | [![NuGet](https://img.shields.io/nuget/v/Ogu.Extensions.Logging.HttpMiddleware.svg?color=1ecf18)](https://nuget.org/packages/Ogu.Extensions.Logging.HttpMiddleware) | [![NuGet](https://img.shields.io/nuget/v/Ogu.Extensions.Logging.Abstractions.svg?color=1ecf18)](https://nuget.org/packages/Ogu.Extensions.Logging.Abstractions) |
|                  | [![Nuget](https://img.shields.io/nuget/dt/Ogu.Extensions.Logging.HttpClient.svg?logo=nuget)](https://nuget.org/packages/Ogu.Extensions.Logging.HttpClient) | [![Nuget](https://img.shields.io/nuget/dt/Ogu.Extensions.Logging.HttpMiddleware.svg?logo=nuget)](https://nuget.org/packages/Ogu.Extensions.Logging.HttpMiddleware) | [![Nuget](https://img.shields.io/nuget/dt/Ogu.Extensions.Logging.Abstractions.svg?logo=nuget)](https://nuget.org/packages/Ogu.Extensions.Logging.Abstractions) |


# Ogu.Extensions.Logging.HttpClient

Ogu.Extensions.Logging.HttpClient enables enhanced logging capabilities for HTTP client requests. [More info](https://github.com/ogulcanturan/Ogu.Extensions.Logging/tree/master/src/Ogu.Extensions.Logging.HttpClient#readme)

## Installation

You can install the library via NuGet Package Manager:

```bash
dotnet add package Ogu.Extensions.Logging.HttpClient
```

# Ogu.Extensions.Logging.HttpMiddleware

Ogu.Extensions.Logging.HttpMiddleware provides middleware-based logging for HTTP requests within applications (Similar to Serilog's [RequestLoggingMiddleware](https://github.com/serilog/serilog-aspnetcore/blob/dev/src/Serilog.AspNetCore/AspNetCore/RequestLoggingMiddleware.cs). [More info](https://github.com/ogulcanturan/Ogu.Extensions.Logging/tree/master/src/Ogu.Extensions.Logging.HttpMiddleware#readme)

## Installation

You can install the library via NuGet Package Manager:

```bash
dotnet add package Ogu.Extensions.Logging.HttpMiddleware
```

## Sample Application
A sample application demonstrating the usage of Ogu.Extensions.Logging.HttpClient & Ogu.Extensions.Logging.HttpMiddleware be found [here](https://github.com/ogulcanturan/Ogu.Extensions.Logging/tree/master/samples/SampleHttp.Api).
