using System;
using Ionic.Zlib;
using UnityEngine;

namespace Framework.Network
{
    public class CompressUtil
    {
        public static bool DebugLog = false;
        public static byte[] UncompressBuffer(int MsgID,byte[] bytes)
        {
            if (DebugLog)
            {
                Debug.Log(string.Format("UncompressBuffer,MSG ID:{0}", MsgID));
                int length = bytes.Length;
                DateTime beforeDT = DateTime.Now;
                byte[] RetBytes = DeflateStream.UncompressBuffer(bytes);
                DateTime afterDT = DateTime.Now;
                TimeSpan ts = afterDT.Subtract(beforeDT);
                Debug.Log(string.Format("Use:{0},before:{1},after:{2}", ts.Milliseconds, length, RetBytes.Length));
                return RetBytes;
            }
            return DeflateStream.UncompressBuffer(bytes);
        }
    }

}