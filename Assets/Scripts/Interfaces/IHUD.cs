using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Interfaces
{
    public interface IHUD : IHealthBar
    {
        float Health { get; }
        float PAProgress { get; }
        event EventHandler PAProgressChanged;
    }
}
