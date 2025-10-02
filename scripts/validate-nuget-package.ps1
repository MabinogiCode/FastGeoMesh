# FastGeoMesh NuGet Package Validation Script
# Validates package build, dependencies, icon, and metadata before release

param(
    [switch]$DryRun = $false,
    [switch]$Verbose = $false,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$VerbosePreference = if ($Verbose) { "Continue" } else { "SilentlyContinue" }

Write-Host "FastGeoMesh NuGet Package Validation" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Dry Run: $DryRun" -ForegroundColor Gray
Write-Host ""

# Change to solution root
$solutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $solutionRoot

# Initialize version variable
$version = "Unknown"

try {
    # Step 1: Clean and restore
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Clean failed with exit code $LASTEXITCODE" }

    Write-Host "Restoring packages..." -ForegroundColor Yellow
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore failed with exit code $LASTEXITCODE" }

    # Step 2: Build solution
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }

    # Step 3: Run tests
    Write-Host "Running tests..." -ForegroundColor Yellow  
    dotnet test --configuration $Configuration --no-build --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Tests failed with exit code $LASTEXITCODE" }

    # Step 4: Validate project file
    Write-Host "Validating project metadata..." -ForegroundColor Yellow
    $projectPath = "src/FastGeoMesh/FastGeoMesh.csproj"
    
    if (-not (Test-Path $projectPath)) {
        throw "Project file not found: $projectPath"
    }

    [xml]$project = Get-Content $projectPath
    $properties = $project.Project.PropertyGroup

    # Required properties for NuGet package
    $requiredProps = @{
        "PackageId" = "FastGeoMesh"
        "Authors" = "MabinogiCode"
        "Description" = $null
        "PackageTags" = $null
        "RepositoryUrl" = "https://github.com/MabinogiCode/FastGeoMesh"
        "PackageLicenseExpression" = "MIT"
        "PackageReadmeFile" = "nuget-readme.md"
        "PackageIcon" = "mabinogi-icon.png"
    }

    foreach ($prop in $requiredProps.Keys) {
        $value = $properties.$prop
        if ([string]::IsNullOrWhiteSpace($value)) {
            throw "Missing required property: $prop"
        }
        if ($requiredProps[$prop] -and $value -ne $requiredProps[$prop]) {
            throw "Invalid value for $prop : expected '$($requiredProps[$prop])', got '$value'"
        }
        Write-Verbose "Valid property $prop : $value"
    }

    # Validate version format and store it
    $version = $properties.Version
    if (-not ($version -match '^\d+\.\d+\.\d+$')) {
        throw "Invalid version format: $version (expected x.y.z)"
    }
    Write-Verbose "Valid version: $version"

    # Step 5: Validate icon file
    Write-Host "Validating package icon..." -ForegroundColor Yellow
    $iconPath = "src/FastGeoMesh/mabinogi-icon.png"
    if (-not (Test-Path $iconPath)) {
        throw "Package icon not found: $iconPath"
    }
    
    $iconSize = (Get-Item $iconPath).Length
    if ($iconSize -lt 1024) {
        Write-Warning "Icon file is very small ($iconSize bytes) - ensure it's a valid PNG"
    }
    Write-Verbose "Valid icon file exists ($iconSize bytes)"

    # Step 6: Validate README file
    Write-Host "Validating README file..." -ForegroundColor Yellow
    $readmePath = "nuget-readme.md"
    if (-not (Test-Path $readmePath)) {
        throw "README file not found: $readmePath"
    }
    
    $readmeContent = Get-Content $readmePath -Raw
    if ($readmeContent.Length -lt 1000) {
        Write-Warning "README file is quite short - ensure it contains adequate documentation"
    }
    Write-Verbose "Valid README file exists ($($readmeContent.Length) characters)"

    # Step 7: Create package (dry run or actual)
    Write-Host "Creating NuGet package..." -ForegroundColor Yellow
    
    $packArgs = @(
        "pack", $projectPath,
        "--configuration", $Configuration,
        "--no-build",
        "--output", "artifacts",
        "--verbosity", "normal"
    )
    
    if ($DryRun) {
        Write-Host "   [DRY RUN] Would execute: dotnet $($packArgs -join ' ')" -ForegroundColor Gray
    } else {
        # Ensure artifacts directory exists
        if (-not (Test-Path "artifacts")) {
            New-Item -Type Directory -Path "artifacts" | Out-Null
        }
        
        dotnet @packArgs
        if ($LASTEXITCODE -ne 0) { throw "Pack failed with exit code $LASTEXITCODE" }
        
        # Validate generated package
        $packagePattern = "artifacts/FastGeoMesh.$version.nupkg"
        $packageFile = Get-ChildItem $packagePattern -ErrorAction SilentlyContinue
        
        if (-not $packageFile) {
            throw "Package file not created: $packagePattern"
        }
        
        Write-Verbose "Package created: $($packageFile.Name) ($($packageFile.Length) bytes)"
        
        # Validate symbols package
        $symbolsPattern = "artifacts/FastGeoMesh.$version.snupkg"
        $symbolsFile = Get-ChildItem $symbolsPattern -ErrorAction SilentlyContinue
        
        if ($symbolsFile) {
            Write-Verbose "Symbols package created: $($symbolsFile.Name) ($($symbolsFile.Length) bytes)"
        } else {
            Write-Warning "Symbols package not found - this is optional but recommended"
        }
    }

    # Step 8: Dependency analysis
    Write-Host "Analyzing dependencies..." -ForegroundColor Yellow
    
    # Parse package references from project file
    $packageRefs = $project.Project.ItemGroup.PackageReference
    if ($packageRefs) {
        Write-Host "   Dependencies:" -ForegroundColor Gray
        foreach ($ref in $packageRefs) {
            $name = $ref.Include
            $version = $ref.Version
            Write-Host "   - $name $version" -ForegroundColor Gray
        }
    } else {
        Write-Host "   No dependencies found" -ForegroundColor Gray
    }

    # Step 9: Summary
    Write-Host ""
    Write-Host "Validation completed successfully!" -ForegroundColor Green
    Write-Host "Package: FastGeoMesh $version" -ForegroundColor Green
    Write-Host "Configuration: $Configuration" -ForegroundColor Green
    
    if ($DryRun) {
        Write-Host "Status: DRY RUN - No package created" -ForegroundColor Yellow
    } else {
        Write-Host "Status: Package ready for publication" -ForegroundColor Green
        Write-Host "Location: artifacts/FastGeoMesh.$version.nupkg" -ForegroundColor Green
    }

} catch {
    Write-Host ""
    Write-Host "Validation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "- Review package contents with: dotnet nuget verify artifacts/FastGeoMesh.$version.nupkg" -ForegroundColor Gray
Write-Host "- Test installation with: dotnet add package FastGeoMesh --source ./artifacts" -ForegroundColor Gray
Write-Host "- Publish with: dotnet nuget push artifacts/FastGeoMesh.$version.nupkg --source nuget.org --api-key YOUR_KEY" -ForegroundColor Gray
