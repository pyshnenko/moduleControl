using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfApp1.classes;

namespace WpfApp1
{
    public class antennaState
    {
        private bool connectState = false;
        private int azAngle = 324123;
        private int incAngle = 0;
        private bool azMeasUni = false;
        private bool incMeasUni = false;
        private bool azMeasDec = false;
        private bool incMeasDec = false;
        private bool manualState = false;
        private int manualAzAngle = 0;
        private int manualIncAngle = 0;
        private int ustAz = 0;
        private int ustInc = 0;
        private CheckedParameters.NowWork workMode;
        private int needAzAngle = 0;
        private int needIncAngle = 0;
        private long lastAzTime = 0;
        private long lastIncTime = 0;
        private CheckedParameters pars;
        private SerialPort readPort = null;
        TextBox text = null;
        public StartVoltage startVoltageObj = null;
        public StartVoltageSector startVoltageObjSector = null;

        public antennaState() { }
        public antennaState(string azMeas, string incMeas)
        {
            azMeasDec = false; incMeasDec = false; azMeasUni = false; incMeasUni = false;
            if (azMeas == "Код") azMeasDec = true;
            if (incMeas == "Код") incMeasDec = true;
            if (azMeas == "Значение") azMeasUni = true;
            if (incMeas == "Значение") incMeasUni = true;
        }
        public void setStartVoltage(StartVoltage val) { this.startVoltageObj = val; }
        public void setStartVoltageSector(StartVoltageSector val) { this.startVoltageObjSector = val; }
        public void SetAntennaAtPosition(int az, int inc)
        {
            int deltaAz = -(getAzAngle() - az);
            int deltaInc = -(getIncAngle() - inc);
            deltaAz = Math.Abs(deltaAz) > 1024 ? deltaAz < 0 ? -1024 : 1024 : deltaAz;
            deltaInc = Math.Abs(deltaInc) > 1024 ? deltaInc < 0 ? -1024 : 1024 : deltaInc;
            setUstAz(deltaAz);
            setUstInc(deltaInc);
        }
        public void SetCheckedParameters(CheckedParameters pars) { this.pars = pars; }
        public CheckedParameters GetCheckedParameters() { return pars; }
        public CheckedParameters.NowWork GoToNextParameters()
        {
            workMode = pars.SetNextWork(workMode);
            setWorkMode(workMode);
            return workMode;
        }
        public void setLastAzTime(long val) { lastAzTime = val; }
        public void setLastIncTime(long val) { lastIncTime = val; }
        public long getLastAzTime() { return lastAzTime; }
        public long getLastIncTime() { return lastIncTime; }
        public void setNeedAzAngle(int val) { needAzAngle = val; }
        public void setNeedIncAngle(int val) { needIncAngle = val; }
        public int getNeedAzAngle() { return needAzAngle; }
        public int getNeedIncAngle() { return needIncAngle; }
        public void setWorkMode(CheckedParameters.NowWork workMode) { this.workMode = workMode; }
        public CheckedParameters.NowWork getWorkMode() { return workMode; }
        public void setManualAzAngle(int val) { manualAzAngle = val; }
        public void setManualIncAngle(int val) { manualIncAngle = val; }
        public int getManualAzAngle() {  return manualAzAngle; }
        public int getManualIncAngle() { return manualIncAngle; }
        public void setUstAz(int val) { ustAz = val;}
        public void setUstInc(int val) { ustInc = val; }
        public int getUstAz() { return ustAz; }
        public int getUstInc() { return ustInc; }
        public void setTextBox(TextBox text) { this.text = text; }
        public TextBox getTextBox() { return text; }
        public void setReadPort(SerialPort port) { readPort = port; }
        public void sendMessage(string mess) { readPort.Write(mess); }
        public SerialPort getPort() { return readPort; }
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
            int bufAngle = Math.Abs(angle);
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
            int extMin = (int)((Math.Abs(angle))%3600 / 60);
            int extSeck = (int)((Math.Abs(angle)) % 60);
            return extGrad + "º" + extMin + "'" + extSeck + "\"";
        }
    }
}
