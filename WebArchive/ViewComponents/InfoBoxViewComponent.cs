using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebArchive.Data;

namespace WebArchive.ViewComponents
{
   [ViewComponent(Name = "InfoBox")]
    public class InfoBoxViewComponent : ViewComponent
    {
        private readonly WebDataContext database;

        public InfoBoxViewComponent(WebDataContext db)
        {
            database = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string key)
        {
            var app = database.Projects.FirstOrDefault(x => x.Keygen == key);
            return View(app);
        }
    }
}
