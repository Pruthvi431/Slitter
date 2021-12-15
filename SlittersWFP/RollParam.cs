using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;

namespace SlittersWPF

{

    public class RollParam
    {
        public double[] WrapDataInit = { 1118.0, 557.0, 557.0, 1118.0, 292.0, 292.0, 1118.0, 1118.0, 1118.0, 1118.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        public double[] WrapData = { 1118.0, 557.0, 557.0, 1118.0, 292.0, 292.0, 1118.0, 1118.0, 1118.0, 1118.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        public double[] WrapData1 = { 635.0, 635.0, 635.0, 635.0, 686.0, 635.0, 686.0, 635.0, 686.0, 635.0, 635.0, 635.0, 635.0, 0.0, 0.0, 0.0, 0.0, 0.0 }; //Wrapmation Roll Width Data
        public double[] WrapExcel = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        public int OrderNumb = 0;
        public int MaxRolls = 18;
        public int MaxSlitters = 19;
        public int MaxCuts = 19;
        public Int32 ActPosnCuts = 0;
        public Int32 ParkPosnCuts = 0;
        public Int32 ParkLmtCuts = 0;
        public double MinSlitStpt = 0.0;
        public double MinDistBetweenSlitters = 153.0;
        public double[,] WrapExcel2 = new double[1000, 18];
        public double[] ActPosnSp = new double[19];
        public double[] ParkPosnSp = new double[19];
        public double[] ParkLmtSp = new double[19];
        public double[] ActPosnSpPartial = new double[19];
        public double[] ParkPosnSpPartial = new double[19];
        public double[] ParkLmtSpPartial = new double[19];
        public double[] VerifyBandStpts = new double[19];
        public double[] RollWidth = new double[18];
        public double[] RollWidthNoShrk = new double[18];
        public double[] RollWidthSP = new double[18];
        public double[] RollWidthChecker = new double[18];
        public double[] RollSPWithDsTrim = new double[19];
        public double MinRollWidth = 0.0;
        public double[] ParkPosn = { 250.0, 472.22, 944.42, 1416.67, 1888.89, 2361.11, 2833.33, 3305.55, 3777.78, 4250.0, 4722.22, 5194.44, 5666.67, 6138.89, 6611.11, 7083.33, 7555.56, 8027.78, 8250.0 };
        public double[] BandActPosn = new double[19];
        public double[] BladeActPosn = new double[19];
        public double[] BandStpt = new double[19];
        public double[] BandStptPartial = new double[19];
        public double[] BladeStpt = new double[19];
        public double[] CalibrateOffsets = new double[19];
        public double[] RollWidthBand = new double[18];
        public double[] RollWidthBlade = new double[18];
        public double TotalWidth = 0.0;
        public double OutOfTolerance = 2.0;
        public double CoarseWindow = 2.5;//2.5
        public double UnSelectedPosWindow = 1.0; //0.5
        public double InPosWindow = 0.125; //0.125
        public bool CmdOffsetChgd = false;
        public bool SlitOutOfServDetect = false; // used to signal plc writes
        public bool ActPosSelected = false;
        public bool ParkSelected = false;
        public bool ParkSelectedLimit = false;
        public double MaxParkSlit = 0.0;
        public double MaxActSlit = 0.0;
        public int MaxParkSlitNumb = 0;
        public int MaxActSlitNumb = 0;
        public int NumbOfslitParkSelectdLmt = 0;
        public int NumbOfSlitParkSelected = 0;
        public int NumbOfSlitActPosSelected = 0;
        public double TotalParkPosn = 0.0;
        public double TotalActPosn = 0.0;
        public int NumbOfRolls = 0;
        public double MaxWidth = 8500.0;
        public double MaxRollWidth = 2800.0;
        public double Shrinkage = 0.15; // Equals 0.15 %
        public double TotalWidthNoShrk = 0.0;
        public double DsTrim = 0.0;
        public double DSTrimFixed = 0.0;
        public bool CalibrateMode = true;
        public bool[] SlitterDisable = new bool[19];
        public bool[] SlitterOutofService = new bool[19];
        public bool[] SlitterOutofServicePLC = new bool[19];
        public static Int16[] SlitterLifeCycle = new Int16[38];
        public static Int16[] SlitterLifeCycleResets = new Int16[3];
        public static bool LifeCycleReset = false;
        public bool[] BandParkSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] BladeParkSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] BandActPosnSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] BladeActPosnSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] BandParkLimitSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] CalibrateCmdSelected = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public double[] BandLowerLimit = { 0.0, 150.0, 350.0, 1000.0, 1300.0, 1600.0, 1900.0, 2200.0, 2500.0, 3550.0, 3700.0, 3850.0, 4000.0, 4500.0, 5000.0, 5500.0, 6000.0, 6500.0, 8000.0 };
        public double[] BandUpperLimit = { 500.0, 2350.0, 2600.0, 3300.0, 3600.0, 3900.0, 4200.0, 4500.0, 4800.0, 5850.0, 6000.0, 6150.0, 6300.0, 6800.0, 7300.0, 7800.0, 8300.0, 8320.0, 8500.0 };
        public double[] BladeLowerLimit = { 0.0, 150.0, 350.0, 1000.0, 1300.0, 1600.0, 1900.0, 2200.0, 2500.0, 3550.0, 3700.0, 3850.0, 4000.0, 4500.0, 5000.0, 5500.0, 6000.0, 6500.0, 8000.0 };
        public double[] BladeUpperLimit = { 500.0, 2350.0, 2600.0, 3300.0, 3600.0, 3900.0, 4200.0, 4500.0, 4800.0, 5850.0, 6000.0, 6150.0, 6300.0, 6800.0, 7300.0, 7800.0, 8300.0, 8320.0, 8500.0 };
        public double[] SlitterLowerLimit = new double[19];
        public double[] SlitterUpperLimit = new double[19];
        public Boolean[] ManualSelect = new Boolean[19];
        public Boolean InvalidStpt = false;
        //Chamged Band and Blade Upper limit for Slitter 18 to prevent if from being trim slitter.  8500 to 8320.0

        public double[] BandCalibration = { 10.00, 470.00, 940.00, 1410.00, 1880.00, 2350.00, 2820.00, 3290.00, 3760.00, 4230.00, 4700.00, 5170.00, 5640.00, 6110.00, 6580.00, 7050.00, 7520.00, 7990.00, 8490.00 };
        public double[] BladeCalibration = { 10.00, 470.00, 940.00, 1410.00, 1880.00, 2350.00, 2820.00, 3290.00, 3760.00, 4230.00, 4700.00, 5170.00, 5640.00, 6110.00, 6580.00, 7050.00, 7520.00, 7990.00, 8490.00 };
        public bool[] SlitterCalibTextChgd = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

        public static bool[,] SlitCut = {{true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},  //x = Slitters Used for y = Slitter Cuts 19X19
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                                   {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true}};
        // Table to show which slitters will be selected for order
        public bool[,] SolutionSelect = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},  //x = Slitters Used for y = Slitter Cuts 19x19
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false}};
        public bool[,] SolutionSelectMax = {{true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},  //x = Slitters Used for y = Slitter Cuts 19x19
                                   {false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true}};
        public bool[,] SolutionSelectPark = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},  //x = Slitters Used for y = Slitter Cuts 19x19
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false}};
        public bool[,] SolutionSelectAct = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},  //x = Slitters Used for y = Slitter Cuts 19x19
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false}};
        public bool[,] SolutionSelectParkLmt = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},  //x = Slitters Used for y = Slitter Cuts 19x19
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false},
                                   {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false}};

        public bool[] SlittersSelected = new bool[19];
        public bool[] SlittersSelectedPLC = new bool[19];
        public bool[] SlittersSelectedChk = new bool[19];


        //Default Constructor
        public RollParam()
        { }

        // DeConstructor
        ~RollParam()
        { }

        public void SlitterLimits()
        {
            for (int x = 0; x < MaxSlitters; x++)
            {
                SlitterLowerLimit[x] = Math.Max(BandLowerLimit[x], BladeLowerLimit[x]);
                SlitterUpperLimit[x] = Math.Min(BandUpperLimit[x], BladeUpperLimit[x]);
            }
        }

        // Load initial Wrap Data
        public void LoadInitWrapData()
        {
            WrapData = WrapDataInit;
        }
        // Load  Wrap Data
        public void LoadWrapData()
        {
            if (WrapExcel[0] > 0.0)
            {
                WrapData = WrapExcel;
            }
            else
            {
                WrapData = WrapDataInit;
            }
        }
        // Zero Out Wrap Data 
        public void ZeroOutWrapData()  //Checks out >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        {
            for (int i = 0; i < MaxRolls; i++)
            {
                WrapData[i] = 0.0;
                RollWidthSP[i] = 0.0;
            }
            //Zero Out Band and Blade Stpt 
            for (int ZeroStps = 0; ZeroStps < MaxSlitters; ZeroStps++)
            {
                BandStpt[ZeroStps] = 0.0;
                BladeStpt[ZeroStps] = 0.0;
                RollSPWithDsTrim[ZeroStps] = 0.0;
            }
            //Zero Out Selected Slitters before Selecting 
            for (int ZeroOut = 0; ZeroOut < MaxSlitters; ZeroOut++)
            {
                BandParkSelected[ZeroOut] = false;
                BandActPosnSelected[ZeroOut] = false;
                BladeParkSelected[ZeroOut] = false;
                BladeActPosnSelected[ZeroOut] = false;
                BandParkLimitSelected[ZeroOut] = false;
                SlittersSelected[ZeroOut] = false;
            }
            //Set 19 Element Multi Array to False
            for (int x = 0; x < MaxSlitters; x++)
            {
                for (int y = 0; y < MaxSlitters; y++)
                {
                    SolutionSelect[x, y] = false;
                    SolutionSelectAct[x, y] = false;
                    SolutionSelectPark[x, y] = false;
                    SolutionSelectParkLmt[x, y] = false;
                    SlitCut[x, y] = false;
                }
            }

        }

        public void ZeroOutSlitterData()
        {
            //Zero Out Band and Blade Stpt 
            for (int ZeroStps = 0; ZeroStps < MaxSlitters; ZeroStps++)
            {
                BandStpt[ZeroStps] = 0.0;
                BladeStpt[ZeroStps] = 0.0;
                SlitterCalibTextChgd[ZeroStps] = false;
                CalibrateCmdSelected[ZeroStps] = false;
                CalibrateOffsets[ZeroStps] = 0.0;
            }
            //Zero Out Selected Slitters before Selecting 
            for (int ZeroOut = 0; ZeroOut < MaxSlitters; ZeroOut++)
            {
                BandParkSelected[ZeroOut] = false;
                BandActPosnSelected[ZeroOut] = false;
                BladeParkSelected[ZeroOut] = false;
                BladeActPosnSelected[ZeroOut] = false;
                BandParkLimitSelected[ZeroOut] = false;
                SlittersSelected[ZeroOut] = false;
            }
            //Set 19 Element Multi Array to False
            for (int x = 0; x < MaxSlitters; x++)
            {
                for (int y = 0; y < MaxSlitters; y++)
                {
                    SolutionSelect[x, y] = false;
                    SolutionSelectAct[x, y] = false;
                    SolutionSelectPark[x, y] = false;
                    SolutionSelectParkLmt[x, y] = false;
                    SlitCut[x, y] = false;
                }
            }
        }
        // Calculate Roll Parameters  CHECKS OUT>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public void CalcRollParam()
        {
            TotalWidth = 0.0;
            TotalWidthNoShrk = 0.0;
            NumbOfRolls = 0;

            for (int i = 0; i < MaxRolls; i++)
            {
                // Calculate Witdth for all Rolls in Set
                RollWidth[i] = (WrapData[i] * (Shrinkage * 0.01)) + WrapData[i];
                if (WrapData[i] > MinRollWidth)
                {
                    TotalWidth = TotalWidth + RollWidth[i];
                    RollWidthSP[i] = TotalWidth;
                    TotalWidthNoShrk += WrapData[i];
                    NumbOfRolls = NumbOfRolls + 1;
                }
            }
            // Add DS Trim to Slitter Septoint

            DsTrim = ((MaxWidth - TotalWidth) / 2);
            DSTrimFixed = DsTrim;
            RollSPWithDsTrim[0] = DsTrim;
            for (int x = 0; x < MaxRolls; x++)
            {
                if (RollWidthSP[x] > MinRollWidth)
                {
                    RollSPWithDsTrim[x + 1] = RollWidthSP[x] + DsTrim;
                }
                else
                {
                    RollSPWithDsTrim[x + 1] = 0.0;
                }
            }

        }

        // Calculate Roll Parameters  Fixed DS Trim
        public void CalcRollParamFixedDSTrim()
        {
            TotalWidth = 0.0;
            TotalWidthNoShrk = 0.0;
            NumbOfRolls = 0;

            for (int i = 0; i < MaxRolls; i++)
            {
                // Calculate Witdth for all Rolls in Set
                RollWidth[i] = (WrapData[i] * (Shrinkage * 0.01)) + WrapData[i];
                if (WrapData[i] > MinRollWidth)
                {
                    TotalWidth = TotalWidth + RollWidth[i];
                    RollWidthSP[i] = TotalWidth;
                    TotalWidthNoShrk += WrapData[i];
                    NumbOfRolls = NumbOfRolls + 1;
                }
            }
            // Add DS Trim to Slitter Septoint

            RollSPWithDsTrim[0] = DSTrimFixed;
            for (int x = 0; x < MaxRolls; x++)
            {
                if (RollWidthSP[x] > MinRollWidth)
                {
                    RollSPWithDsTrim[x + 1] = RollWidthSP[x] + DSTrimFixed;
                }
                else
                {
                    RollSPWithDsTrim[x + 1] = 0.0;
                }
            }

        }

        // Select Slitter for Cuts. Every slitter is evaluated for each cut. Slitter can be selected for several cuts.  True and False depending on lower and upper limits. i3 = cuts and j1 = slitters>>>>>>>>>>>>>>>>>>
        public void SelectSlittersForCuts()
        {
            for (int i3 = 0; i3 < MaxSlitters; i3++)
            {
                for (int j1 = 0; j1 < MaxSlitters; j1++)
                {
                    if ((SlitterLowerLimit[j1] < RollSPWithDsTrim[i3]) && (RollSPWithDsTrim[i3] <= SlitterUpperLimit[j1]))
                        SlitCut[i3, j1] = true;
                    else SlitCut[i3, j1] = false;
                }
            }
        }

        // // Calculate using Park Position - Select Slitter for Cut
        public void SelectslitterforCutsParkPosn()
        {

            //Cycle through how many cuts in set.  Ignore Cut 0 and Slit 0 (Slitter 1) because it's is selected and setpoint assigned.
            for (int x = 1; x < NumbOfRolls; x++)
            {
                MaxRollWidth = 8500.0;
                for (int y = 1; y < (MaxSlitters - 1); y++)
                {
                    double SlitDiff = Math.Abs(RollSPWithDsTrim[x] - BladeCalibration[y]);
                    double SlitMin = Math.Min(SlitDiff, MaxRollWidth);
                    if ((SlitMin < MaxRollWidth) && (RollSPWithDsTrim[x] > MinSlitStpt) && (BandParkSelected[y] == false) && (SlitterDisable[y] == false) && SlitCut[x, y])
                    {
                        MaxRollWidth = SlitMin;
                        SolutionSelectPark[x, y] = true;
                        // if above statement true then set lower slitters to false
                        for (int z = (y - 1); z != -1; --z)
                        {
                            SolutionSelectPark[x, z] = false;
                        }
                    }
                }
                for (int k = 0; k < (MaxSlitters - 1); k++)
                {
                    if (SolutionSelectPark[x, k] == true)
                    {
                        BandParkSelected[k] = true;
                    }
                }
            }
            BandParkSelected[0] = true;
            BandParkSelected[18] = true;
            SolutionSelectPark[0, 0] = true;
            SolutionSelectPark[(NumbOfRolls), 18] = true;
        }

        // Calculate using Acutal Slitter Position - Select Slitter for Cut!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void SelectSitterforCutsActPosn()
        {

            for (int x = 1; x < NumbOfRolls; x++)
            {
                MaxRollWidth = 8500.0;
                for (int y = 1; y < (MaxSlitters - 1); y++)
                {
                    double SlitDiff = Math.Abs(RollSPWithDsTrim[x] - BandActPosn[y]);
                    double SlitMin = Math.Min(SlitDiff, MaxRollWidth);
                    if ((SlitMin < MaxRollWidth) && (RollSPWithDsTrim[x] > MinSlitStpt) && (BandActPosnSelected[y] == false) && (SlitterDisable[y] == false) && SlitCut[x, y])
                    {
                        MaxRollWidth = SlitMin;
                        SolutionSelectAct[x, y] = true;
                        for (int z = (y - 1); z != -1; --z)
                        {
                            SolutionSelectAct[x, z] = false;
                        }
                    }
                }
                for (int k = 0; k < (MaxSlitters - 1); k++)
                {
                    if (SolutionSelectAct[x, k] == true)
                    {
                        BandActPosnSelected[k] = true;
                    }
                }
            }
            BandActPosnSelected[0] = true;
            BandActPosnSelected[18] = true;
            SolutionSelectAct[0, 0] = true;
            SolutionSelectAct[(NumbOfRolls), 18] = true;
        }

        // Calculate using Slitter Upper Band Limit - Select Slitter for Cut!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void SelectSitterforCutsParkLimit()
        {

            for (int x = 1; x < NumbOfRolls; x++)
            {
                MaxRollWidth = 8500.0;
                for (int y = 1; y < (MaxSlitters - 1); y++)
                {

                    double SlitDiff = Math.Abs(RollSPWithDsTrim[x] - SlitterUpperLimit[y]);
                    double SlitMin = Math.Min(SlitDiff, MaxRollWidth);
                    if ((SlitMin < MaxRollWidth) && (RollSPWithDsTrim[x] > MinSlitStpt) && (BandParkLimitSelected[y] == false) && (SlitterDisable[y] == false) && SlitCut[x, y])
                    {
                        MaxRollWidth = SlitMin;
                        SolutionSelectParkLmt[x, y] = true;

                        for (int z = (y - 1); z != -1; --z)
                        {
                            SolutionSelectParkLmt[x, z] = false;
                        }
                    }
                }
                for (int k = 0; k < (MaxSlitters - 1); k++)
                {
                    if (SolutionSelectParkLmt[x, k] == true)
                    {
                        BandParkLimitSelected[k] = true;
                    }
                }
            }
            BandParkLimitSelected[0] = true;
            BandParkLimitSelected[18] = true;
            SolutionSelectParkLmt[0, 0] = true;
            SolutionSelectParkLmt[(NumbOfRolls), 18] = true;
        }

        public void CompareSlittersUsedToCuts()
        {
            //Compare Number of cuts to number of slitters selected *************************
            NumbOfSlitParkSelected = 0;
            NumbOfSlitActPosSelected = 0;
            NumbOfslitParkSelectdLmt = 0;

            for (int x = 0; x < MaxSlitters; x++)
            {
                if (BandActPosnSelected[x])
                {
                    NumbOfSlitActPosSelected = NumbOfSlitActPosSelected + 1;
                }
            }

            for (int y = 0; y < MaxSlitters; y++)
            {
                if (BandParkSelected[y])
                {
                    NumbOfSlitParkSelected = NumbOfSlitParkSelected + 1;
                }
            }

            for (int z = 0; z < MaxSlitters; z++)
            {
                if (BandParkLimitSelected[z])
                {
                    NumbOfslitParkSelectdLmt = NumbOfslitParkSelectdLmt + 1;
                }
            }
        }

        //Calculate Band Setpoints
        public double[] CalcSelectedBandStpts(bool[,] SlitterSolution)
        {
            SolutionSelect = SlitterSolution;
            for (int x = 0; x < MaxSlitters; x++)
            {
                SlittersSelected[x] = false;
                BandStptPartial[x] = 0.0;
            }

            //Always select slitter 1 and slitter 19 for trim slitters
            for (int i = 0; i < MaxSlitters; i++) //increment cuts
            {
                for (int j = 0; j < MaxSlitters; j++) // increment slitters
                {
                    if (SolutionSelect[i, j])
                    {
                        BandStptPartial[j] = RollSPWithDsTrim[i];
                        SlittersSelected[j] = true;
                    }
                }
            }
            return BandStptPartial;
        }

        // Calculate Setpoint for Slitters not used.  
        public double[] CalcBandStptsNotUsedActPosn(double[] SetpointBandActPosn, bool[] Sel)
        {
            double[] StptBandActPosn = new double[19];
            double SlitterCount = 0.0;
            double LowerStptCaptured = 0.0;
            double UpperStptCaptured = 0.0;
            Boolean FirstSlitDetected = false;
            Boolean SecondSlitDetected = false;
            StptBandActPosn = SetpointBandActPosn;

            for (int x = 0; x < MaxSlitters; x++)
            {
                SlitterCount = 0;
                LowerStptCaptured = 0.0;
                UpperStptCaptured = 0.0;
                FirstSlitDetected = false;
                SecondSlitDetected = false;
                if (!Sel[x] && !FirstSlitDetected)
                {
                    LowerStptCaptured = StptBandActPosn[x - 1];
                    FirstSlitDetected = true;
                    for (int y = (x + 1); y < MaxSlitters; y++)
                    {
                        if (Sel[y] && !SecondSlitDetected)
                        {
                            UpperStptCaptured = StptBandActPosn[y];
                            SecondSlitDetected = true;
                            SlitterCount++;
                        }
                        if (!SecondSlitDetected)
                        {
                            SlitterCount++;
                        }
                    }
                    if (FirstSlitDetected && SecondSlitDetected)
                    {
                        StptBandActPosn[x] = LowerStptCaptured + ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1));
                        if ((StptBandActPosn[x] > BandUpperLimit[x]) || ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1)) < MinDistBetweenSlitters)
                        {
                            StptBandActPosn[x] = LowerStptCaptured + MinDistBetweenSlitters + 1;
                        }

                    }

                }

            }

            return StptBandActPosn;


        }

        // Calculate Setpoint for Slitters not used.  
        public double[] CalcBandStptsNotUsedParkPosn(double[] SetpointBandParkPosn, bool[] Sel)
        {
            double[] StptBandParkPosn = new double[19];
            double SlitterCount = 0.0;
            double LowerStptCaptured = 0.0;
            double UpperStptCaptured = 0.0;
            Boolean FirstSlitDetected = false;
            Boolean SecondSlitDetected = false;
            StptBandParkPosn = SetpointBandParkPosn;

            for (int x = 0; x < MaxSlitters; x++)
            {
                SlitterCount = 0;
                LowerStptCaptured = 0.0;
                UpperStptCaptured = 0.0;
                FirstSlitDetected = false;
                SecondSlitDetected = false;
                if (!Sel[x] && !FirstSlitDetected)
                {
                    LowerStptCaptured = StptBandParkPosn[x - 1];
                    FirstSlitDetected = true;
                    for (int y = (x + 1); y < MaxSlitters; y++)
                    {
                        if (Sel[y] && !SecondSlitDetected)
                        {
                            UpperStptCaptured = StptBandParkPosn[y];
                            SecondSlitDetected = true;
                            SlitterCount++;
                        }
                        if (!SecondSlitDetected)
                        {
                            SlitterCount++;
                        }
                    }
                    if (FirstSlitDetected && SecondSlitDetected)
                    {
                        StptBandParkPosn[x] = LowerStptCaptured + ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1));
                        if ((StptBandParkPosn[x] > BandUpperLimit[x]) || ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1)) < MinDistBetweenSlitters)
                        {
                            StptBandParkPosn[x] = LowerStptCaptured + MinDistBetweenSlitters + 1;
                        }

                    }

                }

            }

            return StptBandParkPosn;


        }

        // Calculate Setpoint for Slitters not used.  
        public double[] CalcBandStptsNotUsedParkLmt(double[] SetpointBandParkLimit, bool[] Sel)
        {
            double[] StptBandParkLimit = new double[19];
            double SlitterCount = 0.0;
            double LowerStptCaptured = 0.0;
            double UpperStptCaptured = 0.0;
            Boolean FirstSlitDetected = false;
            Boolean SecondSlitDetected = false;
            StptBandParkLimit = SetpointBandParkLimit;

            for (int x = 0; x < MaxSlitters; x++)
            {
                SlitterCount = 0;
                LowerStptCaptured = 0.0;
                UpperStptCaptured = 0.0;
                FirstSlitDetected = false;
                SecondSlitDetected = false;
                if (!Sel[x] && !FirstSlitDetected)
                {
                    LowerStptCaptured = StptBandParkLimit[x - 1];
                    FirstSlitDetected = true;
                    for (int y = (x + 1); y < MaxSlitters; y++)
                    {
                        if (Sel[y] && !SecondSlitDetected)
                        {
                            UpperStptCaptured = StptBandParkLimit[y];
                            SecondSlitDetected = true;
                            SlitterCount++;
                        }
                        if (!SecondSlitDetected)
                        {
                            SlitterCount++;
                        }
                    }
                    if (FirstSlitDetected && SecondSlitDetected)
                    {
                        StptBandParkLimit[x] = LowerStptCaptured + ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1));
                        if ((StptBandParkLimit[x] > BandUpperLimit[x]) || ((UpperStptCaptured - LowerStptCaptured) / (SlitterCount + 1)) < MinDistBetweenSlitters)
                        {
                            StptBandParkLimit[x] = LowerStptCaptured + MinDistBetweenSlitters + 1;
                        }

                    }

                }

            }

            return StptBandParkLimit;


        }

        // Verify Slitter Setpoints are minimum 153.00 mm apart. zero out solutionselect and slittersselected
        public Boolean VerifySlitterSetpoints(double[] VerifyBandStpts)
        {
            Boolean Passed = true;
            Boolean Spacing = true;
            Boolean StptCheck = true;
            Boolean LimitViolation = true;

            for (int z = 0; z < MaxRolls; z++)
            {
                if (((Math.Abs(VerifyBandStpts[z] - VerifyBandStpts[z + 1]) < MinDistBetweenSlitters) && Spacing))
                {
                    Spacing = false;
                }
            }

            for (int a = 0; a < MaxRolls; a++)
            {
                if ((VerifyBandStpts[a] > (VerifyBandStpts[a + 1] - MinDistBetweenSlitters)) && StptCheck)
                {
                    StptCheck = false;
                }

            }

            for (int x = 0; x < MaxSlitters; x++)
            {
                if (((VerifyBandStpts[x] > BandUpperLimit[x]) || (VerifyBandStpts[x] < BandLowerLimit[x])) && LimitViolation)
                {
                    LimitViolation = false;
                }
            }

            if (!StptCheck || !Spacing || !LimitViolation)
            {
                Passed = false;
            }
            return Passed;
        }

        //RollWidthCheck Monitors RollWidths and sets a bit in the plc if Out of Tolerance  Set at 2.0mm
        public Boolean RollWidthCheck()
        {
            Boolean FirstCutSelected = false;
            Boolean SecondCutSelected = false;
            Boolean RollWidthCalculated = false;
            Int32[] FirstRollCut = new Int32[19];
            Int32[] SecondRollCut = new Int32[19];
            Int32 z = 0;
            Boolean ExcessiveError = false;

            for (Int32 x = 0; x < MaxRolls; x++) // Cycle through Rolls
            {
                z = 0;
                FirstCutSelected = false;
                SecondCutSelected = false;
                RollWidthCalculated = false;

                for (Int32 y = 0; y < MaxSlitters; y++)
                {
                    if (SlittersSelectedPLC[y] && !FirstCutSelected)
                    {

                        FirstRollCut[z] = z = y;
                        SlittersSelectedPLC[y] = false;
                        FirstCutSelected = true;
                    }
                    if (SlittersSelectedPLC[y] && !SecondCutSelected)
                    {
                        SecondRollCut[y] = y;
                        SecondCutSelected = true;
                    }
                    if (FirstCutSelected && SecondCutSelected && !RollWidthCalculated)
                    {
                        RollWidthBand[x] = Math.Abs(BandActPosn[z] - BandActPosn[y]);
                        RollWidthCalculated = true;
                        if (Math.Abs(RollWidthBand[x] - RollWidthChecker[x]) > OutOfTolerance)
                        {
                            ExcessiveError = true;
                        }
                    }


                }
            }
            return ExcessiveError;
        }

        //checks out >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public void GetExcelData()
        {
            for (int i = 0; i < 18; i++)
            {
                if (WrapExcel2[OrderNumb, i] > 0.0)
                {
                    WrapExcel[i] = WrapExcel2[OrderNumb, i] * 10.0;
                }
            }
        }

        //Checks out >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public void Openexcel()
        {

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkBook = xlApp.Workbooks.Open(@"C:\SourceCode\WpfAppslit\WpfAppslit\WrapOrders.xlsx", 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", true, false, 0, true, 1, 0);
            Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            object misValue = System.Reflection.Missing.Value;
            Excel.Range xlRange = xlWorkSheet.get_Range("B5", "S510");
            int MaxColumns = 19;
            int iRowCounter = xlWorkSheet.UsedRange.Rows.Count;

            for (int row = 1; row < 500; row++)
            {

                for (int column = 1; column < MaxColumns; column++)
                {
                    if (xlRange.Cells[row, column].Value2 != null)
                    {
                        WrapExcel2[row - 1, column - 1] = (xlRange.Cells[row, column].Value2);
                    }
                }
            }



            // xlWorkBook.Save();
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkSheet);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                xlApp = null;

            }
            catch (Exception ex)
            {
                xlApp = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }


    }
}




