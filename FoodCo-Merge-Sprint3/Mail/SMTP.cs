using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Mail
{
    public static class SMTP
    {
        private static SmtpClient client = null;
        private static MailMessage mail = null;
        public static string OwnerName;
        public static string OwnerEmail;

        static SMTP()
        {
            OwnerName = WebConfigurationManager.AppSettings["SMTPOwnerName"];
            OwnerEmail = WebConfigurationManager.AppSettings["SMTPOwnerEmail"];

            client = new SmtpClient(WebConfigurationManager.AppSettings["SMTPHost"],
                                    int.Parse(WebConfigurationManager.AppSettings["SMTPPort"]))
            {
                Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SMTPOwnerEmail"],
                                                    WebConfigurationManager.AppSettings["SMTPOwnerPassword"]),

                EnableSsl = bool.Parse(WebConfigurationManager.AppSettings["SMTPEnableSSL"]),
                Timeout = int.Parse(WebConfigurationManager.AppSettings["SMTPTimeOut"])
            };

            mail = new MailMessage
            {
                From = new MailAddress(OwnerEmail, OwnerName, System.Text.Encoding.UTF8),
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
        }

        public static void SendEmail(string toName, string toEmail, string subject, string body)
        {
            mail.To.Clear();
            mail.To.Add(new MailAddress(toEmail, toName, System.Text.Encoding.UTF8));
            mail.Subject = subject;
            mail.Body = body;
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                // return to sender
                mail.Subject = "Problème d'envoi de courriel:" + mail.Subject;
                mail.Body = "<h4>problème d'envoi de courriel. Message du serveur :" +
                            e.Message + "</h4><br/> <h4>Message original envoyé à :" +
                            mail.To[0].Address + "</h4><hr/>" + mail.Body;
                mail.Attachments.Clear();
                mail.To.Clear();
                mail.To.Add(new MailAddress(mail.From.Address, mail.From.DisplayName, System.Text.Encoding.UTF8));
                client.Send(mail);
            }
        }

        public static string EmailVerificationTemplate(string url)
        {
            string template = $@"
            <!DOCTYPE html>
<html>
<head>

  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
  @media screen {{
    @font-face {{
      font-family: 'Inter';
      font-style: normal;
      font-weight: 400;
      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
    }}
  }}
  body,
  table,
  td,
  a {{
    -ms-text-size-adjust: 100%; 
    -webkit-text-size-adjust: 100%; 
  }}
  table,
  td {{
    mso-table-rspace: 0pt;
    mso-table-lspace: 0pt;
  }}
  img {{
    -ms-interpolation-mode: bicubic;
  }}
  a[x-apple-data-detectors] {{
    font-family: inherit !important;
    font-size: inherit !important;
    font-weight: inherit !important;
    line-height: inherit !important;
    color: inherit !important;
    text-decoration: none !important;
  }}
  div[style*=""margin: 16px 0;""] {{
    margin: 0 !important;
  }}
  body {{
    width: 100% !important;
    height: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }}
  table {{
    border-collapse: collapse !important;
  }}
  a {{
    color: #1a82e2;
  }}
  img {{
    height: auto;
    line-height: 100%;
    text-decoration: none;
    border: 0;
    outline: none;
  }}
  </style>

</head>
<body style=""background-color: #e9ecef;"">
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Inter', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">
                Bienvenue sur FoodCo!
            </h1>
            </td>
          </tr>
        </table>

      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Appuyez sur le bouton ci-dessous pour confirmer votre adresse e-mail. Si vous n'avez pas créé de compte avec <a href=""https://FoodCo.ca"">FoodCo</a>, vous pouvez supprimer cet e-mail en toute sécurité.</p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"">
              <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                <tr>
                  <td align=""center"" bgcolor=""#ffffff"" style=""padding: 12px;"">
                    <table border=""0"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td align=""center"" bgcolor=""#FF9A7A"" style=""border-radius: 6px;"">
                          <a href=""{url}"" target=""_blank"" style=""display: inline-block; padding: 16px 36px; font-weight: 700; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;"">Vérifier mon compte</a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Si cela ne fonctionne pas, copiez et collez le lien suivant dans votre navigateur :</p>
                <p style=""margin: 0;""><a href=""{url}"" target=""_blank"">{url}</a></p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf"">
                <p style=""margin: 0;"">Cordialement,<br> FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 24px;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 12px 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;"">
              <p style=""margin: 0;"">Ce courriel a été généré automatiquement, veuillez ne pas y répondre.</p>
              <p style=""margin: 0;"">© 2024 FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
            ";

            return template;
        }


    public static string ForgotPasswordTemplate(string url)
    {
        string template = $@"
            <!DOCTYPE html>
<html>
<head>

  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
  @media screen {{
    @font-face {{
      font-family: 'Inter';
      font-style: normal;
      font-weight: 400;
      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
    }}
  }}
  body,
  table,
  td,
  a {{
    -ms-text-size-adjust: 100%; 
    -webkit-text-size-adjust: 100%; 
  }}
  table,
  td {{
    mso-table-rspace: 0pt;
    mso-table-lspace: 0pt;
  }}
  img {{
    -ms-interpolation-mode: bicubic;
  }}
  a[x-apple-data-detectors] {{
    font-family: inherit !important;
    font-size: inherit !important;
    font-weight: inherit !important;
    line-height: inherit !important;
    color: inherit !important;
    text-decoration: none !important;
  }}
  div[style*=""margin: 16px 0;""] {{
    margin: 0 !important;
  }}
  body {{
    width: 100% !important;
    height: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }}
  table {{
    border-collapse: collapse !important;
  }}
  a {{
    color: #1a82e2;
  }}
  img {{
    height: auto;
    line-height: 100%;
    text-decoration: none;
    border: 0;
    outline: none;
  }}
  </style>

</head>
<body style=""background-color: #e9ecef;"">
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Inter', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">
                Réinitialisation de Mot de Passe
            </h1>
            </td>
          </tr>
        </table>

      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Appuyez sur le bouton ci-dessous pour réinitialiser votre mot de passe. Si vous n'avez pas demandé de réinitialisation de mot de passe pour votre compte avec <a href=""https://FoodCo.ca"">FoodCo</a>, vous pouvez ignorer cet e-mail en toute sécurité.</p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"">
              <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                <tr>
                  <td align=""center"" bgcolor=""#ffffff"" style=""padding: 12px;"">
                    <table border=""0"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td align=""center"" bgcolor=""#FF9A7A"" style=""border-radius: 6px;"">
                          <a href=""{url}"" target=""_blank"" style=""display: inline-block; padding: 16px 36px; font-weight: 700; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;"">Réinitialiser mon mot de passe</a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Si cela ne fonctionne pas, copiez et collez le lien suivant dans votre navigateur :</p>
                <p style=""margin: 0;""><a href=""{url}"" target=""_blank"">{url}</a></p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf"">
                <p style=""margin: 0;"">Cordialement,<br> FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 24px;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 12px 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;"">
              <p style=""margin: 0;"">Ce courriel a été généré automatiquement, veuillez ne pas y répondre.</p>
              <p style=""margin: 0;"">© 2024 FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
            ";

        return template;
    }
        public static string UserIsBlock()
        {
            string template = $@"
            <!DOCTYPE html>
<html>
<head>

  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
  @media screen {{
    @font-face {{
      font-family: 'Inter';
      font-style: normal;
      font-weight: 400;
      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
    }}
  }}
  body,
  table,
  td,
  a {{
    -ms-text-size-adjust: 100%; 
    -webkit-text-size-adjust: 100%; 
  }}
  table,
  td {{
    mso-table-rspace: 0pt;
    mso-table-lspace: 0pt;
  }}
  img {{
    -ms-interpolation-mode: bicubic;
  }}
  a[x-apple-data-detectors] {{
    font-family: inherit !important;
    font-size: inherit !important;
    font-weight: inherit !important;
    line-height: inherit !important;
    color: inherit !important;
    text-decoration: none !important;
  }}
  div[style*=""margin: 16px 0;""] {{
    margin: 0 !important;
  }}
  body {{
    width: 100% !important;
    height: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }}
  table {{
    border-collapse: collapse !important;
  }}
  a {{
    color: #1a82e2;
  }}
  img {{
    height: auto;
    line-height: 100%;
    text-decoration: none;
    border: 0;
    outline: none;
  }}
  </style>

</head>
<body style=""background-color: #e9ecef;"">
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Inter', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">
                Compte Bloqué
            </h1>
            </td>
          </tr>
        </table>

      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Votre compte est maintenant bloqué pour une durée indéterminé, pour toute question contacter l'équipe <a href=""mailto:Support@FoodCo.ca"">Support@FoodCo.ca</a></p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf"">
                <p style=""margin: 0;"">Cordialement,<br> FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 24px;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 12px 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;"">
              <p style=""margin: 0;"">Ce courriel a été généré automatiquement, veuillez ne pas y répondre.</p>
              <p style=""margin: 0;"">© 2024 FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
            ";

            return template;
        }
        public static string UserIsUnblock()
        {
            string template = $@"
            <!DOCTYPE html>
<html>
<head>

  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
  @media screen {{
    @font-face {{
      font-family: 'Inter';
      font-style: normal;
      font-weight: 400;
      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
    }}
  }}
  body,
  table,
  td,
  a {{
    -ms-text-size-adjust: 100%; 
    -webkit-text-size-adjust: 100%; 
  }}
  table,
  td {{
    mso-table-rspace: 0pt;
    mso-table-lspace: 0pt;
  }}
  img {{
    -ms-interpolation-mode: bicubic;
  }}
  a[x-apple-data-detectors] {{
    font-family: inherit !important;
    font-size: inherit !important;
    font-weight: inherit !important;
    line-height: inherit !important;
    color: inherit !important;
    text-decoration: none !important;
  }}
  div[style*=""margin: 16px 0;""] {{
    margin: 0 !important;
  }}
  body {{
    width: 100% !important;
    height: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }}
  table {{
    border-collapse: collapse !important;
  }}
  a {{
    color: #1a82e2;
  }}
  img {{
    height: auto;
    line-height: 100%;
    text-decoration: none;
    border: 0;
    outline: none;
  }}
  </style>

</head>
<body style=""background-color: #e9ecef;"">
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Inter', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">
                Compte débloqué
            </h1>
            </td>
          </tr>
        </table>

      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Votre compte FoodCo est maintenant débloqué, pour tout autre question veuillez contacter  <a href=""mailto:Support@FoodCo.ca"">Support@FoodCo.ca</a></p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf"">
                <p style=""margin: 0;"">Cordialement,<br> FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 24px;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 12px 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;"">
              <p style=""margin: 0;"">Ce courriel a été généré automatiquement, veuillez ne pas y répondre.</p>
              <p style=""margin: 0;"">© 2024 FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
            ";

            return template;
        }

        public static string UserIsDelete()
        {
            string template = $@"
            <!DOCTYPE html>
<html>
<head>

  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">

  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
  @media screen {{
    @font-face {{
      font-family: 'Inter';
      font-style: normal;
      font-weight: 400;
      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
    }}
  }}
  body,
  table,
  td,
  a {{
    -ms-text-size-adjust: 100%; 
    -webkit-text-size-adjust: 100%; 
  }}
  table,
  td {{
    mso-table-rspace: 0pt;
    mso-table-lspace: 0pt;
  }}
  img {{
    -ms-interpolation-mode: bicubic;
  }}
  a[x-apple-data-detectors] {{
    font-family: inherit !important;
    font-size: inherit !important;
    font-weight: inherit !important;
    line-height: inherit !important;
    color: inherit !important;
    text-decoration: none !important;
  }}
  div[style*=""margin: 16px 0;""] {{
    margin: 0 !important;
  }}
  body {{
    width: 100% !important;
    height: 100% !important;
    padding: 0 !important;
    margin: 0 !important;
  }}
  table {{
    border-collapse: collapse !important;
  }}
  a {{
    color: #1a82e2;
  }}
  img {{
    height: auto;
    line-height: 100%;
    text-decoration: none;
    border: 0;
    outline: none;
  }}
  </style>

</head>
<body style=""background-color: #e9ecef;"">
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Inter', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">
                Compte Supprimé
            </h1>
            </td>
          </tr>
        </table>

      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
                <p style=""margin: 0;"">Votre compte FoodCo est maintenant supprimé, pour tout autre question veuillez contacter  <a href=""mailto:Support@FoodCo.ca"">Support@FoodCo.ca</a></p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf"">
                <p style=""margin: 0;"">Cordialement,<br> FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 24px;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" bgcolor=""#e9ecef"" style=""padding: 12px 24px; font-family: 'Inter', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;"">
              <p style=""margin: 0;"">Ce courriel a été généré automatiquement, veuillez ne pas y répondre.</p>
              <p style=""margin: 0;"">© 2024 FoodCo</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
            ";

            return template;
        }
    }
}