using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace CSharpApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Interpreter");
            Console.Write("Enter an Expression: ");
            string input = Console.ReadLine();
            int result = Interpreter.interpret(input);
            Console.WriteLine("Final result = {0}", result);
            Console.ReadLine();
        }
    }
}
