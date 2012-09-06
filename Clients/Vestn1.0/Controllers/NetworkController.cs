using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Accessor;
using Entity;
using Engine;
using System.IO;
using System.Drawing;

namespace UserClientMembers.Controllers
{
    public class NetworkController : BaseController
    {
        NetworkManager networkManager = new NetworkManager();
        LogAccessor logAccessor = new LogAccessor();
        AuthenticaitonEngine authenticationEngine = new AuthenticaitonEngine();
        UserManager userManager = new UserManager();

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string CreateNetwork(int adminUserId = -1)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                if (adminUserId < 0)
                {
                    return AddSuccessHeader(Serialize(networkManager.CreateNetwork()));
                }
                else
                {
                    return AddSuccessHeader(Serialize(networkManager.CreateNetwork(adminUserId)));
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - CreateNetwork", ex.StackTrace);
                return AddErrorHeader("Something went wrong while creating this network", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddChildNetwork(int networkId, string token, string networkName = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                User user = userManager.GetUser(userId);
                Network network = networkManager.GetNetwork(networkId);
                if (network != null)
                {
                    string type = network.GetType().Name;
                    if (network.GetType().Name.Contains("Network_TopNetwork"))
                    {
                        Network_TopNetwork topNet = (Network_TopNetwork)network;
                        if (topNet != null)
                        {

                            JsonModels.Network networkJson = networkManager.CreateSubNetwork(topNet.id, networkName);
                            if (networkJson != null)
                            {
                                return AddSuccessHeader(Serialize(networkJson));
                            }
                            else
                            {
                                return AddErrorHeader("An error occurred while creating this subnetwork", 1);
                            }

                        }
                        else
                        {
                            return AddErrorHeader("error fetching top level network", 1);
                        }
                    }
                    else if (network.GetType().Name.Contains("Network_SubNetwork"))
                    {
                        Network_SubNetwork subNet = (Network_SubNetwork)network;
                        if (subNet != null)
                        {
                            JsonModels.Network networkJson = networkManager.CreateGroupNetwork(subNet.id, networkName);
                            if (networkJson != null)
                            {
                                return AddSuccessHeader(Serialize(networkJson));
                            }
                            else
                            {
                                return AddErrorHeader("An error occurred while creating this group network", 1);
                            }
                        }
                        else
                        {
                            //error fetching subnetwork
                            return AddErrorHeader("error fetching subnetwork", 1);
                        }
                    }
                    else
                    {
                        //can only add child networks to top and sub networks
                        return AddErrorHeader("No additional Child Networks can be added", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("The network was not found in the database", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddChildNetwork", ex.StackTrace);
                return AddErrorHeader("Something went wrong while adding this child network", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddNetworkAdmin(int networkId, string adminEmail, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }

                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    string added = networkManager.AddNetworkAdmin(networkId, adminEmail);
                    if (added == "success")
                    {
                        User admin = userManager.GetUserByEmail(adminEmail);
                        if (admin != null)
                        {
                            JsonModels.NetworkUserShell uShell = new JsonModels.NetworkUserShell();
                            uShell.firstName = admin.firstName;
                            uShell.profileURL = admin.profileURL;
                            uShell.lastName = admin.lastName;
                            uShell.userId = admin.id;
                            if (admin.isPublic == 1)
                            {
                                uShell.visibility = "visible";
                            }
                            else
                            {
                                uShell.visibility = "hidden";
                            }
                            return AddSuccessHeader(Serialize(uShell));
                        }
                        else
                        {
                            return AddSuccessHeader("An email has been sent to "+adminEmail+" with instructions on creating a Vestn Account",true);
                        }
                    }
                    else if(added == "admin not found")
                    {
                        return AddErrorHeader("The administrator you attempted to add does not have a Vestn account. Please have your administrator create an account, then try adding them again", 1);
                    }
                    else if (added == "admin exists")
                    {
                        return AddErrorHeader("This user is already an administrator", 1);
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred while adding this administrator" , 1);
                    }
                    return AddErrorHeader("An error occurred while adding this administrator", 1);
                }
                else
                {
                    return AddErrorHeader("User must be an admin to add network administrators", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddNetworkAdmin", ex.StackTrace);
                return AddErrorHeader("something went wrong while adding this network administrator", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddNetworkUsers(int networkId, IEnumerable<string> userEmails, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    Network network = networkManager.GetNetwork(networkId);
                    JsonModels.Network networkJson = networkManager.AddNetworkUsers(network, userEmails);
                    if (networkJson != null)
                    {
                        JsonModels.NetworkUsers netUsers = networkManager.GetNetworkUsers(network.id);
                        return AddSuccessHeader(Serialize(netUsers));
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred adding network users", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("User must be a network administrator to add users", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddNetworkUsers", ex.StackTrace);
                return AddErrorHeader("something went wrong while adding network users", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetNetworkUsers(int networkId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                JsonModels.NetworkUsers netUserJson = networkManager.GetNetworkUsers(networkId);
                if (netUserJson != null)
                {
                    return AddSuccessHeader(Serialize(netUserJson));
                }
                else
                {
                    return AddErrorHeader("An error occurred fetching the network users", 1);
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkUsers", ex.StackTrace);
                return AddErrorHeader("something went wrong while retrieving network users", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetNetworkInformation(int networkId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                Network network = networkManager.GetNetwork(networkId);
                JsonModels.Network networkJson = networkManager.GetNetworkJson(network);
                if (network.GetType().Name.Contains("Network_TopNetwork"))
                {
                    foreach (User u in network.admins)
                    {
                        if (u.id == userId)
                        {
                            networkJson.role = "admin";
                        }
                    }
                    foreach (User v in network.networkUsers)
                    {
                        if (v.id == userId)
                        {
                            if (networkJson.role == null)
                            {
                                networkJson.role = "member";
                            }
                        }
                    }
                }
                else if (network.GetType().Name.Contains("Network_SubNetwork"))
                {
                    Network_SubNetwork sn = (Network_SubNetwork)network;
                    foreach (User u in network.admins)
                    {
                        if (u.id == userId)
                        {
                            networkJson.role = "admin";
                        }
                    }
                    if (networkJson.role == null)
                    {
                        foreach (User u in sn.Network_TopNetwork.admins)
                        {
                            if (u.id == userId)
                            {
                                networkJson.role = "admin";
                            }
                        }
                    }
                    if (networkJson.role == null)
                    {
                        foreach (User v in network.networkUsers)
                        {
                            if (v.id == userId)
                            {
                                networkJson.role = "member";
                            }
                        }
                    }
                }
                else
                {
                    Network_Group gn = (Network_Group)network;
                    foreach (User u in network.admins)
                    {
                        if (u.id == userId)
                        {
                            networkJson.role = "admin";
                        }
                    }
                    if (networkJson.role == null)
                    {
                        foreach (User u in gn.Network_SubNetwork.admins)
                        {
                            if (u.id == userId)
                            {
                                networkJson.role = "admin";
                            }
                        }
                    }
                    if (networkJson.role == null)
                    {
                        foreach (User u in gn.Network_SubNetwork.Network_TopNetwork.admins)
                        {
                            if (u.id == userId)
                            {
                                networkJson.role = "admin";
                            }
                        }
                    }
                    if (networkJson.role == null)
                    {
                        foreach (User v in network.networkUsers)
                        {
                            if (v.id == userId)
                            {
                                networkJson.role = "member";
                            }
                        }
                    }
                }
                if (networkJson != null)
                {
                    return AddSuccessHeader(Serialize(networkJson));
                }
                else
                {
                    return AddErrorHeader("An error occurred while retrieving the network information", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkInformation", ex.StackTrace);
                return AddErrorHeader("something went wrong while getting the network information", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetNetworkJoinCode(int networkId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    Network network = networkManager.GetNetwork(networkId);
                    if (network != null)
                    {
                        if (network.networkIdentifier == null)
                        {
                            string identifier = networkManager.SetNetworkIdentifier(network);
                            if (identifier != null)
                            {
                                return AddSuccessHeader(identifier, true);
                            }
                            else
                            {
                                return AddErrorHeader("An Error Occurred", 1);
                            }
                        }
                        else
                        {
                            return AddSuccessHeader(network.networkIdentifier, true);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Network id not found", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Must be network admin to access join code", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkJoinCode", ex.StackTrace);
                return AddErrorHeader("something went wrong while getting the network join code", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string DeactivateNetworkJoinCode(int networkId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                Network network = networkManager.GetNetwork(networkId);
                if (network != null)
                {
                    if (networkManager.IsNetworkAdmin(networkId, userId))
                    {
                        if (networkManager.SetNetworkIdentifier(network) != null)
                        {
                            return AddSuccessHeader("Identifier de-activated", true);
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred processing your request", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("You must be a network administrator to de-activate the network join code" , 3);
                    }
                }
                else
                {
                    return AddErrorHeader("The network was not found", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkJoinCode", ex.StackTrace);
                return AddErrorHeader("something went wrong while de-activating the network join code", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string GetNetworkByURL(string networkURL, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }

                Network network = networkManager.GetNetworkByUrl(networkURL);
                if (network == null)
                {
                    return AddErrorHeader("Network with networkURL was not found", 1);
                }
                else
                {
                    return AddSuccessHeader(Serialize(networkManager.GetNetworkJson(network)));
                }

            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while attmpting to retreive this networkURL", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateNetworkURL(int networkId, string desiredURL, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    string status = networkManager.UpdateNetworkUrl(networkId, desiredURL);
                    if (status != null)
                    {
                        if (status == "success")
                        {
                            return AddSuccessHeader("Network URL updated successfully");
                        }
                        else if (status == "URL taken")
                        {
                            return AddErrorHeader("Network URL is already taken.", 1);
                        }
                        else
                        {
                            return AddErrorHeader("error", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred while attempting to update this networkURL", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("User must be network administrator to update the URL", 3);
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while attempting to update this networkURL", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string UpdateNetworkModel(IEnumerable<JsonModels.Network> network, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("You are not authenticated, please log in!", 2);
                }
                if (network != null)
                {
                    JsonModels.Network netFromJson = network.FirstOrDefault();
                    if (networkManager.IsNetworkAdmin(netFromJson.id, userId))
                    {
                        JsonModels.Network updatedNetworkJson = networkManager.UpdateNetworkModel(netFromJson);
                        if (updatedNetworkJson != null)
                        {
                            return AddSuccessHeader(Serialize(updatedNetworkJson));
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred while updating the network model", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("User must be network admin to update the network model", 3);
                    }
                }
                else
                {
                    return AddErrorHeader("The network model passed in was unable to be parsed", 1);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - UpdateNetworkModel", ex.StackTrace);
                return AddErrorHeader("something went wrong while updating the network information", 1);
            }
        }

        [AcceptVerbs("OPTIONS","POST")]
        [AllowCrossSiteJson]
        public string UpdateNetworkCoverPicture(int networkId, string token, string qqfile = null)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("Not Authenticated", 2);
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    if (qqfile != null || Request.Files.Count == 1)
                    {
                        var length = Request.ContentLength;
                        var bytes = new byte[length];
                        Stream s;
                        if (qqfile == "System.Web.HttpPostedFileWrapper")
                        {
                            qqfile = Request.Files[0].FileName;
                            s = Request.Files[0].InputStream;
                        }
                        else
                        {
                            Request.InputStream.Read(bytes, 0, length);
                            s = new MemoryStream(bytes);
                        }
                        try
                        {
                            Bitmap test = new Bitmap(s);
                            if (test.Height < 170 || test.Width < 170)
                            {
                                return AddErrorHeader("The cover picture must be at least 170px wide and 170px tall", 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            return AddErrorHeader("The image is invalid",1);
                        }

                        string returnPic = networkManager.UpdateCoverPicture(networkId, s);

                        return AddSuccessHeader(returnPic, true);
                    }
                    else
                    {
                        return AddErrorHeader("No files posted to server", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Not Authorized", 3);
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - UpdateNetworkCoverPicture", ex.StackTrace);
                return AddErrorHeader("something went wrong while updating the NetworkCoverPicture", 1);
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string RemoveNetworkUser(int networkId, int networkUserId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("Not Authenticated", 2);
                }
                if (networkId > 0 && networkUserId > 0)
                {
                    if (networkManager.IsNetworkAdmin(networkId, userId))
                    {
                        bool removed = networkManager.DeleteNetworkUser(networkId, networkUserId);
                        if (removed)
                        {
                            return AddSuccessHeader("User removed successfully");
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred while removing this user", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Not Authorized", 3);
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid integers for networkId or networkUserId", 1);
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - RemoveNetworkUser", ex.StackTrace);
                return AddErrorHeader("something went wrong while removing the network user", 1);
            }

        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string RemoveNetworkAdmin(int networkId, int networkAdminId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("Not Authenticated", 2);
                }
                if (userId == networkAdminId)
                {
                    return AddErrorHeader("You probably don't want to remove yourself as an administrator", 1);
                }
                if (networkId > 0 && networkAdminId > 0)
                {
                    if (networkManager.IsNetworkAdmin(networkId, userId))
                    {
                        bool removed = networkManager.DeleteNetworkAdmin(networkId, networkAdminId);
                        if (removed)
                        {
                            return AddSuccessHeader("Admin removed successfully");
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred while removing this admin", 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Not Authorized", 3);
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid integers for networkId or networkAdminId", 1);
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - RemoveNetworkAdmin", ex.StackTrace);
                return AddErrorHeader("something went wrong while removing the network admin", 1);
            }

        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string RemoveChildNetwork(int networkId, int childNetworkId, string token)
        {
            if (Request.RequestType.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))  //This is a preflight request
            {
                return null;
            }
            try
            {
                int userId = authenticationEngine.authenticate(token);
                if (userId < 0)
                {
                    return AddErrorHeader("Not Authenticated", 2);
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    Network network = networkManager.GetNetwork(networkId);
                    if (network.GetType().Name.Contains("Network_TopNetwork"))
                    {
                        Network_TopNetwork tn = (Network_TopNetwork)network;
                        Network_SubNetwork sn = (Network_SubNetwork)networkManager.GetNetwork(childNetworkId);
                        string status = networkManager.DeleteSubNetwork(tn, sn);
                        if (status == "Success")
                        {
                            return AddSuccessHeader("SubNetwork removed");
                        }
                        else
                        {
                            return AddErrorHeader(status, 1);
                        }
                    }
                    else if (network.GetType().Name.Contains("Network_SubNetwork"))
                    {
                        Network_SubNetwork sn = (Network_SubNetwork)network;
                        Network_Group gn = (Network_Group)networkManager.GetNetwork(childNetworkId);
                        string status = networkManager.DeleteGroupNetwork(sn, gn);
                        if (status == "Success")
                        {
                            return AddSuccessHeader("GroupNetwork removed");
                        }
                        else
                        {
                            return AddErrorHeader(status, 1);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("This will never get hit", 1);
                    }
                }
                else
                {
                    return AddErrorHeader("Not Authorized", 3);
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - RemoveChildNetwork", ex.StackTrace);
                return AddErrorHeader("something went wrong while removing the network admin", 1);
            }

        }

    }
}
