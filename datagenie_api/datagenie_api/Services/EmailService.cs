using Dapper;
using datagenie_api.Data;
using datagenie_api.Interfaces;
using datagenie_api.Model;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Net.Mail;
using static datagenie_api.Repositorys.HelperRepository;

public class EmailService : IEmailService
{
    private readonly DapperContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(DapperContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendEmailFromTemplate(TokenModel objtokenmodel, CommunicationTemplate CommunicationTemplateID)
    {
        using var connection = _context.CreateConnection();

        _logger.LogInformation("Fetching email template for TemplateID: {TemplateID}", CommunicationTemplateID);

        var parameters = new { CommunicationTemplateID = CommunicationTemplateID };

        var template = connection.QueryFirstOrDefault<CommunicationTemplateDetails>(
            "usp_GetCommunicationTemplateDetails",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        if (template == null)
        {
            _logger.LogWarning("No email template found for TemplateID: {TemplateID}", CommunicationTemplateID);
            return;
        }

        string emailBody = template.Body;
        string masterHtml = template.MasterHtml;
        string subject = template.Subject;
        string bcc = template.BCC;
        string cc = template.CC;
        string emailAddress = template.EmailAddress;
        string displayName = template.DisplayName;
        string host = template.Host;
        int port = template.Port;
        string userName = template.UserName;
        string password = template.Password;

        emailBody = masterHtml.Replace("{CONTENT}", emailBody);
        emailBody = UpdateEmailBody(objtokenmodel, emailBody);

        if ((int)CommunicationTemplateID == 10)
        {
            subject = subject.Replace("{Status}", objtokenmodel.Status);
        }

        _logger.LogInformation("Sending email to: {To}", objtokenmodel.EmailID);

        bool status = SendEmail(
            objtokenmodel.EmailID,
            emailAddress,
            subject,
            emailBody,
            bcc,
            cc,
            displayName,
            host,
            port,
            userName,
            password
        );

        _logger.LogInformation("Email sending status to {Email}: {Status}", objtokenmodel.EmailID, status);
    }

    public bool SendEmail(string strTo, string strFrom, string strSubject, string strBody,
                          string bcc, string cc, string displayName,
                          string host, int port, string userName, string password)
    {
        _logger.LogInformation("Preparing to send email to: {To}, Subject: {Subject}", strTo, strSubject);

        bool blndone = false;

        var msg = new MailMessage
        {
            From = new MailAddress(strFrom, displayName),
            Subject = strSubject,
            Body = strBody,
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        foreach (var to in strTo.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            msg.To.Add(new MailAddress(to.Trim()));
        }

        if (!string.IsNullOrEmpty(bcc))
        {
            foreach (var b in bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                msg.Bcc.Add(new MailAddress(b.Trim()));
            }
        }

        if (!string.IsNullOrEmpty(cc))
        {
            foreach (var c in cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                msg.CC.Add(new MailAddress(c.Trim()));
            }
        }

        var smtp = new SmtpClient(host)
        {
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential(userName, password),
            Port = port
        };

        smtp.Send(msg);
        blndone = true;

        _logger.LogInformation("Email sent successfully to: {To}", strTo);
        return blndone;
    }

    public string UpdateEmailBody(TokenModel objTokenModel, string strbody)
    {
        _logger.LogDebug("Updating email body placeholders with token model values...");

        foreach (var propertyInfo in objTokenModel.GetType().GetProperties())
        {
            if (propertyInfo.CanRead)
            {
                var value = propertyInfo.GetValue(objTokenModel, null);
                if (value != null)
                {
                    strbody = strbody.Replace("{" + propertyInfo.Name + "}", value.ToString());
                }
            }
        }

        return strbody;
    }

    public bool sendEmailLogInfo(string strTo, string strFrom, string strSubject, string strBody, int i)
    {
        _logger.LogInformation("Sending log email to: {To}, Subject: {Subject}", strTo, strSubject);

        bool blndone = false;

        var msg = new MailMessage
        {
            From = new MailAddress(strFrom),
            Subject = strSubject,
            Body = strBody,
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        foreach (var to in strTo.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            msg.To.Add(new MailAddress(to.Trim()));
        }

        if (i == 1)
        {
            msg.CC.Add(new MailAddress("danishp@groupji.co"));
            msg.CC.Add(new MailAddress("Michaelr@groupji.co"));
        }

        var smtp = new SmtpClient("213.171.222.69")
        {
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential("support@datagenie.co", "!8Qh5e2t")
        };

        smtp.Send(msg);
        blndone = true;

        _logger.LogInformation("Log email sent to {To}", strTo);
        return blndone;
    }
}
