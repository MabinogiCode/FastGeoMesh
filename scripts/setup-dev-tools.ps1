# FastGeoMesh Development Tools Setup Script
# Installs and configures all necessary tools for development

param(
    [switch]$Install = $false,
    [switch]$Update = $false,
    [switch]$Verify = $false
)

$ErrorActionPreference = "Stop"

Write-Host "FastGeoMesh Development Tools Setup" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if tool is installed
function Test-DotNetTool {
    param([string]$ToolName)
    try {
        $result = dotnet tool list --global | Select-String $ToolName
        return $result -ne $null
    } catch {
        return $false
    }
}

# Function to install/update tools
function Install-DevelopmentTools {
    Write-Host "Installing/Updating .NET development tools..." -ForegroundColor Yellow
    
    # Restore local tools from manifest
    Write-Host "Restoring tools from .config/dotnet-tools.json..." -ForegroundColor Gray
    dotnet tool restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to restore some tools from manifest"
    } else {
        Write-Host "Local tools restored successfully" -ForegroundColor Green
    }
}

# Function to verify installation
function Test-DevelopmentEnvironment {
    Write-Host "Verifying development environment..." -ForegroundColor Yellow
    Write-Host ""
    
    # Check .NET SDK version
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK Version: $dotnetVersion" -ForegroundColor Gray
    
    # Verify global.json requirements
    if (Test-Path "global.json") {
        $globalJson = Get-Content "global.json" | ConvertFrom-Json
        $requiredSdk = $globalJson.sdk.version
        Write-Host "Required SDK Version: $requiredSdk" -ForegroundColor Gray
        
        if ($dotnetVersion -ge $requiredSdk) {
            Write-Host "SDK version compatible" -ForegroundColor Green
        } else {
            Write-Host "SDK version may be incompatible" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    
    # Test project compilation
    Write-Host "Testing project compilation..." -ForegroundColor Gray
    dotnet build --configuration Release --verbosity quiet --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "Project build failed" -ForegroundColor Red
    }
}

# Function to show configuration summary
function Show-ConfigurationSummary {
    Write-Host ""
    Write-Host "Configuration Summary" -ForegroundColor Cyan
    Write-Host "====================" -ForegroundColor Cyan
    Write-Host ""
    
    $configs = @(
        @{ File = ".editorconfig"; Description = "Code style and formatting rules" },
        @{ File = "Directory.Build.props"; Description = "Common MSBuild properties" },
        @{ File = "global.json"; Description = ".NET SDK version lock" },
        @{ File = ".config/dotnet-tools.json"; Description = "Local tool manifest" },
        @{ File = "NuGet.Config"; Description = "Package source configuration" }
    )
    
    foreach ($config in $configs) {
        if (Test-Path $config.File) {
            Write-Host "OK $($config.File) - $($config.Description)" -ForegroundColor Green
        } else {
            Write-Host "MISSING $($config.File) - $($config.Description)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Development Environment Status:" -ForegroundColor Cyan
    
    $score = 0
    $total = 5
    
    if (Test-Path ".editorconfig") { $score++ }
    if (Test-Path "Directory.Build.props") { $score++ }
    if (Test-Path "global.json") { $score++ }
    if (Test-Path ".config/dotnet-tools.json") { $score++ }
    if (Test-Path "NuGet.Config") { $score++ }
    
    $percentage = [math]::Round(($score / $total) * 100)
    
    if ($percentage -eq 100) {
        Write-Host "Perfect setup: $score/$total files ($percentage%)" -ForegroundColor Green
    } elseif ($percentage -ge 80) {
        Write-Host "Good setup: $score/$total files ($percentage%)" -ForegroundColor Yellow
    } else {
        Write-Host "Incomplete setup: $score/$total files ($percentage%)" -ForegroundColor Red
    }
}

# Main execution
try {
    if ($Install) {
        Install-DevelopmentTools
    }
    
    if ($Update) {
        Install-DevelopmentTools
    }
    
    if ($Verify) {
        Test-DevelopmentEnvironment
    }
    
    if (-not $Install -and -not $Update -and -not $Verify) {
        # Default: show summary
        Show-ConfigurationSummary
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor Cyan
        Write-Host "  -Install    Install development tools"
        Write-Host "  -Update     Update existing tools"
        Write-Host "  -Verify     Verify development environment"
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Gray
        Write-Host "  .\setup-dev-tools.ps1 -Install"
        Write-Host "  .\setup-dev-tools.ps1 -Verify"
        Write-Host "  .\setup-dev-tools.ps1 -Update"
    }
    
    Show-ConfigurationSummary
    
} catch {
    Write-Host ""
    Write-Host "Setup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
