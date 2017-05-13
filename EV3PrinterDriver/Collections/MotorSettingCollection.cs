using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace EV3PrinterDriver.Collections
{
    class MotorSettingCollection<T> : IEnumerable<T>
    {
        readonly T[] _values;
        public MotorSettingCollection(int size)
        {
            _values = new T[size];
        }

        public T this[MotorPort port] 
        {
            get
            {
                return _values[(int)port];
            }
            set
            {
                _values[(int)port] = value;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _values.Length;i++)
                _values[i] = default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // see: http://stackoverflow.com/questions/1272673/obtain-generic-enumerator-from-an-array
            return ((IEnumerable<T>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }
}
