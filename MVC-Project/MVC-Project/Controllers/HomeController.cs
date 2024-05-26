using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Project.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MVC_Project.Controllers
{
    public class HomeController : Controller
    {
        public ManagemantAppDbContext Context { get; private set; }

        public HomeController(ManagemantAppDbContext db)
        {
            Context = db;
        }

        private void InitialSettings()
        {

        }

        public IActionResult Index()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Index"))
                return View("ErrorMessage", 3);
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(string login, string imgSrc, string password, string password2, string firstname, string lastname)
        {
            if (password != password2)
                return View("ErrorMessage", 1);
            if (Context.LoginsPasswords.Any(x => x.Login == login))
                return View("ErrorMessage", 0);
            LoginPassword lp = new LoginPassword()
            {
                Login = login,
                Password = password,
            };
            Context.LoginsPasswords.Add(lp);
            await Context.SaveChangesAsync();
            User user = new User()
            {
                Firstname = firstname,
                Lastname = lastname,
                LoginPasswordId = lp.Id,
                ImageSrc = imgSrc,
                RoleId = null
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            Context.Bids.Add(new Bid()
            {
                DateTime = DateTime.Now,
                UserId = user.Id
            });
            await Context.SaveChangesAsync();
            return View("Message", "Administrator has not accepted your bid yet, please wait");
        }

        [HttpGet]
        public IActionResult Authorization()
        {
            return View("Authorization");
        }

        [HttpPost]
        public IActionResult Authorization(string login, string password)
        {
            LoginPassword lp = Context.LoginsPasswords.FirstOrDefault(x => x.Login == login && x.Password == password);
            if (lp is null)
                return View("ErrorMessage", 2);
            User user = Context.Users.First(x => x.LoginPasswordId == lp.Id);
            if (user.RoleId is null)
                return View("Message", "Administrator has not accepted your bid yet, please wait");
            string role = Context.Roles.First(x => x.Id == user.RoleId).Name;
            SaveUser(user.Id, role);
            HttpContext.Session.SetString("accesses", JsonConvert.SerializeObject(GetAccesses()));
            return View();
        }

        [HttpGet]
        public IActionResult Bids()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Bids"))
                return View("ErrorMessage", 3);
            BidsViewModel bidsViewModel = new BidsViewModel();
            List<Bid> bids = Context.Bids.ToList();
            for (int i = 0; i < bids.Count; i++)
            {
                bids[i].User = Context.Users.First(x => x.Id == bids[i].UserId);
                bids[i].User.LoginPassword = Context.LoginsPasswords.First(x => x.Id == bids[i].User.LoginPasswordId);
            }
            bidsViewModel.Bids = bids;
            bidsViewModel.Roles = Context.Roles.ToList();
            return View(bidsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BidDelete(int id)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Bids"))
                return View("ErrorMessage", 3);
            Bid bid = Context.Bids.First(x => x.Id == id);
            User user = Context.Users.First(x => x.Id == bid.UserId);
            LoginPassword lp = Context.LoginsPasswords.First(x => x.Id == user.LoginPasswordId);
            Context.LoginsPasswords.Remove(lp);
            Context.Users.Remove(user);
            Context.Bids.Remove(bid);
            await Context.SaveChangesAsync();
            return RedirectToAction("Bids");
        }

        [HttpPost]
        public async Task<IActionResult> BidAppoint(int formBidId, int roleId)
        {
            Bid bid = Context.Bids.First(x => x.Id == formBidId);
            int userId = bid.UserId;
            Context.Bids.Remove(bid);
            await Context.SaveChangesAsync();
            Context.Users.First(x => x.Id == userId).RoleId = roleId;
            await Context.SaveChangesAsync();
            return RedirectToAction("Bids");
        }

        [HttpGet]
        public IActionResult Account()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            int userId = GetUserId();
            User user = Context.Users.First(x => x.Id == userId);
            AccountViewModel accountViewModel = new AccountViewModel()
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                ImgSrc = user.ImageSrc,
                UserId = userId
            };
            LoginPassword lp = Context.LoginsPasswords.First(x => x.Id == user.LoginPasswordId);
            accountViewModel.Login = lp.Login;
            accountViewModel.Password = lp.Password;
            return View(accountViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeAccoutData(int userId, string imgSrc, string login, string oldPassword, string password, string password2, string firstname, string lastname)
        {
            User user = Context.Users.First(x => x.Id == userId);
            LoginPassword lp = Context.LoginsPasswords.First(x => x.Id == user.LoginPasswordId);
            if (lp.Password != oldPassword)
                return View("ErrorMessage", 4);
            if (lp.Login != login)
            {
                if (Context.LoginsPasswords.Any(x => x.Login == login))
                    return View("ErrorMessage", 0);
            }
            if (password != password2)
                return View("ErrorMessage", 1);
            Context.Users.First(x => x.Id == userId).Firstname = firstname;
            Context.Users.First(x => x.Id == userId).Lastname = lastname;
            Context.Users.First(x => x.Id == userId).ImageSrc = imgSrc;
            await Context.SaveChangesAsync();
            Context.LoginsPasswords.First(x => x.Id == lp.Id).Login = login;
            Context.LoginsPasswords.First(x => x.Id == lp.Id).Password = password;
            await Context.SaveChangesAsync();
            return RedirectToAction("Account");
        }

        [HttpPost]
        public async Task<IActionResult> WorkerAdditional(string login, string phone, string email, int positionId, int roleId, string type)
        {
            if (!Context.LoginsPasswords.Any(x => x.Login == login))
                return View("ErrorMessage", 2);
            int userId = Context.Users.First(x => x.LoginPasswordId == Context.LoginsPasswords.First(x => x.Login == login).Id).Id;
            WorkerAdditional? wadd = Context.WorkersAdditionals.FirstOrDefault(x => x.UserId == userId);
            if (type == "delete")
            {
                if (wadd == null)
                    return View("ErrorMessage", 5);

                Context.WorkersAdditionals.Remove(wadd);
                await Context.SaveChangesAsync();
            }
            else if (type == "change")
            {
                if (wadd == null)
                {
                    wadd = new WorkerAdditional();
                    ContactDetail cd = new ContactDetail() { Email = email, PhoneNumber = phone };
                    Context.ContactDetails.Add(cd);
                    await Context.SaveChangesAsync();
                    wadd.ContactDetailId = cd.Id;
                    wadd.UserId = userId;
                    wadd.PositionId = positionId;
                    Context.WorkersAdditionals.Add(wadd);
                    Context.Users.First(x => x.Id == userId).RoleId = roleId;
                    await Context.SaveChangesAsync();
                }
                else
                {
                    Context.WorkersAdditionals.First(x => x.Id == wadd.Id).PositionId = positionId;
                    Context.ContactDetails.First(x => x.Id == wadd.ContactDetailId).Email = email;
                    Context.ContactDetails.First(x => x.Id == wadd.ContactDetailId).PhoneNumber = phone;
                    Context.Users.First(x => x.Id == userId).RoleId = roleId;
                    await Context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult WorkerAdditional()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("WorkerAdditional"))
                return View("ErrorMessage", 3);
            WorkerAdditionalViewModel workerAdditionalViewModel = new WorkerAdditionalViewModel()
            {
                Positions = Context.Positions.ToList(),
                Roles = Context.Roles.ToList()
            };
            return View(workerAdditionalViewModel);
        }

        [HttpGet]
        public IActionResult Roles()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Roles"))
                return View("ErrorMessage", 3);
            List<Role> roles = Context.Roles.ToList();
            return View(roles);
        }

        [HttpPost]
        public IActionResult CheckRole(int id)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            RoleViewModel checkRoleModelView = new RoleViewModel()
            {
                AllAppAccess = Context.AppAccesses.ToList()
            };
            checkRoleModelView.Role = Context.Roles.First(x => x.Id == id);
            int roleId = checkRoleModelView.Role.Id;
            checkRoleModelView.AppAccessForRole = Context.AppAccesses.Where(x => Context.RolesAppAccesses.Where(x => x.RoleId == roleId).Select(x => x.AppAccessId).Contains(x.Id)).ToList();
            return View(checkRoleModelView);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRoleAccesses(int roleId, int[] accesses)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            foreach (int item in accesses)
            {
                if (!Context.RolesAppAccesses.Any(x => x.RoleId == roleId && x.AppAccessId == item))
                {
                    Context.RolesAppAccesses.Add(new RoleAppAccess() { AppAccessId = item, RoleId = roleId });
                }
            }
            if (Context.RolesAppAccesses.Any(x => x.RoleId == roleId && !accesses.Contains(x.AppAccessId)))
            {
                List<RoleAppAccess> roleAppAccesses = Context.RolesAppAccesses.Where(x => x.RoleId == roleId && !accesses.Contains(x.AppAccessId)).ToList();
                for (int i = 0; i < roleAppAccesses.Count; i++)
                {
                    Context.RolesAppAccesses.Remove(roleAppAccesses[i]);
                }
            }
            await Context.SaveChangesAsync();
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            return View(Context.AppAccesses.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string name, int[] accesses)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            Role role = new Role()
            {
                Name = name
            };
            Context.Roles.Add(role);
            await Context.SaveChangesAsync();
            int roleId = role.Id;
            foreach (int i in accesses)
            {
                Context.RolesAppAccesses.Add(new RoleAppAccess()
                {
                    RoleId = roleId,
                    AppAccessId = i
                });
            }
            await Context.SaveChangesAsync();
            return RedirectToAction("Roles");
        }
        [HttpPost]
        public IActionResult Exit()
        {
            HttpContext.Session.Remove("token");
            HttpContext.Session.Remove("key");
            HttpContext.Session.Remove("accesses");
            return View("Authorization");
        }

        [HttpGet]
        public IActionResult Users()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Users"))
                return View("ErrorMessage", 3);
            List<User> users = Context.Users.Where(x => x.RoleId != null).ToList();
            for (int i = 0; i < users.Count; i++)
            {
                users[i].Role = Context.Roles.First(x => x.Id == users[i].RoleId);
                users[i].LoginPassword = Context.LoginsPasswords.First(x => x.Id == users[i].LoginPasswordId);
            }
            return View(users);
        }

        [HttpPost]
        public IActionResult CheckUser(int userId)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            UserViewModel userViewModel = new UserViewModel()
            {
                User = Context.Users.First(x => x.Id == userId),
                WorkerAdditional = Context.WorkersAdditionals.FirstOrDefault(x => x.UserId == userId)
            };
            userViewModel.User.Role = Context.Roles.First(x => x.Id == userViewModel.User.RoleId);
            userViewModel.Login = Context.LoginsPasswords.First(x => x.Id == userViewModel.User.LoginPasswordId).Login;
            userViewModel.ContactDetail = userViewModel.WorkerAdditional != null ? Context.ContactDetails.FirstOrDefault(x => x.Id == userViewModel.WorkerAdditional.ContactDetailId) : null;
            userViewModel.Positions = Context.Positions.ToList();
            userViewModel.Roles = Context.Roles.ToList();
            return View(userViewModel);
        }

        [HttpPost]
        public IActionResult ChangeUser(int userId, string type, string imgSrc, string login, string firstname, string lastname, int roleId, string phone, string email, int positionId)
        {
            if(type == "with")
            {

            }
            else if(type == "without")
            {

            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private int GetUserId()
        {
            string token = HttpContext.Session.GetString("token");
            string key = HttpContext.Session.GetString("key");
            return Authentification.GetIdOfCurrentUser(token, key);
        }

        private void SaveUser(int userId, string role)
        {
            List<string> list = Authentification.GenerateToken(userId, role);
            HttpContext.Session.SetString("token", list[0]);
            HttpContext.Session.SetString("key", list[1]);
        }

        private List<AppAccess>? GetAccesses()
        {
            if (!IsAuthentificated())
                return null;
            string role = GetRole();
            if (string.IsNullOrEmpty(role))
                return null;
            int roleId = Context.Roles.First(x => x.Name == role).Id;
            List<AppAccess> accesses = Context.AppAccesses.Join(Context.RolesAppAccesses, x => x.Id, y => y.AppAccessId, (x, y) => new { AppAccess = new AppAccess() { Href = x.Href, Name = x.Name }, y.RoleId }).Where(x => x.RoleId == roleId).Select(x => x.AppAccess).ToList();
            if (!accesses.Any())
                return null;
            return accesses;
        }

        private bool IsAuthentificated()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")) && !string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
            {
                return true;
            }
            return false;
        }

        private string GetRole()
        {
            string? token = HttpContext.Session.GetString("token");
            string? key = HttpContext.Session.GetString("key");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(key))
            {
                return "";
            }

            return Authentification.GetRoleOfCurrentUser(token, key);
        }

        private bool CheckAccesses(string pageName)
        {
            AppAccess? appAccess = Context.AppAccesses.FirstOrDefault(x => x.Href == pageName);
            string roleStr = GetRole();
            Role? role = Context.Roles.FirstOrDefault(x => x.Name == roleStr);
            if (appAccess == null || role == null)
                return false;
            return Context.RolesAppAccesses.Where(x => x.AppAccessId == appAccess.Id).Any(x => x.RoleId == role.Id);
        }

        private bool CheckAuthorization(string role)
        {
            return Authentification.CheckAuthorization(HttpContext.Session.GetString("token"), HttpContext.Session.GetString("key"), role);
        }
    }
}
