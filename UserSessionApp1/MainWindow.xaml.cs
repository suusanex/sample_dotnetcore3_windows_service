using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
using Grpc.Core;
using Grpc.Net.Client;
using ServiceToUserSession;

namespace UserSessionApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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

            string _Result;
            public string Result
            {
                get => _Result;
                set { _Result = value; OnPropertyChanged(nameof(Result)); }
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

        private GrpcChannel m_Channel;
        private AsyncDuplexStreamingCall<UserSesionToServiceRequest, ServiceToUserSessionResponse> m_DuplexStream;
        private CancellationTokenSource m_ResponseWaitCancel;

        void TraceWrite(string msg)
        {
            Dispatcher.Invoke(() => m_Bind.Result += $"{msg}{Environment.NewLine}");
        }

        private async void OnConnectWindowsService(object sender, RoutedEventArgs e)
        {
            try
            {
                await DisposeAsync();

                TraceWrite($"{nameof(OnConnectWindowsService)} Start");

                AppContext.SetSwitch(
                    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                m_Channel = GrpcChannel.ForAddress("http://localhost:51232");

                m_ResponseWaitCancel = new CancellationTokenSource();
                var client = new WindowsServiceToUserSessionService.WindowsServiceToUserSessionServiceClient(m_Channel);
                m_DuplexStream = client.Subscribe(cancellationToken: m_ResponseWaitCancel.Token);

                TraceWrite($"Subscribe End, Stream={m_DuplexStream},{m_DuplexStream.GetHashCode()}");

                await m_DuplexStream.RequestStream.WriteAsync(new UserSesionToServiceRequest
                {
                    RegisterUserSession = new RegisterUserSessionRequest
                    {
                        SessionId = Process.GetCurrentProcess().SessionId
                    }
                });

                TraceWrite("RegisterUserSessionRequest End");

                var stream = m_DuplexStream.ResponseStream;

                await foreach (var command in stream.ReadAllAsync(m_ResponseWaitCancel.Token))
                {
                    TraceWrite($"Read, {command.ActionCase}");

                    switch (command.ActionCase)
                    {
                        case ServiceToUserSessionResponse.ActionOneofCase.None:
                            break;
                        case ServiceToUserSessionResponse.ActionOneofCase.ExpandEnvironmentStringsAsUserCall:
                            var val = command.ExpandEnvironmentStringsAsUserCall;
                            var path = Environment.ExpandEnvironmentVariables(val.PathEnv);
                            await m_DuplexStream.RequestStream.WriteAsync(new UserSesionToServiceRequest
                            {
                                ExpandEnvironmentStringsAsUserReturn = new ExpandEnvironmentStringsAsUserResponse
                                {
                                    Path = path
                                }
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }


            }
            catch (Exception exception)
            {
                TraceWrite(exception.ToString());
            }


            TraceWrite($"{nameof(OnConnectWindowsService)} End");
        }


        public async ValueTask DisposeAsync()
        {
            m_ResponseWaitCancel?.Cancel();

            if (m_DuplexStream != null)
            {
                try
                {
                    await m_DuplexStream.RequestStream.CompleteAsync().ConfigureAwait(false);
                }
                finally
                {
                    m_DuplexStream.Dispose();
                }
            }

            m_Channel?.Dispose();

        }

        private int CallTestIndex;

        private async void OnCallServerCallTest(object sender, RoutedEventArgs e)
        {
            CallTestIndex++;

            await m_DuplexStream.RequestStream.WriteAsync(new UserSesionToServiceRequest
            {
                ServerCallTestRequestCall = new ServerCallTestRequest
                {
                    Number = CallTestIndex
                }
            });

            TraceWrite($"ServerCallTest {CallTestIndex} End");
        }
    }
}
