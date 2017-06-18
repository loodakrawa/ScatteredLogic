using ScatteredLogic;
using System.Collections.Generic;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            IEntityManager em = EntityManagerFactory.Create(BitmaskSize.Bit64, 10000, 0);
            string strComp = "I'm a string";


            List<Entity> entities = new List<Entity>();
            while(true)
            {
                for(int i=0; i<10000; ++i)
                {
                    Entity e = em.CreateEntity();
                    entities.Add(e);
                    e.AddComponent(i);
                    e.AddComponent(strComp);
                }

                em.Update(0);

                foreach(Entity en in entities)
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
