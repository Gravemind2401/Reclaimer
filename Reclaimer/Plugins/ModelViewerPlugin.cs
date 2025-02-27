using Reclaimer.Annotations;
using Reclaimer.Controls.Editors;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Compatibility;
using Reclaimer.Utilities;
using Reclaimer.Windows;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Reclaimer.Plugins
{
    public class ModelViewerPlugin : Plugin
    {
        private delegate bool GetDataFolder(out string dataFolder);
        private GetDataFolder getDataFolderFunc;

        private delegate bool SaveImage(IContentProvider<IBitmap> provider, string baseDir);
        private SaveImage saveImageFunc;

        internal override int? FilePriority => 1;

        public override string Name => "Model Viewer";

        internal static ModelViewerSettings Settings;

        private PluginContextItem ExtractBitmapsContextItem => new PluginContextItem("ExtractBitmaps", "Extract Bitmaps", OnContextItemClick);

        public override void Initialise() => Settings = LoadSettings<ModelViewerSettings>();

        public override void PostInitialise()
        {
            getDataFolderFunc = Substrate.GetSharedFunction<GetDataFolder>(Constants.SharedFuncGetDataFolder);
            saveImageFunc = Substrate.GetSharedFunction<SaveImage>(Constants.SharedFuncBatchSaveImage);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            if (context.File.OfType<IContentProvider<Scene>>().Any())
                yield return ExtractBitmapsContextItem;
        }

        public override void Suspend() => SaveSettings(Settings);

        public override bool CanOpenFile(OpenFileArgs args) => args.File.Any(i => i is IContentProvider<Scene>);

        public override void OpenFile(OpenFileArgs args)
        {
            var model = args.File.OfType<IContentProvider<Scene>>().FirstOrDefault();
            DisplayModel(args.TargetWindow, model, args.FileName);
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            var provider = context.File.OfType<IContentProvider<Scene>>().FirstOrDefault();
            ExportBitmaps(provider, false, true);
        }

        [SharedFunction]
        public void ExportBitmaps(IContentProvider<Scene> provider, bool filtered, bool async)
        {
            if (!getDataFolderFunc(out var folder))
                return;

            //TODO: offload this to batch extractor so it can be cancelled
            if (async)
                Task.Run(Execute);
            else
                Execute();

            void Execute()
            {
                var scene = provider.GetContent();
                var textures = filtered ? scene.EnumerateExportedTextures() : scene.EnumerateTextures();

                foreach (var tex in textures)
                {
                    var bitm = tex.ContentProvider;
                    try
                    {
                        SetWorkingStatus($"Extracting {bitm.Name}");
                        saveImageFunc(bitm, folder);
                        LogOutput($"Extracted {bitm.Name}.{bitm.Class}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error extracting {bitm.Name}.{bitm.Class}", ex, true);
                    }
                }

                ClearWorkingStatus();
                LogOutput($"Recursive bitmap extract complete for {provider.Name}.{provider.Class}");
            };
        }

        [SharedFunction]
        public void DisplayModel(ITabContentHost targetWindow, IContentProvider<Scene> model, string fileName)
        {
            var tabId = $"{Key}::{model.SourceFile}::{model.Id}";
            if (Substrate.ShowTabById(tabId))
                return;

            var container = targetWindow.DocumentPanel;

            LogOutput($"Loading model: {fileName}");

            try
            {
                var viewer = new Controls.DirectX.ModelViewer
                {
                    LogOutput = LogOutput,
                    LogError = LogError,
                    SetStatus = SetWorkingStatus,
                    ClearStatus = ClearWorkingStatus
                };

                viewer.TabModel.ContentId = tabId;
                viewer.LoadGeometry(model);

                container.AddItem(viewer.TabModel);

                LogOutput($"Loaded model: {fileName}");
            }
            catch (Exception e)
            {
                LogError($"Error loading model: {fileName}", e, true);
            }
        }

        #region Model Exports

        private static readonly ExportFormat[] StandardFormats =
        [
            new ExportFormat(FormatId.RMF,              "rmf",  "RMF Files", (model, fileName) => model.GetContent().WriteRMF(fileName)),
            new ExportFormat(FormatId.AMF,              "amf",  "AMF Files", (model, fileName) => model.GetContent().WriteAMF(fileName, 100f)),
            new ExportFormat(FormatId.JMS,              "jms",  "JMS Files", (model, fileName) => model.GetContent().WriteJMS(fileName, 100f)),
            new ExportFormat(FormatId.OBJNoMaterials,   "obj",  "OBJ Files"),
            new ExportFormat(FormatId.OBJ,              "obj",  "OBJ Files with materials"),
            new ExportFormat(FormatId.Collada,          "dae",  "COLLADA Files"),
        ];

        private static readonly Dictionary<string, ExportFormat> UserFormats = new Dictionary<string, ExportFormat>();

        private static IEnumerable<ExportFormat> ExportFormats => UserFormats.Values.Concat(StandardFormats);

        [SharedFunction]
        public static IEnumerable<string> GetExportFormats() => ExportFormats.Select(f => f.FormatId);

        [SharedFunction]
        public static string GetFormatExtension(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase)).Extension;

        [SharedFunction]
        public static string GetFormatDescription(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase)).Description;

        [SharedFunction]
        public static void RegisterExportFormat(string formatId, string extension, string description, Action<IContentProvider<Scene>, string> exportFunction)
        {
            Exceptions.ThrowIfNullOrWhiteSpace(formatId);
            Exceptions.ThrowIfNullOrWhiteSpace(extension);
            ArgumentNullException.ThrowIfNull(exportFunction);

            formatId = formatId.ToLower();
            extension = extension.ToLower();

            if (UserFormats.ContainsKey(formatId))
                throw new ArgumentException("A format with the same ID has already been added.", nameof(formatId));

            UserFormats.Add(formatId, new ExportFormat(formatId, extension, description, exportFunction));
        }

        [SharedFunction]
        public static void WriteModelFile(IContentProvider<Scene> provider, string fileName, string formatId)
        {
            ArgumentNullException.ThrowIfNull(provider);
            Exceptions.ThrowIfNullOrWhiteSpace(fileName);

            formatId = (formatId ?? Settings.DefaultSaveFormat).ToLower();
            if (!ExportFormats.Any(f => f.FormatId == formatId))
                throw new ArgumentException($"{formatId} is not a supported format.", nameof(formatId));

            var ext = "." + GetFormatExtension(formatId);
            if (!fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                fileName += ext;

            var format = ExportFormats.First(f => f.FormatId == formatId);

            if (format.ExportFunction != null)
                format.ExportFunction(provider, fileName);
            else
            {
                using (var context = new Assimp.AssimpContext())
                {
                    var scene = provider.GetContent().CreateAssimpScene(context, formatId);
                    context.ExportFile(scene, fileName, formatId);
                }
            }
        }

        private readonly struct ExportFormat
        {
            public string FormatId { get; }
            public string Extension { get; }
            public string Description { get; }
            public Action<IContentProvider<Scene>, string> ExportFunction { get; }

            public ExportFormat(string formatId, string extension, string description, Action<IContentProvider<Scene>, string> exportFunction = null)
            {
                FormatId = formatId;
                Extension = extension;
                Description = description;
                ExportFunction = exportFunction;
            }
        }

        #endregion
    }

    internal static class FormatId
    {
        public const string RMF = "rmf";
        public const string AMF = "amf";
        public const string JMS = "jms";
        public const string OBJNoMaterials = "objnomtl";
        public const string OBJ = "obj";
        public const string Collada = "collada";
    }

    internal sealed class ModelViewerSettings
    {
        [ItemsSource(typeof(ModelFormatItemsSource))]
        [DisplayName("Default Save Format")]
        [DefaultValue(FormatId.RMF)]
        public string DefaultSaveFormat { get; set; }

        [DisplayName("Embedded Material Extension")]
        [DefaultValue("tif")]
        public string MaterialExtension { get; set; }

        [DisplayName("Assimp Scale")]
        [DefaultValue(0.03048f)]
        public float AssimpScale { get; set; }
    }

    public static class ModelViewerExtensions
    {
        public static Assimp.Vector2D ToAssimp2D(this System.Numerics.Vector2 v) => new Assimp.Vector2D(v.X, v.Y);
        public static Assimp.Vector3D ToAssimp3D(this System.Numerics.Vector3 v) => new Assimp.Vector3D(v.X, v.Y, v.Z);
        public static Assimp.Vector3D ToAssimp3D(this System.Numerics.Vector3 v, float scale) => new Assimp.Vector3D(v.X * scale, v.Y * scale, v.Z * scale);
        public static Assimp.Vector3D ToAssimpUV(this System.Numerics.Vector2 v) => new Assimp.Vector3D(v.X, 1f - v.Y, 0f);
        public static Assimp.Matrix4x4 ToAssimp4x4(this System.Numerics.Matrix4x4 m) => m.ToAssimp4x4(1f);

        public static Assimp.Matrix4x4 ToAssimp4x4(this System.Numerics.Matrix4x4 m, float offsetScale)
        {
            return new Assimp.Matrix4x4
            {
                A1 = m.M11,
                A2 = m.M21,
                A3 = m.M31,
                A4 = m.M41 * offsetScale,
                B1 = m.M12,
                B2 = m.M22,
                B3 = m.M32,
                B4 = m.M42 * offsetScale,
                C1 = m.M13,
                C2 = m.M23,
                C3 = m.M33,
                C4 = m.M43 * offsetScale,
                D1 = m.M14,
                D2 = m.M24,
                D3 = m.M34,
                D4 = m.M44,
            };
        }

        public static System.Numerics.Matrix4x4 ToNumeric4x4(this Assimp.Matrix4x4 m)
        {
            return new System.Numerics.Matrix4x4
            {
                M11 = m.A1,
                M12 = m.B1,
                M13 = m.C1,
                M14 = m.D1,
                M21 = m.A2,
                M22 = m.B2,
                M23 = m.C2,
                M24 = m.D2,
                M31 = m.A3,
                M32 = m.B3,
                M33 = m.C3,
                M34 = m.D3,
                M41 = m.A4,
                M42 = m.B4,
                M43 = m.C4,
                M44 = m.D4
            };
        }

        public static Assimp.Scene CreateAssimpScene(this Scene scene, Assimp.AssimpContext context, string formatId)
        {
            const float scale = 100f;

            //either Assimp or collada has issues when there is a name conflict
            const string bonePrefix = "~";
            const string geomPrefix = "-";
            const string scenPrefix = "$";

            var assimpScene = new Assimp.Scene();
            assimpScene.RootNode = new Assimp.Node($"{scenPrefix}{scene.Name}");

            //Assimp is Y-up in inches by default - this forces it to export as Z-up in meters
            assimpScene.RootNode.Transform = (CoordinateSystem.HaloCEX.WorldMatrix * ModelViewerPlugin.Settings.AssimpScale).ToAssimp4x4();

            //material ID -> assimp mat index
            var materialLookup = new Dictionary<int, int>();

            #region Materials

            //bitmap ID -> bitmap meta
            var bitmapLookup = scene.EnumerateExportedTextures()
                .Select(t => t.ContentProvider.GetContent())
                .ToDictionary(t => t.Id);

            foreach (var mat in scene.EnumerateExportedMaterials())
            {
                var assimpMaterial = new Assimp.Material { Name = mat?.Name ?? "unused" };

                //prevent max from making every material super shiny
                assimpMaterial.ColorEmissive = assimpMaterial.ColorReflective = assimpMaterial.ColorSpecular = new Assimp.Color4D(0, 0, 0, 1);
                assimpMaterial.ColorDiffuse = assimpMaterial.ColorTransparent = new Assimp.Color4D(1);

                //max only seems to care about diffuse
                var dif = mat?.TextureMappings.FirstOrDefault(m => m.Usage == TextureUsage.Diffuse);
                if (dif != null)
                {
                    var bitmap = bitmapLookup[dif.Texture.Id];
                    var suffix = bitmap.SubmapCount > 1 ? "[0]" : string.Empty;
                    var filePath = $"{bitmap.Name}{suffix}.{ModelViewerPlugin.Settings.MaterialExtension}";

                    //collada spec says it requires URI formatting, and Assimp doesn't do it for us
                    //for some reason "new Uri(filePath, UriKind.Relative)" doesnt change the slashes, have to use absolute uri
                    if (formatId == FormatId.Collada)
                        filePath = new Uri("X:\\", UriKind.Absolute).MakeRelativeUri(new Uri(System.IO.Path.Combine("X:\\", filePath))).ToString();

                    assimpMaterial.TextureDiffuse = new Assimp.TextureSlot
                    {
                        BlendFactor = 1,
                        FilePath = filePath,
                        TextureType = Assimp.TextureType.Diffuse
                    };
                }

                materialLookup.Add(mat.Id, assimpScene.MaterialCount);
                assimpScene.Materials.Add(assimpMaterial);
            }

            #endregion

            //(model, mesh index) -> assimp mesh index
            var meshLookup = new Dictionary<(Model, int), int>();

            #region Meshes

            foreach (var model in scene.EnumerateExportedModels())
            {
                var exportedMeshes = model.Regions
                    .Where(r => r.Export)
                    .SelectMany(r => r.Permutations.Where(p => p.Export))
                    .SelectMany(p => p.MeshIndices)
                    .Distinct()
                    .Order()
                    .Select(i => (model.Meshes[i], i));

                foreach (var (mesh, meshIndex) in exportedMeshes)
                {
                    var key = (model, meshIndex);

                    if (mesh == null || mesh.Segments.Count == 0)
                    {
                        meshLookup.Add(key, -1);
                        continue;
                    }

                    meshLookup.Add(key, assimpScene.MeshCount);

                    foreach (var sub in mesh.Segments)
                    {
                        var assimpMesh = new Assimp.Mesh($"mesh{assimpScene.MeshCount:D3}");
                        var indices = mesh.GetTriangleIndicies(sub);

                        var minIndex = indices.Min();
                        var maxIndex = indices.Max();
                        var vertCount = maxIndex - minIndex + 1;

                        indices = indices.Select(x => x - minIndex);

                        var posTransform = mesh.PositionBounds.CreateExpansionMatrix();
                        var texTransform = mesh.TextureBounds.CreateExpansionMatrix();

                        var positions = mesh.GetPositions(minIndex, vertCount)?.Select(v => System.Numerics.Vector3.Transform(v, posTransform)).ToList();
                        var texcoords = mesh.GetTexCoords(minIndex, vertCount)?.Select(v => System.Numerics.Vector2.Transform(v, texTransform)).ToList();
                        var normals = mesh.GetNormals(minIndex, vertCount)?.ToList();
                        var blendIndices = mesh.GetBlendIndices(minIndex, vertCount)?.ToList();
                        var blendWeights = mesh.GetBlendWeights(minIndex, vertCount)?.ToList();
                        var colors = mesh.GetColors(minIndex, vertCount)?.ToList();

                        if (positions != null)
                            assimpMesh.Vertices.AddRange(positions.Select(v => (v * scale).ToAssimp3D()));

                        if (normals != null)
                            assimpMesh.Normals.AddRange(normals.Select(v => v.ToAssimp3D()));

                        if (texcoords != null)
                            assimpMesh.TextureCoordinateChannels[0].AddRange(texcoords.Select(v => v.ToAssimpUV()));

                        //assimp appears to have issues exporting obj when a colour channel exists so only do this for collada
                        if (formatId == FormatId.Collada)
                        {
                            if (colors != null)
                                assimpMesh.VertexColorChannels[0].AddRange(colors.Select(v => new Assimp.Color4D(v.X, v.Y, v.Z, v.W)));
                            else if (positions != null && mesh.Flags.HasFlag(MeshFlags.VertexColorFromPosition))
                                assimpMesh.VertexColorChannels[0].AddRange(mesh.VertexBuffer.PositionChannels[0].GetSubset(minIndex, vertCount).Select(v => new Assimp.Color4D { R = v.W }));
                        }

                        var boneLookup = new Dictionary<int, Assimp.Bone>();
                        for (var vIndex = 0; vIndex < vertCount; vIndex++)
                        {
                            if (!(mesh.BoneIndex.HasValue || mesh.VertexBuffer.HasBlendIndices || mesh.VertexBuffer.HasBlendWeights))
                                continue;

                            #region Vertex Weights
                            var weights = new HashSet<(int BoneIndex, float Weight)>(4);

                            if (mesh.BoneIndex.HasValue)
                                weights.Add((mesh.BoneIndex.Value, 1));
                            else
                            {
                                var ind = blendIndices[vIndex];
                                var wt = blendWeights?[vIndex] ?? System.Numerics.Vector4.One;

                                if (wt.X > 0)
                                    weights.Add(((int)ind.X, wt.X));
                                if (wt.Y > 0)
                                    weights.Add(((int)ind.Y, wt.Y));
                                if (wt.Z > 0)
                                    weights.Add(((int)ind.Z, wt.Z));
                                if (wt.W > 0)
                                    weights.Add(((int)ind.W, wt.W));
                            }

                            foreach (var (boneIndex, weight) in weights)
                            {
                                if (!boneLookup.TryGetValue(boneIndex, out var assimpBone))
                                {
                                    var offsetTransform = model.GetBoneWorldTransform(boneIndex).Inverse();

                                    assimpBone = new Assimp.Bone
                                    {
                                        Name = bonePrefix + model.Bones[boneIndex].Name,
                                        OffsetMatrix = offsetTransform.ToAssimp4x4(scale)
                                    };

                                    assimpMesh.Bones.Add(assimpBone);
                                    boneLookup.Add(boneIndex, assimpBone);
                                }

                                assimpBone.VertexWeights.Add(new Assimp.VertexWeight(vIndex, weight));
                            }
                            #endregion
                        }

                        assimpMesh.SetIndices(indices.ToArray(), 3);
                        assimpMesh.MaterialIndex = materialLookup[sub.Material.Id];

                        assimpScene.Meshes.Add(assimpMesh);
                    }
                }
            }

            #endregion

            AppendSceneGroup(assimpScene.RootNode, scene.RootNode);

            return assimpScene;

            void AppendSceneGroup(Assimp.Node assimpParentNode, SceneGroup groupNode)
            {
                foreach (var childNode in groupNode.ChildGroups.Where(g => g.Export))
                {
                    var assimpNode = new Assimp.Node($"{scenPrefix}{childNode.Name}");
                    assimpParentNode.Children.Add(assimpNode);
                    AppendSceneGroup(assimpNode, childNode);
                }

                foreach (var placement in groupNode.ChildObjects.Where(o => o.Export))
                {
                    var assimpNode = new Assimp.Node($"{scenPrefix}{placement.Name}");
                    assimpParentNode.Children.Add(assimpNode);

                    //assimpNode.Transform = placement.Transform.ToAssimp4x4();

                    var model = placement.Object as Model;

                    #region Bones

                    var assimpBones = new List<Assimp.Node>();
                    foreach (var bone in model.Bones)
                    {
                        var result = new Assimp.Node($"{bonePrefix}{bone.Name}");
                        result.Transform = bone.LocalTransform.ToAssimp4x4(scale);
                        assimpBones.Add(result);
                    }

                    for (var i = 0; i < model.Bones.Count; i++)
                    {
                        var bone = model.Bones[i];
                        if (bone.ParentIndex >= 0)
                            assimpBones[bone.ParentIndex].Children.Add(assimpBones[i]);
                        else
                            assimpNode.Children.Add(assimpBones[i]);
                    }

                    #endregion

                    #region Regions

                    foreach (var reg in model.Regions.Where(r => r.Export))
                    {
                        var regNode = new Assimp.Node($"{geomPrefix}{reg.Name}");
                        foreach (var perm in reg.Permutations.Where(p => p.Export))
                        {
                            var meshStart = meshLookup[(model, perm.MeshRange.Index)];
                            if (meshStart < 0)
                                continue;

                            var permNode = new Assimp.Node($"{geomPrefix}{perm.Name}");
                            permNode.Transform = perm.GetFinalTransform().ToAssimp4x4(scale);

                            var meshCount = perm.MeshIndices.Sum(i => model.Meshes[i].Segments.Count);
                            permNode.MeshIndices.AddRange(Enumerable.Range(meshStart, meshCount));

                            regNode.Children.Add(permNode);
                        }

                        if (regNode.ChildCount > 0)
                            assimpNode.Children.Add(regNode);
                    }

                    #endregion
                }
            }
        }
    }
}
