using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LHBooksWeb.Models.EF
{
	public class ContactModel
	{
		[Key]
		public string Name { get; set; }
		public string Map { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
		public string Description { get; set; }
		public string? LogoImg { get; set; }

		[NotMapped]
		public IFormFile? ImageUpload {  get; set; } 
	}
}
