        [Theory]
        [InlineData(1.0, 1.0)]   // Perfect square
        [InlineData(2.0, 1.0)]   // Rectangle
        [InlineData(0.1, 1.0)]   // Very thin
        [InlineData(1.0, 0.1)]   // Very tall
        public void ScoreQuadHandlesDifferentAspectRatios(double width, double height)
        {
            // Arrange
            var quad = (
                new Vec2(0, 0), new Vec2(width, 0), 
                new Vec2(width, height), new Vec2(0, height)
            );

            // Act
            var score = QuadQualityHelper.ScoreQuad(quad);

            // Assert
            score.Should().BeInRange(0.0, 1.0, "Score should be in valid range");
            
            // Perfect square should have highest score
            if (Math.Abs(width - height) < 1e-9 && Math.Abs(width - 1.0) < 1e-9)
            {
                score.Should().BeGreaterThanOrEqualTo(0.8, "Perfect unit square must have score >= 0.8");
            }
        }