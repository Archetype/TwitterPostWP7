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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TwitterPost
{
    public partial class App : Application
    {
        DateTime m_StartupTime;

        static private Uri m_DelayedNavUri;
        static public void Navigate(Uri uri)
        {
            if (((App)App.Current).RootFrame == null)
            {
                m_DelayedNavUri = uri;
                return;
            }
            ((App)App.Current).RootFrame.Navigate(uri);
        }

        static public void Back()
        {
            if (((App)App.Current).RootFrame == null)
            {
                return;
            }
            ((App)App.Current).RootFrame.GoBack();
        }

        private ShellTileSchedule m_ShellTileSchedule;

        /// <summary>
        /// Create the application shell tile schedule instance
        /// </summary>
        private void CreateShellTileSchedule()
        {
            m_ShellTileSchedule = new ShellTileSchedule();
            m_ShellTileSchedule.Recurrence = UpdateRecurrence.Interval;
            m_ShellTileSchedule.Interval = UpdateInterval.EveryHour;
            m_ShellTileSchedule.StartTime = DateTime.Now;
            m_ShellTileSchedule.RemoteImageUri = new Uri(@"http://apps.vervewireless.com/wp7/ap_tile/toppix.jpg");
            m_ShellTileSchedule.Start();
        }

        private PhoneApplicationFrame m_RootFrame;
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame
        {
            get
            {
                return m_RootFrame;
            }
            private set
            {
                m_RootFrame = value;
                return;
                if ((m_RootFrame = value) != null && m_DelayedNavUri != null)
                {
                    if (App.Current is App)
                    {
                        ((App)App.Current).RootFrame.Navigate(m_DelayedNavUri);
                        m_DelayedNavUri = null;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            m_StartupTime = DateTime.Now;
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            //// Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Create the shell tile schedule instance
            CreateShellTileSchedule();

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            if (m_RootFrame != null && m_DelayedNavUri != null)
            {
                if (App.Current is App)
                {
                    ((App)App.Current).RootFrame.Navigate(m_DelayedNavUri);
                    m_DelayedNavUri = null;
                }
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // will nav to where we last were automatically
            m_DelayedNavUri = null;
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;


            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}