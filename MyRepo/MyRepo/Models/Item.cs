using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRepo.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "New item";
        public string? Description { get; set; }
        public virtual IdentityUser Owner {get; set ;}
        public bool isPublic { get; set; }
        public float Size { get; set; }
    }
}
