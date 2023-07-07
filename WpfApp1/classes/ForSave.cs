using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class ForSave
    {
        public int progonSpeedAz;
        public int progonSpeedInc;
        public int creetAngleAzP;
        public int creetAngleAzN;
        public int creetAngleIncP;
        public int creetAngleIncN;
        public int speedK;
        public int commandDelay;
        public bool debug;
        public bool correct = false;

        public ForSave(
            int progonSpeedAz,
            int progonSpeedInc,
            int creetAngleAzP,
            int creetAngleAzN,
            int creetAngleIncP,
            int creetAngleIncN,
            int speedK,
            int commandDelay,
            bool debug)
        {
            this.progonSpeedAz = progonSpeedAz;
            this.progonSpeedInc = progonSpeedInc;
            this.creetAngleIncP = creetAngleIncP;
            this.creetAngleIncN = creetAngleIncN;
            this.creetAngleAzP = creetAngleAzP;
            this.creetAngleAzN = creetAngleAzN;
            this.speedK = speedK;
            this.commandDelay = commandDelay;
            this.debug = debug;
        }

        private struct savedData
        {
            public int progonSpeedAz;
            public int progonSpeedInc;
            public int creetAngleAzP;
            public int creetAngleAzN;
            public int creetAngleIncP;
            public int creetAngleIncN;
            public int speedK;
            public int commandDelay;
            public bool debug;

            public savedData(
            int progonSpeedAz,
            int progonSpeedInc,
            int creetAngleAzP,
            int creetAngleAzN,
            int creetAngleIncP,
            int creetAngleIncN,
            int speedK,
            int commandDelay,
            bool debug)
            {
                this.progonSpeedAz = progonSpeedAz;
                this.progonSpeedInc = progonSpeedInc;
                this.creetAngleIncP = creetAngleIncP;
                this.creetAngleIncN = creetAngleIncN;
                this.creetAngleAzP = creetAngleAzP;
                this.creetAngleAzN = creetAngleAzN;
                this.speedK = speedK;
                this.commandDelay = commandDelay;
                this.debug = debug;
            }
        }

        public void unparseAndSave(string text)
        {
            savedData readJson;
            try
            {
                readJson = JsonSerializer.Deserialize<savedData>(text);

                if ((readJson.progonSpeedInc != 0) && (readJson.progonSpeedAz != 0)) this.correct = true;
                else readJson = baseCreate();
            }
            catch
            {
                readJson = baseCreate();
                this.correct = false;
            }

            this.progonSpeedAz = readJson.progonSpeedAz;
            this.progonSpeedInc = readJson.progonSpeedInc;
            this.creetAngleIncP = readJson.creetAngleIncP;
            this.creetAngleIncN = readJson.creetAngleIncN;
            this.creetAngleAzP = readJson.creetAngleAzP;
            this.creetAngleAzN = readJson.creetAngleAzN;
            this.speedK = readJson.speedK;
            this.commandDelay = readJson.commandDelay;
            this.debug = readJson.debug;
        }

        public string parsedString()
        {
            savedData sdata = new savedData
            {
                progonSpeedAz = progonSpeedAz,
                progonSpeedInc = progonSpeedInc,
                creetAngleIncP = creetAngleIncP,
                creetAngleIncN = creetAngleIncN,
                creetAngleAzP = creetAngleAzP,
                creetAngleAzN = creetAngleAzN,
                speedK = speedK,
                commandDelay = commandDelay,
                debug = debug
            };

            string data = JsonSerializer.Serialize(sdata);
            return data;
        }

        private savedData baseCreate()
        {
            return new savedData
            {
                progonSpeedAz = 25,
                progonSpeedInc = 25,
                creetAngleIncP = 90,
                creetAngleIncN = -90,
                creetAngleAzP = 10,
                creetAngleAzN = -30,
                speedK = 158,
                commandDelay = 2000,
                debug = true
            };
        }
    }
}
