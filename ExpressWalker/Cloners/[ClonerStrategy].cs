﻿using ExpressWalker.Helpers;
using System;
using System.Collections.Generic;

namespace ExpressWalker.Cloners
{
    internal abstract class ClonerStrategy
    {
        public abstract int Priority { get; }

        public abstract bool IsMatch(Type elementType);

        public abstract ClonerBase GetCloner(Type elementType);

        protected object Create(Type genericTypeDef, params Type[] genericTypeArgs)
        {
            var concreteType = genericTypeDef.MakeGenericType(genericTypeArgs);
            return Activator.CreateInstance(concreteType);
        }
    }

    /// <summary>
    /// Makes cloner for collection type that has constructor accepting single parameter of type IEnumerable<T> (like List<T>).
    /// </summary>
    internal class ListStrategy : ClonerStrategy
    {
        public override int Priority { get { return 40; } }

        public override bool IsMatch(Type elementType)
        {
            if (Util.IsSimpleType(elementType) || !Util.ImplementsIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var enumParamType = typeof(IEnumerable<>).MakeGenericType(itemsType);
            return Util.HasCollectionCtor(elementType, enumParamType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetItemsType(elementType);
            return (ClonerBase)Create(typeof(ListCloner<,>), elementType, itemsType);
        }
    }

    /// <summary>
    ///Makes cloner for collection type that has constructor accepting single parameter of type IList<T> (like Collection<T>).
    /// </summary>
    internal class CollectionStrategy : ClonerStrategy
    {
        public override int Priority { get { return 30; } }

        public override bool IsMatch(Type elementType)
        {

            if (Util.IsSimpleType(elementType) || !Util.ImplementsIEnumerable(elementType))
            {
                return false;
            }

            var itemsType = Util.GetItemsType(elementType);
            if (itemsType == null)
            {
                return false;
            }

            var listParamType = typeof(IList<>).MakeGenericType(itemsType);
            return Util.HasCollectionCtor(elementType, listParamType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetItemsType(elementType);
            return (ClonerBase)Create(typeof(CollectionClonner<,>), elementType, itemsType); 
        }
    }

    /// <summary>
    /// Makes cloner for explicit IEnumerable<T> types or concrete array types (like T[]).
    /// </summary>
    internal class ArrayStrategy : ClonerStrategy
    {
        public override int Priority { get { return 20; } }

        public override bool IsMatch(Type elementType)
        {
            return !Util.IsSimpleType(elementType) && (Util.IsIEnumerable(elementType) || elementType.IsArray);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            var itemsType = Util.GetItemsType(elementType);
            return (ClonerBase)Create(typeof(ArrayClonner<,>), elementType, itemsType);
        }
    }

    /// <summary>
    /// Makes cloner for non-collection types.
    /// </summary>
    internal class InstanceStrategy : ClonerStrategy
    {
        public override int Priority { get { return 10; } }

        public override bool IsMatch(Type elementType)
        {
            return !Util.IsSimpleType(elementType) && Util.HasParameterlessCtor(elementType);
        }

        public override ClonerBase GetCloner(Type elementType)
        {
            return (ClonerBase)Create(typeof(InstanceCloner<>), elementType);
        }
    }
}
