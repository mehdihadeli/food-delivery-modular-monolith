#### Migration Scripts

```bash
dotnet ef migrations add InitialOrdersMigration -o Shared/Data/Migrations/Order -c OrdersDbContext
dotnet ef database update -c OrdersDbContext
```
