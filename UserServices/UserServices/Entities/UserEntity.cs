using System;
using System.ComponentModel.DataAnnotations;
using UserServicesDotNetCore.Helpers;

namespace UserServicesDotNetCore.Entities {
    public class UserEntity {

        [Key]
        public Guid Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; }
        [MaxLength(14)]
        public string CpfCnpj { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }
        public string PictureUrl { get; set; }
        [Required]
        public Role Role { get; set; }
    }
}
