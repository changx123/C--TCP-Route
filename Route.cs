using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class Route : TcpTlv
{
    public struct Module
    {
        public ushort module;
        public ushort action;
        public Tlv tlv;
    }

    public delegate void ReaderModuleDelegate(Module module);
    
    public ReaderModuleDelegate ReaderModuleHandler;

    // private Socket socketClient;
    // private TcpTlv tcpTlv;

    // public void Route(){
        // TcpTlv tcpTlv = new TcpTlv();
        // tcpTlv.NewConn(socketClient);
        // tcpTlv.SetEndian(true);
        // tcpTlv.Reader(ReaderHandler);
    // }

    /// <summary>  
    /// 发送数据byte 
    /// </summary>
    public int Write(ushort module,ushort action,byte[] b){
        b = _add_module(module,action,b);
        return _write(0,b);
    }

    /// <summary>  
    /// 发送数据string 
    /// </summary>
    public int Write(ushort module,ushort action,string str){
        byte[] b = _stringToByte(str);
        b = _add_module(module,action,b);
        return _write(0,b);
    }

    public void Reader(ReaderModuleDelegate readerModuleHandler){
        ReaderHandler = new ReaderDelegate(readerHandlerP);
        _readerMsg = new Thread(_reader);
        _readerMsg.Start();
        ReaderModuleHandler = new ReaderModuleDelegate(readerModuleHandler);
    }

    //加入module action
    private byte[] _add_module(ushort module,ushort action,byte[] b){
        MemoryStream arr = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(arr);
        byte[] m = BitConverter.GetBytes(module);
        if (_tlvConn.Endian)
        {
            Array.Reverse(m);
        }
        binaryWriter.Write(m);
        byte[] a = BitConverter.GetBytes(action);
        if (_tlvConn.Endian)
        {
            Array.Reverse(a);
        }
        binaryWriter.Write(a);
        binaryWriter.Write(b);
        return arr.ToArray();
    }

    private Module _reader_module(TcpTlv.Tlv tlv){
        Module module;
        MemoryStream arr = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(arr);
        BinaryReader binaryReader = new BinaryReader(arr);
        binaryWriter.Write(tlv.Value);
        arr.Seek(0, SeekOrigin.Begin);
        byte[] mBt = binaryReader.ReadBytes(2);
        if (_tlvConn.Endian)
        {
            Array.Reverse(mBt);
        }
        ushort m = System.BitConverter.ToUInt16(mBt,0);
        arr.Seek(2, SeekOrigin.Begin);
        byte[] aBt = binaryReader.ReadBytes(2);
        if (_tlvConn.Endian)
        {
            Array.Reverse(aBt);
        }
        ushort a = System.BitConverter.ToUInt16(aBt,0);
        module.module = m;
        module.action = a;
        arr.Seek(4, SeekOrigin.Begin);
        byte[] valBt = binaryReader.ReadBytes((int)(tlv.Length - 4));
        tlv.Length -= 4;
        tlv.Value = valBt;
        module.tlv = tlv;
        return module;
    }
    
    private void readerHandlerP(TcpTlv.Tlv tlv){
        Module module = _reader_module(tlv);
        ReaderModuleHandler(module);
    }
}