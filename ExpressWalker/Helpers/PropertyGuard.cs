using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressWalker
{
    public class PropertyGuard
    {
        private List<int> _path;

        public PropertyGuard()
        {
            _path = new List<int>();
        }

        private PropertyGuard(IEnumerable<int> path)
        {
            _path = new List<int>(path);
        }

        public void Next(PropertyInfo property)
        {
            if (property == null)
            {
                return;
            }

            var hash = GetHash(property.DeclaringType.FullName, property.Name);

            if (!IsRepeating(hash))
            {
                _path.Add(hash);
            }
        }

        private bool IsRepeating(int hash)
        {
            //var matches = _path.Where(h => h == hash).Count();

            //if (_path.Count == 0)
            //{
            //    return !_path.Any(h => h == hash);
            //}

            //var currentIndex = 1;

            //var matchingIndex = 0;

            //while (currentIndex < _path.Count / 2 && matchingIndex == 0)
            //{
            //    currentIndex++;
            //}

            return false;
        }

        public PropertyGuard Copy()
        {
            return new PropertyGuard(_path);
        }

        public int GetHash(string declaringType, string propertyName)
        {
            return string.Format("{0}|{1}", declaringType, propertyName).GetHashCode();
        }
    }
}
