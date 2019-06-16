using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        wb.Document.GetElementById("P12_IDENTIFICADOR").SetAttribute("value",  ((int)settings.Credentials.id).ToString());          
        wb.Document.GetElementById("P12_USUARI").SetAttribute("value", settings.credentials.user);
        wb.Document.GetElementById("P12_PASSWORD").SetAttribute("value", settings.credentials.pass);

        List<HtmlElement> inputs = new List<HtmlElement>(wb.Document.GetElementsByTagName("input").Cast<HtmlElement>());            
        HtmlElement accept = inputs.Where(x => x.GetAttribute("type").Equals("button") && x.GetAttribute("name").Equals("P12_ACCEPTA")).SingleOrDefault();
        accept.InvokeMember("click");        
      }

      private string ReadDate(WebBrowser wb){       
          List<HtmlElement> tds = new List<HtmlElement>(wb.Document.GetElementsByTagName("td").Cast<HtmlElement>());            
          HtmlElement node = tds.Where(x => x.InnerText != null && x.InnerText.Equals("Última modificació")).FirstOrDefault();     
          
          tds = new List<HtmlElement>(node.Parent.NextSibling.Children.Cast<HtmlElement>());  
          node = tds.Last();

          return node.InnerText;                     
      }

      private void CheckChanges(string date){
         var definition = new { 
          last = DateTime.Now,
          log = new List<string>()
        };

        string json = File.ReadAllText("output.json");
        var settings = JsonConvert.DeserializeAnonymousType(json, definition);

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);

        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;

            writer.WriteStartObject();
            writer.WritePropertyName("CPU");
            writer.WriteValue("Intel");
            writer.WritePropertyName("PSU");
            writer.WriteValue("500W");
            writer.WritePropertyName("Drives");
            writer.WriteStartArray();
            writer.WriteValue("DVD read/writer");
            writer.WriteComment("(broken)");
            writer.WriteValue("500 gigabyte hard drive");
            writer.WriteValue("200 gigabyte hard drive");
            writer.WriteEnd();
            writer.WriteEndObject();
        }

        //TODO: read the last date and update if it's newer; also update its value an send an email or warning.
        //TODO: add a log for tracking issues
      }

      private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
      {
        switch(_step++){
          case 0:                         
            DoLogin((WebBrowser)sender);
            break;

          case 1:
            string date = ReadDate((WebBrowser)sender);
            CheckChanges(date);
            break;
        }
      }
    }
}