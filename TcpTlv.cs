using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class TcpTlv
{
    public struct Tlv
    {
        //type T
        public byte Type;
        //Length L
        public uint Length;
        //Value V
        public byte[] Value;
    }

    protected struct TlvConn
    {
        //链接
        public Socket Conn;
        //每次读取Buffer长度 默认1024
        public int ReadBufferSize;
        //端绪 false默认小端序 true 大端序
        public bool Endian;
    }

    ///初始化socket连接结构
    protected TlvConn _tlvConn;

    ///声明委托
    // public delegate void ReaderDelegate(Tlv tlv,TcpTlv _this);
    ///声明委托
    public delegate void ReaderDelegate(Tlv tlv);
    
    public ReaderDelegate ReaderHandler;

    protected Thread _readerMsg;//处理消息的线程

    /// <summary>  
    /// 初始化数据  
    /// </summary> 
    public TcpTlv()
    {
        _tlvConn.ReadBufferSize = 1024;
        _tlvConn.Endian = false;
    }
    
    /// <summary>  
    /// 传入socket连接  
    /// </summary> 
    public void NewConn(Socket conn){
        _tlvConn.Conn = conn;
    }

    /// <summary>  
    /// 设置接收数据buffer字节  
    /// </summary> 
    public void SetReadBufferSize(int size){
        _tlvConn.ReadBufferSize = size;
    }

    /// <summary>  
    /// 设置端序 true为大端 false为小端 默认大端  
    /// </summary> 
    public void SetEndian(bool endian){
        _tlvConn.Endian = endian;
    }

    /// <summary>  
    /// 发送数据byte  
    /// </summary> 
    public int Write(byte[] b){
        return _write(0,b);
    }

    /// <summary>  
    /// 发送数据byte  
    /// </summary> 
    public int Write(byte type,byte[] b){
        return _write(type,b);
    }

    /// <summary>  
    /// 发送数据string 
    /// </summary>
    public int Write(string str){
        byte[] b = _stringToByte(str);
        return _write(0,b);
    }

    /// <summary>  
    /// 发送数据string  
    /// </summary>
    public int Write(byte type,string str){
        byte[] b = _stringToByte(str);
        return _write(type,b);
    }

    /// <summary>  
    /// 发送数据
    /// </summary>
    protected int _write(byte t,byte[] b){
        uint len = (uint)b.Length;
        Tlv tlv = _writeTlv(t,len,b);
        byte[] data = _writeTlvToBt(tlv);
        return _tlvConn.Conn.Send(data);
    }

    /// <summary>  
    /// 写入tlv结构  
    /// </summary>
    protected Tlv _writeTlv(byte t,uint l,byte[] v){
        Tlv tlv;
        tlv.Type = t;
        tlv.Length = l;
        tlv.Value = v;
        return tlv;
    }

    /// <summary>  
    /// 字符串转换byte[]  
    /// </summary>
    protected byte[] _stringToByte(string str){
        byte[] b = Encoding.UTF8.GetBytes(str);
        return b;
    }

    /// <summary>  
    /// byte[]转换字符串
    /// </summary>
    public string ByteToString(byte[] b){
        return System.Text.Encoding.UTF8.GetString(b);
    }

    /// <summary>  
    /// Tlv结构转为byte[]  
    /// </summary>
    private byte[] _writeTlvToBt(Tlv tlv){
        MemoryStream arr = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(arr);
        // arr.Seek(0, SeekOrigin.Begin);
        binaryWriter.Write(tlv.Type);
        byte[] len = BitConverter.GetBytes(tlv.Length);
        if (_tlvConn.Endian)
        {
            Array.Reverse(len);
        }
        binaryWriter.Write(len);
        byte[] val = tlv.Value;
        binaryWriter.Write(val);
        byte[] b = arr.ToArray();
        return b;
    }

    public void Reader(ReaderDelegate readerHandler){
        ReaderHandler = new ReaderDelegate(readerHandler);
        _readerMsg = new Thread(_reader);
        _readerMsg.Start();
    }

    protected void _reader(){
        MemoryStream Bio = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(Bio);
        BinaryReader binaryReader = new BinaryReader(Bio);
        uint Biol = 0;
        byte[] readerIo = new byte[_tlvConn.ReadBufferSize];
        Tlv tlv = new Tlv();
        tlv.Type = 0;
        tlv.Length = 0;
        //EOF 标识
        bool EOF = false;
        int Reader_index = 0;
        while (true){
            if(tlv.Length == 0 && Biol >= 5){
                Bio.Seek(Reader_index, SeekOrigin.Begin);
                Reader_index += 1;
                tlv.Type = binaryReader.ReadByte();
                byte[] LenBt = new byte[4];
                LenBt = binaryReader.ReadBytes(4);
                Reader_index += 4;
                if (_tlvConn.Endian)
                {
                    Array.Reverse(LenBt);
                }
                tlv.Length = System.BitConverter.ToUInt32(LenBt,0); 
                Biol -= 5;
            }
            if (tlv.Length != 0 && Biol >= tlv.Length)
            {
                tlv.Value = new byte[(int)tlv.Length];
                tlv.Value = binaryReader.ReadBytes((int)tlv.Length);
                Biol -= tlv.Length;
                Reader_index += (int)tlv.Length;
                // ReaderHandler(tlv,this);
                Thread _handler = new Thread(new ParameterizedThreadStart(Th_handler));
                _handler.Start(tlv);
                tlv = new Tlv();
                if(Biol <= 0 && EOF){
                    break;
                }
                continue;
            }
            int len = _tlvConn.Conn.Receive(readerIo,0, readerIo.Length, SocketFlags.None);
            if (len > 0)
            {
                byte[] temp = new byte[len];
                Array.Copy(readerIo, 0, temp, 0, len);
                binaryWriter.Write(temp);
                Biol += (uint)len;
            }else{
                EOF = true;
            }
        }
    }

    protected void Th_handler(object rc){
        Tlv tlv = (Tlv)rc;
        ReaderHandler(tlv);
    }
}