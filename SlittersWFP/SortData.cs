using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlittersWPF
{
    public class SortData
    {
        public char[] splitChars = new[] { '@', '!', ';', ',' };
        public static String OrderNumber = "";
        //public char[] splitCharsRW = new[] { 'R', 'W' };
        // Constructor
        public SortData()
        {
        }

        public double[] GetData()
        {
            var splitstring = TCPServSocket.wraptext.Split(splitChars);
            String[] wrapstring = new String[splitstring.Length];
            String[] wrapdata = new String[splitstring.Length];
            String[] orderdata = new String[splitstring.Length];
            double[] WrapData = new double[18]; //18 was NumberofRollsInSet
            wrapstring = splitstring;
            int row = 0;
            int RWDetected = 0;
            bool FindRW = false;
            int NumberOfRollsInSet = 0;

            foreach(String element in wrapstring)
            {
                FindRW = element.StartsWith("RW");
                if (FindRW)
                {
                    wrapstring[row] = wrapstring[row].Replace("R", "").Replace("W", "");
                    RWDetected = row;
                    NumberOfRollsInSet = row - 1;
                    NumberOfRollsInSet =Convert.ToInt32(wrapstring[row - 1]);
                }
                row++;
                OrderNumber = wrapstring[2];
                OrderNumber = OrderNumber.Replace("A", "").Replace("I", "");
            }

            
            // clear data
            for(int x = 0; x < 18; x++)
            {
                WrapData[x] = 0.0;
            }

            for (int i = 0; i < NumberOfRollsInSet; i++)
            {
                wrapdata[i] = wrapstring[i + RWDetected];
                WrapData[i] = Convert.ToDouble(wrapdata[i]);
            }
             
            return WrapData;
        }
    }
}
