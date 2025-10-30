using System.Collections.Generic;

namespace Common.Interfaces.PDF
{
    public interface IExtendedList<T> :IList<T>
    {
        void Update(IList<T> items);
        void Clear();
        void AddRange(IList<T> items);
        void UpdateValue(IList<T> items);

    }
}
