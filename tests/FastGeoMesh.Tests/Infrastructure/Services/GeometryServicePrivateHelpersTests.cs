using System.Reflection;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Infrastructure.Services
{
    public class GeometryServicePrivateHelpersTests
    {
        private static object InvokePrivate(object instance, string name, params object[] args)
        {
            var mi = instance.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Should().NotBeNull($"Private method {name} not found");
            return mi!.Invoke(instance, args)!;
        }

        [Fact]
        public void DoesEdgeCrossHorizontalRayEarlyExitWhenSameSideY()
        {
            var svc = new GeometryService();
            var a = new Vec2(0, 2); // both above pointY
            var b = new Vec2(1, 3);
            var pointY = 1.0;
            var pointX = 0.0;

            var res = (bool)InvokePrivate(svc, "DoesEdgeCrossHorizontalRay", a, b, pointX, pointY);
            res.Should().BeFalse();
        }

        [Fact]
        public void DoesEdgeCrossHorizontalRayBoundaryAndToggleCases()
        {
            var svc = new GeometryService();
            // Edge from (0,0) to (2,2) crosses horizontal line y=1 at x=1
            var a = new Vec2(0, 0);
            var b = new Vec2(2, 2);
            double y = 1.0;

            ((bool)InvokePrivate(svc, "DoesEdgeCrossHorizontalRay", a, b, 0.5, y)).Should().BeTrue();  // pointX < intersectionX
            ((bool)InvokePrivate(svc, "DoesEdgeCrossHorizontalRay", a, b, 1.0, y)).Should().BeFalse(); // equality => not strictly to the right
            ((bool)InvokePrivate(svc, "DoesEdgeCrossHorizontalRay", a, b, 1.5, y)).Should().BeFalse(); // pointX > intersectionX
        }

        [Fact]
        public void IsPointOnSegmentCoversDotAndToleranceBranches()
        {
            var svc = new GeometryService();
            // Access private IsPointOnSegment(px,py,ax,ay,bx,by,tol)
            string name = "IsPointOnSegment";

            // Off-line point (cross > tol) => false
            ((bool)InvokePrivate(svc, name, 1.0, 1.0, 0.0, 0.0, 2.0, 0.0, 1e-9)).Should().BeFalse();

            // Before segment start (dot < 0) on-line => false
            ((bool)InvokePrivate(svc, name, -0.1, 0.0, 0.0, 0.0, 2.0, 0.0, 1e-9)).Should().BeFalse();

            // After segment end (dot > len^2) on-line => false
            ((bool)InvokePrivate(svc, name, 2.1, 0.0, 0.0, 0.0, 2.0, 0.0, 1e-9)).Should().BeFalse();

            // Within segment bounds => true
            ((bool)InvokePrivate(svc, name, 1.0, 0.0, 0.0, 0.0, 2.0, 0.0, 1e-9)).Should().BeTrue();

            // Near-collinear within tolerance => true
            ((bool)InvokePrivate(svc, name, 1.0, 1e-12, 0.0, 0.0, 2.0, 0.0, 1e-9)).Should().BeTrue();
        }
    }
}
