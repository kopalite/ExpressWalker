using Mapster;

namespace ExpressWalker
{
    public class ProxyWalker<TRootType>
    {
        private IElementVisitor<TRootType> _visitor;

        private int _depth;

        public ProxyWalker(int depth) : this(null, depth)
        {

        }

        public ProxyWalker(IElementVisitor<TRootType> visitor, int depth)
        {
            _visitor = visitor;

            _depth = depth;
        }
        

        public TRootType Unproxy(TRootType @object)
        {
            if (_visitor != null)
            {
                _visitor.Visit(@object);
            }

            var clone = Clone(@object);

            return clone;
        }

        private TRootType Clone(TRootType @object)
        {
            TypeAdapterConfig.GlobalSettings.Default.Settings.PreserveReference = true;
            return TypeAdapter.Adapt<TRootType>(@object);
        }
    }
}
