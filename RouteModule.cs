using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Reflection;

public class RouteModule : Route
{

    public static Dictionary <ushort, Radio> ModuleS = new Dictionary<ushort, Radio>();

    public RouteModule(){

    }

    public void RunClient (string ip,ushort port){
        _reader_objS();
        _run(ip,port);
    }

    private void _run(string ip,ushort port){
        IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse(ip), port);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(serverIp, asyncResult =>
        {
            NewConn(socket);
            SetEndian(true);
            Reader(ReaderModuleHandler);
        }, null);
    }

    private void _reader_objS(){
        Dictionary <int, string> objS = new Dictionary<int, string>();
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] typeArr = assembly.GetTypes();
        int i = 0;
        foreach (Type t in typeArr)
        {
            if(t.Namespace == "Module"){
                objS.Add(i,t.Name);
                i++;
            }
        }
        foreach (var item in objS){
            assembly.CreateInstance("Module."+item.Value);
        }
    }

    public static void AddModule(ushort module,Radio obj){
        ModuleS.Add(module,obj);
    }

    public void ReaderModuleHandler(Module module){
        if (ModuleS.ContainsKey(module.module))
        {
            Radio obj = ModuleS[module.module];
            obj.Write(module);
        }
    }
}