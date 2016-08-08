using ExpressWalker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressWalker.Cloners
{
    internal abstract class ShallowCloner
    {
        public abstract object Clone(object element);

        public static ShallowCloner Create(Type elementType)
        {
            if (IsCloneableList(elementType))
            {
                return GetListCloner(elementType);
            }
            if (IsCloneableCollection(elementType))
            {
                return GetCollectionCloner(elementType);
            }
            else if (ISCloneableArray(elementType))
            {
                return GetArrayCloner(elementType);
            }
            else if (IsCloneableInstance(elementType))
            {
                return GetInstanceCloner(elementType);
            }

            throw new Exception(string.Format("Cannot make shallow clone for type '{0}'.", elementType.Name));
        }

        #region [ Collection clonning ]

        private static bool IsCloneableList(Type type)
        {
            var itemsType = Util.GetItemsType(type);
            if (itemsType == null)
            {
                return false;
            }

            var enumeParamType = typeof(IEnumerable<>).MakeGenericType(itemsType);
            
            return Util.ImplementsIEnumerable(type) && Util.HasCollectionCtor(type, enumeParamType);
        }

        private static ShallowCloner GetListCloner(Type elementType)
        {
            var typeDefinition = typeof(ListCloner<,>);
            var itemsType = Util.GetItemsType(elementType);
            var concreteType = typeDefinition.MakeGenericType(elementType, itemsType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        private static bool IsCloneableCollection(Type type)
        {
            var itemsType = Util.GetItemsType(type);
            if (itemsType == null)
            {
                return false;
            }
            
            var listParamType = typeof(IList<>).MakeGenericType(itemsType);

            return Util.ImplementsIEnumerable(type) && Util.HasCollectionCtor(type, listParamType);
        }

        private static ShallowCloner GetCollectionCloner(Type elementType)
        {
            var typeDefinition = typeof(CollectionClonner<,>);
            var itemsType = Util.GetItemsType(elementType);
            var concreteType = typeDefinition.MakeGenericType(elementType, itemsType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        #endregion

        #region [ Sequence clonning ]

        private static bool ISCloneableArray(Type type)
        {
            return Util.IsIEnumerable(type);
        }

        private static ShallowCloner GetArrayCloner(Type elementType)
        {
            var typeDefinition = typeof(ArrayClonner<>);
            var concreteType = typeDefinition.MakeGenericType(elementType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        #endregion

        #region [ Non-Collection clonning ]

        private static bool IsCloneableInstance(Type type)
        {
            return Util.HasParameterlessCtor(type); 
        }

        private static ShallowCloner GetInstanceCloner(Type elementType)
        {
            var typeDefinition = typeof(InstanceCloner<>);
            var concreteType = typeDefinition.MakeGenericType(elementType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        #endregion
    }

    
}
