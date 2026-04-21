using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI.Commands;

public abstract class Command
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Usage { get; }
    public abstract void Execute(string[] args);
    public App App { get; private set; }
    public Command (App app)
    {
        App = app;
    }
}
