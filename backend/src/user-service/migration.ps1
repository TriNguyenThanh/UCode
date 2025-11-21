dotnet ef database drop -s .\Api\ -p .\Infrastructure\ -f
rm .\Infrastructure\Migrations\ -Recurse -Force

dotnet ef migrations add InitDb -s .\Api\ -p .\Infrastructure\

dotnet ef database update -s .\Api\ -p .\Infrastructure\