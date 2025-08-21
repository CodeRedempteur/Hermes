# Define database name and PostgreSQL connection details
$scriptPath = "C:\Users\SnifL\Documents\ProjetInformatique\R_Project\Hermes.Website\Hermes.Database\CreateDatabase.psql"
$dbhost = "localhost"
$databaseName = "hermes_db_shop"
$username = "postgres" 
$password = "bateau"
$port = "5432"
$psqlPath = "C:\Program Files\PostgreSQL\17\bin\psql.exe"

# Set encoding
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Function to create the database
function Create-Database {
   param (
       [string]$dbName,
       [string]$user,
       [string]$pwd,
       [string]$dbhost,
       [string]$port
   )
   
   # Run the psql command to create a new database
   $createDbCommand = "CREATE DATABASE $dbName;"
   $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -c `"${createDbCommand}`""
   
   # Run the command with the password as an environment variable
   $env:PGPASSWORD = $pwd
   Invoke-Expression $command
   Remove-Item Env:PGPASSWORD
}

# Function to populate the database using the SQL script with quiet flag
function Populate-Database {
   param (
       [string]$dbName,
       [string]$user,
       [string]$pwd,
       [string]$dbhost,
       [string]$port,
       [string]$sqlFile
   )
   
   # Run the psql command to execute the SQL script with quiet mode
   $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -d $dbName -q -f `"$sqlFile`""
   
   # Run the command with the password as an environment variable
   $env:PGPASSWORD = $pwd
   Invoke-Expression $command
   Remove-Item Env:PGPASSWORD
}

# Function to delete the database if it exists
function Delete-DatabaseIfExists {
   param (
       [string]$dbName,
       [string]$user,
       [string]$pwd,
       [string]$dbhost,
       [string]$port
   )
   # Check if the database exists
   $checkDbCommand = "SELECT 1 FROM pg_database WHERE datname = '$dbName';"
   $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -tAc `"$checkDbCommand`""
   
   # Run the command with the password as an environment variable
   $env:PGPASSWORD = $pwd
   $result = Invoke-Expression $command
   Remove-Item Env:PGPASSWORD
   
   # If the database exists, delete it
   if ($result -eq "1") {
       Write-Output "Database '$dbName' exists. Deleting it..."
       $dropDbCommand = "DROP DATABASE $dbName WITH (FORCE);"
       $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -c `"${dropDbCommand}`""
       
       # Run the command with the password as an environment variable
       $env:PGPASSWORD = $pwd
       Invoke-Expression $command
       Remove-Item Env:PGPASSWORD
       Write-Output "Database '$dbName' has been deleted."
   } else {
       Write-Output "Database '$dbName' does not exist."
   }
}

# Function to list tables
function List-Tables {
   param (
       [string]$dbName,
       [string]$user,
       [string]$pwd,
       [string]$dbhost,
       [string]$port
   )
   
   # Run the psql command to list tables
   $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -d $dbName -c `"\dt`""
   
   # Run the command with the password as an environment variable
   $env:PGPASSWORD = $pwd
   Write-Output "`nTables in database '$dbName':"
   Invoke-Expression $command
   Remove-Item Env:PGPASSWORD
}

# Execute the functions
Write-Output "=== Starting Database Setup ==="
Delete-DatabaseIfExists -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port
start-sleep -Seconds 1
Create-Database -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port

Write-Output "`nCreating database schema..."
Populate-Database -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port -sqlFile $scriptPath
Write-Output "Schema created successfully!"

# Verify tables were created
List-Tables -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port
Write-Output "`n=== Database Setup Complete ==="