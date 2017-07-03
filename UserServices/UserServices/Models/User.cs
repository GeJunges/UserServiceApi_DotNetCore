using System.ComponentModel.DataAnnotations;
using UserServicesDotNetCore.Helpers;

namespace UserServices.Models {
    public class User {

        [Required]
        [MinLength(3, ErrorMessage = "FirstName must be minimum 3 characters")]
        [MaxLength(100, ErrorMessage = "FirstName must be maximum 100 characters")]
        public string FirstName { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "LastName must be minimum 3 characters")]
        [MaxLength(100, ErrorMessage = "LastName must be maximum 100 characters")]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Email must be a maximum 100 characters")]
        [EmailAddress]
        public string Email { get; set; }
        [MinLength(6, ErrorMessage = "Password must be minimum 6 characters")]
        [MaxLength(100, ErrorMessage = "Password must be maximum 100 characters")]
        public string Password { get; set; }
        [MinLength(11, ErrorMessage = "CpfCnpj must be minimum 11 characters")]
        [MaxLength(14, ErrorMessage = "CpfCnpj must be maximum 14 characters")]
        public string CpfCnpj { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }
        public string PictureUrl { get; set; }
        [Required]
        public Role Role { get; set; }
    }
}
