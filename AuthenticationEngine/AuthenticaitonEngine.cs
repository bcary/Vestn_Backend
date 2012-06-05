using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data;
using System.Data.Entity;
using Entity;

namespace Engine
{
    public class AuthenticaitonEngine
    {
        public int authenticate(string token)
        {
            return 4; //user id
        }

        public string logIn(int userId, string userName)
        {
            byte[] key = Encoding.ASCII.GetBytes("BrianIsABoss");
            string dateTime = DateTime.Now.ToString();
            string input = dateTime + userId.ToString() + userName;
            string token = Encode(input, key);
            VestnDB db = new VestnDB();
            return "token";
        }

        public static string Encode(string input, byte[] key)
        {
            HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }


    }
}
