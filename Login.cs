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
    public class Login : Radio
    {
        public Login():base(0)
        {
           
        }
        
        protected override void Route(Route.Module module){
            Debug.Log("内容为:" + module.tlv.Length);
        }
    }

    public class Test : Radio
    {
        public Test():base(1)
        {
           
        }
        
        protected override void Route(Route.Module module){
            Debug.Log("内容为:" + module.tlv.Length);
        }
    }
}