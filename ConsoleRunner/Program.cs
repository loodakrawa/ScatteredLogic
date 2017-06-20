using ScatteredLogic;
using System.Collections.Generic;
using System;
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
            IEntityManager em = EntityManagerFactory.Create(BitmaskSize.Bit64, 1000000, 0);
            string strComp = "I'm a string";

            em.AddSystem(new ArrayAccessSystem());
            em.AddSystem(new CompAccessSystem());

            for (int i = 0; i < 1000000; ++i)
            {
                Entity e = em.CreateEntity();
                e.AddComponent(i);
                e.AddComponent(strComp);
            }

            for(int i=0; i<4; ++i)
            {
                em.Update(0);
            }
        }

        abstract class BaseSystem : ISystem
        {
            public abstract IEnumerable<Type> RequiredComponents { get; }

            public IEntityManager EntityManager { get; set; }
            public IEntitySet Entities { get; set; }
            public EventBus EventBus { get; set; }

            public virtual void Added() {}
            public virtual void EntityAdded(Entity entity) { }
            public virtual void EntityRemoved(Entity entity) { }
            public virtual void Removed() { }

            public virtual void Update(float deltaTime)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                Update();

                sw.Stop();

                Console.WriteLine(GetType().Name + " : " + sw.ElapsedMilliseconds);
            }

            public abstract void Update();
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
            
            public override void Update()
            {
                foreach(Entity e in Entities)
                {
                    string s = strings[e.Id];
                    int i = ints[e.Id];
                }
            }
        }

        class CompAccessSystem : BaseSystem
        {
            public override IEnumerable<Type> RequiredComponents => Types.From<string, int>();

            public override void Update()
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
            IEntityManager em = EntityManagerFactory.Create(BitmaskSize.Bit64, 256, 256);
            string strComp = "I'm a string";

            int intTypeId = em.GetTypeId<int>();

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

                em.Update(0);

                IArray<int> intz = em.GetComponents<int>(intTypeId);

                foreach (Entity en in entities)
                {
                    int n = en.GetComponent<int>();
                    string str = en.GetComponent<string>();
                    em.DestroyEntity(en);
                }

                em.Update(0);

                entities.Clear();
            }
        }
    }
}
