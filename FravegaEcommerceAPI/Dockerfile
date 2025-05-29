FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FravegaEcommerceAPI.sln", "."]
COPY ["src/FravegaEcommerce.API/FravegaEcommerce.API/*.csproj", "src/FravegaEcommerce.API/FravegaEcommerce.API/"]
COPY ["src/FravegaEcommerce.Application/FravegaEcommerce.Application/*.csproj", "src/FravegaEcommerce.Application/FravegaEcommerce.Application/"]
COPY ["src/FravegaEcommerce.Domain/FravegaEcommerce.Domain/*.csproj", "src/FravegaEcommerce.Domain/FravegaEcommerce.Domain/"]
COPY ["test/FravegaEcommerce.Tests/*.csproj", "test/FravegaEcommerce.Tests/"]

RUN dotnet restore "FravegaEcommerceAPI.sln"

COPY . .

RUN dotnet build "FravegaEcommerceAPI.sln" -c Release -o /app/build

RUN dotnet publish "src/FravegaEcommerce.API/FravegaEcommerce.API/FravegaEcommerce.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FravegaEcommerce.API.dll"]