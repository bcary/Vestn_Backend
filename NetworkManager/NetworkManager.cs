using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using Entity;
using Manager;
using Engine;

namespace Manager
{
    public class NetworkManager
    {
        NetworkAccessor networkAccessor = new NetworkAccessor();
        LogAccessor logAccessor = new LogAccessor();
        UserManager userManager = new UserManager();
        

        public JsonModels.Network CreateNetwork(int adminUserId)
        {
            try
            {
                Network_TopNetwork newNetwork = new Network_TopNetwork();
                User networkAdmin = userManager.GetUser(adminUserId);
                newNetwork.admins.Add(networkAdmin);
                if (networkAdmin.networks == null)
                {
                    networkAdmin.networks = newNetwork.id.ToString();
                }
                else
                {
                    networkAdmin.networks += ("," + newNetwork.id.ToString());
                }
                Network returnNetwork = networkAccessor.CreateNetwork(newNetwork);
                return GetNetworkJson(returnNetwork);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - CreateNetwork", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network CreateSubNetwork(int topNetworkId)
        {
            try
            {
                Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(topNetworkId);
                Network_SubNetwork returnNetwork;
                if (topNet != null)
                {
                    Network_SubNetwork newSubNetwork = new Network_SubNetwork();
                    newSubNetwork.Network_TopNetwork_Id = topNetworkId;
                    returnNetwork = (Network_SubNetwork)networkAccessor.CreateNetwork(newSubNetwork);

                    topNet.subNetworks.Add(returnNetwork);
                    networkAccessor.UpdateNetwork(topNet);
                }
                else
                {
                    //cant add a subnetwork without knowing the parent
                    return null;
                }

                return GetNetworkJson(returnNetwork);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - CreateSubNetwork", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network CreateGroupNetwork(int subNetworkId)
        {
            try
            {
                Network_SubNetwork subNet = (Network_SubNetwork)networkAccessor.GetNetwork(subNetworkId);
                Network_Group returnNetwork;
                if (subNet != null)
                {
                    Network_Group newGroup = new Network_Group();
                    newGroup.Network_SubNetwork_Id = subNetworkId;
                    returnNetwork = (Network_Group)networkAccessor.CreateNetwork(newGroup);

                    subNet.groups.Add(returnNetwork);
                    networkAccessor.UpdateNetwork(subNet);
                }
                else
                {
                    //cant add a groud network without knowing the parent sub network
                    return null;
                }

                return GetNetworkJson(returnNetwork);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - CreateSubNetwork", ex.StackTrace);
                return null;
            }
        }

        public Network GetNetwork(int networkId)
        {
            Network network = networkAccessor.GetNetwork(networkId);
            return network;
        }

        public JsonModels.NetworkUsers GetNetworkUsers(int networkId)
        {
            try
            {
                Network network = networkAccessor.GetNetwork(networkId);
                if (network != null)
                {
                    JsonModels.NetworkUsers networkUsersJson = new JsonModels.NetworkUsers();
                    foreach (User u in network.networkUsers)
                    {
                        if (u != null)
                        {
                            JsonModels.NetworkUserShell userShell = new JsonModels.NetworkUserShell();
                            userShell.userId = u.id;
                            userShell.firstName = u.firstName;
                            userShell.lastName = u.lastName;
                            userShell.profileURL = u.profileURL;
                            networkUsersJson.users.Add(userShell);
                        }
                    }
                    return networkUsersJson;
                }
                else
                {
                    //couldnt find network
                    return null;
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - GetNetworkUsers", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network UpdateNetworkModel(JsonModels.Network networkJson)
        {
            try
            {
                if (networkJson != null)
                {
                    Network originalNetwork = networkAccessor.GetNetwork(networkJson.id);
                    if (originalNetwork != null)
                    {
                        originalNetwork.description = networkJson.description;
                        originalNetwork.name = networkJson.name;
                        originalNetwork.privacy = networkJson.privacy;

                        Network returnNetwork = networkAccessor.UpdateNetwork(originalNetwork);
                        return (GetNetworkJson(returnNetwork));
                    }
                    else
                    {
                        //network id not found in database
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - CreateSubNetwork", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network AddNetworkUsers(Network network, IEnumerable<string> userEmails)
        {
            try
            {
                if (network != null)
                {
                    if (userEmails != null)
                    {
                        foreach (string email in userEmails)
                        {
                            if (email != null)
                            {
                                User addUser = userManager.GetUserByEmail(email);
                                if (addUser != null)
                                {
                                    network.networkUsers.Add(addUser);
                                    if (addUser.networks == null)
                                    {
                                        addUser.networks = network.id.ToString();
                                        userManager.UpdateUser(addUser);
                                    }
                                    else
                                    {
                                        addUser.networks += ("," + network.id.ToString());
                                        userManager.UpdateUser(addUser);
                                    }
                                }
                                else
                                {
                                    //TODO
                                    //user does not exist, send invite email with network creds
                                    return null; //for now
                                }
                            }
                        }
                        return GetNetworkJson(networkAccessor.UpdateNetwork(network));
                    }
                    else
                    {
                        //no emails to add
                        return null;
                    }
                }
                else
                {
                    //no network
                    return null;
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - AddNetworkUsers", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network AddNetworkAdmin(Network network, string adminEmail)
        {
            try
            {
                if (network != null)
                {
                    if (adminEmail != null)
                    {
                        User admin = userManager.GetUserByEmail(adminEmail);
                        if (admin != null)
                        {
                            network.admins.Add(admin);
                            if (admin.networks == null)
                            {
                                admin.networks = network.id.ToString();
                            }
                            else
                            {
                                admin.networks += ("," + network.id.ToString());
                            }
                            userManager.UpdateUser(admin);
                            Network returnNetwork = networkAccessor.UpdateNetwork(network);
                            return GetNetworkJson(returnNetwork);
                        }
                        else
                        {
                            //email does not exist in system, send email invitation with network admin creds
                            return null;
                        }
                    }
                    else
                    {
                        //need the email
                        return null;
                    }
                }
                else
                {
                    return null;
                    //must have network
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - AddNetworkAdmin", ex.StackTrace);
                return null;
            }
        }

        public bool IsNetworkAdmin(int networkId, int userId)
        {
            Network network = networkAccessor.GetNetwork(networkId);
            if (network.GetType() == typeof(Network_TopNetwork))
            {
                if (network.admins != null)
                {
                    foreach (User u in network.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (network.GetType() == typeof(Network_SubNetwork))
            {
                Network_SubNetwork subNet = (Network_SubNetwork)network;
                Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(subNet.Network_TopNetwork_Id);
                List<User> allAdmins = new List<User>();
                allAdmins.Concat(subNet.admins);
                allAdmins.Concat(topNet.admins);
                if (allAdmins != null)
                {
                    foreach (User u in allAdmins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (network.GetType() == typeof(Network_Group))
            {
                Network_Group groupNet = (Network_Group)network;
                Network_SubNetwork subNet = (Network_SubNetwork)networkAccessor.GetNetwork(groupNet.Network_SubNetwork_Id);
                Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(subNet.Network_TopNetwork_Id);
                List<User> allAdmins = new List<User>();
                allAdmins.Concat(groupNet.admins);
                allAdmins.Concat(subNet.admins);
                allAdmins.Concat(topNet.admins);
                if (allAdmins != null)
                {
                    foreach (User u in allAdmins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public JsonModels.Network RemoveNetworkUser(int networkId, int userId)
        {
            try
            {
                Network network = networkAccessor.GetNetwork(networkId);
                User networkUser = userManager.GetUser(userId);
                ReorderEngine re = new ReorderEngine();
                List<int> networkIds = re.stringOrderToList(networkUser.networks);
                networkIds.Remove(networkId);
                string newUserNetworkList = null;
                foreach (int i in networkIds)
                {
                    if (newUserNetworkList == null)
                    {
                        newUserNetworkList = i.ToString();
                    }
                    else
                    {
                        newUserNetworkList += ("," + i.ToString());
                    }
                }
                networkUser.networks = newUserNetworkList;
                userManager.UpdateUser(networkUser);

                network.networkUsers.Remove(networkUser);
                Network returnNetwork = networkAccessor.UpdateNetwork(network);
                return GetNetworkJson(returnNetwork);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - RemoveNetworkUser", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network RemoveNetworkAdmin(int networkId, int adminUserId)
        {
            try
            {
                Network network = networkAccessor.GetNetwork(networkId);
                User adminUser = userManager.GetUser(adminUserId);
                ReorderEngine re = new ReorderEngine();
                List<int> networkIds = re.stringOrderToList(adminUser.networks);
                networkIds.Remove(networkId);
                string newUserNetworkList = null;
                foreach (int i in networkIds)
                {
                    if (newUserNetworkList == null)
                    {
                        newUserNetworkList = i.ToString();
                    }
                    else
                    {
                        newUserNetworkList += ("," + i.ToString());
                    }
                }
                adminUser.networks = newUserNetworkList;
                userManager.UpdateUser(adminUser);

                network.admins.Remove(adminUser);
                Network returnedNetwork = networkAccessor.UpdateNetwork(network);
                return GetNetworkJson(returnedNetwork);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - RemoveNetworkAdmin", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network GetNetworkJson(Network network)
        {
            try
            {
                if (network != null)
                {
                    JsonModels.Network networkJson = new JsonModels.Network();
                    networkJson.id = network.id;
                    networkJson.coverPicture = network.coverPicture;
                    networkJson.description = network.description;
                    networkJson.name = network.name;
                    networkJson.privacy = network.privacy;
                    networkJson.profileURL = network.profileURL;
                    if (network.GetType() == typeof(Network_TopNetwork))
                    {
                        Network_TopNetwork topNetwork = (Network_TopNetwork)network;
                        if (topNetwork.subNetworks != null)
                        {
                            List<JsonModels.NetworkShell> subNetShells = new List<JsonModels.NetworkShell>();
                            foreach (Network_SubNetwork subNetwork in topNetwork.subNetworks)
                            {
                                if (subNetwork != null)
                                {
                                    JsonModels.NetworkShell netShell = new JsonModels.NetworkShell();
                                    netShell.networkId = subNetwork.id;
                                    netShell.name = subNetwork.name;
                                    netShell.profileURL = subNetwork.profileURL;
                                    netShell.coverPicture = subNetwork.coverPicture;
                                    netShell.privacy = subNetwork.privacy;
                                    subNetShells.Add(netShell);
                                }
                            }
                            networkJson.subNetworks = subNetShells;
                            networkJson.parentNetwork = null;
                        }
                        else
                        {
                            networkJson.subNetworks = null;
                        }
                    }
                    else if (network.GetType() == typeof(Network_SubNetwork))
                    {
                        Network_SubNetwork subNetwork = (Network_SubNetwork)network;
                        if (subNetwork.groups != null)
                        {
                            List<JsonModels.NetworkShell> groupShells = new List<JsonModels.NetworkShell>();
                            foreach (Network_Group group in subNetwork.groups)
                            {
                                if (group != null)
                                {
                                    JsonModels.NetworkShell gShell = new JsonModels.NetworkShell();
                                    gShell.networkId = subNetwork.id;
                                    gShell.name = subNetwork.name;
                                    gShell.profileURL = subNetwork.profileURL;
                                    gShell.coverPicture = subNetwork.coverPicture;
                                    gShell.privacy = subNetwork.privacy;
                                    groupShells.Add(gShell);
                                }
                            }
                            networkJson.subNetworks = groupShells;

                            JsonModels.NetworkShell parentNetworkShell = new JsonModels.NetworkShell();
                            Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(subNetwork.Network_TopNetwork_Id);
                            parentNetworkShell.networkId = subNetwork.Network_TopNetwork_Id;
                            parentNetworkShell.name = topNet.name;
                            parentNetworkShell.profileURL = topNet.profileURL;
                            parentNetworkShell.coverPicture = topNet.coverPicture;
                            parentNetworkShell.privacy = topNet.privacy;
                            networkJson.parentNetwork = parentNetworkShell;
                        }
                        else
                        {
                            networkJson.subNetworks = null;

                            JsonModels.NetworkShell parentNetworkShell = new JsonModels.NetworkShell();
                            Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(subNetwork.Network_TopNetwork_Id);
                            parentNetworkShell.networkId = subNetwork.Network_TopNetwork_Id;
                            parentNetworkShell.name = topNet.name;
                            parentNetworkShell.profileURL = topNet.profileURL;
                            parentNetworkShell.coverPicture = topNet.coverPicture;
                            parentNetworkShell.privacy = topNet.privacy;
                            networkJson.parentNetwork = parentNetworkShell;
                        }

                    }
                    else if (network.GetType() == typeof(Network_Group))
                    {
                        Network_Group networkGroup = (Network_Group)network;
                        JsonModels.NetworkShell parentNetworkShell = new JsonModels.NetworkShell();
                        Network_SubNetwork subNet = (Network_SubNetwork)networkAccessor.GetNetwork(networkGroup.Network_SubNetwork_Id);
                        parentNetworkShell.networkId = networkGroup.Network_SubNetwork_Id;
                        parentNetworkShell.name = subNet.name;
                        parentNetworkShell.profileURL = subNet.profileURL;
                        parentNetworkShell.coverPicture = subNet.coverPicture;
                        parentNetworkShell.privacy = subNet.privacy;
                        networkJson.parentNetwork = parentNetworkShell;
                    }

                    if (network.admins != null)
                    {
                        List<JsonModels.NetworkUserShell> adminShells = new List<JsonModels.NetworkUserShell>();
                        foreach (User admin in network.admins)
                        {
                            if (admin != null)
                            {
                                JsonModels.NetworkUserShell adminJson = new JsonModels.NetworkUserShell();
                                adminJson.userId = admin.id;
                                adminJson.firstName = admin.firstName;
                                adminJson.lastName = admin.lastName;
                                adminJson.profileURL = admin.profileURL;
                                adminShells.Add(adminJson);
                            }
                        }
                        networkJson.admins = adminShells;
                    }
                    return networkJson;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - GetNetworkJson", ex.StackTrace);
                return null;
            }
        }
    }
}
