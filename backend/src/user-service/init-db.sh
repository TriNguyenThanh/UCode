#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
do
    echo "SQL Server is not ready yet. Waiting..."
    sleep 2
done

echo "SQL Server is ready!"

# Run EF migrations
echo "Running EF migrations for UserService..."
dotnet ef database update --project Infrastructure/UserService.Infrastructure.csproj --startup-project Api/UserService.Api.csproj --connection "$ConnectionStrings__DefaultConnection"

echo "UserService database migration completed!"
