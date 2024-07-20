
IF "%1"=="init-context" dotnet ef migrations add InitialCatalogMigration -o \FoodDelivery.Modules.Catalogs\Shared\Data\Migrations\Catalogs --project .\FoodDelivery.Modules.Catalogs\FoodDelivery.Modules.Catalogs.csproj -c CatalogDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CatalogDbContext --verbose --project .\FoodDelivery.Modules.Catalogs\FoodDelivery.Modules.Catalogs.csproj & goto exit

:exit
