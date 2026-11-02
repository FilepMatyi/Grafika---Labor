using Silk.NET.Maths;

namespace GrafikaSzeminarium
{
    internal class RubikCubeArrangementModel
    {

        public const float CubieSpacing = 1.05f;


        private const double RotationSpeed = Math.PI;

        private const int TopFace = 0;
        private const int FrontFace = 1;
        private const int LeftFace = 2;
        private const int BottomFace = 3;
        private const int BackFace = 4;
        private const int RightFace = 5;

        private const int FaceCount = 6;
        private const int VerticesPerFace = 4;
        private const int ColorComponents = 4;

        private static readonly float[] White = { 1.0f, 1.0f, 1.0f, 1.0f };
        private static readonly float[] Yellow = { 1.0f, 0.85f, 0.0f, 1.0f };
        private static readonly float[] Green = { 0.0f, 0.6f, 0.2f, 1.0f };
        private static readonly float[] Blue = { 0.0f, 0.3f, 0.9f, 1.0f };
        private static readonly float[] Orange = { 1.0f, 0.5f, 0.0f, 1.0f };
        private static readonly float[] Red = { 0.85f, 0.0f, 0.0f, 1.0f };

        private static readonly float[] Inner = { 0.15f, 0.15f, 0.15f, 1.0f };

        public List<Matrix4X4<float>> CubieModelMatrices { get; } = new List<Matrix4X4<float>>();

        public double TopLayerAngle { get; private set; } = 0;

        private int rotationDirection = 0;

        public bool IsRotating
        {
            get
            {
                return rotationDirection != 0;
            }
        }

        public Matrix4X4<float> TopLayerRotationMatrix
        {
            get
            {
                return Matrix4X4.CreateRotationY((float)TopLayerAngle);
            }
        }

        public void AddCubie(int x, int y, int z)
        {
            CubieModelMatrices.Add(Matrix4X4.CreateTranslation(GetCubiePosition(x, y, z)));
        }

        public bool IsOnTopLayer(int cubieIndex)
        {
            return CubieModelMatrices[cubieIndex].M42 > CubieSpacing / 2;
        }

        public void StartTopLayerRotation(int direction)
        {
            if (IsRotating)
                return;

            rotationDirection = direction;
        }

        internal void AdvanceTime(double deltaTime)
        {
            if (!IsRotating)
                return;

            TopLayerAngle += rotationDirection * RotationSpeed * deltaTime;

            if (Math.Abs(TopLayerAngle) >= Math.PI / 2)
            {
                var finalRotation = Matrix4X4.CreateRotationY((float)(rotationDirection * Math.PI / 2));
                for (int i = 0; i < CubieModelMatrices.Count; i++)
                {
                    if (IsOnTopLayer(i))
                    {
                        CubieModelMatrices[i] = CubieModelMatrices[i] * finalRotation;
                    }
                }

                TopLayerAngle = 0;
                rotationDirection = 0;
            }
        }

        public static float[] GetCubieColorArray(int x, int y, int z)
        {
            float[][] faceColors = new float[FaceCount][];
            faceColors[TopFace] = y == 1 ? White : Inner;
            faceColors[FrontFace] = z == 1 ? Green : Inner;
            faceColors[LeftFace] = x == -1 ? Orange : Inner;
            faceColors[BottomFace] = y == -1 ? Yellow : Inner;
            faceColors[BackFace] = z == -1 ? Blue : Inner;
            faceColors[RightFace] = x == 1 ? Red : Inner;

            float[] colorArray = new float[FaceCount * VerticesPerFace * ColorComponents];
            int index = 0;
            for (int face = 0; face < FaceCount; face++)
            {
                for (int vertex = 0; vertex < VerticesPerFace; vertex++)
                {
                    for (int component = 0; component < ColorComponents; component++)
                    {
                        colorArray[index++] = faceColors[face][component];
                    }
                }
            }

            return colorArray;
        }

        public static Vector3D<float> GetCubiePosition(int x, int y, int z)
        {
            return new Vector3D<float>(x * CubieSpacing, y * CubieSpacing, z * CubieSpacing);
        }
    }
}
