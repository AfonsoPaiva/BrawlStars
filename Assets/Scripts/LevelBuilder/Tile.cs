using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GridEditor
{
    public class Tile : Prop
    {
        public enum Propability { Propable, NonPropable }
        public Propability propability = Propability.Propable;
    }
}
