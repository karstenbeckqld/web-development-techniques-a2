
namespace MCBA.Interfaces;
public interface IPayee
{
    int PayeeID { get; }
    string Name { get; }
    string Address { get; }
    string City { get; }
    string State { get; }
    string PostCode { get; }
    string Phone { get; }
}
