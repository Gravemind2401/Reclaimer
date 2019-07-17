using Adjutant.Blam.Common;
using Reclaimer.Utils;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MetaViewer.xaml
    /// </summary>
    public partial class MetaViewer : DocumentItem
    {
        public ObservableCollection<MetaValue> Metadata { get; }

        public MetaViewer()
        {
            InitializeComponent();
            Metadata = new ObservableCollection<MetaValue>();
            DataContext = this;
        }

        public void LoadMetadata(IIndexItem tag, XmlDocument definition)
        {
            TabToolTip = $"{tag.FileName}.{tag.ClassCode}";
            TabHeader = $"{Path.GetFileName(tag.FileName)}.{tag.ClassCode}";

            foreach (XmlNode n in definition.DocumentElement.ChildNodes)
            {
                try
                {
                    var meta = MetaValue.GetValue(n, tag.CacheFile, tag.MetaPointer.Address);
                    Metadata.Add(meta);
                }
                catch { }
            }
        }
    }
}
