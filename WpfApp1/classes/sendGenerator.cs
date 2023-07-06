using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class sendGenerator
    {

        private static readonly byte FL0D = 0x0D;
        private static readonly byte FL0A = 0x0A;
        private static readonly byte pa30_size = 13;
        private static readonly byte header_size = 5;
        private byte[] rs485_buf_out_ = new byte[16];
        public byte P_time_out;

        private SerialPort portW = null;
        private SerialPort portR = null;

        public struct Pa30_data
        {
            public int del_g;
            public int del_v;
            public byte InRK;
            public ushort dTime;

            public Pa30_data (int del_g, int del_v, byte InRK, ushort dTime)
            {
                this.del_g = del_g;
                this.del_v = del_v;
                this.InRK = InRK;
                this.dTime = dTime;
            }
        }

        public struct InpData
        {
            public int angle_a;
            public int angle_n;
            public bool low_ender;
            public bool high_ender;
            public bool left_ender;
            public bool right_ender;
            public bool az_ready;
            public bool inc_ready;
            public bool correct;

            public InpData (int angle_a, int angle_n, bool low_ender, bool high_ender, bool left_ender, bool right_ender, bool az_ready, bool inc_ready)
            {
                this.angle_a = angle_a;
                this.angle_n = angle_n;
                this.low_ender = low_ender;
                this.high_ender = high_ender;
                this.left_ender = left_ender;
                this.right_ender = right_ender;
                this.az_ready = az_ready;
                this.inc_ready = inc_ready;
                correct = false;
            }
        }

        public void setPortW(SerialPort portW, SerialPort portR)
        {
            this.portW = portW;
            this.portR = portR;
        }

        public void pa30_pack(Pa30_data pa30_data)
        {
            ushort i = 0, j = 0;

            byte[] rs485_buf_out = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            rs485_buf_out[0] = FL0D;
            rs485_buf_out[1] = FL0A;
            rs485_buf_out[2] = 0x02;                            // address of mup
            rs485_buf_out[3] = 0x1E;                            // cmd
            rs485_buf_out[4] = (byte)(pa30_size - (header_size + 1));   // amount of bytes of data

            //	pa30_data.del_g = 8;
            //	pa30_data.del_v = 8;
            rs485_buf_out[5] = pa30_data.del_g < 0 ? (byte)0x80 : (byte)0x00;
            rs485_buf_out[5] += (byte)(((ushort)(Math.Abs(pa30_data.del_g)) & 0xff00) >> 8);
            rs485_buf_out[6] = (byte)((ushort)(Math.Abs(pa30_data.del_g)) & 0x00ff);
            rs485_buf_out[7] = (byte)(((ushort)(Math.Abs(pa30_data.del_v)) & 0xff00) >> 8);
            rs485_buf_out[8] = (byte)((ushort)(Math.Abs(pa30_data.del_v)) & 0x00ff);
            rs485_buf_out[9] = pa30_data.InRK; // Code 11 -> CMR dTime 1 ms else 10 mks
            rs485_buf_out[10] = (byte)((pa30_data.dTime & 0xff00) >> 8);
            rs485_buf_out[11] = (byte)(pa30_data.dTime & 0x00ff);

            //	printf("> PA30: del_g: %d del_v: %d\n", pa30_data.del_g, pa30_data.del_v);
            //	for(i = 5; i < 9; i++)
            //		printf("> PA30: rs485_buf_out[%d]: %d\n", i-4, rs485_buf_out[i]);

            // add FL0A if needed
            for (i = header_size, j = header_size; i < pa30_size; i++, j++)
            {
                rs485_buf_out_[j] = rs485_buf_out[i];
                if (rs485_buf_out[i] == FL0A && rs485_buf_out[i + 1] != FL0A)
                {
                    rs485_buf_out_[j + 1] = FL0A;
                    j++;
                }
            }

            // set parity byte in message
            rs485_buf_out_[j - 1] = 0;
            for (i = 0; i < j - 1; i++)
                rs485_buf_out_[j - 1] ^= rs485_buf_out_[i]; //parity byte;

            if (rs485_buf_out_[j - 1] == FL0A)
            {
                rs485_buf_out_[j] = 0;
                j += 1;
            }

            // Для проверки увеличенного time out в 100 раз
            if (P_time_out != 0)
            {
                rs485_buf_out_[j - 1] ^= 111;
                //		printf("pa30_data.InRK=%d pa30_data.dTime=%d\n", pa30_data.InRK, pa30_data.dTime);
            }

            //  add header
            for (i = 0; i < header_size; i++)
                rs485_buf_out_[i] = rs485_buf_out[i];


            try { portW.Write(rs485_buf_out_, 0, j); }
            catch {  };
        } // end of pa30_pack()

        public void askToRead()
        {
            rs485_buf_out_[0] = FL0D;
            rs485_buf_out_[1] = FL0A;
            rs485_buf_out_[2] = 0x02;                       // address of mup
            rs485_buf_out_[3] = 0x20;                       // cmd
            rs485_buf_out_[4] = 0x0; // for MUP NUMD = 0	// amount of bytes of data
            rs485_buf_out_[5] = 0x25;                       // parity byte;
            try { portR.Write(rs485_buf_out_, 0, 6); }
            catch { };
        }

        public InpData readData(byte[] data)
        {
            InpData exData = new InpData();
            if ((data[0] == 0x0D) && (data[1] == 0x0A) && (data[2] == 0x02) && (data[3] == 0x20))
            {
                int az = (int)(data[5] & 0x7f) << 8;
                az += data[6];
                if ((data[5] & 0x80) != 0) az *= (-1);
                az *= 40;
                int inc = (int)(data[7] & 0x7f) << 8;
                inc += data[8];
                inc *= 40;
                if ((data[7] & 0x80) != 0) inc *= (-1);
                InpData rData = new InpData(
                    az,
                    inc, 
                    (data[9]&0x01) == 1,
                    (data[9]&0x02) == 1,
                    (data[9]&0x04) == 1,
                    (data[9]&0x08) == 1,
                    (data[9]&0x10) == 1,
                    (data[9]&0x20) == 1);
                rData.correct = true;
                exData = rData;
            }
            return exData;
        }
    }
}
