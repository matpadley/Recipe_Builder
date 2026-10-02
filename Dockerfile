FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RecipeIngredients.csproj .
RUN dotnet restore RecipeIngredients.csproj

COPY . .
RUN dotnet publish RecipeIngredients.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/data
VOLUME ["/app/data"]

ENV ASPNETCORE_URLS=http://+:8080
ENV IngredientsFile=/app/data/ingredients.json
ENV RecipesFile=/app/data/recipes.json
EXPOSE 8080

ENTRYPOINT ["dotnet", "RecipeIngredients.dll"]
