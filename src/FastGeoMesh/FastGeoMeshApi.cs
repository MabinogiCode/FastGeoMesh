// FastGeoMesh v2.0 - Clean Architecture Public API
// This file exposes the Clean Architecture layers directly for maximum clarity and maintainability.
// Breaking change from v1.x: Users must now reference the appropriate layer namespaces.

// Export main domain types for convenience
global using Vec2 = FastGeoMesh.Domain.Vec2;
global using Vec3 = FastGeoMesh.Domain.Vec3;
global using Polygon2D = FastGeoMesh.Domain.Polygon2D;
global using PrismStructureDefinition = FastGeoMesh.Domain.PrismStructureDefinition;
global using MesherOptions = FastGeoMesh.Domain.MesherOptions;
global using Mesh = FastGeoMesh.Domain.Mesh;
global using ImmutableMesh = FastGeoMesh.Domain.ImmutableMesh;
global using IndexedMesh = FastGeoMesh.Domain.IndexedMesh;

using FastGeoMesh.Domain;
using FastGeoMesh.Application;
using FastGeoMesh.Infrastructure.Exporters;

// V2.0 Breaking Changes:
// - Remove compatibility wrappers
// - Direct exposure of Clean Architecture
// - Simplified naming without layer conflicts
