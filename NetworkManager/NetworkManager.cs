using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessor;
using Entity;
using Manager;
using Engine;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Diagnostics;


namespace Manager
{
    public class NetworkManager
    {
        NetworkAccessor networkAccessor = new NetworkAccessor();
        LogAccessor logAccessor = new LogAccessor();
        UserManager userManager = new UserManager();
        

        public JsonModels.Network CreateNetwork(int adminUserId = -1)
        {
            try
            {
                Network_TopNetwork newNetwork = new Network_TopNetwork();
                if (adminUserId < 0)
                {
                    Network returnNetwork = networkAccessor.CreateNetwork(newNetwork);
                    return GetNetworkJson(returnNetwork);
                }
                else
                {
                    Network returnNetwork = networkAccessor.CreateNetwork(newNetwork);

                    //User networkAdmin = userManager.GetUser(adminUserId);
                    bool adminAdded = networkAccessor.AddAdmin(returnNetwork.id, adminUserId);
                    return GetNetworkJson(returnNetwork);
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - CreateNetwork", ex.StackTrace);
                return null;
            }
        }

        public JsonModels.Network CreateSubNetwork(int topNetworkId, string name = null)
        {
            try
            {
                Network_TopNetwork topNet = (Network_TopNetwork)networkAccessor.GetNetwork(topNetworkId);
                Network_SubNetwork returnNetwork;
                if (topNet != null)
                {
                    Network_SubNetwork newSubNetwork = new Network_SubNetwork();
                    newSubNetwork.name = name;

                    returnNetwork = (Network_SubNetwork)networkAccessor.CreateNetwork(newSubNetwork);

                   bool created = networkAccessor.AddSubNetwork(topNet.id, returnNetwork.id);

                   if (created)
                   {
                       return GetNetworkJson(networkAccessor.GetSubNetwork(returnNetwork.id));
                   }
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

        public JsonModels.Network CreateGroupNetwork(int subNetworkId, string name = null)
        {
            try
            {
                Network_SubNetwork subNet = (Network_SubNetwork)networkAccessor.GetNetwork(subNetworkId);
                Network_Group returnNetwork;
                if (subNet != null)
                {
                    Network_Group newGroup = new Network_Group();
                    newGroup.name = name;
                    //newGroup.SubNetworkId = subNetworkId;
                    returnNetwork = (Network_Group)networkAccessor.CreateNetwork(newGroup);

                    bool created = networkAccessor.AddGroupNetwork(subNet.id, returnNetwork.id);

                    if (created)
                    {
                        return GetNetworkJson(networkAccessor.GetGroupNetwork(returnNetwork.id));
                    }
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
                    networkUsersJson.users = new List<JsonModels.NetworkUserShell>();
                    foreach (User u in network.networkUsers)
                    {
                        if (u != null)
                        {
                            JsonModels.NetworkUserShell userShell = new JsonModels.NetworkUserShell();
                            userShell.userId = u.id;
                            userShell.firstName = u.firstName;
                            userShell.lastName = u.lastName;
                            userShell.profileURL = u.profileURL;
                            userShell.pictureLocation = u.networkPictureThumbnail;
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

                        Network returnNetwork = networkAccessor.UpdateNetworkInformation(originalNetwork);
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

        public string SetNetworkIdentifier(Network network)
        {
            try
            {
                Random r = new Random();
                string num = r.Next(100000, 999999).ToString();
                string newIdentifier = num + network.id.ToString();

                bool set = networkAccessor.UpdateNetworkIdentifier(network.id, newIdentifier);
                if (set)
                {
                    return newIdentifier;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - SetNetworkIdentifier", ex.StackTrace);
                return null;
            }
        }

        public Network GetNetworkByIdentifier(string identifier)
        {
            try
            {
                return networkAccessor.GetNetworkByIdentifier(identifier);
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - GetNetworkByIdentifier", ex.StackTrace);
                return null;
            }
        }

        //public bool DeActivateIdentifier(Network network)
        //{
        //}

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
                                    bool added = networkAccessor.AddNetworkUser(network.id, addUser.id);
                                    if (network.GetType().Name.Contains("Network_SubNetwork"))
                                    {
                                        Network_SubNetwork subNet = (Network_SubNetwork)network;
                                        bool added2 = networkAccessor.AddNetworkUser(subNet.Network_TopNetwork.id, addUser.id);
                                    }
                                    else if (network.GetType().Name.Contains("Network_Group"))
                                    {
                                        Network_Group groupNet = (Network_Group)network;
                                        bool added3 = networkAccessor.AddNetworkUser(groupNet.Network_SubNetwork.id, addUser.id);
                                        bool added4 = networkAccessor.AddNetworkUser(groupNet.Network_SubNetwork.Network_TopNetwork.id, addUser.id);
                                    }
                                }
                                else
                                {
                                    CommunicationManager communicationManager = new CommunicationManager();
                                    if (network.networkIdentifier == null)
                                    {
                                        string identifier = SetNetworkIdentifier(network);
                                        communicationManager.SendRegisterNetworkInvite(email, identifier);
                                    }
                                    else
                                    {
                                        communicationManager.SendRegisterNetworkInvite(email, network.networkIdentifier);
                                    }
                                }
                            }
                        }
                        return GetNetworkJson(networkAccessor.GetNetwork(network.id));
                    }
                    else
                    {
                        //no emails
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

        public bool AddNetworkAdmin(int networkId, string adminEmail)
        {
            try
            {

                if (adminEmail != null)
                {
                    User admin = userManager.GetUserByEmail(adminEmail);
                    if (admin != null)
                    {
                        Network network = networkAccessor.GetNetwork(networkId);
                        if (network != null)
                        {
                            if (network.admins.Contains(admin))
                            {
                                //User is already an admin of this network
                                return false;
                            }
                            else
                            {
                                bool added = networkAccessor.AddAdmin(networkId, admin.id);
                                return added;
                            }
                        }
                        else
                        {
                            //Network not found in database
                            return false;
                        }
                    }
                    else
                    {
                        //TODO when email complete
                        //email does not exist in system, send email invitation with network admin creds
                        return false;
                    }
                }
                else
                {
                    //need the email
                    return false;
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - AddNetworkAdmin", ex.StackTrace);
                return false;
            }
        }

        public string UpdateCoverPicture(int networkId, Stream coverPicture)
        {
            try
            {
                string messageQueueName = "uploadqueue"; //queue name must be in lower case
                CloudQueueClient queueClient;
                CloudQueue queue;
                CloudStorageAccount storageAccount;
                BlobStorageAccessor blobStorageAccessor = new BlobStorageAccessor();
                storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BlobConnectionString"));
                queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference(messageQueueName);
                queue.CreateIfNotExist();

                string fileName = Guid.NewGuid().ToString();
                string artifactURL = string.Format("{0}{1}", fileName, ".jpeg");

                string blobReference = blobStorageAccessor.uploadImage(coverPicture, false).ToString();
                string coverPictureLocation = "http://vestnstaging.blob.core.windows.net/thumbnails/" + artifactURL;
                CloudQueueMessage message = new CloudQueueMessage(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", blobReference, networkId, "thumbnail", "Network", 170, 170, "", artifactURL));
                queue.AddMessage(message);

                bool updated = networkAccessor.UpdateNetworkCoverPicture(networkId, coverPictureLocation);
                if (updated)
                {
                    return coverPictureLocation;
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "Network Manager - AddNetworkAdmin", ex.StackTrace);
                return "Error";
            }
        }

        public bool IsNetworkAdmin(int networkId, int userId)
        {
            Network network = networkAccessor.GetNetwork(networkId);
            if (network.GetType().Name.Contains("Network_TopNetwork"))
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
                }
            }
            else if (network.GetType().Name.Contains("Network_SubNetwork"))
            {
                Network_SubNetwork subNet = (Network_SubNetwork)network;
                Network_TopNetwork topNet = subNet.Network_TopNetwork;
                if (subNet.admins.Count > 0)
                {
                    foreach (User u in subNet.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
                if (topNet.admins.Count > 0)
                {
                    foreach (User u in topNet.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (network.GetType().Name.Contains("Network_Group"))
            {
                Network_Group groupNet = (Network_Group)network;
                Network_SubNetwork subNet = groupNet.Network_SubNetwork;
                Network_TopNetwork topNet = groupNet.Network_SubNetwork.Network_TopNetwork;

                if (subNet.admins != null)
                {
                    foreach (User u in subNet.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
                if (topNet.admins != null)
                {
                    foreach (User u in topNet.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
                if (groupNet.admins != null)
                {
                    foreach (User u in groupNet.admins)
                    {
                        if (u != null)
                        {
                            if (u.id == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool DeleteNetworkUser(int networkId, int userId)
        {
            try
            {
                if (networkAccessor.DeleteNetworkUser(networkId, userId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteNetworkAdmin(int networkId, int adminId)
        {
            try
            {
                if (networkAccessor.DeleteNetworkAdmin(networkId, adminId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string DeleteSubNetwork(Network_TopNetwork topNetwork, Network_SubNetwork subNetwork)
        {
            try
            {
                if (subNetwork.groups.Count > 0)
                {
                    return "SubNetwork contains Groups";
                }
                if (subNetwork.admins.Count > 0)
                {
                    return "SubNetwork contains Admins";
                }
                if (subNetwork.networkUsers.Count > 0)
                {
                    return "Subnetwork contains Members";
                }
                if(networkAccessor.DeleteSubNetwork(topNetwork.id, subNetwork.id))
                {
                    return "Success";
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public string DeleteGroupNetwork(Network_SubNetwork subNetwork, Network_Group groupNetwork)
        {
            try
            {
                if (groupNetwork.admins.Count > 0)
                {
                    return "GroupNetwork contains Admins";
                }
                if (groupNetwork.networkUsers.Count > 0)
                {
                    return "GroupNetwork contains Members";
                }
                if (networkAccessor.DeleteGroupNetwork(subNetwork.id, groupNetwork.id))
                {
                    return "Success";
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }

        public Network GetNetworkByUrl(string networkURL)
        {
            try
            {
                Network network = networkAccessor.GetNetworkByUrl(networkURL);
                return network;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string UpdateNetworkUrl(int networkId, string desiredURL)
        {
            try
            {
                if (networkAccessor.IsNetworkUrlAvailable(desiredURL))
                {
                    Network network = networkAccessor.GetNetwork(networkId);
                    network.profileURL = desiredURL;
                    if (networkAccessor.UpdateNetworkUrl(network) != null)
                    {
                        return "success";
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return "URL taken";
                }

            }
            catch (Exception ex)
            {
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
                    if (network.GetType().Name.Contains("Network_TopNetwork"))
                    {
                        Network_TopNetwork topNetwork = networkAccessor.GetTopNetwork(network.id);
                        if (topNetwork.subNetworks.Count > 0)
                        {
                            List<JsonModels.NetworkShell> subNetShells = new List<JsonModels.NetworkShell>();
                            foreach (Network_SubNetwork subNetwork in topNetwork.subNetworks)
                            {
                                if (subNetwork != null)
                                {
                                    JsonModels.NetworkShell netShell = new JsonModels.NetworkShell();
                                    netShell.id = subNetwork.id;
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
                    else if (network.GetType().Name.Contains("Network_SubNetwork"))
                    {
                        Network_SubNetwork subNetwork = (Network_SubNetwork)network;
                        if (subNetwork.groups.Count > 0)
                        {
                            List<JsonModels.NetworkShell> groupShells = new List<JsonModels.NetworkShell>();
                            foreach (Network_Group group in subNetwork.groups)
                            {
                                if (group != null)
                                {
                                    JsonModels.NetworkShell gShell = new JsonModels.NetworkShell();
                                    gShell.id = group.id;
                                    gShell.name = group.name;
                                    gShell.profileURL = group.profileURL;
                                    gShell.coverPicture = group.coverPicture;
                                    gShell.privacy = group.privacy;
                                    groupShells.Add(gShell);
                                }
                            }
                            networkJson.subNetworks = groupShells;

                            JsonModels.NetworkShell parentNetworkShell = new JsonModels.NetworkShell();
                            Network_TopNetwork topNet = subNetwork.Network_TopNetwork;

                            parentNetworkShell.id = topNet.id;
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
                            Network_TopNetwork topNet = subNetwork.Network_TopNetwork;

                            parentNetworkShell.id = topNet.id;
                            parentNetworkShell.name = topNet.name;
                            parentNetworkShell.profileURL = topNet.profileURL;
                            parentNetworkShell.coverPicture = topNet.coverPicture;
                            parentNetworkShell.privacy = topNet.privacy;
                            networkJson.parentNetwork = parentNetworkShell;
                        }

                    }
                    else if (network.GetType().Name.Contains("Network_Group"))
                    {
                        Network_Group networkGroup = (Network_Group)network;
                        JsonModels.NetworkShell parentNetworkShell = new JsonModels.NetworkShell();
                        Network_SubNetwork subNet = networkGroup.Network_SubNetwork;
                        parentNetworkShell.id = subNet.id;
                        parentNetworkShell.name = subNet.name;
                        parentNetworkShell.profileURL = subNet.profileURL;
                        parentNetworkShell.coverPicture = subNet.coverPicture;
                        parentNetworkShell.privacy = subNet.privacy;
                        networkJson.parentNetwork = parentNetworkShell;
                    }

                    if (network.admins.Count > 0)
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
                                adminJson.pictureLocation = admin.networkPictureThumbnail;
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
