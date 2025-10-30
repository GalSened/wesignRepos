namespace PdfHandler
{
    using Common.Interfaces.PDF;
    using PdfHandler.pdf;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ExtendedList<T> : IExtendedList<T>
    {
        private readonly IList<T> _list = new List<T>();
        private readonly Pdf _pdf;

        public T this[int index] { get { return _list[index]; } set { _list[index] = value; } }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public ExtendedList(Pdf pdf)
        {
            _pdf = pdf;
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
            //_list.Add(item);
        }

        internal void InternalAdd(T item)
        {
            _list.Add(item);
        }

        public void AddRange(IList<T> items)
        {
            _pdf.AddRange(items);
            foreach (var item in items)
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            _list.Clear();
            _pdf.Clear<T>();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
            //return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
            //_list.CopyTo(array, arrayIndex);
        }

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return base.GetEnumerator();
        //    //return _list.GetEnumerator();
        //}

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
            //return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
            //_list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
            //return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
            //_list.RemoveAt(index);
        }

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        public void Update(IList<T> items)
        {
            Clear();
            AddRange(items);
        }


        public void UpdateValue(IList<T> items)
        {
            _pdf.SetFieldsValues(items);            
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        
    }
}
