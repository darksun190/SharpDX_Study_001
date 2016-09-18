using System;
using SharpDX;

namespace SharpDX_Study_001
{
    class Camera
    {
        public Camera()
        {
            mPosition = Vector3.Zero;
            mRight = Vector3.UnitX;
            mUp = Vector3.UnitY;
            mLook = Vector3.UnitZ;
            SetLens(0.2f * SharpDX.MathUtil.Pi, 1.0f, 1.0f, 1000.0f);
        }
        public Vector4 PositionXM
        {
            get
            {
                return new Vector4(mPosition, 0);
            }
        }
        public Vector3 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }
        public Vector4 RightXM
        {
            get
            {
                return new Vector4(mRight, 0);
            }
        }
        public Vector4 UpXM
        {
            get
            {
                return new Vector4(mUp, 0);
            }
        }
        public Vector4 LookXM
        {
            get
            {
                return new Vector4(mLook, 0);
            }
        }
        public Vector3 Right
        {
            get
            {
                return mRight;
            }
        }
        public Vector3 Look
        {
            get
            {
                return mLook;
            }
        }
        public Vector3 Up
        {
            get
            {
                return mUp;
            }
        }
        public float NearZ
        {
            get
            {
                return mNearZ;
            }
        }
        public float FarZ
        {
            get
            {
                return mFarZ;
            }
        }
        public float Aspect
        {
            get
            {
                return mAspect;
            }
        }
        public float FovY
        {
            get
            {
                return mFovY;
            }
        }
        public float FovX
        {
            get
            {
                float halfWidth = 0.5f * NearWindowWidth;
                return 2.0f * Convert.ToSingle(Math.Atan(halfWidth / mNearZ));
            }
        }
        public float NearWindowWidth
        {
            get
            {
                return mAspect * mNearWindowHeight;
            }
        }
        public float NearWindowHeight
        {
            get
            {
                return mNearWindowHeight;
            }
        }
        public float FarWindowWidth
        {
            get
            {
                return mAspect * mFarWindowHeight;
            }
        }
        public float FarWindowHeight
        {
            get
            {
                return mFarWindowHeight;
            }
        }

        /// <summary>
        /// Set frustum.
        /// </summary>
        /// <param name="fovY"></param>
        /// <param name="aspect"></param>
        /// <param name="zn"></param>
        /// <param name="zf"></param>
        public void SetLens(float fovY, float aspect, float zn, float zf)
        {
            // cache properties
            mFovY = fovY;
            mAspect = aspect;
            mNearZ = zn;
            mFarZ = zf;

            mNearWindowHeight = 2.0f * mNearZ * Convert.ToSingle(Math.Tan(0.5f * mFovY));
            mFarWindowHeight = 2.0f * mFarZ * Convert.ToSingle(Math.Tan(0.5f * mFovY));

            mProj = Matrix.PerspectiveFovLH(mFovY, mAspect, mNearZ, mFarZ);
        }

        //       // Define camera space via LookAt parameters.
        public void LookAt(Vector4 pos, Vector4 target, Vector4 worldUp)
        {
            Vector3 L = new Vector3(pos.X, pos.Y, pos.Z);
            Vector3 T = new Vector3(target.X, target.Y, target.Z);
            Vector3 U = new Vector3(worldUp.X, worldUp.Y, worldUp.Z);
            LookAt(L, T, U);
        }
        public void LookAt(Vector3 pos, Vector3 target, Vector3 up)
        {
            mPosition = pos;
            mLook = Vector3.Normalize(target - pos);
            mRight = Vector3.Normalize(Vector3.Cross(up, mLook));
            mUp = Vector3.Cross(mLook, mRight);
        }
        public Matrix View
        {
            get
            {
                return mView;
            }
        }
        public Matrix Proj
        {
            get
            {
                return mProj;
            }
        }
        public Matrix ViewProj
        {
            get
            {
                return View * Proj;
            }
        }
        // Strafe/Walk the camera a distance d.
        public void Strafe(float d)
        {
            // mPosition += d*mRight
            Vector4 s = new Vector4(d);

            var nPos = (s * RightXM + PositionXM);
            mPosition = new Vector3(nPos.X, nPos.Y, nPos.Z);
        }
        public void Walk(float d)
        {
            // mPosition += d*mLook
            Vector4 s = new Vector4(d);
            var nPos = (s * LookXM + PositionXM);
            mPosition = new Vector3(nPos.X, nPos.Y, nPos.Z);
        }

        // Rotate the camera.
        public void Pitch(float angle)
        {
            // Rotate up and look vector about the right vector.

            Matrix R = Matrix.RotationAxis(mRight, angle);
            mUp = Vector3.TransformNormal(mUp, R);
            mLook = Vector3.TransformNormal(mLook, R);
        }
        public void RotateY(float angle)
        {
            // Rotate the basis vectors about the world y-axis.

            Matrix R = Matrix.RotationY(angle);
            mRight = Vector3.TransformNormal(mRight, R);
            mUp = Vector3.TransformNormal(mUp, R);
            mLook = Vector3.TransformNormal(mLook, R);
        }

        // After modifying camera position/orientation, call to rebuild the view matrix.
        public void UpdateViewMatrix()
        {
            var R = mRight;
            var U = mUp;
            var L = mLook;
            var P = mPosition;

            // Keep camera's axes orthogonal to each other and of unit length.
            L = Vector3.Normalize(L);
            U = Vector3.Normalize(Vector3.Cross(mLook, mRight));

            // U, L already ortho-normal, so no need to normalize cross product.
            R = Vector3.Cross(U, L);

            // Fill in the view matrix entries.
            float x = -Vector3.Dot(P, R);
            float y = -Vector3.Dot(P, U);
            float z = -Vector3.Dot(P, L);

            mRight = R;
            mUp = U;
            mLook = L;

            mView.Column1 = new Vector4(mRight, x);
            mView.Column2 = new Vector4(mUp, y);
            mView.Column3 = new Vector4(mLook, z);
            mView.Column4 = Vector4.UnitW;
        }


        private Vector3 mPosition;

        private Vector3 mRight;

        private Vector3 mUp;

        private Vector3 mLook;


        // Cache frustum properties.
        float mNearZ;
        float mFarZ;
        float mAspect;
        float mFovY;
        float mNearWindowHeight;
        float mFarWindowHeight;

        Matrix mView;
        Matrix mProj;

    }
}
