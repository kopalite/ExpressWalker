using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ExpressWalker.Visitors;
using System.Reflection;

namespace ExpressWalker.Factories
{
    public sealed class VisitorsFactory : IVisitorsFactory
    {
        private readonly List<WalkerSettings> _settings;

        private readonly Dictionary<VisitorKey, IVisitor> _visitors;

        private string _category;

        private bool _isLocked;

        public VisitorsFactory()
        {
            _settings = new List<WalkerSettings>();

            _visitors = new Dictionary<VisitorKey, IVisitor>();
        }

        public IVisitorsFactory Category(string category)
        {
            _category = category;

            return this;
        }

        public IVisitorsFactory ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            if (_isLocked)
            {
                throw new Exception("Factory can only be set before calling GetVisitor() method!");
            }

            var category = _category ?? "default";

            if (!_settings.Any(x => x.Category == category))
            {
                _settings.Add(new WalkerSettings(category));
            }

            var settings = _settings.First(x => x.Category == category);

            settings.ForProperty(getNewValue);

            return this;
        }

        public IVisitorsFactory ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName, Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            if (_isLocked)
            {
                throw new Exception("Factory can only be set before calling GetVisitor() method!");
            }

            var category = _category ?? "default";

            if (!_settings.Any(x => x.Category == category))
            {
                _settings.Add(new WalkerSettings(category));
            }

            var settings = _settings.First(x => x.Category == category);

            settings.ForProperty(propertyName, getNewValue);

            return this;
        }

        public IVisitor GetVisitor(string category, Type type)
        {
            _isLocked = true;

            var visitorKey = new VisitorKey(category, type);
            if (_visitors.ContainsKey(visitorKey))
            {
                return _visitors[visitorKey];
            }

            var settings = _settings.FirstOrDefault(x => x.Category == category);
            if (settings == null)
            {
                throw new Exception("Visitors category '{0}' is not being set in visitors factory. It can be set by using one of .ForProperty() methods before asking for visitors.");
            }

            var visitor = settings.GetVisitor(type);
            _visitors.Add(visitorKey, visitor);
            return visitor;
        }
    }

    internal struct VisitorKey
    {
        private readonly string _category;

        private readonly Type _type;

        public VisitorKey(string category, Type type)
        {
            _category = category;

            _type = type;
        }
    }

    internal sealed class WalkerSettings
    {
        private List<Action<dynamic>> _walkerActions;

        public string Category { get; private set; }

        public WalkerSettings(string category)
        {
            _walkerActions = new List<Action<dynamic>>();

            Category = category;
        }

        public void ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _walkerActions.Add(x => x.ForProperty(getNewValue));
        }

        public void ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName, Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _walkerActions.Add(x => x.ForProperty(propertyName, getNewValue));
        }

        public IVisitor GetVisitor(Type type)
        {
            var getVisitor = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                      .FirstOrDefault(x => x.IsGenericMethod && x.Name.StartsWith("GetVisitor"))
                                      .MakeGenericMethod(type);

            return getVisitor.Invoke(this, null) as IVisitor; 
        }

        private IVisitor GetVisitor<TRootType>()
        {
            var walker = TypeWalker<TRootType>.Create();
            _walkerActions.ForEach(x => x(walker));
            return walker.Build();
        }
    }
}
