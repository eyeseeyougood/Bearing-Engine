using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class HideFromInspectorAttribute : Attribute
{
}