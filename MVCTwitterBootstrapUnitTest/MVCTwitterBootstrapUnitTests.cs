using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.Web.ModelBinding;

namespace MVCTwitterBootstrapUnitTests.Tests
{
    [TestClass]
    public class MVCTwitterBootstrapTest
    {

        [TestMethod]
        public void visiblePropertiesIncludesPopertiesMarkedWithKeyAttribute()
        {
            var Model = new List<HomeInputModel>{
                           new HomeInputModel
                               {
                                   HomeId = 1,
                                   Blog = "http://foobar.com",
                                   Name = "asdf",
                                   Password = "asdfsd",
                                   StartDate = DateTime.Now.AddYears(1)
                               },
                           new HomeInputModel
                               {
                                   HomeId = 2,
                                   Blog = "http://foobar.com",
                                   Name = "fffff",
                                   Password = "dddddddasdfsd",
                                   StartDate = DateTime.Now.AddYears(2)
                               },
                       };

            //Visible properties are all properties marked as not hidden or identity properties
            Assert.AreNotEqual(Model.VisibleProperties().Count(), GetAllProperties(Model).Length);
        }

        [TestMethod]
        public void findIdentityPropertyMarkedWithKeyAttribute()
        {
            var Model = new List<HomeInputModel>{
                           new HomeInputModel
                               {
                                   HomeId = 1,
                                   Blog = "http://foobar.com",
                                   Name = "asdf",
                                   Password = "asdfsd",
                                   StartDate = DateTime.Now.AddYears(1)
                               },
                           new HomeInputModel
                               {
                                   HomeId = 2,
                                   Blog = "http://foobar.com",
                                   Name = "fffff",
                                   Password = "dddddddasdfsd",
                                   StartDate = DateTime.Now.AddYears(2)
                               },
                       };

            //Variant of code that finds identity
            Assert.AreEqual(IdentifierPropertyName(Model.FirstOrDefault()), "HomeId");

            //Fails finding indentity
            Assert.AreEqual(Model.FirstOrDefault().IdentifierPropertyName(), "HomeId");
        }

        private Array GetAllProperties(IEnumerable model)
        {
            var elementType = model.GetType().GetElementType();
            if (elementType == null)
            {
                elementType = model.GetType().GetGenericArguments()[0];
            }
            return elementType.GetProperties().ToArray();
        }


        public string IdentifierPropertyName(Object model)
        {
            return IdentifierPropertyName(model.GetType());
        }

        /// <summary>
        /// Possible solution, but then what about multiple Identity keys?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string IdentifierPropertyName(Type type)
        {
            if (type.GetProperties().Any(info => info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true) != null))
            {
                return
                    type.GetProperties().First(info => info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true) != null).Name;
            }
            else if (type.GetProperties().Any(p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)))
            {
                return
                    type.GetProperties().First(
                        p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)).Name;
            }
            return "";
        }

    }

    public class HomeInputModel
    {
        // Does not work
        [Key]
        public int HomeId { get; set; }

        // WORKS
        //public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Url)]
        public string Blog { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public static class DefaultScaffoldingExtensions
    {
        public static string GetControllerName(this Type controllerType)
        {
            return controllerType.Name.Replace("Controller", String.Empty);
        }

        public static string GetActionName(this LambdaExpression actionExpression)
        {
            return ((MethodCallExpression)actionExpression.Body).Method.Name;
        }

        public static PropertyInfo[] VisibleProperties(this IEnumerable Model)
        {
            var elementType = Model.GetType().GetElementType();
            if (elementType == null)
            {
                elementType = Model.GetType().GetGenericArguments()[0];
            }
            return elementType.GetProperties().Where(info => info.Name != elementType.IdentifierPropertyName()).ToArray();
        }

        public static PropertyInfo[] VisibleProperties(this Object model)
        {
            return model.GetType().GetProperties().Where(info => info.Name != model.IdentifierPropertyName()).ToArray();
        }

        public static RouteValueDictionary GetIdValue(this object model)
        {
            var v = new RouteValueDictionary();
            v.Add(model.IdentifierPropertyName(), model.GetId());
            return v;
        }

        public static object GetId(this object model)
        {
            return model.GetType().GetProperty(model.IdentifierPropertyName()).GetValue(model, new object[0]);
        }


        public static string IdentifierPropertyName(this Object model)
        {
            return IdentifierPropertyName(model.GetType());
        }

        //public static string IdentifierPropertyName(this Type type)
        //{
        //    //if (type.GetProperties().Any(info => info.PropertyType.AttributeExists<System.ComponentModel.DataAnnotations.KeyAttribute>()))
        //    if (type.GetProperties().Any(info => info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true) != null))
        //    {
        //        return
        //            type.GetProperties().First(info => info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true) != null).Name;

        //        //type.GetProperties().First(
        //        //    info => info.PropertyType.AttributeExists<System.ComponentModel.DataAnnotations.KeyAttribute>())
        //        //    .Name;
        //    }
        //    else if (type.GetProperties().Any(p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)))
        //    {
        //        return
        //            type.GetProperties().First(
        //                p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)).Name;
        //    }
        //    return "";
        //}

        public static string IdentifierPropertyName(this Type type)
        {
            if (type.GetProperties().Any(info => info.PropertyType.AttributeExists<System.ComponentModel.DataAnnotations.KeyAttribute>()))
            {
                return
                    type.GetProperties().First(
                        info => info.PropertyType.AttributeExists<System.ComponentModel.DataAnnotations.KeyAttribute>())
                        .Name;
            }
            else if (type.GetProperties().Any(p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)))
            {
                return
                    type.GetProperties().First(
                        p => p.Name.Equals("id", StringComparison.CurrentCultureIgnoreCase)).Name;
            }
            return "";
        }

        public static string GetLabel(this PropertyInfo propertyInfo)
        {
            var meta = ModelMetadataProviders.Current.GetMetadataForProperty(null, propertyInfo.DeclaringType, propertyInfo.Name);
            return meta.GetDisplayName();
        }

        public static string ToSeparatedWords(this string value)
        {
            return Regex.Replace(value, "([A-Z][a-z])", " $1").Trim();
        }

    }

    public static class PropertyInfoExtensions
    {
        public static bool AttributeExists<T>(this PropertyInfo propertyInfo) where T : class
        {
            var attribute = propertyInfo.GetCustomAttributes(typeof(T), false)
                                .FirstOrDefault() as T;
            if (attribute == null)
            {
                return false;
            }
            return true;
        }

        public static bool AttributeExists<T>(this Type type) where T : class
        {
            var attribute = type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            if (attribute == null)
            {
                return false;
            }
            return true;
        }

        public static T GetAttribute<T>(this Type type) where T : class
        {
            return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        public static T GetAttribute<T>(this PropertyInfo propertyInfo) where T : class
        {
            return propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        public static string GetLabel(this Object Model)
        {
            return Model.GetType().Name.ToSeparatedWords();
        }

        public static string GetLabel(this IEnumerable Model)
        {
            var elementType = Model.GetType().GetElementType();
            if (elementType == null)
            {
                elementType = Model.GetType().GetGenericArguments()[0];
            }
            return elementType.Name.ToSeparatedWords();
        }
    }
}

