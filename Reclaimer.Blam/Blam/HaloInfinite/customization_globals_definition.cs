using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Linq;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class customization_globals_definition : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        [Offset(16)]
        public BlockCollection<ObjectTheme> Themes { get; set; }

        public customization_globals_definition(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private static void AddAttachment(TagReference attachmentReference, ModelRegion themeRegion, Model model)
        {
            var attachment = attachmentReference.Tag;
            if (attachment != null)
            {
                var attachments = attachment.ReadMetadata<customization_attachment_configuration>().ModelAttachments.FirstOrDefault();
                if (attachments == null)
                    return;

                var attachmentModel = attachments.AttachmentModel.Tag?.ReadMetadata<model>().RenderModel.Tag?.ReadMetadata<render_model>().GetModelContent();
                if (attachmentModel == null)
                    return;

                model.Meshes.AddRange(attachmentModel.Meshes);

                var newPermutation = new ModelPermutation
                {
                    Name = attachmentModel.Name,
                    MeshRange = (model.Meshes.Count - 1, 1)
                };

                themeRegion.Permutations.Add(newPermutation);
            }
        }
        
        private static void AddPermutations(IEnumerable<ObjectRegion> items, Dictionary<string, ModelRegion> armorRegions, ModelRegion themeRegion, Model model)
        {
            foreach (var item in items)
            {
                foreach (var permutationRegion in item.PermutationRegions)
                {
                    if (!armorRegions.TryGetValue(permutationRegion, out ModelRegion modelRegion))
                        continue;

                    foreach (var setting in item.PermutationSettings)
                    {
                        ModelPermutation permutation = null;
                        for (var i = 0; i < modelRegion.Permutations.Count; i++)
                        {
                            if (modelRegion.Permutations[i].Name == setting.Name)
                            {
                                permutation = modelRegion.Permutations[i];
                                break;
                            }
                        }

                        if (permutation != null)
                            themeRegion.Permutations.Add(permutation);

                        AddAttachment(setting.Attachment, themeRegion, model);
                    }
                }
            }
        }

        public Model GetModelContent()
        {
            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            var armor = Themes[0].Model.Tag.ReadMetadata<render_model>().GetModelContent();
            var armorRegions = armor.Regions.ToDictionary(r => r.Name);
            model.Meshes.AddRange(armor.Meshes);
            model.Markers.AddRange(armor.Markers);
            model.Bones.AddRange(armor.Bones);
            var themes = new HashSet<uint>();

            foreach (var theme in Themes[0].ThemeConfigurations)
            {
                if (!themes.Contains(theme.ThemeName.Hash))
                    themes.Add(theme.ThemeName.Hash);
                else
                    continue;

                var themeRegion = new ModelRegion { Name = theme.ThemeName };
                var themeConfig = theme.ThemeConfigs.Tag.ReadMetadata<customization_theme_configuration>();

                AddPermutations(themeConfig.Regions, armorRegions, themeRegion, model);
                AddPermutations(themeConfig.Prosthetics, armorRegions, themeRegion, null);
                AddPermutations(themeConfig.BodyTypes, armorRegions, themeRegion, null);

                themeConfig.Attachments.ToList().ForEach(attachment => AddAttachment(attachment, themeRegion, model));

                model.Regions.Add(themeRegion);
            }

            return model;
        }

        #endregion
    }

    [FixedSize(80)]
    public class ObjectTheme
    {
        [Offset(0)]
        public StringHash AssetName {  get; set; }
        [Offset(4)]
        public TagReference Model { get; set; }
        [Offset(32)]
        public TagReference ObjectReference { get; set; }
        [Offset(60)]
        public BlockCollection<ThemeConfiguration> ThemeConfigurations { get; set; }

    }

    [FixedSize(56)]
    public class ThemeConfiguration
    {
        [Offset(0)]
        public StringHash ThemeName { get; set; }
        [Offset(4)]
        public StringHash ThemeVariantName { get; set; }
        [Offset(8)]
        public TagReference ThemeConfigs { get; set; }
    }
}