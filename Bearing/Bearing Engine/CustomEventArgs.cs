namespace Bearing;

public class DataEventArgs : EventArgs
{
    public List<object> data;
}
public class UIEventArgs : DataEventArgs
{
    public string eventType;

    public List<object> data;
}