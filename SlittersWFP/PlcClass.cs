using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlittersWPF
{
    class PlcClass
    {

        //Constructor
        public  PlcClass()
        {

        }

        //Deconstructor
        ~PlcClass()
        {

        }
        // Winder Running bit
        public Boolean WinderRunning = false;
        //Create two seperate arrays because of limitation of modbus
        //Array is Slitter Setpints and Calibrate Setpoints for the first 9 Slitters
        public Int16[] PLCWriteStptFirst9Registers = new Int16[36];
        //Array is Slitter Setpints and Calibrate Setpoints for the first 9 Slitters
        public Int16[] PLCWriteStptLast10Registers = new Int16[40];
        //Array  Read for Slitter Setpoints and Calibrate Setpoints for first 9 Slitters
        public Int16[] PLCReadStptFirst9Registers = new Int16[36];
        //Array  Read for Slitter Setpoints and Calibrate Setpoints for last10 Slitters
        public Int16[] PLCReadStptLast10Registers = new Int16[40];
        //Array Read for Slitter Positions First 9 
        public Int16[] PLCReadPositionFirst9Slitters = new Int16[36];
        //Array Read for Slitter Positions Last 10 
        public Int16[] PLCReadPositionLast10Slitters = new Int16[40];
        // Array Read for Command Registers in plc
        public Int16[] CmdReadRegisters = new Int16[8];
        // Array Write for Command Registers in plc
        public UInt16[] CmdWriteRegisters = new UInt16[8];
        //Array for Command read and Writes to PLC
        // Multi-dimensional array for command registres
        public bool[,] CmdReg = new bool[8,16];
        // Two dimensional array for control bits
        public bool[,] PLCControlBits = new bool[2, 16];
        public bool[,] PLCWriteControlBits = new bool[2, 16];

        // Read plc control registers and split bits into a two dimentsion array for Registers 14000 and 14001
        //***Write to plc*** Assign Selected Slitter for Cuts to PLC Register  
        // *** write to plc *** Commands for "New Slitter Assignment" - "Auto Position Slitters Command" - Position Core Chucks Command" - " Fault Reset Command"
        // *** Bit 1 New Slitter Assignment Command
        // *** Bit 6 Auto Position Slitters Command
        // *** Bit 7 Positon Core Chucks Command
        // *** Bit 8 Clean Mode Off
        // *** Bit 9 Clean Mode On
        // *** Bit 11 Fault Reset Command
        // *** Bit 12 Excessive Error Command
        public bool[,] CntrlRegisterBitConverter(Int16[] ControlRegisters)

        {
            // Bitval holds result from bit convertor and is passed back as a multi-dimensional array to function call
            bool[,] Bitval = new bool[2, 16];
            //constants for bit calculation

            UInt16[] BitArr = { 32768, 16384, 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1 };
            //ControlRegisters[0];  //R14000
            //ControlRegisters[1];  //R14001

            UInt16[] CntrlRegisters = new UInt16[2];
            for (int y = 0; y < 2; y++)
            {
                CntrlRegisters[y] = (UInt16)(ControlRegisters[y]);
            }


            for (int x = 0; x < 2; x++)
            {
                Bitval[x, 15] = ((CntrlRegisters[x] / BitArr[0]) > 0) ? true : false;
                if (Bitval[x, 15]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[0]); }

                Bitval[x, 14] = ((CntrlRegisters[x] / BitArr[1]) > 0) ? true : false;
                if (Bitval[x, 14]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[1]); }

                Bitval[x, 13] = ((CntrlRegisters[x] / BitArr[2]) > 0) ? true : false;
                if (Bitval[x, 13]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[2]); }

                Bitval[x, 12] = ((CntrlRegisters[x] / BitArr[3]) > 0) ? true : false;
                if (Bitval[x, 12]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[3]); }

                Bitval[x, 11] = ((CntrlRegisters[x] / BitArr[4]) > 0) ? true : false;
                if (Bitval[x, 11]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[4]); }

                Bitval[x, 10] = ((CntrlRegisters[x] / BitArr[5]) > 0) ? true : false;
                if (Bitval[x, 10]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[5]); }

                Bitval[x, 9] = ((CntrlRegisters[x] / BitArr[6]) > 0) ? true : false;
                if (Bitval[x, 9]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[6]); }

                Bitval[x, 8] = ((CntrlRegisters[x] / BitArr[7]) > 0) ? true : false;
                if (Bitval[x, 8]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[7]); }

                Bitval[x, 7] = ((CntrlRegisters[x] / BitArr[8]) > 0) ? true : false;
                if (Bitval[x, 7]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[8]); }

                Bitval[x, 6] = ((CntrlRegisters[x] / BitArr[9]) > 0) ? true : false;
                if (Bitval[x, 6]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[9]); }

                Bitval[x, 5] = ((CntrlRegisters[x] / BitArr[10]) > 0) ? true : false;
                if (Bitval[x, 5]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[10]); }

                Bitval[x, 4] = ((CntrlRegisters[x] / BitArr[11]) > 0) ? true : false;
                if (Bitval[x, 4]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[11]); }

                Bitval[x, 3] = ((CntrlRegisters[x] / BitArr[12]) > 0) ? true : false;
                if (Bitval[x, 3]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[12]); }

                Bitval[x, 2] = ((CntrlRegisters[x] / BitArr[13]) > 0) ? true : false;
                if (Bitval[x, 2]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[13]); }

                Bitval[x, 1] = ((CntrlRegisters[x] / BitArr[14]) > 0) ? true : false;
                if (Bitval[x, 1]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[14]); }

                Bitval[x, 0] = ((CntrlRegisters[x] / BitArr[15]) > 0) ? true : false;
                if (Bitval[x, 0]) { CntrlRegisters[x] = (UInt16)(CntrlRegisters[x] - BitArr[15]); }

            }

            return Bitval;
        }


        public UInt16[] BitRegConvertor(bool[,] BitCollection)
        {
            UInt16[] BitArr = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
            UInt16[] register = new UInt16[8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    if (BitCollection[x,y])
                    {
                        register[x] = (UInt16)(register[x] + BitArr[y]);
                    }
                }
            }
            return register; 
        }

        public bool[,] CmdRegisterBitConverter(Int16[] CommandRegisters)
        {
            // Bitval holds result from bit convertor and is passed back as a multi-dimensional array to form1
            bool[,] Bitval = new bool[8, 16];
            //constants for bit calculation
            
            UInt16[] BitArr = { 32768, 16384, 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1 };
            //CommandRegisters[0];  //R14205
            //CommandRegisters[1];  //R14206
            //CommandRegisters[2];  //R14207
            //CommandRegisters[3];  //R14208
            //CommandRegisters[4];  //R14209
            //CommandRegisters[5];  //R14210
            //CommandRegisters[6];  //R14211
            //CommandRegisters[7];  //R14212
            UInt16[] CmdRegisters = new UInt16[8];
            for (int y= 0; y < 8; y++)
            {
                CmdRegisters[y] = (UInt16)(CommandRegisters[y]);
            }
            
            
            for (int x = 0; x < 8; x++)
            {
                Bitval[x, 15] = ((CmdRegisters[x] / BitArr[0]) > 0) ? true : false;
                if (Bitval[x, 15]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[0]); }

                Bitval[x, 14] = ((CmdRegisters[x] / BitArr[1]) > 0) ? true : false;
                if (Bitval[x, 14]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[1]); }

                Bitval[x, 13] = ((CmdRegisters[x] / BitArr[2]) > 0) ? true : false;
                if (Bitval[x, 13]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[2]); }

                Bitval[x, 12] = ((CmdRegisters[x] / BitArr[3]) > 0) ? true : false;
                if (Bitval[x, 12]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[3]); }

                Bitval[x, 11] = ((CmdRegisters[x] / BitArr[4]) > 0) ? true : false;
                if (Bitval[x, 11]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[4]); }

                Bitval[x, 10] = ((CmdRegisters[x] / BitArr[5]) > 0) ? true : false;
                if (Bitval[x, 10]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[5]); }

                Bitval[x, 9] = ((CmdRegisters[x] / BitArr[6]) > 0) ? true : false;
                if (Bitval[x, 9]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[6]); }

                Bitval[x, 8] = ((CmdRegisters[x] / BitArr[7]) > 0) ? true : false;
                if (Bitval[x, 8]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[7]); }

                Bitval[x, 7] = ((CmdRegisters[x] / BitArr[8]) > 0) ? true : false;
                if (Bitval[x, 7]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[8]); }

                Bitval[x, 6] = ((CmdRegisters[x] / BitArr[9]) > 0) ? true : false;
                if (Bitval[x, 6]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[9]); }

                Bitval[x, 5] = ((CmdRegisters[x] / BitArr[10]) > 0) ? true : false;
                if (Bitval[x, 5]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[10]); }

                Bitval[x, 4] = ((CmdRegisters[x] / BitArr[11]) > 0) ? true : false;
                if (Bitval[x, 4]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[11]); }

                Bitval[x, 3] = ((CmdRegisters[x] / BitArr[12]) > 0) ? true : false;
                if (Bitval[x, 3]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[12]); }

                Bitval[x, 2] = ((CmdRegisters[x] / BitArr[13]) > 0) ? true : false;
                if (Bitval[x, 2]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[13]); }

                Bitval[x, 1] = ((CmdRegisters[x] / BitArr[14]) > 0) ? true : false;
                if (Bitval[x, 1]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[14]); }

                Bitval[x, 0] = ((CmdRegisters[x] / BitArr[15]) > 0) ? true : false;
                if (Bitval[x, 0]) { CmdRegisters[x] = (UInt16)(CmdRegisters[x] - BitArr[15]); }
                
            }
            
            return Bitval;
        }

        //***Write to plc*** Slitter Setpoints must have the whole number and decimal portion seperated and send each component on a seperate register.  This method is for the first 9 Slitter Setpoints

        // *** Format Slitter Blade Position into double array

        public double[] RegisterFormatForSlitterBladePositon()
        {
            int[] FirstRegIndex = { 0, 4, 8, 12, 16, 20, 24, 28, 32 };
            int[] SecondRegIndex = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36 };
            double[] SlitterBladePosnFdbk = new double[19];
            for (int x = 0; x < 10; x++)
                {
                if (x < 9)
                { SlitterBladePosnFdbk[x] = Convert.ToDouble(PLCReadPositionFirst9Slitters[FirstRegIndex[x]]) + Convert.ToDouble(PLCReadPositionFirst9Slitters[(FirstRegIndex[x] + 1)] * 0.001); }
                       SlitterBladePosnFdbk[x + 9] = Convert.ToDouble(PLCReadPositionLast10Slitters[SecondRegIndex[x]]) + Convert.ToDouble(PLCReadPositionLast10Slitters[(SecondRegIndex[x]) + 1] * 0.001);
            }
            
            return SlitterBladePosnFdbk;
        }

        // *** Format Slitter Band Position into double array
        public double[] RegisterFormatForSlitterBandPositon()
        {
            int[] FirstRegIndex = { 2, 6, 10, 14, 18, 22, 26, 30, 34 };
            int[] SecondRegIndex = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38 };
            double[] SlitterBandPosnFdbk = new double[19];
            for (int x = 0; x < 10; x++)
            {
                if (x < 9)
                { SlitterBandPosnFdbk[x] = Convert.ToDouble(PLCReadPositionFirst9Slitters[FirstRegIndex[x]]) + Convert.ToDouble(PLCReadPositionFirst9Slitters[(FirstRegIndex[x] + 1)] * 0.001); }
                    SlitterBandPosnFdbk[x + 9] = Convert.ToDouble(PLCReadPositionLast10Slitters[SecondRegIndex[x]]) + Convert.ToDouble(PLCReadPositionLast10Slitters[(SecondRegIndex[x] + 1)] * 0.001); 
            }
        return SlitterBandPosnFdbk;
        }

        public Int16[] RegisterFormatFirst9SlitterStpt( double[] CalibrateOffset, double[] SlitterStpt)
        {
            int[] RegIndex = {2, 6, 10, 14, 18, 22, 26, 30, 34};
            double HoldingRegister = 0.0;
            double HoldingRegister1 = 0.0;
            double DecimalPortion = 0.0;
            for(int x = 0; x< 9; x++)           
            {

                HoldingRegister = SlitterStpt[x];
                HoldingRegister1 = Math.Truncate(SlitterStpt[x]);
                DecimalPortion = HoldingRegister - HoldingRegister1;
                DecimalPortion = Math.Round(DecimalPortion, 2);
                DecimalPortion = DecimalPortion * 100.0;
                PLCWriteStptFirst9Registers[RegIndex[x]] = (Int16)SlitterStpt[x];
                PLCWriteStptFirst9Registers[RegIndex[x] + 1] = (Int16)DecimalPortion;

            }
            int[] RegIndex1 = { 0, 4, 8, 12, 16, 20, 24, 28, 32 };
            HoldingRegister = 0.0;
            HoldingRegister1 = 0.0;
            DecimalPortion = 0.0;
            for (int x = 0; x < 9; x++)
            {

                HoldingRegister = CalibrateOffset[x];
                HoldingRegister1 = Math.Truncate(CalibrateOffset[x]);
                DecimalPortion = HoldingRegister - HoldingRegister1;
                DecimalPortion = Math.Round(DecimalPortion, 2);
                DecimalPortion = DecimalPortion * 100.0;
                PLCWriteStptFirst9Registers[RegIndex1[x]] = (Int16)CalibrateOffset[x];
                PLCWriteStptFirst9Registers[RegIndex1[x] + 1] = (Int16)DecimalPortion;

            }
            return PLCWriteStptFirst9Registers;
        }

        
        //*** Write to plc *** Slitter Setpoints must have the whole number and decimal portion seperated and send each component on a seperate register.  This method is for the Last 10 Slitter Setpoints
        public Int16[] RegisterFormatLast10SlitterStpt(double[] CalibrateOffset, double[] SlitterStpt)
        {
            int[] RegIndex = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38};
            double HoldingRegister = 0.0;
            double HoldingRegister1 = 0.0;
            double DecimalPortion = 0.0;
            for (int x = 0; x < 10; x++)
            {

                HoldingRegister = SlitterStpt[x + 9];
                HoldingRegister1 = Math.Truncate(SlitterStpt[x  + 9]);
                DecimalPortion = HoldingRegister - HoldingRegister1;
                DecimalPortion = Math.Round(DecimalPortion,2);
                DecimalPortion = DecimalPortion * 100.0;
                PLCWriteStptLast10Registers[RegIndex[x]] = (Int16)SlitterStpt[x + 9];
                PLCWriteStptLast10Registers[RegIndex[x] + 1] = (Int16)DecimalPortion;

            }
            int[] RegIndex1 = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36 };
            HoldingRegister = 0.0;
            HoldingRegister1 = 0.0;
            DecimalPortion = 0.0;
            for (int x = 0; x < 10; x++)
            {

                HoldingRegister = CalibrateOffset[x + 9];
                HoldingRegister1 = Math.Truncate(CalibrateOffset[x + 9]);
                DecimalPortion = HoldingRegister - HoldingRegister1;
                DecimalPortion = Math.Round(DecimalPortion, 2);
                DecimalPortion = DecimalPortion * 100.0;
                PLCWriteStptLast10Registers[RegIndex1[x]] = (Int16)CalibrateOffset[x + 9];
                PLCWriteStptLast10Registers[RegIndex1[x] + 1] = (Int16)DecimalPortion;

            }
            return PLCWriteStptLast10Registers;
        }

        //***Read Slitter Sepoints from plc
        public double[] RegisterSlitterStptReadFromPlc()
        {
            int[] FirstRegIndex = { 2, 6, 10, 14, 18, 22, 26, 30, 34 };
            int[] SecondRegIndex = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38 };
            double[] PLCSlitterStpt = new double[19];
            for (int x = 0; x < 10; x++)
            {
                if (x < 9)
                    { PLCSlitterStpt[x] = Convert.ToDouble(PLCReadStptFirst9Registers[FirstRegIndex[x]]) + Convert.ToDouble(PLCReadStptFirst9Registers[(FirstRegIndex[x] + 1)] * 0.001); }
                        PLCSlitterStpt[x + 9] = Convert.ToDouble(PLCReadStptLast10Registers[SecondRegIndex[x]]) + Convert.ToDouble(PLCReadStptLast10Registers[(SecondRegIndex[x] + 1)] * 0.001);
            }
            return PLCSlitterStpt;
        }

        //***Read Slitter Calibrate Sepoints from plc
        public double[] RegisterSlitterCalibStptReadFromPlc()
        {
            int[] FirstRegIndex = { 0, 4, 8, 12, 16, 20, 24, 28, 32};
            int[] SecondRegIndex = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36};
            double[] PLCSCalibrateStpt = new double[19];
            for (int x = 0; x < 10; x++)
            {
                if (x < 9) { PLCSCalibrateStpt[x] = Convert.ToDouble(PLCReadStptFirst9Registers[FirstRegIndex[x]]) + Convert.ToDouble(PLCReadStptFirst9Registers[(FirstRegIndex[x] + 1)] * 0.001); }
                PLCSCalibrateStpt[x] = Convert.ToDouble(PLCReadStptLast10Registers[SecondRegIndex[x]]) + Convert.ToDouble(PLCReadStptLast10Registers[(SecondRegIndex[x] + 1)] * 0.001);
            }
            return PLCSCalibrateStpt;
        }

       //***Write to plc*** Assign Selected Slitter for Cuts to PLC Register  

       // *** write to plc *** Commands for "New Slitter Assignment" - "Auto Position Slitters Command" - Position Core Chucks Command" - " Fault Reset Command"
       // *** Bit 1 New Slitter Assignment Command
       // *** Bit 6 Auto Position Slitters Command
       // *** Bit 7 Positon Core Chucks Command
       // *** Bit 8 Clean Mode Off
       // *** Bit 9 Clean Mode On
       // *** Bit 11 Fault Reset Command
       // *** Bit 12 Excessive Error Command
       // *** Code not used 
          public UInt16 Commands(bool[] Order)
        {
            UInt16 CmdAssignment = 0;
            UInt16 CmdAssign = 0;
            UInt16[] BitArr = new UInt16[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
            int i = 0;
            foreach (bool element in Order)
            {
                if (element)
                {
                    CmdAssign = BitArr[i];
                    CmdAssignment = (UInt16)(CmdAssignment + CmdAssign); //was (ushort)
                }
                i++;
            }
            return CmdAssignment;
        }
        
        public void BitAssignmentForActiveSlitters(bool[] SelectedSlitters)
        {
            CmdReg[0,0] = SelectedSlitters[0];
            CmdReg[0,5] = SelectedSlitters[1];
            CmdReg[0,10] = SelectedSlitters[2];
            CmdReg[1,0] = SelectedSlitters[3];
            CmdReg[1,5] = SelectedSlitters[4];
            CmdReg[1,10] = SelectedSlitters[5];
            CmdReg[2,0] = SelectedSlitters[6];
            CmdReg[2,5] = SelectedSlitters[7];
            CmdReg[2,10] = SelectedSlitters[8];
            CmdReg[3,0] = SelectedSlitters[9];
            CmdReg[3,5] = SelectedSlitters[10];
            CmdReg[3,10] = SelectedSlitters[11];
            CmdReg[4,0] = SelectedSlitters[12];
            CmdReg[4,5] = SelectedSlitters[13];
            CmdReg[4,10] = SelectedSlitters[14];
            CmdReg[5,0] = SelectedSlitters[15];
            CmdReg[5,5] = SelectedSlitters[16];
            CmdReg[5,10] = SelectedSlitters[17];
            CmdReg[6,0] = SelectedSlitters[18];

        }

        //***Read from plc*** Retrieve Selected Slitters from  PLC Register 
        public bool[] BitReadForActiveSlitters()
        {
            bool[] SelectedSlitters = new bool[19];
            SelectedSlitters[0] = CmdReg[0,0];
            SelectedSlitters[1] = CmdReg[0,5];
            SelectedSlitters[2] = CmdReg[0,10];
            SelectedSlitters[3] = CmdReg[1,0];
            SelectedSlitters[4] = CmdReg[1,5];
            SelectedSlitters[5] = CmdReg[1,10];
            SelectedSlitters[6] = CmdReg[2,0];
            SelectedSlitters[7] = CmdReg[2,5];
            SelectedSlitters[8] = CmdReg[2,10];
            SelectedSlitters[9] = CmdReg[3,0];
            SelectedSlitters[10] = CmdReg[3,5];
            SelectedSlitters[11] = CmdReg[3,10];
            SelectedSlitters[12] = CmdReg[4,0];
            SelectedSlitters[13] = CmdReg[4,5];
            SelectedSlitters[14] = CmdReg[4,10];
            SelectedSlitters[15] = CmdReg[5,0];
            SelectedSlitters[16] = CmdReg[5,5];
            SelectedSlitters[17] = CmdReg[5,10];
            SelectedSlitters[18] = CmdReg[6,0];

            return SelectedSlitters;

        }

        //***Write to plc*** Assign Selected Slitter Calibrate for offsets to PLC Register
        public void BitAssignmentForCalibrateCmd(bool[] CalibrateSlittersCmd)
        {
            CmdReg[0,1] = CalibrateSlittersCmd[0];
            CmdReg[0,6] = CalibrateSlittersCmd[1];
            CmdReg[0,11] = CalibrateSlittersCmd[2];
            CmdReg[1,1] = CalibrateSlittersCmd[3];
            CmdReg[1,6] = CalibrateSlittersCmd[4];
            CmdReg[1,11] = CalibrateSlittersCmd[5];
            CmdReg[2,1] = CalibrateSlittersCmd[6];
            CmdReg[2,6] = CalibrateSlittersCmd[7];
            CmdReg[2,11] = CalibrateSlittersCmd[8];
            CmdReg[3,1] = CalibrateSlittersCmd[9];
            CmdReg[3,6] = CalibrateSlittersCmd[10];
            CmdReg[3,11] = CalibrateSlittersCmd[11];
            CmdReg[4,1] = CalibrateSlittersCmd[12];
            CmdReg[4,6] = CalibrateSlittersCmd[13];
            CmdReg[4,11] = CalibrateSlittersCmd[14];
            CmdReg[5,1] = CalibrateSlittersCmd[15];
            CmdReg[5,6] = CalibrateSlittersCmd[16];
            CmdReg[5,11] = CalibrateSlittersCmd[17];
            CmdReg[6,1] = CalibrateSlittersCmd[18];
        }
        
        //***Write to plc*** Assign Selected Slitter Out of Service to PLC Register 
        public void BitAssignmentForOutofServiceCmd(bool[] OutOfServiceCmd)
        {
            CmdReg[0,3] = OutOfServiceCmd[0];
            CmdReg[0,8] = OutOfServiceCmd[1];
            CmdReg[0,13] = OutOfServiceCmd[2];
            CmdReg[1,3] = OutOfServiceCmd[3];
            CmdReg[1,8] = OutOfServiceCmd[4];
            CmdReg[1,13] = OutOfServiceCmd[5];
            CmdReg[2,3] = OutOfServiceCmd[6];
            CmdReg[2,8] = OutOfServiceCmd[7];
            CmdReg[2,13] = OutOfServiceCmd[8];
            CmdReg[3,3] = OutOfServiceCmd[9];
            CmdReg[3,8] = OutOfServiceCmd[10];
            CmdReg[3,13] = OutOfServiceCmd[11];
            CmdReg[4,3] = OutOfServiceCmd[12];
            CmdReg[4,8] = OutOfServiceCmd[13];
            CmdReg[4,13] = OutOfServiceCmd[14];
            CmdReg[5,3] = OutOfServiceCmd[15];
            CmdReg[5,8] = OutOfServiceCmd[16];
            CmdReg[5,13] = OutOfServiceCmd[17];
            CmdReg[6,3] = OutOfServiceCmd[18];
        }
        //***Read from plc*** Assign Selected Slitter Out of Service to PLC Register 
        public bool[] BitReadOutofServiceCmd()
        {
            bool[] OutOfServiceCmd = new bool[19];
            OutOfServiceCmd[0] = CmdReg[0,3];
            OutOfServiceCmd[1] = CmdReg[0,8];
            OutOfServiceCmd[2] = CmdReg[0,13];
            OutOfServiceCmd[3] = CmdReg[1,3];
            OutOfServiceCmd[4] = CmdReg[1,8];
            OutOfServiceCmd[5] = CmdReg[1,13];
            OutOfServiceCmd[6] = CmdReg[2,3];
            OutOfServiceCmd[7] = CmdReg[2,8];
            OutOfServiceCmd[8] = CmdReg[2,13];
            OutOfServiceCmd[9] = CmdReg[3,3];
            OutOfServiceCmd[10] = CmdReg[3,8];
            OutOfServiceCmd[11] = CmdReg[3,13];
            OutOfServiceCmd[12] = CmdReg[4,3];
            OutOfServiceCmd[13] = CmdReg[4,8];
            OutOfServiceCmd[14] = CmdReg[4,13];
            OutOfServiceCmd[15] = CmdReg[5,3];
            OutOfServiceCmd[16] = CmdReg[5,8];
            OutOfServiceCmd[17] = CmdReg[5,13];
            OutOfServiceCmd[18] = CmdReg[6,3];

            return OutOfServiceCmd;
        }

    }
}
