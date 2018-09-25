using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullRequestListener.web;
using PullRequestListener.web.Controllers;

namespace PullRequestListener.web.Tests.Controllers
{
    [TestClass]
    public class EncodingCheckControllerTest
    {
        [TestMethod]
        public void Post()
        {
            // Arrange
            EncodingCheckController controller = new EncodingCheckController();

            // Act
            controller.Post();

            // Assert
        }
    }
}
