using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace CandidateWebSpy
{
    public partial class WebSpy : Form
    {
      private short _step = 0;
      private Settings _settings;
     
      public WebSpy()
      {
        _settings = Settings.Load();

        InitializeComponent();                
        WebBrowser wb = new WebBrowser();          

        this.Controls.Add(wb);
        wb.Visible = false;
        //wb.Visible = true;
        //wb.Size = new System.Drawing.Size(800, 450);

        wb.DocumentCompleted += DocumentCompleted;
        wb.ScrollBarsEnabled = false;
        wb.ScriptErrorsSuppressed = true;     

        Navigate(wb);     
      }    

      private void Navigate(WebBrowser wb){
        _step = 0;
        wb.Navigate("https://aplicacions.ensenyament.gencat.cat/pls/apex/f?p=2016001:12");       
      }

      private void DoLogin(WebBrowser wb){              
        // Do what ever you want to do here when page is completely loaded.           
        wb.Document.GetElementById("P12_IDENTIFICADOR").SetAttribute("value",  ((int)_settings.Credentials.ID).ToString());          
        wb.Document.GetElementById("P12_USUARI").SetAttribute("value", _settings.Credentials.User);
        wb.Document.GetElementById("P12_PASSWORD").SetAttribute("value", _settings.Credentials.Pass);

        List<HtmlElement> inputs = new List<HtmlElement>(wb.Document.GetElementsByTagName("input").Cast<HtmlElement>());            
        HtmlElement accept = inputs.Where(x => x.GetAttribute("type").Equals("button") && x.GetAttribute("name").Equals("P12_ACCEPTA")).SingleOrDefault();
        accept.InvokeMember("click");        
      }

      private DateTime ReadDate(WebBrowser wb){       
          List<HtmlElement> tds = new List<HtmlElement>(wb.Document.GetElementsByTagName("td").Cast<HtmlElement>());            
          HtmlElement node = tds.Where(x => x.InnerText != null && x.InnerText.Equals("Última modificació")).FirstOrDefault();     
          
          tds = new List<HtmlElement>(node.Parent.NextSibling.Children.Cast<HtmlElement>());  
          node = tds.Last();

          return DateTime.ParseExact(node.InnerText, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
      }

      private void StoreChanges(DateTime date){
        Output output = Output.Load();
        
        if(output.Last >= date) output.Log.Add(string.Format("{0}: No changes detected.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        else{
          output.Last = date;
          output.Log.Add(string.Format("{0}: New changes has been detected on {1}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), date.ToString("dd/MM/yyyy HH:mm")));


          MailMessage mailMessage = new MailMessage(){
            From = new MailAddress(_settings.Mailing.From),            
            IsBodyHtml = true,
            Body = String.Format("<p>New changes has been detected into the applicant's desk, on {0}.<br/><a href='https://aplicacions.ensenyament.gencat.cat/pls/apex/f?p=2016001:12'>Check it here!</a></p>", date.ToString("dd/MM/yyyy HH:mm")),
            Subject = "New changes has been detected into the applicant's desk."
          };

          mailMessage.To.Add(_settings.Mailing.To);         
          using (SmtpClient client = new SmtpClient(_settings.Mailing.SmtpServer)){ 
            client.Port = _settings.Mailing.SmtpPort;       
            client.Credentials = new NetworkCredential(_settings.Mailing.User, _settings.Mailing.Pass);
            client.EnableSsl = true;  
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Send(mailMessage);         
          }      
        }

        while(output.Log.Count > _settings.Log.Entries)
            output.Log.RemoveAt(0);     

        Output.Store(output);     
      }

      private void WaitForPolling(WebBrowser wb){      
        //TODO: show a progress bar with more usefull information and avoid using Thread.Sleep (sorry, not enough time to do things right :p)
        System.Threading.Thread.Sleep(_settings.Polling.Interval*1000);
        Navigate(wb);   
      }

      private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
      {
        WebBrowser wb = (WebBrowser)sender;
        switch(_step++){
          case 0:                         
            DoLogin(wb);
            break;

          case 1:
            DateTime date = ReadDate(wb);
            StoreChanges(date);
            WaitForPolling(wb);             
            break;
        }
      }
    }
}
