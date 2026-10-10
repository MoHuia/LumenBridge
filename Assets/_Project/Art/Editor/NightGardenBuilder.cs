using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project.Art.Editor
{
    /// <summary>
    /// 仅在编辑器中生成原型美术资源和独立场景，不参与游戏模拟。
    /// 材质、网格、预制体均保存为工程资源，运行时无需执行生成器。
    /// </summary>
    public static class NightGardenBuilder
    {
        private const string k_artRoot = "Assets/_Project/Art";
        private const string k_scenePath = "Assets/_Project/Scenes/Prototype_NightGarden.unity";

        /// <summary>从实验场景复制一份原型，保持用户脚本与显式引用。</summary>
        [MenuItem("LumenBridge/Art/Build Night Garden")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先退出 Play。");
            if (File.Exists(k_scenePath)) throw new System.InvalidOperationException("原型已存在，避免覆盖手工修改。");
            var active = EditorSceneManager.GetActiveScene();
            if (active.path != "Assets/_Project/Scenes/Lab_OneFirefly.unity")
                throw new System.InvalidOperationException("请先打开 Lab_OneFirefly。");
            // saveAsCopy 不改变或覆盖原场景，未保存的实验改动也会进入副本。
            EditorSceneManager.SaveScene(active, k_scenePath, true);
            var scene = EditorSceneManager.OpenScene(k_scenePath);
            Directory.CreateDirectory(k_artRoot + "/Models");
            Directory.CreateDirectory(k_artRoot + "/Materials");
            AssetDatabase.Refresh();

            var ground = Material("NightGround", new Color(0.06f, 0.10f, 0.15f), new Color(0.025f,0.045f,0.075f));
            var moss = Material("GardenMoss", new Color(0.20f, 0.42f, 0.37f), new Color(0.045f,0.12f,0.095f));
            var rim = Material("GardenRim", new Color(0.12f, 0.23f, 0.30f), new Color(0.02f,0.04f,0.065f));
            var stone = Material("RiverStone", new Color(0.30f, 0.40f, 0.48f), new Color(0.035f,0.055f,0.075f));
            var grass = Material("MoonGrass", new Color(0.24f, 0.53f, 0.40f), new Color(0.03f,0.08f,0.04f));
            var body = Material("FireflyBody", new Color(0.09f, 0.18f, 0.18f));
            var wings = Material("FireflyWings", new Color(0.55f, 0.72f, 0.60f));
            var glow = Material("FireflyGlow", new Color(1f, 0.78f, 0.29f), new Color(3.5f, 1.8f, 0.35f));
            var halo = new UnityEngine.Material(Shader.Find("LumenBridge/Firefly Halo"));
            halo.SetColor("_Color", new Color(1f, 0.62f, 0.12f, 0.55f));
            AssetDatabase.CreateAsset(halo, k_artRoot + "/Materials/FireflyHalo.mat");

            var environment = new GameObject("Environment_Art");
            Shape("Night Ground", PrimitiveType.Cube, environment.transform, new Vector3(0,-0.65f,0), new Vector3(80,0.2f,80), ground);
            // 两层低面数平台形成明确的活动区域，装饰集中在边缘。
            var disc = DiscMesh(48);
            AssetDatabase.CreateAsset(disc, k_artRoot + "/Models/GardenDisc.asset");
            var rockMesh = RockMesh();
            AssetDatabase.CreateAsset(rockMesh, k_artRoot + "/Models/FacetedStone.asset");
            MeshObject("Stone Foundation", disc, environment.transform, new Vector3(0,-0.42f,0), new Vector3(10.2f,0.28f,6.3f), rim);
            MeshObject("Moss Clearing", disc, environment.transform, new Vector3(0,-0.20f,0), new Vector3(9.9f,0.11f,6f), moss);
            // 固定随机种子确保重新制作资源时布局可复现，不影响玩法随机数。
            var random = new System.Random(23);
            for (int i = 0; i < 26; i++)
            {
                float angle = i * Mathf.PI * 2f / 26f;
                var p = new Vector3(Mathf.Cos(angle)*9.45f,-0.07f,Mathf.Sin(angle)*5.55f);
                float size = 0.25f + (float)random.NextDouble()*0.45f;
                var rock = Shape("Bank Stone " + i, PrimitiveType.Cube, environment.transform, p, new Vector3(size*1.4f,size,size), stone);
                rock.GetComponent<MeshFilter>().sharedMesh = rockMesh;
                rock.transform.rotation = Quaternion.Euler(12f,i*37f,18f);
                for (int j=0;j<3;j++)
                {
                    var blade = Shape("Reed " + i + "-" + j, PrimitiveType.Cube, environment.transform,
                        p + new Vector3(0.35f+j*0.09f,0.18f,0.1f), new Vector3(0.055f,0.38f+j*0.13f,0.055f), grass);
                    blade.transform.rotation = Quaternion.Euler(12f,j*32f,-12f+j*10f);
                }
            }
            // 四盏边缘灯只用自发光网格，不给每个装饰增加实时点光。
            foreach (float x in new[]{-7.5f,7.5f}) foreach(float z in new[]{-3.4f,3.4f})
            {
                Shape("Garden Lantern Base", PrimitiveType.Cylinder, environment.transform, new Vector3(x,0.05f,z), new Vector3(0.35f,0.25f,0.35f), stone);
                Shape("Garden Lantern Ember", PrimitiveType.Sphere, environment.transform, new Vector3(x,0.37f,z), Vector3.one*0.18f, glow);
                Shape("Lantern Ground Glow", PrimitiveType.Quad, environment.transform, new Vector3(x,-0.075f,z), Vector3.one*2f, halo).transform.rotation = Quaternion.Euler(90,0,0);
            }

            var view = GameObject.Find("FireflyView");
            // 保留根对象与 FireflyView 脚本，使 Bootstrap 引用继续有效。
            Object.DestroyImmediate(view.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(view.GetComponent<MeshFilter>());
            Object.DestroyImmediate(view.GetComponent<Collider>());
            view.transform.localScale = Vector3.one;
            Shape("Body", PrimitiveType.Sphere, view.transform, new Vector3(0,0.24f,0), new Vector3(0.18f,0.16f,0.30f), body);
            Shape("Warm Abdomen", PrimitiveType.Sphere, view.transform, new Vector3(0,0.25f,-0.13f), new Vector3(0.19f,0.17f,0.21f), glow);
            Shape("Head", PrimitiveType.Sphere, view.transform, new Vector3(0,0.26f,0.17f), Vector3.one*0.12f, body);
            Shape("Left Wing", PrimitiveType.Sphere, view.transform, new Vector3(-0.14f,0.31f,0.02f), new Vector3(0.24f,0.035f,0.13f), wings).transform.localRotation=Quaternion.Euler(0,-25,0);
            Shape("Right Wing", PrimitiveType.Sphere, view.transform, new Vector3(0.14f,0.31f,0.02f), new Vector3(0.24f,0.035f,0.13f), wings).transform.localRotation=Quaternion.Euler(0,25,0);
            Shape("Soft Halo", PrimitiveType.Quad, view.transform, new Vector3(0,0.22f,0), Vector3.one*1.25f, halo).transform.localRotation=Quaternion.Euler(50,0,0);
            Shape("Ground Glow", PrimitiveType.Quad, view.transform, new Vector3(0,-0.07f,0), Vector3.one*1.5f, halo).transform.localRotation=Quaternion.Euler(90,0,0);
            PrefabUtility.SaveAsPrefabAsset(view, "Assets/_Project/Prefabs/FireflyView.prefab");

            var camera = Camera.main;
            camera.transform.position = new Vector3(0,14,-12);
            camera.transform.LookAt(new Vector3(0,0,0));
            camera.orthographic = true;
            camera.orthographicSize = 7.3f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f,0.035f,0.065f);
            camera.allowHDR = true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var light = GameObject.Find("Directional Light").GetComponent<Light>();
            light.color = new Color(0.56f,0.74f,1f);
            light.intensity = 2.2f;
            light.transform.rotation = Quaternion.Euler(50,-35,0);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f,0.25f,0.33f);
            RenderSettings.fog = false;
            // 独立 VolumeProfile，避免修改模板场景共享的后处理资源。
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile,k_artRoot + "/Materials/NightGardenVolume.asset");
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1f);
            bloom.intensity.Override(0.65f);
            bloom.scatter.Override(0.65f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.20f);
            vignette.smoothness.Override(0.55f);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            GameObject.Find("Global Volume").GetComponent<Volume>().sharedProfile = profile;
            AssetDatabase.AddObjectToAsset(bloom,profile);
            AssetDatabase.AddObjectToAsset(vignette,profile);
            AssetDatabase.AddObjectToAsset(tone,profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene);
            SceneView.lastActiveSceneView?.LookAt(Vector3.zero, camera.transform.rotation, 13f);
            Debug.Log("夜景原型美术已生成；原实验场景保留，玩法仍使用原有单只光虫代码。");
        }

        /// <summary>创建共享 URP 材质，发光只用于显示，不参与光源玩法。</summary>
        private static UnityEngine.Material Material(string name, Color color, Color emission = default)
        {
            var material = new UnityEngine.Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor",color);
            material.SetFloat("_Smoothness",0.18f);
            if (emission.maxColorComponent>0)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor",emission);
            }
            AssetDatabase.CreateAsset(material,k_artRoot + "/Materials/" + name + ".mat");
            return material;
        }

        /// <summary>装饰网格不保留碰撞器；根对象不混入模拟职责。</summary>
        private static GameObject Shape(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, UnityEngine.Material material)
        {
            var go=GameObject.CreatePrimitive(type);
            go.name=name;
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>带侧面的圆形平台，缩放后形成椭圆；保存网格供复用。</summary>
        private static Mesh DiscMesh(int segments)
        {
            var vertices=new Vector3[segments*2+2];
            vertices[0]=Vector3.zero;
            vertices[1]=Vector3.down;
            var triangles=new int[segments*12];
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments;
                vertices[2+i*2]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                vertices[3+i*2]=vertices[2+i*2]+Vector3.down;
            }
            for(int i=0;i<segments;i++)
            {
                int a=2+i*2,b=2+((i+1)%segments)*2,t=i*12;
                int[] face={0,b,a,1,a+1,b+1,a,b,b+1,a,b+1,a+1};
                for(int j=0;j<12;j++) triangles[t+j]=face[j];
            }
            var mesh=new Mesh {name="GardenDisc",vertices=vertices,triangles=triangles};
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>不规则八面体使用独立面顶点，形成清晰的低模石头切面。</summary>
        private static Mesh RockMesh()
        {
            var points = new[]{new Vector3(0.1f,0.6f,0),new Vector3(0,-0.4f,0),
                new Vector3(-0.6f,0,0),new Vector3(0,0,0.55f),
                new Vector3(0.55f,0.08f,0),new Vector3(0,0,-0.45f)};
            int[] faces={0,2,3,0,3,4,0,4,5,0,5,2,1,3,2,1,4,3,1,5,4,1,2,5};
            var vertices=new Vector3[faces.Length];
            var triangles=new int[faces.Length];
            for(int i=0;i<faces.Length;i++){vertices[i]=points[faces[i]];triangles[i]=i;}
            var mesh=new Mesh{name="FacetedStone",vertices=vertices,triangles=triangles};
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>将已保存的网格和共享材质接入场景。</summary>
        private static void MeshObject(string name, Mesh mesh, Transform parent, Vector3 position, Vector3 scale, UnityEngine.Material material)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}
