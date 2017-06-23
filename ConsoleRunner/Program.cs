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
            //SparseSetAccesSpeedTest();
            TestArraySystemSpeed();
            Console.WriteLine("---END---");
            Console.ReadLine();
        }

        //private static void SparseSetAccesSpeedTest()
        //{
        //    int count = 10000000;

        //    HashSet<Entity> ehs = new HashSet<Entity>();
        //    EntitySet es = new EntitySet();
        //    es.Grow(count);

        //    for (int i = 0; i < count; ++i) ehs.Add(new Entity(null, i, 0));
        //    for (int i = 0; i < count; ++i) es.Add(new Entity(null, i, 0));
        //    ehs.Clear();
        //    es.Clear();

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < count; ++i) ehs.Add(new Entity(null, i, 0));
        //    sw.Stop();
        //    Console.WriteLine("HashSet fill: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < count; ++i) es.Add(new Entity(null, i, 0));
        //    sw.Stop();
        //    Console.WriteLine("EntitySet fill: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    foreach (Entity e in ehs)
        //    {
        //        int id = e.Id;
        //    }
        //    sw.Stop();
        //    Console.WriteLine("HashSet foreach: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    foreach (Entity e in es)
        //    {
        //        int id = e.Id;
        //    }
        //    sw.Stop();
        //    Console.WriteLine("EntitySet foreach: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    for(int i=0; i<es.Count; ++i)
        //    {
        //        int id = es[i].Id;
        //    }
        //    sw.Stop();
        //    Console.WriteLine("EntitySet for: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < count; ++i) ehs.Remove(new Entity(null, i, 0));
        //    sw.Stop();
        //    Console.WriteLine("HashSet remove: " + sw.ElapsedMilliseconds);

        //    sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < count; ++i) es.Remove(new Entity(null, i, 0));
        //    sw.Stop();
        //    Console.WriteLine("EntitySet remove: " + sw.ElapsedMilliseconds);
        //}

        private static void TestArraySystemSpeed()
        {
            int count = 1000000;

            IEntitySystemManager em = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit64, count, 0);
            string strComp = "I'm a string";

            em.AddSystem(new ArrayAccessSystem());
            em.AddSystem(new ArrayDirectAccessSystem());
            em.AddSystem(new ArrayDirectRawAccessSystem());
            //em.AddSystem(new CompAccessSystem());

            for (int i = 0; i < count; ++i)
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

            public IEntitySystemManager EntityManager { get; set; }
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

            public override void Update()
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

        class ArrayDirectRawAccessSystem : BaseSystem
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
                string[] sr = strings.Raw;
                int[] ir = ints.Raw;
                int c = Entities.Count;
                for (int i = 0; i < c; ++i)
                {
                    int id = Entities[i].Id;
                    string s = sr[id];
                    int ii = ir[id];
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
            IEntitySystemManager em = EntityManagerFactory.CreateEntitySystemManager(BitmaskSize.Bit64, 256, 256);
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

                em.Update(0);

                IArray<int> intz = em.GetComponents<int>();

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
