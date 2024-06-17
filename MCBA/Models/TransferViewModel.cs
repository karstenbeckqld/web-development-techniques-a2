using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public class TransferViewModel
{
    // Source account number
    public int? ID { get; set; }
    
    [Column(TypeName = "money"), DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    
    [StringLength(30)]
    public string Comment { get; set; }
    
    public char AccountType { get; set; }
    
    public Account Account { get; set; }
    
    public Account DestinationAccount { get; set; }
    
    public int? DestinationAccountId { get; set; }
    
    public string ControllerName { get; set; }

    public int CustomerID { get; set; }
    
    public override string ToString()
    {
        return "\nTransferViewData:"
               + "\nSourceAccountNumber: " + ID
               + "\nSourceAccount: " + Account
               + "\nDestinationAccountNumber: " + DestinationAccountId
               + "\nDestinationAccount: " + DestinationAccount
               + "\nAmount: " + Amount
               + "\nComment: " + Comment
               + "\nAccountType: " + AccountType
               + "\nControllerName: " + ControllerName;
    }
}