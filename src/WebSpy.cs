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

      private OutputDates ReadDates(){      
        this.label.Text = "Reading the update timestamps..."; 
        List<HtmlElement> tds = new List<HtmlElement>(wb.Document.GetElementsByTagName("td").Cast<HtmlElement>());            
        HtmlElement node = tds.Where(x => x.InnerText != null && x.InnerText.Equals("Última modificació")).FirstOrDefault();                   

        
        OutputDates od = new OutputDates();
        DateTime temp = DateTime.Now;
        int s = 0;

        //TODO: fix this
        node = node.Parent;
        while(s++ < 8 && node.NextSibling != null){
          node = node.NextSibling;

          if(s % 2 > 0){
            tds = new List<HtmlElement>(node.Children.Cast<HtmlElement>());  
            temp =DateTime.ParseExact((tds.Last().InnerText == null ? "01/01/1900 00:00" : tds.Last().InnerText), "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            switch(s){
              case 1:
                od.Ratings = temp;
                break;
              
              case 3:
                od.Advertisements = temp;
                break;

              case 5:
                od.Lists = temp;
                break;

              case 7:
                od.Announcements = temp;
                break;
            }          
          }
        }        

        return od;
      }

      private void StoreChanges(OutputDates dates){
        this.label.Text = "Storing changes..."; 
        Output output = Output.Load();

        bool updated = false;
        string msg;
        List<string> messages = new List<string>();
        if(output.Dates.Ratings < dates.Ratings){
          updated = true;
          output.Dates.Ratings = dates.Ratings;                    
          msg = string.Format("{0}: New changes has been detected on {1} for {2}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), dates.Ratings.ToString("dd/MM/yyyy HH:mm"), "ratings");
          messages.Add(msg);
        }

        if(output.Dates.Advertisements < dates.Advertisements){
          updated = true;
          output.Dates.Advertisements = dates.Advertisements;          
          msg = string.Format("{0}: New changes has been detected on {1} for {2}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), dates.Ratings.ToString("dd/MM/yyyy HH:mm"), "advertisements");
          messages.Add(msg);
        }

        if(output.Dates.Lists < dates.Lists){
          updated = true;
          output.Dates.Lists = dates.Lists;          
          msg = string.Format("{0}: New changes has been detected on {1} for {2}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), dates.Ratings.ToString("dd/MM/yyyy HH:mm"), "lists");
          messages.Add(msg);
        }

        if(output.Dates.Announcements < dates.Announcements){
          updated = true;
          output.Dates.Announcements = dates.Announcements;          
          msg = string.Format("{0}: New changes has been detected on {1} for {2}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), dates.Ratings.ToString("dd/MM/yyyy HH:mm"), "announcements");
          messages.Add(msg);
        }       
        
        if(!updated) output.Log.Add(string.Format("{0}: No changes detected.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        else{   

          string body = "<p>New changes has been detected into the applicant's desk: </p><ul>";
          foreach(string m in messages){
            output.Log.Add(m);
            body = string.Format("{0} <li>{1}</li>", body, m);
          }          
          body = string.Format("{0}</ul><p>{1}</p>", body, "<a href='https://aplicacions.ensenyament.gencat.cat/pls/apex/f?p=2016001:12'>Check it here!</a>");


          MailMessage mailMessage = new MailMessage(){
            From = new MailAddress(_settings.Mailing.From),            
            IsBodyHtml = true,
            Body = body,
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
            OutputDates dates = ReadDates();
            StoreChanges(dates);
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
