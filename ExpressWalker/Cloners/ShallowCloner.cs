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
            if (IsCloneableEnumCollection(elementType))
            {
                return GetEnumerableCloner(elementType);
            }
            if (IsCloneableIListCollection(elementType))
            {
                return GetIListCloner(elementType);
            }
            else if (IsCloneableSequence(elementType))
            {
                return GetSequenceCloner(elementType);
            }
            else if (IsCloneableNonSequence(elementType))
            {
                return GetInstanceCloner(elementType);
            }

            throw new Exception(string.Format("Cannot make shallow clone for type '{0}'.", elementType.Name));
        }

        #region [ Collection clonning ]

        private static bool IsCloneableEnumCollection(Type type)
        {
            var itemsType = Util.GetItemsType(type);
            if (itemsType == null)
            {
                return false;
            }

            var enumeParamType = typeof(IEnumerable<>).MakeGenericType(itemsType);
            
            return Util.ImplementsIEnumerable(type) && Util.HasCollectionCtor(type, enumeParamType);
        }

        private static ShallowCloner GetEnumerableCloner(Type elementType)
        {
            var typeDefinition = typeof(CollectionEnumCloner<,>);
            var itemsType = Util.GetItemsType(elementType);
            var concreteType = typeDefinition.MakeGenericType(elementType, itemsType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        private static bool IsCloneableIListCollection(Type type)
        {
            var itemsType = Util.GetItemsType(type);
            if (itemsType == null)
            {
                return false;
            }
            
            var listParamType = typeof(IList<>).MakeGenericType(itemsType);

            return Util.ImplementsIEnumerable(type) && Util.HasCollectionCtor(type, listParamType);
        }

        private static ShallowCloner GetIListCloner(Type elementType)
        {
            var typeDefinition = typeof(CollectionIListCloner<,>);
            var itemsType = Util.GetItemsType(elementType);
            var concreteType = typeDefinition.MakeGenericType(elementType, itemsType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        #endregion

        #region [ Sequence clonning ]

        private static bool IsCloneableSequence(Type type)
        {
            return Util.IsIEnumerable(type);
        }

        private static ShallowCloner GetSequenceCloner(Type elementType)
        {
            var typeDefinition = typeof(SequenceCloner<>);
            var concreteType = typeDefinition.MakeGenericType(elementType);
            return (ShallowCloner)Activator.CreateInstance(concreteType);
        }

        #endregion

        #region [ Non-Collection clonning ]

        private static bool IsCloneableNonSequence(Type type)
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
