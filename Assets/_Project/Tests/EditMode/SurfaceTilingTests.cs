using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace NightShift.Tests
{
    public class SurfaceTilingTests
    {
        [Test]
        public void ForCube_WallAlongX_UsesLengthAndHeight()
        {
            Vector2 tiling = SurfaceTiling.ForCube(new Vector3(4f, 2.4f, 0.05f), 2f);

            Assert.That(tiling, Is.EqualTo(new Vector2(2f, 1.2f)).Using(Vector2EqualityComparer.Instance));
        }

        [Test]
        public void ForCube_WallAlongZ_UsesDepthAndHeight()
        {
            Vector2 tiling = SurfaceTiling.ForCube(new Vector3(0.1f, 2.4f, 4.8f), 2f);

            Assert.That(tiling, Is.EqualTo(new Vector2(2.4f, 1.2f)).Using(Vector2EqualityComparer.Instance));
        }

        [Test]
        public void ForCube_Floor_UsesWidthAndDepth()
        {
            Vector2 tiling = SurfaceTiling.ForCube(new Vector3(5f, 0.1f, 3f), 1f);

            Assert.That(tiling, Is.EqualTo(new Vector2(5f, 3f)).Using(Vector2EqualityComparer.Instance));
        }

        [Test]
        public void ForCube_ThinnestTie_PrefersX()
        {
            Vector2 tiling = SurfaceTiling.ForCube(new Vector3(0.1f, 0.1f, 2f), 1f);

            Assert.That(tiling, Is.EqualTo(new Vector2(2f, 0.1f)).Using(Vector2EqualityComparer.Instance));
        }
    }
}
