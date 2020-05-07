using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace WindowsService1
{
    public class DITestClass : IDITestClass
    {
        private IDITestClass2 _diTestClass2;

        public DITestClass(IDITestClass2 diTestClass2)
        {
            _diTestClass2 = diTestClass2;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void TestCall()
        {
            logger.Info($"{nameof(DITestClass)} TestCall");
            _diTestClass2.TestCall();
        }
    }
}
