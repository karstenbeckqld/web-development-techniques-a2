using Microsoft.EntityFrameworkCore;
using MCBA.Models;

namespace MCBA.Data;

public class MCBAContext : DbContext
{
    public MCBAContext(DbContextOptions<MCBAContext> options) : base(options)
    {
    }


    public DbSet<Customer> Customer { get; set; }
    public DbSet<Account> Account { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
    public DbSet<BillPay> BillPay { get; set; }
    public DbSet<Login> Login { get; set; }
    public DbSet<Payee> Payee { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // prevents the following from being declared as string? -- prevents alot of type casting
        modelBuilder.Entity<Customer>().Property(m => m.TFN).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.Address).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.City).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.State).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.PostCode).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.Mobile).IsRequired(false);
        modelBuilder.Entity<Customer>().Property(m => m.ProfilePicture).IsRequired(false);


        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Login>().ToTable(l => l.HasCheckConstraint("CH_Login_LoginID", "len(LoginID) = 8"));
        modelBuilder.Entity<Login>()
            .ToTable(l => l.HasCheckConstraint("CH_Login_PasswordHash", "len(PasswordHash) = 94"));

        modelBuilder.Entity<Account>().ToTable(a => a.HasCheckConstraint("CH_Account_Balance", "Balance >= 0"));

        modelBuilder.Entity<Transaction>().HasOne(x => x.Account).WithMany(x => x.Transactions)
            .HasForeignKey(x => x.AccountNumber);

        modelBuilder.Entity<Transaction>().ToTable(t => t.HasCheckConstraint("CH_Transaction_Amount", "Amount > 0"));

        modelBuilder.Entity<BillPay>().ToTable(b => b.HasCheckConstraint("CH_BillPayAmount", "Amount > 0"));
    }
}