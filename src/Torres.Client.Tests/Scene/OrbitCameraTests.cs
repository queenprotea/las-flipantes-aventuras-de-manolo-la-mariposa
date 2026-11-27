using Microsoft.Xna.Framework;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class OrbitCameraTests
    {
        private const int Precision = 3;
        private const float InitialDistanceToCenter = 17.5f;
        private const float MinimumDistanceToCenter = 6f;
        private const float MaximumDistanceToCenter = 24f;
        private const float FarZoomInWheelDelta = 5000f;
        private const float FarZoomOutWheelDelta = -5000f;
        private const float FarDragPixels = 1000f;
        private const float HeightAtTheMaximumPitch = 17.4334f;
        private const float HeightAtTheMinimumPitch = 4.5293f;
        private const float SquareAspectRatio = 1f;
        private const float SquareFocalScale = 3.1716f;

        private static readonly Vector3 _orbitCenter = new Vector3(0f, 0f, 1.15f);
        private static readonly Vector2 _farDragUp = new Vector2(0f, FarDragPixels);
        private static readonly Vector2 _farDragDown = new Vector2(0f, -FarDragPixels);

        private readonly OrbitCamera _camera = new OrbitCamera();

        [Fact]
        public void Test_CreateViewMatrix_WithoutMoving_PlacesTheCameraAtTheInitialDistance()
        {
            Matrix viewMatrix = _camera.CreateViewMatrix();

            float distanceToCenter = Vector3.Distance(FindCameraPosition(viewMatrix), _orbitCenter);
            Assert.Equal(InitialDistanceToCenter, distanceToCenter, Precision);
        }

        [Fact]
        public void Test_Zoom_FarIn_StopsAtTheMinimumDistance()
        {
            _camera.Zoom(FarZoomInWheelDelta);

            float distanceToCenter = Vector3.Distance(FindCameraPosition(_camera.CreateViewMatrix()), _orbitCenter);
            Assert.Equal(MinimumDistanceToCenter, distanceToCenter, Precision);
        }

        [Fact]
        public void Test_Zoom_FarOut_StopsAtTheMaximumDistance()
        {
            _camera.Zoom(FarZoomOutWheelDelta);

            float distanceToCenter = Vector3.Distance(FindCameraPosition(_camera.CreateViewMatrix()), _orbitCenter);
            Assert.Equal(MaximumDistanceToCenter, distanceToCenter, Precision);
        }

        [Fact]
        public void Test_Rotate_FarUp_StopsAtTheMaximumPitch()
        {
            _camera.Rotate(_farDragUp);

            float cameraHeight = FindCameraPosition(_camera.CreateViewMatrix()).Y;
            Assert.Equal(HeightAtTheMaximumPitch, cameraHeight, Precision);
        }

        [Fact]
        public void Test_Rotate_FarDown_StopsAtTheMinimumPitch()
        {
            _camera.Rotate(_farDragDown);

            float cameraHeight = FindCameraPosition(_camera.CreateViewMatrix()).Y;
            Assert.Equal(HeightAtTheMinimumPitch, cameraHeight, Precision);
        }

        [Fact]
        public void Test_CreateProjectionMatrix_OfASquareWindow_UsesTheFieldOfViewOfTheCamera()
        {
            Matrix projectionMatrix = OrbitCamera.CreateProjectionMatrix(SquareAspectRatio);

            Assert.Equal(SquareFocalScale, projectionMatrix.M11, Precision);
        }

        private static Vector3 FindCameraPosition(Matrix viewMatrix)
        {
            Vector3 cameraPosition = Matrix.Invert(viewMatrix).Translation;

            return cameraPosition;
        }
    }
}
