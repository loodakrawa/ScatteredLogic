using ScatteredLogic;
using System;
using System.Threading;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            ITask a = new TaskA();
            ITask b = new TaskB();

            TestDelegate td = new TestDelegate();

            td.Reset(a);
            td.WaitCallback(null);
            td.Reset(b);
            td.WaitCallback(null);


            //TestBasic();
            //TestStuff();
            //DoLotsOfStuff();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private class TestDelegate
        {
            public readonly WaitCallback WaitCallback;

            private ITask task;

            public TestDelegate()
            {
                WaitCallback = Execute;
            }

            public void Reset(ITask task)
            {
                this.task = task;
            }

            private void Execute(object state)
            {
                task.Run();
            }
        }

        private class TaskA : ITask
        {
            public void Run()
            {
                Console.WriteLine("A");
            }
        }

        private class TaskB : ITask
        {
            public void Run()
            {
                Console.WriteLine("B");
            }
        }

        public interface ITask
        {
            void Run();
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
            IEntityWorld world = EntityManagerFactory.CreateEntityWorld(2, 2, BitmaskSize.Bit64);

            Handle intGroup = world.CreateAspect(RequiredTypes.From<int>(), "");
            Handle intStrGroup = world.CreateAspect(RequiredTypes.From<int, string>(), "");

            IArray<int> firstInts = world.GetAspectComponents<int>(intGroup);

            IArray<string> secondStrings = world.GetAspectComponents<string>(intStrGroup);
            IArray<int> secondInts = world.GetAspectComponents<int>(intStrGroup);

            Handle e1 = world.CreateEntity();
            Handle e2 = world.CreateEntity();

            world.AddComponent(e1, 5);
            world.AddComponent(e2, 7);

            world.AddComponent(e2, "");
            world.AddComponent(e2, "cmuw");
            world.RemoveComponent<int>(e2);
        }

        private static void DoLotsOfStuff()
        {
            int count = 1000000;

            IEntityWorld world = EntityManagerFactory.CreateEntityWorld(count, 32, BitmaskSize.Bit64);
            Handle aspectId = world.CreateAspect(RequiredTypes.From<int, string>(), "");
            IArray<string> strings = world.GetAspectComponents<string>(aspectId);
            IArray<Handle> entities = world.GetAspectEntities(aspectId);

            for (int c = 0; c < 10; ++c)
            {
                for (int i = 0; i < count; ++i)
                {
                    Handle entity = world.CreateEntity();
                    world.AddComponent(entity, 5);
                    world.AddComponent(entity, "");
                    if (i % 5 == 0) world.DestroyEntity(entity);
                }

                //foreach (Handle entity in entities) Console.WriteLine(entity);
                //Console.WriteLine();

                foreach (Handle entity in entities) world.DestroyEntity(entity);

                //foreach (Handle entity in entities) Console.WriteLine(entity);
                //Console.WriteLine();

            }
        }
    }
}
