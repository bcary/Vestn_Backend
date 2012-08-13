using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager;
using Accessor;
using Entity;
using Engine;

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
                return AddErrorHeader("Something went wrong while creating this network");
            }
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [AllowCrossSiteJson]
        public string AddChildNetwork(int networkId, string token)
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
                    return AddErrorHeader("You are not authenticated, please log in!");
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
                            JsonModels.Network networkJson = networkManager.CreateSubNetwork(topNet.id);
                            if (networkJson != null)
                            {
                                return AddSuccessHeader(Serialize(networkJson));
                            }
                            else
                            {
                                return AddErrorHeader("An error occurred while creating this subnetwork");
                            }
                        }
                        else
                        {
                            return AddErrorHeader("error fetching top level network");
                        }
                    }
                    else if (network.GetType().Name.Contains("Network_SubNetwork"))
                    {
                        Network_SubNetwork subNet = (Network_SubNetwork)network;
                        if (subNet != null)
                        {
                            JsonModels.Network networkJson = networkManager.CreateGroupNetwork(subNet.id);
                            if (networkJson != null)
                            {
                                return AddSuccessHeader(Serialize(networkJson));
                            }
                            else
                            {
                                return AddErrorHeader("An error occurred while creating this group network");
                            }
                        }
                        else
                        {
                            //error fetching subnetwork
                            return AddErrorHeader("error fetching subnetwork");
                        }
                    }
                    else
                    {
                        //can only add child networks to top and sub networks
                        return AddErrorHeader("No additional Child Networks can be added");
                    }
                }
                else
                {
                    return AddErrorHeader("The network was not found in the database");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddChildNetwork", ex.StackTrace);
                return AddErrorHeader("Something went wrong while adding this child network");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }

                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    bool added = networkManager.AddNetworkAdmin(networkId, adminEmail);
                    if (added)
                    {
                        return AddSuccessHeader("Admin Successfully Added", true);
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred while adding the network administrator");
                    }
                }
                else
                {
                    return AddErrorHeader("User must be an admin to add network administrators");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddNetworkAdmin", ex.StackTrace);
                return AddErrorHeader("something went wrong while adding this network administrator");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (networkManager.IsNetworkAdmin(networkId, userId))
                {
                    Network network = networkManager.GetNetwork(networkId);
                    JsonModels.Network networkJson = networkManager.AddNetworkUsers(network, userEmails);
                    if (networkJson != null)
                    {
                        return AddSuccessHeader(Serialize(networkJson));
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred adding network users");
                    }
                }
                else
                {
                    return AddErrorHeader("User must be a network administrator to add users");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - AddNetworkUsers", ex.StackTrace);
                return AddErrorHeader("something went wrong while adding network users");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                JsonModels.NetworkUsers netUserJson = networkManager.GetNetworkUsers(networkId);
                if (netUserJson != null)
                {
                    return AddSuccessHeader(Serialize(netUserJson));
                }
                else
                {
                    return AddErrorHeader("An error occurred fetching the network users");
                }
            }
            catch(Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkUsers", ex.StackTrace);
                return AddErrorHeader("something went wrong while retrieving network users");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                Network network = networkManager.GetNetwork(networkId);
                JsonModels.Network networkJson = networkManager.GetNetworkJson(network);
                if (networkJson != null)
                {
                    return AddSuccessHeader(Serialize(networkJson));
                }
                else
                {
                    return AddErrorHeader("An error occurred while retrieving the network information");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkInformation", ex.StackTrace);
                return AddErrorHeader("something went wrong while getting the network information");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
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
                                return AddErrorHeader("An Error Occurred");
                            }
                        }
                        else
                        {
                            return AddSuccessHeader(network.networkIdentifier, true);
                        }
                    }
                    else
                    {
                        return AddErrorHeader("Network id not found");
                    }
                }
                else
                {
                    return AddErrorHeader("Must be network admin to access join code");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - GetNetworkJoinCode", ex.StackTrace);
                return AddErrorHeader("something went wrong while getting the network join code");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }

                Network network = networkManager.GetNetworkByUrl(networkURL);
                if (network == null)
                {
                    return AddErrorHeader("Network with networkURL was not found");
                }
                else
                {
                    return AddSuccessHeader(Serialize(networkManager.GetNetworkJson(network)));
                }

            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while attmpting to retreive this networkURL");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
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
                            return AddErrorHeader("Network URL is already taken.");
                        }
                        else
                        {
                            return AddErrorHeader("error");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("An error occurred while attempting to update this networkURL");
                    }
                }
                else
                {
                    return AddErrorHeader("User must be network administrator to update the URL");
                }
            }
            catch (Exception ex)
            {
                return AddErrorHeader("Something went wrong while attempting to update this networkURL");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
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
                            return AddErrorHeader("An error occurred while updating the network model");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("User must be network admin to update the network model");
                    }
                }
                else
                {
                    return AddErrorHeader("The network model passed in was unable to be parsed");
                }
            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - UpdateNetworkModel", ex.StackTrace);
                return AddErrorHeader("something went wrong while updating the network information");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (networkId > 0 && networkUserId > 0)
                {
                    if (networkManager.IsNetworkAdmin(networkId, userId))
                    {
                        JsonModels.Network networkJson = networkManager.RemoveNetworkUser(networkId, networkUserId);
                        if (networkJson != null)
                        {
                            return AddSuccessHeader(Serialize(networkJson));
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred while removing this user");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("User must be network administrator to remove users from the network");
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid integers for networkId or networkUserId");
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - RemoveNetworkUser", ex.StackTrace);
                return AddErrorHeader("something went wrong while removing the network user");
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
                    return AddErrorHeader("You are not authenticated, please log in!");
                }
                if (networkId > 0 && networkAdminId > 0)
                {
                    if (networkManager.IsNetworkAdmin(networkId, userId))
                    {
                        JsonModels.Network networkJson = networkManager.RemoveNetworkAdmin(networkId, networkAdminId);
                        if (networkJson != null)
                        {
                            return AddSuccessHeader(Serialize(networkJson));
                        }
                        else
                        {
                            return AddErrorHeader("An error occurred while removing this admin");
                        }
                    }
                    else
                    {
                        return AddErrorHeader("User must be network administrator to remove admins from the network");
                    }
                }
                else
                {
                    return AddErrorHeader("Invalid integers for networkId or networkAdminId");
                }

            }
            catch (Exception ex)
            {
                logAccessor.CreateLog(DateTime.Now, "NetworkController - RemoveNetworkAdmin", ex.StackTrace);
                return AddErrorHeader("something went wrong while removing the network admin");
            }

        }

    }
}
