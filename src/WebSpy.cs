using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

//TODO: minimized to system tray by default

namespace CandidateWebSpy
{
    public partial class WebSpy : Form
    {
      private short _step = 0;
      private Settings _settings;
      private TimeSpan _time;
     
      public WebSpy()
      {
        _settings = Settings.Load();

        InitializeComponent();                
        Navigate();     
      }    

      private void Navigate(){
        _step = 0;
        this.label.Text = "Navigating to the applicant's desk website...";
        this.wb.Navigate("https://aplicacions.ensenyament.gencat.cat/pls/apex/f?p=2016001:12");       
      }

      private void DoLogin(){     
        this.label.Text = "Logging in...";

        // Do what ever you want to do here when page is completely loaded.           
        this.wb.Document.GetElementById("P12_IDENTIFICADOR").SetAttribute("value",  ((int)_settings.Credentials.ID).ToString());          
        this.wb.Document.GetElementById("P12_USUARI").SetAttribute("value", _settings.Credentials.User);
        this.wb.Document.GetElementById("P12_PASSWORD").SetAttribute("value", _settings.Credentials.Pass);

        List<HtmlElement> inputs = new List<HtmlElement>(wb.Document.GetElementsByTagName("input").Cast<HtmlElement>());            
        HtmlElement accept = inputs.Where(x => x.GetAttribute("type").Equals("button") && x.GetAttribute("name").Equals("P12_ACCEPTA")).SingleOrDefault();
        accept.InvokeMember("click");        
      }

      private DateTime ReadDate(){      
        this.label.Text = "Reading the update timestamp..."; 
        List<HtmlElement> tds = new List<HtmlElement>(wb.Document.GetElementsByTagName("td").Cast<HtmlElement>());            
        HtmlElement node = tds.Where(x => x.InnerText != null && x.InnerText.Equals("Última modificació")).FirstOrDefault();     
        
        tds = new List<HtmlElement>(node.Parent.NextSibling.Children.Cast<HtmlElement>());  
        node = tds.Last();

        return DateTime.ParseExact(node.InnerText, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
      }

      private void StoreChanges(DateTime date){
        this.label.Text = "Storing changes..."; 
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

      private void WaitForPolling(){                             
        _time = new TimeSpan(0,0,_settings.Polling.Interval);                
        this.pb.Maximum = _settings.Polling.Interval;

        this.timer.Start();
      }

      private void WebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e){
        switch(_step++){
          case 0:                         
            DoLogin();
            break;

          case 1:
            DateTime date = ReadDate();
            StoreChanges(date);
            WaitForPolling();             
            break;
        }
      }

      private void TimerTick(object sender, EventArgs e){
        _time = _time.Subtract(new TimeSpan(0, 0, 1));
        
        this.label.Text = string.Format("Waiting {0:00}:{1:00}:{2:00} for the next call...", _time.Days, _time.Minutes, _time.Seconds);  
        this.pb.Value++; 

        if(_time.TotalSeconds == 0){
          this.timer.Stop();
          this.pb.Value = 0;
          Navigate();   
        }  
      }
    }
}
