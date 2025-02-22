using Silk.NET.OpenGLES;
using System.Collections;
using System.Collections.Generic;

namespace THREE
{
    [Serializable]
    public class GLGeometries
    {
        private Hashtable geometries = new Hashtable();

        private Hashtable wireframeAttributes = new Hashtable();

        private GLAttributes Attributes;

        private GLInfo Info;

        private GLRenderer Renderer;

        public GLGeometries(GLRenderer renderer, GLAttributes attributes, GLInfo info)
        {
            this.Attributes = attributes;

            this.Info = info;

            this.Renderer = renderer;
        }


        public Geometry Get(Object3D obj, Geometry geometry)
        {
            BufferGeometry bufferGeometry;
            if (this.geometries.Contains(geometry))
            {
                return geometries[geometry] as Geometry;
            }
            else
            {
                bufferGeometry = null;
            }


            if (geometry.IsBufferGeometry)
            {
                bufferGeometry = (BufferGeometry)geometry;
            }
            else if (!geometry.IsBufferGeometry)
            {
                if ((geometry as Geometry).__bufferGeometry == null)
                {
                    geometry.__bufferGeometry = new BufferGeometry().SetFromObject(obj);

                }
                bufferGeometry = geometry.__bufferGeometry;
            }

            geometries.Add(geometry, bufferGeometry);

            Info.memory.Geometries++;

            return bufferGeometry;
        }

        public void Update(Geometry geometry)
        {
            if (geometry is not BufferGeometry bufferGeometry)
            {
                throw new ArgumentException("Expected geometry to be of type BufferGeometry", nameof(geometry));
            }

            var geometryAttributes = bufferGeometry.Attributes;

            foreach (var attribute in geometryAttributes.Values)
            {
                switch (attribute)
                {
                    case BufferAttribute<float> floatAttribute:
                        Attributes.Update(floatAttribute, GLEnum.ArrayBuffer);
                        break;
                    case BufferAttribute<int> intAttribute:
                        Attributes.Update(intAttribute, GLEnum.ArrayBuffer);
                        break;
                    case BufferAttribute<uint> uintAttribute:
                        Attributes.Update(uintAttribute, GLEnum.ArrayBuffer);
                        break;
                    case BufferAttribute<byte> byteAttribute:
                        Attributes.Update(byteAttribute, GLEnum.ArrayBuffer);
                        break;
                    case BufferAttribute<ushort> ushortAttribute:
                        Attributes.Update(ushortAttribute, GLEnum.ArrayBuffer);
                        break;
                }
            }

            var morphAttributes = bufferGeometry.MorphAttributes;

            foreach (var array in morphAttributes.Values)
            {
                if (array is List<BufferAttribute<float>> floatArray)
                {
                    foreach (var attribute in floatArray)
                    {
                        Attributes.Update(attribute, GLEnum.ArrayBuffer);
                    }
                }
            }
        }

        public void UpdateWireframeAttribute(BufferGeometry geometry)
        {
            List<int> index = new List<int>();

            var geometryIndex = geometry.Index;
            var geometryPosition = (BufferAttribute<float>)geometry.Attributes["position"];
            var version = 0;

            if (geometryIndex != null)
            {
                var array = geometryIndex.Array;
                version = geometryIndex.Version;

                for (int i = 0; i < array.Length; i += 3)
                {
                    var a = array[i + 0];
                    var b = array[i + 1];
                    var c = array[i + 2];

                    index.Add(a);
                    index.Add(b);
                    index.Add(b);
                    index.Add(c);
                    index.Add(c);
                    index.Add(a);
                }
            }
            else
            {
                var array = geometryPosition.Array;
                version = geometryPosition.Version;

                for (int i = 0; i < (array.Length / 3 - 1); i += 3)
                {
                    var a = i + 0;
                    var b = i + 1;
                    var c = i + 2;
                    index.Add(a);
                    index.Add(b);
                    index.Add(b);
                    index.Add(c);
                    index.Add(c);
                    index.Add(a);
                }
            }

            var attribute = new BufferAttribute<int>(index.ToArray(), 1);
            attribute.Version = version;

            // Updating index buffer in VAO now. See GLBindingStates
            //Attributes.Update(attribute, BufferTarget.ElementArrayBuffer);

            var previousAttribute = wireframeAttributes[geometry];

            if (previousAttribute != null) Attributes.Remove(previousAttribute);

            if (wireframeAttributes.Contains(geometry))
            {
                wireframeAttributes[geometry] = attribute;
            }
            else
            {
                wireframeAttributes.Add(geometry, attribute);
            }
        }

        public BufferAttribute<T> GetWireframeAttribute<T>(Geometry geometry)
        {
            var currentAttribute = wireframeAttributes[geometry];

            if (currentAttribute != null)
            {
                var geometryIndex = (geometry as BufferGeometry).Index;

                if (geometryIndex != null)
                {
                    if ((currentAttribute as BufferAttribute<int>).Version < geometryIndex.Version)
                    {
                        UpdateWireframeAttribute((BufferGeometry)geometry);
                    }
                }
            }
            else
            {
                UpdateWireframeAttribute((BufferGeometry)geometry);
            }
            return (BufferAttribute<T>)wireframeAttributes[geometry];
        }
    }
}
