using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LHBooksWeb.Models.EF
{
    [Table("Author")]

    public class Author : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        public string? Bio { get; set; }

    }
}
