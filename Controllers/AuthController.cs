using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Uploader_Web.Models;

namespace Uploader_Web.Controllers
{
    public class AuthController : Controller
    {
        Uri BaseUrl = new Uri("http://localhost:5000/api");
        Uri BaseUrl2 = new Uri("http://localhost:5180/api");
        private readonly HttpClient _client;

        public AuthController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseUrl2;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserDto user)
        {
            if (user == null)
            {
                ModelState.AddModelError("", "input is null");
                return View(user);
            }

            try
            {
                    string data = JsonConvert.SerializeObject(user);
                    StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpResponseMessage res = _client.PostAsync(_client.BaseAddress + "/Auth/register", content).Result;
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Auth");

                }
                
                    return View(user);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(user);
            }
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto user)
        {
            if (user == null)
            {
                ModelState.AddModelError("", "input is null");
                return View(user);
            }

            try
            {
                string data = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage res = _client.PostAsync(_client.BaseAddress + "/Auth/login", content).Result;
                string resSt = await res.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(resSt);
                if (res.IsSuccessStatusCode)
                {
                    var token = jsonResponse["token"].ToString();
                    TempData["Token"] = token;
                    HttpContext.Session.SetString("token", token);
                    Console.WriteLine(token);                 
                    return RedirectToAction("PostFile", "Post");
                }
                else if (jsonResponse["statusCode"].ToString() == "404")
                {
                    ModelState.AddModelError("", "invalid credentials");
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(user);
            }
            return View(user);
        }
    }
}
