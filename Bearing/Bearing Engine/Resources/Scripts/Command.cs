using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Command
{
    public Action @do;
    public Action undo;

    public Command(Action @do, Action undo)
    {
        this.@do = @do;
        this.undo = undo;
    }
}
