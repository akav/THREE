﻿using THREE;
using THREE.Silk;
using Color = THREE.Color;
namespace THREE.Silk.Example
{
    [Example("07.Mesh-Phong-Material", ExampleCategory.LearnThreeJS, "Chapter04")]
    public class MeshPhongMaterialExample : MeshLambertMaterialExample
    {
        public MeshPhongMaterialExample():base()
        {

        }

        public override void BuildMeshMaterial()
        {
            meshMaterial = new MeshPhongMaterial();
            meshMaterial.Color = Color.Hex(0x7777ff);
            meshMaterial.Name = "MeshPhongMaterial";

        }
        public override void AddAmbientLight()
        {
            //base.AddAmbientLight();
        }
        public override void AddSpotLight()
        {
            spotLight = new SpotLight(Color.Hex(0xffffff));
            spotLight.Position.Set(0, 60, 60);
            spotLight.Intensity = 0.6f;
            spotLight.CastShadow = true;
            scene.Add(spotLight);
        }

    }
}
