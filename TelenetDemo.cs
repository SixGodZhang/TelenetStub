using System;
using System.Collections.Generic;
using System.Text;

namespace objActiveSolutions
{
    public class TelenetDemo
    {
        public static void Main(string[] args)
        {
            TelnetStub telnet = new TelnetStub("127.0.0.1", 50001);
            bool isConnet = telnet.Connect();
            if (!isConnet)
            {
                Console.WriteLine("failed!");
            }
            else
            {
                Console.WriteLine("success!");
                while(true)
                {
                    String inputText = Console.ReadLine();
                    telnet.SendMessage(inputText);
                }
            }
        }
    }
}
