using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace ColbyDoan
{
    public class ObjectFactory<T> : IFactory where T : class, IFactoryProduct
    {
        private Dictionary<string, Type> itemByName;
        private List<string> itemNames;
        bool IsInitialized => itemByName != null;

        public void InitializeFactory()
        {
            if (IsInitialized) return;
            var itemTypes = Assembly.GetAssembly(typeof(T)).GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(T)));

            itemByName = new Dictionary<string, Type>();
            itemNames = new List<string>();

            foreach (var type in itemTypes)
                AddType(type);
        }

        void AddType(Type type)
        {
            var tempItem = Activator.CreateInstance(type) as T;
            itemByName.Add(tempItem.Name, type);
            itemNames.Add(tempItem.Name);
        }

        public T GetItem(string name)
        {
            InitializeFactory();
            if (itemByName.ContainsKey(name))
            {
                Type type = itemByName[name];
                var item = Activator.CreateInstance(type) as T;
                return item;
            }
            return null;
        }
        public IFactoryProduct GetProduct(string name) => GetItem(name);

        public List<string> GetItemNames()
        {
            InitializeFactory();
            return itemNames;
        }

        //public IEnumerable<string> GetItemNames()
        //{
        //    InitializeFactory();
        //    return itemByName.Keys;
        //}

    }
    public interface IFactory
    {
        List<string> GetItemNames();
        IFactoryProduct GetProduct(string name);
    }
    public interface IFactoryProduct
    {
        string Name { get; }
    }
}