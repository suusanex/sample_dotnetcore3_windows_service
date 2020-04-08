using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceToVSTOClient
{
    public static class VSTOToServiceFactory
    {
        public static IVSTOToService CreateInstance()
        {
            return new VSTOToServiceGRpc();
        }

    }
}
