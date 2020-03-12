using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Data;
using QuizWebApp.Models;
using QuizWebApp.ViewModels;

namespace QuizWebApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public PaginationViewModel<T> GetViewModelData<T>(int pageLength, int pageNumber, IQueryable<T> query)
        {
            //query je bud cely DbSet<T>, alebo uz odfiltrovany pomocou .Where() ak bol searchString != null

            var paginationViewModel = new PaginationViewModel<T>();

            paginationViewModel.Entities = query.Skip((pageNumber - 1) * pageLength).Take(pageLength).ToList();
            paginationViewModel.PageCount = (int)Math.Ceiling((double)query.Count() / pageLength);

            if (paginationViewModel.PageCount == 0)
                paginationViewModel.PageCount = 1;

            paginationViewModel.PageLength = pageLength;
            paginationViewModel.CurrentPage = pageNumber;

            return paginationViewModel;
        }
    }
}