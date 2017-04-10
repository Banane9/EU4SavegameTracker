using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;

namespace EU4SavegameInfo.NightbotUpdater
{
    /// <summary>
    /// Interaction logic for OAuthBrowserWindow.xaml
    /// </summary>
    public partial class OAuthBrowserWindow : Window
    {
        public OAuthBrowserWindow()
        {
            InitializeComponent();
            browser.FrameLoadStart += browser_Navigated;
        }

        private void browser_Navigated(object sender, FrameLoadStartEventArgs e)
        {
            var uri = new Uri(e.Url);
            if (uri.Host == "banane9.github.io")
            {
                var query = uri.Query;
                var codeIndex = query.IndexOf("code");

                if (codeIndex > 0)
                {
                    codeIndex += 5;
                    var ampersandIndex = query.IndexOf('&', codeIndex);

                    string code;
                    if (ampersandIndex < 0)
                        code = query.Substring(codeIndex);
                    else
                    {
                        var codeLength = ampersandIndex - codeIndex;
                        code = query.Substring(codeIndex, codeLength);
                    }

                    Dispatcher.InvokeAsync(() =>
                    {
                        DataContext = code;
                        Close();
                    });
                }
                else if (uri.Query.Contains("access_denied"))
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(this, "You must grant access to the application for the Updater to work!", "Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    });
                }
            }
        }
    }
}