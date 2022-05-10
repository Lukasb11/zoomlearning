using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LearningSystem.Controllers;
using System.Web.Mvc;
using System.Web.Security;

namespace LearningSystem.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestLogin()
        {
            var auth = new AuthController();
            var result = auth.Login() as ViewResult;
            Assert.AreEqual("Login", result.ViewName);
        }

        [TestMethod]
        public void TestZoomAuth()
        {
            var auth = new UserController();
            var result = auth.authZoom();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestZoomSuccess()
        {
            var auth = new UserController();
            var result = auth.Success() as ViewResult;
            Assert.AreEqual("Success", result.ViewName);
        }

        [TestMethod]
        public void TestRegister()
        {
            var auth = new AuthController();
            var result = auth.Register() as ViewResult;
            Assert.AreEqual("Register", result.ViewName);
        }

        [TestMethod]
        public void TestCreate()
        {
            var course = new CourseController();
            var result = course.Create() as ViewResult;
            Assert.AreEqual("Create", result.ViewName);
        }

        [TestMethod]
        public void TestEdit()
        {
            var course = new CourseController();
            var result = (RedirectToRouteResult)course.Edit(null);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void TestDetails()
        {
            var course = new CourseController();
            var result = (RedirectToRouteResult)course.Details(null);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

    }
}