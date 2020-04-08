using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;
using ServiceToVSTO;

namespace WindowsService1
{
    public class WindowsServiceToVSTOServer : WindowsServiceToVSTOService.WindowsServiceToVSTOServiceBase
    {
        class SubscribeData
        {
            public IAsyncStreamReader<VSTOToServiceRequest> RequestStream;
            public ServerCallContext Context;
        }

        private ConcurrentDictionary<int, SubscribeData> m_UserSessions = new ConcurrentDictionary<int, SubscribeData>();

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public override async Task<ServiceToVSTOResponse> Subscribe(IAsyncStreamReader<VSTOToServiceRequest> requestStream, ServerCallContext context)
        {
            logger.Info($"Start, ServerCallContext={context},{context.GetHashCode()}");
            try
            {
                await RequestWaitAsync(new SubscribeData
                {
                    Context = context,
                    RequestStream = requestStream
                }, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.Info($"Connection Canceled, {context},{context.GetHashCode()}");
            }
            catch (Exception e)
            {
                logger.Warn($"{e}");
            }

            logger.Info($"Connection End, {context},{context.GetHashCode()}");

            return new ServiceToVSTOResponse();
        }


        async Task RequestWaitAsync(SubscribeData subscribe, CancellationToken cancellationToken)
        {
            logger.Info($"RequestWaitAsync Start");

            await foreach (var req in subscribe.RequestStream.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                logger.Info($"Case {req.ActionCase}");

                switch (req.ActionCase)
                {

                    case VSTOToServiceRequest.ActionOneofCase.None:
                        logger.Warn("ActionCase None");
                        break;
                    case VSTOToServiceRequest.ActionOneofCase.RegisterVSTO:
                        {
                            var val = req.RegisterVSTO;
                            logger.Info($"RegisterUserSession Call, {val.SessionId}, {val.ProcessId}");
                            m_UserSessions.TryAdd(val.SessionId, subscribe);
                        }
                        break;
                    case VSTOToServiceRequest.ActionOneofCase.ServerCallTestRequestCall:
                        {
                            var val = req.ServerCallTestRequestCall;
                            logger.Info($"ServerCallTestRequestCall Call, {val.Path}");
                        }
                        break;
                    default:
                        logger.Info($"ActionCase Unknown, {req.ActionCase}");
                        break;

                }
            }

        }

    }
}
