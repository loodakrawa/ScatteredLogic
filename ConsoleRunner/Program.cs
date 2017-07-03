using ScatteredLogic;
using System;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestBasic();
            TestStuff();
           // DoLotsOfStuff();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private static void TestBasic()
        {
            IEntityWorld ew = EntityManagerFactory.CreateEntityWorld(5, 5, BitmaskSize.Bit32);

            Handle e1 = ew.CreateEntity();
            Handle e2 = ew.CreateEntity();

            ew.AddComponent(e1, 5);
            ew.AddComponent(e2, 6);
        }

        private static void TestStuff()
        {
            IEntityWorld world = EntityManagerFactory.CreateEntityWorld(10000, 32, BitmaskSize.Bit64);

            Handle intGroup = world.CreateComponentSet(RequiredTypes.From<int>());
            Handle intStrGroup = world.CreateComponentSet(RequiredTypes.From<int, string>());

            IArray<int> firstInts = world.GetSetComponents<int>(intGroup);

            IArray<string> secondStrings = world.GetSetComponents<string>(intStrGroup);
            IArray<int> secondInts = world.GetSetComponents<int>(intStrGroup);

            Handle e1 = world.CreateEntity();
            Handle e2 = world.CreateEntity();

            world.AddComponent(e1, 5);
            world.AddComponent(e2, 7);

            world.AddComponent(e2, "");
            world.UpdateComponent(e2, "cmuw");
            world.RemoveComponent<int>(e2);
        }

        private static void DoLotsOfStuff()
        {
            int count = 10;

            IEntityWorld world = EntityManagerFactory.CreateEntityWorld(count, 32, BitmaskSize.Bit64);

            //for(int c=0; c<10; ++c)
            //{
            //    for (int i = 0; i < count; ++i)
            //    {
            //        Handle entity = world.CreateEntity();
            //        world.AddComponent(entity, 5);
            //        world.AddComponent(entity, "");
            //        //if (i % 5 == 0) world.DestroyEntity(entity);
            //    }

            //    int groupId = world.GetGroupId(RequiredTypes.From<int, string>());

            //    IArray<Handle> entities = world.GetEntitiesForGroup(groupId);
            //    foreach(Handle entity in entities) Console.WriteLine(entity);
            //    Console.WriteLine();

            //    foreach (Handle entity in entities) world.DestroyEntity(entity);

            //    foreach (Handle entity in entities) Console.WriteLine(entity);
            //    Console.WriteLine();

            //}
        }
    }
}
