using ScatteredLogic;
using System;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            TestStuff();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private static void TestStuff()
        {
            IGroupedEntityWorld world = EntityManagerFactory.CreateGroupedEntityWorld(10, BitmaskSize.Bit64);

            int intGroup = world.GetGroupId(RequiredTypes.From<int>());
            int intStrGroup = world.GetGroupId(RequiredTypes.From<int, string>());

            Handle e1 = world.CreateEntity();
            Handle e2 = world.CreateEntity();

            world.AddComponent(e1, 5);
            world.AddComponent(e2, 7);
            
            IHandleSet intSet = world.GetEntitiesForGroup(intGroup);
            IHandleSet intStrSet = world.GetEntitiesForGroup(intStrGroup);

            world.Flush();

            world.AddComponent(e2, "");
            world.Flush();
            world.RemoveComponent<int>(e2);

            world.Flush();
        }

        
    }
}
