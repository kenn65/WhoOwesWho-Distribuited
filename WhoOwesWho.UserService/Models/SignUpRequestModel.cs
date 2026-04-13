using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.UserService.Models
{
    public class SignUpRequestModel
    {
        [Required]
        public UserModel? Entity { get; set; }

        [Required]
        public string? Host { get; set; }
    }
}
