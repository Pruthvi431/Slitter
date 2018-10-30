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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SlittersWPF
{
    /// <summary>
    /// Interaction logic for SLifeCycle.xaml
    /// </summary>
    public partial class SLifeCycle : Window
    {
        RollParam TM = new RollParam();
        double[] Life = new double[38];
        DispatcherTimer Tmr3 = new DispatcherTimer();
        bool LifeReset = false;
        
        public SLifeCycle()
        {
            InitializeComponent();
            UpdateTextBackground();
            UpdateLimtTextBoxes();
            Tmr3.Interval = new TimeSpan(0, 0, 0, 0, 500); //days , hours, minutes, seconds, milliseconds
            Tmr3.Tick += new EventHandler(TimerEventProcessor3);
            Tmr3.Start();
        }

        private void TimerEventProcessor3(Object myObject,
                                        EventArgs myEventArgs)
        {
            UpdateLimtTextBoxes();
            ReadLifeCycle();
            
            if ((RollParam.SlitterLifeCycleResets[0] > 0 || RollParam.SlitterLifeCycleResets[1] > 0 || RollParam.SlitterLifeCycleResets[2] > 0)  && LifeReset == false)
            {
                TestText.Background = Brushes.Red;
                RollParam.LifeCycleReset = true;
                LifeReset = true;
                
            }
            if (RollParam.LifeCycleReset == false)
            {

                TestText.Background = Brushes.Transparent;
                LifeReset = false;
            }
        }

        #region Read Life Cycle values and place in text boxes.
        private void ReadLifeCycle()
        {
            Tmr3.Stop();
                     
            Life[0] = Convert.ToDouble(RollParam.SlitterLifeCycle[0] * 5.0 / 60.0);
            S1BandLife.Text = Life[0].ToString("F1");
            Life[1] = Convert.ToDouble(RollParam.SlitterLifeCycle[1] * 5.0 / 60.0);
            S2BandLife.Text = Life[1].ToString("F1");
            Life[2] = Convert.ToDouble(RollParam.SlitterLifeCycle[2] * 5.0 / 60.0);
            S3BandLife.Text = Life[2].ToString("F1");
            Life[3] = Convert.ToDouble(RollParam.SlitterLifeCycle[3] * 5.0 / 60.0);
            S4BandLife.Text = Life[3].ToString("F1");
            Life[4] = Convert.ToDouble(RollParam.SlitterLifeCycle[4] * 5.0 / 60.0);
            S5BandLife.Text = Life[4].ToString("F1");
            Life[5] = Convert.ToDouble(RollParam.SlitterLifeCycle[5] * 5.0 / 60.0);
            S6BandLife.Text = Life[5].ToString("F1");
            Life[6] = Convert.ToDouble(RollParam.SlitterLifeCycle[6] * 5.0 / 60.0);
            S7BandLife.Text = Life[6].ToString("F1");
            Life[7] = Convert.ToDouble(RollParam.SlitterLifeCycle[7] * 5.0 / 60.0);
            S8BandLife.Text = Life[7].ToString("F1");
            Life[8] = Convert.ToDouble(RollParam.SlitterLifeCycle[8] * 5.0 / 60.0);
            S9BandLife.Text = Life[8].ToString("F1");
            Life[9] = Convert.ToDouble(RollParam.SlitterLifeCycle[9] * 5.0 / 60.0);
            S10BandLife.Text = Life[9].ToString("F1");
            Life[10] = Convert.ToDouble(RollParam.SlitterLifeCycle[10] * 5.0 / 60.0);
            S11BandLife.Text = Life[10].ToString("F1");
            Life[11] = Convert.ToDouble(RollParam.SlitterLifeCycle[11] * 5.0 / 60.0);
            S12BandLife.Text = Life[11].ToString("F1");
            Life[12] = Convert.ToDouble(RollParam.SlitterLifeCycle[12] * 5.0 / 60.0);
            S13BandLife.Text = Life[12].ToString("F1");
            Life[13] = Convert.ToDouble(RollParam.SlitterLifeCycle[13] * 5.0 / 60.0);
            S14BandLife.Text = Life[13].ToString("F1");
            Life[14] = Convert.ToDouble(RollParam.SlitterLifeCycle[14] * 5.0 / 60.0);
            S15BandLife.Text = Life[14].ToString("F1");
            Life[15] = Convert.ToDouble(RollParam.SlitterLifeCycle[15] * 5.0 / 60.0);
            S16BandLife.Text = Life[15].ToString("F1");
            Life[16] = Convert.ToDouble(RollParam.SlitterLifeCycle[16] * 5.0 / 60.0);
            S17BandLife.Text = Life[16].ToString("F1");
            Life[17] = Convert.ToDouble(RollParam.SlitterLifeCycle[17] * 5.0 / 60.0);
            S18BandLife.Text = Life[17].ToString("F1");
            Life[18] = Convert.ToDouble(RollParam.SlitterLifeCycle[18] * 5.0 / 60.0);
            S19BandLife.Text = Life[18].ToString("F1");
            Life[19] = Convert.ToDouble(RollParam.SlitterLifeCycle[19] * 5.0 / 60.0);
            S1BladeLife.Text = Life[19].ToString("F1");
            Life[20] = Convert.ToDouble(RollParam.SlitterLifeCycle[20] * 5.0 / 60.0);
            S2BladeLife.Text = Life[20].ToString("F1");
            Life[21] = Convert.ToDouble(RollParam.SlitterLifeCycle[21] * 5.0 / 60.0);
            S3BladeLife.Text = Life[21].ToString("F1");
            Life[22] = Convert.ToDouble(RollParam.SlitterLifeCycle[22] * 5.0 / 60.0);
            S4BladeLife.Text = Life[22].ToString("F1");
            Life[23] = Convert.ToDouble(RollParam.SlitterLifeCycle[23] * 5.0 / 60.0);
            S5BladeLife.Text = Life[23].ToString("F1");
            Life[24] = Convert.ToDouble(RollParam.SlitterLifeCycle[24] * 5.0 / 60.0);
            S6BladeLife.Text = Life[24].ToString("F1");
            Life[25] = Convert.ToDouble(RollParam.SlitterLifeCycle[25] * 5.0 / 60.0);
            S7BladeLife.Text = Life[25].ToString("F1");
            Life[26] = Convert.ToDouble(RollParam.SlitterLifeCycle[26] * 5.0 / 60.0);
            S8BladeLife.Text = Life[26].ToString("F1");
            Life[27] = Convert.ToDouble(RollParam.SlitterLifeCycle[27] * 5.0 / 60.0);
            S9BladeLife.Text = Life[27].ToString("F1");
            Life[28] = Convert.ToDouble(RollParam.SlitterLifeCycle[28] * 5.0 / 60.0);
            S10BladeLife.Text = Life[28].ToString("F1");
            Life[29] = Convert.ToDouble(RollParam.SlitterLifeCycle[29] * 5.0 / 60.0);
            S11BladeLife.Text = Life[29].ToString("F1");
            Life[30] = Convert.ToDouble(RollParam.SlitterLifeCycle[30] * 5.0 / 60.0);
            S12BladeLife.Text = Life[30].ToString("F1");
            Life[31] = Convert.ToDouble(RollParam.SlitterLifeCycle[31] * 5.0 / 60.0);
            S13BladeLife.Text = Life[31].ToString("F1");
            Life[32] = Convert.ToDouble(RollParam.SlitterLifeCycle[32] * 5.0 / 60.0);
            S14BladeLife.Text = Life[32].ToString("F1");
            Life[33] = Convert.ToDouble(RollParam.SlitterLifeCycle[33] * 5.0 / 60.0);
            S15BladeLife.Text = Life[33].ToString("F1");
            Life[34] = Convert.ToDouble(RollParam.SlitterLifeCycle[34] * 5.0 / 60.0);
            S16BladeLife.Text = Life[34].ToString("F1");
            Life[35] = Convert.ToDouble(RollParam.SlitterLifeCycle[35] * 5.0 / 60.0);
            S17BladeLife.Text = Life[35].ToString("F1");
            Life[36] = Convert.ToDouble(RollParam.SlitterLifeCycle[36] * 5.0 / 60.0);
            S18BladeLife.Text = Life[36].ToString("F1");
            Life[37] = Convert.ToDouble(RollParam.SlitterLifeCycle[37] * 5.0 / 60.0);
            S19BladeLife.Text = Life[37].ToString("F1");
            Tmr3.Start();

        }

        #endregion

        #region Update Text Background
        private void UpdateTextBackground()
        {
            S1BandLife.Background = Brushes.Transparent ;
            S2BandLife.Background = Brushes.Transparent ;
            S3BandLife.Background = Brushes.Transparent ;
            S4BandLife.Background = Brushes.Transparent ;
            S5BandLife.Background = Brushes.Transparent ;
            S6BandLife.Background = Brushes.Transparent ;
            S7BandLife.Background = Brushes.Transparent ;
            S8BandLife.Background = Brushes.Transparent ;
            S9BandLife.Background = Brushes.Transparent ;
            S10BandLife.Background = Brushes.Transparent;
            S11BandLife.Background = Brushes.Transparent ;
            S12BandLife.Background = Brushes.Transparent ;
            S13BandLife.Background = Brushes.Transparent ;
            S14BandLife.Background = Brushes.Transparent ;
            S15BandLife.Background = Brushes.Transparent ;
            S16BandLife.Background = Brushes.Transparent ;
            S17BandLife.Background = Brushes.Transparent ;
            S18BandLife.Background = Brushes.Transparent ;
            S19BandLife.Background = Brushes.Transparent ;
            S1BladeLife.Background = Brushes.Transparent ;
            S2BladeLife.Background = Brushes.Transparent ;
            S3BladeLife.Background = Brushes.Transparent ;
            S4BladeLife.Background = Brushes.Transparent ;
            S5BladeLife.Background = Brushes.Transparent ;
            S6BladeLife.Background = Brushes.Transparent ;
            S7BladeLife.Background = Brushes.Transparent ;
            S8BladeLife.Background = Brushes.Transparent ;
            S9BladeLife.Background = Brushes.Transparent ;
            S10BladeLife.Background = Brushes.Transparent;
            S11BladeLife.Background = Brushes.Transparent;
            S12BladeLife.Background = Brushes.Transparent;
            S13BladeLife.Background = Brushes.Transparent;
            S14BladeLife.Background = Brushes.Transparent;
            S15BladeLife.Background = Brushes.Transparent;
            S16BladeLife.Background = Brushes.Transparent;
            S17BladeLife.Background = Brushes.Transparent;
            S18BladeLife.Background = Brushes.Transparent;
            S19BladeLife.Background = Brushes.Transparent;
            S1BandUpperLimit.Background = Brushes.Transparent;
            S2BandUpperLimit.Background = Brushes.Transparent;
            S3BandUpperLimit.Background = Brushes.Transparent;
            S4BandUpperLimit.Background = Brushes.Transparent;
            S5BandUpperLimit.Background = Brushes.Transparent;
            S6BandUpperLimit.Background = Brushes.Transparent;
            S7BandUpperLimit.Background = Brushes.Transparent;
            S8BandUpperLimit.Background = Brushes.Transparent;
            S9BandUpperLimit.Background = Brushes.Transparent;
            S10BandUpperLimit.Background = Brushes.Transparent;
            S11BandUpperLimit.Background = Brushes.Transparent;
            S12BandUpperLimit.Background = Brushes.Transparent;
            S13BandUpperLimit.Background = Brushes.Transparent;
            S14BandUpperLimit.Background = Brushes.Transparent;
            S15BandUpperLimit.Background = Brushes.Transparent;
            S16BandUpperLimit.Background = Brushes.Transparent;
            S17BandUpperLimit.Background = Brushes.Transparent;
            S18BandUpperLimit.Background = Brushes.Transparent;
            S19BandUpperLimit.Background = Brushes.Transparent;
            S1BandLowerLimit.Background = Brushes.Transparent;
            S2BandLowerLimit.Background = Brushes.Transparent;
            S3BandLowerLimit.Background = Brushes.Transparent;
            S4BandLowerLimit.Background = Brushes.Transparent;
            S5BandLowerLimit.Background = Brushes.Transparent;
            S6BandLowerLimit.Background = Brushes.Transparent;
            S7BandLowerLimit.Background = Brushes.Transparent;
            S8BandLowerLimit.Background = Brushes.Transparent;
            S9BandLowerLimit.Background = Brushes.Transparent;
            S10BandLowerLimit.Background = Brushes.Transparent;
            S11BandLowerLimit.Background = Brushes.Transparent;
            S12BandLowerLimit.Background = Brushes.Transparent;
            S13BandLowerLimit.Background = Brushes.Transparent;
            S14BandLowerLimit.Background = Brushes.Transparent;
            S15BandLowerLimit.Background = Brushes.Transparent;
            S16BandLowerLimit.Background = Brushes.Transparent;
            S17BandLowerLimit.Background = Brushes.Transparent;
            S18BandLowerLimit.Background = Brushes.Transparent;
            S19BandLowerLimit.Background = Brushes.Transparent;
            S1BladeUpperLimit.Background = Brushes.Transparent;
            S2BladeUpperLimit.Background = Brushes.Transparent;
            S3BladeUpperLimit.Background = Brushes.Transparent;
            S4BladeUpperLimit.Background = Brushes.Transparent;
            S5BladeUpperLimit.Background = Brushes.Transparent;
            S6BladeUpperLimit.Background = Brushes.Transparent;
            S7BladeUpperLimit.Background = Brushes.Transparent;
            S8BladeUpperLimit.Background = Brushes.Transparent;
            S9BladeUpperLimit.Background = Brushes.Transparent;
            S10BladeUpperLimit.Background = Brushes.Transparent;
            S11BladeUpperLimit.Background = Brushes.Transparent;
            S12BladeUpperLimit.Background = Brushes.Transparent;
            S13BladeUpperLimit.Background = Brushes.Transparent;
            S14BladeUpperLimit.Background = Brushes.Transparent;
            S15BladeUpperLimit.Background = Brushes.Transparent;
            S16BladeUpperLimit.Background = Brushes.Transparent;
            S17BladeUpperLimit.Background = Brushes.Transparent;
            S18BladeUpperLimit.Background = Brushes.Transparent;
            S19BladeUpperLimit.Background = Brushes.Transparent;
            S1BladeLowerLimit.Background = Brushes.Transparent;
            S2BladeLowerLimit.Background = Brushes.Transparent;
            S3BladeLowerLimit.Background = Brushes.Transparent;
            S4BladeLowerLimit.Background = Brushes.Transparent;
            S5BladeLowerLimit.Background = Brushes.Transparent;
            S6BladeLowerLimit.Background = Brushes.Transparent;
            S7BladeLowerLimit.Background = Brushes.Transparent;
            S8BladeLowerLimit.Background = Brushes.Transparent;
            S9BladeLowerLimit.Background = Brushes.Transparent;
            S10BladeLowerLimit.Background = Brushes.Transparent;
            S11BladeLowerLimit.Background = Brushes.Transparent;
            S12BladeLowerLimit.Background = Brushes.Transparent;
            S13BladeLowerLimit.Background = Brushes.Transparent;
            S14BladeLowerLimit.Background = Brushes.Transparent;
            S15BladeLowerLimit.Background = Brushes.Transparent;
            S16BladeLowerLimit.Background = Brushes.Transparent;
            S17BladeLowerLimit.Background = Brushes.Transparent;
            S18BladeLowerLimit.Background = Brushes.Transparent;
            S19BladeLowerLimit.Background = Brushes.Transparent;
        }
        #endregion

        #region Update Limit Text Boxes

        private void UpdateLimtTextBoxes()
        {
            S1BandUpperLimit.Text = TM.BandUpperLimit[0].ToString("F2");
            S2BandUpperLimit.Text = TM.BandUpperLimit[1].ToString("F2");
            S3BandUpperLimit.Text = TM.BandUpperLimit[2].ToString("F2");
            S4BandUpperLimit.Text = TM.BandUpperLimit[3].ToString("F2");
            S5BandUpperLimit.Text = TM.BandUpperLimit[4].ToString("F2");
            S6BandUpperLimit.Text = TM.BandUpperLimit[5].ToString("F2");
            S7BandUpperLimit.Text = TM.BandUpperLimit[6].ToString("F2");
            S8BandUpperLimit.Text = TM.BandUpperLimit[7].ToString("F2");
            S9BandUpperLimit.Text = TM.BandUpperLimit[8].ToString("F2");
            S10BandUpperLimit.Text = TM.BandUpperLimit[9].ToString("F2");
            S11BandUpperLimit.Text = TM.BandUpperLimit[10].ToString("F2");
            S12BandUpperLimit.Text = TM.BandUpperLimit[11].ToString("F2");
            S13BandUpperLimit.Text = TM.BandUpperLimit[12].ToString("F2");
            S14BandUpperLimit.Text = TM.BandUpperLimit[13].ToString("F2");
            S15BandUpperLimit.Text = TM.BandUpperLimit[14].ToString("F2");
            S16BandUpperLimit.Text = TM.BandUpperLimit[15].ToString("F2");
            S17BandUpperLimit.Text = TM.BandUpperLimit[16].ToString("F2");
            S18BandUpperLimit.Text = TM.BandUpperLimit[17].ToString("F2");
            S19BandUpperLimit.Text = TM.BandUpperLimit[18].ToString("F2");
            S1BandLowerLimit.Text = TM.BandLowerLimit[0].ToString("F2");
            S2BandLowerLimit.Text = TM.BandLowerLimit[1].ToString("F2");
            S3BandLowerLimit.Text = TM.BandLowerLimit[2].ToString("F2");
            S4BandLowerLimit.Text = TM.BandLowerLimit[3].ToString("F2");
            S5BandLowerLimit.Text = TM.BandLowerLimit[4].ToString("F2");
            S6BandLowerLimit.Text = TM.BandLowerLimit[5].ToString("F2");
            S7BandLowerLimit.Text = TM.BandLowerLimit[6].ToString("F2");
            S8BandLowerLimit.Text = TM.BandLowerLimit[7].ToString("F2");
            S9BandLowerLimit.Text = TM.BandLowerLimit[8].ToString("F2");
            S10BandLowerLimit.Text = TM.BandLowerLimit[9].ToString("F2");
            S11BandLowerLimit.Text = TM.BandLowerLimit[10].ToString("F2");
            S12BandLowerLimit.Text = TM.BandLowerLimit[11].ToString("F2");
            S13BandLowerLimit.Text = TM.BandLowerLimit[12].ToString("F2");
            S14BandLowerLimit.Text = TM.BandLowerLimit[13].ToString("F2");
            S15BandLowerLimit.Text = TM.BandLowerLimit[14].ToString("F2");
            S16BandLowerLimit.Text = TM.BandLowerLimit[15].ToString("F2");
            S17BandLowerLimit.Text = TM.BandLowerLimit[16].ToString("F2");
            S18BandLowerLimit.Text = TM.BandLowerLimit[17].ToString("F2");
            S19BandLowerLimit.Text = TM.BandLowerLimit[18].ToString("F2");
            S1BladeUpperLimit.Text = TM.BladeUpperLimit[0].ToString("F2");
            S2BladeUpperLimit.Text = TM.BladeUpperLimit[1].ToString("F2");
            S3BladeUpperLimit.Text = TM.BladeUpperLimit[2].ToString("F2");
            S4BladeUpperLimit.Text = TM.BladeUpperLimit[3].ToString("F2");
            S5BladeUpperLimit.Text = TM.BladeUpperLimit[4].ToString("F2");
            S6BladeUpperLimit.Text = TM.BladeUpperLimit[5].ToString("F2");
            S7BladeUpperLimit.Text = TM.BladeUpperLimit[6].ToString("F2");
            S8BladeUpperLimit.Text = TM.BladeUpperLimit[7].ToString("F2");
            S9BladeUpperLimit.Text = TM.BladeUpperLimit[8].ToString("F2");
            S10BladeUpperLimit.Text = TM.BladeUpperLimit[9].ToString("F2");
            S11BladeUpperLimit.Text = TM.BladeUpperLimit[10].ToString("F2");
            S12BladeUpperLimit.Text = TM.BladeUpperLimit[11].ToString("F2");
            S13BladeUpperLimit.Text = TM.BladeUpperLimit[12].ToString("F2");
            S14BladeUpperLimit.Text = TM.BladeUpperLimit[13].ToString("F2");
            S15BladeUpperLimit.Text = TM.BladeUpperLimit[14].ToString("F2");
            S16BladeUpperLimit.Text = TM.BladeUpperLimit[15].ToString("F2");
            S17BladeUpperLimit.Text = TM.BladeUpperLimit[16].ToString("F2");
            S18BladeUpperLimit.Text = TM.BladeUpperLimit[17].ToString("F2");
            S19BladeUpperLimit.Text = TM.BladeUpperLimit[18].ToString("F2");
            S1BladeLowerLimit.Text = TM.BladeLowerLimit[0].ToString("F2");
            S2BladeLowerLimit.Text = TM.BladeLowerLimit[1].ToString("F2");
            S3BladeLowerLimit.Text = TM.BladeLowerLimit[2].ToString("F2");
            S4BladeLowerLimit.Text = TM.BladeLowerLimit[3].ToString("F2");
            S5BladeLowerLimit.Text = TM.BladeLowerLimit[4].ToString("F2");
            S6BladeLowerLimit.Text = TM.BladeLowerLimit[5].ToString("F2");
            S7BladeLowerLimit.Text = TM.BladeLowerLimit[6].ToString("F2");
            S8BladeLowerLimit.Text = TM.BladeLowerLimit[7].ToString("F2");
            S9BladeLowerLimit.Text = TM.BladeLowerLimit[8].ToString("F2");
            S10BladeLowerLimit.Text = TM.BladeLowerLimit[9].ToString("F2");
            S11BladeLowerLimit.Text = TM.BladeLowerLimit[10].ToString("F2");
            S12BladeLowerLimit.Text = TM.BladeLowerLimit[11].ToString("F2");
            S13BladeLowerLimit.Text = TM.BladeLowerLimit[12].ToString("F2");
            S14BladeLowerLimit.Text = TM.BladeLowerLimit[13].ToString("F2");
            S15BladeLowerLimit.Text = TM.BladeLowerLimit[14].ToString("F2");
            S16BladeLowerLimit.Text = TM.BladeLowerLimit[15].ToString("F2");
            S17BladeLowerLimit.Text = TM.BladeLowerLimit[16].ToString("F2");
            S18BladeLowerLimit.Text = TM.BladeLowerLimit[17].ToString("F2");
            S19BladeLowerLimit.Text = TM.BladeLowerLimit[18].ToString("F2");

        }

        #endregion

        #region Zero Out Checked Boxes
        private void S1BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 1;
            
        }

        private void S2BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 2;
        }

        private void S3BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 4;
        }

        private void S4BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 8;
        }

        private void S5BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 16;
        }

        private void S6BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 32;
        }

        private void S7BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 64;
        }

        private void S8BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 128;
        }
        private void S9BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 256;
        }

        private void S10BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 512;
        }

        private void S11BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 1024;
        }

        private void S12BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 2048;
            
        }

        private void S13BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 4096;
            
        }

        private void S14BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 8192;
            
        }

        private void S15BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[0] = 16384;
            
        }

        private void S16BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 1;
        }

        private void S17BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 2;
        }

        private void S18BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 4;
        }

        private void S19BandZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 8;
        }

        private void S1BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 16;
        }

        private void S2BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 32;
        }

        private void S3BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 64;
        }

        private void S4BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 128;
        }

        private void S5BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 256;
        }

        private void S6BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 512;
        }

        private void S7BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 1024;
        }

        private void S8BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 2048;
        }

        private void S9BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 4096;
        }

        private void S10BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 8192;
        }

        private void S11BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[1] = 16384;
        }

        private void S12BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 1;
        }

        private void S13BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 2;
        }

        private void S14BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 4;
        }

        private void S15BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 8;
        }

        private void S16BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 16;
        }

        private void S17BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 32;
        }

        private void S18BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 64;
        }

        private void S19BladeZeroOut_Checked(object sender, RoutedEventArgs e)
        {
            RollParam.SlitterLifeCycleResets[2] = 128;
        }

        #endregion

      
    }
}
