#!/bin/bash

dotnet ef database drop -s ./Api/ -p ./Infrastructure/ -f
rm -rf ./Infrastructure/Migrations/

dotnet ef migrations add InitDb -s ./Api/ -p ./Infrastructure/
dotnet ef migrations add CreateView -s ./Api/ -p ./Infrastructure/

MigrationName="CreateView"
MigrationDir="./Infrastructure/Migrations"
SourceFile="./Infrastructure/EF/MigrationBuilders/CreateViewSource.txt"

if [ ! -f "$SourceFile" ]; then
    echo "Error: Not found: $SourceFile" >&2
    exit 1
fi

MigrationFile=$(find "$MigrationDir" -type f -name "*_${MigrationName}.cs" -printf '%T@ %p\n' 2>/dev/null | sort -rn | head -n1 | cut -d' ' -f2-)

if [ -z "$MigrationFile" ]; then
    echo "Error: Not found migration '$MigrationName' in $MigrationDir" >&2
    exit 1
fi

cat "$SourceFile" > "$MigrationFile"

echo "Override file migration: $MigrationFile from $SourceFile"
