using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.classes
{
    public class StartVoltageSector
    {
        private CheckedParameters.NowWork work;
        private antennaState state;
        private int creetAzP, creetAzN, creetIncP, creetIncN;

        public int minSpeedAzNN = 0;
        public int minSpeedAzNP = 0;
        public int minSpeedAzZN = 0;
        public int minSpeedAzZP = 0;
        public int minSpeedAzPP = 0;
        public int minSpeedAzPN = 0;
        public int minSpeedInc = 0;

        public int nowSpeed = 0;

        public bool start = false;
        private bool goToPluse = false;
        private bool goToStart = false;
        private int lastAzPosition = 0;
        private int lastIncPosition = 0;
        private int speedK;

        private bool end = false;
        public bool ready = false;

        public StartVoltageSector(antennaState state, int creetAzP, int creetAzN, int creetIncP, int creetIncN, int speedK)
        {
            this.state = state;
            this.creetAzN = creetAzN * 3600;
            this.creetAzP = creetAzP * 3600;
            this.creetIncP = creetIncP * 3600;
            this.creetIncN = creetIncN * 3600;
            this.speedK = speedK;
        }

        public void StartCheck()
        {
            start = true;
            int inc = state.getIncAngle();
            int az = state.getAzAngle();
            if ((Math.Abs(inc) > 100) || (Math.Abs(az) > 100))
            {
                if (Math.Abs(inc) > 100)
                {
                    int ust = 0 - inc / speedK;
                    if (ust > 1024) ust = 1024;
                    else if (ust < -1024) ust = -1024;
                    state.setUstInc(ust);
                }
                if (Math.Abs(az) > 100)
                {
                    int ust = 0 - az / speedK;
                    if (ust > 1024) ust = 1024;
                    else if (ust < -1024) ust = -1024;
                    state.setUstAz(ust);
                }
            }
            else
            {
                ready = true;
                goToStart = true;
            }
        }

        public bool Work()
        {
            work = state.getWorkMode();
            int az = state.getAzAngle();
            int inc = state.getIncAngle();
            if (goToStart)
            {
                if (work.azimuth)
                {
                    if (end)
                    {
                        return true;
                    }
                    if ((az > (creetAzN + 3 * 3600)) || (Math.Abs(inc) > 100))
                    {
                        int ust = 0 - inc / speedK;
                        if (ust > 1024) ust = 1024;
                        else if (ust < -1024) ust = -1024;
                        state.setUstInc(ust);
                        ust = (creetAzN - az) / speedK;
                        if (ust > 1024) ust = 1024;
                        else if (ust < -1024) ust = -1024;
                        state.setUstAz(ust);
                    }
                    else
                    {
                        goToStart = false; goToPluse = true;
                        lastAzPosition = az;
                        lastIncPosition = inc;
                    }
                }
                else
                {
                    if ((inc > (creetIncN + 1 * 3600)) || (Math.Abs(az) > 100))
                    {
                        int ust = 0 - az / speedK;
                        if (ust > 1024) ust = 1024;
                        else if (ust < -1024) ust = -1024;
                        state.setUstAz(ust);
                        ust = (creetIncN - inc) / speedK;
                        if (ust > 1024) ust = 1024;
                        else if (ust < -1024) ust = -1024;
                        state.setUstInc(ust);
                    }
                    else
                    {
                        goToStart = false; goToPluse = true;
                        lastAzPosition = az;
                        lastIncPosition = inc;
                    }
                }
            }
            else
            {
                int ust = 0 - (work.azimuth ? inc : az) / speedK;
                if (ust > 1024) ust = 1024;
                else if (ust < -1024) ust = -1024;
                if (work.azimuth) state.setUstInc(ust);
                else state.setUstAz(ust);
                if ((goToPluse) && (work.azimuth))
                {
                    if (az < (-45 - 3) * 3600)  //сектор -90 - -45
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzNP += 10;
                            nowSpeed = minSpeedAzNP;
                        }
                        state.setUstAz(minSpeedAzNP);
                    }
                    else if (az < (45 - 3) * 3600) //сектор -45 - 45
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzZP += 10;
                            nowSpeed = minSpeedAzZP;
                        }
                        state.setUstAz(minSpeedAzZP);
                    }
                    else if (az < (90 - 3) * 3600) //сектор 45 - 90
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzPP += 10;
                            nowSpeed = minSpeedAzPP;
                        }
                        state.setUstAz(minSpeedAzPP);
                    }
                    else
                    {
                        goToPluse = false;
                    }
                }
                else if ((!goToPluse) && (work.azimuth))
                {
                    if (az > (45 + 3) * 3600) //сектор 90 - 45
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzPN += 10;
                            nowSpeed = -minSpeedAzPN;
                        }
                        state.setUstAz(-minSpeedAzPN);
                    }
                    else if (az > (-45 + 3) * 3600) //сектор 45 - -45
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzZN += 10;
                            nowSpeed = -minSpeedAzZN;
                        }
                        state.setUstAz(-minSpeedAzZN);
                    }
                    else if (az > (-90 + 3) * 3600) //сектор -45 - -90
                    {
                        if (Math.Abs(lastAzPosition - az) < 10)
                        {
                            minSpeedAzNN += 10;
                            nowSpeed = -minSpeedAzNN;
                        }
                        state.setUstAz(-minSpeedAzNN);
                    }
                    else
                    {
                        work = state.GoToNextParameters();
                        goToStart = true;
                        goToPluse = true;
                    }
                }
                else if ((goToPluse) && (!work.azimuth))
                {
                    if (inc < creetIncP - 1 * 3600)
                    {
                        if (Math.Abs(lastIncPosition - inc) < 10)
                        {
                            minSpeedInc += 10;
                        }
                        state.setUstInc(minSpeedInc);
                    }
                    else
                    {
                        goToPluse = false;
                    }
                }
                else if ((!goToPluse) && (!work.azimuth))
                {
                    if (inc > creetIncN + 1 * 3600)
                    {
                        if (Math.Abs(lastIncPosition - inc) < 10)
                        {
                            minSpeedInc += 10;
                        }
                        state.setUstInc(-minSpeedInc);
                    }
                    else
                    {
                        //work = state.GoToNextParameters();
                        goToStart = true;
                        end = true;
                        return true;
                    }
                }
            }
            lastAzPosition = az;
            lastIncPosition = inc;
            return false;
        }
    }
}
