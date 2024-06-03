using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Extensions;
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
            if (!CheckAccesses("CheckRole"))
                return View("ErrorMessage", 3);
            List<AppAccess> appAccesses = Context.AppAccesses.ToList();
            Dictionary<AppAccess, List<AdditionalAppAccess>> nmodel = new Dictionary<AppAccess, List<AdditionalAppAccess>>();
            for (int i = 0; i < appAccesses.Count; i++)
            {
                nmodel.Add(appAccesses[i], Context.AdditionalAppAccesses.Where(x => x.AppAccessId == appAccesses[i].Id).ToList());
            }
            RoleViewModel checkRoleModelView = new RoleViewModel()
            {
                AppAccesses = nmodel
            };
            checkRoleModelView.Role = Context.Roles.First(x => x.Id == id);
            int roleId = checkRoleModelView.Role.Id;
            List<AppAccess> appAccessesForRole = Context.AppAccesses.Where(x => Context.RolesAppAccesses.Where(x => x.RoleId == roleId).Select(x => x.AppAccessId).Contains(x.Id)).ToList();
            Dictionary<AppAccess, List<AdditionalAppAccess>> nmodelForRole = new Dictionary<AppAccess, List<AdditionalAppAccess>>();
            for (int i = 0; i < appAccessesForRole.Count; i++)
            {
                nmodelForRole.Add(appAccessesForRole[i], Context.AdditionalAppAccesses.Where(x => x.AppAccessId == appAccessesForRole[i].Id && Context.RolesAdditionalAppAccesses.Any(y => y.AdditionalAppAccessId == x.Id && y.RoleId == roleId)).ToList());
            }
            checkRoleModelView.AppAccessesForRole = nmodelForRole;
            return View(checkRoleModelView);
        }

        [HttpPost]
        public async Task<IActionResult> RoleDelete(int id)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("RoleDelete"))
                return View("ErrorMessage", 3);
            while (Context.RolesAppAccesses.Any(x => x.RoleId == id))
                Context.RolesAppAccesses.Remove(Context.RolesAppAccesses.First(x => x.RoleId == id));
            Context.Roles.Remove(Context.Roles.First(x => x.Id == id));
            await Context.SaveChangesAsync();
            return RedirectToAction("Roles");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRoleAccesses(int roleId, int[] accesses, int[] additionals)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("ChangeRoleAccesses"))
                return View("ErrorMessage", 3);
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
                    List<RoleAdditionalAppAccess> additionalAppAccesses = Context.RolesAdditionalAppAccesses.Where(x => Context.AdditionalAppAccesses.Select(y => y.AppAccessId).Contains(roleAppAccesses[i].AppAccessId)).ToList();
                    for (int j = 0; j < additionalAppAccesses.Count; j++)
                    {
                        Context.RolesAdditionalAppAccesses.Remove(additionalAppAccesses[i]);
                    }
                    Context.RolesAppAccesses.Remove(roleAppAccesses[i]);
                }
            }
            await Context.SaveChangesAsync();
            foreach (int item in additionals)
            {
                if (!Context.RolesAdditionalAppAccesses.Any(x => x.RoleId == roleId && x.AdditionalAppAccessId == item))
                {
                    Context.RolesAdditionalAppAccesses.Add(new RoleAdditionalAppAccess() { AdditionalAppAccessId = item, RoleId = roleId });
                }
            }
            if (Context.RolesAdditionalAppAccesses.Any(x => x.RoleId == roleId && !additionals.Contains(x.AdditionalAppAccessId)))
            {
                List<RoleAdditionalAppAccess> roleAppAccesses = Context.RolesAdditionalAppAccesses.Where(x => x.RoleId == roleId && !additionals.Contains(x.AdditionalAppAccessId)).ToList();
                for (int i = 0; i < roleAppAccesses.Count; i++)
                {
                    Context.RolesAdditionalAppAccesses.Remove(roleAppAccesses[i]);
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
            if (!CheckAccesses("AddRole"))
                return View("ErrorMessage", 3);

            List<AppAccess> appAccesses = Context.AppAccesses.ToList();
            Dictionary<AppAccess, List<AdditionalAppAccess>> nmodel = new Dictionary<AppAccess, List<AdditionalAppAccess>>();
            for (int i = 0; i < appAccesses.Count; i++)
            {
                nmodel.Add(appAccesses[i], Context.AdditionalAppAccesses.Where(x => x.AppAccessId == appAccesses[i].Id).ToList());
            }
            AddRoleModelView model = new AddRoleModelView()
            {
                AppAccesses = nmodel
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string name, int[] accesses, int[] additionals)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("AddRole"))
                return View("ErrorMessage", 3);
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
            foreach (int i in additionals)
            {
                Context.RolesAdditionalAppAccesses.Add(new RoleAdditionalAppAccess()
                {
                    RoleId = roleId,
                    AdditionalAppAccessId = i
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
            if (!CheckAccesses("CheckUser"))
                return View("ErrorMessage", 3);
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
        public async Task<IActionResult> ChangeUser(int userId, string type, string imgSrc, string login, string firstname, string lastname, int roleId, string phone, string email, int positionId)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("ChangeUser"))
                return View("ErrorMessage", 3);
            if (Context.LoginsPasswords.Any(x => x.Login == login))
                return View("ErrorMessage", 0);
            Context.LoginsPasswords.First(x => x.Id == Context.Users.First(x => x.Id == userId).LoginPasswordId).Login = login;
            Context.Users.First(x => x.Id == userId).ImageSrc = imgSrc;
            Context.Users.First(x => x.Id == userId).Firstname = firstname;
            Context.Users.First(x => x.Id == userId).Lastname = lastname;
            Context.Users.First(x => x.Id == userId).RoleId = roleId;
            await Context.SaveChangesAsync();
            if (type == "with")
            {
                WorkerAdditional? wadd = Context.WorkersAdditionals.FirstOrDefault(x => x.UserId == userId);
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
                    await Context.SaveChangesAsync();
                }
                else
                {
                    Context.WorkersAdditionals.First(x => x.Id == wadd.Id).PositionId = positionId;
                    Context.ContactDetails.First(x => x.Id == wadd.ContactDetailId).Email = email;
                    Context.ContactDetails.First(x => x.Id == wadd.ContactDetailId).PhoneNumber = phone;
                    await Context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("DeleteUser"))
                return View("ErrorMessage", 3);
            User user = Context.Users.First(x => x.Id == userId);
            Context.LoginsPasswords.Remove(Context.LoginsPasswords.First(x => x.Id == user.LoginPasswordId));
            if (Context.WorkersAdditionals.Any(x => x.UserId == userId))
            {
                Context.WorkersAdditionals.Remove(Context.WorkersAdditionals.First(x => x.UserId == userId));
            }
            Context.Users.Remove(Context.Users.First(x => x.Id == userId));
            await Context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        public class PageRequest
        {
            public string Page { get; set; }
        }

        [HttpPost]
        public IActionResult CheckPage([FromBody] PageRequest request)
        {
            string page = request.Page;
            if (!page.EndsWith(".cshtml"))
                page += ".cshtml";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Home", page.TrimStart('/'));
            if (System.IO.File.Exists(path))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult Accesses()
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Accesses"))
                return View("ErrorMessage", 3);
            AccessesViewModel accessesViewModel = new AccessesViewModel()
            {
                AppAccesses = Context.AppAccesses.ToList(),
            };
            return View(accessesViewModel);
        }

        [HttpPost]
        public IActionResult CheckAccess(int id)
        {

            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("CheckAccess"))
                return View("ErrorMessage", 3);

            return View(Context.AppAccesses.First(x => x.Id == id));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeAccess(int id, string name, string href)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("ChangeAccess"))
                return View("ErrorMessage", 3);
            Context.AppAccesses.First(x => x.Id == id).Name = name;
            Context.AppAccesses.First(x => x.Id == id).Href = href;
            await Context.SaveChangesAsync();
            return RedirectToAction("Accesses");
        }

        [HttpPost]
        public async Task<IActionResult> AddAccess(string name, string href)
        {

            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("Bids"))
                return View("AddAccess", 3);
            if (Context.AppAccesses.Any(x => x.Name == name) || Context.AppAccesses.Any(x => x.Href == href))
                return View("ErrorMessage", 5);
            Context.AppAccesses.Add(new AppAccess()
            {
                Href = href,
                Name = name
            });
            await Context.SaveChangesAsync();
            return RedirectToAction("Accesses");
        }

        [HttpPost]
        public async Task<IActionResult> AccessDelete(int id)
        {
            if (!IsAuthentificated())
                return View("Authorization");
            if (!CheckAccesses("AccessDelete"))
                return View("ErrorMessage", 3);
            while (Context.RolesAppAccesses.Any(x => x.AppAccessId == id))
                Context.RolesAppAccesses.Remove(Context.RolesAppAccesses.First(x => x.AppAccessId == id));
            Context.AppAccesses.Remove(Context.AppAccesses.First(x => x.Id == id));
            await Context.SaveChangesAsync();
            return RedirectToAction("Accesses");
        }

        [HttpGet]
        public IActionResult AdditionalAccesses()
        {
            List<AppAccess> appAccesses = Context.AppAccesses.ToList();
            Dictionary<AppAccess, List<AdditionalAppAccess>> nmodel = new Dictionary<AppAccess, List<AdditionalAppAccess>>();
            for (int i = 0; i < appAccesses.Count; i++)
            {
                if (Context.AdditionalAppAccesses.Where(x => x.AppAccessId == appAccesses[i].Id).Count() > 0)
                    nmodel.Add(appAccesses[i], Context.AdditionalAppAccesses.Where(x => x.AppAccessId == appAccesses[i].Id).ToList());
            }
            AdditionalAccessesViewModel model = new AdditionalAccessesViewModel()
            {
                AppAccesses = nmodel
            };
            return View(model);
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
            AdditionalAppAccess? additionalAppAccess = Context.AdditionalAppAccesses.FirstOrDefault(x => x.Href == pageName);
            if (appAccess == null && additionalAppAccess == null)
                return true;
            string roleStr = GetRole();
            Role? role = Context.Roles.FirstOrDefault(x => x.Name == roleStr);
            if (role == null)
                return false;
            if (appAccess != null)
            {
                return Context.RolesAppAccesses.Any(x => x.RoleId == role.Id && x.AppAccessId == appAccess.Id);
            }
            if (additionalAppAccess != null)
            {
                return Context.RolesAdditionalAppAccesses.Any(x => x.RoleId == role.Id && x.AdditionalAppAccessId == additionalAppAccess.Id);
            }
            return false;
        }

        private bool CheckAuthorization(string role)
        {
            return Authentification.CheckAuthorization(HttpContext.Session.GetString("token"), HttpContext.Session.GetString("key"), role);
        }
    }
}
