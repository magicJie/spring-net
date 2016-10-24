#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using System;
using System.Reflection;

using NUnit.Framework;

#endregion

namespace Spring.Proxy
{
	/// <summary>
	/// Unit tests for the CompositionProxyTypeBuilder class.
	/// </summary>
	/// <author>Rick Evans</author>
    /// <author>Bruno Baia</author>
	[TestFixture]
	public class CompositionProxyTypeBuilderTests : AbstractProxyTypeBuilderTests
	{
		[Test]
		public void OnClassThatDoesntImplementAnyInterfaces()
		{
			IProxyTypeBuilder builder = GetProxyBuilder();
			builder.TargetType = typeof (DoesntImplementAnyInterfaces);
            Assert.Throws<ArgumentException>(() => builder.BuildProxyType(), "Composition proxy target must implement at least one interface.");
		}

		[Test]
		public void BuildProxyWithNothingSet()
		{
            IProxyTypeBuilder builder = GetProxyBuilder();
            Assert.Throws<NullReferenceException>(() => builder.BuildProxyType());
		}

		[Test]
		public void ProxyInnerClass()
		{
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(InnerClass);
            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is InnerInterface);
            Assert.IsFalse(foo is InnerClass);
        }

		[Test]
		public void CheckInterfaceImplementation()
		{
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof (MarkerClass);
            Type proxy = builder.BuildProxyType();
			Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
			object foo = Activator.CreateInstance(proxy);
			Assert.IsTrue(foo is IMarkerInterface);
			Assert.IsFalse(foo is MarkerClass);
		}

        [Test]
        public void SetsInterfacesToProxy()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(MultipleInterfaces);
            builder.Interfaces = new Type[] { typeof(IBase) };

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is IBase);
            Assert.IsFalse(foo is IInherited);

            // try to call proxied interface methods
            ((IBase)foo).Base();
        }

        [Test]
        public void DoesNotProxyNonProxiableInterface()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(ApplicationClass);

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsTrue(foo is IApplicationInterface);
            Assert.IsFalse(foo is IFrameworkInterface);

            // try to call proxied interface methods
            ((IApplicationInterface)foo).ApplicationMethod();
        }

        [Test]
        public void ForcesNonProxiableInterfacesToBeProxied()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(ApplicationClass);
            builder.Interfaces = new Type[] { typeof(IFrameworkInterface) };

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");

            object foo = Activator.CreateInstance(proxy);
            Assert.IsFalse(foo is IApplicationInterface);
            Assert.IsTrue(foo is IFrameworkInterface);

            // try to call proxied interface methods
            ((IFrameworkInterface)foo).FrameworkMethod();
        }


        [Test] // SPRNET-1424
        public void DoesNotProxyInterfaceMethodAttributes()
        {
            IProxyTypeBuilder builder = GetProxyBuilder();
            builder.TargetType = typeof(object);
            builder.Interfaces = new Type[] { typeof(IAnotherMarkerInterface) };

            Type proxy = builder.BuildProxyType();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to BuildProxy() was null.");
            MethodInfo method = proxy.GetMethod("Spring.Proxy.IAnotherMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);
            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have 0 attribute applied to the target method.");
            Assert.AreEqual(0, attrs.Length, "Should have 0 attribute applied to the target method.");
        }

		protected override IProxyTypeBuilder GetProxyBuilder()
		{
			return new CompositionProxyTypeBuilder();
        }
    }
}