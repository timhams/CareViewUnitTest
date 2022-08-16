using System.Net;
using System.Net.Mail;

namespace CareViewNotification
{
    public class Client
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Channel Email { get; set; }
        public Channel SMS { get; set; }
        public Channel MobileApp { get; set; }

        public Client(string _FirstName, string _LastName)
        {
            this.FirstName = _FirstName;
            this.LastName = _LastName;
            this.Email = new Channel(false, "");
            this.SMS = new Channel(false, "");
            this.MobileApp = new Channel(false, "");
            this.FailMessage = "";
        }

        public SucFail SucceedFail { get; set; }
        public SucFail Valid { get; set; }

        public string FailMessage { get; set; }


    }


    public enum SucFail
    {
        Fail = 0,
        Success = 1
    }

    ///Channel Object - Email, SMS, Mobile app - operator phone call, postal mail, carrier pidgeon could also be added as additional channels -> addressing extensiblity
    public class Channel
    {
        public bool Enable { get; set; }
        public string Address { get; set; }

        public Channel(bool _Enable, string _Address)
        {
            this.Enable = _Enable;
            this.Address = _Address;
        }

    }

    //we are notifying clients with and text and an a optional attachment
    public class NotificationInfo
    {
        public string Body { get; set; }
        public Attachment? InvoiceAttachment { get; set; }

        public NotificationInfo(string _Body)
        {
            this.Body = _Body;

        }
    }

    /// <summary>
    /// this object contains everything for the communication notification to be send.   It contains the message (Notification Info) <summary>
    /// it also has the list of primary clients and cc clients 
    /// </summary>
    public class ClientNotification
    {
        public ClientNotification(NotificationInfo _info, List<Client> _primaryclient, List<Client> _ccclient)
        {
            this.info = _info;
            this.clients = _primaryclient;
            this.ccclients = _ccclient;
        }
        public List<Client> clients { get; set; }
        public List<Client> ccclients { get; set; }
        public NotificationInfo info { get; set; }

    }

    //this manages the send and receiving of all notifications
    public class NotificationManager
    {
        List<NotificationChannel> channels = new List<NotificationChannel> { new EmailCom(), new SMSCom(), new MobileAppCom() };

        public SucFail NotifyClients(ClientNotification clientinfo)
        {
            if (clientinfo.clients.Count == 0)
                return SucFail.Fail;//no clients
            //notify all clients
            foreach (var cli in clientinfo.clients)
            {
                cli.Valid = SucFail.Success;
                cli.SucceedFail = SucFail.Success;
                bool hasNotificationChannel = false;
                foreach (var chan in channels)
                {
                    chan.Validate(cli, clientinfo.info);
                    if (chan.ActiveNotifcationChanel(cli))
                        hasNotificationChannel = true;
                    if (cli.Valid == SucFail.Success)
                        chan.Send(cli, clientinfo.info);
                }
                if (hasNotificationChannel)
                {
                    cli.Valid = SucFail.Fail;
                    cli.FailMessage = "No Notification channel found for client"
;
                }
            }
            //cc all clients
            foreach (var cli in clientinfo.ccclients)
            {
                cli.Valid = SucFail.Success;
                cli.SucceedFail = SucFail.Success;
                foreach (var chan in channels)
                {
                    chan.Validate(cli, clientinfo.info);
                    if (cli.Valid == SucFail.Success)
                        chan.Send(cli, clientinfo.info);
                }
            }
            return SucFail.Success;
        }

    }

    public interface NotificationChannel
    {
        string CreateBody(Client cli, NotificationInfo info);
        void Send(Client cli, NotificationInfo info);
        void Validate(Client cli, NotificationInfo info);

        bool ActiveNotifcationChanel(Client cli);

    }
    ///make email the base class of communication - probably could use an interface here
    public class EmailCom : NotificationChannel
    {
        public bool ActiveNotifcationChanel(Client cli)
        {
            return (cli.Email.Enable);
        }


        public void Validate(Client cli, NotificationInfo info)
        {
            if (!cli.Email.Enable)
                return;

            if (cli.Email.Address == null)
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Failed Validation - Email Missing";
            }
            else if (!cli.Email.Address.Contains("@"))
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Invalid Email Address not @";
            }
        }
        public string CreateBody(Client cli, NotificationInfo info)
        {
            return "Dear " + cli.FirstName + " " + cli.LastName + ", \r\n" + info.Body;
        }
        public void Send(Client cli, NotificationInfo info)
        {
            try
            {
                if (!cli.Email.Enable)
                    return;
                string email = "timhams@gmail.com";
                string password = "######";//i used my personal email for testing pasword removed
                SmtpClient client = new SmtpClient("smtp.office365.com");
                client.Credentials = new NetworkCredential(email, password);
                client.Port = 587;
                client.EnableSsl = true;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(email);
                {
                    mailMessage.To.Add(cli.Email.Address);
                }

                mailMessage.Subject = "Invoice Email";
                mailMessage.Body = CreateBody(cli, info);
                mailMessage.IsBodyHtml = true;
                if (info.InvoiceAttachment != null)
                {
                    mailMessage.Attachments.Add(info.InvoiceAttachment);

                }
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                cli.SucceedFail = SucFail.Fail;
                cli.FailMessage = ex.Message;
            }
        }
    }

    public class SMSCom : NotificationChannel
    {
        public bool ActiveNotifcationChanel(Client cli)
        {
            return (cli.SMS.Enable);
        }

        bool ValidateMobileNumber(string mobno)
        {
            ///validate
            return true;
        }
        const int TextMesageMaxLength = 100;
        //validate that the sms is less than 
        public void Validate(Client cli, NotificationInfo info)
        {
            if (!cli.SMS.Enable)
                return;

            if (cli.SMS == null)
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Failed Validation - Mobile number Missing";
            }
            else if (!ValidateMobileNumber(cli.SMS.Address))
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Failed Validation - Mobile number Missing";
            }

            if (info.Body.Length > TextMesageMaxLength)
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Failed Validation - Text too long";
            }
        }
        public string CreateBody(Client cli, NotificationInfo info)
        {
            return info.Body;
        }

        public void Send(Client cli, NotificationInfo info)
        {
            try
            {
                if (!cli.SMS.Enable)
                    return;

                //hook into telstra integrated messaging or watever tool we are SMS ing with
            }
            catch (Exception ex)
            {
                cli.SucceedFail = SucFail.Fail;
                cli.FailMessage = ex.Message;
            }
        }
    }

    public class MobileAppCom : NotificationChannel
    {
        public bool ActiveNotifcationChanel(Client cli)
        {
            return (cli.MobileApp.Enable);
        }
        //validate that the sms is less than 
        public void Validate(Client cli, NotificationInfo info)
        {
            if (!cli.MobileApp.Enable)
                return;

            if (cli.MobileApp == null)
            {
                cli.Valid = SucFail.Fail;
                cli.FailMessage = "Failed Validation - Mobile App ID Missing";
            }
            //Preview here
            //maybe we do the preview in the Validate method
            //if preview is not satisfactory then validation fails and message not send
        }

        void Preview(ClientNotification cliinfo)
        {
            //if not satisfatory preview return valid
            //cliinfo.Valid = ClientNotification.SucFail.Fail;
            //cliinfo.FailMessage = "User disliked preview - do not send";

        }
        public string CreateBody(Client cli, NotificationInfo info)
        {
            return info.Body;
        }

        public void Send(Client cli, NotificationInfo info)
        {
            if (!cli.MobileApp.Enable)
                return;

            try
            {
                //hook into telstar integrated messaging or watever tool we are SMS ing with
                //              cliinfo.SucceedFail = ClientNotification.SucFail.Success;
            }
            catch (Exception ex)
            {
                cli.SucceedFail = SucFail.Fail;
                cli.FailMessage = ex.Message;
            }
        }
    }

}