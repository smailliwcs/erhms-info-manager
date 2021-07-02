﻿using ERHMS.Desktop.Infrastructure;
using System.Windows;
using System.Windows.Controls;

namespace ERHMS.Desktop.Controls
{
    public partial class SearchTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = TextBox.TextProperty.AddOwner(
            typeof(SearchTextBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public SearchTextBox()
        {
            InitializeComponent();
            Button.Margin = Button.Margin.Scale(left: -1.0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Text = "";
        }
    }
}
