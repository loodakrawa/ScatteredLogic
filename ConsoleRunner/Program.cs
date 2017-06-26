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
            ArrayDirectAccessSystem s2 = new ArrayDirectAccessSystem();

            em.AddSystem(s1);
            em.AddSystem(s2);
            //em.AddSystem(new CompAccessSystem());

            for (int i = 0; i < count; ++i)
            {
                Entity e = em.CreateEntity();
                e.AddComponent(i);
                e.AddComponent(strComp);
            }

            em.Update();

            for (int i = 0; i < 10; ++i) s1.Update();
            for (int i = 0; i < 10; ++i) s2.Update();

        }

        abstract class BaseSystem : ISystem
        {
            public abstract IEnumerable<Type> RequiredComponents { get; }

            public IEntitySystemManager EntityManager { get; set; }
            public IEntitySet Entities { get; set; }
            public int Index { get; set; }

            public virtual void Added() {}
            public virtual void EntityAdded(Entity entity) { }
            public virtual void EntityRemoved(Entity entity) { }
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
            public override IEnumerable<Type> RequiredComponents => Types.From<string, int>();

            private IArray<string> strings;
            private IArray<int> ints;

            public override void Added()
            {
                strings = EntityManager.GetComponents<string>();
                ints = EntityManager.GetComponents<int>();
            }
            
            public override void DoWork()
            {
                foreach(Entity e in Entities)
                {
                    string s = strings[e.Id];
                    int i = ints[e.Id];
                }
            }
        }

        class ArrayDirectAccessSystem : BaseSystem
        {
            public override IEnumerable<Type> RequiredComponents => Types.From<string, int>();

            private IArray<string> strings;
            private IArray<int> ints;

            public override void Added()
            {
                strings = EntityManager.GetComponents<string>();
                ints = EntityManager.GetComponents<int>();
            }

            public override void DoWork()
            {
                int c = Entities.Count;
                for (int i=0; i<c; ++i)
                {
                    int id = Entities[i].Id;
                    string s = strings[id];
                    int ii = ints[id];
                }
            }
        }

        class CompAccessSystem : BaseSystem
        {
            public override IEnumerable<Type> RequiredComponents => Types.From<string, int>();

            public override void DoWork()
            {
                foreach (Entity e in Entities)
                {
                    string s = e.GetComponent<string>();
                    int i = e.GetComponent<int>();
                }
            }
        }


        static void RunLoop()
        {
            IEntitySystemManager em = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit64, 10000);
            string strComp = "I'm a string";

            List<Entity> entities = new List<Entity>();
            while (true)
            {
                for (int i = 0; i < 10000; ++i)
                {
                    Entity e = em.CreateEntity();
                    entities.Add(e);
                    e.AddComponent(i);
                    e.AddComponent(strComp);
                }

                em.Update();

                IArray<int> intz = em.GetComponents<int>();

                foreach (Entity en in entities)
                {
                    int n = en.GetComponent<int>();
                    string str = en.GetComponent<string>();
                    em.DestroyEntity(en);
                }

                em.Update();

                entities.Clear();
            }
        }
    }
}
