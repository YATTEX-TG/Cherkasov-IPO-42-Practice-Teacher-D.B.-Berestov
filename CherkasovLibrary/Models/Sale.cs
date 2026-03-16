using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CherkasovLibrary.Models
{
    /// <summary>
    /// Продажи продукции партнерам
    /// Таблица: sales_cherkasov
    /// </summary>
    [Table("sales_cherkasov")]
    public class Sale
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("partner_id")]
        public int PartnerId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Навигационные свойства
        public virtual Partner Partner { get; set; }
        public virtual Product Product { get; set; }
    }
}