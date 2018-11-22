using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WSMBT;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;


namespace SlittersWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml Node 10.10.10.112 Version 3.6
    /// </summary>
    
   
    public partial class MainWindow : Window
    {
        #region Objects Timers 
        
        ModbusComm MB = new ModbusComm();
        FltMessage FM = new FltMessage();
        RollParam TM = new RollParam();
        PlcClass PLC = new PlcClass();
        TCPServSocket Sckt = new TCPServSocket();
        SortData DataSort = new SortData();
        WrapRollData wrp = new WrapRollData();
        //Tmr1 Drives Events for reading and writing registers
        DispatcherTimer Tmr1 = new DispatcherTimer();
        //Tmr2 monitors plc connection
        DispatcherTimer Tmr2 = new DispatcherTimer();
        DateTime dt = new DateTime();
        public bool CommandWritten = false;
        public bool CommunicatonPLCFailure = false;
        public bool CenterTrimOn = false;
        public bool CalibParamsLoaded = false;
        public Int32 TimeSlice = 0;
        public Boolean DiagOn = false;
        Boolean OutOfTolerance = false;
        Boolean OutofToleranceDisable = false;
        Boolean MaintMode = false;
        Boolean SlitStptVerified = false;
          
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            #region StartUp
            //Read Properties.Settings from App.config
            TM.MaxWidth = Properties.Settings.Default.MaxWidth;
            TM.MaxRollWidth = Properties.Settings.Default.MaxRollWidth;
            TM.OutOfTolerance = Properties.Settings.Default.OutOfTolerance;
            TM.CoarseWindow = Properties.Settings.Default.CoarseWindow;
            TM.UnSelectedPosWindow = Properties.Settings.Default.UnSelectedPosWindow;
            TM.InPosWindow = Properties.Settings.Default.InPosWindow;
                        
            Tmr1.Interval = new TimeSpan(0,0,0,0,300);
            Tmr2.Interval = new TimeSpan(0,0,0,15,000); //days , hours, minutes, seconds, milliseconds
            Tmr1.Tick += new EventHandler(TimerEventProcessor1);
            Tmr2.Tick += new EventHandler(TimerEventProcessor2);
            // Call xml file for slitter limits
            CheckXMLFileExists();
            // Take min and max values for band and blade limits.
            TM.SlitterLimits();
            OpenModbusConnection();
            TM.ZeroOutWrapData();
            TM.ZeroOutSlitterData();
            LoadInitialPlcData();
            FM.MsgReset();
            DiagReset();
            
            #endregion
        }

        #region Open Modbus Connection
        private void OpenModbusConnection()

        {
            String plciptype = " Rx3i 10.10.10.110 ";
            ModbusMessageTxtBx.Text = MB.ModbusOpenConnection();
            ModbusMessageTxtBx.Text += plciptype;
            CommunicatonPLCFailure = false;
        }
        #endregion

        #region Load Initial PLC Data from Rx3i 
        private void LoadInitialPlcData()
        {
            CenterTrimOn = true;
            TM.CalibrateMode = false;
            DSTrimPosnTxtBx.Text = "0.0";
            TSTrimPosnTxtBx.Text = "0.0";
            RemainingTrimTxtBx.Text = "0.0";
            OrderWidthTxtBx.Text = "0.0";
            ShrkWidthTxtBx.Text = "0.0";
            ShrinkIncDecBtn.Text = Convert.ToString(TM.Shrinkage);
                        
            string ConnectedMessage = "Connected";
            string ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            //Read First Nine Slitter Positions- Starting Register - Number of Registers - 1 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14024, 36, 1);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14024) Method LoadInitialPLCData \n ";
            }

            //Read Last Ten Slitters Position Starting Register - Number of Registers - 2 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14060, 40, 2);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14060) Method LoadInitialPLCData \n";
            }

            //Transfer data from Modbus object to PLc object and seperate data for slitter position feedback
            PLC.PLCReadPositionFirst9Slitters = MB.SlitterPositionFirstNine;
            PLC.PLCReadPositionLast10Slitters = MB.SlitterPositionLastTen;
            TM.BladeActPosn = PLC.RegisterFormatForSlitterBladePositon();
            TM.BandActPosn = PLC.RegisterFormatForSlitterBandPositon();
            //Update Slitter Position Text Boxes
            UpdateSlitterPosition();

            //Read First Nine Slitters Setpoints and Calibrate Setpoints Starting Register - Number of Registers - 3 = return data to slitter and calibrate setpoint Array
            ReadMessage = MB.ReadHoldingRegisters(14224, 36, 3);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14225) Method LoadInitialPLCData \n";
            }

            //Read Last Ten Slitters Setpoints and Calibrate Setpoints Starting Register - Number of Registers - 4 = return data to slitter and calibrate setpoint Array
            ReadMessage = MB.ReadHoldingRegisters(14260, 40, 4);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14261) Method LoadInitialPLCData \n";
            }

            //Transfer data from Modbus object to PLc object and seperate data for slitter setpoints and calibrate setpoints
            PLC.PLCReadStptFirst9Registers = MB.SlitterStptsFirstNine;
            PLC.PLCReadStptLast10Registers = MB.SlitterStptsLastTen;
            TM.BladeStpt = PLC.RegisterSlitterStptReadFromPlc();
            TM.BandStpt = PLC.RegisterSlitterStptReadFromPlc();
            TM.CalibrateOffsets = PLC.RegisterSlitterCalibStptReadFromPlc();
            
            //Update Slitter Setpoint Text Boxes
            UpdateSlitterStpt();

            //Read PLC Control Registers R14000 eg. Slitter Auto Positing On. Writes data to PLcControlInputs[] in ModbusComm class
            ReadMessage = MB.ReadHoldingRegisters(13999, 2, 8);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14000) Method TimerCyle1 \n ";

            }
            // PLCControlBits used to use color of Band and Blade Text Box  - Convert single dimensional array to boolean multi-dimensional array
            PLC.PLCControlBits = PLC.CntrlRegisterBitConverter(MB.PLcControlInputs);

            //Read Command Registers Starting Register - Number of Registers - 6 = return data to slitter and calibrate setpoint Array - goes to CommandRegisters
            ReadMessage = MB.ReadHoldingRegisters(14204, 8, 6);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14205) Method LoadInitialPLCData \n";
            }

            //Transfer data from Modbus object to PLc object and convert registers into array bits
            PLC.CmdReg = PLC.CmdRegisterBitConverter(MB.CommandRegisters);
            TM.SlittersSelected = PLC.BitReadForActiveSlitters();
            
            //Update Selection of Slitters
            SelectionOfSlittersLabels();

            //Check to see if out of service bits are on in plc during application startup
            TM.SlitterOutofService = PLC.BitReadOutofServiceCmd();
            InitialSlittersOutOfServ();

            //Start TCPServerSocet for listening for orders from WrapMation
            TCPServSocket.StartListening();

            //Any communication failure to plc then start Timer 2.  This will give message and allow to retry communications
            if (CommunicatonPLCFailure == false)
            {
                Tmr1.Start();
            }
            else
            {
                Tmr2.Start();
            }
            //Accept Button is yellow because of shrinkage changing on startup.  Put color back.
            AcceptBtn.Background = Brushes.Transparent;
        }
        #endregion

        #region Timer Event Processors
        private void TimerEventProcessor1(Object myObject,
                                        EventArgs myEventArgs)
        {
            if (TCPServSocket.SortRequest == true)
            {
                WrapOrderInit();
                // Put calibration buttons to hidden after order received
                LoadParambtn.Visibility = Visibility.Hidden;
                TransferOffsetsToPLCBtn.Visibility = Visibility.Hidden;
                DiagChkBx.IsChecked = false;
                //Disable Out of Tolerance code when order is sent from Wrapmation
                OutofToleranceDisable = true;
            }
            TCPServSocket.SortRequest = false;
            TimerCycle1();
           
        }

        private void TimerEventProcessor2(Object myObject,
                                           EventArgs myEventArgs)
        {
            MB.ModbusDispose();
            
            MessageBoxResult result = System.Windows.MessageBox.Show("RX3i Communication Failure \"10.10.10.110\"?", "Rx3i", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    ModbusMessageTxtBx.Text = "";
                    ModbusMessageTxtBx.Foreground = Brushes.Green;
                    OpenModbusConnection();
                    TimerCycle1();
                    Tmr2.Stop();
                    break;
                case MessageBoxResult.No:
                    ModbusMessageTxtBx.Text = "Ping 10.10.10.110 to Verify Communication";
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    break;
                case MessageBoxResult.Cancel:
                    ModbusMessageTxtBx.Text = "Ping 10.10.10.110 to Verify Communication";
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    break;
            }
            
        }
        #endregion

        #region Timer Cycle 1
        private void TimerCycle1()
        {
            dt = DateTime.Now;
            DateTimeTxtBx.Text = dt.ToString("f");

            SelectionOfSlittersLabelTextColor();

            // Stop timer while reading Registers
            Tmr1.Stop();
            SlitterPositionColorCntrl();
            UpdateSlitterError();

            String ConnectedMessage = "Connected";
            String ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            

            
            //Read First Nine Slitter Positions- Starting Register - Number of Registers and 1 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14024, 36, 1);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14025) Method TimerCyle1 \n ";
                

            }

            //Read Next Ten Slitters Position Starting Register - Number of Registers and 1 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14060, 40, 2);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14061) Method TimerCyle1 \n ";
            }

            //Transfer data from Modbus object to PLc object and seperate data for slitter position feedback
            PLC.PLCReadPositionFirst9Slitters = MB.SlitterPositionFirstNine;
            PLC.PLCReadPositionLast10Slitters = MB.SlitterPositionLastTen;
            TM.BladeActPosn = PLC.RegisterFormatForSlitterBladePositon();
            TM.BandActPosn = PLC.RegisterFormatForSlitterBandPositon();
            //Update Slitter Position Text Boxes
            UpdateSlitterPosition();

            //Read Fault Register - Starting Register - Number of Registers and 5 = return data to Fault Array
            ReadMessage = MB.ReadHoldingRegisters(14149, 9, 5);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14150) Method TimerCyle1 \n ";
                
            }

            //Update Fault table on Main Window
            UpdateFltMsgListBox();
            //**************************NEW
            //Read Command Registers Starting Register - Number of Registers - 6 = return data to slitter and calibrate setpoint Array - goes to CommandRegisters
            ReadMessage = MB.ReadHoldingRegisters(14204, 8, 6);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14205) Method LoadInitialPLCData \n";
            }

            //Transfer data from Modbus object to PLc object and convert registers into array bits
            PLC.CmdReg = PLC.CmdRegisterBitConverter(MB.CommandRegisters);
            TM.SlittersSelectedPLC = PLC.BitReadForActiveSlitters();
                        
            //Check to see if out of service bits are on in plc during application startup
            TM.SlitterOutofServicePLC = PLC.BitReadOutofServiceCmd();

            //*****************************New

            //Read PLC Control Registers R14000 eg. Slitter Auto Positing On. Writes data to PLcControlInputs[] in ModbusComm class
            ReadMessage = MB.ReadHoldingRegisters(13999, 2, 8);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14000) Method TimerCyle1 \n ";

            }
            // PLCControlBits used to use color of Band and Blade Text Box  Use different bit  this bit flashes.
            PLC.PLCControlBits = PLC.CntrlRegisterBitConverter(MB.PLcControlInputs);
            if (PLC.PLCControlBits[0,4])
            {
                BandTxtBx.Background = Brushes.LightGreen;
                BladeTxtBx.Background = Brushes.LightGreen;
                TM.InPosWindow = 0.125;
                
            }
            else
            {
                BandTxtBx.Background = Brushes.Transparent;
                BladeTxtBx.Background = Brushes.Transparent;
                TM.InPosWindow = 1.00;
                
            }
            //Auto Position Detected Toggles on and off when positioning. [0,1] = Auto Positioning, pusles on and off - [0,4] STF to VFD On
            if (PLC.PLCControlBits[0, 4])
            {
                OrderInfoLbl.Background = Brushes.GreenYellow;
                OrderInfoLbl.Content = "Positioning";
            }
            else 
            {
                OrderInfoLbl.Background = Brushes.Transparent;
                OrderInfoLbl.Content = "Order Number";
            }

            //Write PLC Control Writes to PLCWrites in MB
            ReadMessage = MB.ReadHoldingRegisters(14199, 2, 9);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14200) Method TimerCyle1 \n ";

            }
            PLC.PLCWriteControlBits = PLC.CntrlRegisterBitConverter(MB.PLCWrites);
            //Reset bit is detected.  Clear Reset Bit
            if (PLC.PLCWriteControlBits[0, 10])
            {
                ReadMessage = MB.WriteSingleRegiste(14199, 0);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                CommandWritten = true;
                if (CmpResult == false && CommunicatonPLCFailure == false)
                {
                    MB.ModbusCloseConnection();
                    CommunicatonPLCFailure = true;
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14200) Method TimerCycle1 \n ";

                }
            }
            
            //Update Slitter Error Text Boxes if DiagOn
            
            //Run Acknowledgement from PLC  R14000 bit 15  Used in UpdateSlitterError()
            if (PLC.PLCControlBits[0, 15] )
            {
                PLC.WinderRunning = true;
                
            }
            else
            {
                PLC.WinderRunning = false;
                
               
            }
            // If RollWidth is out of 2.00 mm tolerance send command to plc to prevent thread for next set.
            if(PLC.WinderRunning && !OutOfTolerance && !OutofToleranceDisable)
            {
                OutOfTolerance = TM.RollWidthCheck();
                if (OutOfTolerance)
                {
                    SelectionOfSlittersLabelsBorder();
                    SlitterPLCWrites(2048);
                }
            }

            //Read LifeCycle Register - Starting Register - Number of Registers and 7 = return data to SlitterLifeCycle in ModbusComm
            ReadMessage  = MB.ReadHoldingRegisters(14100, 38, 7);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14300) Method TimerCyle1 \n ";
                
            }
            // Transfer Modbus Registers to RollParam
            RollParam.SlitterLifeCycle = MB.SlitterLifeCycle;
               
            if (RollParam.LifeCycleReset && TimeSlice == 0)
            {
                //Write Slitter LifeCycle Resets to PLC - Starting Register - Number of Registers 
                ReadMessage = MB.WriteMultipleRegisters(14196, RollParam.SlitterLifeCycleResets, 3);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                if (CmpResult == false && CommunicatonPLCFailure == false)
                {
                    MB.ModbusCloseConnection();
                    CommunicatonPLCFailure = true;
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14196) Method SlitterLifeCycleResets \n ";
                }
                TimeSlice = 1;
            }

            if (RollParam.LifeCycleReset)
            {
                TimeSlice = TimeSlice + 1;
            }

            if (RollParam.LifeCycleReset == true && TimeSlice > 4)
            {
                RollParam.SlitterLifeCycleResets[0] = 0;
                RollParam.SlitterLifeCycleResets[1] = 0;
                RollParam.SlitterLifeCycleResets[2] = 0;
                RollParam.LifeCycleReset = false;


                //Write Slitter LifeCycle Resets to PLC - Starting Register - Number of Registers Zero out Resets
                ReadMessage = MB.WriteMultipleRegisters(14196, RollParam.SlitterLifeCycleResets, 3);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                if (CmpResult == false && CommunicatonPLCFailure == false)
                {
                    MB.ModbusCloseConnection();
                    CommunicatonPLCFailure = true;
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14196) Method SlitterLifeCycleResets \n ";
                }

                TimeSlice = 0;
            }

            if (CommunicatonPLCFailure == false)
            {
                Tmr1.Start();
            }
            else
            {
                
                Tmr2.Start();
            }
        }
        #endregion

        #region Wrap Order Calculatons on Order Sent

        private void WrapOrderInit()
        {
            
            TM.ZeroOutWrapData();
            TM.ZeroOutSlitterData();
            TM.WrapData = DataSort.GetData();
            OrderInfoTxtBx.Text = TCPServSocket.wraptext;
            OrderNoTxtBx.Text = SortData.OrderNumber;
            OrderNoTxtBx.Background = Brushes.Transparent;
            TM.CalcRollParam();
            UpdateRollTextBox();
            UpdateRollShrinkTxt();
            ShrkWidthTxtBx.Text = TM.TotalWidth.ToString("F2");
            OrderWidthTxtBx.Text = TM.TotalWidthNoShrk.ToString("F2");
            NumbOfRollsTxt.Text = Convert.ToString(TM.NumbOfRolls);
            if (TM.TotalWidth > TM.MaxWidth)
            {
                System.Windows.MessageBox.Show("Maximum Trim Exceeded");
            }
            // Select Slittes Available for Cut fro True False Grid on Form6
            TM.SelectSlittersForCuts();
            // Calculate using Park Position - Select Slitter for Cut.  Populates SolutionSelectPark
            TM.SelectslitterforCutsParkPosn();

            // Calculate using Acutal Slitter Positin - Select Slitter for Cut. Populates SolutionSelectAct
            TM.SelectSitterforCutsActPosn();

            // Calculate using Band Upper Limits  - Select Slitter for Cut Populates  SolutionSelectParkLmt
            TM.SelectSitterforCutsParkLimit();

            //Checks to ses if selected slitters matches number of cuts needed
            TM.CompareSlittersUsedToCuts();

            //Select Best Solution from methods TotalActPosnMovement() and TotalParkMovement()
            TM.SlitCutsUsedToRollCuts();

            //Calculate Setpoints for Slitters selected for cuts
            TM.CalcSelectedBandStpts();

            // Calculate Setpoints for Slitters not selected for cuts
            TM.CalcBandStptsNotUsed();

            // Verify Slitters are minimum distance from each other (153.00 mm)
            SlitStptVerified = TM.VerifySlitterSetpoints();
            if (!SlitStptVerified)
            {
                TM.SlitCutsUsedToRollCuts();
                TM.CalcSelectedBandStpts();
                TM.CalcBandStptsNotUsed();
            }


            TM.BladeStpt = TM.BandStpt;
            UpdateSlitterStpt();
            RemainingTrimTxtBx.Text = (TM.MaxWidth - TM.TotalWidth).ToString("F2"); ;
            TotalParkSlitTxt.Text = Convert.ToString(TM.NumbOfSlitParkSelected);
            TotalParkSlitLimitTxt.Text = Convert.ToString(TM.NumbOfslitParkSelectdLmt);
            TotalActSlitTxt.Text = Convert.ToString(TM.NumbOfSlitActPosSelected);
            DSTrimPosnTxtBx.Text = ((TM.MaxWidth - TM.TotalWidth) / 2).ToString("F2");
            TSTrimPosnTxtBx.Text = ((TM.MaxWidth - TM.TotalWidth) / 2).ToString("F2");
            SelectionOfSlittersLabels();
            PLC.BitAssignmentForActiveSlitters(TM.SlittersSelected);
            TM.CalibrateMode = false;
            DiagReset();
            CenterTrimOn = true;

        }
            
        #endregion

        #region Slitter PLC Writes to Rx3i
        private void SlitterPLCWrites(Int16 numb)
        {
            Tmr1.Stop();

            PLC.PLCWriteStptFirst9Registers = PLC.RegisterFormatFirst9SlitterStpt(TM.CalibrateOffsets, TM.BandStpt);
            PLC.PLCWriteStptLast10Registers = PLC.RegisterFormatLast10SlitterStpt(TM.CalibrateOffsets, TM.BandStpt);


            String ConnectedMessage = "Connected";
            String ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            

                //Write First Nine Slitter Calibrate Offsets and Slitter Setpoints- Starting Register - Number of Registers 
                ReadMessage = MB.WriteMultipleRegisters(14224, PLC.PLCWriteStptFirst9Registers, 36);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                    if (CmpResult == false && CommunicatonPLCFailure == false)
                    {
                        MB.ModbusCloseConnection();
                        CommunicatonPLCFailure = true;
                        ModbusMessageTxtBx.Foreground = Brushes.Red;
                        ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14225) Method SlitterPLCWrites \n ";
                    }

                //Write Last Ten Slitter Calibrate Offsets and Slitter Setpoints- Starting Register - Number of Registers 
                ReadMessage = MB.WriteMultipleRegisters(14260, PLC.PLCWriteStptLast10Registers, 40);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                    if (CmpResult == false && CommunicatonPLCFailure == false)
                    {
                         MB.ModbusCloseConnection();
                         CommunicatonPLCFailure = true;
                         ModbusMessageTxtBx.Foreground = Brushes.Red;
                         ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14261) Method SlitterPLCWrites \n ";
                    }
            // Pack Slitter Selected, CalibrateCmdSelected and SlitterOutofService into CmdWriteRegisters
            if (MaintMode)
            {
                PLC.BitAssignmentForActiveSlitters(TM.ManualSelect);
            }
            else
            {
                PLC.BitAssignmentForActiveSlitters(TM.SlittersSelected);
            }
              
                PLC.BitAssignmentForCalibrateCmd(TM.CalibrateCmdSelected);
                PLC.BitAssignmentForOutofServiceCmd(TM.SlitterOutofService);
                PLC.CmdWriteRegisters = PLC.BitRegConvertor(PLC.CmdReg);

            

            //Write Commands to PLC - Selected Slitters - Calibrate Cmd - Out of Service Cmd - Starting Register - Number of Registers 
            ReadMessage = MB.WriteMultipleCmdRegisters(14204, PLC.CmdWriteRegisters, 8);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                    if (CmpResult == false && CommunicatonPLCFailure == false)
                    {
                        MB.ModbusCloseConnection();
                        CommunicatonPLCFailure = true;
                        ModbusMessageTxtBx.Foreground = Brushes.Red;
                        ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14205) Method SlitterPLCWrites \n ";
                
                    }
            // Zero out Slitter Calibrate Commands
            if (TM.CmdOffsetChgd == true || TM.SlitOutOfServDetect == true)
            {
                for (int i = 0; i < TM.MaxSlitters; i++)
                {
                    TM.CalibrateCmdSelected[i] = false;
                    TM.SlitterCalibTextChgd[i] = false;
                }
                TM.CmdOffsetChgd = false;
                TM.SlitOutOfServDetect = false;

            }

            //Command Register gets zeroed out i TimerCylce1()
            //Write Command Registers for Slitter Assignment, Auto Position Slitters Cmd, Position Core Chucks Cmd and Fault Reset Command 97 = New Assignment, Auto Position Command, Send CoreChucks
            ReadMessage = MB.WriteSingleRegiste(14199, numb);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            CommandWritten = true;
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14200) Method SlitterPLCWrites \n ";

            }

            OutofToleranceDisable = false;
            ZeroOutPLCCommands();

            if (CommunicatonPLCFailure == false)
            {
                Tmr1.Start();
            }
            else
            {
                Tmr2.Start();
            }

        }

        #endregion

        #region Zero out Slitter Write Commands
        private void ZeroOutPLCCommands()
        {
            String ConnectedMessage = "Connected";
            String ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            Int16 numb = 0;
            //Write Command Registers for Slitter Assignment, Auto Position Slitters Cmd, Position Core Chucks Cmd and Fault Reset Command  ****Zeros out Command Register*****

            if (CommandWritten)
            {
                ReadMessage = MB.WriteSingleRegiste(14199, numb);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                if (CmpResult == false && CommunicatonPLCFailure == false)
                {
                    MB.ModbusCloseConnection();
                    CommunicatonPLCFailure = true;
                    ModbusMessageTxtBx.Foreground = Brushes.Red;
                    ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14200) Method ZeroOutPLCComands \n ";
                    //MessageBox.Show("14200, Connection Closed " + ReadMessage);
                }
                CommandWritten = false;
            }

            // Zero out Commands that applicable
            PLC.BitAssignmentForActiveSlitters(TM.SlittersSelected);
            PLC.BitAssignmentForCalibrateCmd(TM.CalibrateCmdSelected);
            PLC.BitAssignmentForOutofServiceCmd(TM.SlitterOutofService);
            PLC.CmdWriteRegisters = PLC.BitRegConvertor(PLC.CmdReg);

            //Write Commands to PLC - Selected Slitters - Calibrate Cmd - Out of Service Cmd - Starting Register - Number of Registers 
            ReadMessage = MB.WriteMultipleCmdRegisters(14204, PLC.CmdWriteRegisters, 8);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14205) Method ZeroOutPLCComands \n ";

            }
                       

        }
        #endregion

        #region Update Fault Message List Box
        private void UpdateFltMsgListBox()
        {
            //Nine Registers for faults
            UInt16[] mess = new UInt16[9];
            //
            for (int k = 0; k < 9; k++)
            {
               mess[k] = (UInt16)MB.FaultPLCMessages[k];
            }
            // Multi-dimensional bool array for function return
            bool[,] FltArrList = new bool[9, 16];
            // Pass Registers to BitConvertor and return multi-dimensional bool array
            FltArrList = FM.FltMessBitConverter(mess);
            // delcare array for messages that are needed
            bool[] MsgList = new bool[144];
            // call MsgChk and returns which messages to add to listbox.  Uses mask to prevent repeatable alarms
            MsgList = FM.MsgChk(FltArrList);
            // delclare string array for messae list
            String[] MsgListArr = new String[144];
            // Retrieve message list
            MsgListArr = FM.RetrieveFaultMessage();
            //loop through an add messages to listbox.
            for (int x = 0; x < 144; x++)
            {
                if (MsgList[x])
                {
                    FltMsgListBx.Items.Add(MsgListArr[x]);
                }
            }
        }
        #endregion

        #region Update Rollwidth Text Boxes
        private void UpdateRollTextBox()
        {
            this.Roll1.Text = TM.WrapData[0].ToString("F2");
            this.Roll2.Text = TM.WrapData[1].ToString("F2");
            this.Roll3.Text = TM.WrapData[2].ToString("F2");
            this.Roll4.Text = TM.WrapData[3].ToString("F2");
            this.Roll5.Text = TM.WrapData[4].ToString("F2");
            this.Roll6.Text = TM.WrapData[5].ToString("F2");
            this.Roll7.Text = TM.WrapData[6].ToString("F2");
            this.Roll8.Text = TM.WrapData[7].ToString("F2");
            this.Roll9.Text = TM.WrapData[8].ToString("F2");
            this.Roll10.Text = TM.WrapData[9].ToString("F2");
            this.Roll11.Text = TM.WrapData[10].ToString("F2");
            this.Roll12.Text = TM.WrapData[11].ToString("F2");
            this.Roll13.Text = TM.WrapData[12].ToString("F2");
            this.Roll14.Text = TM.WrapData[13].ToString("F2");
            this.Roll15.Text = TM.WrapData[14].ToString("F2");
            this.Roll16.Text = TM.WrapData[15].ToString("F2");
            this.Roll17.Text = TM.WrapData[16].ToString("F2");
            this.Roll18.Text = TM.WrapData[17].ToString("F2");
        }
        #endregion 

        #region Update Rollwidth with Shrinkage added Text Boxes
        private void UpdateRollShrinkTxt()
        {
            this.Roll1ShrinkTxt.Text = TM.RollWidth[0].ToString("F");
            this.Roll2ShrinkTxt.Text = TM.RollWidth[1].ToString("F");
            this.Roll3ShrinkTxt.Text = TM.RollWidth[2].ToString("F");
            this.Roll4ShrinkTxt.Text = TM.RollWidth[3].ToString("F");
            this.Roll5ShrinkTxt.Text = TM.RollWidth[4].ToString("F");
            this.Roll6ShrinkTxt.Text = TM.RollWidth[5].ToString("F");
            this.Roll7ShrinkTxt.Text = TM.RollWidth[6].ToString("F");
            this.Roll8ShrinkTxt.Text = TM.RollWidth[7].ToString("F");
            this.Roll9ShrinkTxt.Text = TM.RollWidth[8].ToString("F");
            this.Roll10ShrinkTxt.Text = TM.RollWidth[9].ToString("F");
            this.Roll11ShrinkTxt.Text = TM.RollWidth[10].ToString("F");
            this.Roll12ShrinkTxt.Text = TM.RollWidth[11].ToString("F");
            this.Roll13ShrinkTxt.Text = TM.RollWidth[12].ToString("F");
            this.Roll14ShrinkTxt.Text = TM.RollWidth[13].ToString("F");
            this.Roll15ShrinkTxt.Text = TM.RollWidth[14].ToString("F");
            this.Roll16ShrinkTxt.Text = TM.RollWidth[15].ToString("F");
            this.Roll17ShrinkTxt.Text = TM.RollWidth[16].ToString("F");
            this.Roll18ShrinkTxt.Text = TM.RollWidth[17].ToString("F");


        }
        #endregion

        #region Update Sliiter Setpoints Text Boxes
        private void UpdateSlitterStpt()
        {
            this.BandStpt1.Text = TM.BandStpt[0].ToString("F2");
            this.BandStpt2.Text = TM.BandStpt[1].ToString("F2");
            this.BandStpt3.Text = TM.BandStpt[2].ToString("F2");
            this.BandStpt4.Text = TM.BandStpt[3].ToString("F2");
            this.BandStpt5.Text = TM.BandStpt[4].ToString("F2");
            this.BandStpt6.Text = TM.BandStpt[5].ToString("F2");
            this.BandStpt7.Text = TM.BandStpt[6].ToString("F2");
            this.BandStpt8.Text = TM.BandStpt[7].ToString("F2");
            this.BandStpt9.Text = TM.BandStpt[8].ToString("F2");
            this.BandStpt10.Text = TM.BandStpt[9].ToString("F2");
            this.BandStpt11.Text = TM.BandStpt[10].ToString("F2");
            this.BandStpt12.Text = TM.BandStpt[11].ToString("F2");
            this.BandStpt13.Text = TM.BandStpt[12].ToString("F2");
            this.BandStpt14.Text = TM.BandStpt[13].ToString("F2");
            this.BandStpt15.Text = TM.BandStpt[14].ToString("F2");
            this.BandStpt16.Text = TM.BandStpt[15].ToString("F2");
            this.BandStpt17.Text = TM.BandStpt[16].ToString("F2");
            this.BandStpt18.Text = TM.BandStpt[17].ToString("F2");
            this.BandStpt19.Text = TM.BandStpt[18].ToString("F2");
            this.BladeStpt1.Text = TM.BladeStpt[0].ToString("F2");
            this.BladeStpt2.Text = TM.BladeStpt[1].ToString("F2");
            this.BladeStpt3.Text = TM.BladeStpt[2].ToString("F2");
            this.BladeStpt4.Text = TM.BladeStpt[3].ToString("F2");
            this.BladeStpt5.Text = TM.BladeStpt[4].ToString("F2");
            this.BladeStpt6.Text = TM.BladeStpt[5].ToString("F2");
            this.BladeStpt7.Text = TM.BladeStpt[6].ToString("F2");
            this.BladeStpt8.Text = TM.BladeStpt[7].ToString("F2");
            this.BladeStpt9.Text = TM.BladeStpt[8].ToString("F2");
            this.BladeStpt10.Text = TM.BladeStpt[9].ToString("F2");
            this.BladeStpt11.Text = TM.BladeStpt[10].ToString("F2");
            this.BladeStpt12.Text = TM.BladeStpt[11].ToString("F2");
            this.BladeStpt13.Text = TM.BladeStpt[12].ToString("F2");
            this.BladeStpt14.Text = TM.BladeStpt[13].ToString("F2");
            this.BladeStpt15.Text = TM.BladeStpt[14].ToString("F2");
            this.BladeStpt16.Text = TM.BladeStpt[15].ToString("F2");
            this.BladeStpt17.Text = TM.BladeStpt[16].ToString("F2");
            this.BladeStpt18.Text = TM.BladeStpt[17].ToString("F2");
            this.BladeStpt19.Text = TM.BladeStpt[18].ToString("F2");
        }
        #endregion

        #region Update Slitter Positions
        private void UpdateSlitterPosition()
        {

            Blade1Posn.Text = TM.BladeActPosn[0].ToString("F2");
            Band1Posn.Text = TM.BandActPosn[0].ToString("F2");
            Blade2Posn.Text = TM.BladeActPosn[1].ToString("F2");
            Band2Posn.Text = TM.BandActPosn[1].ToString("F2");
            Blade3Posn.Text = TM.BladeActPosn[2].ToString("F2");
            Band3Posn.Text = TM.BandActPosn[2].ToString("F2");
            Blade4Posn.Text = TM.BladeActPosn[3].ToString("F2");
            Band4Posn.Text = TM.BandActPosn[3].ToString("F2");
            Blade5Posn.Text = TM.BladeActPosn[4].ToString("F2");
            Band5Posn.Text = TM.BandActPosn[4].ToString("F2");
            Blade6Posn.Text = TM.BladeActPosn[5].ToString("F2");
            Band6Posn.Text = TM.BandActPosn[5].ToString("F2");
            Blade7Posn.Text = TM.BladeActPosn[6].ToString("F2");
            Band7Posn.Text = TM.BandActPosn[6].ToString("F2");
            Blade8Posn.Text = TM.BladeActPosn[7].ToString("F2");
            Band8Posn.Text = TM.BandActPosn[7].ToString("F2");
            Blade9Posn.Text = TM.BladeActPosn[8].ToString("F2");
            Band9Posn.Text = TM.BandActPosn[8].ToString("F2");
            Blade10Posn.Text = TM.BladeActPosn[9].ToString("F2");
            Band10Posn.Text = TM.BandActPosn[9].ToString("F2");
            Blade11Posn.Text = TM.BladeActPosn[10].ToString("F2");
            Band11Posn.Text = TM.BandActPosn[10].ToString("F2");
            Blade12Posn.Text = TM.BladeActPosn[11].ToString("F2");
            Band12Posn.Text = TM.BandActPosn[11].ToString("F2");
            Blade13Posn.Text = TM.BladeActPosn[12].ToString("F2");
            Band13Posn.Text = TM.BandActPosn[12].ToString("F2");
            Blade14Posn.Text = TM.BladeActPosn[13].ToString("F2");
            Band14Posn.Text = TM.BandActPosn[13].ToString("F2");
            Blade15Posn.Text = TM.BladeActPosn[14].ToString("F2");
            Band15Posn.Text = TM.BandActPosn[14].ToString("F2");
            Blade16Posn.Text = TM.BladeActPosn[15].ToString("F2");
            Band16Posn.Text = TM.BandActPosn[15].ToString("F2");
            Blade17Posn.Text = TM.BladeActPosn[16].ToString("F2");
            Band17Posn.Text = TM.BandActPosn[16].ToString("F2");
            Blade18Posn.Text = TM.BladeActPosn[17].ToString("F2");
            Band18Posn.Text = TM.BandActPosn[17].ToString("F2");
            Blade19Posn.Text = TM.BladeActPosn[18].ToString("F2");
            Band19Posn.Text = TM.BandActPosn[18].ToString("F2");
        }
        #endregion

        #region Update Slitter Error Text Boxes
        private void UpdateSlitterError()
        {

            // If winder is stopped, clear Excessive Error  Excessive Error wil trigger Flt Message FLT3027   This logic prevents winder going to thread mode.

            Tmr1.Stop();
              
            Band1Err.Text = (TM.BandStpt[0] - TM.BandActPosn[0]).ToString("F2");
            Band2Err.Text = (TM.BandStpt[1] - TM.BandActPosn[1]).ToString("F2");
            Band3Err.Text = (TM.BandStpt[2] - TM.BandActPosn[2]).ToString("F2");
            Band4Err.Text = (TM.BandStpt[3] - TM.BandActPosn[3]).ToString("F2");
            Band5Err.Text = (TM.BandStpt[4] - TM.BandActPosn[4]).ToString("F2");
            Band6Err.Text = (TM.BandStpt[5] - TM.BandActPosn[5]).ToString("F2");
            Band7Err.Text = (TM.BandStpt[6] - TM.BandActPosn[6]).ToString("F2");
            Band8Err.Text = (TM.BandStpt[7] - TM.BandActPosn[7]).ToString("F2");
            Band9Err.Text = (TM.BandStpt[8] - TM.BandActPosn[8]).ToString("F2");
            Band10Err.Text = (TM.BandStpt[9] - TM.BandActPosn[9]).ToString("F2");
            Band11Err.Text = (TM.BandStpt[10] - TM.BandActPosn[10]).ToString("F2");
            Band12Err.Text = (TM.BandStpt[11] - TM.BandActPosn[11]).ToString("F2");
            Band13Err.Text = (TM.BandStpt[12] - TM.BandActPosn[12]).ToString("F2");
            Band14Err.Text = (TM.BandStpt[13] - TM.BandActPosn[13]).ToString("F2");
            Band15Err.Text = (TM.BandStpt[14] - TM.BandActPosn[14]).ToString("F2");
            Band16Err.Text = (TM.BandStpt[15] - TM.BandActPosn[15]).ToString("F2");
            Band17Err.Text = (TM.BandStpt[16] - TM.BandActPosn[16]).ToString("F2");
            Band18Err.Text = (TM.BandStpt[17] - TM.BandActPosn[17]).ToString("F2");
            Band19Err.Text = (TM.BandStpt[18] - TM.BandActPosn[18]).ToString("F2");

            Blade1Err.Text = (TM.BladeStpt[0] - TM.BladeActPosn[0]).ToString("F2");
            Blade2Err.Text = (TM.BladeStpt[1] - TM.BladeActPosn[1]).ToString("F2");
            Blade3Err.Text = (TM.BladeStpt[2] - TM.BladeActPosn[2]).ToString("F2");
            Blade4Err.Text = (TM.BladeStpt[3] - TM.BladeActPosn[3]).ToString("F2");
            Blade5Err.Text = (TM.BladeStpt[4] - TM.BladeActPosn[4]).ToString("F2");
            Blade6Err.Text = (TM.BladeStpt[5] - TM.BladeActPosn[5]).ToString("F2");
            Blade7Err.Text = (TM.BladeStpt[6] - TM.BladeActPosn[6]).ToString("F2");
            Blade8Err.Text = (TM.BladeStpt[7] - TM.BladeActPosn[7]).ToString("F2");
            Blade9Err.Text = (TM.BladeStpt[8] - TM.BladeActPosn[8]).ToString("F2");
            Blade10Err.Text = (TM.BladeStpt[9] - TM.BladeActPosn[9]).ToString("F2");
            Blade11Err.Text = (TM.BladeStpt[10] - TM.BladeActPosn[10]).ToString("F2");
            Blade12Err.Text = (TM.BladeStpt[11] - TM.BladeActPosn[11]).ToString("F2");
            Blade13Err.Text = (TM.BladeStpt[12] - TM.BladeActPosn[12]).ToString("F2");
            Blade14Err.Text = (TM.BladeStpt[13] - TM.BladeActPosn[13]).ToString("F2");
            Blade15Err.Text = (TM.BladeStpt[14] - TM.BladeActPosn[14]).ToString("F2");
            Blade16Err.Text = (TM.BladeStpt[15] - TM.BladeActPosn[15]).ToString("F2");
            Blade17Err.Text = (TM.BladeStpt[16] - TM.BladeActPosn[16]).ToString("F2");
            Blade18Err.Text = (TM.BladeStpt[17] - TM.BladeActPosn[17]).ToString("F2");
            Blade19Err.Text = (TM.BladeStpt[18] - TM.BladeActPosn[18]).ToString("F2");
            Tmr1.Start();
        }
        #endregion

        #region Slitter Position Error Color Control
        private void SlitterPositionColorCntrl()
        {
            if (((TM.BandStpt[0] + TM.InPosWindow) >= TM.BandActPosn[0]) && ((TM.BandStpt[0] - TM.InPosWindow) <= TM.BandActPosn[0]) && TM.SlittersSelected[0])
            { Band1Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[0] + TM.UnSelectedPosWindow) >= TM.BandActPosn[0]) && ((TM.BandStpt[0] - TM.UnSelectedPosWindow) <= TM.BandActPosn[0]) && !TM.SlittersSelected[0])
            { Band1Posn.Background = Brushes.LightGreen; }
            else { Band1Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[1] + TM.InPosWindow) >= TM.BandActPosn[1]) && ((TM.BandStpt[1] - TM.InPosWindow) <= TM.BandActPosn[1]) && TM.SlittersSelected[1])
            { Band2Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[1] + TM.UnSelectedPosWindow) >= TM.BandActPosn[1]) && ((TM.BandStpt[1] - TM.UnSelectedPosWindow) <= TM.BandActPosn[1]) && !TM.SlittersSelected[1])
            { Band2Posn.Background = Brushes.LightGreen; }
            else { Band2Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[2] + TM.InPosWindow) >= TM.BandActPosn[2]) && ((TM.BandStpt[2] - TM.InPosWindow) <= TM.BandActPosn[2]) && TM.SlittersSelected[2])
            { Band3Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[2] + TM.UnSelectedPosWindow) >= TM.BandActPosn[2]) && ((TM.BandStpt[2] - TM.UnSelectedPosWindow) <= TM.BandActPosn[2]) && !TM.SlittersSelected[2])
            { Band3Posn.Background = Brushes.LightGreen; }
            else { Band3Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[3] + TM.InPosWindow) >= TM.BandActPosn[3]) && ((TM.BandStpt[3] - TM.InPosWindow) <= TM.BandActPosn[3]) && TM.SlittersSelected[3])
            { Band4Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[3] + TM.UnSelectedPosWindow) >= TM.BandActPosn[3]) && ((TM.BandStpt[3] - TM.UnSelectedPosWindow) <= TM.BandActPosn[3]) && !TM.SlittersSelected[3])
            { Band4Posn.Background = Brushes.LightGreen; }
            else { Band4Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[4] + TM.InPosWindow) >= TM.BandActPosn[4]) && ((TM.BandStpt[4] - TM.InPosWindow) <= TM.BandActPosn[4]) && TM.SlittersSelected[4])
            { Band5Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[4] + TM.UnSelectedPosWindow) >= TM.BandActPosn[4]) && ((TM.BandStpt[4] - TM.UnSelectedPosWindow) <= TM.BandActPosn[4]) && !TM.SlittersSelected[4])
            { Band5Posn.Background = Brushes.LightGreen; }
            else { Band5Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[5] + TM.InPosWindow) >= TM.BandActPosn[5]) && ((TM.BandStpt[5] - TM.InPosWindow) <= TM.BandActPosn[5]) && TM.SlittersSelected[5])
            { Band6Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[5] + TM.UnSelectedPosWindow) >= TM.BandActPosn[5]) && ((TM.BandStpt[5] - TM.UnSelectedPosWindow) <= TM.BandActPosn[5]) && !TM.SlittersSelected[5])
            { Band6Posn.Background = Brushes.LightGreen; }
            else { Band6Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[6] + TM.InPosWindow) >= TM.BandActPosn[6]) && ((TM.BandStpt[6] - TM.InPosWindow) <= TM.BandActPosn[6]) && TM.SlittersSelected[6])
            { Band7Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[6] + TM.UnSelectedPosWindow) >= TM.BandActPosn[6]) && ((TM.BandStpt[6] - TM.UnSelectedPosWindow) <= TM.BandActPosn[6]) && !TM.SlittersSelected[6])
            { Band7Posn.Background = Brushes.LightGreen; }
            else { Band7Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[7] + TM.InPosWindow) >= TM.BandActPosn[7]) && ((TM.BandStpt[7] - TM.InPosWindow) <= TM.BandActPosn[7]) && TM.SlittersSelected[7])
            { Band8Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[7] + TM.UnSelectedPosWindow) >= TM.BandActPosn[7]) && ((TM.BandStpt[7] - TM.UnSelectedPosWindow) <= TM.BandActPosn[7]) && !TM.SlittersSelected[7])
            { Band8Posn.Background = Brushes.LightGreen; }
            else { Band8Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[8] + TM.InPosWindow) >= TM.BandActPosn[8]) && ((TM.BandStpt[8] - TM.InPosWindow) <= TM.BandActPosn[8]) && TM.SlittersSelected[8])
            { Band9Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[8] + TM.UnSelectedPosWindow) >= TM.BandActPosn[8]) && ((TM.BandStpt[8] - TM.UnSelectedPosWindow) <= TM.BandActPosn[8]) && !TM.SlittersSelected[8])
            { Band9Posn.Background = Brushes.LightGreen; }
            else { Band9Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[9] + TM.InPosWindow) >= TM.BandActPosn[9]) && ((TM.BandStpt[9] - TM.InPosWindow) <= TM.BandActPosn[9]) && TM.SlittersSelected[9])
            { Band10Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[9] + TM.UnSelectedPosWindow) >= TM.BandActPosn[9]) && ((TM.BandStpt[9] - TM.UnSelectedPosWindow) <= TM.BandActPosn[9]) && !TM.SlittersSelected[9])
            { Band10Posn.Background = Brushes.LightGreen; }
            else { Band10Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[10] + TM.InPosWindow) >= TM.BandActPosn[10]) && ((TM.BandStpt[10] - TM.InPosWindow) <= TM.BandActPosn[10]) && TM.SlittersSelected[10])
            { Band11Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[10] + TM.UnSelectedPosWindow) >= TM.BandActPosn[10]) && ((TM.BandStpt[10] - TM.UnSelectedPosWindow) <= TM.BandActPosn[10]) && !TM.SlittersSelected[10])
            { Band11Posn.Background = Brushes.LightGreen; }
            else { Band11Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[11] + TM.InPosWindow) >= TM.BandActPosn[11]) && ((TM.BandStpt[11] - TM.InPosWindow) <= TM.BandActPosn[11]) && TM.SlittersSelected[11])
            { Band12Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[11] + TM.UnSelectedPosWindow) >= TM.BandActPosn[11]) && ((TM.BandStpt[11] - TM.UnSelectedPosWindow) <= TM.BandActPosn[11]) && !TM.SlittersSelected[11])
            { Band12Posn.Background = Brushes.LightGreen; }
            else { Band12Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[12] + TM.InPosWindow) >= TM.BandActPosn[12]) && ((TM.BandStpt[12] - TM.InPosWindow) <= TM.BandActPosn[12]) && TM.SlittersSelected[12])
            { Band13Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[12] + TM.UnSelectedPosWindow) >= TM.BandActPosn[12]) && ((TM.BandStpt[12] - TM.UnSelectedPosWindow) <= TM.BandActPosn[12]) && !TM.SlittersSelected[12])
            { Band13Posn.Background = Brushes.LightGreen; }
            else { Band13Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[13] + TM.InPosWindow) >= TM.BandActPosn[13]) && ((TM.BandStpt[13] - TM.InPosWindow) <= TM.BandActPosn[13]) && TM.SlittersSelected[13])
            { Band14Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[13] + TM.UnSelectedPosWindow) >= TM.BandActPosn[13]) && ((TM.BandStpt[13] - TM.UnSelectedPosWindow) <= TM.BandActPosn[13]) && !TM.SlittersSelected[13])
            { Band14Posn.Background = Brushes.LightGreen; }
            else { Band14Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[14] + TM.InPosWindow) >= TM.BandActPosn[14]) && ((TM.BandStpt[14] - TM.InPosWindow) <= TM.BandActPosn[14]) && TM.SlittersSelected[14])
            { Band15Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[14] + TM.UnSelectedPosWindow) >= TM.BandActPosn[14]) && ((TM.BandStpt[14] - TM.UnSelectedPosWindow) <= TM.BandActPosn[14]) && !TM.SlittersSelected[14])
            { Band15Posn.Background = Brushes.LightGreen; }
            else { Band15Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[15] + TM.InPosWindow) >= TM.BandActPosn[15]) && ((TM.BandStpt[15] - TM.InPosWindow) <= TM.BandActPosn[15]) && TM.SlittersSelected[15])
            { Band16Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[15] + TM.UnSelectedPosWindow) >= TM.BandActPosn[15]) && ((TM.BandStpt[15] - TM.UnSelectedPosWindow) <= TM.BandActPosn[15]) && !TM.SlittersSelected[15])
            { Band16Posn.Background = Brushes.LightGreen; }
            else { Band16Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[16] + TM.InPosWindow) >= TM.BandActPosn[16]) && ((TM.BandStpt[16] - TM.InPosWindow) <= TM.BandActPosn[16]) && TM.SlittersSelected[16])
            { Band17Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[16] + TM.UnSelectedPosWindow) >= TM.BandActPosn[16]) && ((TM.BandStpt[16] - TM.UnSelectedPosWindow) <= TM.BandActPosn[16]) && !TM.SlittersSelected[16])
            { Band17Posn.Background = Brushes.LightGreen; }
            else { Band17Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[17] + TM.InPosWindow) >= TM.BandActPosn[17]) && ((TM.BandStpt[17] - TM.InPosWindow) <= TM.BandActPosn[17]) && TM.SlittersSelected[17])
            { Band18Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[17] + TM.UnSelectedPosWindow) >= TM.BandActPosn[17]) && ((TM.BandStpt[17] - TM.UnSelectedPosWindow) <= TM.BandActPosn[17]) && !TM.SlittersSelected[17])
            { Band18Posn.Background = Brushes.LightGreen; }
            else { Band18Posn.Background = Brushes.LightCoral; }

            if (((TM.BandStpt[18] + TM.InPosWindow) >= TM.BandActPosn[18]) && ((TM.BandStpt[18] - TM.InPosWindow) <= TM.BandActPosn[18]) && TM.SlittersSelected[18])
            { Band19Posn.Background = Brushes.LightGreen; }
            else if (((TM.BandStpt[18] + TM.UnSelectedPosWindow) >= TM.BandActPosn[18]) && ((TM.BandStpt[18] - TM.UnSelectedPosWindow) <= TM.BandActPosn[18]) && !TM.SlittersSelected[18])
            { Band19Posn.Background = Brushes.LightGreen; }
            else { Band19Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[0] + TM.InPosWindow) >= TM.BladeActPosn[0]) && ((TM.BladeStpt[0] - TM.InPosWindow) <= TM.BladeActPosn[0]) && TM.SlittersSelected[0])
            { Blade1Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[0] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[0]) && ((TM.BladeStpt[0] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[0]) && !TM.SlittersSelected[0])
            { Blade1Posn.Background = Brushes.LightGreen; }
            else { Blade1Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[1] + TM.InPosWindow) >= TM.BladeActPosn[1]) && ((TM.BladeStpt[1] - TM.InPosWindow) <= TM.BladeActPosn[1]) && TM.SlittersSelected[1])
            { Blade2Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[1] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[1]) && ((TM.BladeStpt[1] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[1]) && !TM.SlittersSelected[1])
            { Blade2Posn.Background = Brushes.LightGreen; }
            else { Blade2Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[2] + TM.InPosWindow) >= TM.BladeActPosn[2]) && ((TM.BladeStpt[2] - TM.InPosWindow) <= TM.BladeActPosn[2]) && TM.SlittersSelected[2])
            { Blade3Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[2] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[2]) && ((TM.BladeStpt[2] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[2]) && !TM.SlittersSelected[2])
            { Blade3Posn.Background = Brushes.LightGreen; }
            else { Blade3Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[3] + TM.InPosWindow) >= TM.BladeActPosn[3]) && ((TM.BladeStpt[3] - TM.InPosWindow) <= TM.BladeActPosn[3]) && TM.SlittersSelected[3])
            { Blade4Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[3] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[3]) && ((TM.BladeStpt[3] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[3]) && !TM.SlittersSelected[3])
            { Blade4Posn.Background = Brushes.LightGreen; }
            else { Blade4Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[4] + TM.InPosWindow) >= TM.BladeActPosn[4]) && ((TM.BladeStpt[4] - TM.InPosWindow) <= TM.BladeActPosn[4]) && TM.SlittersSelected[4])
            { Blade5Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[4] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[4]) && ((TM.BladeStpt[4] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[4]) && !TM.SlittersSelected[4])
            { Blade5Posn.Background = Brushes.LightGreen; }
            else { Blade5Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[5] + TM.InPosWindow) >= TM.BladeActPosn[5]) && ((TM.BladeStpt[5] - TM.InPosWindow) <= TM.BladeActPosn[5]) && TM.SlittersSelected[5])
            { Blade6Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[5] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[5]) && ((TM.BladeStpt[5] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[5]) && !TM.SlittersSelected[5])
            { Blade6Posn.Background = Brushes.LightGreen; }
            else { Blade6Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[6] + TM.InPosWindow) >= TM.BladeActPosn[6]) && ((TM.BladeStpt[6] - TM.InPosWindow) <= TM.BladeActPosn[6]) && TM.SlittersSelected[6])
            { Blade7Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[6] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[6]) && ((TM.BladeStpt[6] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[6]) && !TM.SlittersSelected[6])
            { Blade7Posn.Background = Brushes.LightGreen; }
            else { Blade7Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[7] + TM.InPosWindow) >= TM.BladeActPosn[7]) && ((TM.BladeStpt[7] - TM.InPosWindow) <= TM.BladeActPosn[7]) && TM.SlittersSelected[7])
            { Blade8Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[7] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[7]) && ((TM.BladeStpt[7] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[7]) && !TM.SlittersSelected[7])
            { Blade8Posn.Background = Brushes.LightGreen; }
            else { Blade8Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[8] + TM.InPosWindow) >= TM.BladeActPosn[8]) && ((TM.BladeStpt[8] - TM.InPosWindow) <= TM.BladeActPosn[8]) && TM.SlittersSelected[8])
            { Blade9Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[8] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[8]) && ((TM.BladeStpt[8] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[8]) && !TM.SlittersSelected[8])
            { Blade9Posn.Background = Brushes.LightGreen; }
            else { Blade9Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[9] + TM.InPosWindow) >= TM.BladeActPosn[9]) && ((TM.BladeStpt[9] - TM.InPosWindow) <= TM.BladeActPosn[9]) && TM.SlittersSelected[9])
            { Blade10Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[9] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[9]) && ((TM.BladeStpt[9] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[9]) && !TM.SlittersSelected[9])
            { Blade10Posn.Background = Brushes.LightGreen; }
            else { Blade10Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[10] + TM.InPosWindow) >= TM.BladeActPosn[10]) && ((TM.BladeStpt[10] - TM.InPosWindow) <= TM.BladeActPosn[10]) && TM.SlittersSelected[10])
            { Blade11Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[10] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[10]) && ((TM.BladeStpt[10] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[10]) && !TM.SlittersSelected[10])
            { Blade11Posn.Background = Brushes.LightGreen; }
            else { Blade11Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[11] + TM.InPosWindow) >= TM.BladeActPosn[11]) && ((TM.BladeStpt[11] - TM.InPosWindow) <= TM.BladeActPosn[11]) && TM.SlittersSelected[11])
            { Blade12Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[11] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[11]) && ((TM.BladeStpt[11] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[11]) && !TM.SlittersSelected[11])
            { Blade12Posn.Background = Brushes.LightGreen; }
            else { Blade12Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[12] + TM.InPosWindow) >= TM.BladeActPosn[12]) && ((TM.BladeStpt[12] - TM.InPosWindow) <= TM.BladeActPosn[12]) && TM.SlittersSelected[12])
            { Blade13Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[12] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[12]) && ((TM.BladeStpt[12] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[12]) && !TM.SlittersSelected[12])
            { Blade13Posn.Background = Brushes.LightGreen; }
            else { Blade13Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[13] + TM.InPosWindow) >= TM.BladeActPosn[13]) && ((TM.BladeStpt[13] - TM.InPosWindow) <= TM.BladeActPosn[13]) && TM.SlittersSelected[13])
            { Blade14Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[13] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[13]) && ((TM.BladeStpt[13] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[13]) && !TM.SlittersSelected[13])
            { Blade14Posn.Background = Brushes.LightGreen; }
            else { Blade14Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[14] + TM.InPosWindow) >= TM.BladeActPosn[14]) && ((TM.BladeStpt[14] - TM.InPosWindow) <= TM.BladeActPosn[14]) && TM.SlittersSelected[14])
            { Blade15Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[14] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[14]) && ((TM.BladeStpt[14] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[14]) && !TM.SlittersSelected[14])
            { Blade15Posn.Background = Brushes.LightGreen; }
            else { Blade15Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[15] + TM.InPosWindow) >= TM.BladeActPosn[15]) && ((TM.BladeStpt[15] - TM.InPosWindow) <= TM.BladeActPosn[15]) && TM.SlittersSelected[15])
            { Blade16Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[15] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[15]) && ((TM.BladeStpt[15] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[15]) && !TM.SlittersSelected[15])
            { Blade16Posn.Background = Brushes.LightGreen; }
            else { Blade16Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[16] + TM.InPosWindow) >= TM.BladeActPosn[16]) && ((TM.BladeStpt[16] - TM.InPosWindow) <= TM.BladeActPosn[16]) && TM.SlittersSelected[16])
            { Blade17Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[16] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[16]) && ((TM.BladeStpt[16] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[16]) && !TM.SlittersSelected[16])
            { Blade17Posn.Background = Brushes.LightGreen; }
            else { Blade17Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[17] + TM.InPosWindow) >= TM.BladeActPosn[17]) && ((TM.BladeStpt[17] - TM.InPosWindow) <= TM.BladeActPosn[17]) && TM.SlittersSelected[17])
            { Blade18Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[17] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[17]) && ((TM.BladeStpt[17] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[17]) && !TM.SlittersSelected[17])
            { Blade18Posn.Background = Brushes.LightGreen; }
            else { Blade18Posn.Background = Brushes.LightCoral; }

            if (((TM.BladeStpt[18] + TM.InPosWindow) >= TM.BladeActPosn[18]) && ((TM.BladeStpt[18] - TM.InPosWindow) <= TM.BladeActPosn[18]) && TM.SlittersSelected[18])
            { Blade19Posn.Background = Brushes.LightGreen; }
            else if (((TM.BladeStpt[18] + TM.UnSelectedPosWindow) >= TM.BladeActPosn[18]) && ((TM.BladeStpt[18] - TM.UnSelectedPosWindow) <= TM.BladeActPosn[18]) && !TM.SlittersSelected[18])
            { Blade19Posn.Background = Brushes.LightGreen; }
            else { Blade19Posn.Background = Brushes.LightCoral; }

        }

        
        #endregion

        #region Initial Slitters Out of Service 
        //Checks for Slitters out of service when starting up program
        private void InitialSlittersOutOfServ()
        {
            if (PLC.PLCControlBits[0, 11])
            {
            /*TM.SlitterDisable[0] = true;
            S1LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S1LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[0] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true; */
            }
            
            if (PLC.PLCControlBits[0, 12])
            {
                TM.SlitterDisable[1] = true;
                S2LblServ.BorderBrush = Brushes.Magenta;
                S2LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S2LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[1] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 2");
            }
           
            
            
            if (PLC.PLCControlBits[0, 13])
            {
                TM.SlitterDisable[2] = true;
                S3LblServ.BorderBrush = Brushes.Magenta;
                S3LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S3LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[2] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 3");
            } 
            

            if (PLC.PLCControlBits[0, 14])
            {
                TM.SlitterDisable[3] = true;
                S4LblServ.BorderBrush = Brushes.Magenta;
                S4LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S4LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[3] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 4");
            }
            

            if (PLC.PLCControlBits[1, 0])
            {
                TM.SlitterDisable[4] = true;
                S5LblServ.BorderBrush = Brushes.Magenta;
                S5LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S5LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[4] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 5");

            }
            
            if (PLC.PLCControlBits[1, 1])
            {
                TM.SlitterDisable[5] = true;
                S6LblServ.BorderBrush = Brushes.Magenta;
                S6LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S6LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[5] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 6");

            }
           
            if (PLC.PLCControlBits[1, 2])
            {
                TM.SlitterDisable[6] = true;
                S7LblServ.BorderBrush = Brushes.Magenta;
                S7LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S7LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[6] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 7");

            }
            
            if (PLC.PLCControlBits[1, 3])
            {
                TM.SlitterDisable[7] = true;
                S8LblServ.BorderBrush = Brushes.Magenta;
                S8LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S8LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[7] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 8");

            }
            
            if (PLC.PLCControlBits[1, 4])
            {
                TM.SlitterDisable[8] = true;
                S9LblServ.BorderBrush = Brushes.Magenta;
                S9LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S9LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[8] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 9");

            }
            
            if (PLC.PLCControlBits[1, 5])
            {
                TM.SlitterDisable[9] = true;
                S10LblServ.BorderBrush = Brushes.Magenta;
                S10LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S10LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[9] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 10");

            }
            
            if (PLC.PLCControlBits[1, 6])
            {
                TM.SlitterDisable[10] = true;
                S11LblServ.BorderBrush = Brushes.Magenta;
                S11LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S11LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[10] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 11");

            }
            
            if (PLC.PLCControlBits[1, 7])
            {
                TM.SlitterDisable[11] = true;
                S12LblServ.BorderBrush = Brushes.Magenta;
                S12LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S12LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[11] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 12");

            }
            
            if (PLC.PLCControlBits[1, 8])
            {
                TM.SlitterDisable[12] = true;
                S13LblServ.BorderBrush = Brushes.Magenta;
                S13LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S13LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[12] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 13");

            }
            
            if (PLC.PLCControlBits[1, 9])
            {
                TM.SlitterDisable[13] = true;
                S14LblServ.BorderBrush = Brushes.Magenta;
                S14LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S14LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[13] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 14");

            }
            
            if (PLC.PLCControlBits[1, 10])
            {
                TM.SlitterDisable[14] = true;
                S15LblServ.BorderBrush = Brushes.Magenta;
                S15LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S15LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[14] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 15");

            }
            
            if (PLC.PLCControlBits[1, 11])
            {
                TM.SlitterDisable[15] = true;
                S16LblServ.BorderBrush = Brushes.Magenta;
                S16LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S16LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[15] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 16");

            }
            
            if (PLC.PLCControlBits[1, 12])
            {
                TM.SlitterDisable[16] = true;
                S17LblServ.BorderBrush = Brushes.Magenta;
                S17LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S17LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[16] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 17");

            }
            
            if (PLC.PLCControlBits[1, 13])
            {
                TM.SlitterDisable[17] = true;
                S18LblServ.BorderBrush = Brushes.Magenta;
                S18LblServ.BorderThickness = new Thickness(5.0);
                AcceptBtn.Background = Brushes.Yellow;
                S18LblEn.Visibility = Visibility.Hidden;
                TM.SlitterOutofService[17] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 18");

            }
            
            if (PLC.PLCControlBits[1, 14])
            {
                /* TM.SlitterDisable[18] = true;
            S19LblServ.BorderBrush = Brushes.Magenta;
            S19LblServ.BorderThickness = new Thickness(5.0);
            AcceptBtn.Background = Brushes.Yellow;
            S19LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[18] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;*/

            }

        }
        #endregion

        #region Update Labels for Slitter Selection
        private void SelectionOfSlittersLabels()
        {
            if (TM.SlittersSelected[0]) { this.S1Lbl.Background = Brushes.PaleGoldenrod; } else { this.S1Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[1]) { this.S2Lbl.Background = Brushes.PaleGoldenrod; } else { this.S2Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[2]) { this.S3Lbl.Background = Brushes.PaleGoldenrod; } else { this.S3Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[3]) { this.S4Lbl.Background = Brushes.PaleGoldenrod; } else { this.S4Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[4]) { this.S5Lbl.Background = Brushes.PaleGoldenrod; } else { this.S5Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[5]) { this.S6Lbl.Background = Brushes.PaleGoldenrod; } else { this.S6Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[6]) { this.S7Lbl.Background = Brushes.PaleGoldenrod; } else { this.S7Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[7]) { this.S8Lbl.Background = Brushes.PaleGoldenrod; } else { this.S8Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[8]) { this.S9Lbl.Background = Brushes.PaleGoldenrod; } else { this.S9Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[9]) { this.S10Lbl.Background = Brushes.PaleGoldenrod; } else { this.S10Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[10]) { this.S11Lbl.Background = Brushes.PaleGoldenrod; } else { this.S11Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[11]) { this.S12Lbl.Background = Brushes.PaleGoldenrod; } else { this.S12Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[12]) { this.S13Lbl.Background = Brushes.PaleGoldenrod; } else { this.S13Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[13]) { this.S14Lbl.Background = Brushes.PaleGoldenrod; } else { this.S14Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[14]) { this.S15Lbl.Background = Brushes.PaleGoldenrod; } else { this.S15Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[15]) { this.S16Lbl.Background = Brushes.PaleGoldenrod; } else { this.S16Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[16]) { this.S17Lbl.Background = Brushes.PaleGoldenrod; } else { this.S17Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[17]) { this.S18Lbl.Background = Brushes.PaleGoldenrod; } else { this.S18Lbl.Background = Brushes.Transparent; }
            if (TM.SlittersSelected[18]) { this.S19Lbl.Background = Brushes.PaleGoldenrod; } else { this.S19Lbl.Background = Brushes.Transparent; }
        }

        private void SelectionOfSlittersLabelTextColor()
        {
           
            if (TM.SlittersSelected[0] && TM.SlitterDisable[0]) { this.S1Lbl.Foreground = Brushes.Red; } else { this.S1Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[1] && TM.SlitterDisable[1]) { this.S2Lbl.Foreground = Brushes.Red; } else { this.S2Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[2] && TM.SlitterDisable[2]) { this.S3Lbl.Foreground = Brushes.Red; } else { this.S3Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[3] && TM.SlitterDisable[3]) { this.S4Lbl.Foreground = Brushes.Red; } else { this.S4Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[4] && TM.SlitterDisable[4]) { this.S5Lbl.Foreground = Brushes.Red; } else { this.S5Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[5] && TM.SlitterDisable[5]) { this.S6Lbl.Foreground = Brushes.Red; } else { this.S6Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[6] && TM.SlitterDisable[6]) { this.S7Lbl.Foreground = Brushes.Red; } else { this.S7Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[7] && TM.SlitterDisable[7]) { this.S8Lbl.Foreground = Brushes.Red; } else { this.S8Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[8] && TM.SlitterDisable[8]) { this.S9Lbl.Foreground = Brushes.Red; } else { this.S9Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[9] && TM.SlitterDisable[9]) { this.S10Lbl.Foreground = Brushes.Red; } else { this.S10Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[10] && TM.SlitterDisable[10]) { this.S11Lbl.Foreground = Brushes.Red; } else { this.S11Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[11] && TM.SlitterDisable[11]) { this.S12Lbl.Foreground = Brushes.Red; } else { this.S12Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[12] && TM.SlitterDisable[12]) { this.S13Lbl.Foreground = Brushes.Red; } else { this.S13Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[13] && TM.SlitterDisable[13]) { this.S14Lbl.Foreground = Brushes.Red; } else { this.S14Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[14] && TM.SlitterDisable[14]) { this.S15Lbl.Foreground = Brushes.Red; } else { this.S15Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[15] && TM.SlitterDisable[15]) { this.S16Lbl.Foreground = Brushes.Red; } else { this.S16Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[16] && TM.SlitterDisable[16]) { this.S17Lbl.Foreground = Brushes.Red; } else { this.S17Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[17] && TM.SlitterDisable[17]) { this.S18Lbl.Foreground = Brushes.Red; } else { this.S18Lbl.Foreground = Brushes.Black; }
            if (TM.SlittersSelected[18] && TM.SlitterDisable[18]) { this.S19Lbl.Foreground = Brushes.Red; } else { this.S19Lbl.Foreground = Brushes.Black; }
        }

        private void SelectionOfSlittersLabelsBorder()
        {
            Tmr1.Stop();
            if (((Math.Abs(TM.BandStpt[0] - TM.BandActPosn[0])  > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[0] - TM.BladeActPosn[0]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[0])
                {
                    S1Lbl.BorderBrush = Brushes.Red;
                    S1Lbl.BorderThickness = new Thickness(5.0);
                }
            else
            {
                S1Lbl.BorderBrush = Brushes.Gray;
                S1Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[1] - TM.BandActPosn[1]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[1] - TM.BladeActPosn[1]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[1])
                {
                    S2Lbl.BorderBrush = Brushes.Red;
                    S2Lbl.BorderThickness = new Thickness(5.0);
                }
            else
            {
                S2Lbl.BorderBrush = Brushes.Gray;
                S2Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[2] - TM.BandActPosn[2]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[2] - TM.BladeActPosn[2]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[2])
            {
                S3Lbl.BorderBrush = Brushes.Red;
                S3Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S3Lbl.BorderBrush = Brushes.Gray;
                S3Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[3] - TM.BandActPosn[3]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[3] - TM.BladeActPosn[3]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[3])
            {
                S4Lbl.BorderBrush = Brushes.Red;
                S4Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S4Lbl.BorderBrush = Brushes.Gray;
                S4Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[4] - TM.BandActPosn[4]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[4] - TM.BladeActPosn[4]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[4])
            {
                S5Lbl.BorderBrush = Brushes.Red;
                S5Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S5Lbl.BorderBrush = Brushes.Gray;
                S5Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[5] - TM.BandActPosn[5]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[5] - TM.BladeActPosn[5]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[5])
            {
                S6Lbl.BorderBrush = Brushes.Red;
                S6Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S6Lbl.BorderBrush = Brushes.Gray;
                S6Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[6] - TM.BandActPosn[6]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[6] - TM.BladeActPosn[6]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[6])
            {
                S7Lbl.BorderBrush = Brushes.Red;
                S7Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S7Lbl.BorderBrush = Brushes.Gray;
                S7Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[7] - TM.BandActPosn[7]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[7] - TM.BladeActPosn[7]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[7])
            {
                S8Lbl.BorderBrush = Brushes.Red;
                S8Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S8Lbl.BorderBrush = Brushes.Gray;
                S8Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[8] - TM.BandActPosn[8]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[8] - TM.BladeActPosn[8]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[8])
            {
                S9Lbl.BorderBrush = Brushes.Red;
                S9Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S9Lbl.BorderBrush = Brushes.Gray;
                S9Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[9] - TM.BandActPosn[9]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[9] - TM.BladeActPosn[9]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[9])
            {
                S10Lbl.BorderBrush = Brushes.Red;
                S10Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S10Lbl.BorderBrush = Brushes.Gray;
                S10Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[10] - TM.BandActPosn[10]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[10] - TM.BladeActPosn[10]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[10])
            {
                S11Lbl.BorderBrush = Brushes.Red;
                S11Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S11Lbl.BorderBrush = Brushes.Gray;
                S11Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[11] - TM.BandActPosn[11]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[11] - TM.BladeActPosn[11]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[11])
            {
                S12Lbl.BorderBrush = Brushes.Red;
                S12Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S12Lbl.BorderBrush = Brushes.Gray;
                S12Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[12] - TM.BandActPosn[12]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[12] - TM.BladeActPosn[12]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[12])
            {
                S13Lbl.BorderBrush = Brushes.Red;
                S13Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S13Lbl.BorderBrush = Brushes.Gray;
                S13Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[13] - TM.BandActPosn[13]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[13] - TM.BladeActPosn[13]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[13])
            {
                S14Lbl.BorderBrush = Brushes.Red;
                S14Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S14Lbl.BorderBrush = Brushes.Gray;
                S14Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[14] - TM.BandActPosn[14]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[14] - TM.BladeActPosn[14]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[14])
            {
                S15Lbl.BorderBrush = Brushes.Red;
                S15Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S15Lbl.BorderBrush = Brushes.Gray;
                S15Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[15] - TM.BandActPosn[15]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[15] - TM.BladeActPosn[15]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[15])
            {
                S16Lbl.BorderBrush = Brushes.Red;
                S16Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S16Lbl.BorderBrush = Brushes.Gray;
                S16Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[16] - TM.BandActPosn[16]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[16] - TM.BladeActPosn[16]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[16])
            {
                S17Lbl.BorderBrush = Brushes.Red;
                S17Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S17Lbl.BorderBrush = Brushes.Gray;
                S17Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[17] - TM.BandActPosn[17]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[17] - TM.BladeActPosn[17]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[17])
            {
                S18Lbl.BorderBrush = Brushes.Red;
                S18Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S18Lbl.BorderBrush = Brushes.Gray;
                S18Lbl.BorderThickness = new Thickness(1.0);
            }
            if (((Math.Abs(TM.BandStpt[18] - TM.BandActPosn[18]) > TM.OutOfTolerance) || (Math.Abs(TM.BladeStpt[18] - TM.BladeActPosn[18]) > TM.OutOfTolerance)) && TM.SlittersSelectedPLC[18])
            {
                S19Lbl.BorderBrush = Brushes.Red;
                S19Lbl.BorderThickness = new Thickness(5.0);
            }
            else
            {
                S19Lbl.BorderBrush = Brushes.Gray;
                S19Lbl.BorderThickness = new Thickness(1.0);
            }

            Tmr1.Start();
        }

        #endregion

        #region Clear Data for Disabled Slitter
        private void ClearDataforDisableSliter()
        {
            TM.ZeroOutSlitterData();
            UpdateSlitterStpt();
            SelectionOfSlittersLabels();
        }
        #endregion

        #region Slitter Enalbe Disable Out of Service and Service

        private void S1LblEn_Checked(object sender, RoutedEventArgs e)
        {
            /*TM.SlitterDisable[0] = true;
            S1LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter(); */
        }

        private void S1LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
           /* TM.SlitterDisable[0] = false;
            S1LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow; */
        }

        private void S1LblServ_Checked(object sender, RoutedEventArgs e)
        {

            /*TM.SlitterDisable[0] = true;
            S1LblServ.BorderBrush = Brushes.Magenta;
            S1LblServ.BorderThickness = new Thickness(1.0);
            S1LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S1LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[0] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true; */
        }

        private void S1LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            /* TM.SlitterDisable[0] = false;
             * S1LblServ.BorderBrush = Brushes.Gray;
             S1LblServ.BorderThickness = new Thickness(1.0);
             S1LblServ.Background = Brushes.LimeGreen;
             S1LblEn.Visibility = Visibility.Visible;
             S1LblEn.Background = Brushes.LimeGreen;
             TM.SlitterOutofService[0] = false;
             AcceptBtn.Background = Brushes.Yellow;
             TM.SlitOutOfServDetect = true; */
        }

        private void S2LblEn_Checked(object sender, RoutedEventArgs e)
        {
              TM.SlitterDisable[1] = true;
              S2LblEn.Background = Brushes.Yellow;
              AcceptBtn.Background = Brushes.Yellow;
              ClearDataforDisableSliter();
              System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 2");
        }

        private void S2LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
             
              TM.SlitterDisable[1] = false;
              S2LblEn.Background = Brushes.LimeGreen;
              AcceptBtn.Background = Brushes.Yellow;
              System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 2");
        }

        private void S2LblServ_Checked(object sender, RoutedEventArgs e)
        {
           
              TM.SlitterDisable[1] = true;
              S2LblServ.Background = Brushes.Magenta;
              AcceptBtn.Background = Brushes.Yellow;
              S2LblEn.Visibility = Visibility.Hidden;
              TM.SlitterOutofService[1] = true;
              ClearDataforDisableSliter();
              TM.SlitOutOfServDetect = true;
              System.Windows.MessageBox.Show("Out of Service", "Slitter 2");
        }

        private void S2LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
              TM.SlitterDisable[1] = false;
              S2LblServ.BorderBrush = Brushes.Gray;
              S2LblServ.BorderThickness = new Thickness(1.0);
              S2LblServ.Background = Brushes.LimeGreen;
              S2LblEn.Visibility = Visibility.Visible;
              S2LblEn.Background = Brushes.LimeGreen;
              TM.SlitterOutofService[1] = false;
              AcceptBtn.Background = Brushes.Yellow;
              TM.SlitOutOfServDetect = true;
              System.Windows.MessageBox.Show("In Service", "Slitter 2");
        }

        private void S3LblEn_Checked(object sender, RoutedEventArgs e)
        {
            
             TM.SlitterDisable[2] = true;
             S3LblEn.Background = Brushes.Yellow;
             AcceptBtn.Background = Brushes.Yellow;
             ClearDataforDisableSliter();
             System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 3");

        }

        private void S3LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[2] = false;
            S3LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 3");
        }

        private void S3LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[2] = true;
            S3LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S3LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[2] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 3");
        }

        private void S3LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[2] = false;
            S3LblServ.BorderBrush = Brushes.Gray;
            S3LblServ.BorderThickness = new Thickness(1.0);
            S3LblServ.Background = Brushes.LimeGreen;
            S3LblEn.Visibility = Visibility.Visible;
            S3LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[2] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 3");
        }

        private void S4LblEn_Checked(object sender, RoutedEventArgs e)
        {
            
             TM.SlitterDisable[3] = true;
             S4LblEn.Background = Brushes.Yellow;
             AcceptBtn.Background = Brushes.Yellow;
             ClearDataforDisableSliter();
             System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 4");
        }

        private void S4LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[3] = false;
            S4LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 4");
        }
       
        private void S4LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[3] = true;
            S4LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S4LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[3] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 4");
        }

        private void S4LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[3] = false;
            S4LblServ.BorderBrush = Brushes.Gray;
            S4LblServ.BorderThickness = new Thickness(1.0);
            S4LblServ.Background = Brushes.LimeGreen;
            S4LblEn.Visibility = Visibility.Visible;
            S4LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[3] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 4");
        }

        private void S5LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[4] = true;
            S5LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 5");
        }

        private void S5LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[4] = false;
            S5LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 5");
        }

        private void S5LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[4] = true;
            S5LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S5LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[4] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 5");
        }

        private void S5LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[4] = false;
            S5LblServ.BorderBrush = Brushes.Gray;
            S5LblServ.BorderThickness = new Thickness(1.0);
            S5LblServ.Background = Brushes.LimeGreen;
            S5LblEn.Visibility = Visibility.Visible;
            S5LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[4] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 5");
        }

        private void S6LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[5] = true;
            S6LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 6");
        }

        private void S6LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[5] = false;
            S6LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 6");
        }

        private void S6LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[5] = true;
            S6LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S6LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[5] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 6");
        }
               
        private void S6LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[5] = false;
            S6LblServ.BorderBrush = Brushes.Gray;
            S6LblServ.BorderThickness = new Thickness(1.0);
            S6LblServ.Background = Brushes.LimeGreen;
            S6LblEn.Visibility = Visibility.Visible;
            S6LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[5] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 6");
        }

        private void S7LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[6] = true;
            S7LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 7"); ;
        }

        private void S7LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[6] = false;
            S7LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 7");
        }

        private void S7LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[6] = true;
            S7LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S7LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[6] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 7");
        }

        private void S7LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[6] = false;
            S7LblServ.BorderBrush = Brushes.Gray;
            S7LblServ.BorderThickness = new Thickness(1.0);
            S7LblServ.Background = Brushes.LimeGreen;
            S7LblEn.Visibility = Visibility.Visible;
            S7LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[6] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 7");
        }

        private void S8LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[7] = true;
            S8LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 8");
        }

        private void S8LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[7] = false;
            S8LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 8");
        }

        private void S8LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[7] = true;
            S8LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S8LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[7] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 8");
        }

        private void S8LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[7] = false;
            S8LblServ.BorderBrush = Brushes.Gray;
            S8LblServ.BorderThickness = new Thickness(1.0);
            S8LblServ.Background = Brushes.LimeGreen;
            S8LblEn.Visibility = Visibility.Visible;
            S8LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[7] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 8");
        }

        private void S9LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[8] = true;
            S9LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 9");
        }

        private void S9LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[8] = false;
            S9LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 9");
        }

        private void S9LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[8] = true;
            S9LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S9LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[8] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 9");
        }

        private void S9LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[8] = false;
            S9LblServ.BorderBrush = Brushes.Gray;
            S9LblServ.BorderThickness = new Thickness(1.0);
            S9LblServ.Background = Brushes.LimeGreen;
            S9LblEn.Visibility = Visibility.Visible;
            S9LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[8] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 9");
        }

        private void S10LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[9] = true;
            S10LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 10");
        }

        private void S10LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[9] = false;
            S10LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 10");
        }

        private void S10LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[9] = true;
            S10LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S10LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[9] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 10");
        }
      
        private void S10LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[9] = false;
            S10LblServ.BorderBrush = Brushes.Gray;
            S10LblServ.BorderThickness = new Thickness(1.0);
            S10LblServ.Background = Brushes.LimeGreen;
            S10LblEn.Visibility = Visibility.Visible;
            S10LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[9] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 10");
        }

        private void S11LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[10] = true;
            S11LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 11");
        }

        private void S11LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[10] = false;
            S11LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 11");
        }

        private void S11LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[10] = true;
            S11LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S11LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[10] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 11");
        }
       
        private void S11LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[10] = false;
            S11LblServ.BorderBrush = Brushes.Gray;
            S11LblServ.BorderThickness = new Thickness(1.0);
            S11LblServ.Background = Brushes.LimeGreen;
            S11LblEn.Visibility = Visibility.Visible;
            S11LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[10] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 11");
        }

        private void S12LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[11] = true;
            S12LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 12");
        }

        private void S12LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[11] = false;
            S12LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 12");
        }

        private void S12LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[11] = true;
            S12LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S12LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[11] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 12");
        }      

        private void S12LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[11] = false;
            S12LblServ.BorderBrush = Brushes.Gray;
            S12LblServ.BorderThickness = new Thickness(1.0);
            S12LblServ.Background = Brushes.LimeGreen;
            S12LblEn.Visibility = Visibility.Visible;
            S12LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[11] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 12");
        }

        private void S13LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[12] = true;
            S13LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 13");
        }

        private void S13LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[12] = false;
            S13LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 13");
        }

        private void S13LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[12] = true;
            S13LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S13LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[12] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 13");
        }
        
        private void S13LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[12] = false;
            S13LblServ.BorderBrush = Brushes.Gray;
            S13LblServ.BorderThickness = new Thickness(1.0);
            S13LblServ.Background = Brushes.LimeGreen;
            S13LblEn.Visibility = Visibility.Visible;
            S13LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[12] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 13");
        }

        private void S14LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[13] = true;
            S14LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 14");
        }

        private void S14LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[13] = false;
            S14LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 14");
        }

        private void S14LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[13] = true;
            S14LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S14LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[13] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 14");
        }
        
        private void S14LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[13] = false;
            S14LblServ.BorderBrush = Brushes.Gray;
            S14LblServ.BorderThickness = new Thickness(1.0);
            S14LblServ.Background = Brushes.LimeGreen;
            S14LblEn.Visibility = Visibility.Visible;
            S14LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[13] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 14");
        }

        private void S15LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[14] = true;
            S15LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 15");
        }

        private void S15LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[14] = false;
            S15LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 15");
        }

        private void S15LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[14] = true;
            S15LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S15LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[14] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 15");
        }        

        private void S15LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[14] = false;
            S15LblServ.BorderBrush = Brushes.Gray;
            S15LblServ.BorderThickness = new Thickness(1.0);
            S15LblServ.Background = Brushes.LimeGreen;
            S15LblEn.Visibility = Visibility.Visible;
            S15LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[14] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 15");
        }

        private void S16LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[15] = true;
            S16LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 16");
        }

        private void S16LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[15] = false;
            S16LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 16");
        }

        private void S16LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[15] = true;
            S16LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S16LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[15] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 16");
        }        

        private void S16LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[15] = false;
            S16LblServ.BorderBrush = Brushes.Gray;
            S16LblServ.BorderThickness = new Thickness(1.0);
            S16LblServ.Background = Brushes.LimeGreen;
            S16LblEn.Visibility = Visibility.Visible;
            S16LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[15] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 16");
        }

        private void S17LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[16] = true;
            S17LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 17");
        }

        private void S17LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[16] = false;
            S17LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 17");
        }

        private void S17LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[16] = true;
            S17LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S17LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[16] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 17");
        }       

        private void S17LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[16] = false;
            S17LblServ.BorderBrush = Brushes.Gray;
            S17LblServ.BorderThickness = new Thickness(1.0);
            S17LblServ.Background = Brushes.LimeGreen;
            S17LblEn.Visibility = Visibility.Visible;
            S17LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[16] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 17");
        }

        private void S18LblEn_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[17] = true;
            S18LblEn.Background = Brushes.Yellow;
            AcceptBtn.Background = Brushes.Yellow;
            ClearDataforDisableSliter();
            System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 18");
        }

        private void S18LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[17] = false;
            S18LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;
            System.Windows.MessageBox.Show("Slitter Enabled", "Slitter 18");
        }

        private void S18LblServ_Checked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[17] = true;
            S18LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S18LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[17] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("Out of Service", "Slitter 18");
        }       

        private void S18LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            TM.SlitterDisable[17] = false;
            S18LblServ.BorderBrush = Brushes.Gray;
            S18LblServ.BorderThickness = new Thickness(1.0);
            S18LblServ.Background = Brushes.LimeGreen;
            S18LblEn.Visibility = Visibility.Visible;
            S18LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[17] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;
            System.Windows.MessageBox.Show("In Service", "Slitter 18");
        }

        private void S19LblEn_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result = MessageBox.Show("Do you want to Disable Slitter?", "Slitter Enable or Disable?", MessageBoxButton.YesNoCancel);
             /* TM.SlitterDisable[18] = true;
              S19LblEn.Background = Brushes.Yellow;
              AcceptBtn.Background = Brushes.Yellow;
              ClearDataforDisableSliter();*/
        }

        private void S19LblEn_Unchecked(object sender, RoutedEventArgs e)
        {
            /*TM.SlitterDisable[18] = false;
            S19LblEn.Background = Brushes.LimeGreen;
            AcceptBtn.Background = Brushes.Yellow;*/
        }

        private void S19LblServ_Checked(object sender, RoutedEventArgs e)
        {
           /* TM.SlitterDisable[18] = true;
            S19LblServ.Background = Brushes.Magenta;
            AcceptBtn.Background = Brushes.Yellow;
            S19LblEn.Visibility = Visibility.Hidden;
            TM.SlitterOutofService[18] = true;
            ClearDataforDisableSliter();
            TM.SlitOutOfServDetect = true;*/
        }       

        private void S19LblServ_Unchecked(object sender, RoutedEventArgs e)
        {
            /*TM.SlitterDisable[18] = false;
            S19LblServ.BorderBrush = Brushes.Gray;
            S19LblServ.BorderThickness = new Thickness(1.0);
            S19LblServ.Background = Brushes.LimeGreen;
            S19LblEn.Visibility = Visibility.Visible;
            S19LblEn.Background = Brushes.LimeGreen;
            TM.SlitterOutofService[18] = false;
            AcceptBtn.Background = Brushes.Yellow;
            TM.SlitOutOfServDetect = true;*/
        }

        #endregion

        #region Roll Text Changed
        private void Roll1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll1.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll1.Text = "";
            }
            else
            {
                TM.WrapData[0] = Convert.ToDouble(this.Roll1.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll2_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll2.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll2.Text = "";
            }
            else
            {
                TM.WrapData[1] = Convert.ToDouble(this.Roll2.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll3_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll3.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll3.Text = "";
            }
            else
            {
                TM.WrapData[2] = Convert.ToDouble(this.Roll3.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll4_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll4.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll4.Text = "";
            }
            else
            {
                TM.WrapData[3] = Convert.ToDouble(this.Roll4.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll5_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll5.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll5.Text = "";
            }
            else
            {
                TM.WrapData[4] = Convert.ToDouble(this.Roll5.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll6_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll6.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll6.Text = "";
            }
            else
            {
                TM.WrapData[5] = Convert.ToDouble(this.Roll6.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll7_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll7.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll7.Text = "";
            }
            else
            {
                TM.WrapData[6] = Convert.ToDouble(this.Roll7.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll8_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll8.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll8.Text = "";
            }
            else
            {
                TM.WrapData[7] = Convert.ToDouble(this.Roll8.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll9_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll9.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll9.Text = "";
            }
            else
            {
                TM.WrapData[8] = Convert.ToDouble(this.Roll9.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll10_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll10.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll10.Text = "";
            }
            else
            {
                TM.WrapData[9] = Convert.ToDouble(this.Roll10.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll11_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll11.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll11.Text = "";
            }
            else
            {
                TM.WrapData[10] = Convert.ToDouble(this.Roll11.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll12_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll12.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll12.Text = "";
            }
            else
            {
                TM.WrapData[11] = Convert.ToDouble(this.Roll12.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll13_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll13.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll13.Text = "";
            }
            else
            {
                TM.WrapData[12] = Convert.ToDouble(this.Roll13.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll14_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll14.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll14.Text = "";
            }
            else
            {
                TM.WrapData[13] = Convert.ToDouble(this.Roll14.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }
        private void Roll15_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll15.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll15.Text = "";
            }
            else
            {
                TM.WrapData[14] = Convert.ToDouble(this.Roll15.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll16_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll16.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll16.Text = "";
            }
            else
            {
                TM.WrapData[15] = Convert.ToDouble(this.Roll16.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll17_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll17.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll17.Text = "";
            }
            else
            {
                TM.WrapData[16] = Convert.ToDouble(this.Roll17.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        private void Roll18_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(this.Roll18.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                this.Roll18.Text = "";
            }
            else
            {
                TM.WrapData[17] = Convert.ToDouble(this.Roll18.Text);
                AcceptBtn.Background = Brushes.Yellow;
            }
            Tmr1.Start();
        }

        #endregion R

        #region Buttons

        private void LifeCycleBtn_Click(object sender, RoutedEventArgs e)
        {
            SLifeCycle SWin = new SLifeCycle();
            SWin.ShowDialog();
        }

        private void ShrinkIncDecBtn_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TM.Shrinkage = Convert.ToDouble(ShrinkIncDecBtn.Value);
            AcceptBtn.Background = Brushes.Yellow;
            ExecuteBtn.Background = Brushes.Transparent;
            ShrinkIncDecBtn.Background = Brushes.Transparent;
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            Tmr1.Stop();
            String ConnectedMessage = "Connected";
            String ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            //Command Register gets zeroed out i TimerCylce1()
            //Write Command Registers for Reset
            ReadMessage = MB.WriteSingleRegiste(14199, 1024);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            CommandWritten = true;
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14200) Method ResetBtn_Click \n ";

            }
            OutOfTolerance = false;
            FM.MsgReset();
            FltMsgListBx.Items.Clear();
            Tmr1.Start();

            
        }

        private void AcceptBtn_Click(object sender, EventArgs e)
        {
            bool SolutionFailed = false;
            AcceptBtn.Background = Brushes.Transparent;
            TM.ZeroOutSlitterData();
            if (CenterTrimOn || TM.CalibrateMode)
            {
                TM.CalcRollParam();
            }
            else
            {
                TM.CalcRollParamFixedDSTrim();
            }
            
            UpdateRollTextBox();
            UpdateRollShrinkTxt();
            ShrkWidthTxtBx.Text = TM.TotalWidth.ToString("F2");
            OrderWidthTxtBx.Text = TM.TotalWidthNoShrk.ToString("F2");
            NumbOfRollsTxt.Text = Convert.ToString(TM.NumbOfRolls);

            if (TM.TotalWidth > TM.MaxWidth)
            {
                System.Windows.MessageBox.Show("Maximum Trim Exceeded");
            }
            // Select Slittes Available for Cut fro True False Grid on Form6
            TM.SelectSlittersForCuts();
            // Calculate using Park Position - Select Slitter for Cut.  Populates SolutionSelectPark
            TM.SelectslitterforCutsParkPosn();

            // Calculate using Acutal Slitter Positin - Select Slitter for Cut. Populates SolutionSelectAct
            TM.SelectSitterforCutsActPosn();

            // Calculate using Band Upper Limits  - Select Slitter for Cut Populates  SolutionSelectParkLmt
            TM.SelectSitterforCutsParkLimit();

            //Checks to ses if selected slitters matches number of cuts needed
            TM.CompareSlittersUsedToCuts();

            //Select Best Solution from methods TotalActPosnMovement() and TotalParkMovement()
            SolutionFailed = TM.SlitCutsUsedToRollCuts();
            if (SolutionFailed)
            {
                System.Windows.MessageBox.Show("Solution Failed, Swap Rolls");
            }

            //Calculate Setpoints for Slitters selected for cuts
            TM.CalcSelectedBandStpts();
            // Calculate Setpoints for Slitters not selected for cuts
            TM.CalcBandStptsNotUsed();

            // Verify Slitters are minimum distance from each other (153.00 mm)
            SlitStptVerified = TM.VerifySlitterSetpoints();
            if (!SlitStptVerified)
            {
                TM.SlitCutsUsedToRollCuts();
                TM.CalcSelectedBandStpts();
                TM.CalcBandStptsNotUsed();
            }

            if (TM.BandStpt[18] < TM.BandLowerLimit[18])
            {
                System.Windows.MessageBox.Show("Invalid Roll Data");
            }


            // Calculate Setpoints for Slitters not selected for cuts
            TM.CalcBandStptsNotUsed();
            
            TM.BladeStpt = TM.BandStpt;
            UpdateSlitterStpt();
            RemainingTrimTxtBx.Text = (TM.MaxWidth - TM.TotalWidth).ToString("F2"); ;
            TotalParkSlitTxt.Text = Convert.ToString(TM.NumbOfSlitParkSelected);
            TotalParkSlitLimitTxt.Text = Convert.ToString(TM.NumbOfslitParkSelectdLmt);
            TotalActSlitTxt.Text = Convert.ToString(TM.NumbOfSlitActPosSelected);
            DSTrimPosnTxtBx.Text = TM.DSTrimFixed.ToString("F2");
            TSTrimPosnTxtBx.Text = ((TM.MaxWidth - TM.TotalWidth) - TM.DSTrimFixed).ToString("F2");
            ShrinkmmTxtBx.Text = (TM.TotalWidth - TM.TotalWidthNoShrk).ToString("F2");
            SelectionOfSlittersLabels();
            PLC.BitAssignmentForActiveSlitters(TM.SlittersSelected);
            if (TM.CalibrateMode)
            {
                ExecuteBtn.Background = Brushes.Orange;
            }
            else
            {
                ExecuteBtn.Background = Brushes.Yellow;
            }

            if (AutoCtrTrimChkBx.IsChecked.Value)
            {
                CenterTrimOn = true;
            }
            else
            {
                CenterTrimOn = false;
            }
           
        }
        
        private void ExecuteBtn_Click(object sender, EventArgs e)
        {
            //New Slitter Assignment = 1, Auto Slitter Position Command = 32, Core Chucks Comand = 64  *** Write to PLC 64 + 32 + 1 = 97
            SlitterPLCWrites(97);
            ExecuteBtn.Background = Brushes.Transparent;
            TM.RollWidthChecker = TM.RollWidth;

        }

        private void CalibOrdBtn_Click(object sender, EventArgs e)
        {
            Tmr1.Stop();
            double ZeroOut = 0.0;
            BandCalibTxt1.Visibility = Visibility.Visible;
            BandCalibTxt2.Visibility = Visibility.Visible;
            BandCalibTxt3.Visibility = Visibility.Visible;
            BandCalibTxt4.Visibility = Visibility.Visible;
            BandCalibTxt5.Visibility = Visibility.Visible;
            BandCalibTxt6.Visibility = Visibility.Visible;
            BandCalibTxt7.Visibility = Visibility.Visible;
            BandCalibTxt8.Visibility = Visibility.Visible;
            BandCalibTxt9.Visibility = Visibility.Visible;
            BandCalibTxt10.Visibility = Visibility.Visible;
            BandCalibTxt11.Visibility = Visibility.Visible;
            BandCalibTxt12.Visibility = Visibility.Visible;
            BandCalibTxt13.Visibility = Visibility.Visible;
            BandCalibTxt14.Visibility = Visibility.Visible;
            BandCalibTxt15.Visibility = Visibility.Visible;
            BandCalibTxt16.Visibility = Visibility.Visible;
            BandCalibTxt17.Visibility = Visibility.Visible;
            BandCalibTxt18.Visibility = Visibility.Visible;
            BandCalibTxt19.Visibility = Visibility.Visible;
            TM.WrapData[0] = 460.0;
            TM.WrapData[1] = 470.0;
            TM.WrapData[2] = 470.0;
            TM.WrapData[3] = 470.0;
            TM.WrapData[4] = 470.0;
            TM.WrapData[5] = 470.0;
            TM.WrapData[6] = 470.0;
            TM.WrapData[7] = 470.0;
            TM.WrapData[8] = 470.0;
            TM.WrapData[9] = 470.0;
            TM.WrapData[10] = 470.0;
            TM.WrapData[11] = 470.0;
            TM.WrapData[12] = 470.0;
            TM.WrapData[13] = 470.0;
            TM.WrapData[14] = 470.0;
            TM.WrapData[15] = 470.0;
            TM.WrapData[16] = 470.0;
            TM.WrapData[17] = 500.0;
            BandCalibTxt1.Text = ZeroOut.ToString("F2");
            BandCalibTxt2.Text = ZeroOut.ToString("F2");
            BandCalibTxt3.Text = ZeroOut.ToString("F2");
            BandCalibTxt4.Text = ZeroOut.ToString("F2");
            BandCalibTxt5.Text = ZeroOut.ToString("F2");
            BandCalibTxt6.Text = ZeroOut.ToString("F2");
            BandCalibTxt7.Text = ZeroOut.ToString("F2");
            BandCalibTxt8.Text = ZeroOut.ToString("F2");
            BandCalibTxt9.Text = ZeroOut.ToString("F2");
            BandCalibTxt10.Text = ZeroOut.ToString("F2");
            BandCalibTxt11.Text = ZeroOut.ToString("F2");
            BandCalibTxt12.Text = ZeroOut.ToString("F2");
            BandCalibTxt13.Text = ZeroOut.ToString("F2");
            BandCalibTxt14.Text = ZeroOut.ToString("F2");
            BandCalibTxt15.Text = ZeroOut.ToString("F2");
            BandCalibTxt16.Text = ZeroOut.ToString("F2");
            BandCalibTxt17.Text = ZeroOut.ToString("F2");
            BandCalibTxt18.Text = ZeroOut.ToString("F2");
            BandCalibTxt19.Text = ZeroOut.ToString("F2");
            S1ManSelectBtn.Visibility = Visibility.Visible;
            S2ManSelectBtn.Visibility = Visibility.Visible;
            S3ManSelectBtn.Visibility = Visibility.Visible;
            S4ManSelectBtn.Visibility = Visibility.Visible;
            S5ManSelectBtn.Visibility = Visibility.Visible;
            S6ManSelectBtn.Visibility = Visibility.Visible;
            S7ManSelectBtn.Visibility = Visibility.Visible;
            S8ManSelectBtn.Visibility = Visibility.Visible;
            S9ManSelectBtn.Visibility = Visibility.Visible;
            S10ManSelectBtn.Visibility = Visibility.Visible;
            S11ManSelectBtn.Visibility = Visibility.Visible;
            S12ManSelectBtn.Visibility = Visibility.Visible;
            S13ManSelectBtn.Visibility = Visibility.Visible;
            S14ManSelectBtn.Visibility = Visibility.Visible;
            S15ManSelectBtn.Visibility = Visibility.Visible;
            S16ManSelectBtn.Visibility = Visibility.Visible;
            S17ManSelectBtn.Visibility = Visibility.Visible;
            S18ManSelectBtn.Visibility = Visibility.Visible;
            S19ManSelectBtn.Visibility = Visibility.Visible;
            LoadParambtn.Visibility = Visibility.Visible;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Visible;
            TransferOffsetsToPLCBtn.Background = Brushes.Transparent;
            TM.CalibrateMode = true;
            OrderNoTxtBx.Text = "Calibration";
            OrderNoTxtBx.Background = Brushes.Orange;
            UpdateRollTextBox();
            ShrkWidthTxtBx.Text = TM.TotalWidth.ToString("F2");
            OrderWidthTxtBx.Text = TM.TotalWidthNoShrk.ToString("F2");
            NumbOfRollsTxt.Text = Convert.ToString(TM.NumbOfRolls);
            TM.Shrinkage = 0.0;
            ShrinkIncDecBtn.Value = 0.0;
            ShrinkIncDecBtn.Background = Brushes.LightCoral;
            AcceptBtn.Background = Brushes.Orange;
            for (int x = 0; x < 19; x++)
            {
                TM.SlitterCalibTextChgd[x] = false;
            }

            Tmr1.Start();
        }

        private void LoadParambtn_Click(object sender, EventArgs e)
        {
            Tmr1.Stop();

            BandCalibTxt1.Text = TM.BandCalibration[0].ToString("F2");
            BandCalibTxt2.Text = TM.BandCalibration[1].ToString("F2");
            BandCalibTxt3.Text = TM.BandCalibration[2].ToString("F2");
            BandCalibTxt4.Text = TM.BandCalibration[3].ToString("F2");
            BandCalibTxt5.Text = TM.BandCalibration[4].ToString("F2");
            BandCalibTxt6.Text = TM.BandCalibration[5].ToString("F2");
            BandCalibTxt7.Text = TM.BandCalibration[6].ToString("F2");
            BandCalibTxt8.Text = TM.BandCalibration[7].ToString("F2");
            BandCalibTxt9.Text = TM.BandCalibration[8].ToString("F2");
            BandCalibTxt10.Text = TM.BandCalibration[9].ToString("F2");
            BandCalibTxt11.Text = TM.BandCalibration[10].ToString("F2");
            BandCalibTxt12.Text = TM.BandCalibration[11].ToString("F2");
            BandCalibTxt13.Text = TM.BandCalibration[12].ToString("F2");
            BandCalibTxt14.Text = TM.BandCalibration[13].ToString("F2");
            BandCalibTxt15.Text = TM.BandCalibration[14].ToString("F2");
            BandCalibTxt16.Text = TM.BandCalibration[15].ToString("F2");
            BandCalibTxt17.Text = TM.BandCalibration[16].ToString("F2");
            BandCalibTxt18.Text = TM.BandCalibration[17].ToString("F2");
            BandCalibTxt19.Text = TM.BandCalibration[18].ToString("F2");
            for (int x = 0; x < 19; x++)
            {
                TM.SlitterCalibTextChgd[x] = false;
            }
            CalibParamsLoaded = true;
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void TransferOffsetsToPLCBtn_Click(object sender, EventArgs e)
        {
            Tmr1.Stop();
            TM.CmdOffsetChgd = true;
            LoadParambtn.Background = Brushes.Transparent;
            TransferOffsetsToPLCBtn.Background = Brushes.Transparent;
            

            if (CalibParamsLoaded)
            {

                
                if (TM.SlitterCalibTextChgd[0])
                    {
                        TM.CalibrateOffsets[0] = Convert.ToDouble(BandCalibTxt1.Text);
                    }
                else
                    {
                        TM.CalibrateOffsets[0] = TM.BandCalibration[0];
                    }
                if (TM.SlitterCalibTextChgd[1])
                    {
                        TM.CalibrateOffsets[1] = Convert.ToDouble(BandCalibTxt2.Text);
                    }
                else
                    {
                        TM.CalibrateOffsets[1] = TM.BandCalibration[1];
                    }
                if (TM.SlitterCalibTextChgd[2])
                {
                    TM.CalibrateOffsets[2] = Convert.ToDouble(BandCalibTxt3.Text);
                }
                else
                {
                    TM.CalibrateOffsets[2] = TM.BandCalibration[2];
                }
                if (TM.SlitterCalibTextChgd[3])
                {
                    TM.CalibrateOffsets[3] = Convert.ToDouble(BandCalibTxt4.Text);
                }
                else
                {
                    TM.CalibrateOffsets[3] = TM.BandCalibration[3];
                }
                if (TM.SlitterCalibTextChgd[4])
                {
                    TM.CalibrateOffsets[4] = Convert.ToDouble(BandCalibTxt5.Text);
                }
                else
                {
                    TM.CalibrateOffsets[4] = TM.BandCalibration[4];
                }
                if (TM.SlitterCalibTextChgd[5])
                {
                    TM.CalibrateOffsets[5] = Convert.ToDouble(BandCalibTxt6.Text);
                }
                else
                {
                    TM.CalibrateOffsets[5] = TM.BandCalibration[5];
                }
                if (TM.SlitterCalibTextChgd[6])
                {
                    TM.CalibrateOffsets[6] = Convert.ToDouble(BandCalibTxt7.Text);
                }
                else
                {
                    TM.CalibrateOffsets[6] = TM.BandCalibration[6];
                }
                if (TM.SlitterCalibTextChgd[7])
                {
                    TM.CalibrateOffsets[7] = Convert.ToDouble(BandCalibTxt8.Text);
                }
                else
                {
                    TM.CalibrateOffsets[7] = TM.BandCalibration[7];
                }
                if (TM.SlitterCalibTextChgd[8])
                {
                    TM.CalibrateOffsets[8] = Convert.ToDouble(BandCalibTxt9.Text);
                }
                else
                {
                    TM.CalibrateOffsets[8] = TM.BandCalibration[8];
                }
                if (TM.SlitterCalibTextChgd[9])
                {
                    TM.CalibrateOffsets[9] = Convert.ToDouble(BandCalibTxt10.Text);
                }
                else
                {
                    TM.CalibrateOffsets[9] = TM.BandCalibration[9];
                }
                if (TM.SlitterCalibTextChgd[10])
                {
                    TM.CalibrateOffsets[10] = Convert.ToDouble(BandCalibTxt11.Text);
                }
                else
                {
                    TM.CalibrateOffsets[10] = TM.BandCalibration[10];
                }
                if (TM.SlitterCalibTextChgd[11])
                {
                    TM.CalibrateOffsets[11] = Convert.ToDouble(BandCalibTxt12.Text);
                }
                else
                {
                    TM.CalibrateOffsets[11] = TM.BandCalibration[11];
                }
                if (TM.SlitterCalibTextChgd[12])
                {
                    TM.CalibrateOffsets[12] = Convert.ToDouble(BandCalibTxt13.Text);
                }
                else
                {
                    TM.CalibrateOffsets[12] = TM.BandCalibration[12];
                }
                if (TM.SlitterCalibTextChgd[13])
                {
                    TM.CalibrateOffsets[13] = Convert.ToDouble(BandCalibTxt14.Text);
                }
                else
                {
                    TM.CalibrateOffsets[13] = TM.BandCalibration[13];
                }
                if (TM.SlitterCalibTextChgd[14])
                {
                    TM.CalibrateOffsets[14] = Convert.ToDouble(BandCalibTxt15.Text);
                }
                else
                {
                    TM.CalibrateOffsets[14] = TM.BandCalibration[14];
                }
                if (TM.SlitterCalibTextChgd[15])
                {
                    TM.CalibrateOffsets[15] = Convert.ToDouble(BandCalibTxt16.Text);
                }
                else
                {
                    TM.CalibrateOffsets[15] = TM.BandCalibration[15];
                }
                if (TM.SlitterCalibTextChgd[16])
                {
                    TM.CalibrateOffsets[16] = Convert.ToDouble(BandCalibTxt17.Text);
                }
                else
                {
                    TM.CalibrateOffsets[16] = TM.BandCalibration[16];
                }
                if (TM.SlitterCalibTextChgd[17])
                {
                    TM.CalibrateOffsets[17] = Convert.ToDouble(BandCalibTxt18.Text);
                }
                else
                {
                    TM.CalibrateOffsets[17] = TM.BandCalibration[17];
                }
                if (TM.SlitterCalibTextChgd[18])
                {
                    TM.CalibrateOffsets[18] = Convert.ToDouble(BandCalibTxt19.Text);
                }
                else
                {
                    TM.CalibrateOffsets[18] = TM.BandCalibration[18];
                }



                for (int x = 0; x < 19; x++)
                {
                    
                    TM.CalibrateCmdSelected[x] = true;
                }
            }
            else
            {
                for (int x = 0; x < 19; x++)
                {
                    TM.CalibrateCmdSelected[x] = TM.SlitterCalibTextChgd[x];
                    
                }
            }

            CalibParamsLoaded = false;
            Tmr1.Start();
            SlitterPLCWrites(0);
           
        }

        private void CoreChucksBtn_Click(object sender, RoutedEventArgs e)
        {
            Tmr1.Stop();

            string ConnectedMessage = "Connected";
            string ReadMessage = "Waiting for Connection";
            bool CmpResult = false;
            Int16 numb = 64;
            //Write Command Registers for Slitter Assignment, Auto Position Slitters Cmd, Position Core Chucks Cmd and Fault Reset Command  ****Write CoreChuck Cmd Bit to plc****
            ReadMessage = MB.WriteSingleRegiste(14199, numb);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            CommandWritten = true;
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                ModbusMessageTxtBx.Foreground = Brushes.Red;
                ModbusMessageTxtBx.Text += "Rx3i 10.10.10.110 Comm Failure (R14199) Method SlitterPLCWrites \n ";
                //MessageBox.Show("14200, Connection Closed " + ReadMessage);
            }

            ZeroOutPLCCommands();
            Tmr1.Start();
        }

        private void CleanOnBtn_Click(object sender, RoutedEventArgs e)
        {
            Tmr1.Stop();
            CleanOnBtn.Background = Brushes.Yellow;
            //Write Command Registers for Slitter Assignment = 1, Auto Position Slitters Cmd = 32, Position Core Chucks Cmd 64   ****Write Clean On Mode = 256 Bit to plc**** 256 +64 + 32 +1 = 353
            SlitterPLCWrites(353);
        }

        private void CLeanOffBtn_Click(object sender, RoutedEventArgs e)
        {
            Tmr1.Stop();
            CleanOnBtn.Background = Brushes.Transparent;
            //Write Command Registers Clean Off = 128 Mode ***Bit to plc*** 128 
            SlitterPLCWrites(128);
        }

        #endregion

        #region Manual Select Buttons

        private void MaintModeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode)
            {
                MaintModeBtn.Background = Brushes.Transparent;
                MaintMode = false;
                S1ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S2ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S3ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S4ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S5ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S6ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S7ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S8ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S9ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S10ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S11ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S12ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S13ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S14ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S15ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S16ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S17ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S18ManSelectBtn.Background = Brushes.PaleGoldenrod;
                S19ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            else
            {
                MaintModeBtn.Background = Brushes.Firebrick;
                MaintMode = true;

                for(Int32 x= 0; x < TM.MaxSlitters; x++)
                {
                    TM.ManualSelect[x] = false;
                }
            }

        }

        private void S1ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[0])
            {
                S1ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[0] = true;
            }
            else
            {
                S1ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[0] = false;
            }
            
        }

        private void S2ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[1])
            {
                S2ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[1] = true;
            }
            else
            {
                S2ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[1] = false;
            }
        }

        private void S3ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[2])
            {
                S3ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[2] = true;
            }
            else
            {
                S3ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[2] = false;
            }
        }


        private void S4ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[3])
            {
                S4ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[3] = true;
            }
            else
            {
                S4ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[3] = false;
            }
        }

        private void S5ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[4])
            {
                S5ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[4] = true;
            }
            else
            {
                S5ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[4] = false;
            }
        }

        private void S6ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[5])
            {
                S6ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[5] = true;
            }
            else
            {
                S6ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[5] = false;
            }
        }

        private void S7ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[6])
            {
                S7ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[6] = true;
            }
            else
            {
                S7ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[6] = false;
            }
        }

        private void S8ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[7])
            {
                S8ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[7] = true;
            }
            else
            {
                S8ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[7] = false;
            }
        }

        private void S9ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[8])
            {
                S9ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[8] = true;
            }
            else
            {
                S9ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[8] = false;
            }
        }

        private void S10ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[9])
            {
                S10ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[9] = true;
            }
            else
            {
                S10ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[9] = false;
            }
        }
        private void S11ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[10])
            {
                S11ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[10] = true;
            }
            else
            {
                S11ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[10] = false;
            }
        }

        private void S12ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[11])
            {
                S12ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[11] = true;
            }
            else
            {
                S12ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[11] = false;
            }
        }

        private void S13ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[12])
            {
                S13ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[12] = true;
            }
            else
            {
                S13ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[12] = false;
            }
        }

        private void S14ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[13])
            {
                S14ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[13] = true;
            }
            else
            {
                S14ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[13] = false;
            }
        }

        private void S15ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[14])
            {
                S15ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[14] = true;
            }
            else
            {
                S15ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[14] = false;
            }
        }

        private void S16ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[15])
            {
                S16ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[15] = true;
            }
            else
            {
                S16ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[15] = false;
            }
        }

        private void S17ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[16])
            {
                S17ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[16] = true;
            }
            else
            {
                S17ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[16] = false;
            }
        }

        private void S18ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[17])
            {
                S18ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[17] = true;
            }
            else
            {
                S18ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[17] = false;
            }
        }

        private void S19ManSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MaintMode && !TM.ManualSelect[18])
            {
                S19ManSelectBtn.Background = Brushes.Yellow;
                TM.ManualSelect[18] = true;
            }
            else
            {
                S19ManSelectBtn.Background = Brushes.PaleGoldenrod;
                TM.ManualSelect[18] = false;
            }
        }

        #endregion

        #region Diagnostic Logic
        private void DiagReset()
        {
            ShrinkTxtLbl.Visibility = Visibility.Hidden;
            CleanOnBtn.Visibility = Visibility.Hidden;
            CLeanOffBtn.Visibility = Visibility.Hidden;
            LoadParambtn.Visibility = Visibility.Hidden;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Hidden;
            OrderInfoTxtBx.Visibility = Visibility.Hidden;
            NoRollsLbl.Visibility = Visibility.Hidden;
            NumbOfRollsTxt.Visibility = Visibility.Hidden;
            ActCtsLbl.Visibility = Visibility.Hidden;
            ParkLimitLbl.Visibility = Visibility.Hidden;
            ParkCtsLbl.Visibility = Visibility.Hidden;
            TotalParkSlitTxt.Visibility = Visibility.Hidden;
            TotalActSlitTxt.Visibility = Visibility.Hidden;
            TotalParkSlitLimitTxt.Visibility = Visibility.Hidden;
            AutoCtrTrimChkBx.Visibility = Visibility.Hidden;
            Roll1ShrinkTxt.Visibility = Visibility.Hidden;
            Roll2ShrinkTxt.Visibility = Visibility.Hidden;
            Roll3ShrinkTxt.Visibility = Visibility.Hidden;
            Roll4ShrinkTxt.Visibility = Visibility.Hidden;
            Roll5ShrinkTxt.Visibility = Visibility.Hidden;
            Roll6ShrinkTxt.Visibility = Visibility.Hidden;
            Roll7ShrinkTxt.Visibility = Visibility.Hidden;
            Roll8ShrinkTxt.Visibility = Visibility.Hidden;
            Roll9ShrinkTxt.Visibility = Visibility.Hidden;
            Roll10ShrinkTxt.Visibility = Visibility.Hidden;
            Roll11ShrinkTxt.Visibility = Visibility.Hidden;
            Roll12ShrinkTxt.Visibility = Visibility.Hidden;
            Roll13ShrinkTxt.Visibility = Visibility.Hidden;
            Roll14ShrinkTxt.Visibility = Visibility.Hidden;
            Roll15ShrinkTxt.Visibility = Visibility.Hidden;
            Roll16ShrinkTxt.Visibility = Visibility.Hidden;
            Roll17ShrinkTxt.Visibility = Visibility.Hidden;
            Roll18ShrinkTxt.Visibility = Visibility.Hidden;
            Band1Err.Visibility = Visibility.Hidden;
            Band2Err.Visibility = Visibility.Hidden;
            Band3Err.Visibility = Visibility.Hidden;
            Band4Err.Visibility = Visibility.Hidden;
            Band5Err.Visibility = Visibility.Hidden;
            Band6Err.Visibility = Visibility.Hidden;
            Band7Err.Visibility = Visibility.Hidden;
            Band8Err.Visibility = Visibility.Hidden;
            Band9Err.Visibility = Visibility.Hidden;
            Band10Err.Visibility = Visibility.Hidden;
            Band11Err.Visibility = Visibility.Hidden;
            Band12Err.Visibility = Visibility.Hidden;
            Band13Err.Visibility = Visibility.Hidden;
            Band14Err.Visibility = Visibility.Hidden;
            Band15Err.Visibility = Visibility.Hidden;
            Band16Err.Visibility = Visibility.Hidden;
            Band17Err.Visibility = Visibility.Hidden;
            Band18Err.Visibility = Visibility.Hidden;
            Band19Err.Visibility = Visibility.Hidden;
            Blade1Err.Visibility = Visibility.Hidden;
            Blade2Err.Visibility = Visibility.Hidden;
            Blade3Err.Visibility = Visibility.Hidden;
            Blade4Err.Visibility = Visibility.Hidden;
            Blade5Err.Visibility = Visibility.Hidden;
            Blade6Err.Visibility = Visibility.Hidden;
            Blade7Err.Visibility = Visibility.Hidden;
            Blade8Err.Visibility = Visibility.Hidden;
            Blade9Err.Visibility = Visibility.Hidden;
            Blade10Err.Visibility = Visibility.Hidden;
            Blade11Err.Visibility = Visibility.Hidden;
            Blade12Err.Visibility = Visibility.Hidden;
            Blade13Err.Visibility = Visibility.Hidden;
            Blade14Err.Visibility = Visibility.Hidden;
            Blade15Err.Visibility = Visibility.Hidden;
            Blade16Err.Visibility = Visibility.Hidden;
            Blade17Err.Visibility = Visibility.Hidden;
            Blade18Err.Visibility = Visibility.Hidden;
            Blade19Err.Visibility = Visibility.Hidden;
         
            BandCalibTxt1.Visibility = Visibility.Hidden;
            BandCalibTxt2.Visibility = Visibility.Hidden;
            BandCalibTxt3.Visibility = Visibility.Hidden;
            BandCalibTxt4.Visibility = Visibility.Hidden;
            BandCalibTxt5.Visibility = Visibility.Hidden;
            BandCalibTxt6.Visibility = Visibility.Hidden;
            BandCalibTxt7.Visibility = Visibility.Hidden;
            BandCalibTxt8.Visibility = Visibility.Hidden;
            BandCalibTxt9.Visibility = Visibility.Hidden;
            BandCalibTxt10.Visibility = Visibility.Hidden;
            BandCalibTxt11.Visibility = Visibility.Hidden;
            BandCalibTxt12.Visibility = Visibility.Hidden;
            BandCalibTxt13.Visibility = Visibility.Hidden;
            BandCalibTxt14.Visibility = Visibility.Hidden;
            BandCalibTxt15.Visibility = Visibility.Hidden;
            BandCalibTxt16.Visibility = Visibility.Hidden;
            BandCalibTxt17.Visibility = Visibility.Hidden;
            BandCalibTxt18.Visibility = Visibility.Hidden;
            BandCalibTxt19.Visibility = Visibility.Hidden;
            S1ManSelectBtn.Visibility = Visibility.Hidden;
            S2ManSelectBtn.Visibility = Visibility.Hidden;
            S3ManSelectBtn.Visibility = Visibility.Hidden;
            S4ManSelectBtn.Visibility = Visibility.Hidden;
            S5ManSelectBtn.Visibility = Visibility.Hidden;
            S6ManSelectBtn.Visibility = Visibility.Hidden;
            S7ManSelectBtn.Visibility = Visibility.Hidden;
            S8ManSelectBtn.Visibility = Visibility.Hidden;
            S9ManSelectBtn.Visibility = Visibility.Hidden;
            S10ManSelectBtn.Visibility = Visibility.Hidden;
            S11ManSelectBtn.Visibility = Visibility.Hidden;
            S12ManSelectBtn.Visibility = Visibility.Hidden;
            S13ManSelectBtn.Visibility = Visibility.Hidden;
            S14ManSelectBtn.Visibility = Visibility.Hidden;
            S15ManSelectBtn.Visibility = Visibility.Hidden;
            S16ManSelectBtn.Visibility = Visibility.Hidden;
            S17ManSelectBtn.Visibility = Visibility.Hidden;
            S18ManSelectBtn.Visibility = Visibility.Hidden;
            S19ManSelectBtn.Visibility = Visibility.Hidden;
            MaintModeBtn.Visibility = Visibility.Hidden;

        }

        #endregion
       
        #region Band Calibration Text Changed
        private void BandCalibTxt1_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt1.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,7,7,8,9,. Allowed");
                BandCalibTxt1.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[0] = Convert.ToDouble(BandCalibTxt1.Text);
                TM.SlitterCalibTextChgd[0] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt2_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt2.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt2.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[1] = Convert.ToDouble(BandCalibTxt2.Text);
                TM.SlitterCalibTextChgd[1] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt3_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt3.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt3.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[2] = Convert.ToDouble(BandCalibTxt3.Text);
                TM.SlitterCalibTextChgd[2] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt4_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt4.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt4.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[3] = Convert.ToDouble(BandCalibTxt4.Text);
                TM.SlitterCalibTextChgd[3] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt5_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt5.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt5.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[4] = Convert.ToDouble(BandCalibTxt5.Text);
                TM.SlitterCalibTextChgd[4] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt6_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt6.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt6.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[5] = Convert.ToDouble(BandCalibTxt6.Text);
                TM.SlitterCalibTextChgd[5] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt7_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt7.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt7.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[6] = Convert.ToDouble(BandCalibTxt7.Text);
                TM.SlitterCalibTextChgd[6] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt8_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt8.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt8.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[7] = Convert.ToDouble(BandCalibTxt8.Text);
                TM.SlitterCalibTextChgd[7] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt9_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt9.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt9.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[8] = Convert.ToDouble(BandCalibTxt9.Text);
                TM.SlitterCalibTextChgd[8] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt10_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt10.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt10.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[9] = Convert.ToDouble(BandCalibTxt10.Text);
                TM.SlitterCalibTextChgd[9] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt11_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt11.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt11.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[10] = Convert.ToDouble(BandCalibTxt11.Text);
                TM.SlitterCalibTextChgd[10] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt12_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt12.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt12.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[11] = Convert.ToDouble(BandCalibTxt12.Text);
                TM.SlitterCalibTextChgd[11] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt13_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt13.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt13.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[12] = Convert.ToDouble(BandCalibTxt13.Text);
                TM.SlitterCalibTextChgd[12] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt14_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt14.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt14.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[13] = Convert.ToDouble(BandCalibTxt14.Text);
                TM.SlitterCalibTextChgd[13] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt15_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt15.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt15.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[14] = Convert.ToDouble(BandCalibTxt15.Text);
                TM.SlitterCalibTextChgd[14] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt16_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt16.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt16.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[15] = Convert.ToDouble(BandCalibTxt16.Text);
                TM.SlitterCalibTextChgd[15] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt17_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt17.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt17.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[16] = Convert.ToDouble(BandCalibTxt17.Text);
                TM.SlitterCalibTextChgd[16] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt18_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt18.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt18.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[17] = Convert.ToDouble(BandCalibTxt18.Text);
                TM.SlitterCalibTextChgd[17] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }

        private void BandCalibTxt19_TextChanged(object sender, EventArgs e)
        {
            Tmr1.Stop();
            bool canConVert = false;
            canConVert = decimal.TryParse(BandCalibTxt19.Text, out decimal i);
            if (!canConVert)
            {
                System.Windows.MessageBox.Show("Only 0,1,2,3,4,5,6,7,8,9,. Allowed");
                BandCalibTxt19.Text = "";
            }
            else
            {
                TM.CalibrateOffsets[18] = Convert.ToDouble(BandCalibTxt19.Text);
                TM.SlitterCalibTextChgd[18] = true;
            }
            TransferOffsetsToPLCBtn.Background = Brushes.Yellow;
            Tmr1.Start();
        }
        #endregion

        #region Auto Center Trim Chk Bx - Diag  Chk Bx
        private void AutoCtrTrimChkBx_Checked(object sender, RoutedEventArgs e)
        {
            CenterTrimOn = true;
            AcceptBtn.Background = Brushes.Yellow;
        }

        private void AutoCtrTrimChkBx_Unchecked(object sender, RoutedEventArgs e)
        {
            CenterTrimOn = false;
            AcceptBtn.Background = Brushes.Yellow;
        }
        private void DiagChkBx_Checked(object sender, RoutedEventArgs e)
        {
            DiagOn = true;
            ShrinkTxtLbl.Visibility = Visibility.Visible;
            CleanOnBtn.Visibility = Visibility.Visible;
            CLeanOffBtn.Visibility = Visibility.Visible;
            LoadParambtn.Visibility = Visibility.Visible;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Visible;
            OrderInfoTxtBx.Visibility = Visibility.Visible;
            NoRollsLbl.Visibility = Visibility.Visible;
            NumbOfRollsTxt.Visibility = Visibility.Visible;
            ActCtsLbl.Visibility = Visibility.Visible;
            ParkLimitLbl.Visibility = Visibility.Visible;
            ParkCtsLbl.Visibility = Visibility.Visible;
            TotalParkSlitTxt.Visibility = Visibility.Visible;
            TotalActSlitTxt.Visibility = Visibility.Visible;
            TotalParkSlitLimitTxt.Visibility = Visibility.Visible;
            AutoCtrTrimChkBx.Visibility = Visibility.Visible;
            Roll1ShrinkTxt.Visibility = Visibility.Visible;
            Roll2ShrinkTxt.Visibility = Visibility.Visible;
            Roll3ShrinkTxt.Visibility = Visibility.Visible;
            Roll4ShrinkTxt.Visibility = Visibility.Visible;
            Roll5ShrinkTxt.Visibility = Visibility.Visible;
            Roll6ShrinkTxt.Visibility = Visibility.Visible;
            Roll7ShrinkTxt.Visibility = Visibility.Visible;
            Roll8ShrinkTxt.Visibility = Visibility.Visible;
            Roll9ShrinkTxt.Visibility = Visibility.Visible;
            Roll10ShrinkTxt.Visibility = Visibility.Visible;
            Roll11ShrinkTxt.Visibility = Visibility.Visible;
            Roll12ShrinkTxt.Visibility = Visibility.Visible;
            Roll13ShrinkTxt.Visibility = Visibility.Visible;
            Roll14ShrinkTxt.Visibility = Visibility.Visible;
            Roll15ShrinkTxt.Visibility = Visibility.Visible;
            Roll16ShrinkTxt.Visibility = Visibility.Visible;
            Roll17ShrinkTxt.Visibility = Visibility.Visible;
            Roll18ShrinkTxt.Visibility = Visibility.Visible;
            Band1Err.Visibility = Visibility.Visible;
            Band2Err.Visibility = Visibility.Visible;
            Band3Err.Visibility = Visibility.Visible;
            Band4Err.Visibility = Visibility.Visible;
            Band5Err.Visibility = Visibility.Visible;
            Band6Err.Visibility = Visibility.Visible;
            Band7Err.Visibility = Visibility.Visible;
            Band8Err.Visibility = Visibility.Visible;
            Band9Err.Visibility = Visibility.Visible;
            Band10Err.Visibility = Visibility.Visible;
            Band11Err.Visibility = Visibility.Visible;
            Band12Err.Visibility = Visibility.Visible;
            Band13Err.Visibility = Visibility.Visible;
            Band14Err.Visibility = Visibility.Visible;
            Band15Err.Visibility = Visibility.Visible;
            Band16Err.Visibility = Visibility.Visible;
            Band17Err.Visibility = Visibility.Visible;
            Band18Err.Visibility = Visibility.Visible;
            Band19Err.Visibility = Visibility.Visible;
            Blade1Err.Visibility = Visibility.Visible;
            Blade2Err.Visibility = Visibility.Visible;
            Blade3Err.Visibility = Visibility.Visible;
            Blade4Err.Visibility = Visibility.Visible;
            Blade5Err.Visibility = Visibility.Visible;
            Blade6Err.Visibility = Visibility.Visible;
            Blade7Err.Visibility = Visibility.Visible;
            Blade8Err.Visibility = Visibility.Visible;
            Blade9Err.Visibility = Visibility.Visible;
            Blade10Err.Visibility = Visibility.Visible;
            Blade11Err.Visibility = Visibility.Visible;
            Blade12Err.Visibility = Visibility.Visible;
            Blade13Err.Visibility = Visibility.Visible;
            Blade14Err.Visibility = Visibility.Visible;
            Blade15Err.Visibility = Visibility.Visible;
            Blade16Err.Visibility = Visibility.Visible;
            Blade17Err.Visibility = Visibility.Visible;
            Blade18Err.Visibility = Visibility.Visible;
            Blade19Err.Visibility = Visibility.Visible;

            BandCalibTxt1.Visibility = Visibility.Visible;
            BandCalibTxt2.Visibility = Visibility.Visible;
            BandCalibTxt3.Visibility = Visibility.Visible;
            BandCalibTxt4.Visibility = Visibility.Visible;
            BandCalibTxt5.Visibility = Visibility.Visible;
            BandCalibTxt6.Visibility = Visibility.Visible;
            BandCalibTxt7.Visibility = Visibility.Visible;
            BandCalibTxt8.Visibility = Visibility.Visible;
            BandCalibTxt9.Visibility = Visibility.Visible;
            BandCalibTxt10.Visibility = Visibility.Visible;
            BandCalibTxt11.Visibility = Visibility.Visible;
            BandCalibTxt12.Visibility = Visibility.Visible;
            BandCalibTxt13.Visibility = Visibility.Visible;
            BandCalibTxt14.Visibility = Visibility.Visible;
            BandCalibTxt15.Visibility = Visibility.Visible;
            BandCalibTxt16.Visibility = Visibility.Visible;
            BandCalibTxt17.Visibility = Visibility.Visible;
            BandCalibTxt18.Visibility = Visibility.Visible;
            BandCalibTxt19.Visibility = Visibility.Visible;
            S1ManSelectBtn.Visibility = Visibility.Visible;
            S2ManSelectBtn.Visibility = Visibility.Visible;
            S3ManSelectBtn.Visibility = Visibility.Visible;
            S4ManSelectBtn.Visibility = Visibility.Visible;
            S5ManSelectBtn.Visibility = Visibility.Visible;
            S6ManSelectBtn.Visibility = Visibility.Visible;
            S7ManSelectBtn.Visibility = Visibility.Visible;
            S8ManSelectBtn.Visibility = Visibility.Visible;
            S9ManSelectBtn.Visibility = Visibility.Visible;
            S10ManSelectBtn.Visibility = Visibility.Visible;
            S11ManSelectBtn.Visibility = Visibility.Visible;
            S12ManSelectBtn.Visibility = Visibility.Visible;
            S13ManSelectBtn.Visibility = Visibility.Visible;
            S14ManSelectBtn.Visibility = Visibility.Visible;
            S15ManSelectBtn.Visibility = Visibility.Visible;
            S16ManSelectBtn.Visibility = Visibility.Visible;
            S17ManSelectBtn.Visibility = Visibility.Visible;
            S18ManSelectBtn.Visibility = Visibility.Visible;
            S19ManSelectBtn.Visibility = Visibility.Visible;

            if (Properties.Settings.Default.MaintModeEn)
            {
                MaintModeBtn.Visibility = Visibility.Visible;
            }
            else
            {
                MaintModeBtn.Visibility = Visibility.Hidden;
            }
            
            //Clear out Slitter Calib Text Changed in case value is entered but view of calibration is invisible
            for (int x = 0; x < TM.MaxSlitters; x++)
            {
                TM.SlitterCalibTextChgd[x] = false;
                TM.CalibrateCmdSelected[x] = false;
            }

        }

        private void DiagChkBx_Unchecked(object sender, RoutedEventArgs e)
        {
            DiagOn = false;
            LoadParambtn.Visibility = Visibility.Hidden;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Hidden;
            ShrinkTxtLbl.Visibility = Visibility.Hidden;
            CleanOnBtn.Visibility = Visibility.Hidden;
            CLeanOffBtn.Visibility = Visibility.Hidden;
            OrderInfoTxtBx.Visibility = Visibility.Hidden;
            NoRollsLbl.Visibility = Visibility.Hidden;
            ActCtsLbl.Visibility = Visibility.Hidden;
            ParkLimitLbl.Visibility = Visibility.Hidden;
            ParkCtsLbl.Visibility = Visibility.Hidden;
            TotalParkSlitTxt.Visibility = Visibility.Hidden;
            TotalActSlitTxt.Visibility = Visibility.Hidden;
            TotalParkSlitLimitTxt.Visibility = Visibility.Hidden;
            NumbOfRollsTxt.Visibility = Visibility.Hidden;
            AutoCtrTrimChkBx.Visibility = Visibility.Hidden;
            Roll1ShrinkTxt.Visibility = Visibility.Hidden;
            Roll2ShrinkTxt.Visibility = Visibility.Hidden;
            Roll3ShrinkTxt.Visibility = Visibility.Hidden;
            Roll4ShrinkTxt.Visibility = Visibility.Hidden;
            Roll5ShrinkTxt.Visibility = Visibility.Hidden;
            Roll6ShrinkTxt.Visibility = Visibility.Hidden;
            Roll7ShrinkTxt.Visibility = Visibility.Hidden;
            Roll8ShrinkTxt.Visibility = Visibility.Hidden;
            Roll9ShrinkTxt.Visibility = Visibility.Hidden;
            Roll10ShrinkTxt.Visibility = Visibility.Hidden;
            Roll11ShrinkTxt.Visibility = Visibility.Hidden;
            Roll12ShrinkTxt.Visibility = Visibility.Hidden;
            Roll13ShrinkTxt.Visibility = Visibility.Hidden;
            Roll14ShrinkTxt.Visibility = Visibility.Hidden;
            Roll15ShrinkTxt.Visibility = Visibility.Hidden;
            Roll16ShrinkTxt.Visibility = Visibility.Hidden;
            Roll17ShrinkTxt.Visibility = Visibility.Hidden;
            Roll18ShrinkTxt.Visibility = Visibility.Hidden;
            Band1Err.Visibility = Visibility.Hidden;
            Band2Err.Visibility = Visibility.Hidden;
            Band3Err.Visibility = Visibility.Hidden;
            Band4Err.Visibility = Visibility.Hidden;
            Band5Err.Visibility = Visibility.Hidden;
            Band6Err.Visibility = Visibility.Hidden;
            Band7Err.Visibility = Visibility.Hidden;
            Band8Err.Visibility = Visibility.Hidden;
            Band9Err.Visibility = Visibility.Hidden;
            Band10Err.Visibility = Visibility.Hidden;
            Band11Err.Visibility = Visibility.Hidden;
            Band12Err.Visibility = Visibility.Hidden;
            Band13Err.Visibility = Visibility.Hidden;
            Band14Err.Visibility = Visibility.Hidden;
            Band15Err.Visibility = Visibility.Hidden;
            Band16Err.Visibility = Visibility.Hidden;
            Band17Err.Visibility = Visibility.Hidden;
            Band18Err.Visibility = Visibility.Hidden;
            Band19Err.Visibility = Visibility.Hidden;
            Blade1Err.Visibility = Visibility.Hidden;
            Blade2Err.Visibility = Visibility.Hidden;
            Blade3Err.Visibility = Visibility.Hidden;
            Blade4Err.Visibility = Visibility.Hidden;
            Blade5Err.Visibility = Visibility.Hidden;
            Blade6Err.Visibility = Visibility.Hidden;
            Blade7Err.Visibility = Visibility.Hidden;
            Blade8Err.Visibility = Visibility.Hidden;
            Blade9Err.Visibility = Visibility.Hidden;
            Blade10Err.Visibility = Visibility.Hidden;
            Blade11Err.Visibility = Visibility.Hidden;
            Blade12Err.Visibility = Visibility.Hidden;
            Blade13Err.Visibility = Visibility.Hidden;
            Blade14Err.Visibility = Visibility.Hidden;
            Blade15Err.Visibility = Visibility.Hidden;
            Blade16Err.Visibility = Visibility.Hidden;
            Blade17Err.Visibility = Visibility.Hidden;
            Blade18Err.Visibility = Visibility.Hidden;
            Blade19Err.Visibility = Visibility.Hidden;

            BandCalibTxt1.Visibility = Visibility.Hidden;
            BandCalibTxt2.Visibility = Visibility.Hidden;
            BandCalibTxt3.Visibility = Visibility.Hidden;
            BandCalibTxt4.Visibility = Visibility.Hidden;
            BandCalibTxt5.Visibility = Visibility.Hidden;
            BandCalibTxt6.Visibility = Visibility.Hidden;
            BandCalibTxt7.Visibility = Visibility.Hidden;
            BandCalibTxt8.Visibility = Visibility.Hidden;
            BandCalibTxt9.Visibility = Visibility.Hidden;
            BandCalibTxt10.Visibility = Visibility.Hidden;
            BandCalibTxt11.Visibility = Visibility.Hidden;
            BandCalibTxt12.Visibility = Visibility.Hidden;
            BandCalibTxt13.Visibility = Visibility.Hidden;
            BandCalibTxt14.Visibility = Visibility.Hidden;
            BandCalibTxt15.Visibility = Visibility.Hidden;
            BandCalibTxt16.Visibility = Visibility.Hidden;
            BandCalibTxt17.Visibility = Visibility.Hidden;
            BandCalibTxt18.Visibility = Visibility.Hidden;
            BandCalibTxt19.Visibility = Visibility.Hidden;
            S1ManSelectBtn.Visibility = Visibility.Hidden;
            S2ManSelectBtn.Visibility = Visibility.Hidden;
            S3ManSelectBtn.Visibility = Visibility.Hidden;
            S4ManSelectBtn.Visibility = Visibility.Hidden;
            S5ManSelectBtn.Visibility = Visibility.Hidden;
            S6ManSelectBtn.Visibility = Visibility.Hidden;
            S7ManSelectBtn.Visibility = Visibility.Hidden;
            S8ManSelectBtn.Visibility = Visibility.Hidden;
            S9ManSelectBtn.Visibility = Visibility.Hidden;
            S10ManSelectBtn.Visibility = Visibility.Hidden;
            S11ManSelectBtn.Visibility = Visibility.Hidden;
            S12ManSelectBtn.Visibility = Visibility.Hidden;
            S13ManSelectBtn.Visibility = Visibility.Hidden;
            S14ManSelectBtn.Visibility = Visibility.Hidden;
            S15ManSelectBtn.Visibility = Visibility.Hidden;
            S16ManSelectBtn.Visibility = Visibility.Hidden;
            S17ManSelectBtn.Visibility = Visibility.Hidden;
            S18ManSelectBtn.Visibility = Visibility.Hidden;
            S19ManSelectBtn.Visibility = Visibility.Hidden;
            MaintModeBtn.Background = Brushes.Transparent;
            MaintModeBtn.Visibility = Visibility.Hidden;
            MaintMode = false;
            S1ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S2ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S3ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S4ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S5ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S6ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S7ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S8ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S9ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S10ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S11ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S12ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S13ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S14ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S15ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S16ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S17ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S18ManSelectBtn.Background = Brushes.PaleGoldenrod;
            S19ManSelectBtn.Background = Brushes.PaleGoldenrod;

            //Clear out Slitter Calib Text Changed in case value is entered but view of calibration is invisible
            for (int x = 0; x < TM.MaxSlitters; x++)
            {
                TM.SlitterCalibTextChgd[x] = false;
                TM.CalibrateCmdSelected[x] = false;
                TM.ManualSelect[x] = false;
            }
           
        }

        #endregion

        private void CheckXMLFileExists()
        {
            try
            {
                String path1 = "RollData.xml";
                
                if (File.Exists(path1))
                {
                   DeSerializeDataSet();
                }
                else
                {
                    SerializeDataSet();
                }


            }
            catch
            {
                System.Windows.MessageBox.Show("XML Serialization failed","RollData.xml", MessageBoxButton.OKCancel);
            }
            finally
            {

            }

        }

        public void SerializeDataSet()
        {
            String path1 = "RollData.xml";
            StreamWriter writer = new StreamWriter(path1);
            try
            {
                XmlSerializer WXML = new XmlSerializer(typeof(WrapRollData));
                //wrp.RollWidthData = TM.WrapData;
                wrp.BandLowerLimit = TM.BandLowerLimit;
                wrp.BandUpperLimit = TM.BandUpperLimit;
                wrp.BladeLowerLimit = TM.BladeLowerLimit;
                wrp.BladeUpperLimit = TM.BladeUpperLimit;
               
                WXML.Serialize(writer, wrp);
            }
            catch
            {
                System.Windows.MessageBox.Show("XML Write Serialization failed", "RollData.xml", MessageBoxButton.OKCancel);
            }
            finally
            {
                writer.Close();
                System.Windows.MessageBox.Show("XML  Write Serialization Successful", "RollData.xml", MessageBoxButton.OKCancel);
            }
            
                             
        }
        public void DeSerializeDataSet()
        {
            String path1 = "RollData.xml";
            StreamReader reader = new StreamReader(path1);
            try
            {
                XmlSerializer RXML = new XmlSerializer(typeof(WrapRollData));

                wrp = (WrapRollData)RXML.Deserialize(reader);
                TM.BandLowerLimit = wrp.BandLowerLimit;
                TM.BandUpperLimit = wrp.BandUpperLimit;
                TM.BladeLowerLimit = wrp.BladeLowerLimit;
                TM.BladeUpperLimit = wrp.BladeUpperLimit;
            }
            catch
            {
                System.Windows.MessageBox.Show("XML Read Serialization failed", "RollData.xml", MessageBoxButton.OKCancel);
            }
            finally
            {
                reader.Close();
                System.Windows.MessageBox.Show("XML Read Serialization Successful", "RollData.xml", MessageBoxButton.OKCancel);
            }

            
            
        }

        private void SlitterAppExit_Click(object sender, RoutedEventArgs e)
        {
            SerializeDataSet();
            Application.Current.Shutdown();
        }
    }
}
