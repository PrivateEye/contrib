using System;
using System.Threading;
using PrivateEye.Bridge;

namespace BasicApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DoSomethingElse();
            DoSomeYetSomethingElse();

            for (int i = 0; i < 1000000; i++)
            {
                var x = AllocateSomething();
                NamedMethodCommands.ForceFlushProfiler();
                Thread.Sleep(1000);
            }
            Console.WriteLine("done");
        }

        private static void DoSomeYetSomethingElse()
        {
            NamedMethodCommands.StartProcessing();
            NamedMethodCommands.EndProcessing();
            NamedMethodCommands.StartProcessing();
            NamedMethodCommands.ForceFlushProfiler();
            NamedMethodCommands.EnterMatryoshka();
            NamedMethodCommands.LeaveMatryoshka();
            NamedMethodCommands.ForceFlushProfiler();
        }

        private static void DoSomethingElse()
        {
        }

        private static Foo AllocateSomething()
        {
            return new Foo();
        }
    }

    internal class Foo
    {
    }
}
