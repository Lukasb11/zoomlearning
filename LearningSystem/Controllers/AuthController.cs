using LearningSystem.Models;
using LearningSystem.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RestSharp;
using System;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;


namespace LearningSystem.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Index()
        {
            string zoomCode = Request.QueryString["code"];

            if (zoomCode != null)
            {
                createZoomAuth(zoomCode);
                return View("../User/Success");
            }
            else
            {
                dbEntities _db = new dbEntities();
                zoom newzoom = new zoom();
                var name = System.Web.HttpContext.Current.User.Identity.Name;
                int userId = string.IsNullOrEmpty(name) ? -1 : int.Parse(name);
                var user = _db.users.Where(x => x.user_id == userId).FirstOrDefault();
                if (user != null)
                {
                    ViewBag.Name = user.first_name;
                }
                return View();
            }
        }

        public ActionResult Register()
        {
            return View("Register");
        }
        public ActionResult Login()
        {
            return View("Login");
        }

        public void createZoomAuth(string zoomCode)
        {
            dbEntities _db = new dbEntities();
            zoom newzoom = new zoom();
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            
            var data = _db.zooms.Where(x => x.user_id == userId && x.is_active == 1).FirstOrDefault();
            if (data != null)
            {
                data.is_active = 0;
                _db.SaveChanges();
            }
            
            newzoom.user_id = userId;
            newzoom.auth_token = zoomCode;
            newzoom.is_active = 1;

            _db.zooms.Add(newzoom);
            _db.SaveChanges();

            getTokens(_db, newzoom);
        }	

        public void getTokens(dbEntities _db, zoom zoom)
        {
            RestRequest restRequest = new RestRequest();
            var client = new RestClient("https://zoom.us/oauth/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic XXX");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", zoom.auth_token);
            request.AddParameter("redirect_uri", "https://localhost:44378");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                JObject joResponse = JObject.Parse(response.Content);
                zoom.access_token = joResponse["access_token"].ToString().Replace("{","").Replace("}", "");
                zoom.refresh_token = joResponse["refresh_token"].ToString().Replace("{", "").Replace("}", "");
                _db.SaveChanges();
            }
            else
            {
                // TODO bad response
            }
        }

        [HttpPost]
        public ActionResult SaveRegisterDetails(Register registerDetails)
        {
            if (ModelState.IsValid)
            {
                using (var databaseContext = new dbEntities())
                {
                    user newUser = new user();
                    string base64Pass = "";
                    byte[] rgbKey = new byte[] { XXX };
                    string base64IV;

                    using (AesManaged myAes = new AesManaged())
                    {
                        myAes.Padding = PaddingMode.PKCS7;
                        byte[] ivKey = myAes.IV;
                        base64IV = Convert.ToBase64String(ivKey);
                        byte[] encrypted = EncryptStringToBytes_Aes(registerDetails.password, rgbKey, myAes.IV);

                        base64Pass = Convert.ToBase64String(encrypted, 0, encrypted.Length);
                    }

                    newUser.first_name = registerDetails.first_name;
                    newUser.last_name = registerDetails.last_name;
                    newUser.email = registerDetails.email;
                    newUser.password = base64Pass + '|' + base64IV;
                    newUser.type = registerDetails.lecturer ? 2 : 1;

                    databaseContext.users.Add(newUser);
                    databaseContext.SaveChanges();
                }

                ViewBag.Message = "User Details Saved";
                return View("Login");
            }
            else
            {
                return View("Register", registerDetails);
            }
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isValidUser = IsValidUser(model);

                if (isValidUser != null)
                {
                    FormsAuthentication.SetAuthCookie(isValidUser.user_id+"", true);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Failure", "Wrong Username and password combination !");
                    return View();
                }
            }
            else
            {
                return View(model);
            }
        }

        public user IsValidUser(LoginViewModel model)
        {
            using (var dataContext = new dbEntities())
            {
                user user = dataContext.users.Where(query => query.email.Equals(model.email)).FirstOrDefault();
                
                byte[] passByte = Convert.FromBase64String(user.password.Split('|')[0]);
                string decryptedPass = "";
                string base64IV = user.password.Substring(user.password.LastIndexOf('|') + 1);
                byte[] IV = Convert.FromBase64String(base64IV);
                byte[] rgbKey = new byte[] { XXX };
                using (AesManaged myAes = new AesManaged())
                {
                    myAes.Padding = PaddingMode.PKCS7;
                    decryptedPass = DecryptStringFromBytes_Aes(passByte, rgbKey, IV);
                }
                Session["Type"] = user.type;
                if (model.password != decryptedPass)
                    return null;
                else
                    return user;
            }
        }


        public ActionResult Logout()
        {
            try
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
            }
            catch (Exception ex)
            { }
            return RedirectToAction("Index");
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = PaddingMode.PKCS7;
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = PaddingMode.PKCS7;
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

    }
}