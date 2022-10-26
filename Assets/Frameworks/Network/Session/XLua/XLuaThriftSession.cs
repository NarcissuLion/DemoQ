using Framework.Network;
using System;

namespace Framework.Network.LuaThrift
{
	public class LuaThriftSession : TCPSession
	{
	    public LuaThriftReceiver LuaThriftReceiver
	    {
	        get
	        {
	            return GetReceiver<LuaThriftReceiver>();
	        }
	    }
	
	    public LuaThriftSender LuaThriftSender
	    {
	        get
	        {
	            return GetSender<LuaThriftSender>();
	        }
	    }
	
	    public LuaThriftSession()
	    {
	        RegisterReceiver<LuaThriftReceiver, int, byte[]>();
	        RegisterSender<LuaThriftSender, int, byte[]>();
	    }

        public void ListenConnected(Action callback)
        {
            OnConnected += callback;
        }

        public void ListenDisconnected(Action callback)
        {
            OnDisconnected += callback;
        }
	}
}

