using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureRecognizerDotNet
{
    public class Gesture
    {
        private int _id;
        public int id { get { return _id; } set { _id = value; } }

        private AccelerationDataStore _accelerationDataStore;
        private AccelerationDataStore accelerationDataStore { get { return _accelerationDataStore; } }
        private DataQuantizer _dataQuantizer;

        public Gesture()
        {
            id = 0;
            _accelerationDataStore = new AccelerationDataStore();
            if (Config.USE_DATA_QUANTIZER)
                _dataQuantizer = new DataQuantizer();
        }



        public void load()
        {
            load(string.Format(Path.Combine(Config.GESTURE_PATH, "{0}.ges"), id));
        }

        public void load(string filename)
        {
            string[] lines = System.IO.File.ReadAllLines(filename);
            foreach(String line in lines)
            {
                AccelerationData accelerationData = new AccelerationData();
                int[] values = line.Split(',').Select(int.Parse).ToArray();
                accelerationData.setAxisValue(AccelerationData.Axis.X, values[0]);
                accelerationData.setAxisValue(AccelerationData.Axis.Y, values[1]);
                accelerationData.setAxisValue(AccelerationData.Axis.Z, values[2]);
            }
        }

        public void save(string name)
        {
            string filename = Path.Combine(Config.GESTURE_PATH, string.Format("{0}.ges", name));
            string[] lines = new string[accelerationDataStore.accelerationData.Count];
            for(int i = 0; i < accelerationDataStore.accelerationData.Count; i++)
            {
                AccelerationData d = accelerationDataStore.accelerationData[i];
                string[] line = new string[3];
                for(int j = 0; j < 3; j++)
                {
                    line[j] = d.getAxisValue((AccelerationData.Axis)j).ToString();
                }

                lines[i] = string.Join(",", line);
            }
            File.WriteAllLines(filename, lines);
        }

        public void import()
        {

        }

        public bool isValid { get { return _accelerationDataStore.totalAccelerationDataItems > 0; } }

        public long calculateDistanceBetween(Gesture other)
        {
            if (!isValid && other.isValid)
                return int.MaxValue;

            AccelerationDataStore otherAccelerationDataStore = other.accelerationDataStore;
            AccelerationData otherAccelerationData = otherAccelerationDataStore.accelerationData[0];
            uint otherTotalAccelerationDataItems = otherAccelerationDataStore.totalAccelerationDataItems;
            long[] table = buildTable(otherTotalAccelerationDataItems);

            uint totalAccelerationDataItems = accelerationDataStore.totalAccelerationDataItems;
            long distance = calculateDTWDistance(
                otherAccelerationDataStore,
                otherTotalAccelerationDataItems,
                (int)totalAccelerationDataItems - 1,
                (int)otherTotalAccelerationDataItems - 1,
                table);

            distance /= (totalAccelerationDataItems + otherTotalAccelerationDataItems);

            return distance;
        }

        private long[] buildTable(uint itemsInOtherGestureToCompare)
        {
            uint totalAccelerationDataItems = accelerationDataStore.totalAccelerationDataItems;
            uint tableSize = totalAccelerationDataItems * itemsInOtherGestureToCompare;

            long[] table = new long[tableSize];

            for (int i = 0; i < tableSize; i++)
                table[i] = -1L;
            return table;
        }

        private long calculateDTWDistance(AccelerationDataStore otherAccelerationDataStore, uint otherTotalAccelerationDataItems, int compareIndex, int otherCompareIndex, long[] table)
        {
            if (compareIndex < 0 || otherCompareIndex < 0)
                return int.MaxValue;

            uint tableWidth = otherTotalAccelerationDataItems;
            long localDistance = 0;

            for(uint axis = 0; axis < 3; axis++)
            {
                int a = accelerationDataStore.accelerationData[compareIndex].getAxisValue((AccelerationData.Axis)axis);
                int b = otherAccelerationDataStore.accelerationData[compareIndex].getAxisValue((AccelerationData.Axis)axis);
                localDistance += ((a - b) * (a - b));
            }

            long sdistance = 0;
            long s1, s2, s3;

            if(compareIndex == 0 && otherCompareIndex == 0)
            {
                if( table[compareIndex * tableWidth + otherCompareIndex] < 0)
                {
                    table[compareIndex * tableWidth + otherCompareIndex] = localDistance;
                }

                return localDistance;
            }

            if (compareIndex == 0)
            {
                if (table[compareIndex * tableWidth + (otherCompareIndex - 1)] < 0)
                    sdistance = calculateDTWDistance(otherAccelerationDataStore, otherTotalAccelerationDataItems, compareIndex, otherCompareIndex - 1, table);
                else
                    sdistance = table[compareIndex * tableWidth + otherCompareIndex - 1];
            }
            else if (otherCompareIndex == 0)
            {
                if (table[(compareIndex - 1) * tableWidth + otherCompareIndex] < 0)
                    sdistance = calculateDTWDistance(otherAccelerationDataStore, otherTotalAccelerationDataItems, compareIndex - 1, otherCompareIndex, table);
                else
                    sdistance = table[(compareIndex - 1) * tableWidth + otherCompareIndex];
            }
            else
            {

                if (table[compareIndex * tableWidth + (otherCompareIndex - 1)] < 0)
                    s1 = calculateDTWDistance(otherAccelerationDataStore, otherTotalAccelerationDataItems, compareIndex, otherCompareIndex - 1, table);
                else
                    s1 = table[compareIndex * tableWidth + (otherCompareIndex - 1)];

                if (table[(compareIndex - 1) * tableWidth + otherCompareIndex] < 0)
                    s2 = calculateDTWDistance(otherAccelerationDataStore, otherTotalAccelerationDataItems, compareIndex - 1, otherCompareIndex, table);
                else
                    s2 = table[(compareIndex - 1) * tableWidth + otherCompareIndex];

                if (table[(compareIndex - 1) * tableWidth + otherCompareIndex - 1] < 0)
                    s3 = calculateDTWDistance(otherAccelerationDataStore, otherTotalAccelerationDataItems, compareIndex - 1, otherCompareIndex - 1, table);
                else
                    s3 = table[(compareIndex - 1) * tableWidth + otherCompareIndex - 1];

                sdistance = s1 < s2 ? s1 : s2;
                sdistance = sdistance < s3 ? sdistance : s3;
            }
            table[compareIndex * tableWidth + otherCompareIndex] = localDistance + sdistance;
            return table[compareIndex * tableWidth + otherCompareIndex];
        }
    }
}