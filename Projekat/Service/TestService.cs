using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service
{
    public class TestService : ITest
    {
        public void TestConnection()
        {
            Console.WriteLine("[ CONNECTION WORKING ] This is a test message.\n");
        }
    }
}
