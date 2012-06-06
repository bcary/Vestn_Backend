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
            VestnDB db = new VestnDB();
            Authentication auth = (from c in db.authentication where c.token == token select c).FirstOrDefault();
            if (auth == null)
            {
                return -1;
            }
            else
            {
                DateTime currentTime = DateTime.Now;
                // want to add time of most recent use of auth token
                return auth.userId;
            }
        }

        public string logIn(int userId, string userName)
        {
            byte[] key = Encoding.ASCII.GetBytes("BrianIsABoss");
            DateTime time = DateTime.Now;
            string dateTime = time.ToString();
            string input = dateTime + userId.ToString() + userName;
            string token = Encode(input, key);
            Authentication auth = new Authentication();
            auth.timeStamp = time;
            auth.token = token;
            auth.userId = userId;
            VestnDB db = new VestnDB();
            
            db.authentication.Add(auth);
            db.SaveChanges();

            return token;
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
