using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SlittersWPF
{
    [Serializable]
    public class WrapRollData
    {
        
        public double[] RollWidthData = new double[18];

        public double[] BandLowerLimit = new double[19];
        public double[] BandUpperLimit = new double[19];
        public double[] BladeLowerLimit = new double[19];
        public double[] BladeUpperLimit = new double[19];

    }
}
