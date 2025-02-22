using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace THREE
{
    [Serializable]
    public class GLPrograms
    {
        public List<GLProgram> Programs = new List<GLProgram>();

        private bool isGL2;

        private bool logarithmicDepthBuffer;

        private bool floatVertexTextures;

        private string precision;

        private int maxVertexUniforms;

        private bool vertexTextures;

        private Dictionary<string, string> shaderIDs = new Dictionary<string, string>();

        private List<string> parameterNames;

        private GLCapabilities capabilities;

        private GLRenderer renderer;

        private GLExtensions extensions;

        public ShaderLib ShaderLib = Global.ShaderLib;

        private GLCubeMap cubeMaps;

        private GLBindingStates bindingStates;

        private GLClipping clipping;
        public GLPrograms(GLRenderer renderer, GLCubeMap cubeMaps, GLExtensions extension, GLCapabilities capabilities, GLBindingStates bindingStates, GLClipping clipping)
        {

            this.maxVertexUniforms = capabilities.maxVertexUniforms;
            this.vertexTextures = capabilities.vertexTextures;

            this.capabilities = capabilities;
            this.renderer = renderer;
            this.extensions = extension;

            this.cubeMaps = cubeMaps;
            this.bindingStates = bindingStates;
            this.clipping = clipping;

            this.isGL2 = capabilities.IsGL2;
            this.logarithmicDepthBuffer = capabilities.logarithmicDepthBuffer;
            this.floatVertexTextures = capabilities.floatVertexTextures;

            this.precision = capabilities.precision;

            shaderIDs.Add("MeshDepthMaterial", "depth");
            shaderIDs.Add("MeshDistanceMaterial", "distanceRGBA");
            shaderIDs.Add("MeshNormalMaterial", "normal");
            shaderIDs.Add("MeshBasicMaterial", "basic");
            shaderIDs.Add("MeshLambertMaterial", "lambert");
            shaderIDs.Add("MeshPhongMaterial", "phong");
            shaderIDs.Add("MeshToonMaterial", "toon");
            shaderIDs.Add("MeshStandardMaterial", "physical");
            shaderIDs.Add("MeshPhysicalMaterial", "physical");
            shaderIDs.Add("MeshMatcapMaterial", "matcap");
            shaderIDs.Add("LineBasicMaterial", "basic");
            shaderIDs.Add("LineDashedMaterial", "dashed");
            shaderIDs.Add("PointsMaterial", "points");
            shaderIDs.Add("ShadowMaterial", "shadow");
            shaderIDs.Add("SpriteMaterial", "sprite");

            parameterNames = new List<string>() {
                "precision", "isGL2", "supportsVertexTextures", "outputEncoding", "instancing", "instancingColor",
                "map", "mapEncoding", "matcap", "matcapEncoding", "envMap", "envMapMode", "envMapEncoding", "envMapCubeUV",
                "lightMap", "lightMapEncoding","aoMap", "emissiveMap", "emissiveMapEncoding", "bumpMap", "normalMap", "objectSpaceNormalMap", "tangentSpaceNormalMap", "clearcoatNormalMap", "displacementMap", "specularMap",
                "roughnessMap", "metalnessMap", "gradientMap",
                "alphaMap", "combine", "vertexColors", "vertexTangents", "vertexUvs", "uvsVertexOnly","fog", "useFog", "fogExp2",
                "flatShading", "sizeAttenuation", "logarithmicDepthBuffer", "skinning",
                "maxBones", "useVertexTexture", "morphTargets", "morphNormals",
                "maxMorphTargets", "maxMorphNormals", "premultipliedAlpha",
                "numDirLights", "numPointLights", "numSpotLights", "numHemiLights", "numRectAreaLights",
                "numDirLightShadows", "numPointLightShadows", "numSpotLightShadows",
                "shadowMapEnabled", "shadowMapType", "toneMapping", "physicallyCorrectLights",
                "alphaTest", "doubleSided", "flipSided", "numClippingPlanes", "numClipIntersection", "depthPacking", "dithering",
                "sheen","transmission","transmissionMap"};

        }

        private int GetMaxBones(SkinnedMesh skinnedMesh)
        {
            var skeleton = skinnedMesh.Skeleton;
            var bones = skeleton.Bones;

            if (this.floatVertexTextures)
            {
                return 1024;
            }
            else
            {
                // default for when object is not specified
                // ( for example when prebuilding shader to be used with multiple objects )
                //
                //  - leave some extra space for other uniforms
                //  - limit here is ANGLE's 254 max uniform vectors
                //    (up to 54 should be safe)

                var nVertexUniforms = maxVertexUniforms;
                var nVertexMatrices = (int)System.Math.Floor((float)((nVertexUniforms - 20) / 4));

                var maxBones = System.Math.Min(nVertexMatrices, bones.Length);

                if (maxBones < bones.Length)
                {
                    Trace.TraceWarning("THREE.Renderers.GLRenderer: Skeleton has" + bones.Length + " bones. This GPU supports " + maxBones + ".");
                    return 0;
                }

                return maxBones;
            }
        }
        private int GetTextureEncodingFromMap(Texture map)
        {
            int encoding = Constants.LinearEncoding;

            if (map != null)
            {
                encoding = Constants.LinearEncoding;
            }
            else if (map is Texture)
            {
                encoding = map.Encoding;
            }
            else if (map is GLRenderTarget)
            {
                //    Trace.TraceWarning("THREE.Renderers.gl.GLPrograms.GetTextureEncodingFromMap: don't use render targets as textures. Use their property instead.");
                encoding = (map as GLRenderTarget).Texture.Encoding;
            }

            return encoding;
        }


        public Hashtable GetParameters(Material material, GLLights lights, List<Light> shadows, Object3D scene, Object3D object3D)
        {
            Fog fog = (scene as Scene)?.Fog;

            Texture environment = material is MeshStandardMaterial ? (scene as Scene)?.Environment : null;

            Texture envMap = cubeMaps.Get(material.EnvMap ?? environment);

            string shaderID = shaderIDs.ContainsKey(material.type) ? shaderIDs[material.type] : "";

            int maxBones = object3D is SkinnedMesh skinnedMesh ? GetMaxBones(skinnedMesh) : 0;

            if (!string.IsNullOrEmpty(material.Precision))
            {
                this.precision = this.capabilities.GetMaxPrecision(material.Precision);

                if (!this.precision.Equals(material.Precision))
                {
                    Trace.TraceWarning("THREE.Renderers.gl.GLPrograms.GetParameters:" + material.Precision + " not supported. using " + this.precision + " instead.");
                }
            }

            string vertexShader;
            string fragmentShader;

            if (!string.IsNullOrEmpty(shaderID))
            {
                GLShader shader = (GLShader)ShaderLib[shaderID];
                vertexShader = shader.VertexShader;
                fragmentShader = shader.FragmentShader;
            }
            else
            {
                vertexShader = (material as ShaderMaterial)?.VertexShader;
                fragmentShader = (material as ShaderMaterial)?.FragmentShader;
            }

            GLRenderTarget currentRenderTarget = this.renderer.GetRenderTarget();

            Hashtable parameters = new Hashtable
            {
                { "isGL2", isGL2 },
                { "shaderId", shaderID },
                { "shaderName", material.type },
                { "vertexShader", vertexShader },
                { "fragmentShader", fragmentShader },
                { "defines", material.Defines },
                { "isRawShaderMaterial", material is RawShaderMaterial },
                { "glslVersion", material.glslVersion },
                { "precision", precision },
                { "instancing", object3D is InstancedMesh },
                { "instancingColor", object3D is InstancedMesh instancedMesh && instancedMesh.InstanceColor != null },
                { "supportsVertexTextures", vertexTextures },
                { "outputEncoding", GetTextureEncodingFromMap(currentRenderTarget?.Texture) },
                { "map", material.Map != null },
                { "mapEncoding", GetTextureEncodingFromMap(material.Map) },
                { "matcap", material is MeshMatcapMaterial matcapMaterial && matcapMaterial.Matcap != null },
                { "matcapEncoding", material is MeshMatcapMaterial ? GetTextureEncodingFromMap((material as MeshMatcapMaterial)?.Matcap) : Constants.LinearEncoding },
                { "envMap", envMap != null },
                { "envMapMode", material.EnvMap?.Mapping },
                { "envMapEncoding", GetTextureEncodingFromMap(material.EnvMap) },
                { "envMapCubeUV", material.EnvMap != null && (material.EnvMap.Mapping == Constants.CubeUVReflectionMapping || material.EnvMap.Mapping == Constants.CubeUVRefractionMapping) },
                { "lightMap", material.LightMap != null },
                { "lightMapEncoding", GetTextureEncodingFromMap(material.LightMap) },
                { "aoMap", material.AoMap != null },
                { "emissiveMap", material.EmissiveMap != null },
                { "emissiveMapEncoding", GetTextureEncodingFromMap(material.EmissiveMap) },
                { "bumpMap", material.BumpMap != null },
                { "normalMap", material.NormalMap != null },
                { "objectSpaceNormalMap", material.NormalMapType == Constants.ObjectSpaceNormalMap },
                { "tangentSpaceNormalMap", material.NormalMapType == Constants.TangentSpaceNormalMap },
                { "clearcoatMap", material.ClearcoatMap != null },
                { "clearcoatRoughnessMap", material.ClearcoatRoughnessMap != null },
                { "clearcoatNormalMap", material.ClearcoatNormalMap != null },
                { "displacementMap", material.DisplacementMap != null },
                { "roughnessMap", material.RoughnessMap != null },
                { "metalnessMap", material.MetalnessMap != null },
                { "specularMap", material.SpecularMap != null },
                { "alphaMap", material.AlphaMap != null },
                { "gradientMap", material.GradientMap != null },
                { "sheen", material.Sheen != null },
                { "transmission", material.Transmission > 0 },
                { "combine", material.Combine != 0 ? material.Combine : (int?)null },
                { "transmissionMap", material.TransmissionMap != null },
                { "thicknessMap", material.ThicknessMap != null },
                { "vertexTangents", material.NormalMap != null && material.VertexTangents },
                { "vertexColors", material.VertexColors },
                { "vertexAlphas", material.VertexColors && object3D.Geometry is BufferGeometry bufferGeometry && bufferGeometry.Attributes.ContainsKey("color") && bufferGeometry.Attributes["color"] is IGLAttribute colorAttribute && colorAttribute.ItemSize == 4 },
                { "vertexUvs", material.Map != null || material.BumpMap != null || material.NormalMap != null || material.SpecularMap != null || material.AlphaMap != null || material.EmissiveMap != null || material.RoughnessMap != null || material.MetalnessMap != null || material.ClearcoatNormalMap != null },
                { "uvsVertexOnly", !(material.Map != null || material.BumpMap != null || material.NormalMap != null || material.SpecularMap != null || material.AlphaMap != null || material.EmissiveMap != null || material.RoughnessMap != null || material.MetalnessMap != null || material.ClearcoatNormalMap != null) && material.DisplacementMap != null },
                { "fog", fog != null },
                { "useFog", material.Fog },
                { "fogExp2", fog is FogExp2 },
                { "flatShading", material.FlatShading },
                { "sizeAttenuation", material.SizeAttenuation },
                { "logarithmicDepthBuffer", logarithmicDepthBuffer },
                { "skinning", material.Skinning && maxBones > 0 },
                { "maxBones", maxBones },
                { "useVertexTexture", floatVertexTextures },
                { "morphTargets", material.MorphTargets },
                { "morphNormals", material.MorphNormals },
                { "maxMorphTargets", renderer.MaxMorphTargets },
                { "maxMorphNormals", renderer.MaxMorphNormals },
                { "numDirLights", lights.state["directional"] is GLUniform[] directional ? directional.Length : 0 },
                { "numPointLights", lights.state["point"] is GLUniform[] point ? point.Length : 0 },
                { "numSpotLights", lights.state["spot"] is GLUniform[] spot ? spot.Length : 0 },
                { "numRectAreaLights", lights.state["rectArea"] is GLUniform[] rectArea ? rectArea.Length : 0 },
                { "numHemiLights", lights.state["hemi"] is GLUniform[] hemi ? hemi.Length : 0 },
                { "numDirLightShadows", lights.state["directionalShadowMap"] is Texture[] directionalShadowMap ? directionalShadowMap.Length : 0 },
                { "numPointLightShadows", lights.state["pointShadowMap"] is Texture[] pointShadowMap ? pointShadowMap.Length : 0 },
                { "numSpotLightShadows", lights.state["spotShadowMap"] is Texture[] spotShadowMap ? spotShadowMap.Length : 0 },
                { "numClippingPlanes", clipping.numPlanes },
                { "numClipIntersection", clipping.numIntersection },
                { "dithering", material.Dithering },
                { "shadowMapEnabled", renderer.ShadowMap.Enabled && shadows.Count > 0 },
                { "shadowMapType", renderer.ShadowMap.type },
                { "toneMapping", material.ToneMapped ? renderer.ToneMapping : Constants.NoToneMapping },
                { "physicallyCorrectLights", renderer.PhysicallyCorrectLights },
                { "premultipliedAlpha", material.PremultipliedAlpha },
                { "alphaTest", material.AlphaTest },
                { "doubleSided", material.Side == Constants.DoubleSide },
                { "flipSided", material.Side == Constants.BackSide },
                { "depthPacking", material is MeshDepthMaterial meshDepthMaterial ? meshDepthMaterial.DepthPacking : 0 },
                { "indexOfAttributeName", material.IndexOAttributeName },
                { "extensionDerivatives", material is ShaderMaterial shaderMaterial && shaderMaterial.extensions.derivatives },
                { "extensionFragDepth", material is ShaderMaterial shaderFragMaterial && shaderFragMaterial.extensions.fragDepth },
                { "extensionDrawBuffers", material is ShaderMaterial shaderDrawMaterial && shaderDrawMaterial.extensions.drawBuffers },
                { "extensionShaderTextureLOD", material is ShaderMaterial shaderLODMaterial && shaderLODMaterial.extensions.shaderTextureLOD },
                { "rendererExtensionFragDepth", isGL2 || extensions.Get("EXT_frag_depth") > -1 },
                { "rendererExtensionDrawBuffers", isGL2 || extensions.Get("GL_draw_buffers") > -1 },
                { "rendererExtensionShaderTextureLOD", isGL2 || extensions.Get("EXT_shader_texture_lod") > -1 },
                { "customProgramCacheKey", material.customProgramCacheKey }
            };

            return parameters;
        }
        public string getProgramCacheKey(Hashtable parameters)
        {
            var array = new List<string>();

            if (!string.IsNullOrEmpty((string)parameters["shaderId"]))
            {
                array.Add((string)parameters["shaderId"]);
            }
            else
            {
                array.Add((string)parameters["fragmentShader"]);
                array.Add((string)parameters["vertexShader"]);
            }

            if (parameters.ContainsKey("defines"))
            {
                foreach (DictionaryEntry entry in parameters["defines"] as Hashtable)
                {
                    array.Add((string)entry.Key);
                    if (entry.Value is string)
                        array.Add((string)entry.Value);
                    else
                        array.Add(Convert.ToString(entry.Value));
                }

            }

            if ((bool)parameters["isRawShaderMaterial"] == false)
            {
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    object obj = parameters[parameterNames[i]];
                    string parameterKey = obj != null ? obj.ToString() : "";
                    array.Add(parameterKey);
                }
            }

            //array.Add(material.OnBeforeCompile.ToString());
            array.Add(renderer.outputEncoding.ToString());
            array.Add(renderer.GammaFactor.ToString());


            return string.Join(",", array);
        }
        public GLUniforms GetUniforms(Material material)
        {
            string shaderId = "";

            if (shaderIDs.ContainsKey(material.type))
                shaderId = shaderIDs[material.type];



            GLUniforms uniforms;

            if (!string.IsNullOrEmpty(shaderId))
            {
                var  shader = (GLShader)ShaderLib[shaderId];
                uniforms = (GLUniforms)shader.Uniforms.Clone();
            }
            else
            {
                uniforms = (material as ShaderMaterial).Uniforms;
            }

            return uniforms;
        }
        public GLProgram AcquireProgram(Hashtable parameters, string cacheKey)
        {
            GLProgram program = null;
            for (var p = 0; p < Programs.Count; p++)
            {
                var programInfo = Programs[p];

                if (programInfo.Code.Equals(cacheKey))
                {
                    program = programInfo;
                    ++program.UsedTimes;
                    break;
                }
            }

            if (program == null)
            {
                program = new GLProgram(renderer, cacheKey, parameters, bindingStates);
                Programs.Add(program);
            }

            return program;
        }




        public void ReleaseProgram(GLProgram program)
        {
            if (--program.UsedTimes == 0)
            {
                var i = Programs.IndexOf(program);
                if (i >= 0)
                    Programs.RemoveAt(i);
            }
        }




    }
}
