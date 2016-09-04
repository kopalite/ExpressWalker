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
            if (_path.Count < 1)
            {
                return _path.All(h => h != hash);
            }

            var reverseIndex = 1;

            while (reverseIndex < _path.Count / 2)
            {
                var currentIndex = _path.Count - reverseIndex;

                var doubleIndex = currentIndex * 2;

                if (hash == _path[currentIndex] && _path[currentIndex] == _path[doubleIndex])
                {
                    //[cnt-1, cnt-curr+1] == [cnt-curr-1, cnt-curr*2+1]
                      
                    //TODO: compare sequences. If are equal, exit and return true;;
                }

                reverseIndex++;
            }

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
