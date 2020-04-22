using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;
using ZedGraph;
using Excel = Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using System.IO;
using System.Collections;

public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved;
}


/////////////////////////////////////////////////////

unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;
    public byte TimeFlag;
    public byte SendType;
    public byte RemoteFlag;//是否是远程帧
    public byte ExternFlag;//是否是扩展帧
    public byte DataLen;

    public fixed byte Data[8];

    public fixed byte Reserved[3];

}

//public struct VCI_CAN_OBJ 
//{
//    public UInt32 ID;
//    public UInt32 TimeStamp;
//    public byte TimeFlag;
//    public byte SendType;
//    public byte RemoteFlag;
//    public byte ExternFlag;
//    public byte DataLen;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
//    public byte[] Data;
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
//    public byte[] Reserved;

//    public void Init()
//    {
//        Data = new byte[8];
//        Reserved = new byte[3];
//    }
//}

public struct VCI_CAN_STATUS
{
    public byte ErrInterrupt;
    public byte regMode;
    public byte regStatus;
    public byte regALCapture;
    public byte regECCapture;
    public byte regEWLimit;
    public byte regRECounter;
    public byte regTECounter;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}

public struct VCI_ERR_INFO
{
    public UInt32 ErrCode;
    public byte Passive_ErrData1;
    public byte Passive_ErrData2;
    public byte Passive_ErrData3;
    public byte ArLost_ErrData;
}

public struct VCI_INIT_CONFIG
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;
    public byte Timing0;
    public byte Timing1;
    public byte Mode;
}

public struct CHGDESIPANDPORT
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}

///////// new add struct for filter /////////
//typedef struct _VCI_FILTER_RECORD{
//    DWORD ExtFrame;	
//    DWORD Start;
//    DWORD End;
//}VCI_FILTER_RECORD,*PVCI_FILTER_RECORD;
public struct VCI_FILTER_RECORD
{
    public UInt32 ExtFrame;
    public UInt32 Start;
    public UInt32 End;
}

namespace Torque_Sensor
{   
    public partial class PForm : Form
    {
        const int VCI_PCI5121 = 1;
        const int VCI_PCI9810 = 2;
        const int VCI_USBCAN1 = 3;
        const int VCI_USBCAN2 = 4;
        const int VCI_USBCAN2A = 4;
        const int VCI_PCI9820 = 5;
        const int VCI_CAN232 = 6;
        const int VCI_PCI5110 = 7;
        const int VCI_CANLITE = 8;
        const int VCI_ISA9620 = 9;
        const int VCI_ISA5420 = 10;
        const int VCI_PC104CAN = 11;
        const int VCI_CANETUDP = 12;
        const int VCI_CANETE = 12;
        const int VCI_DNP9810 = 13;
        const int VCI_PCI9840 = 14;
        const int VCI_PC104CAN2 = 15;
        const int VCI_PCI9820I = 16;
        const int VCI_CANETTCP = 17;
        const int VCI_PEC9920 = 18;
        const int VCI_PCI5010U = 19;
        const int VCI_USBCAN_E_U = 20;
        const int VCI_USBCAN_2E_U = 21;
        const int VCI_PCI5020U = 22;
        const int VCI_EG20T_CAN = 23;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        //static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        unsafe static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, byte* pData);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        static UInt32 m_devtype = 21;//USBCAN-2e-u
        static UInt32[] GCanBrTab = new UInt32[10]{
                    0x060003, 0x060004, 0x060007,
                        0x1C0008, 0x1C0011, 0x160023,
                        0x1C002C, 0x1600B3, 0x1C00E0,
                        0x1C01C1
                };

        //--------Configure CAN--------//
        static byte[] Filter = new byte[2]{
            0, //Dual Filter
            1, //Single Filer
        };
        static byte[] Mode = new byte[2]{
            0, //Normal
            1, //Only Listen
        };

        const UInt32 ACCCODE = 0x00000000;
        const UInt32 ACCMASK = 0xFFFFFFFF;
        const byte TIMING0 = 0x00;
        const byte TIMING1 = 0x14;

        //--------Configure Transmit Data--------//
        const UInt32 _IDCANBUS_ARM_1 = 0x121;
        const UInt32 _IDCANBUS_ARM_2 = 0x122;
        const UInt32 _IDCANBUS_MASS_SHIFTER = 0x123;
        const UInt32 _IDCANBUS_PISTOL = 0x124;
        const UInt32 _IDCANBUS_THRUSTER = 0x125;
        const UInt32 _IDCANBUS_EPC = 0x126;
        const UInt32 _IDCANBUS_RUDDER = 0x127;

        const byte _DATA_TRANSMIT_LENGTH = 8;

        //--------Configure Filter--------//
        static byte[] Filter_Mode = new byte[3]{
            0, //Standard Frame
            1, //Extended Frame
            2, //Disabled Filter
        };
        const UInt32 STARTID = 0x121;
        const UInt32 ENDID = 0x126;

        static byte[] Send_Type = new byte[4]{
            0, //Send normally
            1, //Single Normal Send
            2, //Spontaneous collection
            3, //Single spontaneous collection
        };

        static byte[] Frame_Type = new byte[2]{
            0, //Standard Frame
            1, //Extended Frame
        };

        static byte[] Frame_Format = new byte[2]{
            0, //Data Frame
            1, //Remote Frame
        };

        ////////////////////////////////////////
        const UInt32 STATUS_OK = 1;

        UInt32 m_devind = 0;
        UInt32 m_canind = 0;

        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[50];

        UInt32[] m_arrdevtype = new UInt32[20];

        
        int tickStart = 0;
        int intlen = 0;
        Int32 indexRow = 0;
        Boolean invert_Motor = true;
        Boolean Excel_flag = false;
        Boolean Flag_Motor = false;
        Boolean Flag_Thrust = false;
        Boolean PID_Fuzzy = true;
        Boolean Connect2CANBUS = false;
        Boolean Error_Overload_Motor = false;
        Boolean Error_Sticked_Motor = false;

        //-------------------------------------------------------Transmit data ARM-----------------------------------------------------------------
        Int16 StdID_TX = 0x0123;
        Int16 ExtID_TX = 0x0000;
        byte DLC_TX = 8;

        Int16 StdID_RX = 0x0123;
        Int16 ExtID_RX = 0x0000;

        byte[] CAN_Command2ARM = new byte[8];
        byte[] ByteArrays_Value = new byte[4] { 0, 0, 0, 0 };

        public struct CANUSB_ARM2PC
        {
            public float Percentage_DC, Des_Speed, r_Speed, Current;
        }
        public CANUSB_ARM2PC DataNonCheckSum_ARM2PC = new CANUSB_ARM2PC();

        public CANUSB_ARM2PC DataDoneCheckSum_ARM2PC = new CANUSB_ARM2PC();

        // Gia tri nhan tu thiet bi Jadever-3100
        public struct Received_Jadever_3100
        {
            public float Thrust, Temp_On_Chip, Temp_Amnient;
        }
        public Received_Jadever_3100 Data_Jadever_3100 = new Received_Jadever_3100();

        const int Jadever_Buffer_Len = 16 - 7; // '    0.000<-enter.'

        //------ Image Processing ------//
        Image img = new Bitmap(Properties.Resources.Propeller);

        public PForm()
        {
            InitializeComponent();
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            DialogResult Re = MessageBox.Show("Bạn thật sự muốn thoát chương trình?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (Re == DialogResult.Yes)
            {
                if(Connect2CANBUS)
                {
                    Application.Exit();
                    Connect2CANBUS = false;
                }
                MessageBox.Show("Cảm ơn bạn đã sử dụng chương trình", "VIAM LAB");
                this.Close();
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            if (SlideMenu.Width == 50)
            {
                SlideMenu.Visible = false;
                SlideMenu.Width = 385;
                btnCommunication.Enabled = true;
                btnMeasurement.Enabled = true;
                btnExcel.Enabled = true;
                dTime.Visible = true;
                PanelAnimator.ShowSync(SlideMenu);
                LogoAnimator.ShowSync(LogoBK);
            }
            else
            {
                LogoAnimator.HideSync(LogoBK);
                SlideMenu.Visible = false;
                SlideMenu.Width = 50;
                btnCommunication.Enabled = false;
                btnMeasurement.Enabled = false;
                btnExcel.Enabled = false;
                dTime.Visible = false;
                PanelAnimator.ShowSync(SlideMenu);
            }
        }

        #region Variables
        //----------------------------------------------------------SetText--------------------------------------------------------------------
        delegate void SetTextCallback(string text); // Khai bao delegate 

        private void SetText_txtThrust(string s)
        {
            if (txtThrust.InvokeRequired)
            {
                SetTextCallback sd = new SetTextCallback(SetText_txtThrust);
                txtThrust.Invoke(sd, new object[] { s });
            }
            else
            {
                txtThrust.Text = s;
            }
        }

        private void SetText_Temp_On_Chip(string text)
        {
            if (this.txtTempOnChip.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_Temp_On_Chip); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.txtTempOnChip.Text = text;
        }

        private void SetText_Temp_Thermocouple(string text)
        {
            if (this.txtTempAmbient.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_Temp_Thermocouple); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.txtTempAmbient.Text = text;
        }

        private void SetText_Percent_DC(string text)
        {
            if (this.tbPercent_DC.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_Percent_DC); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.tbPercent_DC.Text = text;
        }

        private void SetText_Des_Speed(string text)
        {
            if (this.tbDes_Speed.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_Des_Speed); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.tbDes_Speed.Text = text;
        }

        private void SetText_r_Speed(string text)
        {
            if (this.tbr_Speed.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_r_Speed); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.tbr_Speed.Text = text;
        }

        private void SetText_iMotor(string text)
        {
            if (this.tbi_Motor.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText_iMotor); // khởi tạo 1 delegate mới gọi đến SetText
                this.Invoke(d, new object[] { text });
            }
            else this.tbi_Motor.Text = text;
        }

        //private void UpdateStatuslb1(string name)
        //{
        //    // InvokeRequired is true if current thread is not the thread which created the control
        //    // use delegate to invoke the function in the thread that created the control
        //    if (StatusCANBUS.InvokeRequired)
        //    {
        //        //? why NOT work
        //        //SetStatusCallback cb = new SetStatusCallback(UpdateStatusUI);
        //        this.StatusCANBUS.Invoke(new Action<string>(n => toolStripStatuslb1.Text = n), new object[] { name });
        //    }
        //    else
        //    {
        //        // false if current thread is the thread which created the control
        //        this.toolStripStatuslb1.Text = name;
        //    }
        //}

        //private void UpdateStatuslb2(string name)
        //{
        //    // InvokeRequired is true if current thread is not the thread which created the control
        //    // use delegate to invoke the function in the thread that created the control
        //    if (StatusCANBUS.InvokeRequired)
        //    {
        //        //? why NOT work
        //        //SetStatusCallback cb = new SetStatusCallback(UpdateStatusUI);
        //        this.StatusCANBUS.Invoke(new Action<string>(n => toolStripStatuslb2.Text = n), new object[] { name });
        //    }
        //    else
        //    {
        //        // false if current thread is the thread which created the control
        //        this.toolStripStatuslb2.Text = name;
        //    }
        //}

        #endregion

        #region Checksum
        private byte Checksum(byte[] data, int start, int count)//tinh tu Data[start] den Data[count+start-1]
        {
            int value = 0;

            //Calculate CheckSum (Byte)
            for (int i = start; i < (count + start); i++)
            {
                value += data[i];
            }
            value = ~value;
            value++;
            return (byte)value;
        }

        unsafe private byte Checksum_UsingPointer(byte* data, int start, int count)//tinh tu Data[start] den Data[count+start-1]
        {
            int value = 0;

            //Calculate CheckSum (Byte)
            for (int i = start; i < (count + start); i++)
            {
                value += data[i];
            }
            value = ~value;
            value++;
            return (byte)value;
        }

        #endregion

        #region Transmit Data
        unsafe private void UCAN_Transmit_Data(UInt32 _id, byte _data_length, byte[] _data)
        {
            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.SendType = (byte)Send_Type[2];
            sendobj.RemoteFlag = (byte)Frame_Format[0];
            sendobj.ExternFlag = (byte)Frame_Type[0];
            sendobj.ID = _id;
            sendobj.DataLen = _data_length;
            sendobj.Data[0] = (byte)_data[0];
            sendobj.Data[1] = (byte)_data[1];
            sendobj.Data[2] = (byte)_data[2];
            sendobj.Data[3] = (byte)_data[3];
            sendobj.Data[4] = (byte)_data[4];
            sendobj.Data[5] = (byte)_data[5];
            sendobj.Data[6] = (byte)_data[6];
            sendobj.Data[7] = (byte)_data[7];
            int nTimeOut = 3000;
            VCI_SetReference(m_devtype, m_devind, m_canind, 4, (byte*)&nTimeOut);
            if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
            {
                MessageBox.Show("Send Fail!!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //string ESC = System.Text.ASCIIEncoding.ASCII.GetString(new byte[] { 27 });
            //serialCOMThrust.NewLine = ESC + "enter.";
            timer_updatedata.Interval = 100;
            timer_updatedata.Enabled = false;
            string[] Measurement = { "Motor Controller", "Thrust Measurement" };
            for(int i = 0; i < Measurement.Length; i++)
            {
                dropMeasurement.AddItem(Measurement[i]);
            }

            string[] productname = { "USBCAN_E_U", "USBCAN_2E_U", "PCI5010U", "PCI5020U" };
            for (int i = 0; i < productname.Length; i++)
            {
                dropProductName.AddItem(productname[i]);
            }

            UInt16 _product_cnt = 0;
            
            foreach (string _product in productname)
            {
                switch(_product)
                {
                    case "USBCAN_E_U":
                    {
                            m_arrdevtype[_product_cnt] = VCI_USBCAN_E_U;
                            break;
                    }
                    case "USBCAN_2E_U":
                    {
                            m_arrdevtype[_product_cnt] = VCI_USBCAN_2E_U;
                            break;
                    }
                    case "PCI5010U":
                    {
                            m_arrdevtype[_product_cnt] = VCI_PCI5010U;
                            break;
                    }
                    case "PCI5020U":
                    {
                            m_arrdevtype[_product_cnt] = VCI_PCI5020U;
                            break;
                    }
                }
                _product_cnt++;
            }

            dropProductName.selectedIndex = 1;
            dropCANIndex.selectedIndex = 0;
            dropIndex.selectedIndex = 0;
            dropBaudrate.selectedIndex = 0;

            /////////////////////////Screen////////////////////////
            lbError_Overload.Visible = false;
            lbError_Sticked.Visible = false;
            PanelCom.Visible = false;
            MeasurementPanel.Visible = false;
            panelExcel.Visible = false;
            LogoAnimator.HideSync(LogoBK);
            SlideMenu.Visible = false;
            SlideMenu.Width = 50;
            btnCommunication.Enabled = false;
            btnMeasurement.Enabled = false;
            btnExcel.Enabled = false;
            dTime.Visible = false;
            PanelAnimator.ShowSync(SlideMenu);
            PanelFuzzyController.Visible = false;
            PanelPIDController.Visible = true;
            PanelSelectMeasurement.Visible = false;
            btnExcel.Location = new Point(-2, 360);
            dTime.Location = new Point(92, 456);
            CheckMotorController.Checked = false;
            CheckTorqueMeasurement.Checked = false;
            CheckThrustMeasurement.Checked = false;
            grHallSensor.Enabled = false;
            grMotorController.Enabled = false;
            grThrustMeasurement.Enabled = false;
            btMotorStop.Enabled = false;
            btnReverse.Enabled = false;
            btnForward.Enabled = false;           
            btnForward.Image = Properties.Resources.Apps_Rotate_Right_icon;
            btnReverse.Image = Properties.Resources.Left_Disable;
            panelCOMMotorController.Visible = false;
            panelCOMThrust.Visible = false;
            btGraphTorqueThrust.Visible = false;
            //////////////////////Motor Zed Graph////////////////////////
            // pane used to draw your chart
            GraphPane myMotorPane = new GraphPane();
            // set your pane
            myMotorPane = zedGraphControl.GraphPane;
            // set a title
            myMotorPane.Title.Text = "BLDC";
            // set X and Y axis titles
            myMotorPane.XAxis.Title.Text = "Time[s]";
            myMotorPane.YAxis.Title.Text = "Speed[rpm]  Current[mA]";

            //create lists of point
            RollingPointPairList list1 = new RollingPointPairList(60000);
            RollingPointPairList list2 = new RollingPointPairList(60000);
            RollingPointPairList list3 = new RollingPointPairList(60000);

            //create curves
            LineItem curve1 = myMotorPane.AddCurve("y_d", list1, Color.Red, SymbolType.None);
            LineItem curve2 = myMotorPane.AddCurve("y", list2, Color.Blue, SymbolType.None);
            LineItem curve3 = myMotorPane.AddCurve("I", list3, Color.Green, SymbolType.None);

            //Set time interval
            timer_draw.Interval = 1;
            timer_draw.Stop();
            //Set up X Axis
            myMotorPane.XAxis.Scale.Min = 0;
            myMotorPane.XAxis.Scale.Max = 30;
            myMotorPane.XAxis.Scale.MinorStep = 1;
            myMotorPane.XAxis.Scale.MajorStep = 5;

            //make the change on graph
            zedGraphControl.AxisChange();

            zedGraphControl.Visible = true;

            ///////////////////////////////Torque Graph/////////////////////////////////
            GraphPane myTorquePane = new GraphPane();
            // set your pane
            myTorquePane = TorqueGraph.GraphPane;
            // set a title
            myTorquePane.Title.Text = "Thrust/Temperature Measurement";
            // set X and Y axis titles
            myTorquePane.XAxis.Title.Text = "Time[s]";
            myTorquePane.YAxis.Title.Text = "Thrust [N]  Temperature[oC]";

            //create lists of point
            RollingPointPairList list4 = new RollingPointPairList(60000);
            RollingPointPairList list5 = new RollingPointPairList(60000);
            RollingPointPairList list6 = new RollingPointPairList(60000);

            //create curves
            LineItem curve4 = myTorquePane.AddCurve("Thrust", list4, Color.Red, SymbolType.None);
            LineItem curve5 = myTorquePane.AddCurve("Temp_On_Chip", list5, Color.Blue, SymbolType.None);
            LineItem curve6 = myTorquePane.AddCurve("Temp_Ambient", list6, Color.Green, SymbolType.None);

            //Set time interval
            timer_draw.Interval = 1;
            timer_draw.Stop();
            //Set up X Axis
            myTorquePane.XAxis.Scale.Min = 0;
            myTorquePane.XAxis.Scale.Max = 30;
            myTorquePane.XAxis.Scale.MinorStep = 1;
            myTorquePane.XAxis.Scale.MajorStep = 5;

            //make the change on graph
            TorqueGraph.AxisChange();

            TorqueGraph.Visible = true;
            //////////////////////////////Excel//////////////////////////////
            for(int i = 0; i < 8; i++)
            {
                this.dataGridExcel.Columns[i].Visible = false;
            }
        }

        private void PForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Connect2CANBUS == true)
            {
                VCI_CloseDevice(m_devtype, m_devind);
            }
        }

        private void btnPID_Fuzzy_Click(object sender, EventArgs e)
        {
            if(PID_Fuzzy == true)
            {
                btnPID_Fuzzy.Image = Properties.Resources.Switch_Fuzzy;
                PanelFuzzyController.Visible = true;
                PanelPIDController.Visible = false;
                PID_Fuzzy = false;
            }
            else
            {
                btnPID_Fuzzy.Image = Properties.Resources.Switch_PID_2;
                PanelPIDController.Visible = true;
                PanelFuzzyController.Visible = false;
                PID_Fuzzy = true;
            }
        }

        private void btnCommunication_Click(object sender, EventArgs e)
        {
            PanelSelectMeasurement.Visible = false;
            btnExcel.Location = new Point(-2, 360);
            dTime.Location = new Point(92, 456);
            PanelCom.Visible = true;
            MeasurementPanel.Visible = false;
            panelExcel.Visible = false;
        }

        private void btnMeasurement_Click(object sender, EventArgs e)
        {
            btnExcel.Location = new Point(-2, 435);
            dTime.Location = new Point(92, 509);
            PanelSelectMeasurement.Visible = true;
            PanelCom.Visible = false;
            MeasurementPanel.Visible = true;
            panelExcel.Visible = false;
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            btnExcel.Location = new Point(-2, 435);
            dTime.Location = new Point(92, 509);
            PanelSelectMeasurement.Visible = true;
            PanelCom.Visible = false;
            MeasurementPanel.Visible = true;
            panelExcel.Visible = true;
        }

        private void dropMeasurement_onItemSelected(object sender, EventArgs e)
        {
            if(dropMeasurement.selectedIndex == 0)
            {
                panelCOMMotorController.Visible = true;
                panelCOMThrust.Visible = false;
            }
            else
            {
                panelCOMThrust.Visible = true;
                panelCOMMotorController.Visible = false;              
            }
        }

        unsafe private void btnConnectMotor_Click(object sender, EventArgs e)
        {
            if (btnConnectMotor.ButtonText == "Connect")
            {
                try
                {
                    m_devtype = m_arrdevtype[dropProductName.selectedIndex];
                    m_devind = (UInt32)dropIndex.selectedIndex;
                    m_canind = (UInt32)dropCANIndex.selectedIndex;

                    if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
                    {
                        MessageBox.Show("Can not open deivice!!!. Please check Device Type and Index", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    UInt32 baud;
                    baud = GCanBrTab[dropBaudrate.selectedIndex];
                    if (VCI_SetReference(m_devtype, m_devind, m_canind, 0, (byte*)&baud) != STATUS_OK)
                    {

                        MessageBox.Show("Error baudrate. Can not open device!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(m_devtype, m_devind);
                        return;
                    }

                    //////////////////////////////////////////////////////////////////////////
                    VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
                    config.AccCode = ACCCODE;
                    config.AccMask = ACCMASK;
                    config.Timing0 = TIMING0;
                    config.Timing1 = TIMING1;
                    config.Filter = (Byte)Filter[1];
                    config.Mode = (Byte)Mode[0];
                    VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);

                    //////////////////////////////////////////////////////////////////////////
                    Int32 filterMode = Filter_Mode[0];
                    if (2 != filterMode)
                    {
                        VCI_FILTER_RECORD filterRecord = new VCI_FILTER_RECORD();
                        filterRecord.ExtFrame = (UInt32)filterMode;
                        filterRecord.Start = STARTID;
                        filterRecord.End = ENDID;

                        VCI_SetReference(m_devtype, m_devind, m_canind, 1, (byte*)&filterRecord);

                        byte tm = 0;
                        if (VCI_SetReference(m_devtype, m_devind, m_canind, 2, &tm) != STATUS_OK)
                        {
                            MessageBox.Show("Configure Filter Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            VCI_CloseDevice(m_devtype, m_devind);
                            return;
                        }
                    }

                    if(VCI_StartCAN(m_devtype, m_devind, m_canind) == 1)
                    {
                        Connect2CANBUS = true;

                        if (Connect2CANBUS == true)
                        {
                            Flag_Motor = true;
                            btnConnectMotor.ButtonText = "Disconnect";
                            CheckMotorController.Checked = true;
                            grHallSensor.Enabled = true;
                            grMotorController.Enabled = true;
                            btMotorStop.Enabled = true;
                            if (invert_Motor == true)
                            {
                                btnReverse.Enabled = true;
                                btnForward.Enabled = false;
                            }
                            else
                            {
                                btnForward.Enabled = true;
                                btnReverse.Enabled = false;
                            }

                            CAN_Command2ARM[0] = (byte)'C';//Open
                            CAN_Command2ARM[1] = (byte)'A';//Loop
                            CAN_Command2ARM[2] = (byte)'N';

                            CAN_Command2ARM[3] = (byte)'O';
                            CAN_Command2ARM[4] = 0x00;
                            CAN_Command2ARM[5] = 0x00;
                            CAN_Command2ARM[6] = 0x00;
                            CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                            UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                            //--------------------------------------------------------------------------------------------------------------
                            Thread.Sleep(500);
                            timer_updatedata.Enabled = true;
                            bgrReceiveData.RunWorkerAsync();
                            ///////////ProgressBar//////////////
                            pgStatusMotor.Value = 100;
                            for (int i = 0; i < 4; i++)
                            {
                                this.dataGridExcel.Columns[i].Visible = true;
                            }
                        }
                    }
                   
                }
                catch (Exception)
                {
                }
            }
            else
            {
                if (bgrReceiveData.IsBusy)
                {
                    bgrReceiveData.CancelAsync();
                }                
                timer_updatedata.Enabled = false;

                Flag_Motor = false;
                btnConnectMotor.ButtonText = "Connect";
                CheckMotorController.Checked = false;
                grHallSensor.Enabled = false;
                grMotorController.Enabled = false;
                btMotorStop.Enabled = false;
                btnForward.Enabled = false;
                btnReverse.Enabled = false;

                CAN_Command2ARM[0] = (byte)'C';//Open
                CAN_Command2ARM[1] = (byte)'A';//Loop
                CAN_Command2ARM[2] = (byte)'N';

                CAN_Command2ARM[3] = (byte)'C';
                CAN_Command2ARM[4] = 0x00;
                CAN_Command2ARM[5] = 0x00;
                CAN_Command2ARM[6] = 0x00;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                
                //--------------------------------------------------------------------------------------------------------------
                pgStatusMotor.Value = 0;
                for (int i = 0; i < 4; i++)
                {
                    this.dataGridExcel.Columns[i].Visible = false;
                }
          
                VCI_ResetCAN(m_devtype, m_devind, m_canind);
                VCI_CloseDevice(m_devtype, m_devind);
            }
        }

        private void btnConnectThrust_Click(object sender, EventArgs e)
        {
            if (btnConnectThrust.ButtonText == "Connect")
            {
                try
                {
                    if (dropCOMThrust.selectedValue.ToString() != "")
                    {
                        ////////////Set up COM//////////////
                        serialCOMThrust.PortName = dropCOMThrust.selectedValue.ToString();
                        serialCOMThrust.BaudRate = Int32.Parse(dropBaudRateThrust.selectedValue.ToString());
                        serialCOMThrust.DataBits = Int32.Parse(dropDatabitsThrust.selectedValue.ToString());
                        if (dropParityThrust.selectedValue.ToString() == "None")
                        {
                            serialCOMThrust.Parity = Parity.None;
                        }
                        if (dropParityThrust.selectedValue.ToString() == "Odd")
                        {
                            serialCOMThrust.Parity = Parity.Odd;
                        }
                        if (dropParityThrust.selectedValue.ToString() == "Even")
                        {
                            serialCOMThrust.Parity = Parity.Even;
                        }
                        switch (dropStopbitsThrust.selectedValue.ToString())
                        {
                            case "1":
                                {
                                    serialCOMThrust.StopBits = StopBits.One;
                                    break;
                                }
                            case "1.5":
                                {
                                    serialCOMThrust.StopBits = StopBits.OnePointFive;
                                    break;
                                }
                            case "2":
                                {
                                    serialCOMThrust.StopBits = StopBits.Two;
                                    break;
                                }
                        }
                        serialCOMThrust.Open();
                        if (serialCOMThrust.IsOpen)
                        {
                            Flag_Thrust = true;
                            btnConnectThrust.ButtonText = "Disconnect";
                            dropCOMThrust.Enabled = false;
                            dropDatabitsThrust.Enabled = false;
                            dropBaudRateThrust.Enabled = false;
                            dropParityThrust.Enabled = false;
                            dropStopbitsThrust.Enabled = false;
                            CheckThrustMeasurement.Checked = true;
                            grThrustMeasurement.Enabled = true;
                            ///////////ProgressBar//////////////
                            pgStatusThrust.Value = 100;
                            this.dataGridExcel.Columns[4].Visible = true;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Không thể kết nối với cổng COM", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Flag_Thrust = false;
                btnConnectThrust.ButtonText = "Connect";
                dropCOMThrust.Enabled = true;
                dropDatabitsThrust.Enabled = true;
                dropBaudRateThrust.Enabled = true;
                dropParityThrust.Enabled = true;
                dropStopbitsThrust.Enabled = true;
                CheckThrustMeasurement.Checked = false;
                grThrustMeasurement.Enabled = false;
                pgStatusThrust.Value = 0;
                this.dataGridExcel.Columns[4].Visible = false;
                serialCOMThrust.Close();
            }
        }

        private void tabGraph_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabGraph.SelectedIndex == 0)
            {
                btGraphMotor.Visible = true;
                btGraphTorqueThrust.Visible = false;
            }
            else if(tabGraph.SelectedIndex == 1)
            {
                btGraphTorqueThrust.Visible = true;
                btGraphMotor.Visible = false;
            }
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnCompact_Click(object sender, EventArgs e)
        {
            if (btGraphMotor.ButtonText == "Scroll")
            {
                btGraphMotor.ButtonText = "Compact";
            }
            else
            {
                btGraphMotor.ButtonText = "Scroll";
            }
        }

        private void btGraphTorqueThrust_Click(object sender, EventArgs e)
        {
            if (btGraphTorqueThrust.ButtonText == "Scroll")
            {
                btGraphTorqueThrust.ButtonText = "Compact";
            }
            else
            {
                btGraphTorqueThrust.ButtonText = "Scroll";
            }
        }

        private void timer_draw_Tick(object sender, EventArgs e)
        {
            if(Flag_Motor == true)
            {
                drawMotorGraph(DataDoneCheckSum_ARM2PC.Des_Speed, DataDoneCheckSum_ARM2PC.r_Speed, DataDoneCheckSum_ARM2PC.Current);
            }
            if((Flag_Thrust == true)||(Flag_Motor == true))
            {
                drawTorqueGraph(Data_Jadever_3100.Thrust,Data_Jadever_3100.Temp_On_Chip,Data_Jadever_3100.Temp_Amnient);
            }   
               
            //-------------------------------------------- Write Excel------------------------------------------------------
            if (Excel_flag == true)
            {
                dataGridExcel.Rows.Add(1);
                if(Flag_Motor == true)
                {
                    dataGridExcel[0, indexRow].Value = Math.Round(DataDoneCheckSum_ARM2PC.Percentage_DC, 4);
                    dataGridExcel[1, indexRow].Value = Math.Round(DataDoneCheckSum_ARM2PC.Des_Speed, 4);
                    dataGridExcel[2, indexRow].Value = Math.Round(DataDoneCheckSum_ARM2PC.r_Speed, 4);
                    dataGridExcel[3, indexRow].Value = Math.Round(DataDoneCheckSum_ARM2PC.Current, 4);
                }
                if(Flag_Thrust == true)
                {
                    dataGridExcel[4, indexRow].Value = Math.Round(Data_Jadever_3100.Thrust, 2);
                }        
                
                dataGridExcel.FirstDisplayedScrollingRowIndex = dataGridExcel.RowCount - 1;
                indexRow++;
            }
        }

        #region Receive_Data

        private void serialCOMThrust_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (serialCOMThrust.IsOpen)
            {
                string s = serialCOMThrust.ReadLine();
                if (s.Length == Jadever_Buffer_Len)
                {
                    string temp = "";
                    for (int i = 1; i < Jadever_Buffer_Len; i++)
                    {
                        temp += s[i].ToString();
                    }
                    float.TryParse(temp, out Data_Jadever_3100.Thrust);
                    if (s[0] == '-')
                    {
                        Data_Jadever_3100.Thrust *= (float)-10.0;
                    }
                    else
                    {
                        Data_Jadever_3100.Thrust *= (float)10.0;
                    }
                    SetText_txtThrust(Data_Jadever_3100.Thrust.ToString("0.0") + " " + "N");
                }
                //while (serialCOMThrust.BytesToRead >= Jadever_Buffer_Len )
                //{
                //    string s = serialCOMThrust.ReadExisting();
                //    string s_1 = s[5].ToString() + s[6].ToString() + s[7].ToString() + s[8].ToString();


                //    float.TryParse(s_1, out temp);
                //    Data_Jadever_3100.Thrust = temp/ 100f;
                //    SetText_txtThrust(Data_Jadever_3100.Thrust.ToString("0.0"));
                /*if (s[5] == 13 && s[6] == 10)
                {
                    float.TryParse(s[9].ToString() + s[10] + s[11] + s[12] + s[13] + s[14] + s[15] + s[16], out Data_Jadever_3100.Thrust);
                    if (s[7] == ' ')
                    {
                        Data_Jadever_3100.Thrust *= (float)0.1;

                    }
                    else if (s[7] == '-')
                    {
                        Data_Jadever_3100.Thrust *= (float)-0.1;
                    }
                    SetText_txtThrust(Data_Jadever_3100.Thrust.ToString("0.0") + " " + "N");
                }*/
                //}
            }
        }

        #endregion

        #region Motor Controller

        private void btSend_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtDC.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else if ((Int16.Parse(txtDC.Text) < 0) || (Int16.Parse(txtDC.Text) > 100))
                {
                    MessageBox.Show("Dữ liệu nhập sai! Vui lòng nhập lại 0 -> 100", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtDC.Focus();
                }
                else if ((Int16.Parse(txtDC.Text) > 70))
                {
                    DialogResult Re = MessageBox.Show("Bạn thật sự muốn điều khiển với " + txtDC.Text + "%", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (Re == DialogResult.Yes)
                    {
                        txtOpenLoop.ForeColor = Color.Red;
                        txtCloseLoop.ForeColor = Color.White;
                        btnReverse.Enabled = false;
                        btnForward.Enabled = false;
                        bt_des_Speed.Enabled = false;
                       
                        CAN_Command2ARM[0] = (byte)'O';//Open
                        CAN_Command2ARM[1] = (byte)'L';//Loop

                        if (invert_Motor == true)
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'R';//Right_direction
                        }
                        else
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'L';//Left_direction
                        }

                        //Value
                        ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtDC.Text));//Duty cycle

                        CAN_Command2ARM[3] = ByteArrays_Value[3];
                        CAN_Command2ARM[4] = ByteArrays_Value[2];
                        CAN_Command2ARM[5] = ByteArrays_Value[1];
                        CAN_Command2ARM[6] = ByteArrays_Value[0];
                        CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                        UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                        timer_rotatepropeller.Enabled = true;
                        //--------------------------------------------------------------------------------------------------------------
                    }
                    else
                    {
                        txtDC.Text = "0";
                        txtDC.Focus();
                    }
                }
                else
                {
                    txtOpenLoop.ForeColor = Color.Red;
                    txtCloseLoop.ForeColor = Color.White;
                    btnReverse.Enabled = false;
                    btnForward.Enabled = false;
                    bt_des_Speed.Enabled = false;

                    CAN_Command2ARM[0] = (byte)'O';//Open
                    CAN_Command2ARM[1] = (byte)'L';//Loop

                    if (invert_Motor == true)
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'R';//Right_direction
                    }
                    else
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'L';//Left_direction
                    }

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtDC.Text));//Duty cycle

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    timer_rotatepropeller.Enabled = true;
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btMotorStop_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                lbError_Overload.Visible = false;
                lbError_Sticked.Visible = false;
                Error_Overload_Motor = false;
                Error_Sticked_Motor = false;

                if (invert_Motor == true)
                {
                    btnReverse.Enabled = true;
                    btnForward.Enabled = false;
                }
                else
                {
                    btnForward.Enabled = true;
                    btnReverse.Enabled = false;
                }

                if (txtOpenLoop.ForeColor == Color.Red)
                {
                    txtOpenLoop.ForeColor = Color.White;
                    btnPID_Fuzzy.Enabled = true;
                    bt_des_Speed.Enabled = true;
                 
                    CAN_Command2ARM[0] = (byte)'O';//Open
                    CAN_Command2ARM[1] = (byte)'L';//Loop

                    if (invert_Motor == true)
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'R';//Right_direction
                    }
                    else
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'L';//Left_direction
                    }

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(0));//Duty cycle

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    timer_rotatepropeller.Enabled = false;
                    //--------------------------------------------------------------------------------------------------------------
                }
                else if (txtCloseLoop.ForeColor == Color.Red)
                {
                    txtCloseLoop.ForeColor = Color.White;
                    if (PID_Fuzzy == true)
                    {
                        txtCloseLoop.ForeColor = Color.White;
                        btnPID_Fuzzy.Enabled = true;
                        btSend.Enabled = true;
                        CAN_Command2ARM[0] = (byte)'C';//Close
                        CAN_Command2ARM[1] = (byte)'P';//Loop

                        if (invert_Motor == true)
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'R';//Right_direction
                        }
                        else
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'L';//Left_direction
                        }

                        //Value
                        ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(0));//Duty cycle

                        CAN_Command2ARM[3] = ByteArrays_Value[3];
                        CAN_Command2ARM[4] = ByteArrays_Value[2];
                        CAN_Command2ARM[5] = ByteArrays_Value[1];
                        CAN_Command2ARM[6] = ByteArrays_Value[0];
                        CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                        UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                        timer_rotatepropeller.Enabled = false;
                        //--------------------------------------------------------------------------------------------------------------
                    }
                    else if (PID_Fuzzy == false)
                    {
                        txtCloseLoop.ForeColor = Color.White;
                        btnPID_Fuzzy.Enabled = true;
                        btSend.Enabled = true;
                        CAN_Command2ARM[0] = (byte)'C';//Open
                        CAN_Command2ARM[1] = (byte)'F';//Loop

                        if (invert_Motor == true)
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'R';//Right_direction
                        }
                        else
                        {
                            //ID
                            CAN_Command2ARM[2] = (byte)'L';//Left_direction
                        }

                        //Value
                        ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(0));//Duty cycle

                        CAN_Command2ARM[3] = ByteArrays_Value[3];
                        CAN_Command2ARM[4] = ByteArrays_Value[2];
                        CAN_Command2ARM[5] = ByteArrays_Value[1];
                        CAN_Command2ARM[6] = ByteArrays_Value[0];
                        CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                        UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                        timer_rotatepropeller.Enabled = false;
                        //--------------------------------------------------------------------------------------------------------------
                    }
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void bt_KP_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtKP.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'K';

                    CAN_Command2ARM[2] = (byte)'P';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtKP.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);

                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void bt_KI_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtKI.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'K';

                    CAN_Command2ARM[2] = (byte)'I';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtKI.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void bt_KD_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtKD.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'K';

                    CAN_Command2ARM[2] = (byte)'D';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtKD.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void bt_des_Speed_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                btnForward.Enabled = false;
                btnReverse.Enabled = false;
                if (tb_des_Speed.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else if (PID_Fuzzy == true)
                {
                    txtOpenLoop.ForeColor = Color.White;
                    txtCloseLoop.ForeColor = Color.Red;
                    btnPID_Fuzzy.Enabled = false;
                    btSend.Enabled = false;

                    CAN_Command2ARM[0] = (byte)'C';//Close
                    CAN_Command2ARM[1] = (byte)'P';//Loop

                    if (invert_Motor == true)
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'R';//Right_direction
                    }
                    else
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'L';//Left_direction
                    }

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(tb_des_Speed.Text));//Duty cycle

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    timer_rotatepropeller.Enabled = true;
                    //--------------------------------------------------------------------------------------------------------------
                }
                else if (PID_Fuzzy == false)
                {
                    txtOpenLoop.ForeColor = Color.White;
                    txtCloseLoop.ForeColor = Color.Red;
                    btnPID_Fuzzy.Enabled = false;
                    btSend.Enabled = false;

                    CAN_Command2ARM[0] = (byte)'C';//Close
                    CAN_Command2ARM[1] = (byte)'F';//Loop

                    if (invert_Motor == true)
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'R';//Right_direction
                    }
                    else
                    {
                        //ID
                        CAN_Command2ARM[2] = (byte)'L';//Left_direction
                    }

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(tb_des_Speed.Text));//Duty cycle

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    timer_rotatepropeller.Enabled = true;
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                btnReverse.Enabled = false;
                btnForward.Enabled = false;
                btnForward.Image = Properties.Resources.Apps_Rotate_Right_icon;
                btnReverse.Image = Properties.Resources.Left_Disable;
                btnForward.Size = new Size(65, 65);
                btnForward.Location = new Point(383, 33);
                btnReverse.Size = new Size(40, 40);
                btnReverse.Location = new Point(333, 55);

                invert_Motor = true;
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnReverse_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                btnReverse.Enabled = false;
                btnForward.Enabled = false;
                btnForward.Image = Properties.Resources.Right_Disable;
                btnReverse.Image = Properties.Resources.Apps_Rotate_Left_icon_1;
                btnForward.Size = new Size(40, 40);
                btnForward.Location = new Point(405, 57);
                btnReverse.Size = new Size(65, 65);
                btnReverse.Location = new Point(324, 31);

                invert_Motor = false;
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnGE_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtGE.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'G';

                    CAN_Command2ARM[2] = (byte)'E';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtGE.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnGDE_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtGDE.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'D';

                    CAN_Command2ARM[2] = (byte)'E';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtGDE.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnGDU_Click(object sender, EventArgs e)
        {
            if (Connect2CANBUS)
            {
                if (txtGDU.Text == "")
                {
                    MessageBox.Show("Chưa có dữ liệu!", "Thông báo");
                }
                else
                {
                    CAN_Command2ARM[0] = (byte)'G';

                    CAN_Command2ARM[1] = (byte)'D';

                    CAN_Command2ARM[2] = (byte)'U';

                    //Value
                    ByteArrays_Value = BitConverter.GetBytes((float)Convert.ToDouble(txtGDU.Text));

                    CAN_Command2ARM[3] = ByteArrays_Value[3];
                    CAN_Command2ARM[4] = ByteArrays_Value[2];
                    CAN_Command2ARM[5] = ByteArrays_Value[1];
                    CAN_Command2ARM[6] = ByteArrays_Value[0];
                    CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);

                    UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                    //--------------------------------------------------------------------------------------------------------------
                }
            }
            else MessageBox.Show("COM chưa mở.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    
        #region Excel

        // Tham khảo :
        // http://diendan.congdongcviet.com/threads/t172462::ket-xuat-du-lieu-tu-window-form-sang-excel-thu-vien-npoi.cpp

        private void btnExport_Click(object sender, EventArgs e)
        {
            #region New_Version
            SaveFileDialog ExportFileDialog = new SaveFileDialog();
            ExportFileDialog.Title = "Export to MS Excel";
            ExportFileDialog.Filter = "Excel Workbook|*.xls";
            DialogResult ExportResult = ExportFileDialog.ShowDialog();

            //Người dùng bấm OK để export :
            if (ExportResult == DialogResult.OK)
            {
                string[] pathList = ExportFileDialog.FileName.Split('.');
                string path = pathList[0] + "_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "." + pathList[1];
                bool IsExportOK, IsDataNull;

                ExportDataToExcel(path, out IsExportOK, out IsDataNull);

                if (IsExportOK)
                {
                    pathExcel.Text = "Path: " + path;
                    MessageBox.Show("Xuất Excel thành công !!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);      
                    try
                    {
                        DialogResult OpenExcel = MessageBox.Show("Bạn muốn mở file Excel", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (OpenExcel == DialogResult.Yes)
                        {
                            FileInfo fileExcel = new FileInfo(path);
                            System.Diagnostics.Process.Start(path);
                        }                        
                    }   
                    catch
                    {
                        MessageBox.Show("Không tồn tại file Excel", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }        
                }
                else if (IsDataNull)
                {
                    MessageBox.Show("Không có dữ liệu để xuất !!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            #endregion
        }

        public void ExportDataToExcel(string path, out bool IsExportOK, out bool IsAllDataNull)
        {
            IsAllDataNull = (dataGridExcel.Rows.Count == 1);
            if (IsAllDataNull)
            {
                IsExportOK = false;
                IsAllDataNull = true;
                return;
            }

            // Khởi tạo và cài đặt file excel :
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();

            //Create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "";
            hssfworkbook.DocumentSummaryInformation = dsi;

            //Create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "";
            hssfworkbook.SummaryInformation = si;
            try
            {
                CreateNewSheet(ref hssfworkbook, "Brushless DC Motor", dataGridExcel);

                WriteToFile(path, hssfworkbook);

                IsExportOK = true;
            }
            catch
            {
                IsExportOK = false;
            }
        }

        private void CreateNewSheet(ref HSSFWorkbook hssfworkbook, string SheetName, DataGridView DataGridName)
        // Hàm tạo trang tính mới với dữ liệu lấy từ DataGridName.
        // Trang tính được tạo trả về cho đối số hssfworkbook.
        {
            // Tạo trang tính :
            ISheet sheet = hssfworkbook.CreateSheet(SheetName);
            sheet.DefaultColumnWidth = 20;

            //Các đối tượng trên trang tính :
            IRow row;
            ICell cell;

            // Viết hàng header trong datagrigview vào excel :
            row = sheet.CreateRow(0);
            for (int j = 0; j < DataGridName.Columns.Count; j++)
            {
                cell = row.CreateCell(j);
                sheet.GetRow(0).GetCell(j).SetCellValue(DataGridName.Columns[j].HeaderText.ToString());
            }

            //Ghi dữ liệu vào trang tính :
            for (int i = 0; i < DataGridName.Rows.Count; i++)
            {
                row = sheet.CreateRow(i + 1);
                for (int j = 0; j < DataGridName.Columns.Count; j++)
                {
                    cell = row.CreateCell(j);
                    if (DataGridName.Rows[i].Cells[j].Value != null)
                    {
                        try
                        {
                            sheet.GetRow(i + 1).GetCell(j).SetCellValue(Convert.ToDouble(DataGridName.Rows[i].Cells[j].Value));
                        }
                        catch
                        {
                            // Dữ liệu trong bảng tính có thể không là kiểu số :
                            sheet.GetRow(i + 1).GetCell(j).SetCellValue(DataGridName.Rows[i].Cells[j].Value.ToString());
                        }
                    }
                }
            }
        }

        private void WriteToFile(string path, HSSFWorkbook hssfworkbook)
        {
            //Write the stream data of workbook to the root directory
            FileStream file = new FileStream(path, FileMode.Create);
            hssfworkbook.Write(file);
            file.Close();
        }

        #endregion

        #region drawGraph

        public void drawMotorGraph(double setpoint1, double setpoint2, double setpoint3)
        {
            if (zedGraphControl.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve1 = zedGraphControl.GraphPane.CurveList[0] as LineItem;
            LineItem curve2 = zedGraphControl.GraphPane.CurveList[1] as LineItem;
            LineItem curve3 = zedGraphControl.GraphPane.CurveList[2] as LineItem;

            if (curve1 == null)
                return;
            if (curve2 == null)
                return;
            if (curve3 == null)
                return;

            IPointListEdit list1 = curve1.Points as IPointListEdit;
            IPointListEdit list2 = curve2.Points as IPointListEdit;
            IPointListEdit list3 = curve3.Points as IPointListEdit;

            if (list1 == null)
                return;
            if (list2 == null)
                return;
            if (list3 == null)
                return;

            double time = (Environment.TickCount - tickStart) / 1000.0;
            list1.Add(time, setpoint1);
            list2.Add(time, setpoint2);
            list3.Add(time, setpoint3);

            Scale xScale = zedGraphControl.GraphPane.XAxis.Scale;

            if (time > (xScale.Max - xScale.MajorStep))
            {
                if (btGraphMotor.ButtonText == "Compact")
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 30;
                }
                else if (btGraphMotor.ButtonText == "Scroll")
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = 0;
                }
            }

            zedGraphControl.AxisChange();

            zedGraphControl.Invalidate();
        }

        public void drawTorqueGraph(double setpoint4, double setpoint5, double setpoint6)
        {
            if (TorqueGraph.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve4 = TorqueGraph.GraphPane.CurveList[0] as LineItem;
            LineItem curve5 = TorqueGraph.GraphPane.CurveList[1] as LineItem;
            LineItem curve6 = TorqueGraph.GraphPane.CurveList[2] as LineItem;

            if (curve4 == null)
                return;
            if (curve5 == null)
                return;
            if (curve6 == null)
                return;

            IPointListEdit list4 = curve4.Points as IPointListEdit;
            IPointListEdit list5 = curve5.Points as IPointListEdit;
            IPointListEdit list6 = curve6.Points as IPointListEdit;

            if (list4 == null)
                return;
            if (list5 == null)
                return;
            if (list6 == null)
                return;

            double time = (Environment.TickCount - tickStart) / 1000.0;

            list4.Add(time, setpoint4);
            list5.Add(time, setpoint5);
            list6.Add(time, setpoint6);

            Scale xScale = TorqueGraph.GraphPane.XAxis.Scale;

            if (time > (xScale.Max - xScale.MajorStep))
            {
                if (btGraphTorqueThrust.ButtonText == "Compact")
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 30;
                }
                else if (btGraphTorqueThrust.ButtonText == "Scroll")
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = 0;
                }
            }

            TorqueGraph.AxisChange();

            TorqueGraph.Invalidate();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if ((Connect2CANBUS == true) || (serialCOMThrust.IsOpen == true))
            {
                CheckMotorController.Enabled = false;
                CheckTorqueMeasurement.Enabled = false;
                timer_draw.Start();

                if (tickStart == 0)
                {
                    tickStart = Environment.TickCount;
                }
                Excel_flag = true;
                
            }
            else
            {
                MessageBox.Show("No connect to COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            CheckMotorController.Enabled = true;
            CheckTorqueMeasurement.Enabled = true;
            timer_draw.Stop();
            Excel_flag = false;
            indexRow = 0;            
        }
        
        private byte CRC_8(byte[] _data, int _start, int _end)
        {
            byte sum = 0;
            for(int i = _start; i < _start + _end;  i++)
            {
                sum += _data[i];
            }
            return sum;
        }
        #endregion

        private void timer_updatedata_Tick(object sender, EventArgs e)
        {
            if (Connect2CANBUS == true)
            {
                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'R';
                CAN_Command2ARM[4] = (byte)'P';
                CAN_Command2ARM[5] = (byte)'V';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                Thread.Sleep(1);

                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'A';
                CAN_Command2ARM[4] = (byte)'O';
                CAN_Command2ARM[5] = (byte)'C';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                Thread.Sleep(1);

                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'A';
                CAN_Command2ARM[4] = (byte)'T';
                CAN_Command2ARM[5] = (byte)'K';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                Thread.Sleep(1);

                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'A';
                CAN_Command2ARM[4] = (byte)'I';
                CAN_Command2ARM[5] = (byte)'M';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                Thread.Sleep(1);

                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'T';
                CAN_Command2ARM[4] = (byte)'D';
                CAN_Command2ARM[5] = (byte)'C';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
                Thread.Sleep(1);

                CAN_Command2ARM[0] = (byte)'R';
                CAN_Command2ARM[1] = (byte)'E';
                CAN_Command2ARM[2] = (byte)'Q';
                CAN_Command2ARM[3] = (byte)'R';
                CAN_Command2ARM[4] = (byte)'S';
                CAN_Command2ARM[5] = (byte)'P';
                CAN_Command2ARM[6] = 0x0A;
                CAN_Command2ARM[7] = Checksum(CAN_Command2ARM, 0, 7);
                UCAN_Transmit_Data(_IDCANBUS_MASS_SHIFTER, _DATA_TRANSMIT_LENGTH, CAN_Command2ARM);
            }
        }

        unsafe private void bgrReceiveData_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                if(bgrReceiveData.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    UInt32 res = new UInt32();
                    res = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);
                    if (res == 0)
                        return;
                    else
                    {
                        //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0],50, 100);

                        /////////////////////////////////////
                        //UInt32 con_maxlen = 50;
                        IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);

                        res = VCI_Receive(m_devtype, m_devind, m_canind, pt, res, 100);
                        ////////////////////////////////////////////////////////

                        for (UInt32 i = 0; i < res; i++)
                        {
                            VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));

                            if ((obj.ID == _IDCANBUS_ARM_1) && (obj.RemoteFlag == 0) && (obj.ExternFlag == 0) && (obj.Data[7] == Checksum_UsingPointer(obj.Data, 0, 7)))
                            {
                                if ((obj.Data[0] == 'R') && (obj.Data[1] == 'P') && (obj.Data[2] == 'V'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    DataDoneCheckSum_ARM2PC.r_Speed = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(0, data_FloatingPoint);
                                }

                                if ((obj.Data[0] == 'A') && (obj.Data[1] == 'O') && (obj.Data[2] == 'C'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    Data_Jadever_3100.Temp_On_Chip = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(1, data_FloatingPoint);
                                }

                                if ((obj.Data[0] == 'A') && (obj.Data[1] == 'T') && (obj.Data[2] == 'K'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    Data_Jadever_3100.Temp_Amnient = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(2, data_FloatingPoint);
                                }

                                if ((obj.Data[0] == 'A') && (obj.Data[1] == 'I') && (obj.Data[2] == 'M'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    DataDoneCheckSum_ARM2PC.Current = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(3, data_FloatingPoint);
                                }

                                if ((obj.Data[0] == 'T') && (obj.Data[1] == 'D') && (obj.Data[2] == 'C'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    DataDoneCheckSum_ARM2PC.Percentage_DC = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(4, data_FloatingPoint);
                                }

                                if ((obj.Data[0] == 'R') && (obj.Data[1] == 'S') && (obj.Data[2] == 'P'))
                                {
                                    byte[] bytes_FloatingPoint = new byte[4];
                                    bytes_FloatingPoint[0] = obj.Data[3];
                                    bytes_FloatingPoint[1] = obj.Data[4];
                                    bytes_FloatingPoint[2] = obj.Data[5];
                                    bytes_FloatingPoint[3] = obj.Data[6];
                                    float data_FloatingPoint = BitConverter.ToSingle(bytes_FloatingPoint, 0);
                                    DataDoneCheckSum_ARM2PC.Des_Speed = data_FloatingPoint;
                                    bgrReceiveData.ReportProgress(5, data_FloatingPoint);
                                }

                                //-------ERROR------//
                                if((obj.Data[0] == 'E') && (obj.Data[1] == 'R') && (obj.Data[2] == 'R'))
                                {
                                    if ((obj.Data[3] == 'I') && (obj.Data[4] == 'M') && (obj.Data[5] == 'O') && (obj.Data[6] == 'T'))
                                    {
                                        Error_Overload_Motor = true;
                                    }

                                    if ((obj.Data[3] == 'S') && (obj.Data[4] == 'T') && (obj.Data[5] == 'I') && (obj.Data[6] == 'C'))
                                    {
                                        Error_Sticked_Motor = true;
                                    }
                                }
                            }
                        }

                        Marshal.FreeHGlobal(pt);
                    }
                }                       
            }
        }

        private void bgrReceiveData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            float data_FloatingPoint = (float)e.UserState;
            if (e.ProgressPercentage == 0)
            {                
                SetText_r_Speed(data_FloatingPoint.ToString("0.0000"));
            }
            else if (e.ProgressPercentage == 1)
            {
                SetText_Temp_On_Chip(data_FloatingPoint.ToString("0.0000"));
                LinearGaugeTempMotor.Value = data_FloatingPoint;
            }
            else if (e.ProgressPercentage == 2)
            {
                SetText_Temp_Thermocouple(data_FloatingPoint.ToString("0.0000"));
                //LinearGaugeTempMotor.Value = data_FloatingPoint;
            }
            else if (e.ProgressPercentage == 3)
            {
                SetText_iMotor(data_FloatingPoint.ToString("0.0000"));
            }
            else if (e.ProgressPercentage == 4)
            {
                SetText_Percent_DC(data_FloatingPoint.ToString("0.0000"));
            }
            else if (e.ProgressPercentage == 5)
            {
                SetText_Des_Speed(data_FloatingPoint.ToString("0.0000"));
            }
        }

        #region Rotate Image
        //https://www.codeproject.com/Articles/58815/C-Image-PictureBox-Rotations

        /// <summary>
        /// Creates a new Image containing the same image only rotated
        /// </summary>
        /// <param name=""image"">The <see cref=""System.Drawing.Image"/"> to rotate
        /// <param name=""offset"">The position to rotate from.
        /// <param name=""angle"">The amount to rotate the image, clockwise, in degrees
        /// <returns>A new <see cref=""System.Drawing.Bitmap"/"> of the same size rotated.</see>
        /// <exception cref=""System.ArgumentNullException"">Thrown if <see cref=""image"/"> 
        /// is null.</see>
        public static Bitmap RotateImage_withoffset(Image image, PointF offset, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }

        /// <summary>
        /// Creates a new Image containing the same image only rotated
        /// </summary>
        /// <param name="image">The <see cref="System.Drawing.Image"/> to rotate</param>
        /// <param name="angle">The amount to rotate the image, clockwise, in degrees</param>
        /// <returns>A new <see cref="System.Drawing.Bitmap"/> of the same size rotated.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="image"/> is null.</exception>
        public static Bitmap RotateImage(Image image, float angle)
        {
            return RotateImage_withoffset(image, new PointF((float)image.Width / 2, (float)image.Height / 2), angle);
        }

        Int16 _angle = 0;
        UInt16 _state_error_cnt = 0;

        private void timer_rotatepropeller_Tick(object sender, EventArgs e)
        {
            if(DataDoneCheckSum_ARM2PC.r_Speed != 0.0f)
            {               
                if (invert_Motor)
                {
                    _angle += 36;
                    ImgPropeller.Image = RotateImage(img, _angle);
                    if (_angle >= 360) _angle = 0;
                }
                else
                {
                    _angle -= 36;
                    ImgPropeller.Image = RotateImage(img, _angle);
                    if (_angle <= -360) _angle = 0;
                }
            }

            if(Error_Overload_Motor)
            {
                if((_state_error_cnt%2) == 1) lbError_Overload.Visible = true;
                else lbError_Overload.Visible = false;
            }

            if(Error_Sticked_Motor)
            {
                if ((_state_error_cnt % 2) == 1) lbError_Sticked.Visible = true;
                else lbError_Sticked.Visible = false;
            }

            if (_state_error_cnt >= 1) _state_error_cnt = 0;
            else _state_error_cnt++;
        }
        #endregion
    }
}
