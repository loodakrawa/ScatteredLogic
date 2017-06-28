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
            DoLotsOfStuff();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private static void TestBasic()
        {
            IEntityWorld ew = EntityManagerFactory.CreateEntityWorld(5, 5);

            Handle e1 = ew.CreateEntity();
            Handle e2 = ew.CreateEntity();

            ew.AddComponent(e1, 5);
            ew.AddComponent(e2, 6);
        }

        private static void TestStuff()
        {
            IGroupedEntityWorld world = EntityManagerFactory.CreateGroupedEntityWorld(10000, BitmaskSize.Bit64);

            int intGroup = world.GetGroupId(RequiredTypes.From<int>());
            int intStrGroup = world.GetGroupId(RequiredTypes.From<int, string>());

            Handle e1 = world.CreateEntity();
            Handle e2 = world.CreateEntity();

            world.AddComponent(e1, 5);
            world.AddComponent(e2, 7);

            IArray<Handle> intSet = world.GetEntitiesForGroup(intGroup);
            IArray<Handle> strSet = world.GetEntitiesForGroup(intStrGroup);

            world.Commit();

            IArray<Handle> intChangeSet = world.GetChangesForGroup(intGroup);
            IArray<Handle> strChangeSet = world.GetChangesForGroup(intStrGroup);

            world.AddComponent(e2, "");
            world.Commit();
            world.RemoveComponent<int>(e2);

            world.Commit();
        }

        private static void DoLotsOfStuff()
        {
            int count = 10;

            IGroupedEntityWorld world = EntityManagerFactory.CreateGroupedEntityWorld(10, BitmaskSize.Bit64);

            for(int c=0; c<10; ++c)
            {
                for (int i = 0; i < count; ++i)
                {
                    Handle entity = world.CreateEntity();
                    world.AddComponent(entity, 5);
                    world.AddComponent(entity, "");
                    if (i % 5 == 0) world.DestroyEntity(entity);
                }

                int groupId = world.GetGroupId(RequiredTypes.From<int, string>());

                world.Commit();

                IArray<Handle> entities = world.GetEntitiesForGroup(groupId);
                foreach(Handle entity in entities) Console.WriteLine(entity);
                Console.WriteLine();

                foreach (Handle entity in entities) world.DestroyEntity(entity);

                world.Commit();

                foreach (Handle entity in entities) Console.WriteLine(entity);
                Console.WriteLine();

            }
        }
    }
}
