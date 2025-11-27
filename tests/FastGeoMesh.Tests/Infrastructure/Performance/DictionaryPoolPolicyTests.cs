using FastGeoMesh.Infrastructure.Performance;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Performance
{
    public class DictionaryPoolPolicyTests
    {
        [Fact]
        public void CreateReturnsEmptyDictionary()
        {
            var policy = new DictionaryPoolPolicy<int, string>();
            var dict = policy.Create();
            dict.Should().NotBeNull();
            dict.Count.Should().Be(0);
        }

        [Fact]
        public void ReturnClearsAndAcceptsReasonableSize()
        {
            var policy = new DictionaryPoolPolicy<int, string>();
            var dict = new Dictionary<int, string> { [1] = "a", [2] = "b" };
            var accepted = policy.Return(dict);
            accepted.Should().BeTrue();
            dict.Count.Should().Be(0);
        }

        [Fact]
        public void ReturnRejectsNull()
        {
            var policy = new DictionaryPoolPolicy<int, string>();
            policy.Return(null!).Should().BeFalse();
        }
    }
}
