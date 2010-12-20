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

namespace TwitterPost.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ICommand m_ClickPostTweet;
        public ICommand ClickPostTweet
        {
            get
            {
                return m_ClickPostTweet;
            }
            set
            {
                if (m_ClickPostTweet == value)
                {
                    return;
                }
                m_ClickPostTweet = value;
                NotifyPropertyChanged("ClickPostTweet");
            }
        }



        private TwitterPostViewModel m_TwitterPostViewModel;
        public TwitterPostViewModel TwitterPostViewModel
        {
            get
            {
                return m_TwitterPostViewModel;
            }
            set
            {
                if (m_TwitterPostViewModel == value)
                {
                    return;
                }
                m_TwitterPostViewModel = value;
                NotifyPropertyChanged("TwitterPostViewModel");
            }
        }


        public MainPageViewModel()
        {
            TwitterPostViewModel = new TwitterPostViewModel();

            ClickPostTweet = new DelegateCommand
            {
                ExecuteCommand = delegate()
                {
                    Account.Instance.VerifyCredentials(delegate(string authorizationUrl, string error)
                    {
                        if (authorizationUrl != null)
                        {
                            TwitterPostViewModel.WebUrl = authorizationUrl;

                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                            {
                                TwitterPost.App.Navigate(new Uri("/Pages/TwitterPost.xaml", UriKind.Relative));
                            });
                        }
                        else
                        {
                            if (error != null)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                {
                                    MessageBox.Show(error);
                                });
                            }
                            else
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                {
                                    MessageBox.Show("Already authorized");
                                });

                                Random random = new Random();

                                int count = random.Next(100, 999);
                                string status = "Test Status Update from saved credentials 私のさえずりを設定する " + count.ToString();

                                MainPageViewModel.PostTestTweet(status);
                            }
                        }
                    });
                }
            };

        }

        public static void PostTestTweet(string status)
        {
            Account.Instance.UpdateStatus(status, delegate(string twitterStatusUpdateError)
            {
                if (twitterStatusUpdateError != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        MessageBox.Show("Error updating twitter status: " + twitterStatusUpdateError);
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        MessageBox.Show("Tweet successfully posted");
                    });
                }
            });
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
