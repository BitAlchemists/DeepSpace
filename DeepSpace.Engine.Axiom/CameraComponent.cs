using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core;
using Axiom.Math;
using Axiom.Core;

namespace DeepSpace.Engine.Axiom
{
    public class CameraComponent : GameObjectComponent
    {
        protected Camera camera;
        internal Camera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                camera = value;
            }
        }
        Vector3 acceleration;
        Vector3 velocity;
        Vector3 rotation;

        public CameraComponent()
        {
            camera = null;
            acceleration = Vector3.Zero;
            velocity = Vector3.Zero;
            rotation = Vector3.Zero;
        }

        public override void Update(float dT)
        {
            float cameraAcceleration = 500 * dT;
            float rotationDegrees = 40.0f;

            velocity += (acceleration * cameraAcceleration);

            // move the camera based on the accumulated movement vector
            camera.MoveRelative(velocity * dT);
            
            rotation.Normalize();
            //camera.Rotate(rotation, rotationDegrees * dT);
            Quaternion q = Quaternion.FromAngleAxis(Utility.DegreesToRadians(rotationDegrees * dT), rotation);
            camera.Orientation = camera.Orientation * q;

            // Now dampen the Velocity - only if user is not accelerating
            if (acceleration == Vector3.Zero)
            {
                velocity *= (1 - (6 * dT)); //TE: seems risky if dT rises to greater than 1./6.
            }

            // reset acceleration zero
            acceleration = Vector3.Zero;
            rotation = Vector3.Zero;
        }

        public void MoveLeft()
        {
            acceleration.x = -1.0f;
        }

        public void MoveRight()
        {
            acceleration.x = 1.0f;
        }

        public void MoveUp()
        {
            acceleration.y = 1.0f;
        }

        public void MoveDown()
        {
            acceleration.y = -1.0f;
        }

        public void MoveForward()
        {
            acceleration.z = -1.0f;
        }

        public void MoveBackward()
        {
            acceleration.z = 1.0f;
        }

        public void YawLeft()
        {
            rotation.y = 1.0f;
        }

        public void YawRight()
        {
            rotation.y = -1.0f;
        }

        public void RollLeft()
        {
            rotation.z = 1.0f;
        }

        public void RollRight()
        {
            rotation.z = -1.0f;
        }

        public void PitchUp()
        {
            rotation.x = 1.0f;
        }

        public void PitchDown()
        {
            rotation.x = -1.0f;
        }

    }
}
