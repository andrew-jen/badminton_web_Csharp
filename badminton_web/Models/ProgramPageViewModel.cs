using Microsoft.AspNetCore.Mvc;

namespace MemberSystemMVC.Models
{
    public class ProgramPageViewModel
    {
        public List<Program> Courses { get; set; }
        public List<ViewProgramList> Registrations { get; set; }
    }

}
