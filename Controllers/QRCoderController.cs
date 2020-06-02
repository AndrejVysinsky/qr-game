using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuizWebApp.Data;
using QuizWebApp.ViewModels;

namespace QuizWebApp.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class QRCoderController : BaseController
    {
        public QRCoderController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment) : base(context, hostEnvironment)
        {
        }

        public IActionResult ViewQRCodes(int contestId)
        {
            var questionIDs = _context.ContestQuestions.Include(q => q.Contest)
                                                        .Where(q => q.ContestId == contestId)
                                                        .OrderBy(q => q.QuestionNumber)
                                                        .Select(q => q.Id)
                                                        .ToList();

            var urls = new List<string>(questionIDs.Count);
            foreach (var questionID in questionIDs)
            {
                var actionUrl = Url.Action("QuestionForm", "Users", new { id = questionID });
                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{actionUrl}";
                urls.Add(url);
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            List<Byte[]> qrByteData = new List<Byte[]>();

            for (int i = 0; i < urls.Count; i++)
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(urls[i], QRCodeGenerator.ECCLevel.M);

                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                qrByteData.Add(BitmapToBytes(qrCodeImage));
            }

            var contest = _context.Contests.SingleOrDefault(c => c.Id == contestId);
            var qrOutputViewModel = new QROutputViewModel()
            {
                ContestName = contest.Name,
                QrByteData = qrByteData
            };

            return View(qrOutputViewModel);
        }
        
        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}