using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using FastFoodSystem.Scripts.Billing;

namespace ControlCodeAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            string dosificationCode = "9rCB7Sv4X29d)5k7N%3ab89p-3(5[A";
            string authorizationCode = "29040011007";
            string billNumber = "1503";
            string clientNit = "4189179011";
            DateTime saleDateTime = new DateTime(2007, 7, 2);
            decimal totalValue = 2500;

            string code = ControlCodeGenerator.Generate(billNumber, clientNit, saleDateTime, totalValue, dosificationCode, authorizationCode);
            Console.WriteLine(code);
        }
    }
}
