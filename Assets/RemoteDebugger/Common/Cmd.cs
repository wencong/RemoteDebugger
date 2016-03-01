/*!lic_info

The MIT License (MIT)

Copyright (c) 2015 SeaSunOpenSource

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/


using System.Collections.Generic;
using System.Text;
using System;
using System.Runtime.InteropServices;

public enum CmdIOErrorCode 
{
	ReadOverflow,
	WriteOverflow,
	TypeMismatched,
}

public class CmdIOError : Exception
{
	static Dictionary<CmdIOErrorCode, string> InfoLut = new Dictionary<CmdIOErrorCode, string> () {
		{ CmdIOErrorCode.ReadOverflow, "Not enough space for reading." },
		{ CmdIOErrorCode.WriteOverflow, "Not enough space for writing." },
		{ CmdIOErrorCode.TypeMismatched, "Reading/writing a string as a primitive." },
	};

	public CmdIOError(CmdIOErrorCode code) : base("[UsCmdIOError] " + InfoLut[code]) 
	{
		ErrorCode = code;
	}

	public CmdIOErrorCode ErrorCode;
}

public class Cmd {
	public const int STRIP_NAME_MAX_LEN = 64;
	public const int BUFFER_SIZE = 1024;
	
	public Cmd()
	{
		_buffer = new byte[BUFFER_SIZE];
	}

    public Cmd(byte[] given)
	{
		_buffer = given;
	}

	public byte[] Buffer { get { return _buffer; }	}
	public int WrittenLen { get { return _writeOffset; } }

	public object ReadPrimitive<T>()
	{
		if (typeof(T) == typeof(string)) 
			throw new CmdIOError(CmdIOErrorCode.TypeMismatched);
		
		if (_readOffset + Marshal.SizeOf(typeof(T)) > _buffer.Length)
			throw new CmdIOError(CmdIOErrorCode.ReadOverflow);

		object val = Generic.Convert<T> (_buffer, _readOffset);
		_readOffset += Marshal.SizeOf(typeof(T));
		return val;
	}

	public string ReadString()
	{
		int strLen = ReadInt32();
        if (strLen == 0)
            return "";

		if (_readOffset + (int)strLen > _buffer.Length)
			throw new CmdIOError(CmdIOErrorCode.ReadOverflow);
		
		string ret = Encoding.Default.GetString(_buffer, _readOffset, (int)strLen);
		_readOffset += (int)strLen;
		return ret;
	}
	
	public void WritePrimitive<T>(T value) 
	{
		if (typeof(T) == typeof(string)) 
			throw new CmdIOError(CmdIOErrorCode.TypeMismatched);

		if (_writeOffset + Marshal.SizeOf(typeof(T)) > _buffer.Length)
			throw new CmdIOError(CmdIOErrorCode.WriteOverflow);
		
		byte[] byteArray = Generic.Convert(value);
		if (byteArray == null) 
			throw new CmdIOError(CmdIOErrorCode.TypeMismatched);

		byteArray.CopyTo(_buffer, _writeOffset);
		_writeOffset += Marshal.SizeOf(typeof(T));
	}

	public void WriteStringStripped(string value, int stripLen)
	{
        if (string.IsNullOrEmpty(value))
        {
            WritePrimitive((int)0);
        }
        else
        {
            //string stripped = value.Length > stripLen ? value.Substring(0, stripLen) : value;
            byte[] byteArray = Encoding.Default.GetBytes(value);

            WritePrimitive((int)byteArray.Length);

            byteArray.CopyTo(_buffer, _writeOffset);
            _writeOffset += byteArray.Length;
        }
	}
	
	public NetCmd ReadNetCmd() 			{ return (NetCmd)ReadInt16(); }
	public short ReadInt16() 				{ return (short)ReadPrimitive<short>();	}
	public int ReadInt32() 					{ return (int)ReadPrimitive<int>();	}
	public float ReadFloat()				{ return (float)ReadPrimitive<float>();	}
	
	public void WriteNetCmd(NetCmd cmd)	{	WritePrimitive((short)cmd);	}
	public void WriteInt16(short value)		{	WritePrimitive(value);	}
	public void WriteInt32(int value) 		{	WritePrimitive(value);	}
	public void WriteFloat(float value)		{	WritePrimitive(value);	}
	public void WriteStringStripped(string value) 	{ WriteStringStripped (value, STRIP_NAME_MAX_LEN); }
	public void WriteString(string value) 			{ WriteStringStripped (value, int.MaxValue); }
	
	private int _writeOffset = 0;
	private int _readOffset = 0;
	private byte[] _buffer;
}

