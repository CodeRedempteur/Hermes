# Define database name and PostgreSQL connection details 
$scriptPath = "SampleData.psql" 
$cleanscriptPath = "EmptyDatabase.psql" 
$scriptPath = "C:\Users\SnifL\Documents\ProjetInformatique\R_Project\Hestia\src\Hestia.Database\SampleData.psql"
# Define the project path 
$psqlPath = "C:\Program Files\PostgreSQL\17\bin\psql.exe" 

# D�finir directement les param�tres de connexion
$dbhost = "localhost"
$databaseName = "cookdb"
$username = "postgres"
$password = "bateau"
$port = "5432"

Write-Host "Connexion � PostgreSQL avec les param�tres suivants :" 
Write-Host "H�te: $dbhost" 
Write-Host "Base de donn�es: $databaseName" 
Write-Host "Utilisateur: $username" 
Write-Host "Mot de passe: $password" 
Write-Host "Port: $port" 

function Psql-ExecuteScript { 
    param ( 
        [string]$dbName, 
        [string]$user, 
        [string]$pwd, 
        [string]$dbhost, 
        [string]$port, 
        [string]$sqlFile 
    )
    
    # D�finir l'environnement PGPASSWORD
    $env:PGPASSWORD = $pwd
    
    # Run the psql command to execute the SQL script on the specified database 
    $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -d $dbName -f `"$sqlFile`""


    
    Write-Host "Ex�cution de la commande: $command"
    
    # Execute the command
    try {
        Invoke-Expression $command
        Write-Host "Commande ex�cut�e avec succ�s" -ForegroundColor Green
    }
    catch {
        Write-Host "Erreur lors de l'ex�cution de la commande: $_" -ForegroundColor Red
    }
    finally {
        # Nettoyer la variable d'environnement, v�rifier d'abord si elle existe
        if (Test-Path Env:PGPASSWORD) {
            Remove-Item Env:PGPASSWORD
        }
    }
} 

# Ex�cuter le script pour vider la base de donn�es
Write-Host "Ex�cution du script de nettoyage de base de donn�es..." -ForegroundColor Cyan
Psql-ExecuteScript -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port -sqlFile $cleanscriptPath 

# Ex�cuter le script pour charger les donn�es d'exemple
Write-Host "Ex�cution du script de chargement des donn�es..." -ForegroundColor Cyan
Psql-ExecuteScript -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port -sqlFile $scriptPath