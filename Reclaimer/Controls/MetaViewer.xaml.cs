using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Plugins.MetaViewer;
using Reclaimer.Utilities;
using Studio.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MetaViewer.xaml
    /// </summary>
    public partial class MetaViewer : IDisposable
    {
        #region Dependency Properties
        public static readonly DependencyProperty ShowInvisiblesProperty =
            DependencyProperty.Register(nameof(ShowInvisibles), typeof(bool), typeof(MetaViewer), new PropertyMetadata(false, ShowInvisiblesChanged));

        public bool ShowInvisibles
        {
            get => (bool)GetValue(ShowInvisiblesProperty);
            set => SetValue(ShowInvisiblesProperty, value);
        }

        public static void ShowInvisiblesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MetaViewerPlugin.Settings.ShowInvisibles = e.NewValue as bool? ?? false;
        }
        #endregion

        private object item;
        private string fileName;

        private Plugins.MetaViewer.Halo3.MetaContext context;

        public TabModel TabModel { get; }
        public ObservableCollection<MetaValueBase> Metadata { get; }

        public MetaViewer()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Document);
            Metadata = new ObservableCollection<MetaValueBase>();
            DataContext = this;
            ShowInvisibles = MetaViewerPlugin.Settings.ShowInvisibles;
        }

        public void ExportJson(string fileName)
        {
            var tempMetadata = new ObservableCollection<MetaValueBase>();
            var tempContext = default(Plugins.MetaViewer.Halo3.MetaContext);

            if (item is IIndexItem)
                LoadDataHalo3(tempMetadata, ref tempContext);
            else if (item is Blam.Halo5.ModuleItem)
                LoadDataHalo5(tempMetadata);
            else if (item is Blam.HaloInfinite.ModuleItem)
                LoadDataHaloInfinite(tempMetadata);

            var root = new JObject();
            foreach (var item in tempMetadata.Where(i => !string.IsNullOrWhiteSpace(i.Name)))
            {
                var propName = root.ContainsKey(item.Name) ? $"{item.Name}_{item.Offset}" : item.Name;
                root.Add(propName, item.GetJValue());
            }

            File.WriteAllText(fileName, root.ToString());
        }

        public void LoadMetadata(IIndexItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.TagName}.{tag.ClassCode}";
            TabModel.Header = $"{Utils.GetFileName(tag.TagName)}.{tag.ClassCode}";

            item = tag;
            fileName = xmlFileName;

            LoadData();
        }

        public void LoadMetadata(Blam.Halo5.ModuleItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.TagName}.{tag.ClassCode}";
            TabModel.Header = $"{tag.FileName}.{tag.ClassCode}";

            item = tag;
            fileName = xmlFileName;

            LoadData();
        }

        public void LoadMetadata(Blam.HaloInfinite.ModuleItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.TagName}.{tag.ClassCode}";
            TabModel.Header = $"{tag.FileName}.{tag.ClassCode}";

            item = tag;
            fileName = xmlFileName;

            LoadData();
        }

        private void LoadData()
        {
            if (item is IIndexItem)
                LoadDataHalo3(Metadata, ref context);
            else if (item is Blam.Halo5.ModuleItem)
                LoadDataHalo5(Metadata);
            else if (item is Blam.HaloInfinite.ModuleItem)
                LoadDataHaloInfinite(Metadata);
        }

        private void LoadDataHalo3(IList<MetaValueBase> collection, ref Plugins.MetaViewer.Halo3.MetaContext context)
        {
            var tag = item as IIndexItem;
            collection.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            context?.DataSource?.Dispose();
            context = new Plugins.MetaViewer.Halo3.MetaContext(doc, tag.CacheFile, tag);

            foreach (var n in doc.DocumentElement.GetChildElements())
            {
                try
                {
                    var meta = MetaValueBase.GetMetaValue(n, context, tag.MetaPointer.Address);
                    collection.Add(meta);
                }
                catch { }
            }

            context.UpdateBlockIndices();
        }

        private void LoadDataHalo5(IList<MetaValueBase> collection)
        {
            var tag = item as Blam.Halo5.ModuleItem;
            collection.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            var offset = 0;
            using (var tagReader = tag.CreateReader())
            {
                var header = new Blam.Halo5.MetadataHeader(tagReader);
                using (var reader = tagReader.CreateVirtualReader(header.Header.HeaderSize))
                {
                    var rootIndex = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;
                    var mainBlock = header.DataBlocks[rootIndex];

                    foreach (var n in doc.DocumentElement.GetChildElements())
                    {
                        try
                        {
                            var def = FieldDefinition.GetHalo5Definition(n);
                            var meta = MetaValueBase.GetMetaValue(n, tag, header, mainBlock, reader, mainBlock.Offset, offset);
                            collection.Add(meta);
                            offset += def.Size;
                        }
                        catch { break; }
                    }
                }
            }
        }

        private void LoadDataHaloInfinite(IList<MetaValueBase> collection)
        {
            var tag = item as Blam.HaloInfinite.ModuleItem;
            collection.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            var offset = 0;
            using (var tagReader = tag.CreateReader())
            {
                var header = new Blam.HaloInfinite.MetadataHeader(tagReader);
                using (var reader = tagReader.CreateVirtualReader(header.Header.HeaderSize))
                {
                    var rootIndex = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;
                    var mainBlock = header.DataBlocks[rootIndex];

                    foreach (var n in doc.DocumentElement.GetChildElements())
                    {
                        try
                        {
                            var def = FieldDefinition.GetHaloInfiniteDefinition(n);
                            var meta = MetaValueBase.GetMetaValue(n, tag, header, mainBlock, reader, mainBlock.Offset, offset);
                            collection.Add(meta);
                            offset += def.Size;
                        }
                        catch { break; }
                    }
                }
            }
        }

        private static void RecursiveToggle(IEnumerable<MetaValueBase> collection, bool value)
        {
            foreach (var s in collection.OfType<IExpandable>())
            {
                s.IsExpanded = value;
                RecursiveToggle(s.Children, value);
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e) => LoadData();
        
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = (item as IIndexItem)?.FileName ??
                   (item as Blam.Halo5.ModuleItem)?.FileName ??
                   (item as Blam.HaloInfinite.ModuleItem)?.FileName,
                Filter = "JSON Files|*.json",
                FilterIndex = 1,
                AddExtension = true
            };

            if (sfd.ShowDialog() == true)
                ExportJson(sfd.FileName);
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e) => RecursiveToggle(Metadata, false);
        private void btnExpandAll_Click(object sender, RoutedEventArgs e) => RecursiveToggle(Metadata, true);

        public void Dispose() => context?.Dispose();

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Plugins.MetaViewer.Halo3.TagReferenceValue h3ref && h3ref.SelectedItem is not null)
            {
                var item = h3ref.SelectedItem.Context;
                var fileName = $"{item.TagName}.{item.ClassName}";
                var fileKey = $"Blam.{item.CacheFile.CacheType}.{item.ClassCode}";
                var args = new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), item);
                Substrate.OpenWithDefault(args);
            }
            else if (e.Parameter is Plugins.MetaViewer.Halo5.TagReferenceValue h5ref && h5ref.SelectedItem is not null)
            {
                var item = h5ref.SelectedItem.Context;
                var fileName = $"{item.TagName}.{item.ClassName}";
                var fileKey = $"Blam.{item.Module.ModuleType}.{item.ClassCode}";
                var args = new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), item);
                Substrate.OpenWithDefault(args);
            }
            else if (e.Parameter is Plugins.MetaViewer.HaloInfinite.TagReferenceValue infref && infref.SelectedItem is not null)
            {
                var item = infref.SelectedItem.Context;
                var fileName = $"{item.TagName}.{item.ClassName}";
                var fileKey = $"Blam.{item.Module.ModuleType}.{item.ClassCode}";

                ContentFactory.TryGetPrimaryContent(item, out var content);
                var args = new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), content ?? item);
                Substrate.OpenWithDefault(args);
            }
        }
    }
}
