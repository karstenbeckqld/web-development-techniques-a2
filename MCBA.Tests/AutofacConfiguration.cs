using Autofac;
using MCBA.Factories;
using MCBA.Interfaces;
using MCBA.Models;
using System.Runtime.InteropServices;

namespace MCBA
{
    public static class AutofacConfiguration
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();


            // Register Factories and Interfaces
            builder.RegisterType<AccountFactory>().As<IAccountFactory>().SingleInstance();
            builder.RegisterType<CustomerFactory>().As<ICustomerFactory>().SingleInstance();
            builder.RegisterType<BillPayFactory>().As<IBillPayFactory>().SingleInstance();
            builder.RegisterType<PayeeFactory>().As<IPayeeFactory>().SingleInstance();
            builder.RegisterType<LoginFactory>().As<ILoginFactory>().SingleInstance();
            builder.RegisterType<TransactionFactory>().As<ITransactionFactory>().SingleInstance();

            return builder.Build();
        }
    }
}