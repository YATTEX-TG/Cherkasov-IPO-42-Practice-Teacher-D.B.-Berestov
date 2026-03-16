using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CherkasovLibrary.Models
{
    /// <summary>
    /// Продукция компании
    /// Таблица: products_cherkasov
    /// </summary>
    [Table("products_cherkasov")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("article")]
        [MaxLength(50)]
        public string Article { get; set; }

        [Column("price")]
        public decimal Price { get; set; }
    }
}