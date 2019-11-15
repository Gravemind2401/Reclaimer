using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Models
{
    public class DocumentWell : TabWellBase
    {
        public DocumentPanel ParentPanel => Parent as DocumentPanel;

        protected override void TogglePinStatusExecuted(TabItem item)
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
