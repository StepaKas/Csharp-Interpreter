using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PJP_projekt
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var inputFile2 = new StreamReader(@"..\..\..\Text.txt");
            AntlrInputStream input = new AntlrInputStream(inputFile2);

            GrammarLexer lexer = new GrammarLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);


            var parser = new GrammarParser(tokens);
         
            var context =  parser.program();

            
            parser.AddErrorListener(new VerboseListener());


            //

            if (parser.NumberOfSyntaxErrors == 0)
            {
                var visitor = new GrammarVisitor();
                visitor.Visit(context);
                var str = visitor.sb.ToString();
                //Console.WriteLine(str);
                //VirtualMachine vm = new VirtualMachine(str);
                //vm.Run();
            }
            else
            {
                Console.WriteLine(context.ToStringTree(parser));
            }
            //{
            //    ParseTreeWalker walker = new ParseTreeWalker();
            //    walker.Walk(new EvalListener(), tree);


            //    //EvalVisitor visitor = new EvalVisitor();
            //    //visitor.Visit(tree);


            //}
        }
    }
}
