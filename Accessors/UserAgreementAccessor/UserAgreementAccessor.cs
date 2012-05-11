using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entity;
using System.Data.Entity;
using System.Data;
using System.Data.Linq;

namespace Accessor
{
    public class UserAgreementAccessor
    {
        public UserAgreement CreateAgreement(DateTime agreementTime, string userName, string value, string IPAddress)
        {
            UserAgreement userAgreement = new UserAgreement();
            userAgreement.agreementTime = agreementTime;
            userAgreement.userName = userName;
            userAgreement.value = value;
            userAgreement.IPAddress = IPAddress;

            try
            {
                VestnDB db = new VestnDB();
                db.userAgreements.Add(userAgreement);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                LogAccessor logAccessor = new LogAccessor();
                logAccessor.CreateLog(DateTime.Now, this.GetType().ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name.ToString(), e.ToString());
            }
            return userAgreement;
        }

    }
}
