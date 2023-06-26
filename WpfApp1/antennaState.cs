using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class antennaState
    {
        private bool connectState = false;
        private int azAngle = 324123;
        private int incAngle = 0;
        private bool azMeasUni = false;
        private bool incMeasUni = false;
        private bool azMeasDec = false;
        private bool incMeasDec = false;
        private bool manualState = false;
        private SerialPort readPort = null;

        public antennaState() { }
        public antennaState(string azMeas, string incMeas)
        {
            azMeasDec = false; incMeasDec = false; azMeasUni = false; incMeasUni = false;
            if (azMeas == "Код") azMeasDec = true;
            if (incMeas == "Код") incMeasDec = true;
            if (azMeas == "Значение") azMeasUni = true;
            if (incMeas == "Значение") incMeasUni = true;
        }

        public void setReadPort(SerialPort port)
        {
            readPort = port;
        }

        public void sendMessage(string mess)
        {
            readPort.Write(mess);
        }

        public SerialPort getPort()
        {
            return readPort;
        }

        public bool setManualState(bool state)
        {
            manualState = state;
            return manualState;
        }

        public bool getManualState()
        {
            return manualState;
        }
        public void setAzMeas (string meas)
        {
            if (meas == "Градус")
            {
                azMeasDec = false;
                azMeasUni = false;
            }
            else  if (meas == "Код")
            {
                azMeasDec = false;
                azMeasUni = true;
            }
            else
            {
                azMeasDec = true;
                azMeasUni = false;
            }
        }
        public void setIncMeas(string meas)
        {
            if (meas == "Градус")
            {
                incMeasDec = false;
                incMeasUni = false;
            }
            else if (meas == "Код")
            {
                incMeasDec = false;
                incMeasUni = true;
            }
            else
            {
                incMeasDec = true;
                incMeasUni = false;
            }
        }

        public void setAzAngle (string angle, int angleInt)
        {
            if (angle == "")
                azAngle = angleInt;
            else
            {
                int buf = 0;
                for (int i = angle.Length - 1; i >= 0; i--)
                {
                    buf += (int)angle[i] * 2 ^ (angle.Length - 1 - i);
                }
                azAngle = buf;
            }
        }

        public int getAzAngle ()
        {
            return azAngle;
        }

        public string getAzAngleText ()
        {
            return azMeasDec ? azAngle.ToString() : azMeasUni ? intToUni(azAngle) : intToAng(azAngle);
        }

        public void setIncAngle(string angle, int angleInt)
        {
            if (angle == "")
                incAngle = angleInt;
            else
            {
                int buf = 0;
                for (int i=angle.Length -1; i >= 0; i--)
                {
                    buf += (int)angle[i] * 2 ^ (angle.Length - 1 -i);
                }
                incAngle = buf;
            }
        }

        public int getIncAngle ()
        {
            return incAngle;
        }

        public string getIncAngleText()
        {
            return incMeasDec ? incAngle.ToString() : incMeasUni ? intToUni(incAngle) : intToAng(incAngle);
        }

        public bool connsectionGet()
        {
            return connectState;
        }

        public bool connectionSet(bool state)
        {
            connectState = state;
            return connectState;
        }

        private string intToUni(int angle)
        {
            string buf = "";
            StringBuilder bBuf = new StringBuilder(buf);
            int i =  0;
            int bufAngle = angle;
            while (bufAngle > 1)
            {
                bBuf.Insert(0, (bufAngle % 2).ToString());
                i++;
                bufAngle /= 2;
            }
            bBuf.Insert(0, bufAngle.ToString());
            buf = bBuf.ToString();
            return buf;
        }

        private string intToAng (int angle)
        {
            int extGrad = (int)(angle/3600);
            int extMin = (int)(angle%3600 / 60);
            int extSeck = (int)(angle % 60);
            return extGrad + "º" + extMin + "'" + extSeck + "\"";
        }
    }
}
