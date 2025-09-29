using System;
using FastGeoMesh.Geometry;
using FastGeoMesh.Utils;

// Test simple pour vérifier si PointInPolygon fonctionne
var square = new Vec2[]
{
    new(0, 0), new(10, 0), new(10, 10), new(0, 10)
};

// Test du point central (5,5) - doit être TRUE
bool centerInside = GeometryHelper.PointInPolygon(square, 5, 5);
Console.WriteLine($"Point (5,5) inside square: {centerInside}");

// Test de points sur les bords - doivent être TRUE
bool cornerInside = GeometryHelper.PointInPolygon(square, 0, 0);
Console.WriteLine($"Point (0,0) on corner: {cornerInside}");

bool edgeInside = GeometryHelper.PointInPolygon(square, 5, 0);
Console.WriteLine($"Point (5,0) on edge: {edgeInside}");

// Test de points à l'extérieur - doivent être FALSE
bool outsideLeft = GeometryHelper.PointInPolygon(square, -1, 5);
Console.WriteLine($"Point (-1,5) outside left: {outsideLeft}");

bool outsideRight = GeometryHelper.PointInPolygon(square, 11, 5);
Console.WriteLine($"Point (11,5) outside right: {outsideRight}");

// Test pour vérifier l'algorithme
Console.WriteLine("\n=== RESULTS ===");
Console.WriteLine($"CENTER (5,5): {(centerInside ? "✅ PASS" : "❌ FAIL")}");
Console.WriteLine($"CORNER (0,0): {(cornerInside ? "✅ PASS" : "❌ FAIL")}");
Console.WriteLine($"EDGE (5,0): {(edgeInside ? "✅ PASS" : "❌ FAIL")}");
Console.WriteLine($"OUTSIDE (-1,5): {(!outsideLeft ? "✅ PASS" : "❌ FAIL")}");
Console.WriteLine($"OUTSIDE (11,5): {(!outsideRight ? "✅ PASS" : "❌ FAIL")}");

// Test SpatialPolygonIndex aussi
var spatialIndex = new SpatialPolygonIndex(square);
bool spatialCenterInside = spatialIndex.IsInside(5, 5);
Console.WriteLine($"\nSpatialIndex (5,5): {(spatialCenterInside ? "✅ PASS" : "❌ FAIL")}");
