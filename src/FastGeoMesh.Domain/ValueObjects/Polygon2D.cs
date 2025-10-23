namespace FastGeoMesh.Domain {
    /// <summary>Simple polygon in CCW order (no self-intersections).</summary>
    public sealed class Polygon2D {
        /// <summary>Ordered vertices (CCW).</summary>
        public IReadOnlyList<Vec2> Vertices { get; }

        /// <summary>Vertex count.</summary>
        public int Count => this.Vertices.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon2D"/> class from an enumerable of vertices (auto-CCW).
        /// </summary>
        /// <param name="verts">Enumerable of vertices to create polygon from.</param>
        public Polygon2D(IEnumerable<Vec2> verts) {
            var list = verts.ToList();

            if (list.Count < 3) {
                throw new ArgumentException("Polygon must have at least 3 vertices.");
            }

            if (SignedArea(list) < 0) {
                list.Reverse();
            }

            if (!Validate(list, out var error)) {
                throw new ArgumentException($"Invalid polygon: {error}");
            }

            this.Vertices = list;
        }

        /// <summary>Helper construct from points.</summary>
        /// <param name="verts">Enumerable of vertices to create polygon from.</param>
        /// <returns>A new <see cref="Polygon2D"/> instance.</returns>
        public static Polygon2D FromPoints(IEnumerable<Vec2> verts) => new(verts);

        /// <summary>Signed area (positive if CCW).</summary>
        /// <param name="verts">Vertices of polygon.</param>
        /// <returns>Signed area value (positive for CCW orientation).</returns>
        public static double SignedArea(IReadOnlyList<Vec2> verts) {
            ArgumentNullException.ThrowIfNull(verts);

            double a = 0;

            for (int i = 0, j = verts.Count - 1; i < verts.Count; j = i++) {
                a += (verts[j].X * verts[i].Y) - (verts[i].X * verts[j].Y);
            }

            return 0.5 * a;
        }

        /// <summary>Perimeter length.</summary>
        /// <returns>Perimeter length of the polygon.</returns>
        public double Perimeter() {
            double p = 0;

            for (int i = 0; i < this.Count; i++) {
                var a = this.Vertices[i];
                var b = this.Vertices[(i + 1) % this.Count];
                p += (b - a).Length();
            }

            return p;
        }

        /// <summary>Detect axis-aligned rectangle; returns bounding corner min/max.</summary>
        /// <param name="min">Lower-left corner if rectangle.</param>
        /// <param name="max">Upper-right corner if rectangle.</param>
        /// <param name="eps">Tolerance for alignment checks.</param>
        /// <returns>True if polygon is an axis-aligned rectangle.</returns>
        public bool IsRectangleAxisAligned(out Vec2 min, out Vec2 max, double eps = 1e-9) {
            min = new Vec2(double.PositiveInfinity, double.PositiveInfinity);
            max = new Vec2(double.NegativeInfinity, double.NegativeInfinity);

            if (this.Count != 4) {
                return false;
            }

            foreach (var v in this.Vertices) {
                if (v.X < min.X) {
                    min = new Vec2(v.X, min.Y);
                }

                if (v.Y < min.Y) {
                    min = new Vec2(min.X, v.Y);
                }

                if (v.X > max.X) {
                    max = new Vec2(v.X, max.Y);
                }

                if (v.Y > max.Y) {
                    max = new Vec2(max.X, v.Y);
                }
            }

            var corners = new HashSet<(double, double)>
            {
                (min.X, min.Y),
                (min.X, max.Y),
                (max.X, min.Y),
                (max.X, max.Y),
            };

            if (this.Vertices.Any(v => !corners.Contains((v.X, v.Y)))) {
                return false;
            }

            for (int i = 0; i < 4; i++) {
                var a = this.Vertices[i];
                var b = this.Vertices[(i + 1) % 4];

                if (Math.Abs(a.X - b.X) > eps && Math.Abs(a.Y - b.Y) > eps) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Validate polygon (non-degenerate, simple).</summary>
        /// <param name="verts">Vertices to validate.</param>
        /// <param name="error">Output error message when invalid.</param>
        /// <param name="eps">Tolerance used for numeric checks.</param>
        /// <returns>True if polygon is valid; otherwise false.</returns>
        public static bool Validate(IReadOnlyList<Vec2> verts, out string? error, double eps = 1e-9) {
            ArgumentNullException.ThrowIfNull(verts);
            error = null;

            int n = verts.Count;

            if (n < 3) {
                error = "Less than 3 vertices";
                return false;
            }

            if (Math.Abs(SignedArea(verts)) < eps) {
                error = "Degenerate area (collinear vertices)";
                return false;
            }

            for (int i = 0; i < n; i++) {
                var a = verts[i];
                var b = verts[(i + 1) % n];

                if ((b - a).Length() < eps) {
                    error = $"Zero-length edge at index {i}";
                    return false;
                }

                for (int j = i + 1; j < n; j++) {
                    var c = verts[j];
                    if ((c - a).Length() < eps) {
                        error = $"Duplicate/near-coincident vertices at indices {i} and {j}";
                        return false;
                    }
                }
            }

            for (int i = 0; i < n; i++) {
                var a1 = verts[i];
                var a2 = verts[(i + 1) % n];

                for (int j = i + 1; j < n; j++) {
                    if (j == i) {
                        continue;
                    }

                    if ((j == i + 1) || (i == 0 && j == n - 1)) {
                        continue;
                    }

                    var b1 = verts[j];
                    var b2 = verts[(j + 1) % n];

                    if (Polygon2DHelpers.SegmentsIntersect(a1, a2, b1, b2, eps)) {
                        error = $"Self-intersection between edges {i}-{(i + 1) % n} and {j}-{(j + 1) % n}";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
