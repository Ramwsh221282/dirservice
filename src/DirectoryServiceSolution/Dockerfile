FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DirectoryServiceSolution.sln ./
COPY Core/DirectoryService.Core/*.csproj Core/DirectoryService.Core/
COPY Core/DirectoryService.Tests/*.csproj Core/DirectoryService.Tests/
COPY Contracts/DirectoryService.Contracts/*.csproj Contracts/DirectoryService.Contracts/
COPY UseCases/DirectoryService.UseCases/*.csproj UseCases/DirectoryService.UseCases/
COPY Infrastructure/DirectoryService.Infrastructure.PostgreSQL/*.csproj Infrastructure/DirectoryService.Infrastructure.PostgreSQL/
COPY Infrastructure/DirectoryService.Infrastructure.Identity/*.csproj Infrastructure/DirectoryService.Infrastructure.Identity/
COPY Presenters/DirectoryService.WebApi/*.csproj Presenters/DirectoryService.WebApi/
COPY Presenters/DirectoryService.Integrational.Tests/*.csproj Presenters/DirectoryService.Integrational.Tests/
COPY UtilityLibraries/ResultLibrary/*.csproj UtilityLibraries/ResultLibrary/
COPY UtilityLibraries/ResultLibrary.AspNetCore/*.csproj UtilityLibraries/ResultLibrary.AspNetCore/

RUN dotnet restore Presenters/DirectoryService.WebApi/DirectoryService.WebApi.csproj

COPY . .

RUN dotnet publish Presenters/DirectoryService.WebApi/DirectoryService.WebApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DirectoryService.WebApi.dll"]
