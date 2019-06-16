using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CandidateWebSpy
{
    public partial class WebSpy : Form
    {
      private short _step = 0;
     
      public WebSpy()
      {
          InitializeComponent();      
          this.Start();    
      }
      
      public void Start(){      
          WebBrowser wb = new WebBrowser();
          this.Controls.Add(wb);
          wb.Visible = true;
          wb.Size = new System.Drawing.Size(800, 450);

          wb.DocumentCompleted += DocumentCompleted;
          wb.Visible = true;
          wb.ScrollBarsEnabled = false;
          wb.ScriptErrorsSuppressed = true;
          wb.Navigate("https://aplicacions.ensenyament.gencat.cat/pls/apex/f?p=2016001:12");       
      }

      private void DoLogin(WebBrowser wb){              
        Settings settings = Settings.Load();

        // Do what ever you want to do here when page is completely loaded.           
        wb.Document.GetElementById("P12_IDENTIFICADOR").SetAttribute("value",  ((int)settings.Credentials.ID).ToString());          
        wb.Document.GetElementById("P12_USUARI").SetAttribute("value", settings.Credentials.User);
        wb.Document.GetElementById("P12_PASSWORD").SetAttribute("value", settings.Credentials.Pass);

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

      private void CheckChanges(DateTime date){
        Output output = Output.Load();
        
        if(output.Last >= date) output.Log.Add(string.Format("{0}: No changes detected.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        else{
          output.Last = date;
          output.Log.Add(string.Format("{0}: New changes has been detected on {1}.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), date.ToString("dd/MM/yyyy HH:mm")));
        }

        Output.Store(output);     
      }

      private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
      {
        switch(_step++){
          case 0:                         
            DoLogin((WebBrowser)sender);
            break;

          case 1:
            DateTime date = ReadDate((WebBrowser)sender);
            CheckChanges(date);
            break;
        }
      }
    }
}
