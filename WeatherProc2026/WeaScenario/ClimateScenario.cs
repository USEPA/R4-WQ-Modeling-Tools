using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotSpatial.Controls;

namespace WeaScenario
{
    public class ClimateScenario
    {
        SpatialStatusStrip strip;
        ToolStripStatusLabel slabel;

        public ClimateScenario(SpatialStatusStrip _strip, ToolStripStatusLabel _slabel)
        {
            strip = _strip;
            slabel = _slabel;
        }
    }
}
