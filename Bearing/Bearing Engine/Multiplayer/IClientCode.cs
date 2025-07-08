using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing.Multiplayer;

public interface IClientCode
{
    public bool isMine { get; set; }
}