using Framework.Network;
using System;
using Framework.Buffer;
using Ionic.Zlib;
using UnityEngine;

namespace Framework.Network.LuaThrift
{
	public class LuaThriftReceiver : Receiver<int, byte[]>
	{
        private bool debugUncompress = true;
        // 因为tolua不支持给event添加面向对象监听，所以只能加这个方法
        public void Listen(Action<int, byte[]> onReceived)
        {
            OnReceived += onReceived;
        }

        protected override bool OnReceive(int msgID, int msgBodyLength, IBufferReader bufferReader, bool bCompress)
        {
            byte[] bytes = bufferReader.ReadBytes(msgBodyLength);
            if (bCompress) bytes = CompressUtil.UncompressBuffer(msgID,bytes);
            Enqueue(msgID, bytes);
	        return true;
	    }
	}
}

