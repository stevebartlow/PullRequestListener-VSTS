using System;
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

        #region Passing Tests
        [TestMethod]
        public void PassUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"), false);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassUTF8BOMFile()
        {
            bool compare = Encoding.GetEncoding("utf-8").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8 BOM.txt"), true);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassANSIFile()
        {
            bool compare = Encoding.ASCII.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"), false);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassBigEndianUnicodeFile()
        {
            bool compare = Encoding.GetEncoding("unicodeFFFE").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"), true);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassLittleEndianUnicodeFile()
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 LE BOM.txt"), true);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassUTF32BEBOMUnicodeFile()
        {
            bool compare = Encoding.GetEncoding("utf-32BE").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 BE BOM.txt"), true);
            compare.Should().BeTrue();
        }
        [TestMethod]
        [Ignore("This doesn't work yet")]
        public void PassUTF32UnicodeFile()
        {
            bool compare = Encoding.GetEncoding("utf-32").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 with Unicode.txt"), false);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassUTF16BEAsUTF16LEFile() //UTF-16 LE and BE are effectively the same, and will be recognized as such
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"), true);
            compare.Should().BeTrue();
        }
        [TestMethod]
        public void PassANSIAsUTF8File() //UTF-8 without BOM and ANSI are effective the same, and will be recognized as such
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"), false);
            compare.Should().BeTrue();
        }

        #endregion


        #region UTF-32 Comparison
        [TestMethod]
        public void FailUTF8AsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8BOMAsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8 BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8BOMUnicodeAsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8 BOM with Unicode.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailANSIAsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16BEAsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16LEAsUTF32File()
        {
            bool compare = Encoding.UTF32.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 LE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        #endregion


        #region UTF8 Comparisons
        [TestMethod]
        public void FailUTF32LEAsUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 LE BOM.txt"), false);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF32BEAsUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 BE BOM.txt"), false);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16LEAsUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 LE BOM.txt"), false);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16BEAsUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"), false);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8BOMAsUTF8File()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8 BOM.txt"), false);
            compare.Should().BeFalse();
        }

        #endregion


        #region UTF8BOM Comparisons
        [TestMethod]
        public void FailUTF32LEAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 LE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF32BEAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 BE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16LEAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 LE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF16BEAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-16 BE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8BOMAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailANSIAsUTF8BOMFile()
        {
            bool compare = Encoding.UTF8.IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"), true);
            compare.Should().BeFalse();
        }
        #endregion


        #region UTF16LE Comparisons
        [TestMethod]
        public void FailUTF32LEAsUTF16LEFile()
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-32 LE BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8BOMAsUTF16LEFile()
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8 BOM.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailUTF8AsUTF16LEFile()
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.UTF-8.txt"), true);
            compare.Should().BeFalse();
        }
        [TestMethod]
        public void FailANSIAsUTF16LEFile()
        {
            bool compare = Encoding.GetEncoding("utf-16").IsOfEncoding(GetResource("PullRequestListener.EncodingChecker.Tests.TestFiles.ANSI.txt"), true);
            compare.Should().BeFalse();
        }

        #endregion

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
