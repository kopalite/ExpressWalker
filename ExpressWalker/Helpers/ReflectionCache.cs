using ExpressWalker.Visitors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressWalker.Helpers
{
    internal class ReflectionCache
    {
        private static Dictionary<string, TypeData> TypeData = new Dictionary<string, TypeData>();

        private static Dictionary<string, TypeInfo> TypeInfo = new Dictionary<string, TypeInfo>();

        public static TypeData GetData(Type type)
        {
            TypeData data = null;

            if (!TypeData.TryGetValue(type.FullName, out data))
            {
                data = new TypeData(type);

                TypeData.Add(type.FullName, data);
            }

            return data;
        }

        public static TypeInfo GetInfo(Type type)
        {
            TypeInfo info = null;

            if (!TypeInfo.TryGetValue(type.FullName, out info))
            {
                info = new TypeInfo(type);

                TypeInfo.Add(type.FullName, info);
            }

            return info;
        }

        public void Reset()
        {
            TypeData.Clear();
        }
    }

    internal class TypeData
    {
        public TypeInfo TypeInfo { get; private set; }

        public Dictionary<string, PropertyData> Properties { get; private set; }

        public TypeData(Type type)
        {
            TypeInfo = ReflectionCache.GetInfo(type);

            Properties = new Dictionary<string, PropertyData>();

            foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Properties.Add(info.Name, new PropertyData(info.Name, TypeInfo, info.PropertyType, info.GetCustomAttribute<VisitorHierarchyAttribute>() != null));
            }
        }
    }

    internal class PropertyData
    {
        public string PropertyName { get; private set; }

        public TypeInfo DeclaringType { get; private set; }

        public TypeInfo PropertyType { get; private set; }

        public bool IsVisitorHierarchy { get; private set; }

        public PropertyData(string propertyName, TypeInfo declaringType, Type propertyType, bool isVisitorHierarchy)
        {
            PropertyName = propertyName;

            DeclaringType = declaringType;

            PropertyType =  ReflectionCache.GetInfo(propertyType);

            IsVisitorHierarchy = isVisitorHierarchy;
        }
    }

    internal class TypeInfo
    {
        public Type RawType { get; private set; }

        public string Name { get; private set; }

        public bool IsSimpleType { get; private set; }

        public bool IsDictionary { get; private set; }

        public bool IsGenericEnumerable { get; private set; }

        public bool ImplementsGenericIEnumerable { get; private set; }

        public Type CollectionItemsType { get; private set; }

        public bool IsCollectionItemsTypeSimple { get; private set; }

        public TypeInfo(Type type)
        {
            RawType = type;

            Name = type.FullName;

            IsSimpleType = Util.IsSimpleType(type);

            IsDictionary = Util.IsDictionary(type);

            IsGenericEnumerable = Util.IsGenericEnumerable(type);

            ImplementsGenericIEnumerable = Util.ImplementsGenericIEnumerable(type);

            CollectionItemsType = Util.GetItemsType(type);

            if (CollectionItemsType != null)
            {
                IsCollectionItemsTypeSimple = Util.IsSimpleType(CollectionItemsType);
            }
        }
    }
}

    
