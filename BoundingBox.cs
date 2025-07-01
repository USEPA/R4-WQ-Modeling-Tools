using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCEIData
{
    public class BoundingBox
    {
        private float north, south, east, west;

        public BoundingBox(float _north, float _south, float _west, float _east)
        {
            //bbox i N,S,E,W coordinate of selected grid
            north = _north;
            south = _south;
            west = _west;
            east = _east;
        }

        public float North
        {
            get { return north; }
            set { north = value; }
        }
        public float South
        {
            get { return south; }
            set { south = value; }
        }
        public float West
        {
            get { return west; }
            set { west = value; }
        }
        public float East
        {
            get { return east; }
            set { east = value; }
        }
    }
}
