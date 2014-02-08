using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VJoyDemo
{
    class VJoy
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct JoystickState
        {
            public byte ReportId;
            public byte Slider;
            public byte XAxis;
            public byte YAxis;
            public byte ZAxis;
            public byte XRotation;
            public byte YRotation;
            public byte ZRotation;
            public byte POV;
            public uint Buttons;
        };

        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VJoy_Initialize(StringBuilder name, StringBuilder serial);

        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void VJoy_Shutdown();

        [DllImport("VJoy.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VJoy_UpdateJoyState(int id, ref JoystickState joyState);

        private JoystickState[] m_joyState;

        public VJoy()
        {
        }

        public bool Initialize()
        {
            m_joyState = new JoystickState[2];

            m_joyState[0] = new JoystickState();
            m_joyState[1] = new JoystickState();

            StringBuilder Name = new StringBuilder("RoboJoy");
            StringBuilder Serial = new StringBuilder("");

            return VJoy_Initialize(Name, Serial);
            //VJoy_Initialize(Name, Serial);
        }

        public void Shutdown()
        {
            VJoy_Shutdown();
        }

        public bool Update(int id)
        {
            return VJoy_UpdateJoyState(id, ref m_joyState[id]);
        }

        public void Reset()
        {
            m_joyState[0].ReportId = 0;
            m_joyState[0].Slider = 0;
            m_joyState[0].XAxis = 0;
            m_joyState[0].YAxis = 0;
            m_joyState[0].ZAxis = 0;
            m_joyState[0].XRotation = 0;
            m_joyState[0].YRotation = 0;
            m_joyState[0].ZRotation = 0;
            m_joyState[0].POV = 0;
            m_joyState[0].Buttons = 0;

            m_joyState[1].ReportId = 0;
            m_joyState[1].Slider = 0;
            m_joyState[1].XAxis = 0;
            m_joyState[1].YAxis = 0;
            m_joyState[1].ZAxis = 0;
            m_joyState[1].XRotation = 0;
            m_joyState[1].YRotation = 0;
            m_joyState[1].ZRotation = 0;
            m_joyState[1].POV = 0;
            m_joyState[1].Buttons = 0;
        }

        public ushort GetXAxis(int index)
        {
            return m_joyState[index].XAxis;
        }

        public void SetXAxis(int index, byte value)
        {
            m_joyState[index].XAxis = value;
        }

        public byte GetYAxis(int index)
        {
            return m_joyState[index].YAxis;
        }

        public void SetYAxis(int index, byte value)
        {
            m_joyState[index].YAxis = value;
        }

        public byte GetZAxis(int index)
        {
            return m_joyState[index].ZAxis;
        }

        public void SetZAxis(int index, byte value)
        {
            m_joyState[index].ZAxis = value;
        }

        public byte GetXRotation(int index)
        {
            return m_joyState[index].XRotation;
        }

        public void SetXRotation(int index, byte value)
        {
            m_joyState[index].XRotation = value;
        }

        public byte GetYRotation(int index)
        {
            return m_joyState[index].YRotation;
        }

        public void SetYRotation(int index, byte value)
        {
            m_joyState[index].YRotation = value;
        }

        public byte GetZRotation(int index)
        {
            return m_joyState[index].ZRotation;
        }

        public void SetZRotation(int index, byte value)
        {
            m_joyState[index].ZRotation = value;
        }

        public byte GetSlider(int index)
        {
            return m_joyState[index].Slider;
        }

        public void SetSlider(int index, byte value)
        {
            m_joyState[index].Slider = value;
        }

        public void SetPOV(int index, byte button, bool value)
        {
            if (value)
            {
                if (button == 0)
                { m_joyState[index].POV = (byte)(16); } //0,1,2,3
                if (button == 1)
                { m_joyState[index].POV = (byte)(17); }
                if (button == 2)
                { m_joyState[index].POV = (byte)(18); }
                if (button == 3)
                { m_joyState[index].POV = (byte)(19); }
                if (button == 4)
                { m_joyState[index].POV = (byte)(20); }
                if (button == 5)
                { m_joyState[index].POV = (byte)(21); }
                if (button == 6)
                { m_joyState[index].POV = (byte)(22); }
                if (button == 7)
                { m_joyState[index].POV = (byte)(23); }
                
            }
            //m_joyState[index].POV  
            //m_joyState[index].POV |= (byte)(1 << button);
            //else
            //{
            //    if (button == 0)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 1)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 2)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 3)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 4)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 5)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 6)
            //    { m_joyState[index].POV = (byte)(0); }
            //    if (button == 7)
            //    { m_joyState[index].POV = (byte)(0); }
            //    //m_joyState[index].POV &= (byte)~(1 << button);
            //}
        }
        public bool GetPOV(int index, byte button)
        {
            return ((m_joyState[index].POV & (1 << button)) == 1);
        }

        public void SetButton(int index, int button, bool value)
        {
            if(value)
                m_joyState[index].Buttons |= (uint)(1 << button);
            else
                m_joyState[index].Buttons &= (uint)~(1 << button);
        }

        public bool GetButton(int index, int button)
        {
            return ((m_joyState[index].Buttons & (1 << button)) == 1);
        }
    }
}
