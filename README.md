# Quartz.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/Quartz.AspNetCore.svg)](https://www.nuget.org/packages/Quartz.AspNetCore)

A package to help you use Quartz.NET in Asp.Net core by DI easily

#### Installation

+ Package Manager: Install-Package Quartz.AspNetCore -Version 1.0.1
+ .NET CLI: dotnet add package Quartz.AspNetCore --version 1.0.1

#### UseMemoryStore

1. Register service in ConfigureServices method in Startup file.

			services.AddQuartz(options =>
			{
				options.UseMemoryStore();
			});

2. Start quartz scheduler in Configure method

			app.UseQuartz();

#### UseSqlServer

1. Prepare your database: https://github.com/quartznet/quartznet/blob/master/database/tables/tables_sqlServer.sql
2. Register service in ConfigureServices method in Startup file.

			services.AddQuartz(options =>
			{
				options.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=QUARTZ;Integrated Security=True");
			});

3. Start quartz scheduler in Configure method

			app.UseQuartz();
#### Plan

1. Support MySql, PG, etc...
2. Support configuration file

#### Contribution

1. Fork the project
2. Create Feat_xxx branch
3. Commit your code
4. Create Pull Request