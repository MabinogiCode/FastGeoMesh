# Coverage collection script for FastGeoMesh .NET 8
# Optimized for CI/CD and local development

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "tests/TestResults/coverage",
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateReport = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$OpenReport = $false,
    
    [Parameter(Mandatory=$false)]
    [int]$Threshold = 75
)

Write-Host "?? FastGeoMesh Coverage Collection Script (.NET 8)" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Output Directory: $OutputDir" -ForegroundColor Cyan

# Ensure output directory exists
$fullOutputDir = Join-Path $PSScriptRoot $OutputDir
if (!(Test-Path $fullOutputDir)) {
    New-Item -ItemType Directory -Path $fullOutputDir -Force | Out-Null
    Write-Host "? Created output directory: $fullOutputDir" -ForegroundColor Green
}

# Clean previous results
Write-Host "?? Cleaning previous coverage results..." -ForegroundColor Yellow
Get-ChildItem $fullOutputDir -Recurse | Remove-Item -Force -Recurse
Write-Host "? Previous results cleaned" -ForegroundColor Green

# Set environment variables for deterministic builds
$env:DOTNET_DETERMINISTIC_SOURCE_PATHS = "true"
$env:ContinuousIntegrationBuild = "true"

try {
    # Run tests with coverage collection
    Write-Host "?? Running tests with coverage collection..." -ForegroundColor Yellow
    
    $testCommand = @(
        "test"
        "tests/FastGeoMesh.Tests/FastGeoMesh.Tests.csproj"
        "--configuration", $Configuration
        "--logger", "trx"
        "--logger", "console;verbosity=normal"
        "--collect:`"XPlat Code Coverage`""
        "--results-directory", $fullOutputDir
        "--settings", "tests/FastGeoMesh.Tests/coverlet.runsettings"
        "/p:CollectCoverage=true"
        "/p:CoverletOutput=$fullOutputDir/"
        "/p:CoverletOutputFormat=`"lcov,opencover,cobertura`""
        "/p:Threshold=$Threshold"
        "/p:ThresholdType=line,branch,method"
        "/p:UseSourceLink=true"
    )
    
    $result = & dotnet @testCommand
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -ne 0) {
        Write-Host "? Tests failed with exit code: $exitCode" -ForegroundColor Red
        exit $exitCode
    }
    
    Write-Host "? Tests completed successfully" -ForegroundColor Green
    
    # Find coverage files
    $coverageFiles = @(
        Get-ChildItem -Path $fullOutputDir -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
        Get-ChildItem -Path $fullOutputDir -Recurse -Filter "coverage.opencover.xml" | Select-Object -First 1
        Get-ChildItem -Path $fullOutputDir -Recurse -Filter "coverage.info" | Select-Object -First 1
    ) | Where-Object { $_ -ne $null }
    
    if ($coverageFiles.Count -eq 0) {
        Write-Host "??  No coverage files found in expected locations" -ForegroundColor Yellow
        Get-ChildItem -Path $fullOutputDir -Recurse -Name | ForEach-Object {
            Write-Host "Found: $_" -ForegroundColor Gray
        }
    } else {
        Write-Host "? Coverage files generated:" -ForegroundColor Green
        $coverageFiles | ForEach-Object {
            Write-Host "  ?? $($_.Name) - $(($_.Length / 1KB).ToString('F2')) KB" -ForegroundColor Cyan
        }
    }
    
    # Generate HTML report if requested
    if ($GenerateReport) {
        Write-Host "?? Generating HTML coverage report..." -ForegroundColor Yellow
        
        $coberturaFile = Get-ChildItem -Path $fullOutputDir -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
        if ($coberturaFile) {
            $reportDir = Join-Path $fullOutputDir "html"
            
            & dotnet tool run reportgenerator `
                "-reports:$($coberturaFile.FullName)" `
                "-targetdir:$reportDir" `
                "-reporttypes:HtmlInline_AzurePipelines;Badges" `
                "-sourcedirs:src" `
                "-title:FastGeoMesh Coverage Report"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "? HTML report generated: $reportDir" -ForegroundColor Green
                
                if ($OpenReport) {
                    $indexFile = Join-Path $reportDir "index.html"
                    if (Test-Path $indexFile) {
                        Start-Process $indexFile
                        Write-Host "?? Report opened in browser" -ForegroundColor Green
                    }
                }
            } else {
                Write-Host "??  Report generation failed" -ForegroundColor Yellow
            }
        } else {
            Write-Host "??  No Cobertura file found for report generation" -ForegroundColor Yellow
        }
    }
    
    # Summary
    Write-Host "" -ForegroundColor White
    Write-Host "?? Coverage Collection Summary:" -ForegroundColor Green
    Write-Host "  Output Directory: $fullOutputDir" -ForegroundColor Cyan
    Write-Host "  Files Generated: $($coverageFiles.Count)" -ForegroundColor Cyan
    Write-Host "  Threshold: $Threshold%" -ForegroundColor Cyan
    Write-Host "" -ForegroundColor White
    
    if ($coverageFiles.Count -gt 0) {
        Write-Host "?? Coverage collection completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "??  Coverage collection completed with warnings" -ForegroundColor Yellow
        exit 1
    }
    
} catch {
    Write-Host "? Error during coverage collection:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
} finally {
    # Clean up environment variables
    Remove-Item Env:DOTNET_DETERMINISTIC_SOURCE_PATHS -ErrorAction SilentlyContinue
    Remove-Item Env:ContinuousIntegrationBuild -ErrorAction SilentlyContinue
}