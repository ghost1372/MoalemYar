﻿using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls
{
    public class Rate : SimpleItemsControl
    {
        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
            "ItemMargin", typeof(Thickness), typeof(Rate), new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(Rate), new PropertyMetadata(ValueBoxes.Double20Box));

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(double), typeof(Rate), new PropertyMetadata(ValueBoxes.Double20Box));

        public static readonly DependencyProperty AllowHalfProperty = DependencyProperty.Register(
            "AllowHalf", typeof(bool), typeof(Rate), new FrameworkPropertyMetadata(ValueBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            "AllowClear", typeof(bool), typeof(Rate), new FrameworkPropertyMetadata(ValueBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(Geometry), typeof(Rate), new FrameworkPropertyMetadata(default(Geometry), FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof(int), typeof(Rate), new PropertyMetadata(ValueBoxes.Int5Box));

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
            "DefaultValue", typeof(double), typeof(Rate), new PropertyMetadata(ValueBoxes.Double0Box));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(Rate), new PropertyMetadata(ValueBoxes.Double0Box));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Rate), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
            "ShowText", typeof(bool), typeof(Rate), new PropertyMetadata(ValueBoxes.FalseBox));

        private bool _isLoaded;

        public Rate()
        {
            AddHandler(RateItem.SelectedChangedEvent, new RoutedEventHandler(RateItemSelectedChanged));
            AddHandler(RateItem.ValueChangedEvent, new RoutedEventHandler(RateItemValueChanged));

            Loaded += (s, e) =>
            {
                if (DesignerHelper.IsInDesignMode) return;
                if (_isLoaded) return;
                _isLoaded = true;
                if (Value <= 0)
                {
                    if (DefaultValue > 0)
                    {
                        Value = DefaultValue;
                        UpdateItems();
                    }
                }
                else
                {
                    UpdateItems();
                }
            };
        }

        public Thickness ItemMargin
        {
            get => (Thickness) GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        public double ItemWidth
        {
            get => (double) GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public double ItemHeight
        {
            get => (double) GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public bool AllowHalf
        {
            get => (bool) GetValue(AllowHalfProperty);
            set => SetValue(AllowHalfProperty, value);
        }

        public bool AllowClear
        {
            get => (bool) GetValue(AllowClearProperty);
            set => SetValue(AllowClearProperty, value);
        }

        public Geometry Icon
        {
            get => (Geometry) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public int Count
        {
            get => (int) GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public double DefaultValue
        {
            get => (double) GetValue(DefaultValueProperty);
            set => SetValue(DefaultValueProperty, value);
        }

        public double Value
        {
            get => (double) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool ShowText
        {
            get => (bool) GetValue(ShowTextProperty);
            set => SetValue(ShowTextProperty, value);
        }

        private void RateItemValueChanged(object sender, RoutedEventArgs e)
        {
            Value =
                (from RateItem item in Items where item.IsSelected select item.IsHalf ? 0.5 : 1).Sum();
        }

        private void RateItemSelectedChanged(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is RateItem rateItem)
            {
                var index = rateItem.Index;
                for (var i = 0; i < index; i++)
                {
                    if (Items[i] is RateItem item)
                    {
                        item.IsSelected = true;
                        item.IsHalf = false;
                    }
                }

                for (var i = index; i < Count; i++)
                {
                    if (Items[i] is RateItem item)
                    {
                        item.IsSelected = false;
                        item.IsHalf = false;
                    }
                }
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RateItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RateItem();
        }

        public override void OnApplyTemplate()
        {
            if (!_isLoaded)
            {
                Items.Clear();

                for (var i = 1; i <= Count; i++)
                {
                    Items.Add(new RateItem
                    {
                        Index = i
                    });
                }
            }

            base.OnApplyTemplate();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            UpdateItems();
        }

        private void UpdateItems()
        {
            var count = (int) Value;
            for (var i = 0; i < count; i++)
            {
                if (Items[i] is RateItem rateItem)
                {
                    rateItem.IsSelected = true;
                    rateItem.IsHalf = false;
                }
            }

            if (Value > count)
            {
                if (Items[count] is RateItem rateItem)
                {
                    rateItem.IsSelected = true;
                    rateItem.IsHalf = true;
                }

                count += 1;
            }

            for (var i = count; i < Count; i++)
            {
                if (Items[i] is RateItem rateItem)
                {
                    rateItem.IsSelected = false;
                    rateItem.IsHalf = false;
                }
            }
        }

        public void Reset()
        {
            Value = DefaultValue;
        }
    }
}