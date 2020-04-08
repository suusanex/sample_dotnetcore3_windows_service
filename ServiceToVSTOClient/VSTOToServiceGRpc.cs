using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;
using ServiceToVSTO;

namespace ServiceToVSTOClient
{
    class VSTOToServiceGRpc : IVSTOToService
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Channel m_Channel;
        private AsyncClientStreamingCall<VSTOToServiceRequest, ServiceToVSTOResponse> m_ServiceCallStream;
        private CancellationTokenSource m_ResponseWaitCancel;

        public async Task ConnectAsync()
        {
            try
            {
                m_Channel = new Channel("localhost:51232", ChannelCredentials.Insecure);
                m_ResponseWaitCancel = new CancellationTokenSource();
                var client = new WindowsServiceToVSTOService.WindowsServiceToVSTOServiceClient(m_Channel);
                m_ServiceCallStream = client.Subscribe(cancellationToken: m_ResponseWaitCancel.Token);

                logger.Info($"Subscribe End, Stream={m_ServiceCallStream},{m_ServiceCallStream.GetHashCode()}");


                using (var currentProcess = Process.GetCurrentProcess())
                {
                    await m_ServiceCallStream.RequestStream.WriteAsync(new VSTOToServiceRequest
                    {
                        RegisterVSTO = new RegisterVSTORequest
                        {
                            SessionId = currentProcess.SessionId,
                            ProcessId = currentProcess.Id
                        }
                    }).ConfigureAwait(false);
                }

                logger.Info("RegisterUserSessionRequest End");
            }
            catch (Exception e)
            {
                logger.Warn($"{e}");
                throw;
            }


        }

        public async Task ServerCallTestAsync(string path)
        {
            try
            {
                await m_ServiceCallStream.RequestStream.WriteAsync(new VSTOToServiceRequest
                {
                    ServerCallTestRequestCall = new ServerCallTestRequest
                    {
                        Path = path
                    }
                });

                logger.Info($"ServerCallTest {path} End");
            }
            catch (Exception exception)
            {
                logger.Warn(exception.ToString());
                throw;
            }
        }

        readonly TimeSpan m_DisposeTimeout = new TimeSpan(0, 0, 10);

        public void Dispose()
        {
            m_ResponseWaitCancel?.Cancel();

            if (m_ServiceCallStream != null)
            {
                try
                {
                    if (!m_ServiceCallStream.RequestStream.CompleteAsync().Wait(m_DisposeTimeout))
                    {
                        logger.Warn($"Dispose CompleteAsync Timeout");
                    }
                }
                finally
                {
                    m_ServiceCallStream.Dispose();
                }
            }

            if (m_Channel != null)
            {
                if (!m_Channel.ShutdownAsync().Wait(m_DisposeTimeout))
                {
                    logger.Warn($"Dispose ShutdownAsync Timeout");
                }
            }

        }
    }
}
