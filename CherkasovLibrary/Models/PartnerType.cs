using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CherkasovLibrary.Models
{
    /// <summary>
    /// Типы партнеров (ООО, ЗАО, ИП и т.д.)
    /// Таблица: partner_types_cherkasov
    /// </summary>
    [Table("partner_types_cherkasov")]
    public class PartnerType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}