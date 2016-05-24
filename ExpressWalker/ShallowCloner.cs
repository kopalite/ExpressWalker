using System.Reflection;

namespace ExpressWalker
{
    internal sealed class ShallowCloner<TElement>
    {
        private MethodInfo _cloneMethod;

        public ShallowCloner()
        {
            _cloneMethod = typeof(TElement).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public TElement Clone(TElement element)
        {
            if (element.Equals(default(TElement)))
            {
                return default(TElement);
            } 
            
            return (TElement)_cloneMethod.Invoke(element, null);
        }
    }
}
