using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SlittersWPF
{
    /*Class FltMessage has 9 registers, 6 bits each, giving a total of 144 messages.  Messages are read every scan.  If a message is detected,
    it is masked out to prevent message from continously reading in.  The mask is reset to one's after the Message reset is applied.
    Spare messages can be applied. Apply messages to RetrieveFaultMessage() and have bit programmed in plc corresponding to the bit to activate the alarm
    */
    class FltMessage
    {
        public int MaxNumMessages = 144;
        public String[] FaultMsg = new String[144];
        //Constructor
        public FltMessage()
        {
        }

        //DeConstructor
        ~FltMessage()
        {
        }

        public bool[,] FltMsgMask = new bool[9,16];
        //Function resets the 9 registers with 16 bits each to true.
        public void MsgReset()
        {
            
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        FltMsgMask[x, y] = true;
                    }
                }
            }
        }

        //Receives an multi-dimensional array from plc and checks for any alarms.  If alarm exists, it is masked out and returns a boolean indicating 
        // fault was received
        public bool[] MsgChk(bool[,] MsgArr)
        {
            bool[] FltMsgAct = new bool[MaxNumMessages];
                        
            int k = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (MsgArr[i,j] & FltMsgMask[i,j])
                    {
                        FltMsgAct[k] = true;
                        FltMsgMask[i, j] = false;
                    }
                    k++;
                }
            }
                       
            return FltMsgAct;
        }
        public bool[,] FltMessBitConverter(UInt16[] FltRegisters)
        {
            // Bitval holds result from bit convertor and is passed back as a multi-dimensional array to form1
            bool[,] Bitval = new bool[9,16];
            //constants for bit calculation

           
            UInt16[] BitArr = { 32768, 16384, 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1 };
            //FltRegisters[0];  //R14150
            //FltRegisters[1];  //R14151
            //FltRegisters[3];  //R14153
            //FltRegisters[4];  //R14154
            //FltRegisters[5];  //R14155
            //FltRegisters[6];  //R14156
            //FltRegisters[7];  //R14157
            //FltRegisters[8];  //R14158
           
            for (int x = 0; x < 9; x++)
            {
                Bitval[x,15] = ((FltRegisters[x] / BitArr[0]) > 0) ? true : false;
                if (Bitval[x,15]) { FltRegisters[x] = (UInt16)( FltRegisters[x] - BitArr[0]); }

                Bitval[x,14] = ((FltRegisters[x] / BitArr[1]) > 0) ? true : false;
                if (Bitval[x,14]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[1]); }

                Bitval[x,13] = ((FltRegisters[x] / BitArr[2]) > 0) ? true : false;
                if (Bitval[x,13]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[2]); }

                Bitval[x,12] = ((FltRegisters[x] / BitArr[3]) > 0) ? true : false;
                if (Bitval[x,12]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[3]); }

                Bitval[x,11] = ((FltRegisters[x] / BitArr[4]) > 0) ? true : false;
                if (Bitval[x,11]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[4]); }

                Bitval[x,10] = ((FltRegisters[x] / BitArr[5]) > 0) ? true : false;
                if (Bitval[x,10]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[5]); }

                Bitval[x,9] = ((FltRegisters[x] / BitArr[6]) > 0) ? true : false;
                if (Bitval[x,9]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[6]); }

                Bitval[x,8] = ((FltRegisters[x] / BitArr[7]) > 0) ? true : false;
                if (Bitval[x,8]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[7]); }

                Bitval[x,7] = ((FltRegisters[x] / BitArr[8]) > 0) ? true : false;
                if (Bitval[x,7]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[8]); }

                Bitval[x,6] = ((FltRegisters[x] / BitArr[9]) > 0) ? true : false;
                if (Bitval[x,6]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[9]); }

                Bitval[x,5] = ((FltRegisters[x] / BitArr[10]) > 0) ? true : false;
                if (Bitval[x,5]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[10]); }

                Bitval[x,4] = ((FltRegisters[x] / BitArr[11]) > 0) ? true : false;
                if (Bitval[x,4]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[11]); }

                Bitval[x,3] = ((FltRegisters[x] / BitArr[12]) > 0) ? true : false;
                if (Bitval[x,3]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[12]); }

                Bitval[x,2] = ((FltRegisters[x] / BitArr[13]) > 0) ? true : false;
                if (Bitval[x,2]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[13]); }

                Bitval[x,1] = ((FltRegisters[x] / BitArr[14]) > 0) ? true : false;
                if (Bitval[x,1]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[14]); }

                Bitval[x,0] = ((FltRegisters[x] / BitArr[15]) > 0) ? true : false;
                if (Bitval[x,0]) { FltRegisters[x] = (UInt16)(FltRegisters[x] - BitArr[15]); }
            }
            return Bitval;
        }

        //Declarations

        

        public String[] RetrieveFaultMessage()
        {
            FaultMsg[0] = "FLT3000: Spare Fault Message R14150 Bit 1";              //R14150 Bit 1
            FaultMsg[1] = "FLT3001: Band Slitters Not Coarse Position Possible";    //R14150 Bit 2
            FaultMsg[2] = "FLT3002: Band slitters Not Fine Position Possible";      //R14150 Bit 3
            FaultMsg[3] = "FLT3003: Safety PEC's Not Clear While Positioning";      //R14150 Bit 4
            FaultMsg[4] = "FLT3004: Auto Position fault E-Stop Present";            //R14150 Bit 5
            FaultMsg[5] = "FLT3005: Auto Position Fault Can Not Position while Drive Running";  //R14150 Bit 6
            FaultMsg[6] = "FLT3006: Auto Position Fault Selector Switch is in Manual";  //R14150 Bit 7
            FaultMsg[7] = "FLT3007: Blade # 1 Position Not Logical";                //R14150 Bit 8
            FaultMsg[8] = "FLT3008: Blade #2 Position Not Logical";                 //R14150 Bit 9
            FaultMsg[9] = "FLT3009: Blade #3 Position Not Logical";                //R14150 Bit 10
            FaultMsg[10] = "FLT3010: Blade #4 Position Not Logical";                //R14150 Bit 11
            FaultMsg[11] = "FLT3011: Blade #5 Position Not Logical";                //R14150 Bit 12
            FaultMsg[12] = "FLT3012: Blade #6 Position Not Logical";                //R14150 Bit 13 
            FaultMsg[13] = "FLT3013: Blade #7 Position Not Logical";                //R14150 Bit 14
            FaultMsg[14] = "FLT3014: Blade #8 Position Not Logical";                //R14150 Bit 15
            FaultMsg[15] = "FLT3015: Blade #9 Position Not Logical";                //R14150 Bit 16
            FaultMsg[16] = "FLT3016: Blade #10 Position Not Logical";                //R14151 Bit 1
            FaultMsg[17] = "FLT3017: Blade #11 Position Not Logical";               //R14151 Bit 2
            FaultMsg[18] = "FLT3018: Blade #12 Position Not Logical";               //R14151 Bit 3
            FaultMsg[19] = "FLT3019: Blade #13 Position Not Logical";               //R14151 Bit 4
            FaultMsg[20] = "FLT3020: Blade #14 Position Not Logical";               //R14151 Bit 5
            FaultMsg[21] = "FLT3021: Blade Slitter Drive Fault ";                   //R14151 Bit 6
            FaultMsg[22] = "FLT3xxx: Contact to Test Alarms R14151 Bit 7";          //R14151 Bit 7
            FaultMsg[23] = "FLT3022: Blade #15 Position Not Logical";               //R14151 Bit 8
            FaultMsg[24] = "FLT3023: Blade #16 Position Not Logical";               //R14151 Bit 9
            FaultMsg[25] = "FLT3024: Blade #17 Position Not Logical";               //R14151 Bit 10
            FaultMsg[26] = "FLT3025: Blade #18 Position Not Logical";               //R14151 Bit 11
            FaultMsg[27] = "FLT3026: Blade #19 Position Not Logical";               //R14151 Bit 12
            FaultMsg[28] = "FLT3027: Slitter Out of 2.0mm Tolerance";               //R14151 Bit 13
            FaultMsg[29] = "FLT3028: Spare Fault Message R14151 Bit 14";            //R14151 Bit 14
            FaultMsg[30] = "FLT3029: Spare Fault Message R14151 Bit 15";            //R14151 Bit 15
            FaultMsg[31] = "FLT3030: Spare Fault Message R14151 Bit 16";            //R14151 Bit 16
            FaultMsg[32] = "FLT3032: Spare Fault Message R14152 Bit 1";             //R14152 Bit 1
            FaultMsg[33] = "FLT3033: Blade Slitters Not Coarse Position Possible";  //R14152 Bit 2
            FaultMsg[34] = "FLT3034: Blade slitters Not Fine Position Possible";    //R14152 Bit 3
            FaultMsg[35] = "FLT3035: Band # 1 Position Not Logical";                //R14152 Bit 4
            FaultMsg[36] = "FLT3036: Band # 2 Position Not Logical";                //R14152 Bit 5
            FaultMsg[37] = "FLT3037: Band # 3 Position Not Logical";                //R14152 Bit 6
            FaultMsg[38] = "FLT3038: Band # 4 Position Not Logical";                //R14152 Bit 7
            FaultMsg[39] = "FLT3039: Band # 5 Position Not Logical";                //R14152 Bit 8
            FaultMsg[40] = "FLT3040: Band # 6 Position Not Logical";                //R14152 Bit 9
            FaultMsg[41] = "FLT3041: Band # 7 Position Not Logical";                //R14152 Bit 10
            FaultMsg[42] = "FLT3042: Band # 8 Position Not Logical";                //R14152 Bit 11
            FaultMsg[43] = "FLT3043: Band # 9 Position Not Logical";                //R14152 Bit 12
            FaultMsg[44] = "FLT3044: Band # 10 Position Not Logical";               //R14152 Bit 13
            FaultMsg[45] = "FLT3045: Band # 11 Position Not Logical";               //R14152 Bit 14
            FaultMsg[46] = "FLT3046: Band # 12 Position Not Logical";               //R14152 Bit 15
            FaultMsg[47] = "FLT3047: Band # 13 Position Not Logical";               //R14152 Bit 16
            FaultMsg[48] = "FLT3048: Band # 14 Position Not Logical";               //R14153 Bit 1
            FaultMsg[49] = "FLT3049: Band Slitter Drive Fault";                     //R14153 Bit 2
            FaultMsg[50] = "FLT3050: Band # 15 Position Not Logical";               //R14153 Bit 3
            FaultMsg[51] = "FLT3051: Band # 16 Position Not Logical";               //R14153 Bit 4
            FaultMsg[52] = "FLT3052: Band # 17 Position Not Logical";               //R14153 Bit 5
            FaultMsg[53] = "FLT3053: Band # 18 Position Not Logical";               //R14153 Bit 6
            FaultMsg[54] = "FLT3054: Band # 19 Position Not Logical";               //R14153 Bit 7
            FaultMsg[55] = "FLT3055: Fault Message Spare R14153 Bit 8";             //R14153 Bit 8
            FaultMsg[56] = "FLT3056: Fault Message Spare R14153 Bit 9";             //R14153 Bit 9
            FaultMsg[57] = "FLT3057: Fault Message Spare R14153 Bit 10";            //R14153 Bit 10
            FaultMsg[58] = "FLT3058: Fault Message Spare R14153 Bit 11";            //R14153 Bit 11
            FaultMsg[59] = "FLT3059: Fault Message Spare R14153 Bit 12";            //R14153 Bit 12
            FaultMsg[60] = "FLT3060: Fault Message Spare R14153 Bit 13";            //R14153 Bit 13
            FaultMsg[61] = "FLT3061: Fault Message Spare R14153 Bit 14";            //R14153 Bit 14
            FaultMsg[62] = "FLT3062: Fault Message Spare R14153 Bit 15";            //R14153 Bit 15
            FaultMsg[63] = "FLT3063: Fault Message Spare R14153 Bit 16";            //R14153 Bit 16
            FaultMsg[64] = "FLT3064: Spare Fault Message R14154 Bit 1";             //R14154 Bit 1
            FaultMsg[65] = "FLT3065: Band # 1 Not Possible";                        //R14154 Bit 2
            FaultMsg[66] = "FLT3066: Blade # 1 Not Possible";                       //R14154 Bit 3
            FaultMsg[67] = "FLT3067: Band # 2 Not Possible";                        //R14154 Bit 4
            FaultMsg[68] = "FLT3068: Blade # 2 Not Possible";                       //R14154 Bit 5
            FaultMsg[69] = "FLT3069: Band # 3 Not Possible";                        //R14154 Bit 6
            FaultMsg[70] = "FLT3070: Blade # 3 Not Possible";                       //R14154 Bit 7
            FaultMsg[71] = "FLT3071: Band # 4 Not Possible";                        //R14154 Bit 8
            FaultMsg[72] = "FLT3072: Blade # 4 Not Possible";                       //R14154 Bit 9
            FaultMsg[73] = "FLT3073: Band # 5 Not Possible";                        //R14154 Bit 10
            FaultMsg[74] = "FLT3074: Blade # 5 Not Possible";                       //R14154 Bit 11
            FaultMsg[75] = "FLT3075: Band # 6 Not Possible";                        //R14154 Bit 12
            FaultMsg[76] = "FLT3076: Blade # 6 Not Possible";                       //R14154 Bit 13
            FaultMsg[77] = "FLT3077: Band # 7 Not Possible";                        //R14154 Bit 14
            FaultMsg[78] = "FLT3078: Blade # 7 Not Possible";                       //R14154 Bit 15
            FaultMsg[79] = "FLT3079: Band # 8 Not Possible";                        //R14154 Bit 16
            FaultMsg[80] = "FLT3080: Blade # 8 Not Possible";                       //R14155 Bit 1
            FaultMsg[81] = "FLT3081: Band # 9 Not Possible";                        //R14155 Bit 2
            FaultMsg[82] = "FLT3082: Blade # 9 Not Possible";                       //R14155 Bit 3
            FaultMsg[83] = "FLT3083: Band # 10 Not Possible";                       //R14155 Bit 4
            FaultMsg[84] = "FLT3084: Blade # 10 Not Possible";                      //R14155 Bit 5
            FaultMsg[85] = "FLT3085: Band # 11 Not Possible";                       //R14155 Bit 6
            FaultMsg[86] = "FLT3086: Blade # 11 Not Possible";                      //R14155 Bit 7
            FaultMsg[87] = "FLT3087: Band # 12 Not Possible";                       //R14155 Bit 8
            FaultMsg[88] = "FLT3088: Blade # 12 Not Possible";                      //R14155 Bit 9
            FaultMsg[89] = "FLT3089: Band # 13 Not Possible";                       //R14155 Bit 10
            FaultMsg[90] = "FLT3090: Blade # 13 Not Possible";                      //R14155 Bit 11
            FaultMsg[91] = "FLT3091: Band # 14 Not Possible";                       //R14155 Bit 12
            FaultMsg[92] = "FLT3092: Blade # 14 Not Possible";                      //R14155 Bit 13
            FaultMsg[93] = "FLT3093: Band # 15 Not Possible";                       //R14155 Bit 14
            FaultMsg[94] = "FLT3094: Blade # 15 Not Possible";                      //R14155 Bit 15
            FaultMsg[95] = "FLT3095: Band # 16 Not Possible";                       //R14155 Bit 16
            FaultMsg[96] = "FLT3096: Blade # 16 Not Possible";                      //R14156 Bit 1
            FaultMsg[97] = "FLT3097: Band # 17 Not Possible";                       //R14156 Bit 2
            FaultMsg[98] = "FLT3098: Blade # 17 Not Possible";                      //R14156 Bit 3
            FaultMsg[99] = "FLT3099: Band # 18 Not Possible";                       //R14156 Bit 4
            FaultMsg[100] = "FLT3100: Blade # 18 Not Possible";                     //R14156 Bit 5
            FaultMsg[101] = "FLT3101: Band # 19 Not Possible";                      //R14156 Bit 6
            FaultMsg[102] = "FLT3102: Blade # 19 Not Possible";                     //R14156 Bit 7
            FaultMsg[103] = "FLT3103: Fault Message Spare R14156 Bit 8";            //R14156 Bit 8
            FaultMsg[104] = "FLT3104: Fault Message Spare R14156 Bit 9";            //R14156 Bit 9
            FaultMsg[105] = "FLT3105: Fault Message Spare R14156 Bit 10";           //R14156 Bit 10
            FaultMsg[106] = "FLT3106: Fault Message Spare R14156 Bit 11";           //R14156 Bit 11
            FaultMsg[107] = "FLT3107: Fault Message Spare R14156 Bit 12";           //R14156 Bit 12
            FaultMsg[108] = "FLT3108: Fault Message Spare R14156 Bit 13";           //R14156 Bit 13
            FaultMsg[109] = "FLT3109: Fault Message Spare R14156 Bit 14";           //R14156 Bit 14
            FaultMsg[110] = "FLT3110: Fault Message Spare R14156 Bit 15";           //R14156 Bit 15
            FaultMsg[111] = "FLT3111: Fault Message Spare R14156 Bit 16";           //R14156 Bit 16
            FaultMsg[112] = "FLT3112: Fault Message Spare R14157 Bit 1";            //R14157 Bit 1
            FaultMsg[113] = "FLT3113: Fault Message Spare R14157 Bit 2";            //R14157 Bit 2
            FaultMsg[114] = "FLT3114: Fault Message Spare R14157 Bit 3";            //R14157 Bit 3
            FaultMsg[115] = "FLT3115: Fault Message Spare R14157 Bit 4";            //R14157 Bit 4
            FaultMsg[116] = "FLT3116: Fault Message Spare R14157 Bit 5";            //R14157 Bit 5
            FaultMsg[117] = "FLT3117: Fault Message Spare R14157 Bit 6";            //R14157 Bit 6
            FaultMsg[118] = "FLT3118: Fault Message Spare R14157 Bit 7";            //R14157 Bit 7
            FaultMsg[119] = "FLT3119: Fault Message Spare R14157 Bit 8";            //R14157 Bit 8
            FaultMsg[120] = "FLT3120: Fault Message Spare R14157 Bit 9";            //R14157 Bit 9
            FaultMsg[121] = "FLT3121: Fault Message Spare R14157 Bit 10";           //R14157 Bit 10
            FaultMsg[122] = "FLT3122: Fault Message Spare R14157 Bit 11";           //R14157 Bit 11
            FaultMsg[123] = "FLT3123: Slit #1 Overload OverTemp Fault";             //R14157 Bit 12
            FaultMsg[124] = "FLT3124: Slit #2 Overload OverTemp Fault";             //R14157 Bit 13
            FaultMsg[125] = "FLT3125: Slit #3 Overload OverTemp Fault";             //R14157 Bit 14
            FaultMsg[126] = "FLT3126: Slit #4 Overload OverTemp Fault";             //R14157 Bit 15
            FaultMsg[127] = "FLT3127: Slit #5 Overload OverTemp Fault";             //R14157 Bit 16
            FaultMsg[128] = "FLT3128: Slit #6 Overload OverTemp Fault";             //R14158 Bit 1
            FaultMsg[129] = "FLT3129: Slit #7 Overload OverTemp Fault";             //R14158 Bit 2
            FaultMsg[130] = "FLT3130: Slit #8 Overload OverTemp Fault";             //R14158 Bit 3
            FaultMsg[131] = "FLT3131: Slit #9 Overload OverTemp Fault";             //R14158 Bit 4
            FaultMsg[132] = "FLT3132: Slit #10 Overload OverTemp Fault";            //R14158 Bit 5
            FaultMsg[133] = "FLT3133: Slit #11 Overload OverTemp Fault";            //R14158 Bit 6
            FaultMsg[134] = "FLT3134: Slit #12 Overload OverTemp Fault";            //R14158 Bit 7
            FaultMsg[135] = "FLT3135: Slit #13 Overload OverTemp Fault";            //R14158 Bit 8
            FaultMsg[136] = "FLT3136: Slit #14 Overload OverTemp Fault";            //R14158 Bit 9
            FaultMsg[137] = "FLT3137: Fault Message Spare R14158 Bit 10";           //R14158 Bit 10
            FaultMsg[138] = "FLT3138: Air Pressure Missing";                        //R14158 Bit 11
            FaultMsg[139] = "FLT3140: Slit #15 Overload OverTemp Fault";            //R14158 Bit 12
            FaultMsg[140] = "FLT3141: Slit #16 Overload OverTemp Fault";            //R14158 Bit 13
            FaultMsg[141] = "FLT3142: Slit #17 Overload OverTemp Fault";            //R14158 Bit 14
            FaultMsg[142] = "FLT3143: Slit #18 Overload OverTemp Fault";            //R14158 Bit 15
            FaultMsg[143] = "FLT3144: Slit #19 Overload OverTemp Fault";            //R14158 Bit 16

            return FaultMsg;
        }
    }
}
