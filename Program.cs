using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;
using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            string ApplicationName = "Cliente web 2";
            Console.WriteLine(ApplicationName);
            Console.WriteLine("AccessToken");
            string accessToken = Console.ReadLine() ?? throw new Exception("line is null");
            GoogleCredential userCredentials = GoogleCredential.FromAccessToken(accessToken);
            Console.WriteLine(userCredentials);

            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = ApplicationName
            });

            Console.WriteLine("1/ List Labels");
            Console.WriteLine("2/ Send Email");
            int option = int.Parse(Console.ReadLine() ?? throw new Exception("option null"));

            if (option == 1)
            {
                UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");
                IList<Label> labels = request.Execute().Labels;
                Console.WriteLine("Labels:");
                if (labels == null || labels.Count == 0)
                {
                    Console.WriteLine("No labels found.");
                    return;
                }
                foreach (var labelItem in labels)
                {
                    Console.WriteLine("{0}", labelItem.Name);
                }
            }

            if (option == 2)
            {
                var email = new MimeMessage();
                Console.WriteLine("Enter email sender:");
                var sender = Console.ReadLine();
                Console.WriteLine("Enter email receiver:");
                var receiver = Console.ReadLine();

                email.From.Add(new MailboxAddress("Josemontes", sender));
                email.To.Add(new MailboxAddress("Inbox", receiver));
                email.Subject = "Test Email Subject";
                email.Body = new TextPart("plain") { Text = "Hello, this is a test email!" };

                // Solicitar archivo para adjuntar
                Console.WriteLine("Enter the path of the file to attach (or press Enter to skip):");
                var filePath = Console.ReadLine();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    var attachment = new MimePart("application/octet-stream")
                    {
                        Content = new MimeContent(File.OpenRead(filePath)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(filePath)
                    };

                    var multipart = new Multipart("mixed") { email.Body, attachment };
                    email.Body = multipart;
                }

                using (var memoryStream = new MemoryStream())
                {
                    email.WriteTo(memoryStream);
                    var base64Email = Convert.ToBase64String(memoryStream.ToArray())
                        .Replace("+", "-").Replace("/", "_").Replace("=", "");

                    // Crear el mensaje para enviar
                    var message = new Message
                    {
                        Raw = base64Email
                    };

                    // Enviar el correo
                    var request = new UsersResource.MessagesResource.SendRequest(service, message, "me");
                    request.Execute();
                    Console.WriteLine("Email sent successfully.");
                }
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
