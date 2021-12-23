using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Soundboard.ViewModel;
using System;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
#pragma warning disable CS8622
namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);

            NotifyIcon ni = new()
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = true
            };

            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                };

            ni.ContextMenuStrip = new();

            ni.ContextMenuStrip.Items.Add("Beenden", null, (sender, args) =>
            {
                Close();
            });

            //Hide();
        }

        private void App_Closing(object sender, CancelEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.SaveConfig();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }
    }
}
#pragma warning restore CS8622
