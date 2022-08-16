using CareViewNotification;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [TestCategory("Validation")]
        public void TestMethod1()
        {
            NotificationManager nm = new NotificationManager();
            //we need to create and fill the ClientNotification object and then pass it to NotificationManager send
            NotificationInfo clientinfoinfo = new NotificationInfo("This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters");
            Client cli = new Client("Fred", "Smith");
            cli.Email = new Channel(false, "");
            cli.SMS = new Channel(true, "0421645737");
            cli.MobileApp = new Channel(false, "");

            ClientNotification clientinfo = new ClientNotification(clientinfoinfo, new List<Client> { cli }, new List<Client>());

            nm.NotifyClients(clientinfo);

            Assert.AreEqual(SucFail.Fail, cli.Valid);
        }

        [TestMethod]
        [TestCategory("Validation")]
        ///check for validation Fail when Email Address is invalid
        public void TestMethod2()
        {
            NotificationManager nm = new NotificationManager();
            //we need to create and fill the ClientNotification object and then pass it to NotificationManager send
            NotificationInfo clientinfoinfo = new NotificationInfo("Sample messag");

            Client cli = new Client("Tim", "Hams");
            cli.Email = new Channel(true, "timhamsgmailcom");
            cli.SMS = new Channel(false, "");
            cli.MobileApp = new Channel(false, "");

            ClientNotification clientinfo = new ClientNotification(clientinfoinfo, new List<Client> { cli }, new List<Client>());
            //first client

            nm.NotifyClients(clientinfo);
            //            

            Assert.AreEqual(SucFail.Fail, cli.Valid);
        }


        [TestMethod]
        [TestCategory("Validation")]
        ///Check for validation fail when no clients have been included in the notification
        public void TestMethod3()
        {
            NotificationManager nm = new NotificationManager();
            //we need to create and fill the ClientNotification object and then pass it to NotificationManager send
            NotificationInfo clientinfoinfo = new NotificationInfo("This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters");


            ClientNotification clientinfo = new ClientNotification(clientinfoinfo, new List<Client>(), new List<Client>());//no clients

            SucFail res = nm.NotifyClients(clientinfo);

            Assert.AreEqual(SucFail.Fail, res);//should be a fail return as no clients were include in the notification
        }

        [TestMethod]
        [TestCategory("Validation")]
        ///check for validation fail where client has no email, no sms, no mobile app
        public void TestMethod4()
        {
            NotificationManager nm = new NotificationManager();
            //we need to create and fill the ClientNotification object and then pass it to NotificationManager send
            NotificationInfo clientinfoinfo = new NotificationInfo("This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters. This statement is much longer than 100 characters");
            Client cli = new Client("John", "White");
            cli.Email = new Channel(false, "");
            cli.SMS = new Channel(false, "");
            cli.MobileApp = new Channel(false, "");

            ClientNotification clientinfo = new ClientNotification(clientinfoinfo, new List<Client> { cli }, new List<Client>());//no clients

            SucFail res = nm.NotifyClients(clientinfo);

            Assert.AreEqual(cli.Valid, res);//should be a fail return as no clients were include in the notification
        }

    }
}