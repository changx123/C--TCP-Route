using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Reflection;
namespace Module 
{
    public class Demo : Radio
    {
        public Demo():base(0)
        {
           
        }
        
        protected override void Route(Route.Module module){
            Debug.Log("路由module:"+ module.module +" 内容为:" + ByteToString(module.tlv.Value));
        }
    }

    public class Test : Radio
    {
        public Test():base(1)
        {
           
        }
        
        protected override void Route(Route.Module module){
            Debug.Log("路由module:"+ module.module +" 内容为:" + ByteToString(module.tlv.Value));
        }
    }
}