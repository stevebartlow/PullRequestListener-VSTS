﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullRequestListener.EncodingChecker;
using FluentAssertions;


namespace PullRequestListener.EncodingChecker.Tests
{
    [TestClass]
    public class EncodingExtensionTest
    {
        [TestMethod]
        public void CompareUTF8File()
        {
            bool compare = Encoding.UTF8.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"));
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void CompareANSIFile()
        {
            bool compare = Encoding.ASCII.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"));
            compare.Should().BeTrue();
        }


        [TestMethod]
        public void FailUTF8File()
        {
            bool compare = Encoding.UTF32.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"));
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void Fail16LEBOMFile()
        {
            bool compare = Encoding.UTF8.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 LE BOM.txt"));
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void Fail16BEBOMFile()
        {
            bool compare = Encoding.UTF8.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"));
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailANSIFile()
        {
            bool compare = Encoding.UTF32.isOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"));
            compare.Should().BeFalse();
        }

        protected Stream GetResource(string resourceName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic) { continue; }
                string[] manifestResourceNames = assembly.GetManifestResourceNames();
                if (manifestResourceNames.Contains(resourceName))
                {
                    return assembly.GetManifestResourceStream(resourceName);
                }
            }
            return null;
        }
    }
}
