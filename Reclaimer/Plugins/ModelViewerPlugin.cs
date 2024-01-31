using Adjutant.Geometry;
using Reclaimer.Annotations;
using Reclaimer.Controls.Editors;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
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
            saveImageFunc = Substrate.GetSharedFunction<SaveImage>(Constants.SharedFuncSaveImage);
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

        private static readonly ExportFormat[] StandardFormats = new[]
        {
            new ExportFormat(FormatId.RMF,              "rmf",  "RMF Files", (model, fileName) => model.GetContent().WriteRMF(fileName)),
            new ExportFormat(FormatId.AMF,              "amf",  "AMF Files", (model, fileName) => model.GetContent().WriteAMF(fileName, Settings.GeometryScale)),
            new ExportFormat(FormatId.JMS,              "jms",  "JMS Files", (model, fileName) => model.GetContent().WriteJMS(fileName, Settings.GeometryScale)),
            new ExportFormat(FormatId.OBJNoMaterials,   "obj",  "OBJ Files"),
            new ExportFormat(FormatId.OBJ,              "obj",  "OBJ Files with materials"),
            new ExportFormat(FormatId.Collada,          "dae",  "COLLADA Files"),
        };

        private static readonly Dictionary<string, ExportFormat> UserFormats = new Dictionary<string, ExportFormat>();

        private static IEnumerable<ExportFormat> ExportFormats => UserFormats.Values.Concat(StandardFormats);

        [SharedFunction]
        public static IEnumerable<string> GetExportFormats() => ExportFormats.Select(f => f.FormatId);

        [SharedFunction]
        public static string GetFormatExtension(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId == formatId.ToLower()).Extension;

        [SharedFunction]
        public static string GetFormatDescription(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId == formatId.ToLower()).Description;

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
                //TODO: assimp support
                //using (var context = new Assimp.AssimpContext())
                //{
                //    var scene = model.CreateAssimpScene(context, formatId);
                //    context.ExportFile(scene, fileName, formatId);
                //}
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

        [DisplayName("Geometry Scale")]
        [DefaultValue(100f)]
        public float GeometryScale { get; set; }

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

        public static Assimp.Scene CreateAssimpScene(this IGeometryModel model, Assimp.AssimpContext context, string formatId)
        {
            var scale = ModelViewerPlugin.Settings.GeometryScale;

            //either Assimp or collada has issues when there is a name conflict
            const string bonePrefix = "~";
            const string geomPrefix = "-";
            const string scenPrefix = "$";

            var scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node($"{scenPrefix}{model.Name}");

            //Assimp is Y-up in inches by default - this forces it to export as Z-up in meters
            scene.RootNode.Transform = (CoordinateSystem.HaloCEX.WorldMatrix * ModelViewerPlugin.Settings.AssimpScale).ToAssimp4x4();

            #region Nodes
            var allNodes = new List<Assimp.Node>();
            foreach (var node in model.Nodes)
            {
                var result = new Assimp.Node($"{bonePrefix}{node.Name}");

                var q = new System.Numerics.Quaternion(node.Rotation.X, node.Rotation.Y, node.Rotation.Z, node.Rotation.W);
                var mat = System.Numerics.Matrix4x4.CreateFromQuaternion(q);
                mat.Translation = new System.Numerics.Vector3(node.Position.X * scale, node.Position.Y * scale, node.Position.Z * scale);
                result.Transform = mat.ToAssimp4x4();

                allNodes.Add(result);
            }

            for (var i = 0; i < model.Nodes.Count; i++)
            {
                var node = model.Nodes[i];
                if (node.ParentIndex >= 0)
                    allNodes[node.ParentIndex].Children.Add(allNodes[i]);
                else
                    scene.RootNode.Children.Add(allNodes[i]);
            }
            #endregion

            var meshLookup = new List<int>();

            #region Meshes
            for (var i = 0; i < model.Meshes.Count; i++)
            {
                var geom = model.Meshes[i];
                if (geom.Submeshes.Count == 0)
                {
                    meshLookup.Add(-1);
                    continue;
                }

                meshLookup.Add(scene.MeshCount);

                foreach (var sub in geom.Submeshes)
                {
                    var m = new Assimp.Mesh($"mesh{i:D3}");
                    var indices = geom.GetTriangleIndicies(sub);

                    var minIndex = indices.Min();
                    var maxIndex = indices.Max();
                    var vertCount = maxIndex - minIndex + 1;

                    indices = indices.Select(x => x - minIndex);

                    var posTransform = model.Bounds?.ElementAtOrDefault(geom.BoundsIndex ?? -1)?.AsTransform() ?? System.Numerics.Matrix4x4.Identity;
                    var texTransform = model.Bounds?.ElementAtOrDefault(geom.BoundsIndex ?? -1)?.AsTextureTransform() ?? System.Numerics.Matrix4x4.Identity;

                    var positions = geom.GetPositions(minIndex, vertCount)?.Select(v => System.Numerics.Vector3.Transform(v, posTransform)).ToList();
                    var texcoords = geom.GetTexCoords(minIndex, vertCount)?.Select(v => System.Numerics.Vector2.Transform(v, texTransform)).ToList();
                    var normals = geom.GetNormals(minIndex, vertCount)?.ToList();
                    var blendIndices = geom.GetBlendIndices(minIndex, vertCount)?.ToList();
                    var blendWeights = geom.GetBlendWeights(minIndex, vertCount)?.ToList();

                    if (positions != null)
                    {
                        m.Vertices.AddRange(positions.Select(v => (v * scale).ToAssimp3D()));

                        //TODO:reimplement this using buffers
                        ////some Halo shaders use position W as the colour alpha - add it to a colour channel to preserve it
                        ////also assimp appears to have issues exporting obj when a colour channel exists so only do this for collada
                        //if (formatId == "collada" && v.Color.Count == 0 && !float.IsNaN(v.Position[0].W))
                        //    m.VertexColorChannels[0].Add(new Assimp.Color4D { R = v.Position[0].W });
                    }

                    if (normals != null)
                        m.Normals.AddRange(normals.Select(v => v.ToAssimp3D()));

                    if (texcoords != null)
                        m.TextureCoordinateChannels[0].AddRange(texcoords.Select(v => v.ToAssimpUV()));

                    var boneLookup = new Dictionary<int, Assimp.Bone>();
                    for (var vIndex = 0; vIndex < vertCount; vIndex++)
                    {
                        if (geom.VertexWeights == VertexWeights.None)
                            continue;

                        #region Vertex Weights
                        var weights = new HashSet<(int Index, float Weight)>(4);

                        if (geom.NodeIndex.HasValue)
                            weights.Add((geom.NodeIndex.Value, 1));
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

                        foreach (var (index, weight) in weights)
                        {
                            Assimp.Bone b;
                            if (boneLookup.ContainsKey(index))
                                b = boneLookup[index];
                            else
                            {
                                var t = model.Nodes[index].OffsetTransform;
                                t.M41 *= scale;
                                t.M42 *= scale;
                                t.M43 *= scale;

                                b = new Assimp.Bone
                                {
                                    Name = bonePrefix + model.Nodes[index].Name,
                                    OffsetMatrix = t.ToAssimp4x4()
                                };

                                m.Bones.Add(b);
                                boneLookup.Add(index, b);
                            }

                            b.VertexWeights.Add(new Assimp.VertexWeight(vIndex, weight));
                        }
                        #endregion
                    }

                    m.SetIndices(indices.ToArray(), 3);
                    m.MaterialIndex = sub.MaterialIndex;

                    scene.Meshes.Add(m);
                }
            }
            #endregion

            #region Regions
            foreach (var reg in model.Regions)
            {
                var regNode = new Assimp.Node($"{geomPrefix}{reg.Name}");
                foreach (var perm in reg.Permutations)
                {
                    var meshStart = meshLookup[perm.MeshIndex];
                    if (meshStart < 0)
                        continue;

                    var permNode = new Assimp.Node($"{geomPrefix}{perm.Name}");
                    if (perm.TransformScale != 1 || !perm.Transform.IsIdentity)
                        permNode.Transform = Assimp.Matrix4x4.FromScaling(new Assimp.Vector3D(perm.TransformScale)) * perm.Transform.ToAssimp4x4(scale);

                    var meshCount = Enumerable.Range(perm.MeshIndex, perm.MeshCount).Sum(i => model.Meshes[i].Submeshes.Count);
                    permNode.MeshIndices.AddRange(Enumerable.Range(meshStart, meshCount));

                    regNode.Children.Add(permNode);
                }

                if (regNode.ChildCount > 0)
                    scene.RootNode.Children.Add(regNode);
            }
            #endregion

            #region Materials
            foreach (var mat in model.Materials)
            {
                var m = new Assimp.Material { Name = mat?.Name ?? "unused" };

                //prevent max from making every material super shiny
                m.ColorEmissive = m.ColorReflective = m.ColorSpecular = new Assimp.Color4D(0, 0, 0, 1);
                m.ColorDiffuse = m.ColorTransparent = new Assimp.Color4D(1);

                //max only seems to care about diffuse
                var dif = mat?.Submaterials.FirstOrDefault(s => s.Usage == 0);
                if (dif != null)
                {
                    var suffix = dif.Bitmap.SubmapCount > 1 ? "[0]" : string.Empty;
                    //var filePath = $"{dif.Bitmap.Name}{suffix}.{ModelViewerPlugin.Settings.MaterialExtension}";
                    var filePath = $"{suffix}.{ModelViewerPlugin.Settings.MaterialExtension}"; // TODO

                    //collada spec says it requires URI formatting, and Assimp doesn't do it for us
                    //for some reason "new Uri(filePath, UriKind.Relative)" doesnt change the slashes, have to use absolute uri
                    if (formatId == FormatId.Collada)
                        filePath = new Uri("X:\\", UriKind.Absolute).MakeRelativeUri(new Uri(System.IO.Path.Combine("X:\\", filePath))).ToString();

                    m.TextureDiffuse = new Assimp.TextureSlot
                    {
                        BlendFactor = 1,
                        FilePath = filePath,
                        TextureType = Assimp.TextureType.Diffuse
                    };
                }

                scene.Materials.Add(m);
            }
            #endregion

            return scene;
        }
    }
}
