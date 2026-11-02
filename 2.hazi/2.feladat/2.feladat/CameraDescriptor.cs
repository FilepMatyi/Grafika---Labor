using Silk.NET.Maths;

namespace GrafikaSzeminarium
{
    internal class CameraDescriptor
    {

        public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0, 2, 8);

        public double AngleYaw { get; private set; } = 0;

        public double AnglePitch { get; private set; } = -Math.PI / 180 * 14;

        const float MoveStepSize = 0.3f;

        const double AngleChangeStepSize = Math.PI / 180 * 5;

        public Vector3D<float> Forward
        {
            get
            {
                return GetDirectionFromAngles(AngleYaw, AnglePitch);
            }
        }

        public Vector3D<float> UpVector
        {
            get
            {
                return GetDirectionFromAngles(AngleYaw, AnglePitch + Math.PI / 2);
            }
        }

        public Vector3D<float> Target
        {
            get
            {
                return Position + Forward;
            }
        }

        public void MoveForward()
        {
            Position = Position + Forward * MoveStepSize;
        }

        public void MoveBackward()
        {
            Position = Position - Forward * MoveStepSize;
        }

        public void MoveUp()
        {
            Position = Position + new Vector3D<float>(0, MoveStepSize, 0);
        }

        public void MoveDown()
        {
            Position = Position - new Vector3D<float>(0, MoveStepSize, 0);
        }

        public void TurnLeft()
        {
            AngleYaw -= AngleChangeStepSize;
        }

        public void TurnRight()
        {
            AngleYaw += AngleChangeStepSize;
        }

        public void TiltUp()
        {
            AnglePitch += AngleChangeStepSize;
        }

        public void TiltDown()
        {
            AnglePitch -= AngleChangeStepSize;
        }

        private static Vector3D<float> GetDirectionFromAngles(double angleYaw, double anglePitch)
        {
            var x = Math.Cos(anglePitch) * Math.Sin(angleYaw);
            var y = Math.Sin(anglePitch);
            var z = -Math.Cos(anglePitch) * Math.Cos(angleYaw);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }
    }
}
