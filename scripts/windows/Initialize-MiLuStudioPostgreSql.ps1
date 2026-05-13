param(
    [string]$PsqlPath = "D:\soft\program\PostgreSQL\18\bin\psql.exe",
    [string]$HostName = "127.0.0.1",
    [int]$Port = 5432,
    [string]$AdminDatabase = "postgres",
    [string]$DatabaseName = "milu",
    [string]$Username = "root",
    [string]$Password = "root",
    [string]$BootstrapUsername = "postgres",
    [string]$BootstrapPassword = "root"
)

$ErrorActionPreference = "Stop"

if ($DatabaseName -notmatch '^[A-Za-z0-9_]+$') {
    throw "DatabaseName can contain only letters, numbers, and underscores."
}

$oldPassword = $env:PGPASSWORD

if (-not (Test-Path -LiteralPath $PsqlPath)) {
    throw "psql.exe not found: $PsqlPath"
}

function Invoke-PsqlScalar {
    param(
        [string]$User,
        [string]$UserPassword,
        [string]$Database,
        [string]$Sql
    )

    $env:PGPASSWORD = $UserPassword
    $result = & $PsqlPath `
        -h $HostName `
        -p $Port `
        -U $User `
        -d $Database `
        -t `
        -A `
        -c $Sql

    if ($LASTEXITCODE -ne 0) {
        throw "psql command failed for user '$User'."
    }

    return $result
}

function Invoke-PsqlCommand {
    param(
        [string]$User,
        [string]$UserPassword,
        [string]$Database,
        [string]$Sql
    )

    $env:PGPASSWORD = $UserPassword
    & $PsqlPath `
        -h $HostName `
        -p $Port `
        -U $User `
        -d $Database `
        -c $Sql

    if ($LASTEXITCODE -ne 0) {
        throw "psql command failed for user '$User'."
    }
}

try {
    $exists = Invoke-PsqlScalar `
        -User $Username `
        -UserPassword $Password `
        -Database $AdminDatabase `
        -Sql "select 1 from pg_database where datname = '$DatabaseName';"

    if ($exists -eq "1") {
        Write-Host "Database '$DatabaseName' already exists."
        return
    }

    $canCreate = Invoke-PsqlScalar `
        -User $Username `
        -UserPassword $Password `
        -Database $AdminDatabase `
        -Sql "select case when rolsuper or rolcreatedb then 1 else 0 end from pg_roles where rolname = current_user;"

    if ($canCreate -eq "1") {
        Invoke-PsqlCommand `
            -User $Username `
            -UserPassword $Password `
            -Database $AdminDatabase `
            -Sql "create database ""$DatabaseName"" owner ""$Username"" encoding 'UTF8';"
    }
    else {
        Write-Host "User '$Username' cannot create databases; using bootstrap user '$BootstrapUsername' to create '$DatabaseName' owned by '$Username'."
        Invoke-PsqlCommand `
            -User $BootstrapUsername `
            -UserPassword $BootstrapPassword `
            -Database $AdminDatabase `
            -Sql "create database ""$DatabaseName"" owner ""$Username"" encoding 'UTF8';"
    }

    Write-Host "Database '$DatabaseName' created for application user '$Username'."
}
finally {
    $env:PGPASSWORD = $oldPassword
}
