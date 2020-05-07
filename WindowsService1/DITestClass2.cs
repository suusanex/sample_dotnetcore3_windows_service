using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace WindowsService1
{
    public class DITestClass2 : IDITestClass2
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void TestCall()
        {
            logger.Info($"{nameof(DITestClass2)} TestCall");
        }
    }
}
