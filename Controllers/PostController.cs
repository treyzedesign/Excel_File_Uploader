using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;
using OfficeOpenXml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Uploader_Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        Uri BaseUrl = new Uri("http://localhost:5180/api");
        private readonly HttpClient _client;
        public PostController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PostFile()
        {
            //if (HttpContext.Session.GetString("token") == null)
            //{
            //    return RedirectToAction("Login", "Auth");
            //}
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PostFile(IFormFile file)
        {
           
            if (file == null || file.FileName == null || file.FileName.EndsWith(".xlsx") == false)
            {
                return RedirectToAction("Error", "Home");
            }
            string webroot = _hostingEnvironment.WebRootPath;
            string uploadsFolder = Path.Combine(webroot, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            FileInfo existingfile = new FileInfo(filePath);
            List<Dictionary<string, string>> excelFile = new List<Dictionary<string, string>>();
            using (ExcelPackage package = new ExcelPackage(existingfile))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int noOfRow = worksheet.Dimension.End.Row;     //get row count
                List<string> headers = Enumerable.Range(1, colCount).Select(col => worksheet.Cells[1, col].Value.ToString().Trim()).ToList();
                for (int row = 2; row <= noOfRow; row++)
                {
                    #nullable disable
                    Dictionary<string, string> rowData = new Dictionary<string, string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        //string key = worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? "";
                        rowData[headers[col - 1]] = worksheet.Cells[row, col].Value?.ToString() ?? "";

                    }

                    excelFile.Add(rowData);

                }
            }
            string json = JsonConvert.SerializeObject(excelFile);
            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                string Accesstoken = HttpContext.Session.GetString("token");
                Console.WriteLine(Accesstoken);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);
                //StringContent content = new StringContent(file, Encoding.UTF8, "multipart/form-data");
                content.Add(fileContent, "file", file.FileName);
                HttpResponseMessage res = await _client.PostAsync(_client.BaseAddress + "/Post/postFile", content);
                var result = res.IsSuccessStatusCode;
                if (result)
                {

                     ViewBag.json = json;            
                    
                    Console.WriteLine("done succesfully");
                    return View();
                }
                else if (res.StatusCode == HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Forbidden");
                    return View();

                }
                else if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ViewBag.message = "Unauthorized action";
                    Console.WriteLine("Unauthorized");
                    return View();

                }
                else
                {
                    return View();

                }

            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message.ToString();
                Console.WriteLine(ex.Message);
                return View();

            }

        }

 
        
        public async Task<IActionResult> GetPost()
        {
            try
            {
                HttpResponseMessage res = await _client.GetAsync(_client.BaseAddress + "/Post/getFile");
                string Content = await res.Content.ReadAsStringAsync();
                JArray jsonResponse = JArray.Parse(Content);
                if (res.IsSuccessStatusCode)
                {

                    ViewBag.PostData = jsonResponse;


                    return View();
                }
               
                else if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Unauthorized");
                    return View();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
            return View();

        }

        public IActionResult Delete()
        {
            return View();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] List<string> UserIds)
        {
            var data = JsonConvert.SerializeObject(UserIds);
            //StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            var idsString = string.Join(",", UserIds); // Convert the list of IDs to a comma-separated string
            string url = $"{_client.BaseAddress}/Post/delete?UserId={idsString}";
            HttpResponseMessage res = _client.DeleteAsync(url).Result;
            Console.WriteLine(data);
            if (res.IsSuccessStatusCode)
            {
                Console.WriteLine("successfully done");
                return Ok();
            }
            else
            {
                Console.WriteLine("something went wrong");
                return BadRequest("something went wrong");
            }
        }
    }
}
