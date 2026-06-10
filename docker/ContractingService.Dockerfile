FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore insurance-platform.sln

RUN dotnet publish src/ContractingService/InsurancePlatform.ContractingService.Api/InsurancePlatform.ContractingService.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "InsurancePlatform.ContractingService.Api.dll"]
