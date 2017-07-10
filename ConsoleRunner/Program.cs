using ScatteredLogic;
using System;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestBasic();
            //TestStuff();
            //DoLotsOfStuff();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private static void TestBasic()
        {
            IEntityWorld ew = EntityManagerFactory.CreateEntityWorld(5, 32, BitmaskSize.Bit32);

            Handle e1 = ew.CreateEntity();
            Handle e2 = ew.CreateEntity();

            ew.AddComponent(e1, 5);
            ew.AddComponent(e2, 6);
        }
    }
}
