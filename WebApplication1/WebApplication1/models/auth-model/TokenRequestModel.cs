using System.ComponentModel.DataAnnotations;

namespace WebApplication1.models.auth_model
{
    public class TokenRequestModel
    {
        [Required]
        public string Email { get; set; }/*58e77976-ba72-4987-ad41-1df948bc383d*/  /*aminnnnnA@1*/

        [Required]
        public string Password { get; set; } /* */
    }
}
