﻿using System;
using System.Linq;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Windsor.Extension.Scope
{
    public static class ContainerExtensions
    {
        private static readonly ProxyGenerator ProxyGenerator;
        private static readonly IInterceptor NopRegisterInterceptor;

        static ContainerExtensions()
        {
            ProxyGenerator = new ProxyGenerator();
            NopRegisterInterceptor = new NopInterceptor(invocation => invocation.Method.Name == "Register");
        }

        public static IWindsorContainer Is<TScope>(this IWindsorContainer extended, TScope scope)
            where TScope : class
        {
            extended.Register(
                Component
                    .For<TScope>()
                    .Instance(scope)
            );

            return extended;
        }

        public static IWindsorContainer If<TScope>(this IWindsorContainer extended, params TScope[] scopes)
            where TScope : class
        {
            var @dynamic = IsDynamic(extended.GetType());
            if (dynamic)
            {
                return extended;
            }

            var actual = extended.Resolve<TScope>();
            var inScope = scopes.Contains(actual);
            if (inScope)
            {
                return extended;
            }

            var decorated = ProxyGenerator.CreateInterfaceProxyWithTarget(extended, ProxyGenerationOptions.Default, NopRegisterInterceptor);
            return decorated;
        }

        private static bool IsDynamic(Type type)
        {
            try
            {
                var location = type.Assembly.Location;
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }
    }
}