using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceToVSTOClient
{
    public interface IVSTOToService : IDisposable
    {
        Task ConnectAsync();

        Task ServerCallTestAsync(string path);

    }

}
