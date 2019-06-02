using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Controls
{
    public abstract class ControlBase : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyPropertyKey TabHeaderPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TabHeader), typeof(object), typeof(ControlBase), new PropertyMetadata());

        public static readonly DependencyProperty TabHeaderProperty = TabHeaderPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey TabToolTipPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TabToolTip), typeof(object), typeof(ControlBase), new PropertyMetadata());

        public static readonly DependencyProperty TabToolTipProperty = TabToolTipPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey TabIconPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TabIcon), typeof(object), typeof(ControlBase), new PropertyMetadata());

        public static readonly DependencyProperty TabIconProperty = TabIconPropertyKey.DependencyProperty;

        public object TabHeader
        {
            get { return GetValue(TabHeaderProperty); }
            protected set { SetValue(TabHeaderPropertyKey, value); }
        }

        public object TabToolTip
        {
            get { return GetValue(TabToolTipProperty); }
            protected set { SetValue(TabToolTipPropertyKey, value); }
        }

        public object TabIcon
        {
            get { return GetValue(TabIconProperty); }
            protected set { SetValue(TabIconPropertyKey, value); }
        }

        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (property == null && value == null)
                return false;

            if (property != null && value != null && property.Equals(value))
                return false;

            property = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
