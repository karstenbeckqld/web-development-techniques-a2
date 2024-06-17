using MCBA.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models
{
    public class BillPay : IBillPay
    {
        [Required]
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

        [NotMapped]
        public int CustomerID { get; set; }

        public BillPay()
        {

        }

        public BillPay(int billPayID, int accountNumber, int payeeID, decimal amount, DateTime scheduleDate, char period, bool lockedPayment)
        {

            if (billPayID <= 0)
            {
                throw new ArgumentException("BillpayID must not be null!", nameof(billPayID));
            }
            if(accountNumber <=0 )
            {
                throw new ArgumentException("Invalid AccountNumber must be greater than 0", nameof(accountNumber));
            }
            if(payeeID <=0 )
            {
                throw new ArgumentException("Invalid PayeeID, must be greater than 0", nameof(payeeID));
            }
            if(amount <= 0)
            {
                throw new ArgumentException("Invalid amount, must be greater than 0", nameof(amount));
            }
            if(period != 'O' && period != 'M' && period != 'L' && period != 'F')
            {
                throw new ArgumentException("Invalid Period, Must be 'O', 'M' , 'L' or 'F'", nameof(period));
            }
           
          
            BillPayID = billPayID;
            AccountNumber = accountNumber;
            PayeeID = payeeID;
            Amount = amount;
            ScheduleDate = scheduleDate;
            Period = period;
            LockedPayment = lockedPayment;

        }
      

    }

    
}
