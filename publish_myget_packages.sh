#!/usr/bin/env bash
rm -rf src/Quartz.AspNetCore/bin/Release
rm -rf src/Quartz.AspNetCore.MySqlConnector/bin/Release
dotnet publish Quartz.AspNetCore.sln -c Release
nuget push src/Quartz.AspNetCore/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json
nuget push src/Quartz.AspNetCore.MySqlConnector/bin/Release/*.nupkg -source https://www.myget.org/F/zlzforever/api/v3/index.json