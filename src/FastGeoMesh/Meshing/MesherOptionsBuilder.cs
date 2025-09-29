using System;

namespace FastGeoMesh.Meshing
{
    public sealed class MesherOptionsBuilder
    {
        private readonly MesherOptions _options = new MesherOptions();

        public MesherOptionsBuilder WithTargetEdgeLengthXY(double length) 
        { 
            _options.TargetEdgeLengthXY = length; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithTargetEdgeLengthZ(double length) 
        { 
            _options.TargetEdgeLengthZ = length; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithCaps(bool bottom = true, bool top = true) 
        { 
            _options.GenerateBottomCap = bottom; 
            _options.GenerateTopCap = top; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithEpsilon(double epsilon) 
        { 
            _options.Epsilon = epsilon; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithHoleRefinement(double targetLength, double band) 
        { 
            _options.TargetEdgeLengthXYNearHoles = targetLength; 
            _options.HoleRefineBand = band; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithSegmentRefinement(double targetLength, double band) 
        { 
            _options.TargetEdgeLengthXYNearSegments = targetLength; 
            _options.SegmentRefineBand = band; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithMinCapQuadQuality(double quality) 
        { 
            _options.MinCapQuadQuality = quality; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithRejectedCapTriangles(bool output = true) 
        { 
            _options.OutputRejectedCapTriangles = output; 
            return this; 
        }

        public MesherOptionsBuilder WithHighQualityPreset() 
        { 
            _options.TargetEdgeLengthXY = 0.5; 
            _options.TargetEdgeLengthZ = 0.5; 
            _options.MinCapQuadQuality = 0.7; 
            _options.OutputRejectedCapTriangles = true; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptionsBuilder WithFastPreset() 
        { 
            _options.TargetEdgeLengthXY = 2.0; 
            _options.TargetEdgeLengthZ = 2.0; 
            _options.MinCapQuadQuality = 0.3; 
            _options.OutputRejectedCapTriangles = false; 
            _options.ResetValidation(); 
            return this; 
        }

        public MesherOptions Build() 
        { 
            _options.Validate(); 
            return _options; 
        }
    }
}
