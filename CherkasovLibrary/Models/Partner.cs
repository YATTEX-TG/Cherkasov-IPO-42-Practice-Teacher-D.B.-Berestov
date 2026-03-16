using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CherkasovLibrary.Models
{
    [Table("partners_cherkasov")]
    public class Partner
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("director_fullname")]
        [MaxLength(200)]
        public string DirectorFullname { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; }



        [Column("address")]
        public string Address { get; set; }



        [Column("rating")]
        public int? Rating { get; set; }

        [Column("discount")]
        public int Discount { get; set; } // Теперь это поле хранится в БД

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Навигационные свойства
        public virtual PartnerType PartnerType { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}