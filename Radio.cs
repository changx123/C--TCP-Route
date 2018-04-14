using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public abstract class Radio
{
    public Radio(ushort Id){
        RouteModule.AddModule(Id,(Radio)this);
    }

    public void Write(Route.Module module){
            Route(module);
    }
    
    protected abstract void Route(Route.Module module);
}
