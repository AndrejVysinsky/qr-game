using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class QROutputViewModel
    {
        public string ContestName { get; set; }
        public List<Byte[]> qrByteData { get; set; }
    }
}
