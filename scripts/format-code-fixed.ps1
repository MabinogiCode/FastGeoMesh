# FastGeoMesh Code Formatting Script
# For local development and pre-commit hooks

param(
    [Parameter(Mandatory=$false)]
    [switch]$Fix = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Check = $false
)

Write-Host "FastGeoMesh Code Formatting Tool" -ForegroundColor Green

# Ensure we're in the solution directory
$solutionFile = Get-ChildItem -Filter "*.sln" | Select-Object -First 1
if (-not $solutionFile) {
    Write-Host "Error: No solution file found. Please run from solution directory." -ForegroundColor Red
    exit 1
}

Write-Host "Working with solution: $($solutionFile.Name)" -ForegroundColor Cyan

try {
    if ($Fix) {
        Write-Host "Running code formatter (fixing issues)..." -ForegroundColor Yellow
        
        # Run formatter with explicit solution file
        $formatResult = & dotnet format $solutionFile.Name --verbosity normal 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "Code formatting completed successfully" -ForegroundColor Green
            
            # Check for changes
            $changedFiles = & git diff --name-only 2>$null
            if ($changedFiles) {
                Write-Host "Formatted files:" -ForegroundColor Cyan
                $changedFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
                Write-Host "Files have been auto-formatted. Please review and commit changes." -ForegroundColor Yellow
            } else {
                Write-Host "No formatting changes needed." -ForegroundColor Green
            }
        } else {
            Write-Host "Code formatting failed with exit code: $exitCode" -ForegroundColor Red
            $formatResult | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
            exit $exitCode
        }
        
    } elseif ($Check) {
        Write-Host "Checking code formatting (no changes)..." -ForegroundColor Yellow
        
        # Check formatting without making changes
        $result = & dotnet format $solutionFile.Name --verify-no-changes --verbosity normal 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "Code is properly formatted" -ForegroundColor Green
        } else {
            Write-Host "Code formatting issues detected" -ForegroundColor Yellow
            
            Write-Host ""
            Write-Host "To fix formatting issues locally:" -ForegroundColor Cyan
            Write-Host "  dotnet format" -ForegroundColor White
            Write-Host "  # OR use the provided scripts:" -ForegroundColor Gray
            Write-Host "  ./scripts/format-code.ps1 -Fix   # Windows" -ForegroundColor White
            
            exit $exitCode
        }
        
    } else {
        # Default: show help and current status
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor White
        Write-Host "  ./scripts/format-code.ps1 -Fix    # Fix formatting issues" -ForegroundColor Cyan
        Write-Host "  ./scripts/format-code.ps1 -Check  # Check formatting only" -ForegroundColor Cyan
        Write-Host ""
        
        # Quick status check
        Write-Host "Quick formatting status check..." -ForegroundColor Yellow
        
        # Run quick format check
        $result = & dotnet format $solutionFile.Name --verify-no-changes --verbosity quiet 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "Code is properly formatted" -ForegroundColor Green
        } else {
            Write-Host "Formatting issues detected. Use -Fix to resolve." -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "Tip: Add this to your pre-commit hook:" -ForegroundColor Cyan
        Write-Host "    ./scripts/format-code.ps1 -Check" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "Error during formatting:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Formatting operations completed" -ForegroundColor Green
