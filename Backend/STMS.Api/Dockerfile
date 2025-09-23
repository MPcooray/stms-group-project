FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy entire backend folder and restore
COPY Backend/ ./Backend/

WORKDIR /src/Backend/STMS.Api

RUN dotnet restore "STMS.Api.csproj"
RUN dotnet publish "STMS.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "STMS.Api.dll"]
