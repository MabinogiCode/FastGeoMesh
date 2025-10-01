# Script de test pour la couverture de code locale
Write-Host "Test de la couverture de code locale" -ForegroundColor Yellow

# Nettoyer les anciens resultats
if (Test-Path "tests\TestResults") {
    Write-Host "Nettoyage des anciens resultats..." -ForegroundColor Gray
    Remove-Item "tests\TestResults" -Recurse -Force
}

Write-Host ""
Write-Host "Build du projet..." -ForegroundColor Cyan
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors du build" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Execution des tests avec couverture..." -ForegroundColor Cyan
dotnet test --configuration Release --no-build `
    --collect:"XPlat Code Coverage" `
    --results-directory ./tests/TestResults `
    --logger trx `
    --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors des tests" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Analyse des fichiers generes..." -ForegroundColor Cyan

# Rechercher les fichiers de couverture
$coverageFiles = Get-ChildItem -Path "tests\TestResults" -Recurse -Include "*coverage*" -File

Write-Host "Fichiers de couverture trouves:" -ForegroundColor Green
foreach ($file in $coverageFiles) {
    $size = [math]::Round($file.Length / 1KB, 2)
    Write-Host "  $($file.Name) ($size KB)" -ForegroundColor Green
    Write-Host "     $($file.FullName)" -ForegroundColor Gray
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "Aucun fichier de couverture trouve!" -ForegroundColor Red
    
    Write-Host ""
    Write-Host "Debug - Contenu de TestResults:" -ForegroundColor Yellow
    if (Test-Path "tests\TestResults") {
        Get-ChildItem -Path "tests\TestResults" -Recurse | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  Repertoire TestResults n'existe pas" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "Couverture de code generee avec succes!" -ForegroundColor Green
    Write-Host "Nombre de fichiers: $($coverageFiles.Count)" -ForegroundColor Green
    
    # Afficher des extraits de couverture si possible
    $coberturaFile = $coverageFiles | Where-Object { $_.Name -like "*cobertura*" } | Select-Object -First 1
    if ($coberturaFile) {
        Write-Host ""
        Write-Host "Apercu du fichier Cobertura:" -ForegroundColor Cyan
        $content = Get-Content $coberturaFile.FullName -Head 10
        $content | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "Test de couverture termine!" -ForegroundColor Green
