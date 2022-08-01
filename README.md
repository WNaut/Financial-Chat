# Installation guide

## Prerequisites
* Visual Studio 2019
* SQL Server
* Instance of RabbitMQ
* Docker Desktop (if you don't want to set up a RabbitMQ instance)

**Note:** If you don't have an instance of RabbitMQ the easiest way to get it,
is to run it in a Docker container (that's why Docker Desktop is a prerequisite),
once you have installed Docker Desktop, run the following command in Powershell or Bash:

```sh
docker run -d --hostname my-rabbit --name rabbit rabbitmq:3-management
```

---

## Setup
In order to run this project locally:

1. Make sure you can run .NET 5 and Angular apps. For this, you'll need to have installed
[NodeJS v14.16.0](https://nodejs.org/download/release/v14.16.0/) or [higher](https://nodejs.org/es/),
[Angular cli](https://angular.io/cli) and [.NET SDK version 5](https://dotnet.microsoft.com/download/dotnet/5.0).

2. Open the project solution in Visual Studio, go to the  **FinancialChat.WebApp** project and
set the connection string in the `appsettings.json` file (also adjust RabbitMQ configuration, if needed).

3. Open the **Package Manager Console** (if you can't find it, click `Tools` on the menu
and click `NuGet Package Manager > Package Manager Console`)
   - Select **FinancialChat.Persistence** in the Default Project dropdown from the Package Manager Console.
   - Make sure you have set **FinancialChat.WebApp** as startup project (to do this, right click the project and select `Set as Startup Project`)
   - Run the following command:
   
```
Update-Database
```

4. If you decided to run the RabbitMQ instance as a Docker container, open Powershell or Bash
and run the following command to start the RabbitMQ Docker image as a container.
It's really important to keep this window open while running the application.

```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

5. Set **FinancialChat.WebApp** and **FinancialChat.Bot.App** as startup projects, to do this, right click the solution
and click on `Set Startup Projects...` then a window will open, select `Multiple startup projects`
and on the `Action` column set **FinancialChat.WebApp** and **FinancialChat.Bot.App** to `Start`.

8. Now you can run the application.

---

## Usage
Once the application is running, you just need to register as a user and login to the app.


## Bonuses

- Handle messages that are not understood or any exceptions raised within the bot.
- Have more than one chatroom.