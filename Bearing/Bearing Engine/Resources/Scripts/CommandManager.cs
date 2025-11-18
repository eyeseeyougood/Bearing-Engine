using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bearing;
using Silk.NET.Input;

public static class CommandManager
{
    public static Stack<Command> timeline = new Stack<Command>();
    public static Stack<Command> bin = new Stack<Command>();

    public static void Do(Command cmd)
    {
        cmd.@do.Invoke();
        timeline.Push(cmd);
        bin.Clear();
    }

    public static void Do(Action @do, Action undo)
    {
        Command cmd = new Command(@do, undo);
        
        Do(cmd);
    }

    public static void Register(Command cmd)
    {
        timeline.Push(cmd);
        bin.Clear();
    }

    public static void Register(Action @do, Action undo)
    {
        Command cmd = new Command(@do, undo);

        Register(cmd);
    }

    public static void Redo()
    {
        if (bin.Count == 0) return;

        Command cmd = bin.Pop();
        cmd.@do.Invoke();
        timeline.Push(cmd);
    }

    public static void Undo()
    {
        if (timeline.Count == 0) return;

        Command cmd = timeline.Pop();
        cmd.undo.Invoke();
        bin.Push(cmd);
    }

    public static void Tick()
    {
        if (Input.GetKey(Key.ControlLeft) && Input.GetKeyDown(Key.Z))
        {
            if (!Input.GetKey(Key.ShiftLeft))
                Undo();
            else
                Redo();
        }
    }
}