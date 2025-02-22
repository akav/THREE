using ImGuiNET;
using OpenTK;
using OpenTK.Windowing.Common;
using System;
using THREE;
using THREEExample.Learning.Utils;
using THREEExample.ThreeImGui;
using Color = THREE.Color;
namespace THREEExample.Learning.Chapter10
{
    [Example("10-displacement-map", ExampleCategory.LearnThreeJS, "Chapter10")]
    public class DisplacementMapExample : TemplateExample
    {
        private const float GroundPlaneYPosition = -10f;
        private const float SphereRadius = 8f;
        private const int SphereWidthSegments = 180;
        private const int SphereHeightSegments = 180;
        private const int AmbientLightColor = 0x444444;
        private const float SphereMetalness = 0.02f;
        private const float SphereRoughness = 0.07f;
        private const int SphereColor = 0xffffff;
        private const float RotationSpeed = 0.01f;
        private const float DisplacementScaleMin = -5.0f;
        private const float DisplacementScaleMax = 5.0f;
        private const float DisplacementBiasMin = -5.0f;
        private const float DisplacementBiasMax = 5.0f;

        private MeshStandardMaterial sphereMaterial;
        private Mesh sphereMesh;

        public DisplacementMapExample() : base()
        {
        }

        public override void SetGeometryWithTexture()
        {
            var groundPlane = DemoUtils.AddLargeGroundPlane(scene);
            groundPlane.Position.Y = GroundPlaneYPosition;
            groundPlane.ReceiveShadow = true;

            scene.Add(new AmbientLight(new Color(AmbientLightColor)));

            var sphere = new SphereBufferGeometry(SphereRadius, SphereWidthSegments, SphereHeightSegments);
            sphereMaterial = new MeshStandardMaterial()
            {
                Map = LoadTexture("../../../../assets/textures/w_c.jpg"),
                DisplacementMap = LoadTexture("../../../../assets/textures/w_d.png"),
                Metalness = SphereMetalness,
                Roughness = SphereRoughness,
                Color = new THREE.Color(SphereColor)
            };

            sphereMesh = new Mesh(sphere, sphereMaterial);
            scene.Add(sphereMesh);
        }

        private Texture LoadTexture(string path)
        {
            var texture = TextureLoader.Load(path);
            if (texture == null)
            {
                throw new Exception($"Failed to load texture: {path}");
            }
            return texture;
        }

        public override void Init()
        {
            base.Init();

            AddGuiControlsAction = () =>
            {
                ImGui.SliderFloat("displacementScale", ref sphereMaterial.DisplacementScale, DisplacementScaleMin, DisplacementScaleMax);
                ImGui.SliderFloat("displacementBias", ref sphereMaterial.DisplacementBias, DisplacementBiasMin, DisplacementBiasMax);
            };
        }

        public override void Render()
        {
            if (!imGuiManager.ImWantMouse)
                controls.Enabled = true;
            else
                controls.Enabled = false;

            controls.Update();
            this.renderer.Render(scene, camera);

            // Ensure ImGui frame is started and ended correctly
            ImGui.NewFrame();
            ShowGUIControls();
            ImGui.Render();
            imGuiManager.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());

            sphereMesh.Rotation.Y += RotationSpeed;
        }
    }
}
