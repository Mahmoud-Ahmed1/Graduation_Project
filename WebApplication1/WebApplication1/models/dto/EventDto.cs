using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.models.dto
{
    public class EventDto
    {
        public string typeEvent { get; set; }
        public string eventDate { get; set; }
        public string? taxt { get; set; }
        public string username { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string imgeurl { get; set; }

    }
}
