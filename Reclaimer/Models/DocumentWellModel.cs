using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Models
{
    public class DocumentWellModel : TabWellModelBase
    {
        public DocumentPanelModel ParentPanel => Parent as DocumentPanelModel;

        protected override void TogglePinStatusExecuted(TabModel item)
        {
            item.IsPinned = !item.IsPinned;
        }

        protected override void OnChildrenChanged()
        {
            if (Children.Count == 0)
                ParentPanel?.Children.Remove(this);
        }
    }
}
