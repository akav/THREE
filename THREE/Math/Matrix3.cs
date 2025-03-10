﻿using System;
using System.Diagnostics;


namespace THREE
{
    [Serializable]
    public class Matrix3 : IEquatable<Matrix3>, ICloneable
    {
        public float[] Elements = new float[9] {1,0,0,
                                                0,1,0,
                                                0,0,1};

        //public static readonly Matrix3 Identity = new Matrix3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        private Vector3 _vector = new Vector3();

        public Matrix3()
        {
        }
        public static Matrix3 Identity()
        {
            return new Matrix3();
        }
        public object Clone()
        {
            return new Matrix3().FromArray(this.Elements);
        }

        //public Matrix3 Identity()
        //{
        //    Set(1,0,0,
        //        0,1,0,
        //        0,0,1);
        //    return this;
        //}

        public Matrix3(float n11, float n12, float n13,
                        float n21, float n22, float n23,
                        float n31, float n32, float n33)
        {
            var te = this.Elements;

            te[0] = n11; te[1] = n21; te[2] = n31;
            te[3] = n12; te[4] = n22; te[5] = n32;
            te[6] = n13; te[7] = n23; te[8] = n33;
        }

        public Matrix3 Set(float n11, float n12, float n13,
                        float n21, float n22, float n23,
                        float n31, float n32, float n33)
        {
            var te = this.Elements;

            te[0] = n11; te[1] = n21; te[2] = n31;
            te[3] = n12; te[4] = n22; te[5] = n32;
            te[6] = n13; te[7] = n23; te[8] = n33;

            return this;
        }

        public Matrix3 Copy(Matrix3 m)
        {
            var te = this.Elements;
            var me = m.Elements;

            te[0] = me[0]; te[1] = me[1]; te[2] = me[2];
            te[3] = me[3]; te[4] = me[4]; te[5] = me[5];
            te[6] = me[6]; te[7] = me[7]; te[8] = me[8];

            return this;
        }

        public Matrix3 SetFromMatrix4(Matrix4 m)
        {

            var me = m.Elements;

            this.Set(
                me[0], me[4], me[8],
                me[1], me[5], me[9],
                me[2], me[6], me[10]
            );

            return this;
        }



        public BufferAttribute<float> ApplyToBufferAttribute(BufferAttribute<float> attribute)
        {

            for (int i = 0, l = attribute.count; i < l; i++)
            {

                _vector.X = attribute.GetX(i);
                _vector.Y = attribute.GetY(i);
                _vector.Z = attribute.GetZ(i);

                _vector.ApplyMatrix3(this);

                attribute.SetXYZ(i, _vector.X, _vector.Y, _vector.Z);

            }

            return attribute;

        }
        public Matrix3 Multiply(Matrix3 m)
        {
            return this.MultiplyMatrices(this, m);
        }

        public Matrix3 PreMultiply(Matrix3 m)
        {
            return this.MultiplyMatrices(m, this);
        }

        public Matrix3 MultiplyMatrices(Matrix3 a, Matrix3 b)
        {
            var ae = a.Elements;
            var be = b.Elements;
            var te = this.Elements;

            float a11 = ae[0], a12 = ae[3], a13 = ae[6];
            float a21 = ae[1], a22 = ae[4], a23 = ae[7];
            float a31 = ae[2], a32 = ae[5], a33 = ae[8];

            float b11 = be[0], b12 = be[3], b13 = be[6];
            float b21 = be[1], b22 = be[4], b23 = be[7];
            float b31 = be[2], b32 = be[5], b33 = be[8];

            te[0] = a11 * b11 + a12 * b21 + a13 * b31;
            te[3] = a11 * b12 + a12 * b22 + a13 * b32;
            te[6] = a11 * b13 + a12 * b23 + a13 * b33;

            te[1] = a21 * b11 + a22 * b21 + a23 * b31;
            te[4] = a21 * b12 + a22 * b22 + a23 * b32;
            te[7] = a21 * b13 + a22 * b23 + a23 * b33;

            te[2] = a31 * b11 + a32 * b21 + a33 * b31;
            te[5] = a31 * b12 + a32 * b22 + a33 * b32;
            te[8] = a31 * b13 + a32 * b23 + a33 * b33;

            return this;
        }

        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            var ae = a.Elements;
            var be = b.Elements;
            Matrix3 m = new Matrix3();
            var te = m.Elements;

            float a11 = ae[0], a12 = ae[3], a13 = ae[6];
            float a21 = ae[1], a22 = ae[4], a23 = ae[7];
            float a31 = ae[2], a32 = ae[5], a33 = ae[8];

            float b11 = be[0], b12 = be[3], b13 = be[6];
            float b21 = be[1], b22 = be[4], b23 = be[7];
            float b31 = be[2], b32 = be[5], b33 = be[8];

            te[0] = a11 * b11 + a12 * b21 + a13 * b31;
            te[3] = a11 * b12 + a12 * b22 + a13 * b32;
            te[6] = a11 * b13 + a12 * b23 + a13 * b33;

            te[1] = a21 * b11 + a22 * b21 + a23 * b31;
            te[4] = a21 * b12 + a22 * b22 + a23 * b32;
            te[7] = a21 * b13 + a22 * b23 + a23 * b33;

            te[2] = a31 * b11 + a32 * b21 + a33 * b31;
            te[5] = a31 * b12 + a32 * b22 + a33 * b32;
            te[8] = a31 * b13 + a32 * b23 + a33 * b33;

            return m;
        }

        public Matrix3 MultiplyScalar(float s)
        {

            var te = this.Elements;

            te[0] *= s; te[3] *= s; te[6] *= s;
            te[1] *= s; te[4] *= s; te[7] *= s;
            te[2] *= s; te[5] *= s; te[8] *= s;

            return this;

        }

        public float Determinant()
        {
            var te = this.Elements;

            float a = te[0], b = te[1], c = te[2],
                d = te[3], e = te[4], f = te[5],
                g = te[6], h = te[7], i = te[8];

            return a * e * i - a * f * h - b * d * i + b * f * g + c * d * h - c * e * g;

        }

        public Matrix3 GetInverse(Matrix3 matrix)
        {
            float[] me = matrix.Elements;
            float[] te = this.Elements;

            float n11 = me[0], n21 = me[1], n31 = me[2],
                  n12 = me[3], n22 = me[4], n32 = me[5],
                  n13 = me[6], n23 = me[7], n33 = me[8],

                  t11 = n33 * n22 - n32 * n23,
                  t12 = n32 * n13 - n33 * n12,
                 t13 = n23 * n12 - n22 * n13,

                det = n11 * t11 + n21 * t12 + n31 * t13;

            if (det == 0)
            {

                Trace.TraceError("THREE.Math.Matrix3 : GetInverse() can't invert matrix, determinant is 0");

                return new Matrix3();

            }

            var detInv = 1 / det;

            te[0] = t11 * detInv;
            te[1] = (n31 * n23 - n33 * n21) * detInv;
            te[2] = (n32 * n21 - n31 * n22) * detInv;

            te[3] = t12 * detInv;
            te[4] = (n33 * n11 - n31 * n13) * detInv;
            te[5] = (n31 * n12 - n32 * n11) * detInv;

            te[6] = t13 * detInv;
            te[7] = (n21 * n13 - n23 * n11) * detInv;
            te[8] = (n22 * n11 - n21 * n12) * detInv;

            return this;

        }

        public Matrix3 Inverted()
        {
            Matrix3 m = (Matrix3)Clone();
            return GetInverse(m);
        }

        public Matrix3 Transpose()
        {
            float tmp;
            var m = this.Elements;

            tmp = m[1]; m[1] = m[3]; m[3] = tmp;
            tmp = m[2]; m[2] = m[6]; m[6] = tmp;
            tmp = m[5]; m[5] = m[7]; m[7] = tmp;

            return this;
        }

        public Matrix3 Transposed()
        {
            Matrix3 m = (Matrix3)Clone();

            return m.Transpose();
        }

        public Matrix3 GetNormalMatrix(Matrix4 matrix4)
        {
            return this.SetFromMatrix4(matrix4).GetInverse(this).Transpose();
        }

        public Matrix3 transposeIntoArray(float[] r)
        {
            var m = this.Elements;

            r[0] = m[0];
            r[1] = m[3];
            r[2] = m[6];
            r[3] = m[1];
            r[4] = m[4];
            r[5] = m[7];
            r[6] = m[2];
            r[7] = m[5];
            r[8] = m[8];

            return this;
        }

        public void SetUvTransform(float tx, float ty, float sx, float sy, float rotation, float cx, float cy)
        {

            var c = (float)System.Math.Cos(rotation);
            var s = (float)System.Math.Sin(rotation);

            this.Set(
                sx * c, sx * s, -sx * (c * cx + s * cy) + cx + tx,
                -sy * s, sy * c, -sy * (-s * cx + c * cy) + cy + ty,
                0, 0, 1
            );
        }

        public Matrix3 Scale(float sx, float sy)
        {

            var te = this.Elements;

            te[0] *= sx; te[3] *= sx; te[6] *= sx;
            te[1] *= sy; te[4] *= sy; te[7] *= sy;

            return this;

        }

        public Matrix3 Rotate(float theta)
        {

            var c = (float)System.Math.Cos(theta);
            var s = (float)System.Math.Sin(theta);

            var te = this.Elements;

            float a11 = te[0], a12 = te[3], a13 = te[6];
            float a21 = te[1], a22 = te[4], a23 = te[7];

            te[0] = c * a11 + s * a21;
            te[3] = c * a12 + s * a22;
            te[6] = c * a13 + s * a23;

            te[1] = -s * a11 + c * a21;
            te[4] = -s * a12 + c * a22;
            te[7] = -s * a13 + c * a23;

            return this;
        }

        public Matrix3 Translate(float tx, float ty)
        {
            var te = this.Elements;

            te[0] += tx * te[2]; te[3] += tx * te[5]; te[6] += tx * te[8];
            te[1] += ty * te[2]; te[4] += ty * te[5]; te[7] += ty * te[8];

            return this;
        }

        public Matrix3 FromArray(float[] array, int? offset = null)
        {

            int index = 0;
            if (offset != null) index = (int)offset;

            for (var i = 0; i < 9; i++)
            {

                this.Elements[i] = array[i + index];

            }

            return this;
        }

        public bool Equals(Matrix3 matrix)
        {

            var te = this.Elements;
            var me = matrix.Elements;

            for (var i = 0; i < 9; i++)
            {

                if (te[i] != me[i]) return false;
            }
            return true;
        }


        public float[] ToArray(float[] array = null, int? offset = null)
        {

            int index = 0;
            if (array == null) array = new float[9];
            if (offset != null) index = (int)offset;

            var te = this.Elements;

            array[index] = te[0];
            array[index + 1] = te[1];
            array[index + 2] = te[2];

            array[index + 3] = te[3];
            array[index + 4] = te[4];
            array[index + 5] = te[5];

            array[index + 6] = te[6];
            array[index + 7] = te[7];
            array[index + 8] = te[8];

            return array;

        }

    }
}
