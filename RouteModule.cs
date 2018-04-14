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
    private Route route;

    public static Dictionary <ushort, Radio> ModuleS = new Dictionary<ushort, Radio>();

    public RouteModule(){

    }

    public void Run (string ip,ushort port){
        _reader_objS();
        _run(ip,port);
    }

    private void _run(string ip,ushort port){
        route = new Route();
        IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(serverIp, asyncResult =>
        {
            route.NewConn(socket);
            route.SetEndian(true);
            route.Write(0,0,"{json:123456,123456}阿萨德拉斯代理卡是");
            route.Reader(ReaderModuleHandler);
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
            //Console.WriteLine(item.Key + item.Value);
            assembly.CreateInstance("Module."+item.Value);
            // _add();
            // Debug.Log(item.Key + item.Value);
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
            // MethodInfo objMI = obj.GetType().GetMethod("Run");
            // objMI.Invoke(obj, null);
            // MethodInfo objMI = obj.GetType().GetMethod("Route");
            // object[] parametor = new string[] { "test"}; 
            // string s = "阿萨德离开静安寺了肯德基熬枯受淡杰卡斯";
            // objMI.Invoke(obj, new object[] { s });
        }
    }

    // public void Add(ushort module,string module_type){
    //     ModuleList.Add(module,module_type);
    // }
}