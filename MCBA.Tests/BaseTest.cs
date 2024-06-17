using System;
using Autofac;

namespace MCBA.Tests
{
    public abstract class BaseTest : IDisposable
    {
        private readonly IContainer _container;

        protected BaseTest()
        {
            _container = AutofacConfiguration.Configure();
        }

        protected T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public virtual void Dispose()
        {
            _container.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}