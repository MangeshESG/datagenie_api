using datagenie_api.Model;
using static datagenie_api.Repositorys.HelperRepository;

public interface IEmailService
{
    Task SendEmailFromTemplate(TokenModel objtokenmodel, CommunicationTemplate CommunicationTemplateID);
    bool SendEmail(string strTo, string strFrom, string strSubject, string strBody,string bcc, string cc, string displayName,string host, int port, string userName, string password);
    string UpdateEmailBody(TokenModel objtokenmodel, string emailBody);
    bool sendEmailLogInfo(string strTo, string strFrom, string strSubject, string strBody, int i);
}
