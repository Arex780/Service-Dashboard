using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebArchive.Models;
using WebArchive.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using WebArchive.Filters;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.AspNetCore.Authorization;

namespace WebArchive.Controllers
{

    public class ProjectController : Controller
    {
        private readonly ILogger<ProjectController> logger;
        private readonly WebDataContext database;

        public ProjectController(WebDataContext db, ILogger<ProjectController> log_session)
        {
            database = db;
            logger = log_session;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("")]
        public async Task<ActionResult> Index(string sort, string cat, string stat)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sort) ? "name_desc" : "";
            ViewData["CategorySortParm"] = sort == "category" ? "category_desc" : "category";
            ViewData["StatusSortParm"] = sort == "status" ? "status_desc" : "status";

            var projects = from s in database.Projects
                select s;

            ViewData["CategoryFilter"] = cat;
            ViewData["StatusFilter"] = stat;

            if (!String.IsNullOrEmpty(cat))
                projects = projects.Where(s => s.Category.Equals(cat));
            if (!String.IsNullOrEmpty(stat))
                projects = projects.Where(s => s.Status == (Status)Enum.Parse(typeof(Status), stat));

            string[] category = projects.Where(s => s.Category != null).Select(s => s.Category).ToArray().Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();

            Status[] status = projects.Select(s => s.Status).ToArray().Distinct().ToArray();

            ViewData["CategoryList"] = category;
            ViewData["StatusList"] = status;
            ViewData["SelectedSort"] = sort;

            switch (sort)
            {
                case "name_desc":
                    projects = projects.OrderByDescending(s => s.Name);
                    break;
                case "status_desc":
                    projects = projects.OrderByDescending(s => s.Status);
                    break;
                case "status":
                    projects = projects.OrderBy(s => s.Status);
                    break;
                case "category_desc":
                    projects = projects.OrderByDescending(s => s.Category);
                    break;
                case "category":
                    projects = projects.OrderBy(s => s.Category);
                    break;
                default:
                    projects = projects.OrderBy(s => s.Name);
                    break;
            }
            return View(await projects.AsNoTracking().ToArrayAsync());
        }

        [Route("Logs")]
        public IActionResult Logs()
        {
            ViewData["Enum"] = database.Projects.OrderBy(x => x.Name).ToArray();
            return View();
        }

        [Route("Project/Log/{key}")]
        public IActionResult Log(string key)
        {
            ViewData["Enum"] = database.Projects.OrderBy(x => x.Name).ToArray();
            var app = database.Projects.FirstOrDefault(x => x.Keygen == key);
            return View(app);
        }

        [Route("Project/{key}")]
        public IActionResult Project(string key)
        {
            ViewData["Enum"] = database.Projects.OrderBy(x => x.Name).ToArray();
            var app = database.Projects.FirstOrDefault(x => x.Keygen == key);
            return View(app);
        }

        [Authorize(Policy = "IsBlueBadge")]
        [HttpGet, Route("Project/add")]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Policy = "IsBlueBadge")]
        [HttpPost, Route("Project/add")]
        public async Task<IActionResult> Add(Project project, IFormFile imageUI, IFormFile imageLogo)
        {
            if (database.Projects.Any(x => x.Keygen == project.Keygen))
                ModelState.AddModelError("Project", "This project already exists");

            if (!ModelState.IsValid) 
                return View();

            project.PostCreator = User.Identity.Name;
            project.PostTime = DateTime.Now;
            project.EditTime = DateTime.Now;

            if (imageLogo != null)
                project.Logo = ConvertImageToByteArray(imageLogo);
            if (imageUI != null)
                project.ImageUI = ConvertImageToByteArray(imageUI);

            project.Status = await StatusChecker.CheckIfOnline(project);

            database.Projects.Add(project);
            database.SaveChanges();
            Utils.LogHandler.ProjectAddLog(project, User.Identity.Name);

            return RedirectToAction("Project", "Project", new
            {
                key = project.Keygen
            });
        }

        [HttpGet, Route("Project/Edit/{key}")]
        public IActionResult Edit(string key)
        {
            var app = database.Projects.FirstOrDefault(x => x.Keygen == key);
            if (!IsAuthorized(app))
            {
                return Forbid();
            }
            return View(app);
        }

        [HttpPost, Route("Project/Edit/{key}")]
        public async Task<IActionResult> Edit([Bind("Id,Keygen,Name,Authors,Owner,WebAdress,Description,PostCreator," +
            "PostTime,EditTime,Status,Repository,Team,WrittenIn,Userland,Logo,ImageUI,Version,ShortDesc,Category")] 
        Project project, IFormFile imageUI, IFormFile imageLogo, string previousKeygen, bool deleteUI, bool deleteLogo)
        {
            if (!IsAuthorized(project))
            {
                return Forbid();
            }

            if (project.Keygen != previousKeygen && (database.Projects.Any(x => x.Keygen == project.Keygen)))
                ModelState.AddModelError("Project", "This project already exists");

            project.EditTime = DateTime.Now;

            // Delete image from database if checkbox is checked
            if (deleteUI == true)
                project.ImageUI = null;
            if (deleteLogo == true)
                project.Logo = null;

            // Check if image was updated
            if (imageLogo != null && deleteLogo == false)
                project.Logo = ConvertImageToByteArray(imageLogo);
            if (imageUI != null && deleteUI == false)
                project.ImageUI = ConvertImageToByteArray(imageUI);

            project.Status = await StatusChecker.CheckIfOnline(project);

            if (ModelState.IsValid)
            {
                try
                {
                    database.Update(project);
                    await database.SaveChangesAsync(User.Identity.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExist(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(project);
        }

        private bool ProjectExist(long id)
        {
            return database.Projects.Any(x => x.Id == id);
        }

        [HttpGet, Route("Project/Delete/{key}")]
        public  IActionResult Delete(string key)
        {
            var app = database.Projects.FirstOrDefault(x => x.Keygen == key);
            if (!IsAuthorized(app))
            {
                return Forbid();
            }
            return View(app);
        }

        [HttpPost, Route("Project/Delete/{key}")]
        public async Task<IActionResult> Delete(long id)
        {
            var project = await database.Projects.FindAsync(id);
            if (!IsAuthorized(project))
            {
                return Forbid();
            }
            database.Projects.Remove(project);
            await database.SaveChangesAsync();
            Utils.LogHandler.ProjectDeleteLog(project, User.Identity.Name);
            return RedirectToAction("Index");
        }

        public byte[] ConvertImageToByteArray(IFormFile image)
        {
            byte[] byteArray = null;

                if (image.Length > 0)
                {
                    using (var fileStream = image.OpenReadStream())
                        using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);

                        // Upload if file is less than 1MB
                        if (memoryStream.Length < 1048576)
                            byteArray = memoryStream.ToArray();
                        else
                            ModelState.AddModelError("Image", "The file is too large (max. 1MB)");
                    }
                    return byteArray;
                }
                else
                    return null;
        }

        private bool IsAuthorized(Project project)
        {
            return (User.Identity.Name == project.PostCreator || 
                   ((AdminConfig)ViewBag.AdminConfig).AdminList.Any(s => s == User.Identity.Name));
        }

    }
}
