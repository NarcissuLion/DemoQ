using System;
using Framework.Buffer;

namespace Framework.Network.LuaThrift
{
	public class LuaThriftSender : Sender<int, byte[]>
	{
	    private byte[] _Bytes;
	
	    public override void Send(int id, byte[] bytes)
	    {
	        _Bytes = bytes;
	        WriteAndSend(id);
	    }
	
	    protected override int OnWriteBody(IBufferWriteOperator bufferWriter)
	    {
	        bufferWriter.Write(_Bytes);
	        return _Bytes.Length;
	    }
	}
}
