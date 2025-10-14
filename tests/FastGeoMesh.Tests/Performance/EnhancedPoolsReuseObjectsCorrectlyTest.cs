using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Performance;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    public sealed class EnhancedPoolsReuseObjectsCorrectlyTest
    {
        [Fact]
        public void Test()
        {
            var list1 = MeshingPools.IntListPool.Get();
            var list2 = MeshingPools.DoubleListPool.Get();
            var vec2List = MeshingPools.Vec2ListPool.Get();

            list1.Add(1);
            list1.Add(2);
            list2.Add(1.5);
            vec2List.Add(new Vec2(1, 2));

            MeshingPools.IntListPool.Return(list1);
            MeshingPools.DoubleListPool.Return(list2);
            MeshingPools.Vec2ListPool.Return(vec2List);

            var reusedList1 = MeshingPools.IntListPool.Get();
            var reusedList2 = MeshingPools.DoubleListPool.Get();
            var reusedVec2List = MeshingPools.Vec2ListPool.Get();

            reusedList1.Should().NotBeNull();
            reusedList1.Count.Should().Be(0);
            reusedList2.Count.Should().Be(0);
            reusedVec2List.Count.Should().Be(0);

            MeshingPools.IntListPool.Return(reusedList1);
            MeshingPools.DoubleListPool.Return(reusedList2);
            MeshingPools.Vec2ListPool.Return(reusedVec2List);
        }
    }
}
