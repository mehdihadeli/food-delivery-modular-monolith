
IF "%1"=="init-context" dotnet ef migrations add InitialCustomersMigration -o \FoodDelivery.Modules.Customer\Shared\Data\Migrations\Customer --project .\FoodDelivery.Modules.Customers\FoodDelivery.Modules.Customers.csproj -c CustomersDbContext --verbose & goto exit
IF "%1"=="update-context" dotnet ef database update -c CustomersDbContext --verbose --project .\FoodDelivery.Modules.Customers\FoodDelivery.Modules.Customers.csproj & goto exit

:exit
