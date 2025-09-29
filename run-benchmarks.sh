#!/bin/bash

# FastGeoMesh Benchmarks Runner Script
# Facilite l'exécution des différents types de benchmarks

set -e

BENCHMARK_PROJECT="FastGeoMesh.Benchmarks"
BUILD_CONFIG="Release"

# Couleurs pour l'affichage
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}FastGeoMesh Performance Benchmarks${NC}"
echo -e "${BLUE}===================================${NC}"
echo ""

# Fonction d'aide
show_help() {
    echo "Usage: $0 [OPTION]"
    echo ""
    echo "Options:"
    echo "  --geometry      Benchmarks géométriques (Vec2, Vec3, GeometryHelper)"
    echo "  --meshing       Benchmarks de maillage (PrismMesher, Options, Async)"
    echo "  --utils         Benchmarks utilitaires (Pools, Constants, Collections)"
    echo "  --collections   Benchmarks des FrozenCollections (.NET 8)"
    echo "  --async         Benchmarks Task vs ValueTask"
    echo "  --all           Tous les benchmarks"
    echo "  --filter PATTERN   Filtre personnalisé pour BenchmarkDotNet"
    echo "  --build         Compiler uniquement"
    echo "  --help          Afficher cette aide"
    echo ""
    echo "Exemples:"
    echo "  $0 --geometry                    # Benchmarks géométriques"
    echo "  $0 --filter \"*Vec2*\"             # Seulement Vec2"
    echo "  $0 --filter \"*Frozen*\"           # Seulement FrozenCollections"
}

# Fonction de build
build_project() {
    echo -e "${YELLOW}Building $BENCHMARK_PROJECT in $BUILD_CONFIG mode...${NC}"
    dotnet build "$BENCHMARK_PROJECT" -c "$BUILD_CONFIG" --nologo -v q
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Build successful${NC}"
    else
        echo -e "${RED}✗ Build failed${NC}"
        exit 1
    fi
}

# Fonction d'exécution des benchmarks
run_benchmark() {
    local category="$1"
    echo -e "${YELLOW}Running $category benchmarks...${NC}"
    echo ""
    
    cd "$BENCHMARK_PROJECT"
    dotnet run -c "$BUILD_CONFIG" --no-build -- "$category"
    cd ..
}

# Fonction d'exécution avec filtre personnalisé
run_filtered_benchmark() {
    local filter="$1"
    echo -e "${YELLOW}Running benchmarks with filter: $filter${NC}"
    echo ""
    
    cd "$BENCHMARK_PROJECT"
    dotnet run -c "$BUILD_CONFIG" --no-build -- --filter "$filter"
    cd ..
}

# Vérification de l'existence du projet
if [ ! -d "$BENCHMARK_PROJECT" ]; then
    echo -e "${RED}Error: $BENCHMARK_PROJECT directory not found${NC}"
    echo "Make sure you're running this script from the solution root directory."
    exit 1
fi

# Traitement des arguments
case "${1:-}" in
    --help|-h)
        show_help
        exit 0
        ;;
    --build)
        build_project
        echo -e "${GREEN}Build completed. Use other options to run benchmarks.${NC}"
        exit 0
        ;;
    --geometry)
        build_project
        run_benchmark "--geometry"
        ;;
    --meshing)
        build_project
        run_benchmark "--meshing"
        ;;
    --utils)
        build_project
        run_benchmark "--utils"
        ;;
    --collections)
        build_project
        run_benchmark "--collections"
        ;;
    --async)
        build_project
        run_benchmark "--async"
        ;;
    --all)
        build_project
        run_benchmark "--all"
        ;;
    --filter)
        if [ -z "${2:-}" ]; then
            echo -e "${RED}Error: --filter requires a pattern argument${NC}"
            show_help
            exit 1
        fi
        build_project
        run_filtered_benchmark "$2"
        ;;
    "")
        echo -e "${YELLOW}No option provided. Showing help...${NC}"
        echo ""
        show_help
        ;;
    *)
        echo -e "${RED}Error: Unknown option '$1'${NC}"
        echo ""
        show_help
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}Benchmarks completed!${NC}"
echo -e "${BLUE}Results are saved in BenchmarkDotNet.Artifacts/ directory${NC}"
