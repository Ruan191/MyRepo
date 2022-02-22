using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRepo.Models
{
    public class UserItem
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("HasAccess")]
        public virtual IdentityUser User { get; set; }
        [ForeignKey("Items")]
        public virtual Item Item { get; set; }
    }
}
