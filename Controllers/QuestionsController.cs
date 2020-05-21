using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Data;
using QuizWebApp.Models;
using QuizWebApp.ViewModels;

namespace QuizWebApp.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class QuestionsController : BaseController
    {
        public QuestionsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment) : base(context, hostEnvironment)
        {
        }

        public ViewResult Index()
        {
            var questionPaginationViewModel = new PaginationViewModel<Question>();

            questionPaginationViewModel.Entities = _context.Questions.Take(questionPaginationViewModel.PageLength).ToList();
            questionPaginationViewModel.PageCount = (int)Math.Ceiling((double)_context.Questions.Count() / questionPaginationViewModel.PageLength);

            return View(questionPaginationViewModel);
        }

        public ActionResult GetPartialViewData(string searchString, int pageLength, int pageNumber)
        {
            IQueryable<Question> query = _context.Questions;

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(q => q.Name.Contains(searchString));

            var questionPaginationViewModel = GetViewModelData(searchString, pageLength, pageNumber, query);

            return PartialView("_QuestionsPartial", questionPaginationViewModel);
        }

        public ActionResult Create()
        {
            Question question = new Question();
            question.Answers = new List<Answer>()
            {
                new Answer(),
                new Answer()
            };

            ClearTemps();

            return View(question);
        }

        public IActionResult CheckQuestionName(string questionName, int questionID)
        {
            bool result = _context.Questions.Any(q => q.Name == questionName && q.Id != questionID);
            return new JsonResult(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Question question)
        {
            int radioIndex = Int32.Parse(Request.Form["IsCorrect"]);
            question.Answers[radioIndex].IsCorrect = true;

            MoveFiles(question, false);

            _context.Questions.Add(question);
            _context.SaveChanges();

            return RedirectToAction("Index", "Questions");
        }

        [HttpPost]
        public ActionResult AddAnswer(Question question, bool editing)
        {
            question.Answers.Add(new Answer());

            if (editing)
            {
                return View("Edit", question);
            }
            else
            {
                return View("Create", question);
            }            
        }

        [HttpPost]
        public ActionResult RemoveAnswer(Question question, bool editing)
        {
            int last = question.Answers.Count - 1;

            if (question.Answers[last].IsCorrect)
            {
                question.Answers[last - 1].IsCorrect = true;
            }

            TryToDeleteImage(question.Answers[last].Image, true);
            question.Answers.RemoveAt(last);
            
            if (editing)
            {
                return View("Edit", question);
            }
            else
            {
                return View("Create", question);
            }
        }

        public ActionResult Details(int id)
        {
            var question = _context.Questions.Include(q => q.Answers).SingleOrDefault(q => q.Id == id);

            if (question == null)
                return NotFound();

            return View("Details", question);
        }

        public ActionResult Edit(int id, bool forceedit)
        {
            var question = _context.Questions.Include(q => q.Answers).SingleOrDefault(q => q.Id == id);
            
            if (question == null)
                return NotFound();

            bool userAnswersExist = _context.ContestQuestionUsers.Include(cqu => cqu.ContestQuestion).Any(cqu => cqu.ContestQuestion.QuestionId == question.Id);

            if (userAnswersExist && !forceedit)
                return View("ErrorOnEdit", question);

            ClearTemps();
            //prekopiruj vsetky obrazky do tempu
            question = MoveFiles(question, true);

            return View(question);
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSave(Question question)
        {
            var questionDb = _context.Questions.Include(q => q.Answers).Single(q => q.Id == question.Id);

            if (questionDb.Answers.Count > question.Answers.Count)
            {
                //delete questions
                while (questionDb.Answers.Count > question.Answers.Count)
                {
                    Answer answer = questionDb.Answers[questionDb.Answers.Count - 1];
                    TryToDeleteImage(answer.Image, false);

                    questionDb.Answers.RemoveAt(questionDb.Answers.Count - 1);
                    _context.Answers.Remove(answer);
                }                    
            }
            else
            {
                //add questions
                while (questionDb.Answers.Count < question.Answers.Count)
                {
                    questionDb.Answers.Add(new Answer());
                }
            }
            /*
             * Musia sa uložiť zmeny v DB pred tým ako sa začnú updatovať stĺpce.
             * Inak posledný upravený záznam sa nahrá ako posledný, čo môže spôsobiť
             * uloženie v zlom poradí. Napríklad, že sa vymenia jednotlivé odpovede. 
             * (z C sa stane B a z B sa stane C
             *      -> nastane v prípade ak B je ozačené ako správna odpoveď)
            */
            _context.SaveChanges();

            //ak existuje, vymaz ho a riad sa tym co je v temp (teda v question.Image)

            questionDb.Name = question.Name;
            questionDb.Text = question.Text;

            if (questionDb.Image != null)
                TryToDeleteImage(questionDb.Image, false);

            questionDb.Image = question.Image;

            for (int i = 0; i < questionDb.Answers.Count; i++)
            {
                questionDb.Answers[i].Text = question.Answers[i].Text;
                questionDb.Answers[i].IsCorrect = false;

                if (questionDb.Answers[i].Image != null)
                    TryToDeleteImage(questionDb.Answers[i].Image, false);    

                questionDb.Answers[i].Image = question.Answers[i].Image;
            }

            MoveFiles(questionDb, false);

            int radioIndex = Int32.Parse(Request.Form["IsCorrect"]);
            questionDb.Answers[radioIndex].IsCorrect = true;
            
            _context.SaveChanges();

            return RedirectToAction("Index", "Questions");
        }

        [HttpPost]
        public ActionResult DeleteUserAnswers(Question question)
        {
            var questionId = question.Id;

            var contextQuestionUsersDb = _context.ContestQuestionUsers
                                                .Include(cqu => cqu.ContestQuestion)
                                                .Where(cqu => cqu.ContestQuestion.QuestionId == questionId)
                                                .ToList();

            _context.RemoveRange(contextQuestionUsersDb);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var question = _context.Questions.Include(q => q.Answers).SingleOrDefault(q => q.Id == id);

            if (question == null)
                return NotFound();

            return View(question);
        }

        [HttpPost]
        public ActionResult Delete(Question question)
        {
            var questionDb = _context.Questions.Include(q => q.Answers).Single(q => q.Id == question.Id);
            
            var contestQuestionDb = _context.ContestQuestions.Include(cq => cq.Contest).Where(cq => cq.QuestionId == question.Id).Select(cq => cq.Contest.Name).ToList();

            if (contestQuestionDb.Count != 0)
            {
                //error handling
                var errorViewModel = new QuestionDeleteViewModel() 
                {
                    QuestionName = questionDb.Name,
                    QuestionText = questionDb.Text,
                    ContestNames = contestQuestionDb
                };
                return View("ErrorOnDelete", errorViewModel);
            }

            TryToDeleteImage(questionDb.Image, false);

            while (questionDb.Answers.Any())
            {
                Answer answer = questionDb.Answers[0];
                TryToDeleteImage(answer.Image, false);
                questionDb.Answers.RemoveAt(0);
                _context.Answers.Remove(answer);
            }
            _context.Questions.Remove(questionDb);
            _context.SaveChanges();

            return RedirectToAction("Index", "Questions");
        }

        [HttpPost]
        public ActionResult RemoveImage(Question question, string img, bool editing)
        {
            if (question.Image == img)
            {
                TryToDeleteImage(question.Image, true);
                question.Image = null;
            }
            foreach (Answer answer in question.Answers)
            {
                if (answer.Image == img)
                {
                    TryToDeleteImage(answer.Image, true);
                    answer.Image = null;
                    break;
                }
            }
            ModelState.Clear();

            if (editing)
            {
                return View("Edit", question);
            }
            else
            {
                return View("Create", question);
            }
        }

        [HttpPost]
        public ActionResult UploadImage(Question question, bool editing)
        {
            string webRootPath = _hostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            foreach (var file in files)
            {
                var uploads = Path.Combine(webRootPath, @"uploads/temps");

                var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);

                using (var filestream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                {
                    file.CopyTo(filestream);

                    if (file.Name == "qImage")
                    {
                        TryToDeleteImage(question.Image, true);
                        question.Image = fileName;
                    }
                    else
                    {
                        for (int i = 0; i < question.Answers.Count; i++)
                        {
                            if (file.Name == i.ToString())
                            {
                                TryToDeleteImage(question.Answers[i].Image, true);
                                question.Answers[i].Image = fileName;
                            }
                        }
                    }
                }
            }

            ModelState.Clear();

            if (editing)
            {
                return View("Edit", question);
            } 
            else
            {
                return View("Create", question);
            }
        }

        private Question MoveFiles(Question question, bool onlyCopy)
        {
            var sourcePath = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/temps");
            var targetPath = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/images");

            if (onlyCopy)
            {
                //kopiruje sa z images do temp
                var temp = sourcePath;
                sourcePath = targetPath;
                targetPath = temp;
            }

            if (question.Image != null)
            {
                var sourceImgPath = Path.Combine(sourcePath, question.Image);
                var targetImgPath = Path.Combine(targetPath, question.Image);

                if (System.IO.File.Exists(sourceImgPath))
                {
                    if (onlyCopy)
                        System.IO.File.Copy(sourceImgPath, targetImgPath, true);
                    else
                        System.IO.File.Move(sourceImgPath, targetImgPath);
                }
            }

            foreach (var answer in question.Answers)
            {
                if (answer.Image != null)
                {
                    var sourceImgPath = Path.Combine(sourcePath, answer.Image);
                    var targetImgPath = Path.Combine(targetPath, answer.Image);

                    if (System.IO.File.Exists(sourceImgPath))
                    {
                        if (onlyCopy)
                            System.IO.File.Copy(sourceImgPath, targetImgPath, true);
                        else
                            System.IO.File.Move(sourceImgPath, targetImgPath);
                    }
                }                
            }
            return question;
        }

        private void TryToDeleteImage(string imgName, bool isTemp)
        {
            if (imgName == null)
                return;

            var path = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/images", imgName);

            if (isTemp)
                path = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/temps", imgName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private void ClearTemps()
        {
            var tempsPath = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/temps");

            DirectoryInfo di = new DirectoryInfo(tempsPath);

            foreach (FileInfo file in di.GetFiles())
                file.Delete();
        }
    }
}