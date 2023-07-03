using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class CheckedParameters
    {
        private static readonly string[] workMode = new string[6] { "naprTrogan", "kPered", "maxSpeed", "minLength", "progon", "zero" };
        private static readonly bool[] mode = new bool[6];
        public readonly bool az;
        public readonly bool inc;

        public struct NowWork 
        {
            public string name;
            public bool azimuth;

            public NowWork(string name, bool azimuth) { this.name = name; this.azimuth = azimuth; }
        }

        public CheckedParameters(bool naprTrogan, bool kPered, bool maxSpeed, bool minLength, bool progon, bool az, bool inc)
        {
            mode[0] = naprTrogan;
            mode[1] = kPered;
            mode[2] = maxSpeed;
            mode[3] = minLength;
            mode[4] = progon;
            mode[5] = true;
            this.az = az;
            this.inc = inc;
        }

        public NowWork SetNextWork(NowWork working)
        {
            NowWork exData = new NowWork("zero", true);
            if (az || inc)
            {
                if (az && inc)
                {
                    if (working.azimuth) { working.azimuth = false; return working; }
                    else
                    {
                        int index = Array.IndexOf(mode, working.name) + 1;
                        while (!mode[index]) index++;
                        exData.name = workMode[index];
                        exData.azimuth = true;
                        return exData;
                    }
                }
                else
                {
                    int index = Array.IndexOf(mode, working.name) + 1;
                    while (!mode[index]) index++;
                    exData.name = workMode[index];
                    exData.azimuth = working.azimuth;
                    return exData;
                }
            }
            else return exData;
        }

        public NowWork FirstStart()
        {
            int index = 0;
            while (!mode[index]) index++;
            NowWork exData = new NowWork( az && inc ? workMode[index] : "zero", az == true);
            return exData;
        }

        public NowWork ZeroMode() {
            NowWork zeroMode = new NowWork("zero", true);
            return zeroMode;
        }
    }
}
