using System;
using FastGeoMesh.Domain;

namespace FastGeoMesh.Meshing
{
    public class MesherOptionsBuilder
    {
        private readonly MesherOptions _options = new MesherOptions();
        public MesherOptionsBuilder SetTargetEdgeLengthXY(double value)
        {
            _options.TargetEdgeLengthXY = EdgeLength.From(value);
            return this;
        }
        public MesherOptionsBuilder SetTargetEdgeLengthZ(double value)
        {
            _options.TargetEdgeLengthZ = EdgeLength.From(value);
            return this;
        }
        public MesherOptionsBuilder SetGenerateBottomCap(bool value)
        {
            _options.GenerateBottomCap = value;
            return this;
        }
        public MesherOptionsBuilder SetGenerateTopCap(bool value)
        {
            _options.GenerateTopCap = value;
            return this;
        }
        public MesherOptionsBuilder SetEpsilon(double value)
        {
            _options.Epsilon = Tolerance.From(value);
            return this;
        }
        public MesherOptionsBuilder SetTargetEdgeLengthXYNearHoles(double? value)
        {
            _options.TargetEdgeLengthXYNearHoles = value.HasValue ? EdgeLength.From(value.Value) : null;
            return this;
        }
        public MesherOptionsBuilder SetHoleRefineBand(double value)
        {
            _options.HoleRefineBand = value;
            return this;
        }
        public MesherOptionsBuilder SetTargetEdgeLengthXYNearSegments(double? value)
        {
            _options.TargetEdgeLengthXYNearSegments = value.HasValue ? EdgeLength.From(value.Value) : null;
            return this;
        }
        public MesherOptionsBuilder SetSegmentRefineBand(double value)
        {
            _options.SegmentRefineBand = value;
            return this;
        }
        public MesherOptionsBuilder SetMinCapQuadQuality(double value)
        {
            _options.MinCapQuadQuality = value;
            return this;
        }
        public MesherOptionsBuilder SetOutputRejectedCapTriangles(bool value)
        {
            _options.OutputRejectedCapTriangles = value;
            return this;
        }
        public MesherOptions Build() => _options;
    }
}
