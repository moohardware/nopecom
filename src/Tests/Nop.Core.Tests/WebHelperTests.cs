﻿using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Tests;
using NUnit.Framework;
using Rhino.Mocks;

namespace Nop.Core.Tests
{
    [TestFixture]
    public class WebHelperTests
    {
        private DefaultHttpContext _httpContext;
        private INopFileProvider _fileProvider;
        private IWebHelper _webHelper;

        [SetUp]
        public void SetUp()
        {
            _httpContext = new DefaultHttpContext();
            var queryString = new QueryString("");
            queryString = queryString.Add("Key1", "Value1");
            queryString = queryString.Add("Key2", "Value2");
            _httpContext.Request.QueryString = queryString;
            _httpContext.Request.Headers.Add(HeaderNames.Host, "www.Example.com");

            _fileProvider = MockRepository.GenerateMock<INopFileProvider>();

            _webHelper = new WebHelper(new HostingConfig(), new FakeHttpContextAccessor(_httpContext), _fileProvider);
        }

        [Test]
        public void Can_get_storeHost_without_ssl()
        {
            _webHelper.GetStoreHost(false).ShouldEqual("http://www.Example.com/");
        }

        [Test]
        public void Can_get_storeHost_with_ssl()
        {
            _webHelper.GetStoreHost(true).ShouldEqual("https://www.Example.com/");
        }

        [Test]
        public void Can_get_storeLocation_without_ssl()
        {
            _webHelper.GetStoreLocation(false).ShouldEqual("http://www.Example.com/");
        }

        [Test]
        public void Can_get_storeLocation_with_ssl()
        {
            _webHelper.GetStoreLocation(true).ShouldEqual("https://www.Example.com/");
        }

        [Test]
        public void Can_get_storeLocation_in_virtual_directory()
        {
            _httpContext.Request.PathBase = "/nopCommercepath";
            _webHelper.GetStoreLocation(false).ShouldEqual("http://www.Example.com/nopCommercepath/");
        }

        [Test]
        public void Can_get_queryString()
        {
            _webHelper.QueryString<string>("Key1").ShouldEqual("Value1");
            _webHelper.QueryString<string>("Key2").ShouldEqual("Value2");
            _webHelper.QueryString<string>("Key3").ShouldEqual(null);
        }

        [Test]
        public void Can_remove_queryString()
        {
            //empty URL
            _webHelper.RemoveQueryString(null, null).ShouldEqual(string.Empty);
            //empty key
            _webHelper.RemoveQueryString("http://www.example.com/", null).ShouldEqual("http://www.example.com/");
            //non-existing param with fragment
            _webHelper.RemoveQueryString("http://www.example.com/#fragment", "param").ShouldEqual("http://www.example.com/#fragment");
            //first param (?)
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value1", "param1")
                .ShouldEqual("http://www.example.com/?param2=value1");
            //second param (&)
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value1", "param2")
                .ShouldEqual("http://www.example.com/?param1=value1");
            //non-existing param
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value1", "param3")
                .ShouldEqual("http://www.example.com/?param1=value1&param2=value1");
            //with fragment
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value1#fragment", "param1")
                .ShouldEqual("http://www.example.com/?param2=value1#fragment");
            //specific value
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param1=value2&param2=value1", "param1", "value1")
                .ShouldEqual("http://www.example.com/?param1=value2&param2=value1");
            //all values
            _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param1=value2&param2=value1", "param1")
                .ShouldEqual("http://www.example.com/?param2=value1");
        }

        [Test]
        public void Can_modify_queryString()
        {
            //empty URL
            _webHelper.ModifyQueryString(null, null).ShouldEqual(string.Empty);
            //empty key
            _webHelper.ModifyQueryString("http://www.example.com/", null).ShouldEqual("http://www.example.com/");
            //empty value
            _webHelper.ModifyQueryString("http://www.example.com/", "param").ShouldEqual("http://www.example.com/?param=");
            //first param (?)
            _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value1", "Param1", "value2")
                .ShouldEqual("http://www.example.com/?param1=value2&param2=value1");
            //second param (&)
            _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value1", "param2", "value2")
                .ShouldEqual("http://www.example.com/?param1=value1&param2=value2");
            //non-existing param
            _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value1", "param3", "value1")
                .ShouldEqual("http://www.example.com/?param1=value1&param2=value1&param3=value1");
            //multiple values
            _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value1", "param1", "value1", "value2", "value3")
                .ShouldEqual("http://www.example.com/?param1=value1,value2,value3&param2=value1");
            //with fragment
            _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value1#fragment", "param1", "value2")
                .ShouldEqual("http://www.example.com/?param1=value2&param2=value1#fragment");
        }
    }

    public class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public FakeHttpContextAccessor(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext HttpContext { get; set; }
    }
}