using static PublicMessageWebsite.Models.CoreModel.FilePath;
using System.Xml;
using static PublicMessageWebsite.Models.CoreModel;

namespace PublicMessageWebsite
{
    public class Program
    {
        const string version = "1.3.0.20241114";        
        public static void Main(string[] args)
        {
            Console.WriteLine("服务端开发者: Hgnim");
            Console.WriteLine($"服务端版本: V{version}");
            Console.WriteLine("禁止将该服务用于任何违法用途。因任何原因导致的任何后果都将由用户承担，开发者对此不承担任何责任。");

			if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            if (!Directory.Exists(messageDir)) Directory.CreateDirectory(messageDir);
            if (!File.Exists(configFile))
            {
                XmlTextWriter xmlWriter = new(configFile, System.Text.Encoding.GetEncoding("utf-8")) { Formatting = System.Xml.Formatting.Indented };
                xmlWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                xmlWriter.WriteStartElement("PublicMessageWebsite");
                xmlWriter.WriteStartElement("Config");

                xmlWriter.WriteStartElement("WebTitle");
                xmlWriter.WriteString(PageInfo.webTitle);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Title");
                xmlWriter.WriteString(PageInfo.textTitle);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("BottomText");
				xmlWriter.WriteString(PageInfo.bottomText);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("OneIpAddMessageFrequency");
				xmlWriter.WriteString(Config.IpAddMsgFrequency.ToString());
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("ApiOutputMsgDay");
				xmlWriter.WriteString(Config.ApiOutputMsgDay.ToString());
				xmlWriter.WriteEndElement();



				xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Website");

				xmlWriter.WriteStartElement("Addr");
				xmlWriter.WriteString(UseUrlValue.addr);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("UrlRoot");
				xmlWriter.WriteString(UseUrlValue.urlRoot);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("Port");
				xmlWriter.WriteString(UseUrlValue.port);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("UseHttps");
				xmlWriter.WriteString(UseUrlValue.isHttps.ToString());
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("UseXFFRequestHeader");
				xmlWriter.WriteString(Config.useXFFRequestHeader.ToString());
				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndElement();

				xmlWriter.WriteFullEndElement();
                xmlWriter.Close();
            }
            else
            {
                XmlDocument xmlDoc = new();
                XmlNode xmlRoot;
                xmlDoc.Load(configFile);
                xmlRoot = xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Config")!;
                XmlNodeList xmlNL = xmlRoot.ChildNodes;
                XmlElement xmlE;
                foreach (XmlNode xn in xmlNL)
                {
                    xmlE = (XmlElement)xn;
                    switch (xmlE.Name)
                    {
                        case "WebTitle":
                            PageInfo.webTitle = xmlE.InnerText;
                            break;
                        case "Title":
                            PageInfo.textTitle= xmlE.InnerText;
                            break;
                        case "BottomText":
                            PageInfo.bottomText= xmlE.InnerText;
                            break;
						case "OneIpAddMessageFrequency":
                            Config.IpAddMsgFrequency = int.Parse(xmlE.InnerText);
                            break;
                        case "ApiOutputMsgDay":
                            Config.ApiOutputMsgDay = int.Parse(xmlE.InnerText);
                            break;
					}
                }
				xmlRoot = xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Website")!;
				xmlNL = xmlRoot.ChildNodes;
				foreach (XmlNode xn in xmlNL)
				{
					xmlE = (XmlElement)xn;
					switch (xmlE.Name)
					{
						case "Addr":
							UseUrlValue.addr= xmlE.InnerText;
							break;
						case "UrlRoot":
							UseUrlValue.urlRoot = xmlE.InnerText;
							break;
                        case "Port":
                            UseUrlValue.port= xmlE.InnerText;
                            break;
                        case "UseHttps":
                            UseUrlValue.isHttps=bool.Parse( xmlE.InnerText);
                            break;
						case "UseXFFRequestHeader":
							Config.useXFFRequestHeader = bool.Parse(xmlE.InnerText);
							break;
					}
				}
			}

            FileEditer.LogWriter_Reload();
            FileEditer.MsgFileWriter_Reload();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.WebHost.UseUrls(UseUrlValue.Get());
            var app = builder.Build();
            app.UsePathBase(UseUrlValue.urlRoot);

            /*// Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }*/

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=InputMsg}/{action=Index}"); // /{id?}");

            app.Run();
        }
    }
}
