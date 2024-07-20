
IF "%1"=="init-context" dotnet ef migrations add InitialIdentityServerMigration -o \FoodDelivery.Modules.Identity\Shared\Data\Migrations\Identity --project .\FoodDelivery.Modules.Identity\FoodDelivery.Modules.Identity.csproj -c IdentityContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c IdentityContext --verbose --project .\FoodDelivery.Modules.Identity\FoodDelivery.Modules.Identity.csproj & goto exit
:exit
