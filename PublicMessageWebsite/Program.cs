using static PublicMessageWebsite.FilePath;
using System.Xml;
using static PublicMessageWebsite.Models.CoreModel;
using static PublicMessageWebsite.DataCore;
using static PublicMessageWebsite.PInfo;

namespace PublicMessageWebsite
{
    public class Program
    {             
        public static void Main(string[] args)
        {
			Console.WriteLine(
@$"欢迎使用公共留言网页服务端。
版本: {version}
{copyright}
{githubUrl_addHead}
禁止将该服务用于任何违法用途。因任何原因导致的任何后果都将由用户承担，开发者对此不承担任何责任。"
);

			if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            if (!Directory.Exists(messageDir)) Directory.CreateDirectory(messageDir);

            if (File.Exists(configFile)) {
				try {
					DataFile.ReadData();
				} catch(Exception ex) { Console.WriteLine($"处理配置文件时出现错误，原因: {ex.Message}"); return; }

				if (DataFiles.config.UpdateConfig == true) {
					DataFiles.config.UpdateConfig = false;
					DataFile.SaveData();
					Console.WriteLine("配置文件已更新，已退出服务端");
					return;
				}
			}
            else {
                if (File.Exists(configFile_Obsolete)) {
                    //为了兼容1.3.0以前还使用xml配置文件的服务端
                    XmlDocument xmlDoc = new();
                    XmlNode xmlRoot;
                    xmlDoc.Load(configFile_Obsolete);
                    xmlRoot = xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Config")!;
                    XmlNodeList xmlNL = xmlRoot.ChildNodes;
                    XmlElement xmlE;
                    {
                        DataFile.ConfigFile.ConfigModel configM = DataFiles.config.Config ;
						foreach (XmlNode xn in xmlNL) {
                            xmlE = (XmlElement)xn;
                            switch (xmlE.Name) {
                                case "WebTitle":
									configM.WebTitle = xmlE.InnerText;
                                    break;
                                case "Title":
									configM.Title = xmlE.InnerText;
                                    break;
                                case "BottomText":
									configM.BottomText = xmlE.InnerText;
                                    break;
                                case "OneIpAddMessageFrequency":
									configM.OneIpAddMessageFrequency = int.Parse(xmlE.InnerText);
                                    break;
                                case "ApiOutputMsgDay":
									configM.ApiOutputMsgDay = int.Parse(xmlE.InnerText);
                                    break;
                            }
                        }
                        DataFiles.config.Config = configM;
                    }
                    xmlRoot = xmlDoc.SelectSingleNode("PublicMessageWebsite")!.SelectSingleNode("Website")!;
                    xmlNL = xmlRoot.ChildNodes;
                    {
                        DataFile.ConfigFile.WebsiteModel websiteM = DataFiles.config.Website;
						foreach (XmlNode xn in xmlNL) {
                            xmlE = (XmlElement)xn;
                            switch (xmlE.Name) {
                                case "Addr":
									websiteM.Addr = xmlE.InnerText;
                                    break;
                                case "UrlRoot":
									websiteM.UrlRoot = xmlE.InnerText;
                                    break;
                                case "Port":
									websiteM.Port = int.Parse( xmlE.InnerText);
                                    break;
                                case "UseHttps":
									websiteM.UseHttps = bool.Parse(xmlE.InnerText);
                                    break;
                                case "UseXFFRequestHeader":
									websiteM.UseXFFRequestHeader = bool.Parse(xmlE.InnerText);
                                    break;
                            }
                        }
                        DataFiles.config.Website = websiteM;
					}

                    File.Delete(configFile_Obsolete);
                }
				DataFile.SaveData();
			}			

			FileEditer.LogWriter_Reload();
            FileEditer.MsgFileWriter_Reload();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.WebHost.UseUrls(UseUrlValue.Get());
            var app = builder.Build();
            app.UsePathBase(UseUrlValue.UrlRoot);

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
