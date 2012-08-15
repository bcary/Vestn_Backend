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
                network = db.networks.Where(n => n.id == networkId).Include(n => n.admins).Include(n => n.networkUsers).FirstOrDefault();
            }
            catch(Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Get Network", ex.StackTrace);
                return null;
            }
            return network;
        }

        public Network_TopNetwork GetTopNetwork(int networkId)
        {
            Network_TopNetwork network;
            VestnDB db = new VestnDB();
            try
            {
                network = db.networks.OfType<Network_TopNetwork>().Where(n => n.id == networkId)
                    .Include(n => n.subNetworks)
                    .Include(n => n.admins)
                    .Include(n => n.networkUsers)
                    .FirstOrDefault();

                return network;
                
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Get Network", ex.StackTrace);
                return null;
            }
        }

        public Network_SubNetwork GetSubNetwork(int networkId)
        {
            Network_SubNetwork network;
            VestnDB db = new VestnDB();
            try
            {
                network = db.networks.OfType<Network_SubNetwork>().Where(n => n.id == networkId)
                    .Include(n => n.groups)
                    .Include(n => n.admins)
                    .Include(n => n.networkUsers)
                    .FirstOrDefault();

                return network;

            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Get Network", ex.StackTrace);
                return null;
            }
        }

        public Network_Group GetGroupNetwork(int networkId)
        {
            Network_Group network;
            VestnDB db = new VestnDB();
            try
            {
                network = db.networks.OfType<Network_Group>().Where(n => n.id == networkId)
                    .Include(n => n.admins)
                    .Include(n => n.networkUsers)
                    .FirstOrDefault();

                return network;

            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Get Network", ex.StackTrace);
                return null;
            }
        }

        public Network GetNetworkByIdentifier(string identifier)
        {
            VestnDB db = new VestnDB();
            try
            {
                Network network = db.networks.Where(n => n.networkIdentifier == identifier)
                    .Include(n => n.admins)
                    .Include(n => n.networkUsers)
                    .FirstOrDefault();
                return network;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor GetNetworkByIdentifier", ex.StackTrace);
                return null;
            }
        }

        public Network UpdateNetwork(Network network)
        {
            try
            {
                VestnDB db = new VestnDB();
                var n = new Network { id = network.id };
                n.profileURL = network.profileURL;
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

        public Network UpdateNetworkUrl(Network network)
        {
            try
            {
                VestnDB db = new VestnDB();
                var n = new Network { id = network.id };
                db.networks.Attach(n);
                n.profileURL = network.profileURL;
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

        public Network UpdateNetworkInformation(Network network)
        {
            try
            {
                VestnDB db = new VestnDB();
                var n = new Network { id = network.id };
                db.networks.Attach(n);
                n.name = network.name;
                n.privacy = network.privacy;
                n.description = network.description;
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

        public bool UpdateNetworkIdentifier(int networkId, string identifier)
        {
            try
            {
                VestnDB db = new VestnDB();
                Network n = new Network { id = networkId };
                db.networks.Attach(n);
                n.networkIdentifier = identifier;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor Update Network Identifier", ex.StackTrace);
                return false;
            }
        }

        public bool AddSubNetwork(int topNetId, int subNetId)
        {
            try
            {
                VestnDB db = new VestnDB();

                var t = new Network_TopNetwork { id = topNetId };
                var s = new Network_SubNetwork { id = subNetId };
                db.networks.Attach(t);
                db.networks.Attach(s);

                t.subNetworks.Add(s);

                db.SaveChanges();

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool AddGroupNetwork(int subNetId, int groupNetId)
        {
            try
            {
                VestnDB db = new VestnDB();

                var s = new Network_SubNetwork { id = subNetId };
                var g = new Network_Group { id = groupNetId};
                db.networks.Attach(s);
                db.networks.Attach(g);

                s.groups.Add(g);

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddAdmin(int networkId, int userId)
        {
            try
            {
                VestnDB db = new VestnDB();

                var n = new Network { id = networkId };
                var u = new User { id = userId };
                db.networks.Attach(n);
                db.users.Attach(u);

                n.admins.Add(u);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor AddAdmin", ex.StackTrace);
                return false;
            }
        }

        public bool AddNetworkUser(int networkId, int userId)
        {
            try
            {
                VestnDB db = new VestnDB();

                var n = new Network { id = networkId };
                var u = new User { id = userId };
                db.networks.Attach(n);
                db.users.Attach(u);

                n.networkUsers.Add(u);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor AddAdmin", ex.StackTrace);
                return false;
            }
        }

        public bool UpdateNetworkCoverPicture(int networkId, string coverPictureLocation)
        {
            try
            {
                VestnDB db = new VestnDB();
                var n = new Network {id = networkId};
                db.networks.Attach(n);
                n.coverPicture = coverPictureLocation;

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor UpdateNetworkCoverPicture", ex.StackTrace);
                return false;
            }
        }

        public bool DeleteNetworkUser(int networkId, int userId)
        {
            try
            {
                VestnDB db = new VestnDB();

                Network net = db.networks.Single(n => n.id == networkId);
                User user = db.users.Single(u => u.id == userId);
                net.networkUsers.Remove(user);
                
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor DeleteNetworkUser", ex.StackTrace);
                return false;
            }
        }

        public bool DeleteNetworkAdmin(int networkId, int adminId)
        {
            try
            {
                VestnDB db = new VestnDB();
                Network net = db.networks.Single(n => n.id == networkId);
                User admin = db.users.Single(u => u.id == adminId);
                net.admins.Remove(admin);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor DeleteNetworkAdmin", ex.StackTrace);
                return false;
            }
        }

        public bool DeleteSubNetwork(int topNetworkId, int subNetworkId)
        {
            try
            {
                VestnDB db = new VestnDB();
                Network_TopNetwork topnet = (Network_TopNetwork)db.networks.Single(top => top.id == topNetworkId);
                Network_SubNetwork subnet = (Network_SubNetwork)db.networks.Single(sub => sub.id == subNetworkId);
                topnet.subNetworks.Remove(subnet);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor DeleteSubnetwork", ex.StackTrace);
                return false;
            }
        }

        public bool DeleteGroupNetwork(int subNetworkId, int groupNetworkId)
        {
            try
            {
                VestnDB db = new VestnDB();
                Network_SubNetwork subnet = (Network_SubNetwork)db.networks.Single(sub => sub.id == subNetworkId);
                Network_Group groupnet = (Network_Group)db.networks.Single(group => group.id == groupNetworkId);
                subnet.groups.Remove(groupnet);

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogAccessor la = new LogAccessor();
                la.CreateLog(DateTime.Now, "Network Accessor DeleteGroupNetwork", ex.StackTrace);
                return false;
            }
        }

        public Network GetNetworkByUrl(string networkURL)
        {
            try
            {
                VestnDB db = new VestnDB();

                Network network = db.networks.Where(n => n.profileURL == networkURL).FirstOrDefault();
                return network;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool IsNetworkUrlAvailable(string networkURL)
        {
            try
            {
                VestnDB db = new VestnDB();

                List<Network> query = db.networks.Where(n => n.profileURL == networkURL).ToList();
                if (query == null || query.Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
