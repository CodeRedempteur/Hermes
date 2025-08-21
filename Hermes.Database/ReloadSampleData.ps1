# Define database name and PostgreSQL connection details 
$scriptPath = "SampleData.psql" 
$cleanscriptPath = "EmptyDatabase.psql" 
$scriptPath = "C:\Users\SnifL\Documents\ProjetInformatique\R_Project\Hestia\src\Hestia.Database\SampleData.psql"
# Define the project path 
$psqlPath = "C:\Program Files\PostgreSQL\17\bin\psql.exe" 

# Définir directement les paramètres de connexion
$dbhost = "localhost"
$databaseName = "cookdb"
$username = "postgres"
$password = "bateau"
$port = "5432"

Write-Host "Connexion à PostgreSQL avec les paramètres suivants :" 
Write-Host "Hôte: $dbhost" 
Write-Host "Base de données: $databaseName" 
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
    
    # Définir l'environnement PGPASSWORD
    $env:PGPASSWORD = $pwd
    
    # Run the psql command to execute the SQL script on the specified database 
    $command = "& `"$psqlPath`" -U $user -h $dbhost -p $port -d $dbName -f `"$sqlFile`""


    
    Write-Host "Exécution de la commande: $command"
    
    # Execute the command
    try {
        Invoke-Expression $command
        Write-Host "Commande exécutée avec succès" -ForegroundColor Green
    }
    catch {
        Write-Host "Erreur lors de l'exécution de la commande: $_" -ForegroundColor Red
    }
    finally {
        # Nettoyer la variable d'environnement, vérifier d'abord si elle existe
        if (Test-Path Env:PGPASSWORD) {
            Remove-Item Env:PGPASSWORD
        }
    }
} 

# Exécuter le script pour vider la base de données
Write-Host "Exécution du script de nettoyage de base de données..." -ForegroundColor Cyan
Psql-ExecuteScript -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port -sqlFile $cleanscriptPath 

# Exécuter le script pour charger les données d'exemple
Write-Host "Exécution du script de chargement des données..." -ForegroundColor Cyan
Psql-ExecuteScript -dbName $databaseName -user $username -pwd $password -dbhost $dbhost -port $port -sqlFile $scriptPath