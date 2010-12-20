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
using Archetype.Commands;
using System.ComponentModel;
using Twitter;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Navigation;

namespace TwitterPost.ViewModels
{
    public class TwitterPostViewModel : INotifyPropertyChanged
    {
        // We set this in the MainPageViewModel
        private string m_WebUrl;
        public string WebUrl
        {
            get
            {
                return m_WebUrl;
            }
            set
            {
                if (m_WebUrl == value)
                {
                    return;
                }
                m_WebUrl = value;
                NotifyPropertyChanged("WebUrl");
            }
        }

        private bool m_IsWebVisible = true;
        public bool IsWebVisible
        {
            get
            {
                return m_IsWebVisible;
            }
            set
            {
                if (m_IsWebVisible == value)
                {
                    return;
                }
                m_IsWebVisible = value;
                NotifyPropertyChanged("IsWebVisible");
            }
        }

        public TwitterPostViewModel()
        {
        }
        
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
