using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Data;
using QuizWebApp.Models;
using QuizWebApp.ViewModels;

namespace QuizWebApp.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class ContestsController : BaseController
    {
        public ContestsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment) : base(context, hostEnvironment)
        {
        }

        public ViewResult Index()
        {
            var contestPaginationViewModel = new PaginationViewModel<Contest>();

            contestPaginationViewModel.Entities = _context.Contests.Take(contestPaginationViewModel.PageLength).ToList();
            contestPaginationViewModel.PageCount = (int)Math.Ceiling((double)_context.Contests.Count() / contestPaginationViewModel.PageLength);

            return View(contestPaginationViewModel);
        }

        public ActionResult GetPartialViewData(string searchString, int pageLength, int pageNumber)
        {
            IQueryable<Contest> query = _context.Contests;

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(q => q.Name.Contains(searchString));

            var contestPaginationViewModel = GetViewModelData(searchString, pageLength, pageNumber, query);

            return PartialView("_ContestsPartial", contestPaginationViewModel);
        }

        public ActionResult Create()
        {
            var questions = _context.Questions.ToList();

            var viewModel = new ContestViewModel()
            {
                SelectedQuestions = new List<KeyValuePair<Question, bool>>(),
                SelectedQuestion = string.Empty,
                Questions = questions
            };

            return View("ContestForm", viewModel);
        }

        [HttpPost]
        public ActionResult Create(ContestViewModel viewModel)
        {
            var contest = new Contest();

            if (viewModel.ContestId != 0)
            {
                contest = _context.Contests.Include(cq => cq.ContestQuestions).SingleOrDefault(c => c.Id == viewModel.ContestId);
                _context.ContestQuestions.RemoveRange(contest.ContestQuestions);
            }

            contest.ContestQuestions = new List<ContestQuestion>();

            contest.Name = viewModel.ContestName;

            var questions = _context.Questions.ToList();

            int questionNumber = 1;
            for (int i = 0; i < viewModel.SelectedQuestions.Count; i++)
            {
                for (int j = 0; j < questions.Count; j++)
                {
                    if (questions[j].Id == viewModel.SelectedQuestions[i].Key.Id)
                    {
                        contest.ContestQuestions.Add(new ContestQuestion()
                        { Contest = contest, Question = questions[j], QuestionNumber = questionNumber });
                        questionNumber++;
                    }
                }
            }

            if (viewModel.ContestId == 0)
                _context.Contests.Add(contest);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddQuestion(ContestViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.SelectedQuestion))
            {
                Question selected = viewModel.Questions.FirstOrDefault(q => q.Name == viewModel.SelectedQuestion);

                if (selected != null)
                {
                    if (viewModel.SelectedQuestions == null)
                        viewModel.SelectedQuestions = new List<KeyValuePair<Question, bool>>();

                    viewModel.SelectedQuestions.Add(new KeyValuePair<Question, bool>(selected, false));
                    viewModel.Questions.Remove(selected);
                }
                viewModel.SelectedQuestion = string.Empty;
                ModelState.Clear();
            }
            return View("ContestForm", viewModel);
        }

        [HttpPost]
        public ActionResult RemoveQuestion(ContestViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.SelectedQuestion))
            {
                KeyValuePair<Question, bool> selected = viewModel.SelectedQuestions.First(q => q.Key.Name == viewModel.SelectedQuestion);

                if (selected.Key != null)
                {
                    viewModel.Questions.Add(selected.Key);
                    viewModel.SelectedQuestions.Remove(selected);
                }
                viewModel.SelectedQuestion = string.Empty;
                ModelState.Clear();
            }
            return View("ContestForm", viewModel);
        }

        public ActionResult Details(int id)
        {
            var contest = _context.Contests.SingleOrDefault(c => c.Id == id);
            var cq = _context.ContestQuestions.Include(cq => cq.Question).Include(cq => cq.ContestQuestionUsers).Where(cq => cq.ContestId == id).OrderBy(cq => cq.QuestionNumber).ToList();
            
            contest.ContestQuestions = cq;

            if (contest == null)
                return NotFound();

            return View("Details", contest);
        }

        public ActionResult Edit(int id)
        {
            var contest = _context.Contests.Include(cq => cq.ContestQuestions).ThenInclude(cq => cq.Question).SingleOrDefault(c => c.Id == id);

            if (contest == null)
                return NotFound();

            var selectedQuestions = new List<KeyValuePair<Question, bool>>();

            bool warning = false;
            foreach (var item in contest.ContestQuestions)
            {
                Question q = _context.Questions.SingleOrDefault(q => q.Id == item.QuestionId);

                var answerExists = _context.ContestQuestionUsers
                                    .Include(cqu => cqu.ContestQuestion)
                                    .Where(cqu => cqu.ContestQuestion.ContestId == id)
                                    .Where(cqu => cqu.ContestQuestion.QuestionId == q.Id)
                                    .Any();
                
                if (answerExists)
                    warning = true;

                //hodnota true znamená, že otázka už má záznam o odpovedi -> teda nebude možné ju vymazať
                selectedQuestions.Add(new KeyValuePair<Question, bool>(q, answerExists));
            }
            
            //otazky ktore nie su este v sutazi -> vsetky - zvolene
            var notSelectedQuestions = _context.Questions.Where(q => !contest.ContestQuestions.Select(cq => cq.QuestionId).Any(cqID => q.Id == cqID)).ToList();

            var viewModel = new ContestViewModel()
            {
                ContestId = contest.Id,
                ContestName = contest.Name,
                Warning = warning,
                SelectedQuestions = selectedQuestions,
                SelectedQuestion = string.Empty,
                Questions = notSelectedQuestions
            };

            return View("ContestForm", viewModel);
        }


        public ActionResult Delete(int id)
        {
            var contest = _context.Contests.Include(c => c.ContestQuestions).SingleOrDefault(q => q.Id == id);

            if (contest == null)
                return NotFound();

            return View(contest);
        }

        [HttpPost]
        public ActionResult Delete(Contest contest)
        {
            var contestDb = _context.Contests.Include(c => c.ContestQuestions).SingleOrDefault(q => q.Id == contest.Id);

            var contestHasAnswers = _context.ContestQuestionUsers.Include(cqu => cqu.ContestQuestion).
                Where(cqu => cqu.ContestQuestion.ContestId == contestDb.Id).Any();

            if (contestHasAnswers)
            {
                //error handling
                return View("ErrorOnDelete", contestDb);
            }

            while (contestDb.ContestQuestions.Any())
            {
                ContestQuestion contestQuestion = contestDb.ContestQuestions[0];
                contestDb.ContestQuestions.RemoveAt(0);
                _context.ContestQuestions.Remove(contestQuestion);
            }
            _context.Contests.Remove(contestDb);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteUserAnswers(Contest contest, ContestViewModel viewModel)
        {
            var contestId = 0;
            if (contest.Id == 0)
                contestId = viewModel.ContestId;
            else
                contestId = contest.Id;
            
            var contextQuestionUsersDb = _context.ContestQuestionUsers
                                                .Include(cqu => cqu.ContestQuestion)
                                                .Where(cqu => cqu.ContestQuestion.ContestId == contestId)
                                                .ToList();

            _context.RemoveRange(contextQuestionUsersDb);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Activate(int id)
        {
            var contest = _context.Contests.SingleOrDefault(q => q.Id == id);

            if (contest == null)
                return NotFound();

            if (contest.IsActive)
                contest.IsActive = false;
            else
                contest.IsActive = true;
            
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult GenerateQRCodes(int id)
        {
            var questionIDs = _context.ContestQuestions.Include(q => q.Contest)
                                                        .Where(q => q.ContestId == id)
                                                        .OrderBy(q => q.QuestionNumber)
                                                        .Select(q => q.Id)
                                                        .ToList();

            var urls = new List<string>(questionIDs.Count);
            foreach(var questionID in questionIDs)
            {
                var actionUrl = Url.Action("QuestionForm", "Users", new { id = questionID });
                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{actionUrl}";
                urls.Add(url);
            }

            return RedirectToAction("ViewQRCodes", "QRCoder", new { urls, contestId = id });
        }

    }
}


