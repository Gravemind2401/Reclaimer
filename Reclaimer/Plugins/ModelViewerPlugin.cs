using Adjutant.Geometry;
using Adjutant.Utilities;
using Reclaimer.Annotations;
using Reclaimer.Controls.Editors;
using Reclaimer.Utilities;
using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Reclaimer.Plugins
{
    public class ModelViewerPlugin : Plugin
    {
        private delegate bool GetDataFolder(out string dataFolder);
        private GetDataFolder getDataFolderFunc;

        private delegate bool SaveImage(IBitmap bitmap, string baseDir);
        private SaveImage saveImageFunc;

        internal override int? FilePriority => 1;

        public override string Name => "Model Viewer";

        internal static ModelViewerSettings Settings;

        private PluginContextItem ExtractBitmapsContextItem
        {
            get
            {
                return new PluginContextItem("ExtractBitmaps", "Extract Bitmaps", OnContextItemClick);
            }
        }

        public override void Initialise()
        {
            Settings = LoadSettings<ModelViewerSettings>();
        }

        public override void PostInitialise()
        {
            getDataFolderFunc = Substrate.GetSharedFunction<GetDataFolder>(Constants.SharedFuncGetDataFolder);
            saveImageFunc = Substrate.GetSharedFunction<SaveImage>(Constants.SharedFuncSaveImage);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            if (context.File.OfType<IRenderGeometry>().Any())
                yield return ExtractBitmapsContextItem;
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override bool CanOpenFile(OpenFileArgs args)
        {
            return args.File.Any(i => i is IRenderGeometry);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var model = args.File.OfType<IRenderGeometry>().FirstOrDefault();
            DisplayModel(args.TargetWindow, model, args.FileName);
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            var geometry = context.File.OfType<IRenderGeometry>().FirstOrDefault();
            ExportBitmaps(geometry);
        }

        [SharedFunction]
        public void ExportBitmaps(IRenderGeometry geometry)
        {
            string folder;
            if (!getDataFolderFunc(out folder))
                return;

            Task.Run(() =>
            {
                foreach (var bitm in geometry.GetAllBitmaps())
                {
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
                LogOutput($"Recursive bitmap extract complete for {geometry.Name}.{geometry.Class}");
            });
        }

        [SharedFunction]
        public void DisplayModel(ITabContentHost targetWindow, IRenderGeometry model, string fileName)
        {
            var tabId = $"{Key}::{model.SourceFile}::{model.Id}";
            if (Substrate.ShowTabById(tabId))
                return;

            var container = targetWindow.DocumentPanel;

            LogOutput($"Loading model: {fileName}");

            try
            {
                var viewer = new Controls.ModelViewer
                {
                    LogOutput = LogOutput,
                    LogError = LogError,
                    SetStatus = SetWorkingStatus,
                    ClearStatus = ClearWorkingStatus
                };

                viewer.TabModel.ContentId = tabId;
                viewer.LoadGeometry(model, $"{fileName}");

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
            new ExportFormat(FormatId.AMF,              "amf",  "AMF Files", (model, fileName) => model.WriteAMF(fileName, Settings.GeometryScale)),
            new ExportFormat(FormatId.JMS,              "jms",  "JMS Files", (model, fileName) => model.WriteJMS(fileName, Settings.GeometryScale)),
            new ExportFormat(FormatId.OBJNoMaterials,   "obj",  "OBJ Files"),
            new ExportFormat(FormatId.OBJ,              "obj",  "OBJ Files with materials"),
            new ExportFormat(FormatId.Collada,          "dae",  "COLLADA Files"),
        };

        private static Dictionary<string, ExportFormat> UserFormats = new Dictionary<string, ExportFormat>();

        private static IEnumerable<ExportFormat> ExportFormats => UserFormats.Values.Concat(StandardFormats);

        [SharedFunction]
        public static IEnumerable<string> GetExportFormats() => ExportFormats.Select(f => f.FormatId);

        [SharedFunction]
        public static string GetFormatExtension(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId == formatId.ToLower()).Extension;

        [SharedFunction]
        public static string GetFormatDescription(string formatId) => ExportFormats.FirstOrDefault(f => f.FormatId == formatId.ToLower()).Description;

        [SharedFunction]
        public static void RegisterExportFormat(string formatId, string extension, string description, Action<IGeometryModel, string> exportFunction)
        {
            if (string.IsNullOrWhiteSpace(formatId))
                throw Exceptions.MissingStringParameter(nameof(formatId));

            if (string.IsNullOrWhiteSpace(extension))
                throw Exceptions.MissingStringParameter(nameof(extension));

            if (exportFunction == null)
                throw new ArgumentNullException(nameof(exportFunction));

            formatId = formatId.ToLower();
            extension = extension.ToLower();

            if (UserFormats.ContainsKey(formatId))
                throw new ArgumentException("A format the same ID has already been added.", nameof(formatId));

            UserFormats.Add(formatId, new ExportFormat(formatId, extension, description, exportFunction));
        }

        [SharedFunction]
        public static void WriteModelFile(IGeometryModel model, string fileName, string formatId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            formatId = (formatId ?? Settings.DefaultSaveFormat).ToLower();
            if (!ExportFormats.Any(f => f.FormatId == formatId))
                throw new ArgumentException($"{formatId} is not a supported format.", nameof(formatId));

            var ext = "." + GetFormatExtension(formatId);
            if (!fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                fileName += ext;

            var format = ExportFormats.First(f => f.FormatId == formatId);

            if (format.ExportFunction != null)
                format.ExportFunction(model, fileName);
            else
            {
                using (var context = new Assimp.AssimpContext())
                {
                    var scene = model.CreateAssimpScene(context, formatId);
                    context.ExportFile(scene, fileName, formatId);
                }
            }
        }

        private struct ExportFormat
        {
            public string FormatId { get; }
            public string Extension { get; }
            public string Description { get; }
            public Action<IGeometryModel, string> ExportFunction { get; }

            public ExportFormat(string formatId, string extension, string description, Action<IGeometryModel, string> exportFunction = null)
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
        [DefaultValue(FormatId.AMF)]
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
        public static Assimp.Vector2D ToAssimp2D(this IXMVector v)
        {
            return new Assimp.Vector2D(v.X, v.Y);
        }

        public static Assimp.Vector3D ToAssimp3D(this IXMVector v)
        {
            return new Assimp.Vector3D(v.X, v.Y, v.Z);
        }

        public static Assimp.Vector3D ToAssimp3D(this IXMVector v, float scale)
        {
            return new Assimp.Vector3D(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Assimp.Vector3D ToAssimpUV(this IXMVector v)
        {
            return new Assimp.Vector3D(v.X, 1f - v.Y, 0f);
        }

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
            scene.RootNode.Transform = (CoordinateSystem.HaloCEX * ModelViewerPlugin.Settings.AssimpScale).ToAssimp4x4();

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

            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var node = model.Nodes[i];
                if (node.ParentIndex >= 0)
                    allNodes[node.ParentIndex].Children.Add(allNodes[i]);
                else scene.RootNode.Children.Add(allNodes[i]);
            }
            #endregion

            var meshLookup = new List<int>();

            #region Meshes
            for (int i = 0; i < model.Meshes.Count; i++)
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

                    var indices = geom.Indicies.Skip(sub.IndexStart).Take(sub.IndexLength);

                    var minIndex = indices.Min();
                    var maxIndex = indices.Max();
                    var vertCount = maxIndex - minIndex + 1;

                    if (geom.IndexFormat == IndexFormat.TriangleStrip)
                        indices = indices.Unstrip();

                    indices = indices.Select(x => x - minIndex);
                    var vertices = geom.Vertices.Skip(minIndex).Take(vertCount);

                    if (geom.BoundsIndex >= 0)
                        vertices = vertices.Select(v => (IVertex)new CompressedVertex(v, model.Bounds[geom.BoundsIndex.Value]));

                    int vIndex = -1;
                    var boneLookup = new Dictionary<int, Assimp.Bone>();
                    foreach (var v in vertices)
                    {
                        vIndex++;

                        if (v.Position.Count > 0)
                        {
                            m.Vertices.Add(v.Position[0].ToAssimp3D(scale));

                            //some Halo shaders use position W as the colour alpha - add it to a colour channel to preserve it
                            //also assimp appears to have issues exporting obj when a colour channel exists so only do this for collada
                            if (formatId == "collada" && v.Color.Count == 0 && !float.IsNaN(v.Position[0].W))
                                m.VertexColorChannels[0].Add(new Assimp.Color4D { R = v.Position[0].W });
                        }

                        if (v.Normal.Count > 0)
                            m.Normals.Add(v.Normal[0].ToAssimp3D());

                        if (v.TexCoords.Count > 0)
                            m.TextureCoordinateChannels[0].Add(v.TexCoords[0].ToAssimpUV());

                        if (geom.VertexWeights == VertexWeights.None && !geom.NodeIndex.HasValue)
                            continue;

                        #region Vertex Weights
                        var weights = new List<Tuple<int, float>>(4);

                        if (geom.NodeIndex.HasValue)
                            weights.Add(Tuple.Create<int, float>(geom.NodeIndex.Value, 1));
                        else if (geom.VertexWeights == VertexWeights.Skinned)
                        {
                            var ind = v.BlendIndices[0];
                            var wt = v.BlendWeight[0];

                            if (wt.X > 0)
                                weights.Add(Tuple.Create((int)ind.X, wt.X));
                            if (wt.Y > 0)
                                weights.Add(Tuple.Create((int)ind.Y, wt.Y));
                            if (wt.Z > 0)
                                weights.Add(Tuple.Create((int)ind.Z, wt.Z));
                            if (wt.W > 0)
                                weights.Add(Tuple.Create((int)ind.W, wt.W));
                        }

                        foreach (var val in weights)
                        {
                            Assimp.Bone b;
                            if (boneLookup.ContainsKey(val.Item1))
                                b = boneLookup[val.Item1];
                            else
                            {
                                var t = model.Nodes[val.Item1].OffsetTransform;
                                t.M41 *= scale;
                                t.M42 *= scale;
                                t.M43 *= scale;

                                b = new Assimp.Bone
                                {
                                    Name = bonePrefix + model.Nodes[val.Item1].Name,
                                    OffsetMatrix = t.ToAssimp4x4()
                                };

                                m.Bones.Add(b);
                                boneLookup.Add(val.Item1, b);
                            }

                            b.VertexWeights.Add(new Assimp.VertexWeight(vIndex, val.Item2));
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
                var dif = mat?.Submaterials.FirstOrDefault(s => s.Usage == MaterialUsage.Diffuse);
                if (dif != null)
                {
                    var suffix = dif.Bitmap.SubmapCount > 1 ? "[0]" : string.Empty;
                    var filePath = $"{dif.Bitmap.Name}{suffix}.{ModelViewerPlugin.Settings.MaterialExtension}";

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
