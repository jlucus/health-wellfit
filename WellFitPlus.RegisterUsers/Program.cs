using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;

using WellFitPlus.Common.BindingModels.Identity;
using WellFitPlus.WebAPI.Models;

using Newtonsoft.Json;

using OfficeOpenXml;

namespace WellFitPlus.RegisterUsers
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            { 
                FileInfo usersFile = new FileInfo(ConfigurationManager.AppSettings["ExcelFileLocation"].ToString());

                Console.WriteLine("FileStatus: accessed file? = " + usersFile != null);
                Console.WriteLine("Push Enter to start program");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        // Wait for user interaction
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

                using (ExcelPackage package = new ExcelPackage(usersFile))
                {
                    // get the first worksheet in the workbook
                    ExcelWorksheet userworkSheet = package.Workbook.Worksheets[1];
                    ExcelWorksheet companyworkSheet = package.Workbook.Worksheets[2];


                    Console.WriteLine("Current variable status:");
                    Console.WriteLine("package: " + package != null);
                    Console.WriteLine("userworkSheet: " + userworkSheet != null);
                    Console.WriteLine("companyworkSheet: " + companyworkSheet != null);
                    Console.WriteLine("\n\nPush Enter to continue");

                    do
                    {
                        while (!Console.KeyAvailable)
                        {
                            // Wait for user interaction
                        }
                    } while (Console.ReadKey(true).Key != ConsoleKey.Enter);


                    for (int i = companyworkSheet.Dimension.Start.Row + 1;
                            i <= companyworkSheet.Dimension.End.Row;
                            i++) {
                        CompanyView newCompany = new CompanyView();

                        newCompany.Name = companyworkSheet.Cells[i, 1].Value.ToString();
                        newCompany.Street = companyworkSheet.Cells[i, 2].Value.ToString();
                        newCompany.City = companyworkSheet.Cells[i, 3].Value.ToString();
                        newCompany.State = companyworkSheet.Cells[i, 4].Value.ToString();
                        newCompany.Zip = companyworkSheet.Cells[i, 5].Value.ToString();
                        newCompany.BillingContact = companyworkSheet.Cells[i, 6].Value.ToString();
                        newCompany.SalesContact = companyworkSheet.Cells[i, 7].Value == null ? String.Empty : companyworkSheet.Cells[i, 7].Value.ToString();
                        newCompany.AnnualRenewal = Convert.ToBoolean(userworkSheet.Cells[i, 8].Value);

                        HttpClient client = new HttpClient();

                        client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServerURI"].ToString());

                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        JsonSerializer jser = new JsonSerializer();
                        string postBody = JsonConvert.SerializeObject(newCompany);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpContent content = new StringContent(postBody, Encoding.UTF8, "application/json");
                        //HttpResponseMessage wcfResponse = client.PostAsync("api/Company/Add", content).Result;
                        //HttpResponseMessage wcfResponse = client.PostAsync("WellFitWebAPI/api/Company/Add", content).Result;
                        HttpResponseMessage wcfResponse = client.PostAsync("API/api/Company/Add", content).Result; // Production call


                        Console.WriteLine("Adding Company");
                        Console.WriteLine("wcfResponse: " + wcfResponse.IsSuccessStatusCode);

                        Console.WriteLine("\n\nPush Enter to continue");
                        do
                        {
                            while (!Console.KeyAvailable)
                            {
                                // Wait for user interaction
                            }
                        } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
                    }

                    for (int i = userworkSheet.Dimension.Start.Row + 1;
                            i <= userworkSheet.Dimension.End.Row;
                            i++)
                    {
                        RegisterBindingModel newUser = new RegisterBindingModel();

                        newUser.Email = userworkSheet.Cells[i, 1].Value.ToString();
                        newUser.Password = userworkSheet.Cells[i, 2].Value.ToString();
                        newUser.ConfirmPassword = userworkSheet.Cells[i, 2].Value.ToString();
                        newUser.FirstName = userworkSheet.Cells[i, 3].Value.ToString();
                        newUser.LastName = userworkSheet.Cells[i, 4].Value.ToString();
                        newUser.Role = userworkSheet.Cells[i, 5].Value.ToString();
                        newUser.Company = userworkSheet.Cells[i, 6].Value.ToString();
                        //string usersCompany = userworkSheet.Cells[i, 6].Value.ToString();

                        HttpClient client = new HttpClient();

                        client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServerURI"].ToString());

                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        JsonSerializer jser = new JsonSerializer();
                        string postBody = JsonConvert.SerializeObject(newUser);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpContent content = new StringContent(postBody, Encoding.UTF8, "application/json");
                        //HttpResponseMessage wcfResponse = client.PostAsync("api/Account/Register", content).Result;
                        //HttpResponseMessage wcfResponse = client.PostAsync("WellFitWebAPI/api/Account/Register", content).Result;
                        HttpResponseMessage wcfResponse = client.PostAsync("API/api/Account/Register", content).Result; // Production call

                        Console.WriteLine("Registering user");
                        Console.WriteLine("wcfResponse: " + wcfResponse.IsSuccessStatusCode);
                        Console.WriteLine("Press Enter to continue");
                        do
                        {
                            while (!Console.KeyAvailable)
                            {
                                // Wait for user interaction
                            }
                        } while (Console.ReadKey(true).Key != ConsoleKey.Enter);


                        if (wcfResponse.IsSuccessStatusCode) {

                        }
                    }

                    // Users In Company
                    var query = @"
         INSERT INTO UsersInCompany
           ([Id], [UserID],[CompanyID])
		 SELECT NEWID(), U.Id, C.Id 
		 FROM UserProfiles U 
			INNER JOIN Companies C ON C.Name = U.Company";

                    var connString = System.Configuration.ConfigurationManager.ConnectionStrings["WellFitSQLConnection"].ConnectionString;
                    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connString);
                    conn.Open();

                    Console.WriteLine("connString:" + connString != null);
                    Console.WriteLine("Press Enter to continue");

                    do
                    {
                        while (!Console.KeyAvailable)
                        {
                            // Wait for user interaction
                        }
                    } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

                    var cmd = new System.Data.SqlClient.SqlCommand(query, conn);
                    var sqlResponse = cmd.ExecuteNonQuery();
                    conn.Close();


                    Console.WriteLine("Program Successfull. Press Enter to finish.");
                    do
                    {
                        while (!Console.KeyAvailable)
                        {
                            // Wait for user interaction
                        }
                    } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error occured while accessing sql database");
                Console.WriteLine(ex.ToString());

                Console.WriteLine("\n\nPush Enter to finish program");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        // Wait for user interaction
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
            }
        }
    }
}
