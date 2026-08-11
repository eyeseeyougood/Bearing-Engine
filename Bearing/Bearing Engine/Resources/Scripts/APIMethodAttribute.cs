using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class APIMethodAttribute : Attribute
{
}