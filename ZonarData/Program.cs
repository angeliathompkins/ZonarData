using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.IO;
using System.Xml;
using System.Data;
using System.Xml.Xsl;
using Newtonsoft.Json;
namespace ZonarData
{
    class Program
    {
        static void Main(string[] args)
        {
            string AppName = "Zonar PTO Data";
            try
            {

                // int ExceptionOnScreen = Convert.ToInt32(ConfigurationManager.AppSettings["ExceptionOnScreen"].ToString());
                string ServiceUrl = ConfigurationManager.AppSettings["ServiceUrl"].ToString();
                string ServiceUserName = ConfigurationManager.AppSettings["ServiceUserName"].ToString();
                string ServicePassword = ConfigurationManager.AppSettings["ServicePassword"].ToString();

                string FromDate = "", ToDate = "", Weekendingdate = "";

                // DataSet dsAssetID = cls_DAL.ZonarAsset_PTO_GetList();

                /*if (dsAssetID.Tables[1].Rows.Count > 0)
                {
                    FromDate = dsAssetID.Tables[1].Rows[0]["StartDate"].ToString();
                    ToDate = dsAssetID.Tables[1].Rows[0]["EndDate"].ToString();
                    Weekendingdate = dsAssetID.Tables[1].Rows[0]["WeekendingDate"].ToString();
                }*/
                FromDate = "3/19/2018";
                ToDate = "3/26/2018";
                Weekendingdate = "3/25/2018";

                DateTime StartDate = Convert.ToDateTime(FromDate);
                DateTime EndDate = Convert.ToDateTime(ToDate);
                DateTime Weekending_date = Convert.ToDateTime(Weekendingdate);

                /* int TotalRecords = 0;

                 for (int i = 0; i < dsAssetID.Tables[0].Rows.Count; i++)
                 {
                     Console.WriteLine("Processing " + i.ToString() + " out of " + dsAssetID.Tables[0].Rows.Count.ToString());



                     DataTable Asset_Table_Complete = new DataTable();
                     DataTable Asset_Table = new DataTable();
                     Asset_Table.Columns.Add("AssetID");
                     Asset_Table.Columns.Add("Fleet");
                     Asset_Table.Columns.Add("Qty_Idles");
                     Asset_Table.Columns.Add("Total_Idle_Time");
                     Asset_Table.Columns.Add("Pto_Dur");
                     Asset_Table.Columns.Add("Pto_Diff");
                     Asset_Table.Columns.Add("Viol_Count");
                     Asset_Table.Columns.Add("Entry_Date");
                     Asset_Table.Columns.Add("WeekEnding_Date");

                     DataTable PTO_Table_Complete = new DataTable();
                     DataTable PTO_Table = new DataTable();
                     PTO_Table.Columns.Add("AssetID");
                     PTO_Table.Columns.Add("Fleet");
                     PTO_Table.Columns.Add("WeekEnding_Date");

                     DataRow dtPTOrow;
                     dtPTOrow = PTO_Table.NewRow();

                     dtPTOrow["AssetID"] = dsAssetID.Tables[0].Rows[i][0].ToString();
                     dtPTOrow["Fleet"] = dsAssetID.Tables[0].Rows[i][1].ToString();
                     dtPTOrow["WeekEnding_Date"] = Weekending_date;

                     PTO_Table.Rows.Add(dtPTOrow);

                     if (PTO_Table_Complete.Rows.Count > 0)
                     {
                         PTO_Table_Complete.ImportRow(dtPTOrow);
                     }
                     else
                     {
                         PTO_Table_Complete.Merge(PTO_Table);
                     }
                     */

                while (StartDate < EndDate)
                {
                    Console.WriteLine(StartDate.ToShortDateString());
                    string sdEpoc = ToUnixTime(StartDate).ToString();
                    string edEpoc = ToUnixTime(StartDate.Date.AddDays(1).AddSeconds(-1)).ToString();

                    //string XmlPath = ServiceUrl + "?action=showposition&operation=path&type=Standard&version=2&starttime=" + sdEpoc + "&endtime=" + edEpoc + "&logvers=3.7&format=xml&reqtype=exsid&target=38T304";

                    string XmlPath = @"http://pik3599.zonarsystems.net/interface.php?action=showposition&operation=current&format=xml&logvers=3&version=2";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(XmlPath);
                    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(ServiceUserName + ":" + ServicePassword);
                    string credentials = System.Convert.ToBase64String(toEncodeAsBytes);


                    byte[] byteArray = Encoding.UTF8.GetBytes(XmlPath);
                    // Configure the request content type to be xml, HTTP method to be POST, and set the content length
                    request.Method = "GET";
                    request.UserAgent = "WSDAPI";
                    request.ContentType = "application/soap+xml;charset=UTF-8";
                    request.Timeout = 600000;
                    // Configure the request to use basic authentication, with base64 encoded user name and password, to invoke the service.
                    request.Headers.Add("Authorization", "Basic " + credentials);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                     StreamReader reader = new StreamReader(response.GetResponseStream());
                    //XmlReader x = XmlReader.Create(response.GetResponseStream());
                     var MainArray = reader.ReadToEnd();
                    //var MainArray = x.ReadOuterXml();
                    string xsltLocation = @"c:\Projects\Pike\ZonarData\Zonar.xslt";
                    string newcsv = @"c:\Projects\Pike\ZonarData\ZonarData\Zonartest.json";
                    string sourcefile = @"c:\Projects\Pike\ZonarData\Zonartemp.xml";
                  //  XslCompiledTransform transform = new XslCompiledTransform();
                  //  transform.Load(xsltLocation);
                  
                  // FileStream outputStream = new FileStream(newcsv, FileMode.Append);
                    //x.Settings.ConformanceLevel = ConformanceLevel.Fragment;
                //    transform.Transform(x,null,outputStream);


                    XmlDocument Docs = new XmlDocument();
                  
                    Docs.LoadXml(MainArray);
                    string jsonText = JsonConvert.SerializeXmlNode(Docs);
                    System.IO.File.WriteAllText(newcsv, jsonText);
                    XmlElement root = Docs.DocumentElement;
                   
                    XmlNodeList Headnodes = root.SelectNodes("//get_pto_hours/pto_hours");
                    /* foreach (XmlNode nodess in Headnodes)
                     {
                         DataRow dtrow;
                         dtrow = Asset_Table.NewRow();

                         dtrow["AssetID"] = nodess["assetid"].InnerText;
                         dtrow["Fleet"] = nodess["fleet"].InnerText;
                         dtrow["Qty_Idles"] = nodess["qty_idles"].InnerText;
                         dtrow["Total_Idle_Time"] = nodess["set_total_idle_time"].InnerText;
                         dtrow["Pto_Dur"] = nodess["set_pto_dur"].InnerText;
                         dtrow["Pto_Diff"] = nodess["set_pto_diff"].InnerText;
                         dtrow["Viol_Count"] = nodess["viol_count"].InnerText;
                         dtrow["Entry_Date"] = StartDate;
                         dtrow["WeekEnding_Date"] = Weekending_date;

                         Asset_Table.Rows.Add(dtrow);

                         if (Asset_Table_Complete.Rows.Count > 0)
                         {
                             Asset_Table_Complete.ImportRow(dtrow);
                         }
                         else
                         {
                             Asset_Table_Complete.Merge(Asset_Table);
                         }
                     }
                     StartDate = StartDate.AddDays(1);
                 }
                 int result = cls_DAL.Raw_PTOData_Insert(Asset_Table_Complete, PTO_Table_Complete);
                 TotalRecords = TotalRecords + Asset_Table_Complete.Rows.Count;*/
                    /* if (result == 1)
                     {
                         Console.WriteLine("Data insert failure");
                     }
                     else
                     {
                         Console.WriteLine("Succesfully inserted the data - " + Asset_Table_Complete.Rows.Count.ToString() + " - " + dsAssetID.Tables[0].Rows[i][1].ToString());
                         Console.WriteLine("Total Records - " + TotalRecords.ToString());

                     }*/
                }

            }
            catch (Exception exp)
            {
               /* clsLog objLog = new clsLog();
                objLog.Message = "Zonar data insertion Failure";
                objLog.AppName = AppName;
                objLog.Content = exp.Message.ToString();
                if (exp.InnerException != null)
                {
                    objLog.InnerException = exp.InnerException.ToString();
                }
                else
                {
                    objLog.InnerException = "";
                }

                objLog.Source = exp.Source.ToString();
                objLog.CustomeMessage = "Zonar data insertion Failure";
                objLog.MehodName = MethodBase.GetCurrentMethod().Name;
                objLog.WS = "";
                cls_DAL.UpdateLog_Exception(objLog);
                Console.WriteLine("Zonar Data - Exception - " + exp.Message.ToString());*/
            }

        }
        public static DateTime FromUnixTime(string unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(Convert.ToDouble(unixTime));
        }

        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return Convert.ToInt64((date - epoch).TotalSeconds) + 14400;
        }

    }
}
