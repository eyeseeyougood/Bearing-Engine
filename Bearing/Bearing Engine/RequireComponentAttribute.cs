using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bearing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RequireComponentAttribute : Attribute
{
    private Type[] types;

    // TODO: implement logic for this
    public RequireComponentAttribute(params Type[] components)
    {
        types = components;
    }
}