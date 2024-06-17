using MCBA.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MCBA.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        //Seed Data example referenced from Web Dev Tutorials
        using var context = new MCBAContext(serviceProvider.GetRequiredService<DbContextOptions<MCBAContext>>());

        // Look for customers.
        if (context.Customer.Any())
            return; // DB has already been seeded.


        using var client = new HttpClient();

        // Now we obtain the JSON string by calling the GetStringAsync() method on the client and providing the url of
        // the web service.
        var json = client.GetStringAsync("https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/")
            .Result;


        List<Customer> jsonData = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
        {
            DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
        });

        foreach (var customer in jsonData)
        {
            customer.ProfilePicture = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Models/test.jpg");
            context.Customer.Add(customer);
            context.Login.Add(customer.login);

            foreach (var account in customer.Accounts)
            {
                account.Balance = 0;
                account.Balance += account.Transactions.Sum(transaction => transaction.Amount);

                context.Add(account);

                foreach (var transaction in account.Transactions)
                {
                    transaction.TransactionType = 'D';
                    transaction.AccountNumber = account.AccountNumber;
                    transaction.TransactionTimeUTC = TimeZoneInfo.ConvertTimeToUtc(transaction.TransactionTimeUTC);

                    context.Transaction.Add(transaction);
                }
            }
        }

        context.Payee.AddRange(
            new Payee
            {
                Name = "PowerBill",
                Address = "1 Power Street",
                City = "Brisbane",
                State = "QLD",
                PostCode = "1111",
                Phone = "(04)11 222 333"
            },
            new Payee
            {
                Name = "WaterBill",
                Address = "2 Water Street",
                City = "Brisbane",
                State = "QLD",
                PostCode = "1111",
                Phone = "(04)11 333 111"
            });
        context.SaveChanges();
    }
}