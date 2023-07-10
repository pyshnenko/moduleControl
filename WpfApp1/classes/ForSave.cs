using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class ForSave
    {
        public int progonSpeedAz { get; set; }
        public int progonSpeedInc { get; set; }
        public int creetAngleAzP { get; set; }
        public int creetAngleAzN { get; set; }
        public int creetAngleIncP { get; set; }
        public int creetAngleIncN { get; set; }
        public int speedK { get; set; }
        public int commandDelay { get; set; }
        public bool debug { get; set; }
        public bool readonlyP { get; set; }
        public bool correct = false;

        public bool unparseAndSave (string text)
        {
            ForSave readed = JsonSerializer.Deserialize<ForSave>(text);
            if (readed != null)
            {
                progonSpeedAz = readed.progonSpeedAz;
                progonSpeedInc = readed.progonSpeedInc;
                creetAngleAzN = readed.creetAngleAzN;
                creetAngleAzP = readed.creetAngleAzP;
                creetAngleIncN = readed.creetAngleIncN; 
                creetAngleIncP = readed.creetAngleIncP;
                speedK = readed.speedK;
                commandDelay = readed.commandDelay;
                debug = readed.debug;
                readonlyP = readed.readonlyP;
                correct = true;
                return true;
            }
            return false;
        }
    }
}
