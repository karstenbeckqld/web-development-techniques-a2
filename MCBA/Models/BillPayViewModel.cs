using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MCBA.Models
{
    public class BillPayViewModel
    {
        public int BillPayID { get; set; }

        [Required(ErrorMessage = "Account number field must not be empty")]
        [ForeignKey("Account")]
        public int AccountNumber { get; set; }
        public virtual Account Account { get; set; }


        [Required(ErrorMessage = "PayeeID field must not be empty")]
        [ForeignKey("payee")]
        public int PayeeID { get; set; }
        public virtual Payee payee { get; set; }


        [Required(ErrorMessage = "Bill amount field must not be empty")]
        [Column(TypeName = "money"), DataType(DataType.Currency), Range(0.01, double.MaxValue, ErrorMessage = "Billpay amount must be greater than $0.01")]
        public decimal Amount { get; set; }


        [Required(ErrorMessage = "Date field must not be empty")]
        public DateTime ScheduleDate { get; set; }


        // regex must be 'O' or 'M' - can add here to make more options later??
        [Required(ErrorMessage = "Period must not be empty"), RegularExpression(@"^[O]{1}|[M]{1}|[L]{1}|[F]{1}"), Column(TypeName = "char")]

        public char Period { get; set; }

        [Required]
        public bool LockedPayment { get; set; }

        public int? CustomerID { get; set; }

    }
}
