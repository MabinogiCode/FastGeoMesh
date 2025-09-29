@echo off
REM FastGeoMesh Benchmarks Runner Script (Windows)
REM Facilite l'exécution des différents types de benchmarks

setlocal enabledelayedexpansion

set BENCHMARK_PROJECT=FastGeoMesh.Benchmarks
set BUILD_CONFIG=Release

echo FastGeoMesh Performance Benchmarks
echo ===================================
echo.

REM Fonction d'aide
if "%~1"=="--help" goto :show_help
if "%~1"=="-h" goto :show_help
if "%~1"=="/?" goto :show_help

REM Vérification de l'existence du projet
if not exist "%BENCHMARK_PROJECT%" (
    echo Error: %BENCHMARK_PROJECT% directory not found
    echo Make sure you're running this script from the solution root directory.
    exit /b 1
)

REM Traitement des arguments
if "%~1"=="--build" goto :build_only
if "%~1"=="--geometry" goto :run_geometry
if "%~1"=="--meshing" goto :run_meshing
if "%~1"=="--utils" goto :run_utils
if "%~1"=="--collections" goto :run_collections
if "%~1"=="--async" goto :run_async
if "%~1"=="--all" goto :run_all
if "%~1"=="--filter" goto :run_filter

if "%~1"=="" (
    echo No option provided. Showing help...
    echo.
    goto :show_help
)

echo Error: Unknown option '%~1'
echo.
goto :show_help

:build_project
echo Building %BENCHMARK_PROJECT% in %BUILD_CONFIG% mode...
dotnet build "%BENCHMARK_PROJECT%" -c "%BUILD_CONFIG%" --nologo -v q
if errorlevel 1 (
    echo ✗ Build failed
    exit /b 1
) else (
    echo ✓ Build successful
)
goto :eof

:build_only
call :build_project
echo Build completed. Use other options to run benchmarks.
goto :end

:run_geometry
call :build_project
echo Running geometry benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --geometry
cd ..
goto :end

:run_meshing
call :build_project
echo Running meshing benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --meshing
cd ..
goto :end

:run_utils
call :build_project
echo Running utils benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --utils
cd ..
goto :end

:run_collections
call :build_project
echo Running collections benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --collections
cd ..
goto :end

:run_async
call :build_project
echo Running async benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --async
cd ..
goto :end

:run_all
call :build_project
echo Running all benchmarks...
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --all
cd ..
goto :end

:run_filter
if "%~2"=="" (
    echo Error: --filter requires a pattern argument
    goto :show_help
)
call :build_project
echo Running benchmarks with filter: %~2
echo.
cd "%BENCHMARK_PROJECT%"
dotnet run -c "%BUILD_CONFIG%" --no-build -- --filter "%~2"
cd ..
goto :end

:show_help
echo Usage: %~nx0 [OPTION]
echo.
echo Options:
echo   --geometry      Benchmarks géométriques (Vec2, Vec3, GeometryHelper)
echo   --meshing       Benchmarks de maillage (PrismMesher, Options, Async)
echo   --utils         Benchmarks utilitaires (Pools, Constants, Collections)
echo   --collections   Benchmarks des FrozenCollections (.NET 8)
echo   --async         Benchmarks Task vs ValueTask
echo   --all           Tous les benchmarks
echo   --filter PATTERN   Filtre personnalisé pour BenchmarkDotNet
echo   --build         Compiler uniquement
echo   --help          Afficher cette aide
echo.
echo Exemples:
echo   %~nx0 --geometry                    # Benchmarks géométriques
echo   %~nx0 --filter "*Vec2*"             # Seulement Vec2
echo   %~nx0 --filter "*Frozen*"           # Seulement FrozenCollections
goto :end

:end
echo.
echo Benchmarks completed!
echo Results are saved in BenchmarkDotNet.Artifacts/ directory
