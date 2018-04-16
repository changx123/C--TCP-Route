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

    public string ByteToString(byte[] b){
        return System.Text.Encoding.UTF8.GetString(b);
    }
    
    protected abstract void Route(Route.Module module);
}
