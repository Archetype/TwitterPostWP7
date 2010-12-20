using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Twitter;
using TwitterPost.ViewModels;

namespace TwitterPost
{
    public partial class Page1 : PhoneApplicationPage
    {
        TwitterPostViewModel m_ViewModel;

        public Page1()
        {
            InitializeComponent();
    
            // Note that we could we could use script notify but that
            // would require us to host an html file on a server somewhere.
            webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(delegate(object sender, NavigatingEventArgs args)
                {
                    string s = args.Uri.ToString();

                    if (s.StartsWith("http://ap.org/apwp7"))
                    {
                        //Collapse the WebBrowser now that we don't need it
                        webBrowser.Visibility = System.Windows.Visibility.Collapsed;

                        Account.Instance.CompleteAuthorization(s, delegate(string error)
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
                                    MessageBox.Show("Successfully authorized for twitter");
                                });

                                
                                Random random = new Random();

                                int count = random.Next(100, 999);
                                string status = "Test Status Update 私のさえずりを設定する " + count.ToString();

                                MainPageViewModel.PostTestTweet(status);


                                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                {
                                    this.NavigationService.GoBack();
                                });

                            }
                        });
                    }
                });


        }
    }
}