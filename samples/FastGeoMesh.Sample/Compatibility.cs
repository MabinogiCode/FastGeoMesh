// Backward compatibility shim for legacy namespaces used in samples
// Provides empty namespace declarations so existing `using FastGeoMesh.*` lines compile
// and a global using to import the new Domain types (Polygon2D, PrismStructureDefinition, etc.).

global using FastGeoMesh.Domain;

namespace FastGeoMesh.Geometry { /* legacy namespace placeholder */ }
namespace FastGeoMesh.Structures { /* legacy namespace placeholder */ }
namespace FastGeoMesh.Core { /* legacy namespace placeholder */ }

