using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSTOClientTestApp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        internal class BindingSource : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged実装 
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            internal BindingSource()
            {
            }

            bool _IsEnableUserInput = true;
            public bool IsEnableUserInput
            {
                get => _IsEnableUserInput;
                set { _IsEnableUserInput = value; OnPropertyChanged(nameof(IsEnableUserInput)); }
            }

            //string _DUMMY;
            //public string DUMMY
            //{
            //    get => _DUMMY;
            //    set { _DUMMY = value; OnPropertyChanged(nameof(DUMMY)); }
            //}

        }

        internal BindingSource m_Bind;

        public MainWindow()
        {
            InitializeComponent();

            m_Bind = new BindingSource();

            DataContext = m_Bind;
        }

        private ServiceToVSTOClient.IVSTOToService m_Client = ServiceToVSTOClient.VSTOToServiceFactory.CreateInstance();

        private async void OnConnectWindowsService(object sender, RoutedEventArgs e)
        {
            m_Bind.IsEnableUserInput = false;

            await m_Client.ConnectAsync();

            m_Bind.IsEnableUserInput = true;

        }

        private int Count;

        private async void OnCallServerCallTest(object sender, RoutedEventArgs e)
        {
            m_Bind.IsEnableUserInput = false;

            Count++;
            await m_Client.ServerCallTestAsync($"testPath{Count}");

            m_Bind.IsEnableUserInput = true;

        }
    }
}
