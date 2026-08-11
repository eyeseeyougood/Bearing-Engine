using System.Text;

namespace Bearing;

public interface IBSTSerialisable
{
	public void Serialise(StringBuilder sb, object value);
}