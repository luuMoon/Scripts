using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ArtTools
{
    public class RadarMeshEditor
    {
        public static string resPath = "Assets/ArtRes/Radar/RadarMesh/";
        public static string meshTemplatePath = "Assets/ArtRes/Radar/radar_mesh.FBX";
        public static string atlasName = "radar";
        public static float quadWidth = 2f;
        public static float quadHeight = 2f;
        //AddMesh
        public static string radarName = "RadarMesh";
        public static string radarMeshPath = "Assets/ArtRes/Radar/radarMaterial.mat";

        public static Material radarMat = AssetDatabase.LoadAssetAtPath<Material>(radarMeshPath);
        public static Vector3 radarPos = new Vector3(0, -2, 0);
        public static Vector3 radarEuler = new Vector3(0, 0, 0);

        //已有mesh
        public static List<string> existMeshs = new List<string>();
        //获取所有sprites
        //创建相应数量mesh
        //修改UV并保存
        [MenuItem("美术资源处理/生成雷达meshUV")]
        static void GenerateMeshUV()
        {
            var sprites = Resources.LoadAll<Sprite>(atlasName);
            existMeshs.Clear();
            Debug.Assert(sprites != null && sprites.Length > 0, "Cant Find Radar Atlas!!!");

            var textureRect = new Rect(0, 0, sprites[0].texture.width, sprites[0].texture.height);

            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            else
            {
                //add exist names
                DirectoryInfo dir = new DirectoryInfo(resPath);
                FileInfo[] info = dir.GetFiles("*.*");

                foreach (FileInfo f in info)
                {
                    var meshName = Path.GetFileNameWithoutExtension(f.Name);
                    existMeshs.Add(meshName);
                }
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                //Create Mesh
                //var mesh = createMesh();

                //已包含的跳过
                if (existMeshs.Contains(sprites[i].name)) continue;
                ///CopyMesh
                var mesh = copyMesh();
                Debug.LogError("contains mesh:" + sprites[i].name);
                ////SetUV
                var rect = sprites[i].rect;
                var tempUV = new Vector4(rect.x / textureRect.width,
                        rect.y / textureRect.height, rect.width / textureRect.width, rect.height / textureRect.height);
                setTile(mesh, tempUV.z, tempUV.w);
                setOffset(mesh, tempUV.x, tempUV.y);
                AssetDatabase.CreateAsset(mesh, resPath + sprites[i].name + ".asset");
            }
            AssetDatabase.SaveAssets();
        }

        private static Mesh createMesh()
        {
            var mesh = new Mesh();
            var vertices = new Vector3[4];

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(quadWidth, 0, 0);
            vertices[2] = new Vector3(0, quadHeight, 0);
            vertices[3] = new Vector3(quadWidth, quadHeight, 0);

            mesh.vertices = vertices;

            var tri = new int[6];

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;

            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;

            mesh.triangles = tri;

            var normals = new Vector3[4];

            normals[0] = -Vector3.forward;
            normals[1] = -Vector3.forward;
            normals[2] = -Vector3.forward;
            normals[3] = -Vector3.forward;

            mesh.normals = normals;

            var uv = new Vector2[4];

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            mesh.uv = uv;
            return mesh;
        }

        private static Mesh copyMesh()
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<MeshFilter>(meshTemplatePath).sharedMesh;
            Mesh newmesh = new Mesh();
            newmesh.vertices = mesh.vertices;
            newmesh.triangles = mesh.triangles;
            newmesh.uv = mesh.uv;
            newmesh.normals = mesh.normals;
            newmesh.colors = mesh.colors;
            newmesh.tangents = mesh.tangents;
            return newmesh;
        }

        private static void setTile(Mesh mesh, float x, float y)
        {
            Vector2[] uvs = new Vector2[mesh.vertices.Length];
            for (var i = 0; i < uvs.Length; i++)
            {
                uvs[i].x = mesh.uv[i].x * x;
                uvs[i].y = mesh.uv[i].y * y;
            }
            mesh.uv = uvs;
        }

        private static void setOffset(Mesh mesh, float x, float y)
        {
            Vector2[] uvs = new Vector2[mesh.vertices.Length];
            for (var i = 0; i < uvs.Length; i++)
            {
                uvs[i].x = mesh.uv[i].x + x;
                uvs[i].y = mesh.uv[i].y + y;

                /*(isRotated) {
                    if(!TPVars.isFreeVersion) {
                        Quaternion rot = Quaternion.Euler(0 ,0, -90);
                        uvs[i] = rot * uvs[i];
                    } else {
                        Debug.LogWarning("Texture Rotation is not supported by free version");
                    }

                }*/

            }

            mesh.uv = uvs;
        }

        //添加radarMesh到指定物体
        //获取选中prefab
        //创建物体，添加mesh
        //保存
        [MenuItem("美术资源处理/添加雷达mesh")]
        static void AddRadarMesh()
        {
            var prefabs = Selection.gameObjects;
            foreach (var prefab in prefabs)
            {
                if (null == prefab)
                {
                    Debug.LogError("没有选中角色!!");
                    return;
                }

                GameObject character = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                //Exist Mesh
                GameObject quad;
                var tempRadarMesh = character.transform.Find(radarName);

                if (tempRadarMesh != null)
                {
                    quad = tempRadarMesh.gameObject;
                }
                //Create Mesh
                else
                {
                    quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = radarName;
                    quad.transform.SetParent(character.transform);
                    //偏移中心点
                    quad.transform.localPosition = radarPos;
                    quad.transform.localEulerAngles = radarEuler;
                    quad.transform.localScale = Vector3.one;
                    //Mat
                    if (null != radarMat)
                    {
                        quad.GetComponent<MeshRenderer>().sharedMaterial = radarMat;
                    }
                    //去除Collider
                    var collider = quad.GetComponent<MeshCollider>();
                    Object.DestroyImmediate(collider);
                }

                //RadarController
                var radarCtrl = quad.GetComponent<Game.RadarController>();
                if (radarCtrl == null)
                {
                    radarCtrl = quad.AddComponent<Game.RadarController>();
                }
                //init RadarCtrl
                radarCtrl.InitChangedType();
                var spriteInfo = character.GetComponent<Game.SpriteInfo>();
                if (spriteInfo != null)
                {
                    spriteInfo.radarCtrl = radarCtrl;
                }
                var sceneItemInfo = character.GetComponent<Game.SceneItemInfo>();
                if (sceneItemInfo != null)
                {
                    sceneItemInfo.radarCtrl = radarCtrl;
                }

                quad.layer = Game.Const.LAYER_RADAR;
                PrefabUtility.ReplacePrefab(character, prefab);
                GameObject.DestroyImmediate(character);
            }
        }
    }
}