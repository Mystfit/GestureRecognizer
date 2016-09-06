using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureRecognizerDotNet
{
    public class AccelerationDataStore
    {
        private List<AccelerationData> _accelerationData;

        public AccelerationDataStore()
        {
            _accelerationData = new List<AccelerationData>();
        }

        public void add(AccelerationData accelerationData)
        {
            if (_accelerationData.Count == Config.MAX_ACCELERATION_ITEMS)
            {
                _accelerationData.RemoveAt(0);
            }

            _accelerationData.Add(accelerationData);
        }

        public uint totalAccelerationDataItems { get { return (uint)_accelerationData.Count; } }
        public List<AccelerationData> accelerationData { get { return _accelerationData; } }
    }
}
