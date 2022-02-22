using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRepo.Data;
using MyRepo.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyRepo.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationDbContext> userManager;
        
        public ItemController(ApplicationDbContext db) => _db = db;

        // GET: ItemController
        
        [Route("[controller]/{id}")]
        public ActionResult Index(string id)
        {
            UserItem[] userItems;
            IEnumerable<UserItem> users;
            string user = id.ToLower();
            ViewBag.IsOwnRepo = IsOwnRepo(user);
            ViewBag.HasContent = true;

            if (User.Identity.Name == user)
            {
                IdentityUser identityUser = _db.Users.Single(x => x.UserName == User.Identity.Name);
                userItems = _db.UserItems.Include(x => x.Item).Include(x => x.User).Where(x => x.User == _db.Users.Single(x => x.UserName == User.Identity.Name)).ToArray();
            }
            else
            {
                IdentityUser identityUser = _db.Users.Single(x => x.UserName == user);
                userItems = _db.UserItems.Include(x => x.Item).Include(x => x.User).Where(x => x.User == identityUser && x.Item.isPublic).ToArray();
            }

            return View(userItems.ToList());
        }

        //PUT: request
        [HttpPut]
        public HttpResponseMessage Save([FromBody] FileSaveData data)
        {
            Item item = _db.Items.First(x => x.Id == data.Id);
            item.Description = data.Description;
            item.isPublic = data.IsPublic;

            _db.Items.Update(item);
            _db.SaveChanges();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public class FileSaveData
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public bool IsPublic { get; set; }
        }
        

        // GET: ItemController/Create
        [Authorize]
        [Route("[controller]/{id}/[action]")]
        public ActionResult Create(string id)
        {
            return View();
        }

        // POST: ItemController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Createe(Item item, IFormCollection a)
        {
            if (!ValidateFileName(Path.GetFileNameWithoutExtension(a.Files[0].FileName)))
            {
                Err err = new Err("File name is not accepted");
                ViewBag.Error = err.ErrorMsg;
                return View();
            }
            if (item != null)
            {
                Item item1 = item;
                item1.Owner = _db.Users.Single(x => x.UserName == User.Identity.Name);

                var file = a.Files[0];
                string path = @$"{Directory.GetCurrentDirectory()}\Files\{item1.Owner}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] files = Directory.GetFiles(path);
                int counter = 0;
                string fn = file.FileName;
                foreach (string _file in files)
                {
                    if (_file == @$"{path}\{fn}")
                    {
                        fn = $"({++counter})" + file.FileName;
                    }
                }

                using (var reading = file.OpenReadStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(System.IO.File.OpenWrite(@$"{path}\{fn}")))
                    {
                        reading.CopyTo(writer.BaseStream);
                    }
                }

                item1.Name = fn;
                item1.Size = file.Length / 1024;

                UserItem userItem = new UserItem();
                userItem.Item = item1;
                userItem.User = item1.Owner;
                _db.UserItems.Add(userItem);

                _db.Items.Add(item1);
                
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Err err = new Err("Item was null");
                ViewBag.Error = err.ErrorMsg;
                return View();
            }
        }

        // GET: ItemController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ItemController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/{id}/[action]")]
        public IActionResult GetFile(string id, IFormCollection name)
        {
            string fileId = name.Keys.ToArray()[0].ToString();

            Item item = _db.Items.FirstOrDefault(x => x.Id == Convert.ToInt32(fileId));

            if (item == null) return null;

            string file = item.Name;
            
            string fileLocation = @$"{GetFilePath()}\{id.ToLower()}\{item.Name}";

            if (IsOwnRepo(id.ToLower()) || item.isPublic)
                return File(System.IO.File.ReadAllBytes(fileLocation), "application/x-zip-compressed", file);
            else return null;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("[controller]/{id}/[action]")]
        public HttpResponseMessage Delete([FromBody] DeleteData del)
        {
            IdentityUser identityUser = _db.Users.Single(x => x.UserName == User.Identity.Name);
            Item item = _db.Items.FirstOrDefault(e => e.Id == del.Id);

            if (ValidateFileName(Path.GetFileNameWithoutExtension(item.Name)))
            {
                _db.Items.Remove(item);
                string file = item.Name;
                string fileLocation = @$"{GetFilePath()}\{User.Identity.Name}\{file}";

                System.IO.File.Delete(fileLocation);

                _db.SaveChanges();
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public class DeleteData
        {
            public int Id { get; set; }
        }

        bool ValidateFileName(string fileName)
        {
            foreach(char c in fileName)
            {
                if(!((int)c >= 65 && (int)c <= 90) && !((int)c >= 97 && (int)c <= 122))
                {
                    return false;
                }
            }

            return true;
        }

        private string GetFilePath()
        { 
            return @$"{Directory.GetCurrentDirectory()}\Files\";
        }

        public IActionResult Error()
        {
            return View();
        }

        bool IsOwnRepo(string id)
        {
            if (User.Identity.Name == id) return true;

            return false;
        }

        class Err
        {
            public string ErrorMsg { get; set; }

            public Err(string err)
            {
                ErrorMsg = err;
            }
        }
    }
}
