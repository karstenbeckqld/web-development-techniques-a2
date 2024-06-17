using MCBA.Interfaces;
using MCBA.Models;
using Microsoft.Identity.Client;
using System.Collections.Generic;

namespace MCBA.Factories
{
    public class CustomerFactory : ICustomerFactory
    {
        private readonly IAccountFactory _accountFactory;

        public CustomerFactory(IAccountFactory accountFactory)
        {
            _accountFactory = accountFactory;
        }

        public ICustomer CreateCustomer(int customerId, string name, string tfn, string address, string city, string state, string postCode, string mobile, byte[]? profilePicture, List<IAccounts> accounts, ILogin iLogin)
        {
            var customer = new Customer(customerId, name, tfn, address, city, state, postCode, mobile, profilePicture, accounts,  iLogin);
            customer.Accounts = accounts.ConvertAll(t => (Account)t);  
            customer.login = (Login)iLogin;
            return customer;
        }
    }
}