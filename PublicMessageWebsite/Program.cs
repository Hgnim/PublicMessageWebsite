using static PublicMessageWebsite.FilePath;
using System.Xml;
using static PublicMessageWebsite.Models.CoreModel;
using static PublicMessageWebsite.DataCore;
using static PublicMessageWebsite.PInfo;
using System.Reflection;

namespace PublicMessageWebsite
{
    public class Program
    {             
        public static void Main(string[] args)
        {
			Console.WriteLine(
@$"��ӭʹ�ù���������ҳ����ˡ�
�汾: {version}
{copyright}
{githubUrl_addHead}
��ֹ���÷��������κ�Υ����;�����κ�ԭ���µ��κκ���������û��е��������߶Դ˲��е��κ����Ρ�"
);

			if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            if (!Directory.Exists(messageDir)) Directory.CreateDirectory(messageDir);

            if (File.Exists(configFile)) {
				try {
					DataFile.ReadData();
				} catch(Exception ex) { Console.WriteLine($"���������ļ�ʱ���ִ���ԭ��: {ex.Message}"); return; }

				if (DataFiles.config.UpdateConfig == true) {
					DataFiles.config.UpdateConfig = false;
					{
						/// <summary>
						/// ��Ŀ������п��ܰ���null�������滻ΪĬ��ֵ
						/// </summary>
						/// <param name="targetObj">Ŀ�����</param>
						/// <param name="defaultObj">�����Ĭ������ֵ</param>
						/// <exception cref="ArgumentException"></exception>
						static void ReplaceNullWithDefault(ref object targetObj, object defaultObj) {
							Type objType = targetObj.GetType();
							if (objType != defaultObj.GetType())
								throw new ArgumentException("������Ŀ������Ĭ��ֵ���������ͬһ����");
							//����ÿ������
							foreach (PropertyInfo property in objType.GetProperties()) {
								object? targetObjValue = property.GetValue(targetObj), defaultObjValue = property.GetValue(defaultObj);
								//���Ŀ����������ֵΪnull����ʹ��Ĭ��ֵ
								if (targetObjValue == null) {
									property.SetValue(targetObj, defaultObjValue);
								}
								else if (
									targetObjValue != null &&
									!property.PropertyType.IsPrimitive &&
									property.PropertyType != typeof(string)
									) {
									if (property.PropertyType.IsClass) {
										//�����������(class)���ݹ鴦��
										ReplaceNullWithDefault(ref targetObjValue, defaultObjValue!);
									}
									else if (property.PropertyType.IsValueType) {
										//�������ֵ����(struct)���ݹ鴦��
										ReplaceNullWithDefault(ref targetObjValue, defaultObjValue!);
										property.SetValue(targetObj, targetObjValue);
									}
								}
							}
						}

						object checkObj = DataFiles.config;
						ReplaceNullWithDefault( ref checkObj, new DataFile.ConfigFile());
                        DataFiles.config = (DataFile.ConfigFile)checkObj;
					}
					DataFile.SaveData();
					Console.WriteLine("�����ļ��Ѹ��£����˳������");
					return;
				}
			}
            else {
                if (File.Exists(configFile_Obsolete)) {
                    //Ϊ�˼���1.3.0��ǰ��ʹ��xml�����ļ��ķ����
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
