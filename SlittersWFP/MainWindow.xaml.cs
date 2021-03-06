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
    /// Interaction logic for MainWindow.xaml Node 10.10.10.110 Version 5.6
    /// Drop Down boxes added for slitter disable and out of service
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
        public float ComboboxWidthMouseEnter = 400.0f;
        public float ComboboxWidthMouseLeave = 130.0f;
        public Int32 OutOfServBrkPtComboBox = 6;
        Boolean OutOfTolerance = false;
        Boolean OutofToleranceDisable = false;
        Boolean MaintMode = false;
        public Boolean DisableBandCalibMsg = false;
        Boolean RollCheckFlt = false;
        Int32 Ctr = 0;
        public String OrderInfo = "";

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
            InitDropBox();            
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

        #region Initialize Combo boxes
        private void InitDropBox()
        {
            S1ComboBox.SelectedIndex = 0;
            S2ComboBox.SelectedIndex = 0;
            S3ComboBox.SelectedIndex = 0;
            S4ComboBox.SelectedIndex = 0;
            S5ComboBox.SelectedIndex = 0;
            S6ComboBox.SelectedIndex = 0;
            S7ComboBox.SelectedIndex = 0;
            S8ComboBox.SelectedIndex = 0;
            S9ComboBox.SelectedIndex = 0;
            S10ComboBox.SelectedIndex = 0;
            S11ComboBox.SelectedIndex = 0;
            S12ComboBox.SelectedIndex = 0;
            S13ComboBox.SelectedIndex = 0;
            S14ComboBox.SelectedIndex = 0;
            S15ComboBox.SelectedIndex = 0;
            S16ComboBox.SelectedIndex = 0;
            S17ComboBox.SelectedIndex = 0;
            S18ComboBox.SelectedIndex = 0;
            S19ComboBox.SelectedIndex = 0;

            String Msg0 = "Enabled";
            String Msg1 = "Side Load";
            String Msg2 = "Air Failure";
            String Msg3 = "Rough Cut";
            String Msg4 = "Bad Band";
            String Msg5 = "Bad Blade";
            String Msg6 = "Dis Other";
            String Msg7 = "OOS Elec";
            String Msg8 = "OOS Mech";
            String Msg9 = "OOS PosnErr";
            String Msg10 = "OOS Other";


            S1ComboBox.Items.Add(Msg0);
            /*S1ComboBox.Items.Add(Msg1);
            S1ComboBox.Items.Add(Msg2);
            S1ComboBox.Items.Add(Msg3);
            S1ComboBox.Items.Add(Msg4);
            S1ComboBox.Items.Add(Msg5);
            S1ComboBox.Items.Add(Msg6);
            S1ComboBox.Items.Add(Msg7);
            S1ComboBox.Items.Add(Msg8);
            S1ComboBox.Items.Add(Msg9);
            S1ComboBox.Items.Add(Msg10);*/
            
            S2ComboBox.Items.Add(Msg0);
            S2ComboBox.Items.Add(Msg1);
            S2ComboBox.Items.Add(Msg2);
            S2ComboBox.Items.Add(Msg3);
            S2ComboBox.Items.Add(Msg4);
            S2ComboBox.Items.Add(Msg5);
            S2ComboBox.Items.Add(Msg6);
            S2ComboBox.Items.Add(Msg7);
            S2ComboBox.Items.Add(Msg8);
            S2ComboBox.Items.Add(Msg9);
            S2ComboBox.Items.Add(Msg10);
            
            S3ComboBox.Items.Add(Msg0);
            S3ComboBox.Items.Add(Msg1);
            S3ComboBox.Items.Add(Msg2);
            S3ComboBox.Items.Add(Msg3);
            S3ComboBox.Items.Add(Msg4);
            S3ComboBox.Items.Add(Msg5);
            S3ComboBox.Items.Add(Msg6);
            S3ComboBox.Items.Add(Msg8);
            S3ComboBox.Items.Add(Msg9);
            S3ComboBox.Items.Add(Msg10);
            
            S4ComboBox.Items.Add(Msg0);
            S4ComboBox.Items.Add(Msg1);
            S4ComboBox.Items.Add(Msg2);
            S4ComboBox.Items.Add(Msg3);
            S4ComboBox.Items.Add(Msg4);
            S4ComboBox.Items.Add(Msg5);
            S4ComboBox.Items.Add(Msg6);
            S4ComboBox.Items.Add(Msg8);
            S4ComboBox.Items.Add(Msg9);
            S4ComboBox.Items.Add(Msg10);
            
            S5ComboBox.Items.Add(Msg0);
            S5ComboBox.Items.Add(Msg1);
            S5ComboBox.Items.Add(Msg2);
            S5ComboBox.Items.Add(Msg3);
            S5ComboBox.Items.Add(Msg4);
            S5ComboBox.Items.Add(Msg5);
            S5ComboBox.Items.Add(Msg6);
            S5ComboBox.Items.Add(Msg8);
            S5ComboBox.Items.Add(Msg9);
            S5ComboBox.Items.Add(Msg10);
            
            S6ComboBox.Items.Add(Msg0);
            S6ComboBox.Items.Add(Msg1);
            S6ComboBox.Items.Add(Msg2);
            S6ComboBox.Items.Add(Msg3);
            S6ComboBox.Items.Add(Msg4);
            S6ComboBox.Items.Add(Msg5);
            S6ComboBox.Items.Add(Msg6);
            S6ComboBox.Items.Add(Msg8);
            S6ComboBox.Items.Add(Msg9);
            S6ComboBox.Items.Add(Msg10);
            
            S7ComboBox.Items.Add(Msg0);
            S7ComboBox.Items.Add(Msg1);
            S7ComboBox.Items.Add(Msg2);
            S7ComboBox.Items.Add(Msg3);
            S7ComboBox.Items.Add(Msg4);
            S7ComboBox.Items.Add(Msg5);
            S7ComboBox.Items.Add(Msg6);
            S7ComboBox.Items.Add(Msg8);
            S7ComboBox.Items.Add(Msg9);
            S7ComboBox.Items.Add(Msg10);
            
            S8ComboBox.Items.Add(Msg0);
            S8ComboBox.Items.Add(Msg1);
            S8ComboBox.Items.Add(Msg2);
            S8ComboBox.Items.Add(Msg3);
            S8ComboBox.Items.Add(Msg4);
            S8ComboBox.Items.Add(Msg5);
            S8ComboBox.Items.Add(Msg6);
            S8ComboBox.Items.Add(Msg7);
            S8ComboBox.Items.Add(Msg8);
            S8ComboBox.Items.Add(Msg9);
            S8ComboBox.Items.Add(Msg10);
            
            S9ComboBox.Items.Add(Msg0);
            S9ComboBox.Items.Add(Msg1);
            S9ComboBox.Items.Add(Msg2);
            S9ComboBox.Items.Add(Msg3);
            S9ComboBox.Items.Add(Msg4);
            S9ComboBox.Items.Add(Msg5);
            S9ComboBox.Items.Add(Msg6);
            S9ComboBox.Items.Add(Msg7);
            S9ComboBox.Items.Add(Msg8);
            S9ComboBox.Items.Add(Msg9);
            S9ComboBox.Items.Add(Msg10);
            
            S10ComboBox.Items.Add(Msg0);
            S10ComboBox.Items.Add(Msg1);
            S10ComboBox.Items.Add(Msg2);
            S10ComboBox.Items.Add(Msg3);
            S10ComboBox.Items.Add(Msg4);
            S10ComboBox.Items.Add(Msg5);
            S10ComboBox.Items.Add(Msg6);
            S10ComboBox.Items.Add(Msg7);
            S10ComboBox.Items.Add(Msg8);
            S10ComboBox.Items.Add(Msg9);
            S10ComboBox.Items.Add(Msg10);
            
            S11ComboBox.Items.Add(Msg0);
            S11ComboBox.Items.Add(Msg1);
            S11ComboBox.Items.Add(Msg2);
            S11ComboBox.Items.Add(Msg3);
            S11ComboBox.Items.Add(Msg4);
            S11ComboBox.Items.Add(Msg5);
            S11ComboBox.Items.Add(Msg6);
            S11ComboBox.Items.Add(Msg7);
            S11ComboBox.Items.Add(Msg8);
            S11ComboBox.Items.Add(Msg9);
            S11ComboBox.Items.Add(Msg10);
            
            S12ComboBox.Items.Add(Msg0);
            S12ComboBox.Items.Add(Msg1);
            S12ComboBox.Items.Add(Msg2);
            S12ComboBox.Items.Add(Msg3);
            S12ComboBox.Items.Add(Msg4);
            S12ComboBox.Items.Add(Msg5);
            S12ComboBox.Items.Add(Msg6);
            S12ComboBox.Items.Add(Msg7);
            S12ComboBox.Items.Add(Msg8);
            S12ComboBox.Items.Add(Msg9);
            S12ComboBox.Items.Add(Msg10);

            S13ComboBox.Items.Add(Msg0);
            S13ComboBox.Items.Add(Msg1);
            S13ComboBox.Items.Add(Msg2);
            S13ComboBox.Items.Add(Msg3);
            S13ComboBox.Items.Add(Msg4);
            S13ComboBox.Items.Add(Msg5);
            S13ComboBox.Items.Add(Msg6);
            S13ComboBox.Items.Add(Msg7);
            S13ComboBox.Items.Add(Msg8);
            S13ComboBox.Items.Add(Msg9);
            S13ComboBox.Items.Add(Msg10);
            
            S14ComboBox.Items.Add(Msg0);
            S14ComboBox.Items.Add(Msg1);
            S14ComboBox.Items.Add(Msg2);
            S14ComboBox.Items.Add(Msg3);
            S14ComboBox.Items.Add(Msg4);
            S14ComboBox.Items.Add(Msg5);
            S14ComboBox.Items.Add(Msg6);
            S14ComboBox.Items.Add(Msg7);
            S14ComboBox.Items.Add(Msg8);
            S14ComboBox.Items.Add(Msg10);
            
            S15ComboBox.Items.Add(Msg0);
            S15ComboBox.Items.Add(Msg1);
            S15ComboBox.Items.Add(Msg2);
            S15ComboBox.Items.Add(Msg3);
            S15ComboBox.Items.Add(Msg4);
            S15ComboBox.Items.Add(Msg5);
            S15ComboBox.Items.Add(Msg6);
            S15ComboBox.Items.Add(Msg7);
            S15ComboBox.Items.Add(Msg8);
            S15ComboBox.Items.Add(Msg9);
            S15ComboBox.Items.Add(Msg10);
            
            S16ComboBox.Items.Add(Msg0);
            S16ComboBox.Items.Add(Msg1);
            S16ComboBox.Items.Add(Msg2);
            S16ComboBox.Items.Add(Msg3);
            S16ComboBox.Items.Add(Msg4);
            S16ComboBox.Items.Add(Msg5);
            S16ComboBox.Items.Add(Msg6);
            S16ComboBox.Items.Add(Msg7);
            S16ComboBox.Items.Add(Msg8);
            S16ComboBox.Items.Add(Msg9);
            S16ComboBox.Items.Add(Msg10);

            S17ComboBox.Items.Add(Msg0);
            S17ComboBox.Items.Add(Msg1);
            S17ComboBox.Items.Add(Msg2);
            S17ComboBox.Items.Add(Msg3);
            S17ComboBox.Items.Add(Msg4);
            S17ComboBox.Items.Add(Msg5);
            S17ComboBox.Items.Add(Msg6);
            S17ComboBox.Items.Add(Msg7);
            S17ComboBox.Items.Add(Msg8);
            S17ComboBox.Items.Add(Msg9);
            S17ComboBox.Items.Add(Msg10);

            S18ComboBox.Items.Add(Msg0);
            S18ComboBox.Items.Add(Msg1);
            S18ComboBox.Items.Add(Msg2);
            S18ComboBox.Items.Add(Msg3);
            S18ComboBox.Items.Add(Msg4);
            S18ComboBox.Items.Add(Msg5);
            S18ComboBox.Items.Add(Msg6);
            S18ComboBox.Items.Add(Msg7);
            S18ComboBox.Items.Add(Msg8);
            S18ComboBox.Items.Add(Msg9);
            S18ComboBox.Items.Add(Msg10);

            S19ComboBox.Items.Add(Msg0);
            /*S19ComboBox.Items.Add(Msg1);
            S19ComboBox.Items.Add(Msg2);
            S19ComboBox.Items.Add(Msg3);
            S19ComboBox.Items.Add(Msg4);
            S19ComboBox.Items.Add(Msg5);
            S19ComboBox.Items.Add(Msg6);
            S19ComboBox.Items.Add(Msg7);
            S19ComboBox.Items.Add(Msg8);
            S19ComboBox.Items.Add(Msg9);
            S19ComboBox.Items.Add(Msg10);*/
        }

        #endregion

        #region Open Modbus Connection
        private void OpenModbusConnection()

        {
            String plciptype = " Rx3i 10.10.10.110 ";
            plciptype += MB.ModbusOpenConnection();
            SysMsgListBx.Items.Add(plciptype);
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14024) Method LoadInitialPLCData");
            }

            //Read Last Ten Slitters Position Starting Register - Number of Registers - 2 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14060, 40, 2);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14060) Method LoadInitialPLCData");
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14225) Method LoadInitialPLCData");
            }

            //Read Last Ten Slitters Setpoints and Calibrate Setpoints Starting Register - Number of Registers - 4 = return data to slitter and calibrate setpoint Array
            ReadMessage = MB.ReadHoldingRegisters(14260, 40, 4);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14261) Method LoadInitialPLCData");
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14000) Method TimerCyle1");

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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14205) Method LoadInitialPLCData");
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
                    OpenModbusConnection();
                    TimerCycle1();
                    Tmr2.Stop();
                    break;
                case MessageBoxResult.No:
                    SysMsgListBx.Items.Add("Ping 10.10.10.110 to Verify Communication");
                    break;
                case MessageBoxResult.Cancel:
                    SysMsgListBx.Items.Add("Ping 10.10.10.110 to Verify Communication");
                    break;
            }
            
        }
        #endregion

        #region Timer Cycle 1
        private void TimerCycle1()
        {
            //Counter for delays
            Ctr++;
            if(Ctr > 10)
            {
                Ctr = 0;
            }

            dt = DateTime.Now;
            DateTimeTxtBx.Text = dt.ToString("f");

            SelectionOfSlittersLabelTextColor();

            // Stop timer while reading Registers
            Tmr1.Stop();
            SlitterPositionColorCntrl();
            UpdateSlitterError();
            SlitCalibTextChangedColor();

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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14025) Method TimerCyle1");
                

            }

            //Read Next Ten Slitters Position Starting Register - Number of Registers and 1 = return data to slitter Position Array
            ReadMessage = MB.ReadHoldingRegisters(14060, 40, 2);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14061) Method TimerCyle1");
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14150) Method TimerCyle1");
                
            }

            //Update Fault table on Main Window
            UpdateFltMsgListBox();
            
            //Read Command Registers Starting Register - Number of Registers - 6 = return data to slitter and calibrate setpoint Array - goes to CommandRegisters
            ReadMessage = MB.ReadHoldingRegisters(14204, 8, 6);
            CmpResult = string.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                CommunicatonPLCFailure = true;
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14205) Method LoadInitialPLCData");
            }

            //Transfer data from Modbus object to PLc object and convert registers into array bits
            PLC.CmdReg = PLC.CmdRegisterBitConverter(MB.CommandRegisters);
            TM.SlittersSelectedPLC = PLC.BitReadForActiveSlitters();
                        
            //Check to see if out of service bits are on in plc during application startup
            TM.SlitterOutofServicePLC = PLC.BitReadOutofServiceCmd();

            //Read PLC Control Registers R14000 eg. Slitter Auto Positing On. Writes data to PLcControlInputs[] in ModbusComm class
            ReadMessage = MB.ReadHoldingRegisters(13999, 2, 8);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14000) Method TimerCyle1");

            }
            // PLCControlBits used to use color of Band and Blade Text Box  Use different bit  this bit flashes. [0,1] = Auto Positioning, pusles on and off - [0,4] STF to VFD On
            PLC.PLCControlBits = PLC.CntrlRegisterBitConverter(MB.PLcControlInputs);
            if (PLC.PLCControlBits[0,4])
            {
                BandTxtBx.Background = Brushes.LightGreen;
                BladeTxtBx.Background = Brushes.LightGreen;
                TM.InPosWindow = 0.125;
                OrderInfoLbl.Background = Brushes.GreenYellow;
                OrderInfoLbl.Content = "Positioning";

            }
            else
            {
                BandTxtBx.Background = Brushes.Transparent;
                BladeTxtBx.Background = Brushes.Transparent;
                TM.InPosWindow = 1.00;
                OrderInfoLbl.Background = Brushes.Transparent;
                OrderInfoLbl.Content = "Order Number";

            }

            // Check Roll Setpoints to slitters selected - [0,4] STF to VFD On
            if (!PLC.PLCControlBits[0, 4] && PLC.PLCControlBits[0, 15] && Ctr > 8 && RollCheckFlt)
            {
               Boolean CheckRollFault = TM.RollWidthCheck();
                if (CheckRollFault)
                {
                    SysMsgListBx.Items.Add("Order Mismatch to Slitters");
                    RollCheckFlt = false;
                }
            }

           
            //Write PLC Control Writes to PLCWrites in MB
            ReadMessage = MB.ReadHoldingRegisters(14199, 2, 9);
            CmpResult = String.Equals(ReadMessage, ConnectedMessage);
            if (CmpResult == false && CommunicatonPLCFailure == false)
            {
                MB.ModbusCloseConnection();
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14200) Method TimerCyle1");

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
                    SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14200) Method TimerCycle1");

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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14300) Method TimerCyle1");
                
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
                    SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14196) Method SlitterLifeCycleResets");
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
                    SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14196) Method SlitterLifeCycleResets");
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

        #region Wrap Order Calculations on Order Sent

        private void WrapOrderInit()
        {
            Boolean ActPosnCheck = false;
            Boolean ParkPosnCheck = false;
            Boolean ParkLimitCheck = false;

            TM.ZeroOutWrapData();
            TM.ZeroOutSlitterData();
            TM.WrapData = DataSort.GetData();
            OrderInfo = TCPServSocket.wraptext;
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
                SysMsgListBx.Items.Add("Maximum Trim Exceeded");
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

            //Check Calibration Position mode for alogorithum
            if ((TM.NumbOfRolls + 1) == TM.NumbOfSlitParkSelected)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ParkPosnSp[x] = 0.0;
                    TM.ParkPosnSpPartial[x] = 0.0;
                }
                TM.ParkPosnSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectPark);
                TM.ParkPosnSp = TM.CalcBandStptsNotUsedParkPosn(TM.ParkPosnSpPartial, TM.BandParkSelected);
                ParkPosnCheck = TM.VerifySlitterSetpoints(TM.ParkPosnSp);
                if (ParkPosnCheck)
                {
                    TM.BandStpt = TM.ParkPosnSp;
                    SysMsgListBx.Items.Add("Solution ParkPosn Selected");
                }
                SelectedSolutionTxt.Text = "ParkPosn";
            }
            //Check Actual Position mode for alogorithum
            if ((TM.NumbOfRolls + 1) == TM.NumbOfSlitActPosSelected && !ParkPosnCheck)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ActPosnSp[x] = 0.0;
                    TM.ActPosnSpPartial[x] = 0.0;
                }
                TM.ActPosnSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectAct);
                TM.ActPosnSp = TM.CalcBandStptsNotUsedActPosn(TM.ActPosnSpPartial, TM.BandActPosnSelected);
                ActPosnCheck = TM.VerifySlitterSetpoints(TM.ActPosnSp);
                if (ActPosnCheck)
                {
                    TM.BandStpt = TM.ActPosnSp;
                    SysMsgListBx.Items.Add("Solution ActPosn Selected");
                }
                SelectedSolutionTxt.Text = "ActPosn";
            }
            //Check Park Limit Position mode for alogorithum
            if (((TM.NumbOfRolls + 1) == TM.NumbOfslitParkSelectdLmt) && !ParkPosnCheck && !ActPosnCheck)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ParkLmtSp[x] = 0.0;
                    TM.ParkLmtSpPartial[x] = 0.0;
                }
                TM.ParkLmtSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectParkLmt);
                TM.ParkLmtSp = TM.CalcBandStptsNotUsedParkLmt(TM.ParkLmtSpPartial, TM.BandParkLimitSelected);
                ParkLimitCheck = TM.VerifySlitterSetpoints(TM.ParkLmtSp);
                if (ParkLimitCheck)
                {
                    TM.BandStpt = TM.ParkLmtSp;
                    SysMsgListBx.Items.Add("Solution ParkLmt Selected");
                }
                SelectedSolutionTxt.Text = "ParkLmt";
            }

            if (!ActPosnCheck && !ParkPosnCheck && !ParkLimitCheck)
            {
                SysMsgListBx.Items.Add("Solution not Possible, Try Swapping Rolls");

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
                        SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14225) Method SlitterPLCWrites");
                    }

                //Write Last Ten Slitter Calibrate Offsets and Slitter Setpoints- Starting Register - Number of Registers 
                ReadMessage = MB.WriteMultipleRegisters(14260, PLC.PLCWriteStptLast10Registers, 40);
                CmpResult = String.Equals(ReadMessage, ConnectedMessage);
                    if (CmpResult == false && CommunicatonPLCFailure == false)
                    {
                         MB.ModbusCloseConnection();
                         CommunicatonPLCFailure = true;
                         SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14261) Method SlitterPLCWrites");
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
                        SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14205) Method SlitterPLCWrites");
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14200) Method SlitterPLCWrites");

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
                    SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14200) Method ZeroOutPLCComands");
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14205) Method ZeroOutPLCComands");

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
                S1ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 1");
            }

            if (PLC.PLCControlBits[0, 12])
            {
                S2ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 2");
            }
           
            
            
            if (PLC.PLCControlBits[0, 13])
            {
                S3ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 3");
            } 
            

            if (PLC.PLCControlBits[0, 14])
            {
                S4ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 4");
            }
            

            if (PLC.PLCControlBits[1, 0])
            {
                S5ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 5");

            }
            
            if (PLC.PLCControlBits[1, 1])
            {
                S6ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 6");

            }
           
            if (PLC.PLCControlBits[1, 2])
            {
                S7ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 7");

            }
            
            if (PLC.PLCControlBits[1, 3])
            {
                S8ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 8");

            }
            
            if (PLC.PLCControlBits[1, 4])
            {
                S9ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 9");

            }
            
            if (PLC.PLCControlBits[1, 5])
            {
                S10ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 10");

            }
            
            if (PLC.PLCControlBits[1, 6])
            {
                S11ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 11");

            }
            
            if (PLC.PLCControlBits[1, 7])
            {
                S12ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 12");

            }
            
            if (PLC.PLCControlBits[1, 8])
            {
                S13ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 13");

            }
            
            if (PLC.PLCControlBits[1, 9])
            {
                S14ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 14");

            }
            
            if (PLC.PLCControlBits[1, 10])
            {
                S15ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 15");

            }
            
            if (PLC.PLCControlBits[1, 11])
            {
                S16ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 16");

            }
            
            if (PLC.PLCControlBits[1, 12])
            {
                S17ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 17");

            }
            
            if (PLC.PLCControlBits[1, 13])
            {
                S18ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 18");

            }
            
            if (PLC.PLCControlBits[1, 14])
            {
                S16ComboBox.SelectedIndex = 10;
                System.Windows.MessageBox.Show("Out of Service", "Slitter 19");

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

        private void S1ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            S1ComboBox.Foreground = Brushes.Green;
        }

        private void S2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S2ComboBox.SelectedIndex;

            // Enable and In Service
            if (ItemSel == 0 )
            {
                TM.SlitterDisable[1] = false;
                TM.SlitterOutofService[1] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S2ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 2");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            { 
                TM.SlitterDisable[1] = true;
                ClearDataforDisableSliter();
                S2ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 2");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[1] = true;
                TM.SlitterOutofService[1] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true; 
                S2ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 2");
            }
        }

        private void S3ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S3ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[2] = false;
                TM.SlitterOutofService[2] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S3ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 3");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[2] = true;
                ClearDataforDisableSliter();
                S3ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 3");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[2] = true;
                TM.SlitterOutofService[2] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S3ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 3");
            }

        }

        private void S4ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S4ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[3] = false;
                TM.SlitterOutofService[3] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S4ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 4");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[3] = true;
                ClearDataforDisableSliter();
                S4ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 4");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[3] = true;
                TM.SlitterOutofService[3] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S4ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 4");
            }
        }

        private void S5ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S5ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[4] = false;
                TM.SlitterOutofService[4] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S5ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 5");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[4] = true;
                ClearDataforDisableSliter();
                S5ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 5");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[4] = true;
                TM.SlitterOutofService[4] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S5ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 5");
            }
        }

        private void S6ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S6ComboBox.SelectedIndex;

            //Enalbe and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[5] = false;
                TM.SlitterOutofService[5] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S6ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 6");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[5] = true;
                ClearDataforDisableSliter();
                S6ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 6");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[5] = true;
                TM.SlitterOutofService[5] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S6ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 6");
            }
        }

        private void S7ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S7ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[6] = false;
                TM.SlitterOutofService[6] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S7ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 7");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[6] = true;
                ClearDataforDisableSliter();
                S7ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 7");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[6] = true;
                TM.SlitterOutofService[6] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S7ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 7");
            }
        }

        private void S8ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S8ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[7] = false;
                TM.SlitterOutofService[7] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S8ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 8");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[7] = true;
                ClearDataforDisableSliter();
                S8ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 8");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[7] = true;
                TM.SlitterOutofService[7] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S8ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 8");
            }
        }

        private void S9ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S9ComboBox.SelectedIndex;
            
            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[8] = false;
                TM.SlitterOutofService[8] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S9ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 9");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[8] = true;
                ClearDataforDisableSliter();
                S9ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 9");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[8] = true;
                TM.SlitterOutofService[8] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S9ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 9");
            }
        }

        private void S10ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S10ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[9] = false;
                TM.SlitterOutofService[9] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S10ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 10");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[9] = true;
                ClearDataforDisableSliter();
                S10ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 10");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[9] = true;
                TM.SlitterOutofService[9] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S10ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 10");
            }
        }

        private void S11ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S11ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[10] = false;
                TM.SlitterOutofService[10] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S11ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 11");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[10] = true;
                ClearDataforDisableSliter();
                S11ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 11");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[10] = true;
                TM.SlitterOutofService[10] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S11ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 11");
            }
        }

        private void S12ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S12ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[11] = false;
                TM.SlitterOutofService[11] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S12ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 12");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[11] = true;
                ClearDataforDisableSliter();
                S12ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 12");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[11] = true;
                TM.SlitterOutofService[11] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S12ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
               //System.Windows.MessageBox.Show("Out of Service", "Slitter 12");
            }
        }

        private void S13ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S13ComboBox.SelectedIndex;

            //Enalbe and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[12] = false;
                TM.SlitterOutofService[12] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S13ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 13");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[12] = true;
                ClearDataforDisableSliter();
                S13ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 13");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[12] = true;
                TM.SlitterOutofService[12] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S13ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 13");
            }
        }

        private void S14ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S14ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[13] = false;
                TM.SlitterOutofService[13] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S14ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 14");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[13] = true;
                ClearDataforDisableSliter();
                S14ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 14");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[13] = true;
                TM.SlitterOutofService[13] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S14ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 14");
            }
        }

        private void S15ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S15ComboBox.SelectedIndex;

            //Enalbe and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[14] = false;
                TM.SlitterOutofService[14] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S15ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 15");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[14] = true;
                ClearDataforDisableSliter();
                S15ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 15");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[14] = true;
                TM.SlitterOutofService[14] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S15ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 15");
            }
        }

        private void S16ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S16ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[15] = false;
                TM.SlitterOutofService[15] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S16ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 16");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[15] = true;
                ClearDataforDisableSliter();
                S16ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 16");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[15] = true;
                TM.SlitterOutofService[15] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S16ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 16");
            }
        }

        private void S17ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S17ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[16] = false;
                TM.SlitterOutofService[16] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S17ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 17");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[16] = true;
                ClearDataforDisableSliter();
                S17ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 17");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[16] = true;
                TM.SlitterOutofService[16] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S17ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 17");
            }
        }

        private void S18ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Int32 ItemSel = S18ComboBox.SelectedIndex;

            //Enable and In Service
            if (ItemSel == 0)
            {
                TM.SlitterDisable[17] = false;
                TM.SlitterOutofService[17] = false;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S18ComboBox.Foreground = Brushes.Green;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("In Service", "Slitter 18");
            }
            //Disable Slitter
            if (ItemSel >= 1 && ItemSel <= OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[17] = true;
                ClearDataforDisableSliter();
                S18ComboBox.Foreground = Brushes.DarkBlue;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Slitter Disabled", "Slitter 18");
            }
            //Out of Service
            if (ItemSel > OutOfServBrkPtComboBox)
            {
                TM.SlitterDisable[17] = true;
                TM.SlitterOutofService[17] = true;
                ClearDataforDisableSliter();
                TM.SlitOutOfServDetect = true;
                S18ComboBox.Foreground = Brushes.Magenta;
                AcceptBtn.Background = Brushes.Yellow;
                //System.Windows.MessageBox.Show("Out of Service", "Slitter 18");
            }
        }

        private void S19ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            S19ComboBox.Foreground = Brushes.Green;
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

        // If operator doesn't commit to tranfer than deselecting or selecting diag checkbox or accept button will call this method.
        private void OperatorNoTransferOffsetsToPLC()
        {
            Tmr1.Stop();
            DisableBandCalibMsg = true;

            BandCalibTxt1.Text = "0.0";
            BandCalibTxt2.Text = "0.0";
            BandCalibTxt3.Text = "0.0";
            BandCalibTxt4.Text = "0.0";
            BandCalibTxt5.Text = "0.0";
            BandCalibTxt6.Text = "0.0";
            BandCalibTxt7.Text = "0.0";
            BandCalibTxt8.Text = "0.0";
            BandCalibTxt9.Text = "0.0";
            BandCalibTxt10.Text = "0.0";
            BandCalibTxt11.Text = "0.0";
            BandCalibTxt12.Text = "0.0";
            BandCalibTxt13.Text = "0.0";
            BandCalibTxt14.Text = "0.0";
            BandCalibTxt15.Text = "0.0";
            BandCalibTxt16.Text = "0.0";
            BandCalibTxt17.Text = "0.0";
            BandCalibTxt18.Text = "0.0";
            BandCalibTxt19.Text = "0.0";
            for (int x = 0; x < TM.MaxSlitters; x++)
            {
                TM.CalibrateCmdSelected[x] = false;
                TM.SlitterCalibTextChgd[x] = false;
                TM.CalibrateOffsets[x] = 0.0;
            }
            CalibParamsLoaded = false;
            LoadParambtn.Background = Brushes.Transparent;
            TransferOffsetsToPLCBtn.Background = Brushes.Transparent;
            DisableBandCalibMsg = false;
            Tmr1.Start();

        }

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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14200) Method ResetBtn_Click");

            }
            OutOfTolerance = false;
            FM.MsgReset();
            FltMsgListBx.Items.Clear();
            SysMsgListBx.Items.Clear();
            Tmr1.Start();

            
        }

        private void AcceptBtn_Click(object sender, EventArgs e)
        {

            Boolean ActPosnCheck = false;
            Boolean ParkPosnCheck = false;
            Boolean ParkLimitCheck = false;

            OperatorNoTransferOffsetsToPLC();
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
                SysMsgListBx.Items.Add("Maximum Trim Exceeded");
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

            // Not Used SlitterCheck = TM.SlitCutsUsedToRollCuts();
            //Check Calibration Position mode for alogorithum
            if ((TM.NumbOfRolls + 1) == TM.NumbOfSlitParkSelected)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ParkPosnSp[x] = 0.0;
                    TM.ParkPosnSpPartial[x] = 0.0;
                }
                TM.ParkPosnSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectPark);
                TM.ParkPosnSp = TM.CalcBandStptsNotUsedParkPosn(TM.ParkPosnSpPartial, TM.BandParkSelected);
                ParkPosnCheck = TM.VerifySlitterSetpoints(TM.ParkPosnSp);
                if (ParkPosnCheck)
                {
                    TM.BandStpt = TM.ParkPosnSp;
                    SysMsgListBx.Items.Add("Solution ParkPosn Selected");
                }
                SelectedSolutionTxt.Text = "ParkPosn";
            }
            //Check Actual Position mode for alogorithum
            if ((TM.NumbOfRolls + 1) == TM.NumbOfSlitActPosSelected && !ParkPosnCheck)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ActPosnSp[x] = 0.0;
                    TM.ActPosnSpPartial[x] = 0.0;
                }
                TM.ActPosnSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectAct);
                TM.ActPosnSp = TM.CalcBandStptsNotUsedActPosn(TM.ActPosnSpPartial, TM.BandActPosnSelected);
                ActPosnCheck = TM.VerifySlitterSetpoints(TM.ActPosnSp);
                if (ActPosnCheck)
                {
                    TM.BandStpt = TM.ActPosnSp;
                    SysMsgListBx.Items.Add("Solution ActPosn Selected");
                }
                SelectedSolutionTxt.Text = "ActPosn";
            }
            //Check Park Limit Position mode for alogorithum
            if (((TM.NumbOfRolls + 1) == TM.NumbOfslitParkSelectdLmt) && !ParkPosnCheck && !ActPosnCheck)
            {
                for (int x = 0; x < TM.MaxSlitters; x++)
                {
                    TM.ParkLmtSp[x] = 0.0;
                    TM.ParkLmtSpPartial[x] = 0.0;
                }
                TM.ParkLmtSpPartial = TM.CalcSelectedBandStpts(TM.SolutionSelectParkLmt);
                TM.ParkLmtSp = TM.CalcBandStptsNotUsedParkLmt(TM.ParkLmtSpPartial, TM.BandParkLimitSelected);
                ParkLimitCheck = TM.VerifySlitterSetpoints(TM.ParkLmtSp);
                if (ParkLimitCheck)
                {
                    TM.BandStpt = TM.ParkLmtSp;
                    SysMsgListBx.Items.Add("Solution ParkLmt Selected");
                }
                SelectedSolutionTxt.Text = "ParkLmt";
            }

            if (!ActPosnCheck && !ParkPosnCheck && !ParkLimitCheck)
            {
                SysMsgListBx.Items.Add("Solution not Possible, Try Swapping Rolls");

            }


            if (TM.BandStpt[18] < TM.BandLowerLimit[18])
            {
                System.Windows.MessageBox.Show("Invalid Roll Data");
            }

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
            RollCheckFlt = true;
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
            ShrinkIncDecBtn.Value = ZeroOut;
            ShrinkIncDecBtn.Background = Brushes.LightCoral;
            AcceptBtn.Background = Brushes.Orange;
            for (int x = 0; x < 19; x++)
            {
                TM.SlitterCalibTextChgd[x] = false;
                TM.CalibrateOffsets[x] = ZeroOut;
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
                SysMsgListBx.Items.Add("Rx3i 10.10.10.110 Comm Failure (R14199) Method SlitterPLCWrites");
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

        private void WrapOrderbtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(OrderInfo);
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
            WrapOrderbtn.Visibility = Visibility.Hidden;
            SolutionLbl.Visibility = Visibility.Hidden;
            SelectedSolutionTxt.Visibility = Visibility.Hidden;
            ShrinkTxtLbl.Visibility = Visibility.Hidden;
            CleanOnBtn.Visibility = Visibility.Hidden;
            CLeanOffBtn.Visibility = Visibility.Hidden;
            LoadParambtn.Visibility = Visibility.Hidden;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Hidden;
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

        #region Slitter Calibration Text Changed
        private void SlitCalibTextChangedColor()
        {
            if(TM.SlitterCalibTextChgd[0])
            {
                S1ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S1ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[1])
            {
                S2ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S2ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[2])
            {
                S3ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S3ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[3])
            {
                S4ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S4ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[4])
            {
                S5ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S5ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[5])
            {
                S6ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S6ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[6])
            {
                S7ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S7ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[7])
            {
                S8ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S8ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[8])
            {
                S9ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S9ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[9])
            {
                S10ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S10ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[10])
            {
                S11ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S11ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[11])
            {
                S12ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S12ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[12])
            {
                S13ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S13ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[13])
            {
                S14ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S14ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[14])
            {
                S15ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S15ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[15])
            {
                S16ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S16ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[16])
            {
                S17ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S17ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[17])
            {
                S18ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S18ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
            if (TM.SlitterCalibTextChgd[18])
            {
                S19ManSelectBtn.Background = Brushes.Orange;
            }
            else
            {
                S19ManSelectBtn.Background = Brushes.PaleGoldenrod;
            }
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
            SysMsgListBx.Items.Add("Diagnostics On");
            DiagOn = true;
            WrapOrderbtn.Visibility = Visibility.Visible;
            SolutionLbl.Visibility = Visibility.Visible;
            SelectedSolutionTxt.Visibility = Visibility.Visible;
            ShrinkTxtLbl.Visibility = Visibility.Visible;
            CleanOnBtn.Visibility = Visibility.Visible;
            CLeanOffBtn.Visibility = Visibility.Visible;
            LoadParambtn.Visibility = Visibility.Visible;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Visible;
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
            OperatorNoTransferOffsetsToPLC();
               

        }

        private void DiagChkBx_Unchecked(object sender, RoutedEventArgs e)
        {
            SysMsgListBx.Items.Add("Diagnostics Off");
            DiagOn = false;
            WrapOrderbtn.Visibility = Visibility.Hidden;
            SolutionLbl.Visibility = Visibility.Hidden;
            SelectedSolutionTxt.Visibility = Visibility.Hidden;
            LoadParambtn.Visibility = Visibility.Hidden;
            TransferOffsetsToPLCBtn.Visibility = Visibility.Hidden;
            ShrinkTxtLbl.Visibility = Visibility.Hidden;
            CleanOnBtn.Visibility = Visibility.Hidden;
            CLeanOffBtn.Visibility = Visibility.Hidden;
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

            //Clear out Slitter Calib Text Changed in case value is entered but view of calibration is invisible
            OperatorNoTransferOffsetsToPLC();
                    
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

        #region MouseEnter MouseLeave ComboBox

        private void S1ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S1ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S1ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S1ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S2ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S2ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S2ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S2ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S3ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S3ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S3ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S3ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S4ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S4ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S4ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S4ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S5ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S5ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S5ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S5ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S6ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S6ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S6ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S6ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S7ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S7ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S7ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S7ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S8ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S8ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S8ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S8ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S9ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S9ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S9ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S9ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S10ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S10ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S10ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S10ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S11ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S11ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S11ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S11ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S12ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S12ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S12ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S12ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S13ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S13ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S13ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S13ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S14ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S14ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S14ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S14ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S15ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S15ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S15ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S15ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S16ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S16ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S16ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S16ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S17ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S17ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S17ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S17ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S18ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S18ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S18ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S18ComboBox.Width = ComboboxWidthMouseLeave;
        }

        private void S19ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            S19ComboBox.Width = ComboboxWidthMouseEnter;
        }

        private void S19ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            S19ComboBox.Width = ComboboxWidthMouseLeave;
        }
        #endregion

        
    }
}
