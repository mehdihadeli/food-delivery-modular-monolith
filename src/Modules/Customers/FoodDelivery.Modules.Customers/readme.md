#### Migration Scripts

```bash
dotnet ef migrations add InitialCustomersMigration -o Shared/Data/Migrations/Customer -c CustomersDbContext
dotnet ef database update -c CustomersDbContext
```
