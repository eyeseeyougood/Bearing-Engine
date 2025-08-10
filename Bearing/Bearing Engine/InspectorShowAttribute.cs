using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InspectorShowAttribute : Attribute
{
}