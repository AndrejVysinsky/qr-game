using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuizWebApp.Data;
using QuizWebApp.ViewModels;

namespace QuizWebApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IWebHostEnvironment _hostEnvironment;

        public BaseController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public PaginationViewModel<T> GetViewModelData<T>(string searchString, int pageLength, int pageNumber, IQueryable<T> query)
        {
            //query je bud cely DbSet<T>, alebo uz odfiltrovany pomocou .Where() ak bol searchString != null

            var paginationViewModel = new PaginationViewModel<T>();

            paginationViewModel.Entities = query.Skip((pageNumber - 1) * pageLength).Take(pageLength).ToList();
            paginationViewModel.Entities.Reverse();
            paginationViewModel.PageCount = (int)Math.Ceiling((double)query.Count() / pageLength);

            if (paginationViewModel.PageCount == 0)
                paginationViewModel.PageCount = 1;

            paginationViewModel.PageLength = pageLength;
            paginationViewModel.CurrentPage = pageNumber;
            paginationViewModel.SearchInput = searchString;

            return paginationViewModel;
        }
    }
}