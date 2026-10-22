FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props RecipeIngredients.csproj ./
RUN dotnet restore RecipeIngredients.csproj

COPY . .
RUN dotnet publish RecipeIngredients.csproj -c Release -o /app/publish --no-restore && \
    mkdir -p /app/publish/data

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
VOLUME ["/app/data"]

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RecipeIngredients.dll"]
