using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WSMBT
    
{
    class ModbusComm 
    {
        WSMBT.WSMBTControl wsmbtControl1 = new WSMBT.WSMBTControl();

        public Int16[] SlitterPositionFirstNine = new Int16[36];
        public Int16[] SlitterPositionLastTen = new Int16[40];
        public Int16[] FaultPLCMessages = new Int16[9];
        public Int16[] CommandRegisters = new Int16[8];
        public Int16[] SlitterStptsFirstNine = new Int16[36];
        public Int16[] SlitterStptsLastTen = new Int16[40];
        public Int16[] SlitterLifeCycle = new Int16[38];
        public Int16[] PLcControlInputs = new Int16[2];
        public String ModbusMessage = "Checking Communication";
        public Int16[] PLCWrites = new Int16[2];
                
        //Constructor
        public ModbusComm()
        {
            
        }
        //DeConstructor
        ~ModbusComm()
        {
            wsmbtControl1.Close();
            ModbusMessage = "DisConnected to PLC";         
        }

        public string ModbusOpenConnection()
        {
                wsmbtControl1.LicenseKey("172D-939E-7B24-6DB2-6CEA-3987");
                WSMBT.Result Result;
                wsmbtControl1.Mode = WSMBT.Mode.TCP_IP;
                wsmbtControl1.ResponseTimeout = 1000;
            //ANC PLC Address = "10.10.10.110"
            String PLCAddress = SlittersWPF.Properties.Settings.Default.PLCAddr;
                Result = wsmbtControl1.Connect(PLCAddress, 502); 
                if (Result != WSMBT.Result.SUCCESS)
                {
                    ModbusMessage = (wsmbtControl1.GetLastErrorString());
                }
                else
                {
                    ModbusMessage = "Connected";
                }
            
            return ModbusMessage;
            

        }
        /// <summary>
        /// need to check this out.
        /// </summary>
        public void ModbusDispose()
        {
            wsmbtControl1.Dispose();
            
        }

        public void ModbusCloseConnection()
        {
            wsmbtControl1.Close();
            ModbusMessage = "Program Closed PLC Connection";
        }

        private void ReadCoils()
        {
            bool[] Coils = new bool[10];
            WSMBT.Result Result;
            Result = wsmbtControl1.ReadCoils(1, 0, 10, Coils);
            if (Result == WSMBT.Result.SUCCESS)
            {
                String DataString = "";
                String str = "";

                for (int i = 0; i < 10; i++)
                {
                    str = String.Format("{0:D}", Coils[i]);
                    DataString = DataString + str + "\r\n";
                }
                
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void ReadDiscreteInputs()
        {
            bool[] DiscreteInputs = new bool[10];
            WSMBT.Result Result;
            Result = wsmbtControl1.ReadDiscreteInputs(1, 0, 10, DiscreteInputs);
            if (Result == WSMBT.Result.SUCCESS)
            {
                String DataString = "";
                String str = "";

                for (int i = 0; i < 10; i++)
                {
                    str = String.Format("{0:D}", DiscreteInputs[i]);
                    DataString = DataString + str + "\r\n";
                }
               
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        public String ReadHoldingRegisters(ushort StartAddr, ushort NumbOfRegisters, int selector)
        { 

            
                Int16[] Registers = new Int16[NumbOfRegisters];
                WSMBT.Result Result;

                Result = wsmbtControl1.ReadHoldingRegisters(1, StartAddr, NumbOfRegisters, Registers);
                    if (Result == WSMBT.Result.SUCCESS)
                    {

                    switch (selector)
                    {
                        case 1:  // Read Slitter positions - 36 registers
                            SlitterPositionFirstNine = Registers;
                            break;
                        case 2:  // Read Slitter positions - 40 registers
                            SlitterPositionLastTen = Registers;
                            break;
                        case 3:  // Read first Nine Slitter Setpoints and Calibrate Setpoints  - 36 registers
                            SlitterStptsFirstNine = Registers;
                            break;
                        case 4:  // Read Last Ten Slitter Setpoints  and Calibrate Setpoints - 40 registers
                            SlitterStptsLastTen = Registers;
                            break;
                        case 5:  // Read Slitter faults - 9 registers
                            FaultPLCMessages = Registers; 
                            break;
                        case 6:  // Read Command Registers - 8 registers
                            CommandRegisters = Registers;
                            break;
                        case 7: //Read Slitter LifeCylce Registers 38 registers
                            SlitterLifeCycle = Registers;
                            break;
                        case 8: // Read PLC Control Inputs
                            PLcControlInputs = Registers;
                            break;
                        case 9: // Read PLC Control Writes
                        PLCWrites = Registers;
                        break;
                        default:
                            ModbusMessage = "Retrieval of Data is not possible";
                            break;
                    }
                    }
                
                    else
                        {
                         ModbusMessage = (wsmbtControl1.GetLastErrorString());
                        }
            return ModbusMessage;
        }
            
            
        private void ReadInputRegisters()
        {
            Int16[] Registers = new Int16[10];
            WSMBT.Result Result;
            Result = wsmbtControl1.ReadInputRegisters(1, 0, 10, Registers);
            if (Result == WSMBT.Result.SUCCESS)
            {
                String DataString = "";
                String str = "";

                for (int i = 0; i < 10; i++)
                {
                    str = String.Format("{0:D}", Registers[i]);
                    DataString = DataString + str + "\r\n";
                }
               
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void WriteSingleCoil()
        {
            WSMBT.Result Result;
            Result = wsmbtControl1.WriteSingleCoil(1, 0, true);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        public String WriteSingleRegiste(Int16 PLCReg, Int16 Registers)
        {
            WSMBT.Result Result;

            Result = wsmbtControl1.WriteSingleRegister(1, (ushort)PLCReg, (short)Registers);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
            return ModbusMessage;
        }

        private void WriteMultipleCoils()
        {
            bool[] Coils = new bool[10];
            WSMBT.Result Result;
            for (int i = 0; i < 10; i++)
                Coils[i] = true;                       // Write ON to all 10 coils
            Result = wsmbtControl1.WriteMultipleCoils(1, 0, 10, Coils);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        public String WriteMultipleRegisters(Int16 PLCReg, Int16[] Registers, Int16 NumbOfRegisters)
        {
            Int16 PLCData = 0;
            WSMBT.Result Result;
           for (Int16 i = 0; i < NumbOfRegisters; i++)
                    PLCData = Registers[i];
            Result = wsmbtControl1.WriteMultipleRegisters(2, (ushort)PLCReg, (ushort)NumbOfRegisters, Registers);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
            else
            {
                ModbusMessage = "Connected";
            }
            return ModbusMessage;
        }

        public String WriteMultipleCmdRegisters(Int16 PLCReg, UInt16[] Registers, Int16 NumbOfRegisters)
        {
            WSMBT.Result Result;
            short[] PLCData = new short[NumbOfRegisters];
            for (int i = 0; i < NumbOfRegisters; i++)
            {
                PLCData[i] = Convert.ToInt16(Registers[i]);
            }
            Result = wsmbtControl1.WriteMultipleRegisters(3, (ushort)PLCReg, (ushort)NumbOfRegisters, PLCData);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
            else
            {
                ModbusMessage = "Connected";
            }
            return ModbusMessage;
        }

        private void ReadUserDefinedCoils()
        {
            bool[] Coils = new bool[10];
            WSMBT.Result Result;
            Result = wsmbtControl1.ReadUserDefinedCoils(1, 1, 0, 10, Coils);
            if (Result == WSMBT.Result.SUCCESS)
            {
                String DataString = "";
                String str = "";

                for (int i = 0; i < 10; i++)
                {
                    str = String.Format("{0:D}", Coils[i]);
                    DataString = DataString + str + "\r\n";
                }
                
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void ReadUserDefinedRegisters()
        {
            Int16[] Registers = new Int16[10];
            WSMBT.Result Result;
            Result = wsmbtControl1.ReadUserDefinedRegisters(1, 3, 0, 10, Registers);
            if (Result == WSMBT.Result.SUCCESS)
            {
                String DataString = "";
                String str = "";

                for (int i = 0; i < 10; i++)
                {
                    str = String.Format("{0:D}", Registers[i]);
                    DataString = DataString + str + "\r\n";
                }
                
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void WriteUserDefinedCoils()
        {
            bool[] Coils = new bool[10];
            WSMBT.Result Result;
            for (int i = 0; i < 10; i++)
                Coils[i] = true;                       // Write ON to all 10 coils
            Result = wsmbtControl1.WriteUserDefinedCoils(1, 15, 0, 10, Coils);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void WriteUserDefinedRegisters()
        {
            Int16[] Registers = new Int16[10];
            WSMBT.Result Result;
            for (Int16 i = 0; i < 10; i++)
                Registers[i] = i;
            Result = wsmbtControl1.WriteUserDefinedRegisters(1, 16, 0, 10, Registers);
            if (Result != WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }

        private void ReadWriteMultipleRegisters()
        {
            Int16[] ReadRegisters = new Int16[10];
            Int16[] WriteRegisters = new Int16[10];
            WSMBT.Result Result;
            for (Int16 i = 0; i < 10; i++)
                WriteRegisters[i] = i;
            Result = wsmbtControl1.ReadWriteMultipleRegisters(1, 0, 10, ReadRegisters, 0, 10, WriteRegisters);
            if (Result == WSMBT.Result.SUCCESS)
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
            else
            {
                ModbusMessage = (wsmbtControl1.GetLastErrorString());
            }
        }
    }


}

