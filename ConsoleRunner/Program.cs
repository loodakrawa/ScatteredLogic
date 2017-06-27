using ScatteredLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            RunLoop();
            TestArraySystemSpeed();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        private static void TestArraySystemSpeed()
        {
            int count = 1000000;

            IEntitySystemManager em = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit64, count);
            string strComp = "I'm a string";

            ArrayAccessSystem s1 = new ArrayAccessSystem();
            CompAccessSystem s2 = new CompAccessSystem();

            em.AddSystem(s1);
            em.AddSystem(s2);
            //em.AddSystem(new CompAccessSystem());

            for (int i = 0; i < count; ++i)
            {
                Handle e = em.CreateEntity();
                em.AddComponent(e, i);
                em.AddComponent(e, strComp);
            }

            em.Update();

            for (int i = 0; i < 10; ++i) s1.Update();
            for (int i = 0; i < 10; ++i) s2.Update();

        }

        abstract class BaseSystem : ISystem
        {
            public abstract IEnumerable<Type> RequiredComponents { get; }

            public IEntitySystemManager EntityWorld { get; set; }
            public IHandleSet Entities { get; set; }
            public ISystemInfo Info { get; set; }

            public virtual void Added() {}
            public virtual void EntityAdded(Handle entity) { }
            public virtual void EntityRemoved(Handle entity) { }
            public virtual void Removed() { }

            public virtual void Update()
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                DoWork();

                sw.Stop();

                Console.WriteLine(GetType().Name + " : " + sw.ElapsedMilliseconds);
            }

            public abstract void DoWork();
        }

        class ArrayAccessSystem : BaseSystem
        {
            public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<string, int>();

            private IArray<string> strings;
            private IArray<int> ints;

            public override void Added()
            {
                strings = EntityWorld.GetComponents<string>();
                ints = EntityWorld.GetComponents<int>();
            }
            
            public override void DoWork()
            {
                foreach(Handle e in Entities)
                {
                    string s = strings[e.Index];
                    int i = ints[e.Index];
                }
            }
        }

        class CompAccessSystem : BaseSystem
        {
            public override IEnumerable<Type> RequiredComponents => RequiredTypes.From<string, int>();

            public override void DoWork()
            {
                foreach (Handle e in Entities)
                {
                    string s = EntityWorld.GetComponent<string>(e);
                    int i = EntityWorld.GetComponent<int>(e);
                }
            }
        }


        static void RunLoop()
        {
            int count = 1024;

            IEntityWorld em = EntityManagerFactory.CreateEntityManager(BitmaskSize.Bit64, count);
            string strComp = "I'm a string";

            List<Handle> entities = new List<Handle>();
            while (true)
            {
                for (int i = 0; i < count; ++i)
                {
                    Handle e = em.CreateEntity();
                    entities.Add(e);
                    em.AddComponent(e, i);
                    em.AddComponent(e, strComp);
                }

                IArray<int> intz = em.GetComponents<int>();

                foreach (Handle en in entities)
                {
                    int n = em.GetComponent<int>(en);
                    string str = em.GetComponent<string>(en);
                    em.DestroyEntity(en);
                }

                entities.Clear();
            }
        }
    }
}
