using System;
using DemoModelBuilder.Models;

namespace DemoModelBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            BankMessageSubtypeModelBuilder builder = new BankMessageSubtypeModelBuilder();

            builder.Build();
        }
    }
}
