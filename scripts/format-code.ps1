# FastGeoMesh Code Formatting Script
# For local development and pre-commit hooks

param(
    [Parameter(Mandatory=$false)]
    [switch]$Fix = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Check = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release"
)

Write-Host "🔧 FastGeoMesh Code Formatting Tool" -ForegroundColor Green

# Ensure we're in the solution directory
$solutionFile = Get-ChildItem -Filter "*.sln" | Select-Object -First 1
if (-not $solutionFile) {
    Write-Host "❌ No solution file found. Please run from solution directory." -ForegroundColor Red
    exit 1
}

Write-Host "📂 Working with solution: $($solutionFile.Name)" -ForegroundColor Cyan

try {
    if ($Fix) {
        Write-Host "🔄 Running code formatter (fixing issues)..." -ForegroundColor Yellow
        
        # Save current git status
        $gitStatusBefore = & git status --porcelain 2>$null
        
        # Run formatter with explicit solution file to avoid confusion
        $formatResult = & dotnet format $solutionFile.Name --verbosity normal 2>&1
        $exitCode = $LASTEXITCODE
        
        # Check git status after formatting
        $gitStatusAfter = & git status --porcelain 2>$null
        
        if ($exitCode -eq 0) {
            Write-Host "✅ Code formatting completed successfully" -ForegroundColor Green
            
            # Compare git status to detect changes
            if ($gitStatusBefore -ne $gitStatusAfter) {
                $changedFiles = & git diff --name-only 2>$null
                if ($changedFiles) {
                    Write-Host "📝 Formatted files:" -ForegroundColor Cyan
                    $changedFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
                    Write-Host "📋 Files have been auto-formatted. Please review and commit changes." -ForegroundColor Yellow
                } else {
                    Write-Host "✅ No changes detected after formatting." -ForegroundColor Green
                }
            } else {
                Write-Host "✅ No formatting changes needed." -ForegroundColor Green
            }
        } else {
            Write-Host "❌ Code formatting failed with exit code: $exitCode" -ForegroundColor Red
            Write-Host "Output:" -ForegroundColor Yellow
            $formatResult | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
            exit $exitCode
        }
        
    } elseif ($Check) {
        Write-Host "🔍 Checking code formatting (no changes)..." -ForegroundColor Yellow
        
        # Check formatting without making changes - use explicit solution file
        $result = & dotnet format $solutionFile.Name --verify-no-changes --verbosity normal 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Host "✅ Code is properly formatted" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Code formatting issues detected" -ForegroundColor Yellow
            
            # Identify the specific issues
            $formatIssues = $result | Where-Object { $_ -like "*error*" -or $_ -like "*mis en forme*" }
            if ($formatIssues) {
                Write-Host "Files that need formatting:" -ForegroundColor Yellow
                $formatIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
            }
            
            Write-Host ""
            Write-Host "To fix formatting issues locally:" -ForegroundColor Cyan
            Write-Host "  dotnet format" -ForegroundColor White
            Write-Host "  # OR use the provided scripts:" -ForegroundColor Gray
            Write-Host "  ./scripts/format-code.ps1 -Fix   # Windows" -ForegroundColor White
            Write-Host "  ./scripts/format-code.sh --fix   # Linux/macOS" -ForegroundColor White
            
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
        Write-Host "🔍 Quick formatting status check..." -ForegroundColor Yellow
        
        # Save current state
        $gitStatusBefore = & git status --porcelain 2>$null
        
        # Run quick format check with explicit solution file
        $result = & dotnet format $solutionFile.Name --verify-no-changes --verbosity quiet 2>&1
        $exitCode = $LASTEXITCODE
        
        # Ensure no changes were made during check
        $gitStatusAfter = & git status --porcelain 2>$null
        if ($gitStatusBefore -ne $gitStatusAfter) {
            Write-Host "⚠️  Unexpected: Check mode modified files. Reverting..." -ForegroundColor Yellow
            & git checkout . 2>$null
        }
        
        if ($exitCode -eq 0) {
            Write-Host "✅ Code is properly formatted" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Formatting issues detected. Use -Fix to resolve." -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "💡 Tip: Add this to your pre-commit hook:" -ForegroundColor Cyan
        Write-Host "    ./scripts/format-code.ps1 -Check" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "❌ Error during formatting:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎯 Formatting operations completed" -ForegroundColor Green
