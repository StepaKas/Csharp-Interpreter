using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PJP_projekt
{
    public class VirtualMachine
    {
        List<string> code;
        Stack<object> stack = new Stack<object>();

        public VirtualMachine(string code)
        {
            this.code = code.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public void Run()
        {
            return;
            foreach (var item in code)
            {
                if (item.StartsWith("print"))
                {
                    Console.WriteLine(stack.Pop());
                }
                else if (item.StartsWith("push"))
                {
                    var data = item.Split(" ");
                    var value = int.Parse(data[1]);
                    stack.Push(value);
                }
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    //var result = item switch
                    //{
                    //    "add" => left + right,
                    //    "sub" => left - right,
                    //    "mul" => left * right,
                    //    "div" => left / right,
                    //    _ => throw new Exception("Unexpected instruction")
                    //};
                   // stack.Push(result);
                }
            }
        }
    }
}
