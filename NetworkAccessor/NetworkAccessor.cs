using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using System.Data.Entity;
using System.Data;
using System.Data.Linq;
using Accessor;

namespace Accessor
{
    public class NetworkAccessor
    {
        public Network CreateNetwork(Network network)
        {
            try
            {
                VestnDB db = new VestnDB();
                db.networks.Add(network);
                db.Entry(network).State = EntityState.Modified;
                db.SaveChanges();
                return network;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Create Network", ex.StackTrace);
                return null;
            }
        }

        public Network GetNetwork(int networkId)
        {
            Network network;
            VestnDB db = new VestnDB();
            try
            {
                network = db.networks.Where(n => n.id == networkId).FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Get Network", ex.StackTrace);
                return null;
            }
            return network;
        }

        public Network UpdateNetwork(Network network)
        {
            try
            {
                VestnDB db = new VestnDB();
                db.Entry(network).State = EntityState.Modified;
                db.SaveChanges();
                return network;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Update Network", ex.StackTrace);
                return null;
            }
        }
    }
}
