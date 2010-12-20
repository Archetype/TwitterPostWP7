using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Archetype.Controls
{
    public class CommandButton : Button
    {
        public CommandButton()
        {
            Click += new RoutedEventHandler(OnClick);
        }

        void OnClick(object sender, RoutedEventArgs e)
        {
            if (ClickCommand != null)
            {
                ClickCommand.Execute(ClickCommandArgument ?? DataContext);
            }
        }

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(CommandButton), new PropertyMetadata(null));

        public object ClickCommandArgument
        {
            get { return (object)GetValue(ClickCommandArgumentProperty); }
            set { SetValue(ClickCommandArgumentProperty, value); }
        }

        public static readonly DependencyProperty ClickCommandArgumentProperty = DependencyProperty.Register("ClickCommandArgument", typeof(object), typeof(CommandButton), new PropertyMetadata(null));


    }
}
