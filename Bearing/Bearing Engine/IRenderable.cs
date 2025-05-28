using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

public interface IRenderable
{
    public int id { get; set; }

    public void Render() { }
}